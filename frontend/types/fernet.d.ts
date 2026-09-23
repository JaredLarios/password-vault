declare module "fernet" {
  export class Secret {
    constructor(secret: string);
  }

  export interface TokenOptions {
    secret: Secret;
    token?: string;
    ttl?: number;
  }

  export class Token {
    constructor(options: TokenOptions);
    encode(value: string): string;
    decode(): string;
  }
}
