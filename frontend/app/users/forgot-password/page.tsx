"use client";

import Link from "next/link";
import { useState } from "react";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [submitted, setSubmitted] = useState(false);

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitted(true);
  }

  return (
    <main className="mx-auto max-w-md py-6">
      <section className="rounded-md border border-border bg-surface p-5 shadow-sm sm:p-6">
        <h1 className="mb-2 text-2xl font-bold">Forgot your password?</h1>
        <p className="mb-5 text-sm text-muted-foreground">
          Enter the email address associated with your account.
        </p>

        {submitted ? (
          <p role="status" className="rounded-md border border-border bg-soft-surface p-4 text-sm">
            Password recovery is not available yet. This preview did not send an email or change your password.
          </p>
        ) : (
          <form onSubmit={handleSubmit}>
            <div>
              <label htmlFor="recovery-email">Email</label>
              <input
                id="recovery-email"
                name="email"
                type="email"
                autoComplete="email"
                required
                value={email}
                onChange={(event) => setEmail(event.target.value)}
              />
            </div>
            <button type="submit" className="w-full">
              Request reset link
            </button>
          </form>
        )}

        <Link href="/login" className="mt-5 inline-block text-sm font-medium text-accent underline-offset-4 hover:underline">
          Back to login
        </Link>
      </section>
    </main>
  );
}