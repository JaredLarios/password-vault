"use client";

import { useSyncExternalStore } from "react";

type Theme = "light" | "dark";
const themeStorageKey = "password-vault-theme";
const themeChangeEvent = "password-vault-theme-change";

function restoreThemeFromStorage() {
  const savedTheme = window.localStorage.getItem(themeStorageKey);

  if (savedTheme === "dark" || savedTheme === "light") {
    document.documentElement.dataset.theme = savedTheme;
  }
}

function subscribe(onThemeChange: () => void) {
  function syncTheme() {
    restoreThemeFromStorage();
    onThemeChange();
  }

  function syncThemeWhenVisible() {
    if (document.visibilityState === "visible") syncTheme();
  }

  window.addEventListener(themeChangeEvent, onThemeChange);
  window.addEventListener("storage", syncTheme);
  window.addEventListener("pageshow", syncTheme);
  document.addEventListener("visibilitychange", syncThemeWhenVisible);

  return () => {
    window.removeEventListener(themeChangeEvent, onThemeChange);
    window.removeEventListener("storage", syncTheme);
    window.removeEventListener("pageshow", syncTheme);
    document.removeEventListener("visibilitychange", syncThemeWhenVisible);
  };
}

function getThemeSnapshot(): Theme {
  return document.documentElement.dataset.theme === "dark" ? "dark" : "light";
}

function getServerThemeSnapshot(): Theme {
  return "light";
}

export default function ThemeToggle() {
  const theme = useSyncExternalStore(
    subscribe,
    getThemeSnapshot,
    getServerThemeSnapshot,
  );

  function toggleTheme() {
    const nextTheme: Theme = theme === "light" ? "dark" : "light";
    document.documentElement.dataset.theme = nextTheme;
    window.localStorage.setItem(themeStorageKey, nextTheme);
    window.dispatchEvent(new Event(themeChangeEvent));
  }

  return (
    <button
      type="button"
      className="theme-toggle"
      aria-label={`Switch to ${theme === "light" ? "dark" : "light"} mode`}
      aria-pressed={theme === "dark"}
      onClick={toggleTheme}
    >
      {theme === "light" ? "Dark mode" : "Light mode"}
    </button>
  );
}