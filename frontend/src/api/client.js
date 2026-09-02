const API_URL = import.meta.env.VITE_API_URL;

export class ApiError extends Error {
  constructor(message, status, data = null) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.data = data;
  }
}

export function getToken() {
  return sessionStorage.getItem("token");
}

export function saveToken(token) {
  sessionStorage.setItem("token", token);
}

export function removeToken() {
  sessionStorage.removeItem("token");
}

function getErrorMessage(data, fallback) {
  if (Array.isArray(data) && data.length > 0) {
    return data.join(" ");
  }

  if (data?.message) {
    return data.message;
  }

  if (typeof data === "string" && data) {
    return data;
  }

  return fallback;
}

export async function apiFetch(path, options = {}) {
  const { auth = true, ...fetchOptions } = options;

  const token = auth ? getToken() : null;

  const headers = new Headers(fetchOptions.headers);

  if (fetchOptions.body && !(fetchOptions.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...fetchOptions,
    headers,
  });

  const text = await response.text();

  let data = null;

  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (!response.ok) {
    const message = getErrorMessage(data, "Något gick fel.");

    if (response.status === 401 && auth && token) {
      window.dispatchEvent(new Event("auth:unauthorized"));
    }

    throw new ApiError(message, response.status, data);
  }

  return data;
}