import {
  useEffect,
  useState
} from "react";

import DocumentForm
from "../components/document/DocumentForm";

import DocumentList
from "../components/document/DocumentList";

import {
  createDocument,
  deleteDocument,
  getDocuments
} from "../api/documentApi";

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

      try {

        const res =
          await getDocuments();

        setDocuments(
          res.data
        );

      } catch (error) {

        console.error(
          error
        );

      }
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

    <div
      className="
      h-full
      overflow-auto
      bg-slate-100
      "
    >

      {/* Header */}

      <div
        className="
        bg-white
        border-b
        px-8
        py-5
        shadow-sm
        "
      >

        <h1
          className="
          text-2xl
          font-bold
          text-slate-800
          "
        >
          Knowledge Base
        </h1>

        <p
          className="
          text-sm
          text-slate-500
          mt-1
          "
        >
          Manage documents used by the AI assistant
        </p>

      </div>

      {/* Content */}

      <div
        className="
        max-w-6xl
        mx-auto
        p-8
        "
      >

        <DocumentForm
          onCreate={
            handleCreate
          }
        />

        <div className="mt-8">

          <DocumentList
            documents={
              documents
            }
            onDelete={
              handleDelete
            }
          />

        </div>

      </div>

    </div>

  );
}