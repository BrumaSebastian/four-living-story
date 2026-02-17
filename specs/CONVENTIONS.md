# Conventions

## Naming
- **Projects**: `FourLivingStory.<Layer>` (PascalCase)
- **Namespaces**: match project name
- **Files**: PascalCase for classes, matching the class name
- **Variables/fields**: camelCase; private fields prefixed with `_`
- **Constants**: PascalCase

## Code Style
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Implicit usings enabled
- Minimal API style for endpoints in ApiService
- Razor components in Web follow Blazor conventions

## Project Conventions
- All new features require a spec in `specs/` before implementation
- Endpoints must have a named route (`.WithName(...)`)
- Health checks required on all services

## Git
- Branch naming: `feature/<name>`, `fix/<name>`, `chore/<name>`
- Commit messages: lowercase imperative (`add`, `fix`, `update`, not `Added`)
- No force-push to `main`

## Testing
- Integration tests use Aspire's `DistributedApplicationTestingBuilder`
- Unit tests (when added) go in `FourLivingStory.Tests`
- Test methods: `<Method>_<Scenario>_<ExpectedResult>`
