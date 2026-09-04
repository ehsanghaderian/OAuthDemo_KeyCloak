# OAuthDemo

A practical ASP.NET Core demonstration of securing a REST API using **OAuth 2.0 Client Credentials Flow** and **JWT Bearer Access Tokens**.

The project demonstrates how a machine-to-machine client can authenticate with an authorization server, obtain an access token, and use that token to access protected API endpoints.

## Architecture

```text
┌────────────────────┐
│  OAuth Client      │
│  (Company / App)   │
└─────────┬──────────┘
          │
          │ Client Credentials
          ▼
┌────────────────────┐
│ Authorization      │
│ Server             │
│                    │
│ Token Endpoint     │
└─────────┬──────────┘
          │
          │ JWT Access Token
          ▼
┌────────────────────┐
│ ASP.NET Core API   │
│                    │
│ JWT Authentication │
│ Authorization      │
└─────────┬──────────┘
          │
          ▼
      Protected
      Resources
```

## What is OAuth 2.0?

OAuth 2.0 is an authorization framework that allows an application to obtain limited access to a protected resource without sharing the user's credentials with that resource.

For machine-to-machine communication, OAuth 2.0 provides the **Client Credentials Grant**.

In this flow:

1. A client application authenticates with the authorization server.
2. The authorization server validates the client.
3. The authorization server issues an access token.
4. The client sends the access token to the API.
5. The API validates the token.
6. The API uses the token's claims/scopes to authorize the request.

## Client Credentials Flow

```text
Client
  │
  │ client_id + client_secret
  ▼
Authorization Server
  │
  │ access_token
  ▼
Client
  │
  │ Authorization: Bearer <access_token>
  ▼
Protected API
  │
  │ validate token
  ▼
Resource
```

This flow is appropriate when there is **no user directly involved** in the request.

Typical examples include:

* Company A calling Company B's API
* Microservice-to-microservice communication
* Background services
* Scheduled jobs
* Server-to-server integrations

## Authentication vs Authorization

This project demonstrates both concepts.

### Authentication

Authentication answers:

> Who is calling the API?

The API validates the JWT access token and identifies the client using claims such as:

```text
client_id
sub
iss
aud
```

### Authorization

Authorization answers:

> What is the client allowed to do?

The API can use OAuth scopes or claims to enforce access policies.

For example:

```text
customer.read
customer.write
order.read
order.write
```

A client with:

```text
customer.read
```

can read customers but cannot create or modify them.

## Example Request

After obtaining an access token:

```http
GET /api/customers
Authorization: Bearer eyJhbGciOi...
```

The API validates:

```text
Issuer
Audience
Signature
Expiration
Scopes
Claims
```

If the token is valid and the client has the required permission:

```http
200 OK
```

Otherwise:

```http
401 Unauthorized
```

or:

```http
403 Forbidden
```

## 401 vs 403

This distinction is important.

### 401 Unauthorized

The client has not successfully authenticated.

Examples:

* No access token
* Invalid token
* Expired token
* Invalid token signature

```text
Client
  │
  │ invalid/missing token
  ▼
API
  │
  ▼
401 Unauthorized
```

### 403 Forbidden

The client is authenticated but does not have permission to perform the operation.

```text
Client
  │
  │ valid token
  │ scope = customer.read
  ▼
API
  │
  │ requires customer.write
  ▼
403 Forbidden
```

## Project Structure

A possible structure for the project is:

```text
OAuthDemo/
│
├── OAuthDemo.Api/
│   ├── Controllers/
│   ├── Authorization/
│   ├── Program.cs
│   └── appsettings.json
│
├── OAuthDemo.Client/
│   ├── Program.cs
│   └── appsettings.json
│
└── README.md
```

The API is responsible for protecting resources.

The client represents an external company/application consuming the API.

## Configuration

The API requires the following OAuth configuration:

```json
{
  "Authentication": {
    "Authority": "https://localhost:xxxx",
    "Audience": "oauth-demo-api"
  }
}
```

The exact configuration depends on the authorization server used by the project.

The client requires credentials such as:

```json
{
  "OAuth": {
    "ClientId": "demo-client",
    "ClientSecret": "YOUR_SECRET",
    "TokenEndpoint": "https://localhost:xxxx/connect/token"
  }
}
```

> **Do not commit real client secrets to source control.**

For local development, use environment variables, .NET user secrets, or another secure secrets-management mechanism.

## Running the Demo

### 1. Start the Authorization Server

Start the authorization server first.

It should expose a token endpoint similar to:

```text
/connect/token
```

### 2. Start the API

