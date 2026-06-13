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
            question,
            n_results=5
        )

        context_blocks = []
        sources = []

        if (
            result.get("documents")
            and len(result["documents"]) > 0
            and len(result["documents"][0]) > 0
        ):
            documents = result["documents"][0]
            metadatas = result.get("metadatas", [[]])[0]
            distances = result.get("distances", [[]])[0]
            ids = result.get("ids", [[]])[0]

            for index, document in enumerate(documents):
                metadata = (
                    metadatas[index]
                    if index < len(metadatas)
                    and metadatas[index]
                    else {}
                )
                distance = (
                    distances[index]
                    if index < len(distances)
                    else None
                )
                source_id = (
                    ids[index]
                    if index < len(ids)
                    else metadata.get("source_id", "")
                )
                source_type = metadata.get(
                    "source_type",
                    "document"
                )
                relevance_score = (
                    max(0, min(100, round((1 - distance) * 100)))
                    if distance is not None
                    else 0
                )

                context_blocks.append(
                    f"[Source {index + 1}: {source_type}/{source_id}]\n{document}"
                )
                sources.append({
                    "source_id": source_id,
                    "source_type": source_type,
                    "relevance_score": relevance_score,
                    "content": document
                })

        context = "\n\n".join(context_blocks)

        prompt = f"""
You are a customer support assistant.

Use only the information provided in Context.
Answer in Vietnamese.
Keep the answer concise, clear, and helpful.
If the answer depends on product or policy details, explain it in a customer-friendly way.

If Context does not contain the answer, respond exactly with:
"Toi khong tim thay thong tin phu hop."

Context:
{context}

Question:
{question}
"""

        response = client.chat.completions.create(
            model="gpt-4.1-mini",
            messages=[
                {
                    "role": "user",
                    "content": prompt
                }
            ]
        )

        answer = (
            response
            .choices[0]
            .message.content
        )

        return {
            "answer": answer,
            "context": context,
            "sources": sources
        }
