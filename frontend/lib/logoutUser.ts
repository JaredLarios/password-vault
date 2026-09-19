export async function logoutUser() {
  const response = await fetch("http://localhost:4000/auth/logout", {
    method: "POST",
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error("Logout failed");
  }

  return response.json();
}
