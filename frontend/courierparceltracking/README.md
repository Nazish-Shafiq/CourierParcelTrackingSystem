# Courier Parcel Tracking System

## Frontend setup

Install dependencies with `npm install`, then start the frontend with `npm run dev`.
The local `.env` uses `VITE_API_URL=/api` and proxies API requests to the backend's
HTTPS development endpoint at `https://localhost:7075`. Update
`VITE_API_PROXY_TARGET` if the backend is started on a different local address.
The Vite development proxy accepts the local ASP.NET development certificate.
If you run the backend's HTTP-only profile, set `VITE_API_PROXY_TARGET` to
`http://localhost:5014` instead.

For a deployed frontend, set `VITE_API_URL` to the backend API base URL, including
the `/api` path (for example, `https://your-api-host/api`). Cross-origin production
deployments require backend CORS support or a same-origin reverse proxy.
See [API_MAPPING.md](./API_MAPPING.md) for the backend-derived endpoint and DTO map.

## Phase 1 authentication

Sign in through `POST /api/Auth/login` using the username/email and password
provided by an administrator. The frontend keeps the access token in
`sessionStorage`, validates a restored session through `GET /api/Auth/me`, and
discards the token on logout or an unauthorized response. Public self-registration
is not available.
