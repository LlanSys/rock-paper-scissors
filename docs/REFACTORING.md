# Refactoring Journey: Rock Paper Scissors API

This document highlights the major refactoring and architectural improvements made to the Rock Paper Scissors API. The goal was to modernize the codebase, improve testability, enhance maintainability, and fix existing bugs—all while preserving backward compatibility for existing consumers.

## 1. API Versioning
**What:** Introduced `Asp.Versioning.Mvc` to properly version the API endpoints.<br>
**Why:** The original API (now v1) had significant architectural flaws and bugs, but we needed to preserve its exact request/response contract for legacy clients. Versioning allowed us to introduce a completely redesigned, clean v2 API (`/api/v2/game/...`) alongside the fixed v1 API (`/api/v1/game/...`).

## 2. Clean Architecture & CQRS (v2)
**What:** Implemented the CQRS (Command Query Responsibility Segregation) pattern using `MediatR` for all v2 endpoints. 
**Why:** The legacy v1 controllers contained fat logic—routing, business rules, game evaluation, and string formatting were tightly coupled. CQRS decoupled the HTTP delivery mechanism (Controllers) from the business logic (Handlers), making the codebase highly modular and independently testable.

## 3. Centralized Exception Handling & Standardized Responses (v2)
**What:** Created a standardized `AppResponse<T>` wrapper and a global exception filter.
**Why:** Previously, errors were handled inconsistently or leaked stack traces. The new global filter catches domain exceptions, validation errors, and unhandled exceptions, wrapping them in a consistent JSON format with appropriate HTTP status codes (e.g., 400 Bad Request, 500 Internal Server Error).

## 4. Robust Validation Pipeline (v2)
**What:** Introduced `FluentValidation` combined with MediatR pipeline behaviors.
**Why:** Validation logic was previously scattered inside controllers. Now, requests are automatically validated against strongly-typed rules (e.g., checking if moves are within the allowed elements) *before* reaching the business handlers.

## 5. Extensibility: Dynamic Game Engine (v2)
**What:** Refactored the core game logic into an `IGameEngine` interface and implemented a dynamic `Element` evaluation system.
**Why:** The legacy code used massive `if-else` chains hardcoded for Rock, Paper, Scissors. The new engine evaluates elements dynamically (e.g., Rock crushes Scissors, Fire burns Paper) based on configured rules. Adding a new game variant (like Rock-Paper-Scissors-Lizard-Spock) now requires zero changes to the core engine; you simply configure new elements and their displacement rules.

## 6. Architecture Enforcement Tests
**What:** Integrated `ArchUnitNET` to automatically enforce architectural boundaries via unit tests.
**Why:** To ensure the codebase remains clean over time, these tests prevent developers from accidentally introducing tight coupling (e.g., ensuring the Domain layer never references the HTTP layer, and Controllers only depend on MediatR).

## 7. Performance & Allocation Improvements (v1 & v2)
**What:** Replaced dynamic allocations with efficient constructs.
**Why:** In the legacy code, lists and `Random` instances were reallocated heavily. We refactored the V1 and V2 logic to use `static readonly` arrays, `Random.Shared`, and pattern matching. We avoided unnecessary string allocations where possible, drastically reducing memory pressure during high traffic.

## 8. V1 Legacy Bug Fixes & Refactoring
**What:** Addressed critical logic flaws in the original controllers while intentionally maintaining their legacy architecture.
**Why:** The V1 API needed to remain fully backward-compatible and self-contained within controllers without introducing CQRS. We specifically:
- Fixed a bug in `PlayMultiple` where the computer generated a single random move and used it against the player's entire array. It now generates an independent random move per item.
- Fixed the "Double Appending" bug where a fallback `"You have to play rock, paper or scissors"` message was incorrectly appended to *every* valid game result.
- Added proper case-insensitivity (`ToLowerInvariant()`) to the V1 inputs.
- Preserved the exact JSON response contract so legacy clients are unaffected.

## 9. Comprehensive Testing
**What:** Added rigorous Unit Tests and Integration Tests.
**Why:** The application previously lacked sufficient coverage. We added unit tests with `Moq` for isolated component testing and `WebApplicationFactory` for full end-to-end integration tests that verify HTTP status codes, routing, and JSON serialization.
