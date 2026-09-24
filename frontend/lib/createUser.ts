import api from "@/lib/axios"

export async function createUser(data: {
  username: string;
  name: string;
  lastName: string;
  password: string;
}) {
  const response = await api.post("/user/register", data);

  if (response.status !== 200) {
    throw new Error("Failed to create user");
  }

  return response.data;
}
