---
state: Current
last_updated: "2026-08-28"
translation_of: architecture-overview.md
---

[English](architecture-overview.md) | [简体中文](architecture-overview.zh-CN.md)

# 架构概览

> 本文是 [英文原文](architecture-overview.md) 的简体中文翻译。如有内容不一致，以英文原文为准。

## 背景与驱动因素

本仓库用于讲解和演示垂直切片架构（Vertical Slice Architecture，VSA）与 Feature-Sliced Design（FSD）如何在全栈系统中协同使用。仓库将可复用指南与面向特定技术栈的最小启动模板分开，使每个模板都保持聚焦并可独立采用。

## 仓库边界

- `docs/` 是所有技术栈共享规则的权威来源。
- `templates/` 下每个已完成的目录都是独立启动模板，并且必须记录自己的运行环境和命令。
- 模板可以引用文档，但不能依赖其他模板中的运行时代码。
- 特定技术栈的命令、配置和故障排查说明应保留在所属项目内。

## 组件与职责

| 组件 | 职责 |
| --- | --- |
| `docs/` | 管理跨技术栈的架构原则、依赖规则和验证策略。 |
| `templates/backend-*` | 在单个后端技术栈中演示 VSA 和模块化单体边界。目前已实现 Web API 2/OWIN 模板。 |
| `templates/frontend-*` | 在单个前端技术栈中演示 FSD 依赖方向和公共 API。目前已实现 Vue 3 模板。 |
| `scripts/` | 预留给未来的仓库级自动化。 |

已实现的 Web API 2/OWIN 模板的具体项目布局和运行说明仅由[模板 README](../templates/backend-vsa-webapi2-owin/README.zh-CN.md)维护。

## 依赖规则

- 后端行为在业务模块内按用例组织。详细后端规则请参见 [VSA 指南](vsa-guide.zh-CN.md)。
- 后端模块只能通过有意保持精简的公开契约依赖另一个模块；组合根负责连接契约与实现，消费方不得引用其他模块的领域或持久化内部类型。
- 前端导入遵循 FSD 层级方向，并通过公共 API 跨越切片边界。详细前端规则请参见 [FSD 原则](fsd-principles.zh-CN.md)。
- 后端持久化模型和领域内部类型绝不能成为前端契约。
- 使用生成式 API 客户端时，应根据已发布的 Schema 生成，不得手工编辑。
- 认证、验证、错误、分页和版本控制需要明确的协议层约定。

## 前后端协同

VSA 与 FSD 通过用户可见能力和 HTTP 契约对齐，而不是依靠相同的文件夹名称或强制建立一一对应的切片。

```text
FSD page 或 widget
  -> 一个或多个 FSD feature
    -> HTTP API 契约
      -> 一个 VSA 端点和用例切片
```

- 简单命令可以将一个前端 feature 映射到一个后端切片。
- 一个页面可以组合多个前端 feature，并调用多个后端切片。
- 当契约和授权规则相同时，一个后端切片可以服务多个前端入口。
- 读取模型应围绕用例塑形，而不是暴露持久化实体。

## 接口与数据

前后端项目通过明确的 HTTP API 契约通信。后端负责其已发布 Schema 和兼容性策略；前端在不导入后端实现代码的前提下消费该契约，并负责将传输 DTO 转换为自身的实体或 feature 模型。生成的类型可以减少机械式重复，但不能取代任何一方的领域模型。

## 部署与运维

Web API 2/OWIN 模板当前以 .NET Framework 4.8 控制台进程运行，并由 Katana 通过 `HttpListener` 自托管。Host 项目负责进程启动、OWIN 管道、依赖注入、Web API 配置、OpenAPI 中间件、模块组合、数据库迁移顺序、结构化日志和全局异常边界。显式的 Host-owned 描述符目录让模块依赖、服务注册、Controller 发现和迁移顺序保持一致，而不依赖反射自动发现。Customers 与 Orders 分别拥有自身的 HTTP 切片、领域状态、SQLite Store 和嵌入式迁移，同时共享一个数据库文件。职责严格受限的 `BackendVsaOwin.BuildingBlocks.WebApi` 项目提供共享 Web API 2 传输层基础类型，包括 RFC 9457 Problem Details 和 W3C 请求追踪，但不承载领域规则。同级的 `BackendVsaOwin.BuildingBlocks.Persistence` 项目只提供可复用的 SQLite 连接和 DbUp 迁移基础设施；模块专属 SQL 和 Store 仍归各自模块所有。公开错误响应只暴露 Trace 标识而不包含异常详情，Host 日志则使用同一标识关联完整异常。Orders 只引用公开的 `Customers.Contracts` 程序集，并通过 `ICustomerLookup` 验证客户标识和捕获客户名称快照；即使数据库也强制执行 Orders 到 Customers 的外键，它仍无法访问 Customers 的内部实现。由于 Web API 2 无法根据 HTTP 方法把同一个 URI 路由到多个使用属性路由的 Controller 类型，因此每个模块内的动作文件组成一个 `partial` Controller，同时保留独立的处理器和契约。准确命令和配置请参见 [模板 README](../templates/backend-vsa-webapi2-owin/README.zh-CN.md)。

Vue 3 模板提供可独立运行的 FSD 前端，并实现完整 Orders CRUD、客户创建与按标识查询、Basic 与 OAuth 认证界面、受保护路由和刷新令牌轮换。Customer 能力遵循当前已发布的后端契约，不虚构尚未支持的列表、更新或删除操作。其他模板仍是脚手架。未来每个实现都将负责自己的运行说明和验证流程。

## 质量属性

- **局部性：** 一项业务变更应主要影响一个后端切片和范围最小的相关前端切片。
- **可替换性：** 面向不同技术栈的模板保持相互独立。
- **可追踪性：** 模板应将其实现选择链接回共享规则。
- **可验证性：** 在技术栈支持的情况下，使用自动化检查覆盖架构边界。

## 验证要求

- 后端模板覆盖用例行为和基础设施边界。
- 前端模板覆盖 feature 模型、交互和公共 API。
- 在可行时，使用静态分析或架构测试强制执行模块与 FSD 依赖规则。
- 只有在文档所列构建、测试和启动检查均已于本地通过后，才能将模板描述为“可在本地运行”。发布就绪还要求这些检查在仓库自动化流程中通过。

具体命令、测试夹具和框架选择应保留在所属项目中。存在可运行实现后，仓库级自动化可以聚合这些命令。

## 决策与权衡

本仓库选择在相互独立的模板之间保留显式重复，而不是共享运行时抽象。这会增加一些维护工作，但可以保持各启动模板的可移植性。只有当出现第一个无法在这份持续维护的概览中充分说明理由的长期决策时，才应引入 `adr/` 目录。
