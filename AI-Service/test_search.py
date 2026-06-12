from services.chroma_service import ChromaService

result = ChromaService.search(
    "Tôi được trả hàng trong bao lâu?"
)

print(result)