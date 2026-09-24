"use client";

import CreateUserForm from "@/components/forms/CreateUserForm";

export default function NewUserPage() {
  return (
    <main className="p-6 max-w-xl mx-auto">
      <h1 className="text-2xl font-bold mb-4">Create Account</h1>
      <CreateUserForm />
    </main>
  );
}
