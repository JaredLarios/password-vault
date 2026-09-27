export const authChangeEvent = "password-vault-auth-change";

export function notifyAuthChanged() {
    if (typeof window !== "undefined") {
        window.dispatchEvent(new Event(authChangeEvent));
    }
}