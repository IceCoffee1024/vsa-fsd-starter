[English](fsd-principles.md) | [简体中文](fsd-principles.zh-CN.md)

# Feature-Sliced Design Principles

## Layers

Frontend templates use the standard dependency direction:

```text
app -> pages -> widgets -> features -> entities -> shared
```

A layer may import only from layers below it. `app` wires global providers and routing; `pages` compose route-level experiences; `widgets` compose substantial interface regions; `features` implement user intentions; `entities` model business concepts; and `shared` contains business-neutral foundations.

## Slices and Segments

Business-oriented layers are divided into slices such as `order`, `submit-order`, or `checkout`. Segments such as `ui`, `model`, `api`, and `lib` group code by technical purpose inside a slice.

## Public APIs

Each slice exposes a deliberate public API. Consumers import through that entry point and do not reach into another slice's internal segments. Public APIs should remain small and should not re-export implementation details for convenience.

## Composition Rules

- Pages and widgets may orchestrate multiple lower-level features.
- Features express reusable user intentions, not every visual component or API call.
- Entities own reusable business representations but do not orchestrate user workflows.
- `shared` must not acquire product-specific concepts.
- A frontend feature does not need a one-to-one backend slice; alignment follows user behavior and API contracts.