Run:

```bash
dotnet run
```

The API should be available at something similar to:

```text
https://localhost:5001
```

### 3. Obtain an Access Token

The client sends:

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded
```

with:

```text
grant_type=client_credentials
client_id=demo-client
client_secret=YOUR_SECRET
scope=customer.read
```

The authorization server returns:

```json
{
  "access_token": "eyJhbGciOi...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

### 4. Call the Protected API

Use the returned access token:

```http
GET /api/customers
Authorization: Bearer eyJhbGciOi...
```

The ASP.NET Core API validates the JWT before allowing access to the endpoint.

## ASP.NET Core Authentication

The API uses JWT Bearer authentication.

A simplified configuration looks like:

```csharp
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["Authentication:Authority"];
        options.Audience = configuration["Authentication:Audience"];
    });
```

Authentication middleware validates the incoming access token.

Authorization can then be configured separately:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerRead", policy =>
    {
        policy.RequireClaim("scope", "customer.read");
    });
});
```

An endpoint can require the policy:

```csharp
[Authorize(Policy = "CustomerRead")]
[HttpGet]
public IActionResult GetCustomers()
{
    return Ok();
}
```

## Important OAuth Concepts

### Resource Owner

The entity that owns the protected resource.

### Client

The application requesting access.

In this demo, the client represents an external company or application.

### Authorization Server

The server responsible for authenticating the client and issuing access tokens.

### Resource Server

The API containing protected resources.

In this project, the ASP.NET Core API acts as the resource server.

### Access Token

A credential that the client presents to access protected resources.

Example:

```text
Authorization: Bearer <access_token>
```

### Scope

A permission that defines what the client is allowed to access.

Examples:

```text
customer.read
customer.write
order.read
```

## Why JWT?

The access token in this demo is a JWT.

A JWT commonly contains claims such as:

```json
{
  "iss": "https://identity.example.com",
  "aud": "oauth-demo-api",
  "sub": "demo-client",
  "scope": "customer.read",
  "exp": 1780000000
}
```

The API can validate the JWT without maintaining a server-side session for every request.

## Security Considerations

This project is intended for learning and demonstration purposes.

For production systems:

* Always use HTTPS.
* Never expose client secrets in source control.
* Use secure secret storage.
* Use short-lived access tokens.
* Use appropriate scopes and authorization policies.
* Rotate client credentials when necessary.
* Validate issuer and audience.
* Validate token signatures.
* Validate token expiration.
* Apply rate limiting.
* Log authentication/authorization events without logging tokens or secrets.
* Consider API versioning and backward compatibility.
* Consider mTLS for high-security machine-to-machine integrations.

## OAuth 2.0 vs API Keys

For a simple B2B API, API keys can sometimes be sufficient:

```http
X-API-Key: abc123
```

However, OAuth 2.0 provides a standardized mechanism for:

* Client authentication
* Access tokens
* Token expiration
* Scopes
* Authorization policies
* Delegated access
* Integration with enterprise identity providers

For a multi-company B2B API, OAuth 2.0 Client Credentials is often a strong default.

## When to Use Client Credentials

Use the Client Credentials flow when:

```text
Application
     │
     │
     ▼
Application
```

is communicating without a human user.

Examples:

```text
Company A Backend
        │
        ▼
Your API
```

or:

```text
Order Service
        │
        ▼
Payment Service
```

If a human user is involved, other OAuth/OIDC flows may be more appropriate.

## Future Improvements

Possible extensions to this demo include:

* OAuth 2.0 scopes
* Policy-based authorization
* Role-based authorization
* Refresh tokens where appropriate
* OpenID Connect
* API Gateway
* Rate limiting
* mTLS
* Certificate-based client authentication
* Token introspection
* Token revocation
* Audit logging
* Docker support
* Integration tests
* OpenAPI/Swagger security configuration

## Key Takeaway

The main idea demonstrated by this project is:

```text
OAuth 2.0
    │
    │ Client Credentials Flow
    ▼
Access Token
    │
    │ Bearer Token
    ▼
ASP.NET Core API
    │
    ├── Authentication
    │       └── Who is the client?
    │
    └── Authorization
            └── What can the client do?
```

For a production B2B REST API consumed by external companies, a common architecture is:

```text
HTTPS
  +
OAuth 2.0 Client Credentials
  +
JWT Access Tokens
  +
Scopes / Policy-Based Authorization
  +
Rate Limiting
  +
Audit Logging
```

For higher-security integrations, **mTLS can be added alongside OAuth 2.0** to provide certificate-based client authentication.
