"use client";

import { useEffect, useState } from "react";
import api from "@/lib/axios";
import { useRouter } from "next/navigation";

interface WebsiteResponse {
  websiteName: string;
  urls: string[];
  credentials: {
    websiteUserId: string;
    websiteUsername: string;
    websitePassword: string;
    leakCount?: number;
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
    leakCount?: number;
  }[];
}

function generateStrongPassword(length = 12) {
  const chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+[]{}<>?";
  let password = "";

  for (let i = 0; i < length; i++) {
    const randomIndex = Math.floor(Math.random() * chars.length);
    password += chars[randomIndex];
  }

  return password;
}

export default function UpdateWebsiteForm({ websiteId }: { websiteId: string }) {
  const router = useRouter();
  const [website, setWebsite] = useState<EditableWebsite | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    alert("Copied to clipboard!");
  };

  useEffect(() => {
    let active = true;

    async function fetchWebsite() {
      setLoading(true);
      setError("");

      try {
        // MOCK DATA FOR TESTING
        // const result = {
        //   websiteName: "Test Website",
        //   urls: ["https://example.com"],
        //   credentials: [
        //     {
        //       websiteUserId: "123",
        //       websiteUsername: "angie",
        //       websitePassword: "mypassword",
        //       leakCount: 0,
        //     }
        //   ]
        // };

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
                  leakCount: credential.leakCount ?? 0,
                }))
              : [{ id: "new-credential", username: "", password: "", leakCount: 0 }],
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
  if (!website) return <p role="alert" className="text-danger">{error || "Website not found."}</p>;

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

  const generatePasswordForCredential = (index: number) => {
    const newPassword = generateStrongPassword(12);
    updateCredential(index, "password", newPassword);
  };

  const handleSubmit = async () => {
    setSaving(true);
    setError("");

    try {
      await api.put(`/website/${websiteId}`, {
        websiteName: website.name,
        websiteUrls: website.urls,
        credentials: website.credentials.map((credential) => ({
          websiteUserId: credential.id,
          websiteUsername: credential.username,
          websitePassword: credential.password,
        })),
      });
      router.push("/websites");
    } catch {
      setError("Error updating website.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-md mx-auto">
      <div className="space-y-3 p-3 border rounded bg-white shadow-sm">
        <h2 className="text-lg font-semibold">Update Website</h2>
        {error && <p role="alert" className="text-red-700 text-sm">{error}</p>}

        {/* Website Name */}
        <div className="space-y-1">
          <label className="text-sm font-medium">Website Name</label>
          <input
            className="border p-1 text-sm h-8 w-full bg-white rounded"
            value={website.name}
            onChange={(e) => updateWebsiteName(e.target.value)}
          />
        </div>

        {/* URLs */}
        <section className="space-y-1">
          <h3 className="text-sm font-semibold">URLs</h3>
          {website.urls.map((url, index) => (
            <input
              key={`${website.id}-url-${index}`}
              aria-label={`URL ${index + 1}`}
              value={url}
              readOnly={index > 0}
              onChange={(event) => updateUrl(index, event.target.value)}
              className="border p-1 text-sm h-8 w-full bg-white rounded"
            />
          ))}
        </section>

        {/* Credentials */}
        <section className="space-y-2">
          <h3 className="text-sm font-semibold">Credentials</h3>

          {website.credentials.map((credential, index) => (
            <div key={credential.id} className="space-y-1 border p-2 rounded">

              {/* Username */}
              <div className="flex gap-2">
                <input
                  aria-label={`Username ${index + 1}`}
                  placeholder="Username"
                  value={credential.username}
                  readOnly={index > 0}
                  onChange={(event) =>
                    updateCredential(index, "username", event.target.value)
                  }
                  className="flex-1 min-w-[140px] border p-1 text-xs h-7 bg-white rounded"
                />

                <button
                  type="button"
                  onClick={() => copyToClipboard(credential.username)}
                  className="bg-gray-200 text-xs px-2 py-1 rounded hover:bg-gray-300"
                >
                  Copy Username
                </button>
              </div>

              {/* Password */}
              <div className="flex gap-2">
                <input
                  aria-label={`Password ${index + 1}`}
                  placeholder="Password"
                  type="password"
                  value={credential.password}
                  readOnly={index > 0}
                  onChange={(event) =>
                    updateCredential(index, "password", event.target.value)
                  }
                  className="flex-1 min-w-[140px] border p-1 text-xs h-7 bg-white rounded"
                />

                <button
                  type="button"
                  onClick={() => copyToClipboard(credential.password)}
                  className="bg-gray-200 text-xs px-2 py-1 rounded hover:bg-gray-300"
                >
                  Copy Password
                </button>

                {index === 0 && (
                  <button
                    type="button"
                    onClick={() => generatePasswordForCredential(index)}
                    className="bg-purple-600 text-white text-xs px-2 py-1 rounded hover:bg-purple-700"
                  >
                    Generate Random Password
                  </button>
                )}
              </div>

              {/* Leak Counter */}
              <p className="text-xs text-gray-600">
                Leaks: {credential.leakCount ?? 0}
              </p>

              {(credential.leakCount ?? 0) > 1 && (
                <p className="text-xs text-red-600 font-semibold">
                  ⚠️ Warning: This password has been leaked multiple times!
                </p>
              )}

            </div>
          ))}
        </section>

        <button
          className="rounded-md bg-accent text-sm px-3 py-1.5 text-accent-contrast hover:bg-accent-hover"
          disabled={saving}
          onClick={handleSubmit}
        >
          {saving ? "Saving..." : "Save"}
        </button>
      </div>
    </div>
  );
}
