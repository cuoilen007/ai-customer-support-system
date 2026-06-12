import type { Document } from "../../types/document";

interface Props {

  documents:
    Document[];

  onDelete:
    (id: number)
    => Promise<void>;
}

export default function
DocumentList({
  documents,
  onDelete
}: Props) {

  return (

    <div
      className="
      bg-white
      rounded-lg
      shadow
      "
    >

      <table
        className="
        w-full
        "
      >

        <thead>

          <tr
            className="
            border-b
            "
          >

            <th
              className="
              text-left
              p-4
              "
            >
              Title
            </th>

            <th
              className="
              p-4
              "
            >
              Action
            </th>

          </tr>

        </thead>

        <tbody>

          {
            documents.map(
              (doc) => (

                <tr
                  key={doc.id}
                  className="
                  border-b
                  "
                >

                  <td
                    className="
                    p-4
                    "
                  >
                    {doc.title}
                  </td>

                  <td
                    className="
                    p-4
                    text-center
                    "
                  >

                    <button
                      onClick={() =>
                        onDelete(
                          doc.id
                        )
                      }
                      className="
                      bg-red-500
                      text-white
                      px-3
                      py-1
                      rounded
                      "
                    >
                      Delete
                    </button>

                  </td>

                </tr>

              )
            )
          }

        </tbody>

      </table>

    </div>
  );
}