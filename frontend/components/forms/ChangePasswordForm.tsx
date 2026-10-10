"use client";

import { useState } from "react";
import axios from "axios";
import { changePassword } from "@/lib/changePassword";

export default function ChangePasswordForm() {
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();

        setError("");
        setSuccess("");

        if (newPassword.length < 10 || newPassword.length > 15) {
            setError("Password must be between 10 and 15 characters.");
            return;
        }

        if (newPassword !== confirmPassword) {
            setError("Passwords do not match.");
            return;
        }

        setSubmitting(true);
        try {
            const response = await changePassword(newPassword);
            setSuccess(response.message || "Password updated successfully.");
            setNewPassword("");
            setConfirmPassword("");
        } catch (requestError) {
            const message = axios.isAxiosError<{ message?: string }>(requestError)
                ? requestError.response?.data?.message
                : undefined;
            setError(message || "Unable to update your password. Please try again.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <form className="space-y-4" onSubmit={handleSubmit}>
            <h2 className="text-xl font-bold">Change Password</h2>
            {error && <p role="alert" className="text-danger">{error}</p>}
            {success && <p role="status" className="text-success">{success}</p>}
            <input
                className="border p-2 w-full"
                type="password"
                placeholder="New Password"
                minLength={10}
                maxLength={15}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
            />
            <input
                className="border p-2 w-full"
                type="password"
                placeholder="Confirm New Password"
                minLength={10}
                maxLength={15}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
            />
            <button
                type="submit"
                className="rounded-md bg-accent px-4 py-2 text-accent-contrast hover:bg-accent-hover"
                disabled={submitting}
            >
                {submitting ? "Updating..." : "Update Password"}
            </button>
        </form>
    );
}