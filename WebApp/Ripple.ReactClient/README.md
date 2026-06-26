# Ripple Ticketing React Client

React + Vite UI that consumes the two Web API microservices:

- Event Management API: `https://localhost:5001/api/events`
- Ticket Management API: `https://localhost:6001/api/tickets`

## Features

- Two tabs: Event Management and Ticket Management.
- Event operations:
  - `GET /api/events`
  - `GET /api/events/{id}`
  - `POST /api/events`
  - `PUT /api/events/{id}`
  - `DELETE /api/events/{id}`
- Ticket operations:
  - `POST /api/tickets/purchase`
  - `POST /api/tickets/inventories`
  - `GET /api/tickets/availability/{eventId}`
  - `GET /api/tickets/reports/sales/{eventId}`
- Bearer token input for secured endpoints.
- Formatted response panel with HTTP status, URL, object display, and raw JSON.

## Run the React UI

```bash
cd Ripple.ReactClient
npm install
npm run dev
```

Open the Vite URL shown in the terminal.

## Important CORS note

Your API `appsettings.json` currently allows:

```json
"Cors": {
  "AllowedOrigins": [ "https://localhost:7001", "https://localhost:7002" ]
}
```

Vite usually runs as `http://localhost:7001`, not HTTPS, unless you configure an HTTPS dev certificate. For easiest local testing, add this origin to both API projects:

```json
"Cors": {
  "AllowedOrigins": [
    "https://localhost:7001",
    "https://localhost:7002",
    "http://localhost:7001"
  ]
}
```

Restart both APIs after changing CORS.

## JWT note

All controllers use `[Authorize]` policies. Paste a valid JWT into the Bearer JWT Token field before calling endpoints.

## Microservice interaction reminders

Creating an event calls the Ticket Micro Service to create inventory:

```http
POST https://localhost:5001/api/events
  -> Event API saves event
  -> Event API calls POST https://localhost:6001/api/tickets/inventories
```

Purchasing tickets and checking availability call the Event Micro Service to get event details before reading/updating Ticket DB records.
