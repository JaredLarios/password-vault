import api from "@/lib/axios"
import { AxiosResponse } from "axios";

export async function createUser(data: {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}) {
  const response: AxiosResponse<any> = await api.get("/WeatherForecast"); // Remove this code part because is a sample of connection with backend.

  if (response.status !== 200) {
    throw new Error("Failed to create user");
  }

  return response.data;
}
