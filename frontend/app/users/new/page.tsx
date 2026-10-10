"use client";

import CreateUserForm from "@/components/forms/CreateUserForm";

export default function NewUserPage() {
  return (
    <main className="mx-auto my-auto w-full max-w-3xl py-2">
      <h1 className="mb-4 text-3xl font-bold">Create Account</h1>
      <CreateUserForm />
    </main>
  );
}
