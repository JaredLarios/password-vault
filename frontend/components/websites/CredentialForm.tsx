import { useState } from "react";
import type { Credential } from "@/DTO/Credential";
import api from "@/lib/axios";
import PasswordStrengthMeter from "@/components/PasswordStrengthMeter";

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

    const response = await api.post<Credential>("/website", body);

    onSubmit(response.data);
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
      <PasswordStrengthMeter password={password} />

      <button
        className="bg-green-600 text-white px-3 py-1 rounded"
        onClick={handleSubmit}
      >
        Save Credential
      </button>
    </div>
  );
}
