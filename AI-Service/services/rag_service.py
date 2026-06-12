from openai import OpenAI

from services.config import (
    OPENAI_API_KEY
)

from services.chroma_service import (
    ChromaService
)

client = OpenAI(
    api_key=OPENAI_API_KEY
)


class RagService:

    @staticmethod
    def ask(question: str):

        result = ChromaService.search(
            question
        )

        context = ""

        if (
            result.get("documents")
            and len(result["documents"]) > 0
            and len(result["documents"][0]) > 0
        ):
            context = result["documents"][0][0]

        prompt = f"""
Bạn là nhân viên hỗ trợ khách hàng.

Chỉ sử dụng thông tin trong Context.

Nếu Context không có câu trả lời,
hãy trả lời:

"Tôi không tìm thấy thông tin phù hợp."

Context:
{context}

Question:
{question}
"""

        response = (
            client.chat.completions.create(
                model="gpt-4.1-mini",
                messages=[
                    {
                        "role": "user",
                        "content": prompt
                    }
                ]
            )
        )

        answer = (
            response
            .choices[0]
            .message.content
        )

        return {
            "answer": answer,
            "context": context
        }