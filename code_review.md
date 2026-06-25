# Code Review Report — Rock Paper Scissors API

## 1. Executive Summary
This document provides a senior-level code review of the legacy Rock Paper Scissors API codebase. The codebase consists of an ASP.NET Core API (`Rps.Api.Http`) and an xUnit test project (`Rps.Api.UnitTests`). 

While the application compiles and runs, it exhibits several architectural anti-patterns, design flaws, performance bottlenecks, and critical bugs. Most notably:
- The game's domain logic is coupled directly to the HTTP controllers.
- The unit tests pass silently without verifying actual outcomes because of a string casing mismatch.
- A critical logical bug exists in the `PlayMultiple` endpoint that appends an error message to the output unconditionally.
- High-overhead operations like sorting lists dynamically via random keys are performed on every request.

Below is a detailed breakdown of these findings, categorized by impact area.

---

## 2. Architectural & Design Analysis

### 2.1 Separation of Concerns & SOLID Principles
- **Violation of Single Responsibility Principle (SRP)**: The `GameController` is responsible for handling HTTP requests, routing, game engine rules, random choice selection, input validation, and formatting game outputs.
- **Violation of Open/Closed Principle (OCP)**: Adding new elements (e.g., Fire, Water, Wood) requires modifying the controller's core play methods directly. There is no abstraction to extend the game rules without altering the controller code.
- **Lacking Domain Model**: The game rules are hardcoded inside conditional logic (`if` statements) in the controller rather than being encapsulated in domain models (e.g., a `Move` enum or a `GameEngine` class).

### 2.2 Coupling & Extensibility
- **Tight Coupling to Concrete Implementations**: The controller creates its own instance of `Random` inline and builds the actions list dynamically. There is no abstraction for game rule lookup or random selection.
- **Inheritance vs. API Purpose**: `GameController` inherits from `Controller`, which is intended for ASP.NET MVC with View support (HTML rendering). For Web APIs, it should inherit from `ControllerBase` to avoid loading MVC view-rendering overhead.

### 2.3 Testability, Dependency Injection, & Mockability
- **No Dependency Injection (DI)**: The controller has no constructor and does not declare dependencies. This prevents mocking or substituting the random action generation during unit testing.
- **Untestable Randomness**: Because the computer's action is generated inline via `new Random()`, it is impossible to write deterministic unit tests for specific matchups (e.g., forcing the API to play "rock" to verify a "paper" player wins).
- **Direct Controller Testing**: The unit tests instantiate the `GameController` class directly, forcing the test environment to couple with controller instantiation, MVC routing, and internal controller state.

---

## 3. API Design & Integration

### 3.1 DTO Structure & Reusability
- **Redundant DTOs**: The project uses `PlayRequest` (containing a single string) and `MultiplePlayRequest` (containing a string array). Instead of creating a wrapper request class for multiple actions, the multi-play endpoint should accept a dedicated batch request DTO (`PlayBatchRequest`) wrapping a simple array of strings.
- **Raw Text Response**: The `Play` endpoint returns a raw `string`, and `PlayMultiple` returns a raw `string[]`. Modern APIs should return structured JSON (e.g., wrapped in a standard `AppResponse<T>` envelope containing success status, data, and detailed validation maps) to make integration simple for client applications.

### 3.2 Validation, Error Handling, & Status Codes
- **Missing Early-Exit Validation**: The validation check for valid moves is implemented as a fall-through `return "You have to play rock, paper or scissors";` at the end of the method. Validation should be handled as an early exit or via middleware, preventing execution of any game logic if the input is malformed.
- **Incorrect HTTP Status Codes**: If the player submits an invalid move, the API returns a string error message but still responds with an HTTP status code `200 OK`. The API should return `400 Bad Request` with structured validation messages.
- **Case-Sensitivity Vulnerability**: The input validation checks are case-sensitive. Submitting `"Rock"` or `"Paper"` fails the comparisons, resulting in a fall-through error message.

### 3.3 Versioning Strategy
- **No API Versioning**: The API exposes endpoints directly on the `/game` route. Modifying these routes or changing the rule set to a 5-way game will break existing client integrations. Versioning (e.g., `/v1/game` vs. `/api/v2/game`) is required to introduce the new 5-way game rules safely.

---

## 4. Code Quality & Maintainability

### 4.1 Magic Strings & Hardcoded Rules
- **Hardcoded Rules & Magic Strings**: Elements like `"rock"`, `"paper"`, and `"scissors"`, and result verdicts like `"Winner!"` and `"Loser!"` are represented as raw string literals repeated throughout the code. This is error-prone. These should be represented by strongly-typed elements (enums) and central constants.

