"use client";

import { useState } from "react";
import { Errors, validateUserForm } from "../validations";
import { createUser } from "../../lib/createUser";
import { NewUserDto } from "@/DTO/UserDto";
import PasswordStrengthMeter from "@/components/PasswordStrengthMeter";

export default function CreateUserForm() {
  const [formData, setFormData] = useState({
      username: "",
      name: "",
      lastName: "",
      password: "",
      confirmPassword: "",
    });

  const [errors, setErrors] = useState<Errors>({});
  const [success, setSuccess] = useState("");

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();

    const validationErrors = validateUserForm(formData);
    setErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) return;

    try {
      await createUser(formData as NewUserDto);
      setSuccess("User created successfully!");
      window.location.replace("/login");
    } catch (err) {
      setErrors({ api: "Failed to create user. Try again." });
    }
  }

  return (
    <form className="create-user-form" onSubmit={handleSubmit}>
      <h2>Create New User</h2>

      {success && <p className="success">{success}</p>}
      {errors.api && <p className="error">{errors.api}</p>}

      <label>Name</label>
      <input
        name="name"
        type="name"
        value={formData.name}
        onChange={handleChange}
      />
      {errors.name && <p className="error">{errors.name}</p>}

      <label>Last Name</label>
      <input
        name="lastName"
        type="lastName"
        value={formData.lastName}
        onChange={handleChange}
      />
      {errors.lastName && <p className="error">{errors.lastName}</p>}

      <label>Email</label>
      <input
        name="username"
        type="email"
        value={formData.username}
        onChange={handleChange}
      />
      {errors.username && <p className="error">{errors.username}</p>}

      <label>Password</label>
      <input
        type="password"
        name="password"
        value={formData.password}
        onChange={handleChange}
      />
      {errors.password && <p className="error">{errors.password}</p>}
      <PasswordStrengthMeter password={formData.password} />

      <label>Confirm Password</label>
      <input
        type="password"
        name="confirmPassword"
        value={formData.confirmPassword}
        onChange={handleChange}
      />
      {errors.confirmPassword && <p className="error">{errors.confirmPassword}</p>}

      <button type="submit">Create User</button>
    </form>
  );
}