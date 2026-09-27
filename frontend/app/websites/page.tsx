"use client";

import { useEffect, useState } from "react";
import api from "@/lib/axios";

interface WebsiteCredential {
  websiteUserId: string;
  websiteUsername: string;
  webistePassword: string;
}

interface WebsiteSummary {
  websiteUuid?: string;
  websiteName: string;
  urls: string[];
  credentials: WebsiteCredential[];
}

export default function WebsitesPage() {
  const [websites, setWebsites] = useState<WebsiteSummary[]>([]);
  const [visiblePasswords, setVisiblePasswords] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    api
      .get<WebsiteSummary[]>("/website/")
      .then(({ data }) => {
        if (!cancelled) setWebsites(data);
      })
      .catch(() => {
        if (!cancelled) setError("Unable to load your websites. Please try again.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  function togglePassword(key: string) {
    setVisiblePasswords((current) => ({
      ...current,
      [key]: !current[key],
    }));
  }

  return (
    <main className="mx-auto max-w-5xl p-6">
      <header className="mb-6 border-b border-gray-300 pb-4">
        <h1 className="text-2xl font-bold">Websites</h1>
        <p className="mt-1 text-sm text-gray-600">Your saved website credentials</p>
      </header>

      {loading && <p role="status">Loading websites...</p>}
      {error && <p role="alert" className="text-red-700">{error}</p>}
      {!loading && !error && websites.length === 0 && (
        <p className="text-gray-600">No websites saved yet.</p>
      )}

      <div className="space-y-4">
        {websites.map((website, websiteIndex) => (
          <article
            key={website.websiteUuid ?? `${website.websiteName}-${websiteIndex}`}
            className="border border-gray-300 bg-white p-5"
          >
            <h2 className="mb-4 text-xl font-semibold">{website.websiteName}</h2>

            <div className="grid gap-6 md:grid-cols-2">
              <section aria-label={`${website.websiteName} URLs`}>
                <h3 className="mb-2 font-semibold">URLs</h3>
                {website.urls.length > 0 ? (
                  <ul className="space-y-1">
                    {website.urls.map((url, urlIndex) => (
                      <li key={`${url}-${urlIndex}`} className="break-all text-sm text-gray-700">
                        {url}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-sm text-gray-500">No URLs</p>
                )}
              </section>

              <section aria-label={`${website.websiteName} credentials`}>
                <h3 className="mb-2 font-semibold">Credentials</h3>
                {website.credentials.length > 0 ? (
                  <ul className="divide-y divide-gray-200">
                    {website.credentials.map((credential, credentialIndex) => {
                      const key = `${website.websiteUuid ?? website.websiteName}:${credential.websiteUserId ?? credentialIndex}`;
                      const isVisible = Boolean(visiblePasswords[key]);

                      return (
                        <li key={key} className="py-3 first:pt-0 last:pb-0">
                          <p className="break-all text-sm">
                            <span className="font-medium">Username:</span> {credential.websiteUsername}
                          </p>
                          <div className="mt-1 flex items-center gap-3">
                            <p className="break-all text-sm">
                              <span className="font-medium">Password:</span>{" "}
                              {isVisible ? credential.webistePassword : "********"}
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
                          </div>
                        </li>
                      );
                    })}
                  </ul>
                ) : (
                  <p className="text-sm text-gray-500">No credentials</p>
                )}
              </section>
            </div>
          </article>
        ))}
      </div>
    </main>
  );
}