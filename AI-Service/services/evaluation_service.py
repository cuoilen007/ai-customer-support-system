import json
from openai import OpenAI

from services.config import (
    OPENAI_API_KEY
)

client = OpenAI(
    api_key=OPENAI_API_KEY
)


class EvaluationService:

    @staticmethod
    def evaluate(
        question: str,
        answer: str,
        context: str,
        category: str
    ):

        prompt = f"""
You are an AI quality reviewer for a customer support assistant.

Return only valid JSON with this schema:
{{
  "sentiment": "Positive | Neutral | Negative",
  "intent": "Pricing | ProductInfo | Warranty | ReturnOrRefund | Shipping | Complaint | GeneralSupport",
  "confidence_score": 0-100,
  "needs_human_review": true/false,
  "improvement_note": "short actionable note"
}}

Judge whether the answer is grounded in the retrieved context and useful for the customer.
Lower confidence if the answer is vague, not supported by context, or says no information was found.
The assistant answers customers in Vietnamese, but your output must stay JSON only.

Question:
{question}

Answer:
{answer}

Category:
{category}

Retrieved context:
{context}
"""

        response = client.chat.completions.create(
            model="gpt-4.1-mini",
            response_format={
                "type": "json_object"
            },
            messages=[
                {
                    "role": "user",
                    "content": prompt
                }
            ]
        )

        content = (
            response
            .choices[0]
            .message
            .content
            or "{}"
        )

        try:
            data = json.loads(content)
        except json.JSONDecodeError:
            return {
                "sentiment": "Neutral",
                "intent": "GeneralSupport",
                "confidence_score": 0,
                "needs_human_review": True,
                "improvement_note": "Evaluator returned invalid JSON. Review this exchange manually."
            }

        return {
            "sentiment": data.get("sentiment", "Neutral"),
            "intent": data.get("intent", "GeneralSupport"),
            "confidence_score": int(data.get("confidence_score", 0)),
            "needs_human_review": bool(data.get("needs_human_review", True)),
            "improvement_note": data.get(
                "improvement_note",
                "Review this exchange before using it for training."
            )
        }
