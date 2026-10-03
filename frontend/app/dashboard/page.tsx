"use client";

import { useEffect, useState } from "react";
import { getCurrentUser } from "@/lib/getCurrentUser";
import { UserResponseDto } from "@/DTO/UserDto";
import Link from "next/link";
import SearchWebsites from "@/components/SearchWebsites";

export default function DashboardPage() {
  const [user, setUser] = useState<UserResponseDto|null>(null);

  useEffect(() => {
    async function loadUser() {
      const data = await getCurrentUser();

      if (!data) {
        if (typeof window !== "undefined") {
          window.location.href = "/login";
        }
        return;
      }

      setUser(data.data);
    }

    loadUser();
  }, []);

  if (!user) {
    return <p className="p-6 text-gray-600">Loading your dashboard...</p>;
  }

  return (
    <main className="p-6
    ">
      <h1 className="text-3xl font-bold mb-4">Welcome, {user.name} 👋</h1>

      {/* Profile */}
      <section className="bg-white shadow p-4 rounded border">
        <h2 className="text-xl font-semibold mb-2">Your Profile</h2>

        <p><strong>Name:</strong> {user.name}</p>
        <p><strong>LastName:</strong> {user.lastName}</p>
        <p><strong>Email:</strong> {user.username}</p>
      </section>

      {/* Vault */}
      <section className="mt-6 bg-white shadow p-4 rounded border">
        <h2 className="text-xl font-semibold mb-2">Your Vault</h2>
        <p className="text-gray-600">
          This is where your saved passwords will appear once your vault is connected.
        </p>
      </section>

      {/* Search Websites */}
      <section className="mt-6 bg-white shadow p-4 rounded border">
        <h2 className="text-xl font-semibold mb-2">Search Websites</h2>
        <p className="text-gray-600 mb-4">
          Search your vault for websites with similar names.
        </p>

        <SearchWebsites />
      </section>

      {/* Account Settings */}
      <section className="mt-6 bg-white shadow p-4 rounded border">
        <h2 className="text-xl font-semibold mb-2">Account Settings</h2>
        <p className="text-gray-600">
          Manage your account settings, including changing your password.
        </p>
        <Link href="/users/change-password" className="text-blue-600 hover:underline">
          Change Password
        </Link>
      </section>
    </main>
  );
}
