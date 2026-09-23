import { Secret } from "fernet";

const FERNET_KEY = process.env.NEXT_PUBLIC_PUBLIC_KEY;

if (!FERNET_KEY) {
  throw new Error("VITE_FERNET_KEY is not configured.");
}

export const cryptoService = {
  encrypt(value: string): string {
    const secret = new Secret(FERNET_KEY);
    return secret.encrypt(value);
  },

  decrypt(value: string): string {
    const secret = new Secret(FERNET_KEY);
    return secret.decrypt(value);
  },
};
