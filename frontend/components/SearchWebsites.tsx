"use client";

import api from "@/lib/axios";
import { useState, useEffect } from "react";

export default function SearchWebsites() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

    // Search for websites when the query changes
  useEffect(() => {
    if (!query) {
      setResults([]);
      return;
    }

    const delay = setTimeout(async () => {
      setLoading(true);

      try {
        const res = await api.get(`/website?websiteName=${query}`);
        const data = await res.data;
        setResults(data);
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
            <li key={site.id} className="border-b pb-2">
              <p className="font-semibold">{site.name}</p>
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



