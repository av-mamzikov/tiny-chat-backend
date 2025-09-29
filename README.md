# WebSocket Group Chat Application

Backend service for a real-time group chat built with ASP.NET Core and WebSockets.

## 🚀 Features

- Real-time messaging over WebSockets
- Multi-user group chat broadcasting
- Thread-safe connection management
- Health check endpoint
- CORS configured for public API

## 🛠️ Technology Stack

- **.NET** (TargetFramework: `net10.0`) – Backend framework
- **WebSockets** – Real-time communication
- **Minimal APIs** – Lightweight HTTP endpoints
- **Docker** – Optional containerization (see note below)

## 🚀 Quick Start (Backend Only)

### Option 1: Run locally (recommended)
```bash
git clone <repository-url>
cd websocket-chat/backend

# Run
dotnet run --project WebSocketChat.Backend/WebSocketChat.Backend.csproj
```

Default URLs (from `WebSocketChat.Backend/Properties/launchSettings.json`):
- HTTP: `http://localhost:5088`
- HTTPS: `https://localhost:7182`

### Option 2: Docker (optional)
The provided `Dockerfile` may not yet reflect the backend-only structure. If you plan to run with Docker, ensure it targets `WebSocketChat.Backend` and exposes the desired port. Example run (after fixing the Dockerfile):
```bash
docker build -t websocket-chat-backend .
docker run -p 5088:5088 -e ASPNETCORE_URLS=http://+:5088 websocket-chat-backend
```
Access at: `http://localhost:5088`

## 💻 Usage

Backend endpoints in `WebSocketChat.Backend`:

- __Health__: `GET /health`
  - Example: `curl http://localhost:5088/health`

- __WebSocket__: `/ws`
  - HTTP: `ws://localhost:5088/ws`
  - HTTPS: `wss://localhost:7182/ws`

## 📝 License

This project is created for personal learning to understand WebSocket technology. Feel free to use, modify, and distribute as you wish.

---