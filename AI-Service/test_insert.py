from services.chroma_service import ChromaService

ChromaService.add_document(
    "1",
    """
    Khách hàng được hoàn tiền
    trong vòng 7 ngày.
    """
)

print("Inserted")