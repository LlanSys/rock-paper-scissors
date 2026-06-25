# Rock Paper Scissors API (Wu Xing Edition)

A modernized .NET Web API built with Clean Architecture, CQRS (MediatR), and API Versioning. This repository contains the solution to the 5-way elemental variant of Rock-Paper-Scissors featuring **Fire, Water, Wood, Metal, and Earth**.

## Rules
- Fire burns Wood and melts Metal
- Wood displaces Earth and absorbs Water
- Water extinguishes Fire and rusts Metal
- Earth extinguishes Fire and absorbs Water
- Metal cuts Wood and displaces Earth

## Features
- **Clean Architecture:** Strict boundary enforcement using `NetArchTest`. The Domain layer has zero dependencies on Features or Infrastructure.
- **Dynamic Phrasing:** The game engine uses configurable GameOptions prefix/suffix templates (e.g., WinnerTemplate, LoserTemplate, DrawTemplate), making the outcome phrasing customizable via appsettings.json.
- **CQRS Pattern:** Requests are cleanly separated and handled using MediatR.
- **API Versioning:** Fully supports multiple versions of the game endpoints (`v1` and `v2`).

## API Endpoints

The API provides interactive API documentation using Scalar.

![Scalar Home](docs/images/scalar-home.png)

### 1. Play Single (`v2`)
Play a single round of the elemental Rock-Paper-Scissors game against the computer.

**`POST`** `/api/v2/game/playsingle`
```json
{
  "action": "metal"
}
```

**Example Response:**
![Play Single Response](docs/images/scalar-v2-play-single.png)

### 2. Play Multiple (`v2`)
Play multiple rounds at once by providing a list of actions.

**`POST`** `/api/v2/game/playmultiple`
```json
{
  "actions": ["fire", "earth"]
}
```

**Example Response:**
![Play Multiple Response](docs/images/scalar-v2-play-multiple.png)

### 3. Play Single (`v1` Legacy)
Play a single round of the classic Rock-Paper-Scissors game against the computer using the backward-compatible v1 API.

**`POST`** `/api/v1/game/playsingle`
```json
{
  "action": "rock"
}
```

**Example Response:**
![Play Single v1 Response](docs/images/scalar-v1-play-single.png)

**Invalid Input Example:**
![Play Single v1 Invalid Input](docs/images/scalar-v1-play-single-invalid-input.png)

### 4. Play Multiple (`v1` Legacy)
Play multiple rounds of classic Rock-Paper-Scissors at once.

**`POST`** `/api/v1/game/playmultiple`
```json
{
  "actions": ["rock", "paper", "fire"]
}
```

**Example Response:**
![Play Multiple v1 Response](docs/images/scalar-v1-play-multiple.png)

## Getting Started

### How to Run
Ensure you have the .NET 10 SDK installed. You can run the API from the command line:

```bash
dotnet run --project src/Services/RockPaperScissors/Rps.Api.Http/Rps.Api.Http.csproj
```

Once running, navigate to `http://localhost:<port>/scalar/v2` in your browser to view and interact with the API endpoints.

### How to Test
The solution includes Unit Tests, Integration Tests, and Architecture Enforcement tests. Run the entire test suite using:

```bash
dotnet test
```

### Reviewing the Solution
If you are reviewing this codebase, we highly recommend reading the comprehensive [Refactoring Document](docs/REFACTORING.md) which details the architectural shifts, performance improvements, and bug fixes made during the modernization process from the legacy V1 API to the clean architecture V2 API.
