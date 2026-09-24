import { UserResponseDto } from "@/DTO/UserDto";
import api from "@/lib/axios"
import { AxiosResponse } from "axios";

export async function getCurrentUser(): Promise<AxiosResponse<UserResponseDto> | null> {
  try {
    const response = await api.get<UserResponseDto>("/user/me");

    if (response.status !== 200) return null;

    return response;
  } catch (err) {
    return null;
  }
}
