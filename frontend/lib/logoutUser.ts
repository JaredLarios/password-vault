import api from "./axios";
import { notifyAuthChanged } from "./auth";

export async function logoutUser() {
  const response = await api.post("auth/logout");

  if (response.status !== 200) {
    throw new Error("Logout failed");
  }

  notifyAuthChanged();
  return response.data;
}
