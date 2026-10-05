"use client";

// Form component for changing a user's password
import { useState } from "react";
import PasswordStrengthMeter from "@/components/PasswordStrengthMeter";

export default function ChangePasswordForm() {
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");

        // Function to handle form submission
    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        const payload = {
            currentPassword,
            newPassword
        };
        // Send the API request to change the password
        const res = await fetch("/api/password/change", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        const data = await res.json();
        console.log(data);
    }

    return (
        <form className="space-y-4" onSubmit={handleSubmit}>
            <h2 className="text-xl font-bold">Change Password</h2>
            <input
                className="border p-2 w-full"
                type="password"
                placeholder="Current Password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
            />
            <input
                className="border p-2 w-full"
                type="password"
                placeholder="New Password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
            />
            <PasswordStrengthMeter password={newPassword} />
            <button
                type="submit"
                className="bg-blue-600 text-white px-4 py-2 rounded"
            >
                Update Password
            </button>
        </form>
    );
}