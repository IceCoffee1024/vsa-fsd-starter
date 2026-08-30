[English](README.md) | [简体中文](README.zh-CN.md)

# vsa-fsd-starter

一个务实且带有明确取舍的启动模板与指南：后端结合垂直切片架构（Vertical Slice Architecture，VSA）和模块化单体，前端采用 Feature-Sliced Design（FSD）。

> 本文是 [英文 README](README.md) 的简体中文翻译。如有内容不一致，以英文原文为准。

## 仓库结构

- `docs/` 包含所有实现共享的架构原则和边界规则。
- `templates/` 用于存放各后端或前端技术栈相互独立的最小启动模板。

Web API 2/OWIN 后端模板与 Vue 3 前端模板已经实现并通过本地验证。其余模板会保持相互独立，并在对应技术栈开始建设前保留为脚手架。

## 文档

- [架构概览](docs/architecture-overview.zh-CN.md)
- [垂直切片架构指南](docs/vsa-guide.zh-CN.md)
- [Feature-Sliced Design 原则](docs/fsd-principles.zh-CN.md)

## 模板

- [基于 .NET Framework 4.8 的 Web API 2 与 OWIN](templates/backend-vsa-webapi2-owin/README.zh-CN.md) - 可运行的参考模板
- [基于 .NET 10 的 ASP.NET Core Minimal API](templates/backend-vsa-aspnetcore/README.md)
- [Python FastAPI](templates/backend-vsa-fastapi/README.md)
- [Vue 3](templates/frontend-fsd-vue3/README.zh-CN.md) - 可运行的参考模板
- [React](templates/frontend-fsd-react/README.md)
