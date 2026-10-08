import api from "@/lib/axios";

export async function changePassword(newPassword: string) {
  const response = await api.put("/auth/new-password", { newPassword });

  if (response.status !== 200) {
    throw new Error("Password update failed");
  }

  return response.data;
}