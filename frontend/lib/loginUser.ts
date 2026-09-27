import { LoginDto } from "@/DTO/LoginDto";
import api from "@/lib/axios";
import { notifyAuthChanged } from "@/lib/auth";

export async function loginUser(loginData: LoginDto) {
  const response = await api.post("/auth/login", loginData);

  if (response.status !== 200) {
    throw new Error("Login failed");
  }

  notifyAuthChanged();
  return response.data;
}
