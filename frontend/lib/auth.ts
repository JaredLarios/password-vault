export function isLoggedIn() {
    if (typeof document === "undefined") return false;
    return document.cookie.includes("session=");
}