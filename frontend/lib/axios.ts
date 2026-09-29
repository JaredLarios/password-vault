import axios from "axios";
import { decryptCredentials, encryptCredentials } from "./credentialEncryption";

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "",
  timeout: 3000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  const method = config.method?.toLowerCase();
  const shouldEncrypt = ["post", "put", "patch"].includes(method ?? "");

  if (
    shouldEncrypt &&
    config.data !== undefined &&
    config.data !== null &&
    typeof config.data !== "string"
  ) {
    config.data = encryptCredentials(config.data);
  }

  return config;
});

api.interceptors.response.use(
  (response) => {
    if (
      response.data !== undefined &&
      response.data !== null &&
      typeof response.data === "object"
    ) {
      response.data = decryptCredentials(response.data);
    }

    return response;
  },
  (error) => {
    const pathname = typeof window !== "undefined" ? window.location.pathname : "";
    const isPublicRoute = ["/", "/login", "/users/new"].includes(pathname);

    if (
      error.response?.status === 401 &&
      typeof window !== "undefined" &&
      !isPublicRoute
    ) {
      window.location.replace("/login");
    }

    return Promise.reject(error);
  },
);

export default api;
