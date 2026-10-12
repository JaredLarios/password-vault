import "./globals.css";
import type { Metadata } from "next";
import Script from "next/script";
import { AuthProvider } from "@/components/AuthProvider";
import NavBar from "@/components/NavBar/NavBar";
import SiteHeader from "@/components/SiteHeader";

export const metadata: Metadata = {
  title: "Password Vault",
  description: "Secure password vault for WDD 499",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="flex min-h-screen flex-col bg-background text-foreground">
        <Script id="theme-init" strategy="beforeInteractive">
          {`try { const theme = localStorage.getItem("password-vault-theme"); if (theme === "dark" || theme === "light") document.documentElement.dataset.theme = theme; } catch {}`}
        </Script>
        <AuthProvider>
          <SiteHeader />

          <NavBar />

          <div className="mx-auto flex w-full max-w-6xl flex-1 px-4 py-5 sm:px-6 sm:py-6 lg:py-8">
            {children}
          </div>

          <footer className="site-footer">
            <p>© 2026 Password Vault</p>
          </footer>
        </AuthProvider>
      </body>
    </html>
  );
}
