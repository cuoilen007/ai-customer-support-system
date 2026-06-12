import { useState } from "react";

interface Props {
  onCreate: (
    title: string,
    content: string
  ) => Promise<void>;
}

export default function DocumentForm({
  onCreate,
}: Props) {

  const [title, setTitle] =
    useState("");

  const [content, setContent] =
    useState("");

  const [loading, setLoading] =
    useState(false);

  const handleSubmit =
    async () => {

      if (
        !title.trim() ||
        !content.trim()
      ) {
        return;
      }

      try {

        setLoading(true);

        await onCreate(
          title,
          content
        );

        setTitle("");
        setContent("");

      } finally {

        setLoading(false);

      }
    };

  return (
    <div
      className="
      bg-white
      rounded-lg
      shadow
      p-6
      mb-6
      "
    >

      <h2
        className="
        text-xl
        font-bold
        mb-4
        "
      >
        Add Document
      </h2>

      <input
        value={title}
        onChange={(e) =>
          setTitle(
            e.target.value
          )
        }
        placeholder="Title"
        className="
        w-full
        border
        p-3
        rounded
        mb-3
        "
      />

      <textarea
        rows={6}
        value={content}
        onChange={(e) =>
          setContent(
            e.target.value
          )
        }
        placeholder="Content"
        className="
        w-full
        border
        p-3
        rounded
        mb-3
        "
      />

      <button
        onClick={
          handleSubmit
        }
        disabled={
          loading
        }
        className="
        bg-blue-500
        text-white
        px-6
        py-2
        rounded
        "
      >
        {
          loading
            ? "Saving..."
            : "Add Document"
        }
      </button>

    </div>
  );
}