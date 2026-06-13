from fastapi import FastAPI
from pydantic import BaseModel

from services.chroma_service import (
    ChromaService
)

from services.rag_service import (
    RagService
)

from classifier import (
    predict_category
)

from services.evaluation_service import (
    EvaluationService
)
from services.training_service import (
    TrainingService
)

app = FastAPI(
    title="AI Customer Support",
    version="1.0"
)


# ===================================
# Request Models
# ===================================

class InsertDocumentRequest(
    BaseModel
):
    document_id: str
    content: str
    metadata: dict | None = None


class SearchRequest(
    BaseModel
):
    question: str


class RagRequest(
    BaseModel
):
    question: str

class ClassificationRequest(
    BaseModel
):
    text: str


class EvaluationRequest(
    BaseModel
):
    question: str
    answer: str
    context: str = ""
    category: str = ""


class TrainingExampleRequest(
    BaseModel
):
    input: str
    output: str = ""
    category: str
    intent: str = ""


class TrainingRunRequest(
    BaseModel
):
    examples: list[TrainingExampleRequest]


# ===================================
# Health Check
# ===================================

@app.get("/")
def home():

    return {
        "message":
        "AI Service Running"
    }


@app.get("/health")
def health():

    return {
        "status": "ok"
    }


# ===================================
# Documents
# ===================================

@app.post("/documents")
def insert_document(
    request: InsertDocumentRequest
):

    ChromaService.add_document(
        request.document_id,
        request.content,
        request.metadata
    )

    return {
        "success": True,
        "message":
        "Document inserted"
    }


@app.get("/documents")
def get_documents():

    return (
        ChromaService
        .get_all_documents()
    )


@app.delete(
    "/documents/{document_id}"
)
def delete_document(
    document_id: str
):

    ChromaService.delete_document(
        document_id
    )

    return {
        "success": True
    }


# ===================================
# Semantic Search
# ===================================

@app.post("/search")
def search(
    request: SearchRequest
):

    result = (
        ChromaService.search(
            request.question
        )
    )

    return result


# ===================================
# RAG
# ===================================

@app.post("/rag")
def rag(
    request: RagRequest
):

    return RagService.ask(
        request.question
    )


# ===================================
# Classification
# ===================================
@app.post(
    "/classify"
)
def classify(
    request:
    ClassificationRequest
):
    category = (
        predict_category(
            request.text
        )
    )

    return {
        "category":
        category
    }


@app.post(
    "/evaluate"
)
def evaluate(
    request:
    EvaluationRequest
):
    return EvaluationService.evaluate(
        request.question,
        request.answer,
        request.context,
        request.category
    )


@app.get("/training/status")
def training_status():
    return TrainingService.get_status()


@app.post("/training/run")
def run_training(
    request: TrainingRunRequest
):
    return TrainingService.start_training(
        [example.model_dump() for example in request.examples]
    )
