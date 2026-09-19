export async function loginUser(loginData: any) {
  const response = await fetch("/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(loginData)
  });

  if (!response.ok) {
    throw new Error("Login failed");
  }

  return response.json();
}
