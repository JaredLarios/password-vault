"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import api from "@/lib/axios";

interface WebsiteCredential {
  websiteUserId: string;
  websiteUsername: string;
  websitePassword: string;
}

interface WebsiteSummary {
  websiteId: string;
  websiteName: string;
  urls: string[];
  credentials: WebsiteCredential[];
}

export default function WebsitesPage() {
  const [websites, setWebsites] = useState<WebsiteSummary[]>([]);
  const [visiblePasswords, setVisiblePasswords] = useState<Record<string, boolean>>({});
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [createForm, setCreateForm] = useState({
    websiteName: "",
    websiteUrl: "",
    websiteUsername: "",
    websitePassword: "",
  });
  const [query, setQuery] = useState("");
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState("");
  const [createSuccess, setCreateSuccess] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [actionMessage, setActionMessage] = useState("");
  const [deletingCredentialId, setDeletingCredentialId] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    const timer = setTimeout(async () => {
      try {
        setLoading(true);
        setError("");

        const params =
          query.trim().length > 0
            ? { websiteName: query.trim() }
            : undefined;

        const response = await api.get<WebsiteSummary[]>("/website",
          {
            params,
            signal: controller.signal,
          }
        );

        
        setWebsites(response.data);
      } catch (error) {
        if (!controller.signal.aborted) {
          console.error(error);
          setError("Unable to load websites. Please try again.");
        }
      } finally {
        setLoading(false);
      }
    }, 500);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [query]);

  function togglePassword(key: string) {
    setVisiblePasswords((current) => ({
      ...current,
      [key]: !current[key],
    }));
  }

  async function handleDeleteCredential(credentialUuid: string) {
    if (!window.confirm("Delete these credentials? This action cannot be undone.")) return;

    setDeletingCredentialId(credentialUuid);
    setDeleteError("");
    setActionMessage("");

    try {
      const { data } = await api.delete<{ message: string }>(`/website/${credentialUuid}`);
      setWebsites((current) => current.map((website) => ({
        ...website,
        credentials: website.credentials.filter(
          (credential) => credential.websiteUserId !== credentialUuid,
        ),
      })));
      setActionMessage(data.message || "Credentials deleted successfully.");
    } catch {
      setDeleteError("Unable to delete these credentials. Please try again.");
    } finally {
      setDeletingCredentialId(null);
    }
  }

  async function handleCreateCredentials(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setCreating(true);
    setCreateError("");
    setCreateSuccess("");

    try {
      const { data: response } = await api.post<{ message: string }>("/Website", createForm);
      setCreateSuccess(response.message || "Credentials created successfully.");
      setCreateForm({
        websiteName: "",
        websiteUrl: "",
        websiteUsername: "",
        websitePassword: "",
      });
      setShowCreateForm(false);

      try {
        const { data } = await api.get<WebsiteSummary[]>("/website/");
        setWebsites(data);
      } catch {
        setError("Credentials were created, but the website list could not be refreshed.");
      }
    } catch {
      setCreateError("Unable to create credentials. Please check the details and try again.");
    } finally {
      setCreating(false);
    }
  }

  return (
    <main className="mx-auto max-w-5xl p-6">
      <header className="mb-6 border-b border-border pb-4">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold">Websites</h1>
            <p className="mt-1 text-sm text-muted-foreground">Your saved website credentials</p>
          </div>
          <button
            type="button"
            className="m-0 w-auto"
            onClick={() => {
              setShowCreateForm((visible) => !visible);
              setCreateError("");
            }}
          >
            {showCreateForm ? "Cancel" : "Create credentials"}
          </button>
        </div>
      </header>

      {createSuccess && <p role="status" className="mb-4 text-success">{createSuccess}</p>}
      {actionMessage && <p role="status" className="mb-4 text-success">{actionMessage}</p>}
      {deleteError && <p role="alert" className="mb-4 text-danger">{deleteError}</p>}

      {showCreateForm && (
        <form
          className="mb-6 space-y-4 rounded-md border border-border bg-surface p-5"
          onSubmit={handleCreateCredentials}
        >
          <h2 className="text-lg font-semibold">New website credentials</h2>
          {createError && <p role="alert" className="text-danger">{createError}</p>}

          <div>
            <label htmlFor="websiteName">Website name</label>
            <input
              id="websiteName"
              name="websiteName"
              required
              value={createForm.websiteName}
              onChange={(event) => setCreateForm({ ...createForm, websiteName: event.target.value })}
            />
          </div>

          <div>
            <label htmlFor="websiteUrl">Website URL</label>
            <input
              id="websiteUrl"
              name="websiteUrl"
              type="url"
              placeholder="https://example.com/"
              required
              value={createForm.websiteUrl}
              onChange={(event) => setCreateForm({ ...createForm, websiteUrl: event.target.value })}
            />
          </div>

          <div>
            <label htmlFor="websiteUsername">Username</label>
            <input
              id="websiteUsername"
              name="websiteUsername"
              required
              value={createForm.websiteUsername}
              onChange={(event) => setCreateForm({ ...createForm, websiteUsername: event.target.value })}
            />
          </div>

          <div>
            <label htmlFor="websitePassword">Password</label>
            <input
              id="websitePassword"
              name="websitePassword"
              type="password"
              required
              value={createForm.websitePassword}
              onChange={(event) => setCreateForm({ ...createForm, websitePassword: event.target.value })}
            />
          </div>

          <button type="submit" className="m-0 w-auto" disabled={creating}>
            {creating ? "Creating..." : "Save credentials"}
          </button>
        </form>
      )}

      {loading && <p role="status">Loading websites...</p>}
      {error && <p role="alert" className="text-danger">{error}</p>}
      {!loading && !error && websites.length === 0 && (
        <p className="text-muted-foreground">No websites saved yet.</p>
      )}

      <div className="space-y-4">
        <input
          type="text"
          placeholder="Search websites..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          className="border p-2 w-full rounded"
        />
        {websites.map((website, websiteIndex) => (
          <article
            key={website.websiteId ?? `${website.websiteName}-${websiteIndex}`}
            className="rounded-md border border-border bg-surface p-5 shadow-sm"
          >
            <header className="mb-4 flex flex-wrap items-center justify-between gap-3">
              <h2 className="text-xl font-semibold">{website.websiteName}</h2>
              <Link
                href={`/websites/edit?id=${encodeURIComponent(website.websiteId)}`}
                className="inline-flex w-auto items-center rounded-md border border-border bg-soft-surface px-3 py-2 text-sm font-medium text-foreground hover:bg-border"
              >
                Edit
              </Link>
            </header>

            <div className="grid gap-6 md:grid-cols-2">
              <section aria-label={`${website.websiteName} URLs`}>
                <h3 className="mb-2 font-semibold">URLs</h3>
                {website.urls.length > 0 ? (
                  <ul className="space-y-1">
                    {website.urls.map((url, urlIndex) => (
                      <li key={`${url}-${urlIndex}`} className="break-all text-sm text-muted-foreground">
                        {url}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-sm text-muted-foreground">No URLs</p>
                )}
              </section>

              <section aria-label={`${website.websiteName} credentials`}>
                <h3 className="mb-2 font-semibold">Credentials</h3>
                {website.credentials.length > 0 ? (
                  <ul className="divide-y divide-gray-200">
                    {website.credentials.map((credential, credentialIndex) => {
                      const key = `${website.websiteId}:${credential.websiteUserId ?? credentialIndex}`;
                      const isVisible = Boolean(visiblePasswords[key]);

                      return (
                        <li key={key} className="py-3 first:pt-0 last:pb-0">
                          <p className="break-all text-sm">
                            <span className="font-medium">Username:</span> {credential.websiteUsername}
                          </p>
                          <div className="mt-1 flex items-center gap-3">
                            <p className="break-all text-sm">
                              <span className="font-medium">Password:</span>{" "}
                              {isVisible ? credential.websitePassword : "********"}
                            </p>
                            <button
                              type="button"
                              className="m-0 w-auto shrink-0 px-2 py-1 text-sm"
                              aria-label={`${isVisible ? "Hide" : "Show"} password for ${credential.websiteUsername}`}
                              aria-pressed={isVisible}
                              onClick={() => togglePassword(key)}
                            >
                              {isVisible ? "Hide" : "Show"}
                            </button>
                            <button
                              type="button"
                              className="m-0 w-auto shrink-0 rounded-md bg-transparent px-2 py-1 text-sm text-danger hover:bg-soft-surface"
                              disabled={!credential.websiteUserId || deletingCredentialId === credential.websiteUserId}
                              aria-label={`Delete credentials for ${credential.websiteUsername}`}
                              onClick={() => handleDeleteCredential(credential.websiteUserId)}
                            >
                              {deletingCredentialId === credential.websiteUserId ? "Deleting..." : "Delete"}
                            </button>
                          </div>
                        </li>
                      );
                    })}
                  </ul>
                ) : (
                  <p className="text-sm text-muted-foreground">No credentials</p>
                )}
              </section>
            </div>
          </article>
        ))}
      </div>
    </main>
  );
}