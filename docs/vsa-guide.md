[English](vsa-guide.md) | [简体中文](vsa-guide.zh-CN.md)

# Vertical Slice Architecture Guide

## Unit of Organization

A backend slice represents one observable use case, such as `CreateOrder` or `GetOrderDetails`. Its endpoint, input validation, orchestration, data access, response mapping, and focused tests should be colocated when the stack permits it.

## Module Boundary

Related slices belong to a business module. A module owns its domain rules and persistent state. Other modules interact through an explicit public contract rather than reaching into a slice's internal types or storage.

## Dependency Rules

- Transport code may invoke the use-case handler in the same slice.
- A slice may use its module's domain model and infrastructure ports.
- Cross-module calls must use the target module's public API or an integration message.
- Shared technical utilities must be business-neutral and earn their place through repeated use.
- Generic controller, service, and repository layers must not become mandatory indirection for every use case.

## Minimal Template Standard

Every backend template should eventually contain one small end-to-end slice that demonstrates validation, success and failure responses, persistence-boundary placement, and tests. The example must remain small enough to delete when adopting the template.

## Modular Monolith Guidance

A single deployment does not imply shared internals. Modules should have explicit ownership, directional dependencies, and independently understandable use cases. Extracting a service later should be a deployment decision, not a prerequisite for clean boundaries today.
