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

api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle global errors here (e.g., 401 Unauthorized redirect)
    if (error.response?.status === 401) {
      console.error('Unauthorized! Redirecting...');
    }
    return Promise.reject(error);
  }
);

api.interceptors.request.use(
  (config) => {
    const method = config.method?.toLowerCase();

    const shouldEncrypt =
      method === "post" ||
      method === "put" ||
      method === "patch";

    if (
      shouldEncrypt &&
      config.data !== undefined &&
      config.data !== null
    ) {
      config.data = encryptCredentials(config.data);
      console.log(config.data);
    }

    return config;
  },
);

// -------------------------
// Response interceptor
// -------------------------

api.interceptors.response.use(
  (response) => {
    if (
      response.data !== undefined &&
      response.data !== null
    ) {
      response.data = decryptCredentials(response.data);
      console.log(response.data);
    }

    return response;
  },
);

export default api;
