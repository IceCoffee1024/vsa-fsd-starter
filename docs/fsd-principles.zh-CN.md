[English](fsd-principles.md) | [简体中文](fsd-principles.zh-CN.md)

# Feature-Sliced Design 原则

> 本文是 [英文原文](fsd-principles.md) 的简体中文翻译。如有内容不一致，以英文原文为准。

## 层级

前端模板采用标准依赖方向：

```text
app -> pages -> widgets -> features -> entities -> shared
```

每一层只能导入其下方层级。`app` 组装全局 Provider 和路由；`pages` 组合路由级体验；`widgets` 组合规模较大的界面区域；`features` 实现用户意图；`entities` 对业务概念建模；`shared` 包含与业务无关的基础能力。

## 切片与分段

面向业务的层级会划分为 `order`、`submit-order` 或 `checkout` 等切片。`ui`、`model`、`api` 和 `lib` 等分段则在切片内部按照技术用途组织代码。

## 公共 API

每个切片都提供经过明确设计的公共 API。使用方通过该入口导入，不得直接访问其他切片的内部片段。公共 API 应保持精简，不能为了使用方便而重新导出实现细节。

## Feature-first 用户动作

在 `features` 层内，每个切片代表一个用户可见的意图或业务动作，例如 `create-customer`、`find-customer` 或 `batch-delete-orders`。Feature 切片负责交互流程和局部 UI 状态；可复用的业务模型、共享状态和实体表示仍归 `entities` 所有。这样既能保持用户动作的内聚性，也能避免重复共享实体逻辑。

## 组合规则

- Page 和 widget 可以编排多个较低层级的 feature。
- Feature 表达可复用的用户意图，而不是每个视觉组件或 API 调用。
- Entity 负责可复用的业务表示，但不编排用户工作流。
- `shared` 不能引入产品特有的概念。
- 前端 feature 不需要与后端切片一一对应；二者应按照用户行为和 API 契约对齐。
