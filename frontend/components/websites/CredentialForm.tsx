import { useState } from "react";
import type { Credential } from "@/DTO/Credential";

interface CredentialFormProps {
  url: string;
  onSubmit: (cred: Credential) => void;
}

export default function CredentialForm({ url, onSubmit }: CredentialFormProps) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = async () => {
    if (!username || !password) return alert("All fields required");

    const body = { url, username, password };

    await fetch("/api/credentials/new", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });

    onSubmit(body);
  };

  return (
    <div className="border p-3 mt-3 rounded bg-gray-50">
      <input
        className="border p-2 w-full mb-2"
        placeholder="Username / Email"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
        required
      />

      <input
        className="border p-2 w-full mb-2"
        placeholder="Password"
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
      />

      <button
        className="bg-green-600 text-white px-3 py-1 rounded"
        onClick={handleSubmit}
      >
        Save Credential
      </button>
    </div>
  );
}
