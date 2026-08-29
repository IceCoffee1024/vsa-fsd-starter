[English](fsd-principles.md) | [简体中文](fsd-principles.zh-CN.md)

# Feature-Sliced Design 原则

> 本文是 [英文原文](fsd-principles.md) 的简体中文翻译。如有内容不一致，以英文原文为准。

## 层级

FSD 层级并非必须全部存在。应从能够保持职责清晰的最小结构开始；当前 Vue 模板采用：

```text
app -> pages -> shared
```

当已确认的复用需求需要更多层级时，完整依赖方向为 `app -> pages -> widgets -> features -> entities -> shared`。每一层只能导入其下方层级，同层切片之间不得交叉导入。`app` 负责启动、路由、全局状态 Provider 和应用级样式；`pages` 负责路由级 UI、状态、验证、数据加载与业务流程；`shared` 负责可复用基础设施、CRUD 契约、认证状态、与业务无关的 UI 和工具，但不承载产品工作流。

只有稳定用户交互已经被多个页面实际复用时才增加 `features`，只有稳定领域模型已有多个消费方时才增加 `entities`。不鼓励使用 `widgets`，因为可复用界面区块与用户流程的职责经常重叠；页面专属组合应保留在 `pages`，除非确有边界清晰的特殊复用场景。

## 切片与分段

面向业务的层级会划分为 `orders`、`submit-order` 或 `checkout` 等切片。`ui`、`model`、`api` 和 `lib` 等 Segment 在切片内部按用途组织代码。`app` 与 `shared` 直接使用 Segment，而不包含切片。文件应按领域关注点命名，例如 `orders.ts` 或 `authentication.ts`，不要使用 `types.ts`、`utils.ts`、`helpers.ts` 等技术桶名称。

## 公共 API

每个切片都通过 `index.ts` 提供经过明确设计的公共 API。外部消费方通过该入口导入，不得直接访问其他切片的内部 Segment；切片内部可以使用相对导入。Shared 不包含切片，因此每个 Shared Segment 分别提供公共 API，例如 `shared/api` 或 `shared/auth`。

## Pages 优先与延迟提取

单页面行为应首先保留在所属 Page 切片中，包括规模较大的 UI、Pinia 状态、验证和工作流编排。出现重复并不自动意味着必须提取。只有同一代码当前已被多个消费方使用、这些用法不会始终同步变化，并且提取后的边界只有一个聚焦职责时，才提取 Feature 或 Entity。

普通 CRUD 函数与传输类型属于 `shared/api`，页面专属状态和业务流程保留在 Page model。认证令牌、登录请求、刷新处理和应用级会话状态属于 `shared/auth`。只有传输类型并不足以创建 Entity，也不应为完全相同的传输形状与前端形状预先增加恒等映射。

## 组合规则

- Page 可以编排自身组件、状态、API 调用以及已提取的低层切片。
- Feature 表达可复用的用户意图，而不是每个视觉组件、表单或 API 调用。
- Entity 负责已确认复用的领域行为，而不是普通 CRUD 包装或单纯传输类型。
- `shared` 可以包含应用相关的契约与配置，但不能承载产品工作流或领域计算。
- 前端 feature 不需要与后端切片一一对应；二者应按照用户行为和 API 契约对齐。

## 自动约束

Vue 模板通过 `pnpm check:architecture` 运行官方 Steiger 检查器。架构检查与类型检查、测试和生产构建共同构成文档规定的验证流程。

## 延伸阅读

Vue 应用的具体架构实践可参考官方的 [Vue 应用架构文章](https://feature-sliced.design/blog/vue-application-architecture)。该文章仅作为补充参考；本文件仍是本仓库所采用规则的权威说明。

如需了解 Vue 3 中模块化设计与 FSD 的社区对比，可阅读 [Modular Design vs Feature-Sliced Design in Vue 3](https://dev.to/igornosatov_15/slicing-through-complexity-modular-design-vs-feature-sliced-design-in-vue-3-13dh)。该文章仅提供可选视角，不构成本仓库的架构规则。
