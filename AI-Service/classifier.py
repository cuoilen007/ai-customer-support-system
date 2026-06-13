from pathlib import Path

import joblib


MODEL_PATH = Path(__file__).resolve().parent / "ticket_classifier.pkl"
_model = None
_model_mtime = None


def _ensure_model_loaded(force: bool = False):
    global _model
    global _model_mtime

    if not MODEL_PATH.exists():
        raise FileNotFoundError(f"Model file was not found: {MODEL_PATH}")

    current_mtime = MODEL_PATH.stat().st_mtime

    if force or _model is None or _model_mtime != current_mtime:
        _model = joblib.load(MODEL_PATH)
        _model_mtime = current_mtime

    return _model


def reload_model():
    _ensure_model_loaded(force=True)


def predict_category(text: str):
    model = _ensure_model_loaded()
    return model.predict([text])[0]
