"use client";

import { useState } from "react";
import type React from "react";
import { validateLoginForm, LoginErrors } from "@/lib/validations";
import { loginUser } from "@/lib/loginUser";
import { useRouter } from "next/navigation";
import Link from "next/link";

export default function LoginForm() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    username: "",
    password: ""
  });

  const [errors, setErrors] = useState<LoginErrors>({});

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const validationErrors = validateLoginForm(formData);
    setErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) return;

    try {
      await loginUser(formData);
      router.replace("/dashboard");
    } catch {
      setErrors({ api: "Invalid credentials or server error." });
    }
  }

  return (
    <form
      className="login-form w-full max-w-md space-y-4 rounded-md border border-border bg-surface p-6 shadow-sm"
      onSubmit={handleSubmit}
    >
      <h2 className="text-2xl font-semibold text-center mb-4">Login</h2>

      {errors.api && <p className="text-danger text-sm">{errors.api}</p>}

      <div className="space-y-1">
        <label className="text-sm font-medium">Email</label>
        <input
          name="username"
          type="email"
          value={formData.username}
          onChange={handleChange}
          className="border rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        {errors.email && <p className="text-danger text-sm">{errors.email}</p>}
      </div>

      <div className="space-y-1">
        <label className="text-sm font-medium">Password</label>
        <input
          type="password"
          name="password"
          value={formData.password}
          onChange={handleChange}
          className="border rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        {errors.password && <p className="text-danger text-sm">{errors.password}</p>}
      </div>

      <div className="-mt-2 flex justify-end">
        <Link
          href="/users/forgot-password"
          className="text-sm font-medium text-accent underline-offset-4 hover:underline"
        >
          Forgot password?
        </Link>
      </div>

      <button
        type="submit"
        className="w-full rounded-md bg-accent py-2 text-accent-contrast transition-colors hover:bg-accent-hover"
      >
        Login
      </button>
    </form>
  );
}
