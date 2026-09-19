"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { logoutUser } from "@/lib/logoutUser";
import { isLoggedIn } from "@/lib/auth";
import { useEffect, useState } from "react";

export default function NavBar() {
  const pathname = usePathname();
  const router = useRouter();
  const [loggedIn, setLoggedIn] = useState(false);
  
   useEffect(() => {
    setLoggedIn(isLoggedIn());
  }, []);

  const links = [
    { href: "dashboard", label: "Dashboard"},
    { href: "/users/new", label: "Create User" },
    { href: "/login", label: "Login" }
  ];

  async function handleLogout() {
    try {
      await logoutUser();       // calls your backend logout route
      router.push("/login");    // redirects user to login page
    } catch (err) {
      console.error("Logout failed:", err);
    }
  }

  return (
    <nav className="flex gap-4 p-4 bg-gray-100 border-b">
      {links.map((link) => {
        const isActive = pathname === link.href;

        return (
          <Link
            key={link.href}
            href={link.href}
            className={`px-3 py-1 rounded ${
              isActive
                ? "bg-blue-600 text-white"
                : "text-blue-700 hover:bg-blue-200"
            }`}
          >
            {link.label}
          </Link>
        );
      })}

      {/* Logout Button */}
      <button
        onClick={handleLogout}
        className="logout-btn px-3 py-1 rounded text-red-600 hover:bg-red-200"
      >
        Logout
      </button>
    </nav>
  );
}
