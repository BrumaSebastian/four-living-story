# Architecture

## Overview
.NET Aspire application (net10.0) with a Blazor Server frontend, a Minimal API backend, and Redis for output caching.

## Projects
| Project | Role |
|---|---|
| `FourLivingStory.AppHost` | Aspire orchestration — wires up all services |
| `FourLivingStory.ApiService` | Minimal API — business logic and data access |
| `FourLivingStory.Web` | Blazor Server — UI, calls ApiService via service discovery |
| `FourLivingStory.ServiceDefaults` | Shared defaults — OpenTelemetry, health checks, resilience |
| `FourLivingStory.Tests` | Integration tests — spins up full AppHost via Aspire testing |

## Infrastructure
- **Cache**: Redis (output caching on Web)
- **Observability**: OpenTelemetry (traces, metrics, logs)
- **Health checks**: `/health` on both ApiService and Web

## Data Flow
```
User → Blazor Server (Web)
         → HttpClient (service discovery: https+http://apiservice)
             → Minimal API (ApiService)
                 → [Database / external services TBD]
```

## Architectural Decisions
> Record significant decisions here as the project evolves.

-
