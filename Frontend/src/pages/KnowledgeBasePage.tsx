import {
  useEffect,
  useState
} from "react";

import MainLayout
from "../components/layout/MainLayout";

import DocumentForm
from "../components/document/DocumentForm";

import DocumentList
from "../components/document/DocumentList";

import {
  createDocument,
  deleteDocument,
  getDocuments
}
from "../api/documentApi";

export default function
KnowledgeBasePage() {

  const [
    documents,
    setDocuments
  ] = useState<any[]>([]);

  useEffect(() => {
    loadDocuments();
  }, []);

  const loadDocuments =
    async () => {

      const res =
        await getDocuments();

      setDocuments(
        res.data
      );
    };

  const handleCreate =
    async (
      title: string,
      content: string
    ) => {

      await createDocument(
        title,
        content
      );

      await loadDocuments();
    };

  const handleDelete =
    async (
      id: number
    ) => {

      await deleteDocument(
        id
      );

      await loadDocuments();
    };

  return (

    <MainLayout>

      <div
        className="
        max-w-6xl
        mx-auto
        p-6
        "
      >

        <h1
          className="
          text-3xl
          font-bold
          mb-6
          "
        >
          Knowledge Base
        </h1>

        <DocumentForm
          onCreate={
            handleCreate
          }
        />

        <DocumentList
          documents={
            documents
          }
          onDelete={
            handleDelete
          }
        />

      </div>

    </MainLayout>
  );
}