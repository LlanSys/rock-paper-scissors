# Code Review — Rock Paper Scissors API

## Code Review Summary

This code review examines the legacy Rock Paper Scissors API, which includes an ASP.NET Core controller (`Rps.Api.Http`) and its accompanying xUnit tests (`Rps.Api.UnitTests`).

The application runs and compiles, but it contains several architectural issues, design problems, performance inefficiencies, and a few critical bugs. The main highlights are:
- Game logic is tightly coupled inside the HTTP controller.
- Unit tests pass without actually checking outcomes due to a string casing mismatch.
- The `PlayMultiple` endpoint has a bug that unconditionally appends an error message.
- Inefficient random selection (sorting with fresh `Random` instances on every request) creates unnecessary overhead.

Here’s a detailed walkthrough of the findings, organized by topic.

---

## Architectural & Design Analysis

### Separation of Concerns & SOLID Principles  
The `GameController` currently handles HTTP concerns, game rules, random move selection, validation, and response formatting all in one place. This violates the Single Responsibility Principle.  

Adding new moves (like Fire, Water, etc.) would require changing the core play methods, which goes against the Open/Closed Principle. The rules live in scattered `if` statements instead of a clean domain model such as a `Move` enum or dedicated `GameEngine` class.

### Coupling & Extensibility  
The controller directly instantiates `Random` and rebuilds the list of actions on every request. There’s no abstraction for rules or move selection, making the code hard to extend or test.  

Also, `GameController` inherits from `Controller` (meant for MVC views). For a pure API, it should inherit from `ControllerBase` to avoid unnecessary MVC overhead.

### Testability, Dependency Injection, & Mockability  
No dependency injection is used — the controller has no constructor and creates everything internally. This makes mocking impossible, especially for the random computer move.  

Tests create the controller directly, tying them to its internal implementation rather than testing through a clean service layer.

---

## API Design & Integration

### DTO Structure & Reusability  
We have `PlayRequest` and `MultiplePlayRequest` classes that feel redundant. A cleaner approach would use a simple batch request DTO for multiple plays.  

Responses are raw strings or string arrays. Modern APIs benefit from structured JSON responses (e.g., wrapped in a consistent envelope with success status, data, and error details).

### Validation, Error Handling, & Status Codes  
Validation is a late fall-through return at the end of the method. It should fail fast, preferably with middleware or model validation.  

Invalid moves currently return a 200 OK with an error string — this should be a proper 400 Bad Request with structured details.  

Input checks are case-sensitive, so “Rock” or “PAPER” incorrectly trigger validation errors.

### Versioning Strategy  
No versioning exists. Changing rules or routes will break existing clients. Adding API versioning (e.g., `/v1/game` and a future `/v2/game`) would allow safe evolution.

---

## Code Quality & Maintainability

### Magic Strings & Hardcoded Rules  
Move names (“rock”, “paper”, “scissors”) and result phrases (“Winner!”, “Loser!”) are repeated string literals. These should be enums and constants for type safety and easier maintenance.

### Code Duplication  
The entire game logic and result formatting is duplicated between `Play` and `PlayMultiple`. Any rule change requires updates in two places.

---

## Performance & Resource Optimization

### Random Number Generation  
The current random selection:
```csharp
actions.OrderBy(a => new Random().Next()).First()
```
creates a new `Random` instance for every comparison during sorting — very inefficient and can produce poor randomness due to rapid seeding. `Random.Shared` is a much better, thread-safe option.

### Memory Allocations  
A list of actions is rebuilt on every request. Since the moves never change, this should be a static readonly collection. There’s also a latent bug: if the controller ever becomes singleton-scoped, the list would keep growing indefinitely.

---

## Security Analysis

### Input Sanitization & Validation  
No null checks or trimming. Inputs should be normalized (lowercased and trimmed) before evaluation. While it doesn’t crash, the handling is fragile.

---

## Unit Tests Analysis

### Silent Test Failure  
The main test `GameControllerShould.PlayCorrectly` uses case-sensitive checks like `result.Contains("Rock")`, but the controller returns lowercase text. As a result, the assertions are never reached and the test always passes silently — even if the logic is wrong.

### Non-Deterministic Tests  
Tests rely on real random moves, so outcomes vary between runs. Tests should be deterministic (via mocks or seeded values).

### Naming & Organization  
The test class name `GameControllerShould` is non-standard (usually `GameControllerTests`). Tests are also organized by web folder structure rather than domain logic, and they test the controller directly instead of the game rules.

---

## Recommended Refactor

- Keep the existing `/game` endpoint (with fixes) as v1 and introduce a properly designed v2.
- Introduce a `Move` enum and a clean `GameEngine` service.
- Add dependency injection for `IGameEngine` and `IComputerMoveSelector`.
- Use `Random.Shared` in production and make randomness mockable in tests.
- Adopt consistent record-based DTOs and a standard response wrapper.
- Add FluentValidation, proper error handling with `ProblemDetails`, and FluentAssertions in tests.
- Write comprehensive data-driven tests covering all matchups.
- Consider architecture tests to enforce layering.
- Improve documentation with Scalar/OpenAPI and add a basic CI workflow.

This refactor would make the API much cleaner, more maintainable, and genuinely testable while opening the door for future extensions like additional moves. Happy to discuss any part in more detail!