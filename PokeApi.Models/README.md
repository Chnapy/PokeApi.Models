# PokeApi.Models

C# models for [PokéAPI](https://pokeapi.co/) data, to be used with:

- REST API with https://pokeapi.co endpoints,
- files data with https://github.com/PokeApi/api-data JSON files

[![NuGet](https://img.shields.io/nuget/v/PokeApi.Models.svg)](https://www.nuget.org/packages/PokeApi.Models)
[![Target Frameworks](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com)

Package is published automatically whenever the upstream [PokeAPI/api-data](https://github.com/PokeAPI/api-data) repository is updated. The package is always in sync with the latest schemas.

---

## Usage

```sh
dotnet add package PokeApi.Models
```

### Deserializing an API response

```csharp
using System.Net.Http.Json;
using PokeApi.Models;

var http = new HttpClient { BaseAddress = new Uri("https://pokeapi.co") };

// Fetch a single Pokémon by id or name
var pokemon = await http.GetFromJsonAsync<Pokemon>(
    Pokemon.RestEndpoint.Replace("{id}", "pikachu")
);

Console.WriteLine(pokemon?.Name);        // "pikachu"
Console.WriteLine(pokemon?.BaseExperience); // 112
```

### Deserializing an JSON data

```csharp
using PokeApi.Models;

var pokemon = JsonSerializer.Deserialize<Pokemon>(
    Path.Combine("../pokeapi/api-data/data", Pokemon.FileEndpoint)
);

Console.WriteLine(pokemon?.Name);        // "pikachu"
Console.WriteLine(pokemon?.BaseExperience); // 112
```

### Using the Endpoint constant

Each class exposes endpoints that reflect PokeAPI URL and paths patterns:

```csharp
// File endpoint
Console.WriteLine(Version.FileEndpoint);    // "/api/v2/version/$id/index.json"
// List file endpoint
Console.WriteLine(Version.FileEndpointList);    // "/api/v2/version/index.json"

// REST endpoint
Console.WriteLine(Version.RestEndpoint);    // "/api/v2/version/{id}/"
// List REST endpoint
Console.WriteLine(Version.RestEndpointList);    // "/api/v2/version/"

// Build a URL
var url = $"https://pokeapi.co{Version.RestEndpoint}".Replace("{id}", "cheri");
// Build a file path
var url = $"./pokeapi/api-data/data{Version.FileEndpoint}".Replace("$id", "cheri");
```
