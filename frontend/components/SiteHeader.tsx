"use client";

import Link from "next/link";
import ThemeToggle from "@/components/ThemeToggle";
import { useIsAuthenticated } from "@/components/AuthProvider";

export default function SiteHeader() {
  const isAuthenticated = useIsAuthenticated();

  return (
    <header className="site-header">
      <Link
        href={isAuthenticated ? "/dashboard" : "/"}
        className="rounded-sm text-xl font-bold text-foreground underline-offset-4 hover:underline sm:text-2xl"
      >
        Password Vault
      </Link>
      <ThemeToggle />
    </header>
  );
}