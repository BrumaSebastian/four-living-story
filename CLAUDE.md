# FourLivingStory

## Project Overview
A .NET Aspire application (net10.0). Currently a scaffold — the actual domain is to be built out.

## Structure
```
four-living-story/
├── specs/                        # Feature specs (generate here before coding)
├── src/FourLivingStory/
│   ├── FourLivingStory.slnx
│   ├── FourLivingStory.AppHost/          # Aspire orchestration entry point
│   ├── FourLivingStory.ApiService/       # Minimal API (OpenAPI enabled)
│   ├── FourLivingStory.Web/              # Blazor Server frontend
│   ├── FourLivingStory.ServiceDefaults/  # Shared: OpenTelemetry, health checks
│   └── FourLivingStory.Tests/            # xUnit v3 integration tests (Aspire testing)
└── CLAUDE.md
```

## Key Commands
```bash
# Run the application (from solution root)
dotnet run --project src/FourLivingStory/FourLivingStory.AppHost

# Run tests
dotnet test src/FourLivingStory/FourLivingStory.Tests

# Build
dotnet build src/FourLivingStory/FourLivingStory.slnx
```

## Architecture
- **Aspire orchestration**: AppHost wires up Redis cache, ApiService, and Web frontend
- **ApiService**: Minimal API style, health check at `/health`, OpenAPI at `/openapi/v1.json`
- **Web**: Blazor Server with Redis output caching, calls ApiService via service discovery (`https+http://apiservice`)
- **Tests**: Aspire integration tests that spin up the full AppHost

## Specs Convention
Before implementing any feature, create a spec in `specs/<feature-name>.md`.
Specs should cover: goal, user stories, data model changes, API changes, UI changes.
