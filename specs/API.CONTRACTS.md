# API Contracts

Base URL (local): `https://localhost:<port>`
OpenAPI spec: `/openapi/v1.json`

---

## Conventions
- All endpoints return JSON
- Errors follow RFC 9457 Problem Details (`application/problem+json`)
- Route naming: `Get<Resource>`, `Create<Resource>`, etc.

---

## Endpoints

### Health
| Method | Path | Description |
|---|---|---|
| GET | `/health` | Health check |

---

> Add new endpoint contracts below as features are implemented.

<!--
Template:

### <Resource>

#### GET /resource
**Response 200**
```json
{}
```

#### POST /resource
**Request**
```json
{}
```
**Response 201**
```json
{}
```
-->
