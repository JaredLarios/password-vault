"use client";

import { useState } from "react";
import type React from "react";
import { validateLoginForm, LoginErrors } from "@/lib/validations";
import { useRouter } from 'next/navigation';
import { loginUser } from "@/lib/loginUser";

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
    <form className="login-form" onSubmit={handleSubmit}>
      <h2>Login</h2>

      {success && <p className="success">{success}</p>}
      {errors.api && <p className="error">{errors.api}</p>}

      <label>Email</label>
      <input
        name="username"
        type="email"
        value={formData.username}
        onChange={handleChange}
      />
      {errors.email && <p className="error">{errors.email}</p>}

      <label>Password</label>
      <input
        type="password"
        name="password"
        value={formData.password}
        onChange={handleChange}
      />
      {errors.password && <p className="error">{errors.password}</p>}

      <button type="submit">Login</button>
    </form>
  );
}
