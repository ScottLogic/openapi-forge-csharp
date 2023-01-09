# Swagger Petstore - OpenAPI 3.0 C# Client Library

This is an auto-generated client library for the Swagger Petstore - OpenAPI 3.0 API, via the `openapi-forge-csharp` template.

## Usage example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenApiForge;

// perform any required configuration for accessing the API here
var config = new Configuration() { }

// register the API client with the DI container
var services = new ServiceCollection();
services.Add(new ServiceDescriptor(typeof(Configuration), config));
Startup.RegisterApiClient(services, config);

// get the API client from the DI container
var serviceProvider = services.BuildServiceProvider();
var api = serviceProvider.GetRequiredService<IApiClientPet>();

// perform an operation
var result = await api.findPetsByStatus(...);
```
