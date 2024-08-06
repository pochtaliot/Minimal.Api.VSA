# Minimal.Api.VSA
.NET8 Minimal Web API.
Applied architecture, approaches, practises:

You need to have installed at least MSSQL server Express

1. Minimal APIs
2. Vertical slice architecture
3. CQRS (Mediatr)
4. Conditional Authentication/authorization (None, Bearer, API Key)
5. Swagger
6. Fluent validation
7. EF8

Set "Authentication:Type" property value to None, Bearer or API Key in appsettings.Development.json to select required type.

During first run there will be seeded ProductsApiDb and UsersApiDb.

If you want to test Bearer authorization:
- Set configuration to startup two projects: Minimal.Api.Authentication and Minimal.Api.
- Set "Authentication:Type" property value to Bearer in appsettings.Development.json
- Obtain Bearer token via login endpoint.
Credentials:
Username: user@example.com
Password: string

If you want to test API Key authorization:
- Set "Authentication:Type" property value to ApiKey in appsettings.Development.json
- Set x-api-key http header with "Authentication:Schemes:ApiKey:Key" property value from appsettings.Development.json
