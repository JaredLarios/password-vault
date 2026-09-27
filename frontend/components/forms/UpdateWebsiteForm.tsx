"use client";

import { useEffect, useState } from "react";
import api from "@/lib/axios";
import { router } from "next/client";
import { useRouter } from "next/navigation";

interface WebsiteResponse {
  websiteName: string;
  urls: string[];
  credentials: {
    websiteUserId: string;
    websiteUsername: string;
    websitePassword: string;
  }[];
}

interface EditableWebsite {
  id: string;
  name: string;
  urls: string[];
  credentials: {
    id: string;
    username: string;
    password: string;
  }[];
}

export default function UpdateWebsiteForm({ websiteId }: { websiteId: string }) {
  const router = useRouter();
  const [website, setWebsite] = useState<EditableWebsite | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function fetchWebsite() {
      setLoading(true);
      setError("");

      try {
        const { data } = await api.get<WebsiteResponse[]>("/website", {
          params: { websiteUuid: websiteId },
        });
        const result = Array.isArray(data) ? data[0] : undefined;

        if (!result) {
          if (active) setError("Website not found.");
          return;
        }

        if (active) {
          setWebsite({
            id: websiteId,
            name: result.websiteName,
            urls: result.urls?.length ? result.urls : [""],
            credentials: result.credentials?.length
              ? result.credentials.map((credential) => ({
                  id: credential.websiteUserId,
                  username: credential.websiteUsername,
                  password: credential.websitePassword,
                }))
              : [{ id: "new-credential", username: "", password: "" }],
          });
        }
      } catch {
        if (active) setError("Unable to load this website.");
      } finally {
        if (active) setLoading(false);
      }
    }

    fetchWebsite();

    return () => {
      active = false;
    };
  }, [websiteId]);

  if (loading) return <p>Loading...</p>;
  if (!website) return <p role="alert" className="text-red-700">{error || "Website not found."}</p>;

  const updateWebsiteName = (value: string) => {
    setWebsite({ ...website, name: value });
  };

  const updateUrl = (index: number, value: string) => {
    const updatedUrls = [...website.urls];
    updatedUrls[index] = value;
    setWebsite({ ...website, urls: updatedUrls });
  };

  const updateCredential = (
    index: number,
    field: "username" | "password",
    value: string,
  ) => {
    const updatedCredentials = [...website.credentials];
    updatedCredentials[index] = {
      ...updatedCredentials[index],
      [field]: value,
    };
    setWebsite({ ...website, credentials: updatedCredentials });
  };

  const handleSubmit = async () => {
    setSaving(true);
    setError("");

    try {
      await api.put(`/website/${websiteId}`, {
        websiteName: website.name,
        websiteUrl: website.urls[0],
        websiteUsername: website.credentials[0]?.username,
        websitePassword: website.credentials[0]?.password,
      });
      alert("Website updated successfully!");
    } catch {
      setError("Error updating website.");
    } finally {
      setSaving(false);
      router.push("/websites");
    }
  };

  return (
    <div className="space-y-6 p-6 border rounded bg-white shadow">
      <h2 className="text-2xl font-bold">Update Website</h2>
      {error && <p role="alert" className="text-red-700">{error}</p>}

      {/* Website Name */}
      <div>
        <label className="font-semibold">Website Name</label>
        <input
          className="border p-2 w-full"
          value={website.name}
          onChange={(e) => updateWebsiteName(e.target.value)}
        />
      </div>

      <section className="space-y-3">
        <h3 className="font-semibold">URLs</h3>
        {website.urls.map((url, index) => (
          <input
            key={`${website.id}-url-${index}`}
            aria-label={`URL ${index + 1}`}
            value={url}
            readOnly={index > 0}
            onChange={(event) => updateUrl(index, event.target.value)}
          />
        ))}
      </section>

      <section className="space-y-3">
        <h3 className="font-semibold">Credentials</h3>
        {website.credentials.map((credential, index) => (
          <div key={credential.id} className="space-y-2 border p-3">
            <input
              aria-label={`Username ${index + 1}`}
              placeholder="Username"
              value={credential.username}
              readOnly={index > 0}
              onChange={(event) => updateCredential(index, "username", event.target.value)}
            />
            <input
              aria-label={`Password ${index + 1}`}
              placeholder="Password"
              type="password"
              value={credential.password}
              readOnly={index > 0}
              onChange={(event) => updateCredential(index, "password", event.target.value)}
            />
          </div>
        ))}
      </section>

      <button
        className="bg-blue-600 text-white px-4 py-2 rounded"
        disabled={saving}
        onClick={handleSubmit}
      >
        {saving ? "Saving..." : "Save Changes"}
      </button>
    </div>
  );
}
