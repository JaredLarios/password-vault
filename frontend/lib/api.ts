//POST /users/new;

const API_URL = "http://localhost:4000"; // your backend server

export async function createUser(data: Record<string, unknown>) {
  const res = await fetch(`${API_URL}/users/new`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  return res.json();
}