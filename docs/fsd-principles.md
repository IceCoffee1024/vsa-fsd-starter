[English](fsd-principles.md) | [简体中文](fsd-principles.zh-CN.md)

# Feature-Sliced Design Principles

## Layers

FSD layers are optional. Start with the smallest structure that preserves clear ownership; the current Vue template uses:

```text
app -> pages -> shared
```

When confirmed reuse justifies more layers, the full dependency direction is `app -> pages -> widgets -> features -> entities -> shared`. A layer may import only from layers below it, and slices on the same layer do not cross-import. `app` owns bootstrap, routing, global state providers, and application-wide styles. `pages` own route-level UI, state, validation, data loading, and business flows. `shared` owns reusable infrastructure, CRUD contracts, authentication state, business-neutral UI, and utilities, but no product workflow.

Add `features` only for stable user interactions already reused by multiple pages and `entities` only for stable domain models with multiple consumers. The `widgets` layer is discouraged because reusable interface blocks and user flows commonly overlap; keep screen-specific compositions in `pages` unless an exceptional reusable boundary is clear.

## Slices and Segments

Business-oriented layers are divided into slices such as `orders`, `submit-order`, or `checkout`. Segments such as `ui`, `model`, `api`, and `lib` group code by purpose inside a slice. `app` and `shared` have segments rather than slices. Name files after their domain concern, such as `orders.ts` or `authentication.ts`, instead of technical buckets such as `types.ts`, `utils.ts`, or `helpers.ts`.

## Public APIs

Each slice exposes a deliberate public API through `index.ts`. External consumers import through that entry point and do not reach into another slice's internal segments; code inside a slice may use relative imports. Shared has no slices, so each Shared segment exposes its own public API, such as `shared/api` or `shared/auth`.

## Pages First and Deferred Extraction

Place single-page behavior in that Page slice first, including substantial UI, Pinia state, validation, and workflow orchestration. Duplication alone does not require extraction. Extract a Feature or Entity only when the same code is currently used by multiple consumers, those usages do not always change together, and the resulting boundary has one focused responsibility.

Plain CRUD functions and wire types belong in `shared/api`; page-specific state and business flow remain in the Page model. Authentication tokens, login requests, refresh handling, and application-wide session state belong in `shared/auth`. A transport type alone does not justify an Entity, and identity mappings between identical transport and frontend shapes should not be added speculatively.

## Composition Rules

- Pages may orchestrate their own components, state, API calls, and any extracted lower-level slices.
- Features express reusable user intentions, not every visual component, form, or API call.
- Entities own confirmed reusable domain behavior, not CRUD wrappers or transport types alone.
- `shared` may contain application-aware contracts and configuration but must not own product workflows or domain calculations.
- A frontend feature does not need a one-to-one backend slice; alignment follows user behavior and API contracts.

## Enforcement

The Vue template runs the official Steiger linter with `pnpm check:architecture`. Architecture checking is part of the documented verification sequence alongside type checking, tests, and the production build.

## Further Reading

For Vue-specific application architecture guidance, see the official [Vue application architecture article](https://feature-sliced.design/blog/vue-application-architecture). It is supplementary guidance; this document remains the authoritative description of the rules adopted by this repository.

For a community comparison of modular design and FSD in Vue 3, see [Modular Design vs Feature-Sliced Design in Vue 3](https://dev.to/igornosatov_15/slicing-through-complexity-modular-design-vs-feature-sliced-design-in-vue-3-13dh). It is an optional perspective rather than a repository rule.
