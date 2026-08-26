[English](README.md) | [简体中文](README.zh-CN.md)

# vsa-fsd-starter

A pragmatic, opinionated boilerplate and guideline combining Vertical Slice Architecture & Modular Monolith on the backend with Feature-Sliced Design (FSD) on the frontend.

## Repository Structure

- `docs/` contains the architecture principles and boundary rules shared by every implementation.
- `templates/` is reserved for independent, minimal starters for each supported backend or frontend stack.
- `examples/` is reserved for complete applications that connect selected backend and frontend templates.
- `scripts/` is reserved for repository-wide automation once runnable projects are added.

The Web API 2/OWIN backend template is implemented and locally verified. The remaining template and example directories are scaffolds rather than runnable applications.

## Documentation

- [Architecture overview](docs/architecture-overview.md)
- [Vertical Slice Architecture guide](docs/vsa-guide.md)
- [Feature-Sliced Design principles](docs/fsd-principles.md)

## Templates

- [Web API 2 with OWIN on .NET Framework 4.8](templates/backend-vsa-webapi2-owin/README.md) - runnable reference template
- [ASP.NET Core Minimal API on .NET 10](templates/backend-vsa-aspnetcore/README.md)
- [Python FastAPI](templates/backend-vsa-fastapi/README.md)
- [Vue 3](templates/frontend-fsd-vue3/README.md)
- [React](templates/frontend-fsd-react/README.md)

## Examples

- [Legacy Ordering](examples/legacy-ordering/README.md) - a future Vue 3 and Web API 2/OWIN modernization example
