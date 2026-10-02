"use client";

// Form component for creating a new website with multiple URLs
import { useState } from "react";

export default function CreateWebsiteForm() {
    // State for website name and URLs
    const [name, setName] = useState("");
    // State for website URLs
  const [urls, setUrls] = useState([""]);

    // Function to handle changes to individual URL fields
  function handleUrlChange(index: number, value: string) {
    const updated = [...urls];
    updated[index] = value;
    setUrls(updated);
  }

  function addUrlField() {
    setUrls([...urls, ""]);
  }

    // Function to handle form submission
  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

      // Prepare the payload for the API request
    const payload = {
      name,
      urls: urls.filter((u) => u.trim() !== "")
    };

    const res = await fetch("/api/websites/new", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    console.log(data);
  }

  return (
    <form className="space-y-4" onSubmit={handleSubmit}>
      <h2 className="text-xl font-bold">Add Website</h2>

      <input
        className="border p-2 w-full"
        placeholder="Website Name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        required
      />

      {urls.map((url, index) => (
        <input
          key={index}
          className="border p-2 w-full"
          placeholder="URL"
          value={url}
          onChange={(e) => handleUrlChange(index, e.target.value)}
          required
        />
      ))}

      <button
        type="button"
        onClick={addUrlField}
        className="bg-gray-300 px-3 py-1 rounded"
      >
        + Add Another URL
      </button>

      <button
        type="submit"
        className="bg-blue-600 text-white px-4 py-2 rounded"
      >
        Save Website
      </button>
    </form>
  );
}
