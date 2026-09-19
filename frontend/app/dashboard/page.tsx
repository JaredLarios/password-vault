"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getCurrentUser } from "@/lib/getCurrentUser";

export default function DashboardPage() {
  const router = useRouter();
  const [user, setUser] = useState<any>(null);

  useEffect(() => {
    async function loadUser() {
      const data = await getCurrentUser();

      if (!data) {
        router.push("/login");
        return;
      }

      setUser(data);
    }

    loadUser();
  }, [router]);

  if (!user) {
    return <p className="p-6 text-gray-600">Loading your dashboard...</p>;
  }

  return (
    <main className="p-6">
      <h1 className="text-3xl font-bold mb-4">Welcome, {user.name} 👋</h1>

      <section className="bg-white shadow p-4 rounded border">
        <h2 className="text-xl font-semibold mb-2">Your Profile</h2>

        <p><strong>Name:</strong> {user.name}</p>
        <p><strong>Email:</strong> {user.email}</p>
        <p><strong>Role:</strong> {user.role_name}</p>
      </section>

      <section className="mt-6 bg-white shadow p-4 rounded border">
        <h2 className="text-xl font-semibold mb-2">Your Vault</h2>
        <p className="text-gray-600">
          This is where your saved passwords will appear once your vault is connected.
        </p>
      </section>
    </main>
  );
}
