const credentialNames = ["Username", "Password"];

export function isCredentialField(
  propertyName: string,
): boolean {
  return credentialNames.some((credentialName) => {
    return (
      propertyName.localeCompare(
        credentialName,
        undefined,
        { sensitivity: "accent" },
      ) === 0 ||
      propertyName
        .toLowerCase()
        .endsWith(credentialName.toLowerCase()) ||
      propertyName
        .toLowerCase()
        .startsWith(credentialName.toLowerCase())
    );
  });
}