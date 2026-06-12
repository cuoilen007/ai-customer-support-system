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
        content: str
    ):

        embedding = \
            EmbeddingService.create_embedding(
                content
            )

        collection.add(
            ids=[document_id],
            documents=[content],
            embeddings=[embedding]
        )

    @staticmethod
    def search(question: str):

        embedding = \
            EmbeddingService.create_embedding(
                question
            )

        result = collection.query(
            query_embeddings=[embedding],
            n_results=3
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
