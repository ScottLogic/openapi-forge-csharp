## OpenAPI Forge - C#

This repository is the C# generator for the [OpenAPI Forge](https://github.com/ScottLogic/openapi-forge), see that repository for usage instructions:

https://github.com/ScottLogic/openapi-forge

## Example

You should consult the [OpenAPI Forge](https://github.com/ScottLogic/openapi-forge) repository for a complete user guide. The following is a very brief example that quickly gets you up-and-running with this generator.

Run the `forge` command to generate a client API using this generator as follows:

```
$ openapi-forge forge \
                https://petstore3.swagger.io/api/v3/openapi.json \
                openapi-forge-csharp \
                -o ApiTest
```

This will generate various files in the `api` folder.

### Running the example output

The following provides a brief set of instructions for running the pet store example using the dotnet CLI.

First generate a new console application:

```shell
$ dotnet new console -o ApiTest -f net7.0
```

Generate the pet store API client within the same folder:

```shell
$ openapi-forge forge \
                https://petstore3.swagger.io/api/v3/openapi.json \
                openapi-forge-csharp \
                -o ApiTest
```

Within the `ApiTest` folder you'll find a generated `ApiTest.csproj` file. Add the following assembly references:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="7.0.0" />
  <PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />
</ItemGroup>
```

Finally update `Programe.cs` to the following:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenApiForge;

// perform any required configuration for accessing the API here
var config = new Configuration() {
  BasePath = "https://petstore3.swagger.io"
};

// register the API client with the DI container
var services = new ServiceCollection();
services.Add(new ServiceDescriptor(typeof(Configuration), config));
Startup.RegisterApiClient(services, config);

// get the API client from the DI container
var serviceProvider = services.BuildServiceProvider();
var api = serviceProvider.GetRequiredService<IApiClientPet>();

// add a pet
await api.addPet(new Pet() {
  id = 1,
  name = "Fido",
  photoUrls= new string[0],
});

// fetch the pet
var result = await api.getPetById(1);
Console.WriteLine(result.Data.name);
```

Run from the terminal as follows:

```shell
$ dotnet run
Fido
```

### Testing

The standard test script is used to execute the BDD-style tests against this generator.

```
npm run test
```

The script expects that the openapi-forge project (which is where the BDD feature files are located) is checked out at the same folder-level as this project. You also need to have the .NET CLI installed globally, you can confirm this by running `dotnet` in your terminal window.

### Linting

Two scripts are available to help you find linting errors:

```
npm run lint:check:all
```

This runs eslint in check mode which will raise errors found but not try and fix them. This is also ran on a PR and a push to main. It will fail if any errors were found.

```
npm run lint:write:all
```

This runs eslint in write mode which will raise errors found and try to fix them.
