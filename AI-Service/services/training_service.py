from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path
from threading import Lock, Thread

import joblib
import pandas as pd
from sklearn.base import clone
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.model_selection import StratifiedKFold, cross_val_score
from sklearn.naive_bayes import MultinomialNB
from sklearn.pipeline import Pipeline
from sklearn.svm import SVC
from sklearn.tree import DecisionTreeClassifier

from classifier import reload_model


ROOT_DIR = Path(__file__).resolve().parent.parent
TRAINING_DIR = ROOT_DIR / "training"
REVIEW_CSV_PATH = TRAINING_DIR / "training_run_examples.csv"
REVIEW_JSONL_PATH = TRAINING_DIR / "training_run_examples.jsonl"
STATUS_PATH = TRAINING_DIR / "training_status.json"
MODEL_PATH = ROOT_DIR / "ticket_classifier.pkl"


def _utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


class TrainingService:
    _thread: Thread | None = None
    _thread_lock = Lock()
    _status_lock = Lock()

    @classmethod
    def get_status(cls) -> dict:
        if not STATUS_PATH.exists():
            return cls._default_status()

        try:
            with STATUS_PATH.open("r", encoding="utf-8") as handle:
                return json.load(handle)
        except Exception:
            return cls._default_status(
                status="failed",
                message="Could not read training status.",
                error="status_file_unreadable"
            )

    @classmethod
    def start_training(cls, examples: list[dict]) -> dict:
        with cls._thread_lock:
            if cls._thread is not None and cls._thread.is_alive():
                return cls.get_status()

            previous_status = cls.get_status()
            status = cls._default_status(
                status="running",
                message="Training is in progress.",
                reviewedExampleCount=len(examples),
                startedAt=_utc_now(),
                modelVersion=previous_status.get("modelVersion", 0)
            )
            cls._write_status(status)

            cls._thread = Thread(
                target=cls._run_training,
                args=(examples,),
                daemon=True
            )
            cls._thread.start()

        return status

    @classmethod
    def _run_training(cls, examples: list[dict]) -> None:
        try:
            previous_status = cls.get_status()
            reviewed_examples = cls._normalize_examples(examples)
            cls._write_review_exports(reviewed_examples)

            training_frame = cls._build_training_frame(reviewed_examples)
            metrics = cls._train_model(training_frame)

            cls._write_status({
                "status": "succeeded",
                "message": "Training completed successfully.",
                "startedAt": cls.get_status().get("startedAt"),
                "completedAt": _utc_now(),
                "lastUpdatedAt": _utc_now(),
                "reviewedExampleCount": len(reviewed_examples),
                "datasetSize": int(len(training_frame)),
                "classCount": int(training_frame["category"].nunique()),
                "bestModelName": metrics["bestModelName"],
                "accuracy": metrics["accuracy"],
                "modelVersion": int(previous_status.get("modelVersion", 0)) + 1,
                "modelPath": str(MODEL_PATH),
                "error": ""
            })

            reload_model()
        except Exception as exc:
            previous_status = cls.get_status()
            cls._write_status({
                "status": "failed",
                "message": "Training failed.",
                "startedAt": cls.get_status().get("startedAt"),
                "completedAt": _utc_now(),
                "lastUpdatedAt": _utc_now(),
                "reviewedExampleCount": len(examples),
                "datasetSize": 0,
                "classCount": 0,
                "bestModelName": "",
                "accuracy": 0,
                "modelVersion": int(previous_status.get("modelVersion", 0)),
                "modelPath": str(MODEL_PATH),
                "error": str(exc)
            })

    @classmethod
    def _default_status(cls, **overrides) -> dict:
        status = {
            "status": "idle",
            "message": "Training has not started yet.",
            "startedAt": None,
            "completedAt": None,
            "lastUpdatedAt": _utc_now(),
            "reviewedExampleCount": 0,
            "datasetSize": 0,
            "classCount": 0,
            "bestModelName": "",
            "accuracy": 0,
            "modelVersion": 0,
            "modelPath": str(MODEL_PATH),
            "error": ""
        }
        status.update(overrides)
        return status

    @classmethod
    def _write_status(cls, payload: dict) -> None:
        TRAINING_DIR.mkdir(parents=True, exist_ok=True)
        payload["lastUpdatedAt"] = _utc_now()

        with cls._status_lock:
            with STATUS_PATH.open("w", encoding="utf-8") as handle:
                json.dump(payload, handle, ensure_ascii=False, indent=2)

    @classmethod
    def _normalize_examples(cls, examples: list[dict]) -> list[dict]:
        normalized: list[dict] = []

        for example in examples:
            input_text = str(example.get("input") or "").strip()
            category = str(example.get("category") or "").strip()
            output_text = str(example.get("output") or "").strip()
            intent = str(example.get("intent") or "").strip()

            if not input_text or not category:
                continue

            normalized.append({
                "input": input_text,
                "category": category,
                "output": output_text,
                "intent": intent
            })

        if not normalized:
            raise ValueError("No finalized training examples are available for model training.")

        return normalized

    @classmethod
    def _write_review_exports(cls, examples: list[dict]) -> None:
        TRAINING_DIR.mkdir(parents=True, exist_ok=True)

        frame = pd.DataFrame(examples)
        frame.to_csv(REVIEW_CSV_PATH, index=False, encoding="utf-8")

        with REVIEW_JSONL_PATH.open("w", encoding="utf-8") as handle:
            for example in examples:
                row = {
                    "messages": [
                        {
                            "role": "user",
                            "content": example["input"]
                        },
                        {
                            "role": "assistant",
                            "content": example["output"]
                        }
                    ],
                    "metadata": {
                        "category": example["category"],
                        "intent": example["intent"]
                    }
                }
                handle.write(json.dumps(row, ensure_ascii=False) + "\n")

    @classmethod
    def _build_training_frame(cls, reviewed_examples: list[dict]) -> pd.DataFrame:
        combined = pd.DataFrame(reviewed_examples)[["input", "category"]].rename(
            columns={"input": "text"}
        )
        combined["text"] = combined["text"].fillna("").astype(str).str.strip()
        combined["category"] = combined["category"].fillna("").astype(str).str.strip()
        combined = combined[
            (combined["text"] != "") &
            (combined["category"] != "")
        ].drop_duplicates()

        if combined.empty:
            raise ValueError("The training dataset is empty after normalization.")

        if combined["category"].nunique() < 2:
            raise ValueError("At least two categories are required to retrain the classifier.")

        return combined

    @classmethod
    def _train_model(cls, frame: pd.DataFrame) -> dict:
        x = frame["text"]
        y = frame["category"]

        min_samples = int(y.value_counts().min())
        n_splits = min(5, min_samples)

        models = {
            "Logistic Regression": LogisticRegression(max_iter=2000),
            "Decision Tree": DecisionTreeClassifier(random_state=42),
            "Random Forest": RandomForestClassifier(n_estimators=200, random_state=42),
            "SVM": SVC(),
            "Naive Bayes": MultinomialNB()
        }

        best_model_name = "Naive Bayes"
        best_score = 0.0

        if n_splits >= 2:
            cv = StratifiedKFold(
                n_splits=n_splits,
                shuffle=True,
                random_state=42
            )

            for name, model in models.items():
                pipeline = Pipeline([
                    ("tfidf", TfidfVectorizer(stop_words=None)),
                    ("model", clone(model))
                ])

                scores = cross_val_score(
                    pipeline,
                    x,
                    y,
                    cv=cv,
                    scoring="accuracy"
                )

                mean_score = float(scores.mean())
                if mean_score > best_score:
                    best_score = mean_score
                    best_model_name = name
        else:
            best_model_name = "Naive Bayes"
            best_score = 0.0

        final_model = clone(models[best_model_name])
        final_pipeline = Pipeline([
            ("tfidf", TfidfVectorizer()),
            ("model", final_model)
        ])

        final_pipeline.fit(x, y)
        joblib.dump(final_pipeline, MODEL_PATH)

        return {
            "bestModelName": best_model_name,
            "accuracy": round(best_score, 4)
        }
