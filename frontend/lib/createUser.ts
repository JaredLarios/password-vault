export async function createUser(data: {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}) {
  const response = await fetch("/api/users", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data)
  });

  if (!response.ok) {
    throw new Error("Failed to create user");
  }

  return response.json();
}
