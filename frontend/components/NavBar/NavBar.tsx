"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { logoutUser } from "@/lib/logoutUser";
import { useIsAuthenticated } from "@/components/AuthProvider";

export default function NavBar() {
  const pathname = usePathname();
  const router = useRouter();
  const loggedIn = useIsAuthenticated();

  if (pathname === "/login" || !loggedIn) return null;

  const links = [
    { href: "/dashboard", label: "Dashboard" },
    { href: "/websites", label: "Websites" },
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
    <nav className="flex flex-wrap items-center gap-2 border-b border-border bg-surface px-4 py-3 sm:px-6">
      {links.map((link) => {
        const isActive = pathname === link.href;

        return (
          <Link
            key={link.href}
            href={link.href}
            className={`rounded-md px-3 py-2 text-sm font-medium ${
              isActive
                ? "nav-link-active"
                : "nav-link-inactive"
            }`}
          >
            {link.label}
          </Link>
        );
      })}

      {/* Logout Button */}
      <button
        onClick={handleLogout}
        className="logout-btn ml-auto rounded-md px-3 py-2 text-sm font-medium"
      >
        Logout
      </button>
    </nav>
  );
}
