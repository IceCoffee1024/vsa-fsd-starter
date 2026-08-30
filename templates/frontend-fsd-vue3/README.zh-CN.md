[English](README.md) | [简体中文](README.zh-CN.md)

# 前端 FSD：Vue 3

一个可运行的 Vue 3 启动模板，通过完整订单管理、聚焦的客户用例与 Web API 2/OWIN 模板支持的认证流程演示 Feature-Sliced Design。

## 技术栈

- Vue 3、Composition API 与 `<script setup lang="ts">`
- Vite 与 TypeScript
- Vue Router
- Pinia Setup Store
- Vitest、Vue Test Utils 与 `@pinia/testing`
- Steiger 架构检查
- Lucide Vue 图标（`@lucide/vue`）

## 目录结构

```text
src/
├── main.ts                      # 转交给 app 层的 Vite 入口
├── env.d.ts                     # Vite 环境变量类型声明
├── app/
│   ├── index.ts                 # 应用启动与横切关注点装配
│   ├── App.vue                  # 应用外壳与主导航
│   ├── SessionControl.vue       # 应用级会话操作
│   ├── pinia.ts                 # Pinia 实例
│   ├── router/                  # 路由和认证守卫
│   └── styles/                  # 全局 Token、重置与共享样式
├── pages/
│   ├── sign-in/
│   │   └── ui/                  # 登录页面与页面内表单
│   ├── orders/
│   │   ├── model/               # 订单页面状态与工作流
│   │   └── ui/                  # 订单页面、表单、表格与对话框
│   └── customers/
│       ├── model/               # 客户页面状态与工作流
│       └── ui/                  # 客户页面、表单与摘要
├── shared/
│   ├── api/                     # HTTP Client、CRUD 契约与 Problem Details
│   ├── auth/                    # 会话状态与认证请求
│   ├── config/                  # 面向运行时的前端配置
│   ├── lib/                     # 与业务无关的工具
│   └── ui/                      # 可复用的 UI 基础组件
```

| 层级 | 职责 |
| --- | --- |
| `app/` | 全局初始化和应用级基础设施。 |
| `pages/` | 由单个页面拥有的路由级 UI、状态、验证和工作流。 |
| `shared/` | 可复用基础设施、后端 CRUD 契约、认证以及与业务流程无关的 UI 或工具；不承载产品工作流。 |
| `app/styles/` | 全局设计 Token、重置规则和跨组件样式。 |

当前模板有意采用最小 FSD 依赖方向 `app -> pages -> shared`。FSD 层级并非必须全部存在：只有当一个稳定交互已被多个页面实际复用时才增加 `features`，只有当一个稳定领域模型已有多个消费方时才增加 `entities`；除非可复用组合确有清晰边界，否则避免增加 `widgets`。即使单页面行为包含较多 UI 或业务流程，也应继续保留在所属 Page 切片中。

每个 Page 切片通过 `index.ts` 暴露路由组件，切片内部使用相对导入。Shared 按 Segment 分别提供公共 API，例如 `shared/api` 和 `shared/auth`；消费方不得访问 Segment 内部文件。文件名应说明所属领域，而不是使用 `types.ts`、`utils.ts` 等泛化名称。

Order 页面 Store 管理订单集合，并应用创建、更新、删除、批量创建和批量删除的成功结果。Customer 页面 Store 将创建状态与查询状态相互分离，使两个工作流可以独立进行或失败而不会覆盖彼此状态。表单保留本地交互状态。CRUD 请求函数与传输类型位于 `shared/api`，应用级 Session Store 与令牌生命周期位于 `shared/auth`。

## 开发

环境要求：

- Node.js 22.12 或更高版本
- pnpm 11

安装并验证项目：

```powershell
pnpm install
pnpm check:architecture
pnpm typecheck
pnpm test
pnpm build
```

启动前端：

```powershell
Copy-Item .env.example .env.local
pnpm dev
```

开发服务器监听 `http://localhost:5173`。

## 后端代理

浏览器请求 `/backend/api` 和 `/backend/oauth/token`。开发期间，Vite 会将 `/backend` 代理到 `BACKEND_API_URL` 并移除该前缀。认证请求头由前端会话提供，不再由代理注入，因此 Basic 与 OAuth 行为都能被直接观察和测试。

`.env.example` 的默认值与 Web API 2/OWIN 启动模板一致，但此前端仍是独立模板。生产部署必须提供自己的同源网关、BFF 或 API 认证方案；Vite 开发代理不属于生产构建产物。

路由通过 `createWebHistory` 使用 HTML5 history。生产环境中的静态托管服务或网关必须对 `/sign-in`、`/orders` 等非文件前端路由回退并返回 `index.html`。静态资源和包括 `/backend` 在内的后端路径必须绕过该 SPA 回退规则。

`VITE_BACKEND_BASE_URL` 有意作为公开配置，用于控制浏览器可见的后端前缀。不得在任何 `VITE_*` 变量中存放机密信息。

## 认证

登录页覆盖后端模板实际实现的两种认证方案：

- Basic 认证通过受保护接口验证凭据。生成的 Authorization 请求头只保留在内存中，刷新页面即丢弃。
- OAuth 2.0 使用后端的演示 password grant 和可选公开 `client_id`。访问令牌、轮换刷新令牌、过期时间、用户名和客户端绑定存入 `sessionStorage`；访问令牌临近过期时会在 API 请求前刷新，页头也提供手动刷新操作。

退出登录会清除本地会话。后端尚未提供刷新令牌撤销端点，因此本模板无法执行服务端注销。password grant 与非加密 HTTP 是配套旧式后端模板的兼容性选择，并不是新建公开浏览器客户端的推荐方案。

## 错误契约

`shared/api` 将非成功响应转换为 `ApiError`，并保留 RFC 9457 Problem Details 字段、验证错误和 `traceId`。Page UI 显示适合用户查看的信息，同时保留 Trace 标识供支持人员关联排查。

## 参考资料

本模板遵循 FSD v2.1，并以当前[官方 FSD 文档](https://fsd.how)作为外部参考。较早的官方 [Vue 应用架构文章](https://feature-sliced.design/blog/vue-application-architecture)仍可作为补充材料，但其中部分示例采用了更早、层级更重的风格。本 README 与仓库 FSD 原则文档仍是模板具体取舍的权威来源。

## 当前范围

模板现已覆盖受保护路由、Basic 与 OAuth 登录、OAuth 刷新令牌轮换、本地退出、完整订单管理、客户创建与按标识查询、客户端与服务端验证反馈、选择、确认对话框，以及聚焦的 Store、路由守卫和组件测试。客户列表与选择、客户更新与删除、生成式 OpenAPI Client、刷新令牌撤销和自动化端到端浏览器测试仍留待后续阶段。当前后端契约尚未提供这些客户用例，因此前端有意不模拟它们。
