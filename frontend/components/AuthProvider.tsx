"use client";

import { createContext, useContext, useEffect, useState } from "react";
import type { ReactNode } from "react";
import { authChangeEvent } from "@/lib/auth";
import { getCurrentUser } from "@/lib/getCurrentUser";

const AuthContext = createContext(false);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    let active = true;

    function refreshAuthentication() {
      void getCurrentUser().then((user) => {
        if (active) setIsAuthenticated(user !== null);
      });
    }

    refreshAuthentication();
    window.addEventListener(authChangeEvent, refreshAuthentication);

    return () => {
      active = false;
      window.removeEventListener(authChangeEvent, refreshAuthentication);
    };
  }, []);

  return (
    <AuthContext.Provider value={isAuthenticated}>
      {children}
    </AuthContext.Provider>
  );
}

export function useIsAuthenticated() {
  return useContext(AuthContext);
}