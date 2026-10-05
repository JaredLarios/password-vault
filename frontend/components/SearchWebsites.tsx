"use client";

import api from "@/lib/axios";
import { useState, useEffect } from "react";

interface WebsiteSearchResult {
  websiteId: string;
  websiteName: string;
  urls: string[];
}

export default function SearchWebsites() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<WebsiteSearchResult[]>([]);
  const [loading, setLoading] = useState(false);

  // Search for websites when the query changes
  useEffect(() => {
    const delay = setTimeout(async () => {
      if (!query) {
        setResults([]);
        return;
      }

      setLoading(true);

      try {
        const res = await api.get(`/website?websiteName=${query}`);
        setResults(res.data);
      } catch (err) {
        console.error("Search failed:", err);
      }

      setLoading(false);
    }, 300); // debounce 300ms

    return () => clearTimeout(delay);
  }, [query]);

  return (
    <div className="space-y-4 max-w-md">
      <input
        type="text"
        placeholder="Search websites..."
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className="border p-2 w-full rounded"
      />

      {loading && <p className="text-gray-500">Searching...</p>}

      {results.length > 0 && (
        <ul className="border rounded p-3 space-y-2">
          {results.map((site) => (
            <li key={site.websiteId} className="border-b pb-2">
              <p className="font-semibold">{site.websiteName}</p>
              <ul className="ml-4 list-disc">
                {site.urls.map((u: string, i: number) => (
                  <li key={i}>{u}</li>
                ))}
              </ul>
            </li>
          ))}
        </ul>
      )}

      {!loading && query && results.length === 0 && (
        <p className="text-gray-500">No matching websites found.</p>
      )}
    </div>
  );
}
