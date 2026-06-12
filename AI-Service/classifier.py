import joblib

model = joblib.load(
    "ticket_classifier.pkl"
)

def predict_category(
    text: str
):
    return model.predict(
        [text]
    )[0]