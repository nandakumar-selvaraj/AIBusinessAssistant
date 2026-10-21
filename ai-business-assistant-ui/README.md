# AI Business Assistant UI

Angular 22 dashboard for managing customers, products, and orders, with a chat based business assistant. The UI calls the .NET API through the `/api` path.

## Requirements

- Node.js and npm (the repository uses npm 11)
- The AI Business Assistant API running locally

## Install and run

From this directory:

```bash
npm install
npm start
```

Open [http://localhost:4200](http://localhost:4200). The Angular development server proxies `/api` requests to `https://localhost:7152`, as configured in `proxy.conf.json`. Start the API with its HTTPS launch profile, or update the proxy target to match the API URL.

## Build

```bash
npm run build
```

Production build output is written under `dist/`.

## Unit tests

Run the UI unit tests once with:

```bash
npm test -- --watch=false
```

The test runner uses Vitest with jsdom.

## API URL configuration

The UI uses the `/api` base path defined in `src/app/core/api.config.ts`. In development, `proxy.conf.json` forwards those requests to the API. For another deployment layout, configure the API base URL and the appropriate server routing or proxy together.
