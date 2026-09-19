"use client";

import Link from "next/link";

export default function HomePage() {
  return (
    <div className="max-w-xl mx-auto text-center">
      <h2 className="text-3xl font-bold mb-4">Welcome to Password Vault</h2>
      <p className="mb-6">
        Create an account to securely store and manage your passwords.
      </p>

      <Link
        href="/users/new"
        className="bg-blue-600 text-white px-4 py-2 rounded inline-block"
      >
        Create Account
      </Link>
    </div>
  );
}
