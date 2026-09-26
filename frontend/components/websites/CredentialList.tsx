import { useState } from "react";
import CredentialForm from "./CredentialForm";
import type { Credential } from "@/DTO/Credential";

interface CredentialListProps {
  url: string;
  credentials: Credential[];
  onAddCredential: (cred: Credential) => void;
}

export default function CredentialList({
  url,
  credentials,
  onAddCredential,
}: CredentialListProps) {
  const [showForm, setShowForm] = useState(false);

  return (
    <div className="mt-4">
      <h3 className="font-semibold">Credentials for {url}</h3>

      <ul className="list-disc ml-6">
        {credentials.map((cred: Credential, i) => (
          <li key={i}>
            {cred.username}
          </li>
        ))}
      </ul>

      {showForm ? (
        <CredentialForm
          url={url}
          onSubmit={(cred: Credential) => {
            onAddCredential(cred);
            setShowForm(false);
          }}
        />
      ) : (
        <button
          className="mt-2 bg-purple-600 text-white px-3 py-1 rounded"
          onClick={() => setShowForm(true)}
        >
          Add Credential
        </button>
      )}
    </div>
  );
}
