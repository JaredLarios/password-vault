"use client";

import Link from "next/link";

export default function HomePage() {
  return (
    <main className="mx-auto max-w-xl py-8 text-center sm:py-14">
      <h2 className="text-3xl font-bold mb-4">Welcome to Password Vault</h2>
      <p className="mb-6">
        Create an account to securely store and manage your passwords.
      </p>

      <div className="flex flex-wrap justify-center gap-3">
        <Link
          href="/users/new"
          className="inline-flex rounded-md bg-accent px-4 py-2 text-accent-contrast hover:bg-accent-hover"
        >
          Create Account
        </Link>
      </div>
      <p className="mt-6 mb-3 text-sm text-muted-foreground">
        Already have an account? Sign in to open your vault.
      </p>
      <Link
        href="/login"
        className="inline-flex rounded-md border border-border bg-surface px-4 py-2 font-medium text-foreground hover:bg-soft-surface"
      >
        Log in
      </Link>
    </main>
  );
}
