import { useState } from "react";
import CredentialList from "./CredentialList";
import type { Credential } from "@/DTO/Credential";


export default function WebsiteForm() {
  const [websiteName, setWebsiteName] = useState("");
  const [urls, setUrls] = useState<
  { url: string; credentials: Credential[] }[]
  >([{ url: "", credentials: [] }]);

  const handleAddUrl = () => {
    setUrls([...urls, { url: "", credentials: [] }]);
  };

  const handleUrlChange = (index: number, value: string) => {
    const updated = [...urls];
    updated[index].url = value;
    setUrls(updated);
  };

    const handleSubmit = async () => {
    const body = { websiteName, urls };

    await fetch("http://localhost:4000/users/new", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
  };

  return (
    <div className="space-y-4 p-4 border rounded">
      <h2 className="text-xl font-bold">Add New Website</h2>

      <input
        className="border p-2 w-full"
        placeholder="Website Name"
        value={websiteName}
        onChange={(e) => setWebsiteName(e.target.value)}
        required
      />

      {urls.map((item, index) => (
        <div key={index} className="border p-3 rounded">
          <input
            className="border p-2 w-full"
            placeholder="URL"
            value={item.url}
            onChange={(e) => handleUrlChange(index, e.target.value)}
            required
          />

          <CredentialList
            url={item.url}
            credentials={item.credentials}
            onAddCredential={(cred: Credential) => {
              const updated = [...urls];
              updated[index].credentials.push(cred);
              setUrls(updated);
            }}
          />
        </div>
      ))}

      <button
        className="bg-blue-600 text-white px-4 py-2 rounded"
        onClick={handleAddUrl}
      >
        Add Another URL
      </button>

      <button
        className="bg-green-600 text-white px-4 py-2 rounded"
        onClick={handleSubmit}
      >
        Save Website
      </button>
    </div>
  );
}