### 4.2 Code Duplication
- **Duplicated Game Logic**: The conditional game rules and formatting strings are duplicated verbatim between the `Play` and `PlayMultiple` methods. Any changes to the rules require editing both locations.

---

## 5. Performance & Resource Optimization

### 5.1 Random Number Generation & Efficiency
- **High-Overhead Sorting**: The code selects a random action using:
  ```csharp
  actions.OrderBy(a => new Random().Next()).First()
  ```
  This creates a new instance of the `Random` class for every single comparison during the sorting algorithm. If the list is sorted, `Random` is instantiated multiple times in milliseconds.
- **Weak Randomness**: Instantiating `Random` in rapid succession can lead to identical seed values, causing the same action to be selected repeatedly.
- **Optimized Alternative**: Using `Random.Shared` (which is thread-safe and allocates zero garbage) is significantly faster and more appropriate for a casual game API.

### 5.2 Memory Allocations & Unnecessary Work
- **Per-Request List Allocation**: The list `private readonly List<string> actions = [];` is populated inside the request method:
  ```csharp
  actions.Add("rock");
  actions.Add("paper");
  actions.Add("scissors");
  ```
  This allocates and grows the list on every single request. The available actions are static and should be declared as a `static readonly` collection or an enum representation.
- **Accumulation Risk**: If the controller's lifecycle were changed from transient to singleton (or if it were cached), the `actions` field would grow indefinitely because elements are added continuously without being cleared.

---

## 6. Security Analysis

### 6.1 Input Sanitization & Validation
- **Missing Null Check**: In `Play`, if `request.Action` is null, the code evaluates `action == request.Action` which compares against null. It does not crash, but it proceeds unnecessarily.
- **No Sanitization**: The input action is matched directly against strings. Standardizing input (e.g. trimming whitespace, converting to lowercase) should be done prior to any evaluation.

---

## 7. Unit Tests Analysis

### 7.1 Silent Test Failure (Broken Assertions)
- The unit test `GameControllerShould.PlayCorrectly` evaluates test outcomes using case-sensitive assertions:
  ```csharp
  if (result.Contains("Rock")) { Assert.Contains("draw", result); }
  ```
  However, the controller returns the action names in lowercase (e.g., `"I play rock. Winner!"`). Because `"rock"` does not contain `"Rock"`, all of the `if` conditions evaluate to `false`. As a result, **no assertions are ever executed**, and the test suite passes silently even if the controller returns incorrect game outcomes.

### 7.2 Non-Deterministic Execution
- The unit test invokes the controller method which internally uses random generation. This makes the test non-deterministic; the code path taken depends on the random seed. Unit tests must be deterministic.

### 7.3 Bad Naming Conventions and Layout
- **Test Suffix**: The test class is named `GameControllerShould`, which deviates from standard suffix conventions (`GameControllerTests`).
- **Folders**: Placing the test class in a `Controllers` folder mirrors the HTTP layout but couples test organization directly to the web API's folder structure instead of the project's logical boundaries.
- **Direct Reference**: The tests instantiate the controller directly rather than testing the game rules via a mockable domain service.

---

## 8. Summary of Recommendations for the Refactor

1. **API Versioning**: Retain the 1.0 endpoint under `/game` with bug fixes, and spin up a new `/api/v2/game` route using ASP.NET Core API versioning.
2. **Enum-Based Domain**: Define a strongly-typed `Move` enum (Fire, Water, Wood, Metal, Earth) and encapsulate game rules in a dedicated service (`GameEngine`).
3. **DI and Mocking**: Inject `IGameEngine` and an `IComputerMoveSelector` into the controllers.
4. **Improved Randomness**: Implement `IComputerMoveSelector` using `Random.Shared` to select moves deterministically in testing (via mock) and performantly in production.
5. **Unified Request/Response**: Use records for DTOs and implement the `AppResponse<T>` pattern for API responses. Failures should be converted to standard `ProblemDetails` by a common `BaseController`.
6. **FluentValidation**: Implement request validation and return structured error metadata.
7. **FluentAssertions & Architecture Tests**: Use `FluentAssertions` in xUnit, rename test files to end in `Tests`, write data-driven tests for all 25 matchups, and write an architecture guardrail test checking dependency direction.
8. **Scalar Documentation**: Replace default templates with OpenAPI and Scalar UI.
9. **CI/CD Pipeline**: Add a GitHub actions build-and-test workflow.
