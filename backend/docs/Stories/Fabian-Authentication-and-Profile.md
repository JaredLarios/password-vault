# Authentication and Profile API Contract

## User Story

As a user, I want to create an account, log in securely, log out, and view my profile so that I can use the password vault safely.

## Create Account

### Endpoint

- URL: `/User/register`
- Method: `POST`
- Success: `200`
- Validation or duplicate username: `400`
- Server error: `500`

### Request Body

```json
{
  "username": "fabian@example.com",
  "name": "Fabian",
  "lastName": "Betancourt",
  "password": "Password123!"
}
```

Rules:

- `username` is required, must be a valid email, and has a maximum of 50 characters.
- `firstName` and `lastName` are required and have a maximum of 50 characters.
- `password` is required, has 8 to 15 characters, and is never returned.

### Success Response

```json
{
  "message": "User created successfully",
  "userId": 1
}
```

## Login

### Endpoint

- URL: `/Auth/login`
- Method: `POST`
- Success: `200`
- Bad credentials or invalid body: `400`
- Server error: `500`

### Request Body

```json
{
  "username": "fabian@example.com",
  "password": "Password123!"
}
```

Rules:

- `username` is required, must be a valid email, and has a maximum of 50 characters.
- `password` is required and has a maximum of 15 characters.

### Success Response

```json
{
  "message": "Logged in successfully"
}
```

An `HttpOnly`, `Secure` cookie named `auth_token` is sent with the response.

## Logout

### Endpoint

- URL: `/Auth/logout`
- Method: `POST`
- Success: `200`

### Success Response

```json
{
  "message": "Logged out successfully"
}
```

The `auth_token` cookie is deleted.

## Profile

### Endpoint

- URL: `/User/me`
- Method: `GET`
- Success: `200`
- Missing or invalid authentication: `401`

The request does not have a body. The authenticated JWT cookie is required.

### Success Response

```json
{
  "username": "fabian@example.com",
  "name": "Fabian",
  "lastName": "Betancourt"
}
```

The password and internal password hash are never included in the response.

## Credential Payload Encryption

The existing `SecurityMiddleware` decrypts credential fields in JSON `POST`, `PUT`, and `PATCH` request bodies and encrypts matching credential fields in JSON responses.

- Credential fields are identified by property names containing or ending in `Username` or `Password`.
- The middleware uses `CRYPTO_MIDDLEWARE_SECRET_KEY`; clients must use the same Fernet key to encrypt request values and decrypt response values.
- Website credentials are additionally encrypted at rest with `CRYPTO_DB_SECRET_KEY`; SHA-256 digests are stored alongside them.
- Unrelated strings such as website names and URLs are not transformed.
