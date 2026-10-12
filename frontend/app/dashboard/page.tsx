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
    return <p className="p-6 text-muted-foreground">Loading your dashboard...</p>;
  }

  return (
    <main className="mx-auto max-w-5xl">
      <h1 className="text-3xl font-bold mb-4">Welcome, {user.name} 👋</h1>

      {/* Profile */}
      <section className="rounded-md border border-border bg-surface p-5 shadow-sm">
        <h2 className="text-xl font-semibold mb-2">Your Profile</h2>

        <p><strong>Name:</strong> {user.name}</p>
        <p><strong>LastName:</strong> {user.lastName}</p>
        <p><strong>Email:</strong> {user.username}</p>
      </section>

      {/* Vault */}
      <section className="mt-5 rounded-md border border-border bg-surface p-5 shadow-sm">
        <h2 className="text-xl font-semibold mb-2">Your Vault</h2>
        <p className="text-muted-foreground">
          This is where your saved passwords will appear once your vault is connected.
        </p>
      </section>

      {/* Search Websites */}
      <section className="mt-5 rounded-md border border-border bg-surface p-5 shadow-sm">
        <h2 className="text-xl font-semibold mb-2">Search Websites</h2>
        <p className="mb-4 text-muted-foreground">
          Search your vault for websites with similar names.
        </p>

        <SearchWebsites />
      </section>

      {/* Account Settings */}
      <section className="mt-5 rounded-md border border-border bg-surface p-5 shadow-sm">
        <h2 className="text-xl font-semibold mb-2">Account Settings</h2>
        <p className="text-muted-foreground">
          Manage your account settings, including changing your password.
        </p>
        <Link href="/users/change-password" className="font-medium text-accent underline-offset-4 hover:underline">
          Change Password
        </Link>
      </section>
    </main>
  );
}
