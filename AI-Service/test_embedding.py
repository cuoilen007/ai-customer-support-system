from services.embedding_service import EmbeddingService

text = """
Khách hàng được hoàn tiền
trong vòng 7 ngày.
"""

embedding = EmbeddingService.create_embedding(
    text
)

print(len(embedding))