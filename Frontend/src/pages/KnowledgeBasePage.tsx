import {
  useEffect,
  useMemo,
  useState,
  type ReactNode
} from "react";

import {
  CircleStackIcon,
  DocumentTextIcon,
  PlusIcon,
  ShieldCheckIcon,
  TagIcon,
  TrashIcon
} from "@heroicons/react/24/outline";

import {
  createDocument,
  deleteDocument,
  getDocuments
} from "../api/documentApi";

import {
  createProduct,
  deleteProduct,
  getProducts
} from "../api/productApi";

import {
  createSupportPolicy,
  deleteSupportPolicy,
  getSupportPolicies
} from "../api/supportPolicyApi";

import type { Document } from "../types/document";
import type { Product } from "../types/product";
import type { SupportPolicy } from "../types/supportPolicy";

type KnowledgeTab = "documents" | "products" | "policies";

export default function KnowledgeBasePage() {
  const [activeTab, setActiveTab] =
    useState<KnowledgeTab>("documents");
  const [documents, setDocuments] =
    useState<Document[]>([]);
  const [products, setProducts] =
    useState<Product[]>([]);
  const [policies, setPolicies] =
    useState<SupportPolicy[]>([]);
  const [loading, setLoading] =
    useState(false);
  const [saving, setSaving] =
    useState(false);

  const [documentForm, setDocumentForm] =
    useState({
      title: "",
      content: ""
    });

  const [productForm, setProductForm] =
    useState({
      name: "",
      category: "",
      price: "",
      status: "Active",
      description: ""
    });

  const [policyForm, setPolicyForm] =
    useState({
      title: "",
      policyType: "Return",
      effectiveFrom: new Date().toISOString().slice(0, 10),
      content: ""
    });

  useEffect(() => {
    loadAll();
  }, []);

  const counts = useMemo(() => ({
    documents: documents.length,
    products: products.length,
    policies: policies.length
  }), [
    documents.length,
    products.length,
    policies.length
  ]);

  const loadAll = async () => {
    try {
      setLoading(true);
      const [
        documentRes,
        productRes,
        policyRes
      ] = await Promise.all([
        getDocuments(),
        getProducts(),
        getSupportPolicies()
      ]);

      setDocuments(documentRes.data);
      setProducts(productRes.data);
      setPolicies(policyRes.data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateDocument = async () => {
    if (
      !documentForm.title.trim()
      || !documentForm.content.trim()
    ) {
      return;
    }

    try {
      setSaving(true);
      await createDocument(
        documentForm.title,
        documentForm.content
      );
      setDocumentForm({
        title: "",
        content: ""
      });
      await loadAll();
    } finally {
      setSaving(false);
    }
  };

  const handleCreateProduct = async () => {
    if (
      !productForm.name.trim()
      || !productForm.description.trim()
    ) {
      return;
    }

    try {
      setSaving(true);
      await createProduct({
        name: productForm.name,
        category: productForm.category,
        price: Number(productForm.price || 0),
        status: productForm.status,
        description: productForm.description
      });
      setProductForm({
        name: "",
        category: "",
        price: "",
        status: "Active",
        description: ""
      });
      await loadAll();
    } finally {
      setSaving(false);
    }
  };

  const handleCreatePolicy = async () => {
    if (
      !policyForm.title.trim()
      || !policyForm.content.trim()
    ) {
      return;
    }

    try {
      setSaving(true);
      await createSupportPolicy({
        title: policyForm.title,
        policyType: policyForm.policyType,
        content: policyForm.content,
        effectiveFrom: policyForm.effectiveFrom
      });
      setPolicyForm({
        title: "",
        policyType: "Return",
        effectiveFrom: new Date().toISOString().slice(0, 10),
        content: ""
      });
      await loadAll();
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="h-full overflow-auto bg-slate-100">
      <div className="bg-white border-b px-8 py-5 shadow-sm">
        <div className="flex items-center gap-3">
          <CircleStackIcon className="h-7 w-7 text-blue-600" />
          <div>
            <h1 className="text-2xl font-bold text-slate-800">
              AI Knowledge Base
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Manage the structured data used by the support assistant
            </p>
          </div>
        </div>
      </div>

      <div className="max-w-6xl mx-auto p-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <SummaryCard
            icon={<DocumentTextIcon className="h-5 w-5" />}
            label="Documents"
            value={counts.documents}
          />
          <SummaryCard
            icon={<TagIcon className="h-5 w-5" />}
            label="Products"
            value={counts.products}
          />
          <SummaryCard
            icon={<ShieldCheckIcon className="h-5 w-5" />}
            label="Policies"
            value={counts.policies}
          />
        </div>

        <div className="bg-white border border-slate-200 rounded-lg shadow-sm">
          <div className="flex border-b border-slate-200 px-4 pt-4">
            <TabButton
              active={activeTab === "documents"}
              label="Documents"
              onClick={() => setActiveTab("documents")}
            />
            <TabButton
              active={activeTab === "products"}
              label="Products"
              onClick={() => setActiveTab("products")}
            />
            <TabButton
              active={activeTab === "policies"}
              label="Policies"
              onClick={() => setActiveTab("policies")}
            />
          </div>

          <div className="p-6">
            {activeTab === "documents" && (
              <DocumentPanel
                documents={documents}
                form={documentForm}
                loading={loading}
                saving={saving}
                onChange={setDocumentForm}
                onCreate={handleCreateDocument}
                onDelete={async (id) => {
                  await deleteDocument(id);
                  await loadAll();
                }}
              />
            )}

            {activeTab === "products" && (
              <ProductPanel
                products={products}
                form={productForm}
                loading={loading}
                saving={saving}
                onChange={setProductForm}
                onCreate={handleCreateProduct}
                onDelete={async (id) => {
                  await deleteProduct(id);
                  await loadAll();
                }}
              />
            )}

            {activeTab === "policies" && (
              <PolicyPanel
                policies={policies}
                form={policyForm}
                loading={loading}
                saving={saving}
                onChange={setPolicyForm}
                onCreate={handleCreatePolicy}
                onDelete={async (id) => {
                  await deleteSupportPolicy(id);
                  await loadAll();
                }}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function SummaryCard({
  icon,
  label,
  value
}: {
  icon: ReactNode;
  label: string;
  value: number;
}) {
  return (
    <div className="bg-white border border-slate-200 rounded-lg p-5 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-slate-500">{label}</p>
          <p className="text-3xl font-bold text-slate-900 mt-1">
            {value}
          </p>
        </div>
        <div className="h-10 w-10 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center">
          {icon}
        </div>
      </div>
    </div>
  );
}

function TabButton({
  active,
  label,
  onClick
}: {
  active: boolean;
  label: string;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className={`
        px-4 py-3 text-sm font-semibold border-b-2 transition
        ${active
          ? "border-blue-600 text-blue-600"
          : "border-transparent text-slate-500 hover:text-slate-900"}
      `}
    >
      {label}
    </button>
  );
}

function DocumentPanel({
  documents,
  form,
  loading,
  saving,
  onChange,
  onCreate,
  onDelete
}: {
  documents: Document[];
  form: {
    title: string;
    content: string;
  };
  loading: boolean;
  saving: boolean;
  onChange: (value: {
    title: string;
    content: string;
  }) => void;
  onCreate: () => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-[360px_1fr] gap-6">
      <FormSection
        title="Add document"
        saving={saving}
        onCreate={onCreate}
      >
        <TextInput
          value={form.title}
          placeholder="Title"
          onChange={(title) => onChange({
            ...form,
            title
          })}
        />
        <TextArea
          value={form.content}
          placeholder="Content"
          onChange={(content) => onChange({
            ...form,
            content
          })}
        />
      </FormSection>

      <DataTable
        loading={loading}
        emptyText="No documents yet"
        headers={[
          "Title",
          "Preview",
          ""
        ]}
        rows={documents.map((doc) => [
          doc.title,
          doc.content,
          <DeleteButton
            key={doc.id}
            onClick={() => onDelete(doc.id)}
          />
        ])}
      />
    </div>
  );
}

function ProductPanel({
  products,
  form,
  loading,
  saving,
  onChange,
  onCreate,
  onDelete
}: {
  products: Product[];
  form: {
    name: string;
    category: string;
    price: string;
    status: string;
    description: string;
  };
  loading: boolean;
  saving: boolean;
  onChange: (value: {
    name: string;
    category: string;
    price: string;
    status: string;
    description: string;
  }) => void;
  onCreate: () => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-[360px_1fr] gap-6">
      <FormSection
        title="Add product"
        saving={saving}
        onCreate={onCreate}
      >
        <TextInput
          value={form.name}
          placeholder="Product name"
          onChange={(name) => onChange({
            ...form,
            name
          })}
        />
        <TextInput
          value={form.category}
          placeholder="Category"
          onChange={(category) => onChange({
            ...form,
            category
          })}
        />
        <TextInput
          value={form.price}
          type="number"
          placeholder="Price"
          onChange={(price) => onChange({
            ...form,
            price
          })}
        />
        <select
          value={form.status}
          onChange={(event) => onChange({
            ...form,
            status: event.target.value
          })}
          className="w-full border border-slate-300 p-3 rounded-md text-sm"
        >
          <option value="Active">Active</option>
          <option value="Draft">Draft</option>
          <option value="OutOfStock">Out of stock</option>
        </select>
        <TextArea
          value={form.description}
          placeholder="Description"
          onChange={(description) => onChange({
            ...form,
            description
          })}
        />
      </FormSection>

      <DataTable
        loading={loading}
        emptyText="No products yet"
        headers={[
          "Product",
          "Category",
          "Price",
          "Status",
          ""
        ]}
        rows={products.map((product) => [
          product.name,
          product.category || "-",
          product.price.toLocaleString(),
          product.status,
          <DeleteButton
            key={product.id}
            onClick={() => onDelete(product.id)}
          />
        ])}
      />
    </div>
  );
}

function PolicyPanel({
  policies,
  form,
  loading,
  saving,
  onChange,
  onCreate,
  onDelete
}: {
  policies: SupportPolicy[];
  form: {
    title: string;
    policyType: string;
    effectiveFrom: string;
    content: string;
  };
  loading: boolean;
  saving: boolean;
  onChange: (value: {
    title: string;
    policyType: string;
    effectiveFrom: string;
    content: string;
  }) => void;
  onCreate: () => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-[360px_1fr] gap-6">
      <FormSection
        title="Add policy"
        saving={saving}
        onCreate={onCreate}
      >
        <TextInput
          value={form.title}
          placeholder="Policy title"
          onChange={(title) => onChange({
            ...form,
            title
          })}
        />
        <select
          value={form.policyType}
          onChange={(event) => onChange({
            ...form,
            policyType: event.target.value
          })}
          className="w-full border border-slate-300 p-3 rounded-md text-sm"
        >
          <option value="Return">Return</option>
          <option value="Refund">Refund</option>
          <option value="Warranty">Warranty</option>
          <option value="Shipping">Shipping</option>
        </select>
        <TextInput
          value={form.effectiveFrom}
          type="date"
          placeholder="Effective from"
          onChange={(effectiveFrom) => onChange({
            ...form,
            effectiveFrom
          })}
        />
        <TextArea
          value={form.content}
          placeholder="Policy content"
          onChange={(content) => onChange({
            ...form,
            content
          })}
        />
      </FormSection>

      <DataTable
        loading={loading}
        emptyText="No policies yet"
        headers={[
          "Title",
          "Type",
          "Effective",
          ""
        ]}
        rows={policies.map((policy) => [
          policy.title,
          policy.policyType,
          new Date(policy.effectiveFrom).toLocaleDateString(),
          <DeleteButton
            key={policy.id}
            onClick={() => onDelete(policy.id)}
          />
        ])}
      />
    </div>
  );
}

function FormSection({
  title,
  saving,
  onCreate,
  children
}: {
  title: string;
  saving: boolean;
  onCreate: () => Promise<void>;
  children: ReactNode;
}) {
  return (
    <div className="border border-slate-200 rounded-lg p-4 h-fit">
      <h2 className="text-base font-bold text-slate-800 mb-4">
        {title}
      </h2>
      <div className="space-y-3">
        {children}
      </div>
      <button
        onClick={onCreate}
        disabled={saving}
        className="mt-4 inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white px-4 py-2 rounded-md text-sm font-semibold transition"
      >
        <PlusIcon className="h-4 w-4" />
        {saving ? "Saving..." : "Add"}
      </button>
    </div>
  );
}

function TextInput({
  value,
  placeholder,
  type = "text",
  onChange
}: {
  value: string;
  placeholder: string;
  type?: string;
  onChange: (value: string) => void;
}) {
  return (
    <input
      value={value}
      type={type}
      onChange={(event) => onChange(event.target.value)}
      placeholder={placeholder}
      className="w-full border border-slate-300 p-3 rounded-md text-sm"
    />
  );
}

function TextArea({
  value,
  placeholder,
  onChange
}: {
  value: string;
  placeholder: string;
  onChange: (value: string) => void;
}) {
  return (
    <textarea
      rows={6}
      value={value}
      onChange={(event) => onChange(event.target.value)}
      placeholder={placeholder}
      className="w-full border border-slate-300 p-3 rounded-md text-sm resize-none"
    />
  );
}

function DataTable({
  loading,
  emptyText,
  headers,
  rows
}: {
  loading: boolean;
  emptyText: string;
  headers: string[];
  rows: ReactNode[][];
}) {
  return (
    <div className="border border-slate-200 rounded-lg overflow-hidden">
      <table className="w-full table-fixed text-sm">
        <thead className="bg-slate-50">
          <tr>
            {headers.map((header, index) => (
              <th
                key={header}
                className={`p-3 font-semibold text-slate-600 ${
                  index === headers.length - 1
                    ? "w-32 text-right"
                    : "text-left"
                }`}
              >
                {header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {loading && (
            <tr>
              <td
                colSpan={headers.length}
                className="p-6 text-center text-slate-500"
              >
                Loading...
              </td>
            </tr>
          )}
          {!loading && rows.length === 0 && (
            <tr>
              <td
                colSpan={headers.length}
                className="p-6 text-center text-slate-500"
              >
                {emptyText}
              </td>
            </tr>
          )}
          {!loading && rows.map((row, index) => (
            <tr
              key={index}
              className="border-t border-slate-100"
            >
              {row.map((cell, cellIndex) => (
                <td
                  key={cellIndex}
                  className={`p-3 text-slate-700 ${
                    cellIndex === row.length - 1
                      ? "w-32 text-right whitespace-nowrap"
                      : "max-w-sm truncate"
                  }`}
                >
                  {cell}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function DeleteButton({
  onClick
}: {
  onClick: () => Promise<void>;
}) {
  return (
    <button
      onClick={onClick}
      title="Delete"
      className="inline-flex items-center gap-2 rounded-md border border-red-200 px-3 py-1.5 text-sm font-semibold text-red-600 hover:bg-red-50 transition"
    >
      <TrashIcon className="h-4 w-4" />
      <span>Delete</span>
    </button>
  );
}
