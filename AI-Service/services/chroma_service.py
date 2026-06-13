import chromadb
from services.embedding_service import EmbeddingService

client = chromadb.PersistentClient(
    path="./chroma_db"
)

collection = client.get_or_create_collection(
    name="documents"
)

class ChromaService:

    @staticmethod
    def add_document(
        document_id: str,
        content: str,
        metadata: dict | None = None
    ):

        embedding = \
            EmbeddingService.create_embedding(
                content
            )

        collection.upsert(
            ids=[document_id],
            documents=[content],
            embeddings=[embedding],
            metadatas=[metadata or ChromaService._build_metadata(document_id)]
        )

    @staticmethod
    def search(
        question: str,
        n_results: int = 5
    ):

        embedding = \
            EmbeddingService.create_embedding(
                question
            )

        result = collection.query(
            query_embeddings=[embedding],
            n_results=n_results,
            include=[
                "documents",
                "metadatas",
                "distances"
            ]
        )

        return result
    
    @staticmethod
    def get_all_documents():

        result = collection.get()

        return result
    
    @staticmethod
    def delete_document(
        document_id: str
    ):

        collection.delete(
            ids=[document_id]
        )

    @staticmethod
    def _build_metadata(
        document_id: str
    ):

        source_type = "document"

        if document_id.startswith("product-"):
            source_type = "product"

        if document_id.startswith("support-policy-"):
            source_type = "support_policy"

        return {
            "source_id": document_id,
            "source_type": source_type
        }
