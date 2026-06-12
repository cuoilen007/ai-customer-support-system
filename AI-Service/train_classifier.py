import pandas as pd
import joblib
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.model_selection import StratifiedKFold, cross_val_score
from sklearn.base import clone # <-- Thêm thư viện này để reset model sạch dòng 82

# Thêm các thuật toán
from sklearn.linear_model import LogisticRegression
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.svm import SVC
from sklearn.naive_bayes import MultinomialNB

# =====================================
# Load Data
# =====================================
df = pd.read_csv("training/tickets.csv")
X = df["text"]
y = df["category"]

print("\nDataset size:", len(df))
print("\nClass distribution:\n", y.value_counts())

# Tự động điều chỉnh số fold dựa trên nhãn có số mẫu ít nhất
min_samples = y.value_counts().min()
n_splits = min(5, min_samples) 

if n_splits < 2:
    print("\n⚠️ CẢNH BÁO: Dữ liệu quá ít! Bạn cần bổ sung thêm data vào file CSV (Mỗi nhãn ít nhất 5 dòng).")
    n_splits = 2 # Ép tối thiểu bằng 2 để không lỗi code

# =====================================
# Stratified Cross Validation
# =====================================
cv = StratifiedKFold(
    n_splits=n_splits,
    shuffle=True,
    random_state=42
)

# =====================================
# Candidate Models
# =====================================
models = {
    "Logistic Regression": LogisticRegression(max_iter=2000),
    "Decision Tree": DecisionTreeClassifier(random_state=42),
    "Random Forest": RandomForestClassifier(n_estimators=200, random_state=42),
    "SVM": SVC(),
    "Naive Bayes": MultinomialNB()
}

# =====================================
# Evaluate Models
# =====================================
results = []
best_model_name = None
best_score = 0

print(f"\nModel Evaluation (Cross-Validation Folds = {n_splits})")
print("=" * 50)

for name, model in models.items():
    # Sử dụng clone(model) để đảm bảo mỗi vòng lặp là một model sạch tinh
    pipeline = Pipeline([
        ("tfidf", TfidfVectorizer(stop_words=None)),
        ("model", clone(model)) 
    ])

    scores = cross_val_score(
        pipeline, X, y,
        cv=cv,
        scoring="accuracy"
    )

    mean_score = scores.mean()
    results.append((name, mean_score))

    print(f"{name:<25} Accuracy = {mean_score:.4f}")

    if mean_score > best_score:
        best_score = mean_score
        best_model_name = name

print(f"\nBest Model: {best_model_name} ({best_score:.4f})")

# =====================================
# Train Final Model
# =====================================
# Tạo một thực thể model hoàn toàn mới, sạch sẽ dựa trên bản thiết kế của Best Model
final_model = clone(models[best_model_name])

final_pipeline = Pipeline([
    ("tfidf", TfidfVectorizer()), # Bạn có thể thêm lowercase=True hoặc ngram_range=(1,2) để tăng độ chính xác
    ("model", final_model)
])

# Huấn luyện trên TOÀN BỘ dữ liệu sạch
final_pipeline.fit(X, y)

# Lưu model ra file
joblib.dump(final_pipeline, "ticket_classifier.pkl")
print("\nModel saved successfully: ticket_classifier.pkl")