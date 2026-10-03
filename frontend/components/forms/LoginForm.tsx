"use client";

import { useState } from "react";
import type React from "react";
import { validateLoginForm, LoginErrors } from "@/lib/validations";
import { loginUser } from "@/lib/loginUser";
import { useRouter } from "next/navigation";

export default function LoginForm() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    username: "",
    password: ""
  });

  const [errors, setErrors] = useState<LoginErrors>({});
  const [success, setSuccess] = useState("");

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
    } catch (err) {
      setErrors({ api: "Invalid credentials or server error." });
    }
  }

  return (
    <form
      className="login-form max-w-sm mx-auto p-6 bg-white shadow-md rounded space-y-4"
      onSubmit={handleSubmit}
    >
      <h2 className="text-2xl font-semibold text-center mb-4">Login</h2>

      {success && <p className="text-green-600 text-sm">{success}</p>}
      {errors.api && <p className="text-red-600 text-sm">{errors.api}</p>}

      <div className="space-y-1">
        <label className="text-sm font-medium">Email</label>
        <input
          name="username"
          type="email"
          value={formData.username}
          onChange={handleChange}
          className="border rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        {errors.email && <p className="text-red-600 text-sm">{errors.email}</p>}
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
        {errors.password && <p className="text-red-600 text-sm">{errors.password}</p>}
      </div>

      <button
        type="submit"
        className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 transition"
      >
        Login
      </button>
    </form>
  );
}
