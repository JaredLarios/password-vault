import { cryptoService } from "./cryptoService";
import { isCredentialField } from "./credentialFields";

export function encryptCredentials<T>(data: T): T {
  return transformCredentials(
    data,
    cryptoService.encrypt,
  );
}

export function decryptCredentials<T>(data: T): T {
  return transformCredentials(
    data,
    cryptoService.decrypt,
  );
}

function transformCredentials<T>(
  value: T,
  transform: (value: string) => string,
): T {
  if (typeof value === "string") {
    return value;
  }

  if (Array.isArray(value)) {
    return value.map((item) =>
      transformCredentials(item, transform),
    ) as T;
  }

  if (
    value !== null &&
    typeof value === "object"
  ) {
    const result = { ...value } as Record<string, unknown>;

    for (const [key, propertyValue] of Object.entries(result)) {
      if (
        isCredentialField(key) &&
        typeof propertyValue === "string"
      ) {
        result[key] = transform(propertyValue);
        continue;
      }

      result[key] = transformCredentials(
        propertyValue,
        transform,
      );
    }

    return result as T;
  }

  return value;
}