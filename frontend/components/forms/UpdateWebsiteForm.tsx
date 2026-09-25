"use client";

import { Website } from "@/DTO/Website";
import { useEffect, useState } from "react";
import { Credential } from "@/DTO/Credential";

export default function UpdateWebsiteForm({ websiteId }: { websiteId: string }) {
  const [website, setWebsite] = useState<Website | null>(null);
  const [loading, setLoading] = useState(true);

  // 1. Load existing website data
  useEffect(() => {
    async function fetchWebsite() {
      const res = await fetch(`/api/websites/${websiteId}`);
      const data = await res.json();
      setWebsite(data);
      setLoading(false);
    }
    fetchWebsite();
  }, [websiteId]);

  if (loading || !website) return <p>Loading...</p>;

  // 2. Update website name
  const updateWebsiteName = (value: string) => {
    setWebsite({ ...website, name: value });
  };

  // 3. Update URL value
  const updateUrl = (index: number, value: string) => {
    const updatedUrls = [...website.urls];
    updatedUrls[index].url = value;
    setWebsite({ ...website, urls: updatedUrls });
  };

  // 4. Update credential fields
  const updateCredential = (
  urlIndex: number,
  credIndex: number,
  field: keyof Credential,
  value: string
) => {
  const updatedUrls = [...website.urls];
  updatedUrls[urlIndex].credentials[credIndex][field] = value;
  setWebsite({ ...website, urls: updatedUrls });
};


  // 5. Submit update
  const handleSubmit = async () => {
    const res = await fetch(`/api/websites/${websiteId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(website),
    });

    if (!res.ok) {
      alert("Error updating website");
      return;
    }

    alert("Website updated successfully!");
  };

  return (
    <div className="space-y-6 p-6 border rounded bg-white shadow">
      <h2 className="text-2xl font-bold">Update Website</h2>

      {/* Website Name */}
      <div>
        <label className="font-semibold">Website Name</label>
        <input
          className="border p-2 w-full"
          value={website.name}
          onChange={(e) => updateWebsiteName(e.target.value)}
        />
      </div>

      {/* URLs + Credentials */}
      {website.urls.map((urlEntry, urlIndex) => (
        <div key={urlEntry.id} className="border p-4 rounded bg-gray-50">
          <label className="font-semibold">URL #{urlIndex + 1}</label>
          <input
            className="border p-2 w-full mb-3"
            value={urlEntry.url}
            onChange={(e) => updateUrl(urlIndex, e.target.value)}
          />

          <h3 className="font-semibold">Credentials</h3>

          {urlEntry.credentials.map((cred, credIndex) => (
            <div key={cred.id} className="mt-2 p-3 border rounded bg-white">
              <input
                className="border p-2 w-full mb-2"
                placeholder="Username"
                value={cred.username}
                onChange={(e) =>
                  updateCredential(urlIndex, credIndex, "username", e.target.value)
                }
              />

              <input
                className="border p-2 w-full"
                placeholder="Password"
                type="password"
                value={cred.password}
                onChange={(e) =>
                  updateCredential(urlIndex, credIndex, "password", e.target.value)
                }
              />
            </div>
          ))}
        </div>
      ))}

      <button
        className="bg-blue-600 text-white px-4 py-2 rounded"
        onClick={handleSubmit}
      >
        Save Changes
      </button>
    </div>
  );
}
