"use client";

import { checkPasswordStrength, type PasswordStrengthLevel } from "@/lib/passwordStrength";

const LEVEL_STYLES: Record<PasswordStrengthLevel, { label: string; color: string; width: string }> = {
  "very-weak": { label: "Very weak", color: "bg-red-500", width: "20%" },
  weak: { label: "Weak", color: "bg-orange-500", width: "45%" },
  fair: { label: "Fair", color: "bg-yellow-500", width: "70%" },
  strong: { label: "Strong", color: "bg-green-600", width: "100%" },
};

interface PasswordStrengthMeterProps {
  password: string;
}

export default function PasswordStrengthMeter({ password }: PasswordStrengthMeterProps) {
  if (!password) return null;

  const { level, suggestions } = checkPasswordStrength(password);
  const style = LEVEL_STYLES[level];

  return (
    <div className="mt-1 space-y-1" role="status">
      <div className="h-1.5 w-full rounded bg-gray-200">
        <div
          className={`h-1.5 rounded transition-all ${style.color}`}
          style={{ width: style.width }}
        />
      </div>
      <p className="text-xs text-gray-600">{style.label}</p>
      {suggestions.length > 0 && (
        <ul className="text-xs text-gray-500 list-disc ml-4">
          {suggestions.map((suggestion) => (
            <li key={suggestion}>{suggestion}</li>
          ))}
        </ul>
      )}
    </div>
  );
}
