import { ApiError } from "./client";

async function uploadFile(kind: "image" | "document", file: File): Promise<{ url: string }> {
  const formData = new FormData();
  formData.append("file", file);

  const res = await fetch(`/api/dashboard/uploads/${kind}`, {
    method: "POST",
    credentials: "include",
    body: formData,
  });

  const isJson = res.headers.get("content-type")?.includes("application/json");
  const body = isJson ? await res.json() : undefined;

  if (!res.ok) throw new ApiError(res.status, body);
  return body as { url: string };
}

export const uploadsApi = {
  image: (file: File) => uploadFile("image", file),
  document: (file: File) => uploadFile("document", file),
};
