[English](README.md) | [简体中文](README.zh-CN.md)

# vsa-fsd-starter

一个务实且带有明确取舍的启动模板与指南：后端结合垂直切片架构（Vertical Slice Architecture，VSA）和模块化单体，前端采用 Feature-Sliced Design（FSD）。

> 本文是 [英文 README](README.md) 的简体中文翻译。如有内容不一致，以英文原文为准。

## 仓库结构

- `docs/` 包含所有实现共享的架构原则和边界规则。
- `templates/` 用于存放各后端或前端技术栈相互独立的最小启动模板。
- `examples/` 用于存放连接特定后端与前端模板的完整应用。
- `scripts/` 用于在可运行项目加入后承载仓库级自动化脚本。

Web API 2/OWIN 后端模板已经实现并通过本地验证。其余模板和示例目录目前只是脚手架，尚不是可运行应用。

## 文档

- [架构概览](docs/architecture-overview.zh-CN.md)
- [垂直切片架构指南](docs/vsa-guide.zh-CN.md)
- [Feature-Sliced Design 原则](docs/fsd-principles.zh-CN.md)

## 模板

- [基于 .NET Framework 4.8 的 Web API 2 与 OWIN](templates/backend-vsa-webapi2-owin/README.zh-CN.md) - 可运行的参考模板
- [基于 .NET 10 的 ASP.NET Core Minimal API](templates/backend-vsa-aspnetcore/README.md)
- [Python FastAPI](templates/backend-vsa-fastapi/README.md)
- [Vue 3](templates/frontend-fsd-vue3/README.md)
- [React](templates/frontend-fsd-react/README.md)

## 示例

- [Legacy Ordering](examples/legacy-ordering/README.md) - 规划中的 Vue 3 与 Web API 2/OWIN 现代化改造示例
