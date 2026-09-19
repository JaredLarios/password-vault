export async function getCurrentUser() {
  try {
    const response = await fetch("http://localhost:4000/auth/me", {
      method: "GET",
      credentials: "include"
    });

    if (!response.ok) return null;

    return await response.json();
  } catch (err) {
    return null;
  }
}
