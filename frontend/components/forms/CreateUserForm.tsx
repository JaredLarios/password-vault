"use client";

import { useState } from "react";
import { Errors, validateUserForm } from "../validations";
import { createUser } from "../../lib/createUser";
import { NewUserDto } from "@/DTO/UserDto";

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
    } catch {
      setErrors({ api: "Failed to create user. Try again." });
    }
  }

  return (
    <form className="create-user-form grid grid-cols-1 gap-x-5 gap-y-2 sm:grid-cols-2" onSubmit={handleSubmit}>
      <h2 className="col-span-full text-xl font-semibold">Create New User</h2>

      {success && <p className="success col-span-full">{success}</p>}
      {errors.api && <p className="error col-span-full">{errors.api}</p>}

      <div>
        <label htmlFor="name">Name</label>
        <input
          id="name"
          name="name"
          autoComplete="given-name"
          value={formData.name}
          onChange={handleChange}
          required
        />
        {errors.name && <p className="error mt-1 text-sm">{errors.name}</p>}
      </div>

      <div>
        <label htmlFor="lastName">Last Name</label>
        <input
          id="lastName"
          name="lastName"
          autoComplete="family-name"
          value={formData.lastName}
          onChange={handleChange}
          required
        />
        {errors.lastName && <p className="error mt-1 text-sm">{errors.lastName}</p>}
      </div>

      <div className="sm:col-span-full">
        <label htmlFor="username">Email</label>
        <input
          id="username"
          name="username"
          type="email"
          autoComplete="email"
          value={formData.username}
          onChange={handleChange}
          required
        />
        {errors.username && <p className="error mt-1 text-sm">{errors.username}</p>}
      </div>

      <div>
        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          name="password"
          autoComplete="new-password"
          value={formData.password}
          onChange={handleChange}
          required
        />
        {errors.password && <p className="error mt-1 text-sm">{errors.password}</p>}
      </div>

      <div>
        <label htmlFor="confirmPassword">Confirm Password</label>
        <input
          id="confirmPassword"
          type="password"
          name="confirmPassword"
          autoComplete="new-password"
          value={formData.confirmPassword}
          onChange={handleChange}
          required
        />
        {errors.confirmPassword && <p className="error mt-1 text-sm">{errors.confirmPassword}</p>}
      </div>

      <button className="col-span-full mt-2 w-full" type="submit">Create User</button>
    </form>
  );
}