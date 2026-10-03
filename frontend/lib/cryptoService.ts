import * as fernet from "fernet";

function getSecret() {
  const configuredKey = process.env.NEXT_PUBLIC_MIDDLEWARE_SECRET_KEY;

  if (!configuredKey) {
    throw new Error("NEXT_PUBLIC_MIDDLEWARE_SECRET_KEY is not configured.");
  }

  const FERNET_KEY = configuredKey
    .replace(/\+/g, "-")
    .replace(/\//g, "_");

  return new fernet.Secret(FERNET_KEY);
}

export const cryptoService = {
  encrypt(value: string): string {
    const secret = getSecret();
    return new fernet.Token({ secret }).encode(value);
  },

  decrypt(value: string): string {
    const secret = getSecret();
    return new fernet.Token({ secret, token: value, ttl: 0 }).decode();
  },
};
