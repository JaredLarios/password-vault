export type PasswordStrengthLevel = "very-weak" | "weak" | "fair" | "strong";

export interface PasswordStrengthResult {
  score: number;
  level: PasswordStrengthLevel;
  suggestions: string[];
}

const COMMON_PASSWORDS = new Set([
  "password",
  "12345678",
  "123456789",
  "qwerty123",
  "letmein1",
  "11111111",
  "abc12345",
  "password1",
  "iloveyou",
  "welcome1",
]);

export function checkPasswordStrength(password: string): PasswordStrengthResult {
  if (!password) {
    return { score: 0, level: "very-weak", suggestions: ["Enter a password."] };
  }

  const hasLower = /[a-z]/.test(password);
  const hasUpper = /[A-Z]/.test(password);
  const hasNumber = /\d/.test(password);
  const hasSymbol = /[^A-Za-z0-9]/.test(password);
  const varietyCount = [hasLower, hasUpper, hasNumber, hasSymbol].filter(Boolean).length;
  const isCommon = COMMON_PASSWORDS.has(password.toLowerCase());

  const suggestions: string[] = [];
  if (password.length < 8) suggestions.push("Use at least 8 characters.");
  if (!hasUpper) suggestions.push("Add an uppercase letter.");
  if (!hasLower) suggestions.push("Add a lowercase letter.");
  if (!hasNumber) suggestions.push("Add a number.");
  if (!hasSymbol) suggestions.push("Add a symbol (e.g. !, #, %).");
  if (isCommon) suggestions.push("Avoid common, easily guessed passwords.");

  let score = 0;
  if (password.length >= 8) score += 1;
  if (password.length >= 12) score += 1;
  score += Math.max(0, varietyCount - 1);
  if (isCommon) score = 0;
  score = Math.min(score, 4);

  const level: PasswordStrengthLevel =
    score <= 1 ? "very-weak" : score === 2 ? "weak" : score === 3 ? "fair" : "strong";

  return { score, level, suggestions };
}
