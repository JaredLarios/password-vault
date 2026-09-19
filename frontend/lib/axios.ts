import axios from "axios";

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

export default api;