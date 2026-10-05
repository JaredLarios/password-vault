## Frontend Documentation

This is the [Next.js](https://nextjs.org) app for Password Vault.

### Run Project
1. Set env variables in an `.env.local` file:
```text
NEXT_PUBLIC_API_URL=<BACKEND-API-URL>
NEXT_PUBLIC_MIDDLEWARE_SECRET_KEY=<44-CHAR-BASE64-FERNET-KEY>
```

2. Install project dependencies:
```powershell
npm install
```

3. Run the development server:
```powershell
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

### Other scripts
```powershell
npm run build   # production build
npm run start   # run a production build
npm run lint    # lint the project
```
