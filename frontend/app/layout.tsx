import "./globals.css";
import type { Metadata } from "next";

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
    <html lang="en">
      <body className="min-h-screen bg-gray-100 text-gray-900">
        <header className="bg-blue-600 text-white p-4">
          <h1 className="text-xl font-bold">Password Vault</h1>
        </header>

        <main className="p-6">{children}</main>

        <footer className="bg-gray-800 text-white p-4 text-center mt-10">
          <p>© 2026 Password Vault</p>
        </footer>
      </body>
    </html>
  );
}
