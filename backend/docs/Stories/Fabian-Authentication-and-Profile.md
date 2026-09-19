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

## Encrypted Payload Middleware

The API supports encrypted JSON fields through the `FieldEncryptionMiddleware`.
To use it, send the following header with a JSON request:

```http
X-Encrypted-Payload: true
```

When the header is enabled:

- Every string value in the JSON request body is decrypted before the controller receives it.
- Every string value in a JSON response is encrypted before it is sent to the client.
- The middleware uses `CRYPTO_MIDDLEWARE_SECRET_KEY`.
- The same key and Fernet implementation must be used by the client to encrypt requests and decrypt responses.

Requests without this header continue to use regular plaintext JSON. Passwords are still stored as Argon2 hashes and are never reversibly encrypted in the database.
