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
  const [revealedIds, setRevealedIds] = useState<Set<number>>(new Set());

  const toggleReveal = (i: number) => {
    setRevealedIds((prev) => {
      const next = new Set(prev);
      if (next.has(i)) {
        next.delete(i);
      } else {
        next.add(i);
      }
      return next;
    });
  };

  return (
    <div className="mt-4">
      <h3 className="font-semibold">Credentials for {url}</h3>

      <ul className="list-disc ml-6">
        {credentials.map((cred: Credential, i) => (
          <li key={i} className="flex items-center gap-2">
            <span>{cred.username}</span>
            <span className="font-mono">
              {revealedIds.has(i) ? cred.password : "••••••••"}
            </span>
            <button
              type="button"
              aria-label={revealedIds.has(i) ? "Hide password" : "Show password"}
              onClick={() => toggleReveal(i)}
              className="text-sm text-accent"
            >
              {revealedIds.has(i) ? "🙈" : "👁"}
            </button>
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
          className="mt-2 rounded-md bg-accent px-3 py-1 text-accent-contrast hover:bg-accent-hover"
          onClick={() => setShowForm(true)}
        >
          Add Credential
        </button>
      )}
    </div>
  );
}
