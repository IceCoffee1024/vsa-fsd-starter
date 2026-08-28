[English](README.md) | [简体中文](README.zh-CN.md)

# 前端 FSD：Vue 3

一个可运行的 Vue 3 启动模板，通过完整订单管理与 Web API 2/OWIN 模板支持的认证流程演示 Feature-Sliced Design。

## 技术栈

- Vue 3、Composition API 与 `<script setup lang="ts">`
- Vite 与 TypeScript
- Vue Router
- Pinia Setup Store
- Vitest、Vue Test Utils 与 `@pinia/testing`
- Lucide Vue 图标（`@lucide/vue`）

## 目录结构

```text
src/
├── app/                         # Pinia Provider、路由和认证守卫
├── pages/                       # 登录与订单路由级组合
├── widgets/order-list/          # CRUD 工具栏、选择状态和对话框编排
├── features/                    # 认证与聚焦的订单操作
├── entities/
│   ├── order/                   # Order 模型、API 适配、Store 和表格 UI
│   └── session/                 # Basic/OAuth API、会话状态和令牌轮换
├── shared/
│   ├── api/                     # HTTP 与 Problem Details 适配
│   ├── config/                  # 面向运行时的前端配置
│   ├── lib/                     # 与业务无关的工具
│   └── ui/                      # 可复用的对话框基础组件
└── styles/                      # 全局 Token 和重置样式
```

依赖按照 `app -> pages -> widgets -> features -> entities -> shared` 向下流动。调用方通过切片的 `index.ts` 导入，不直接访问其他切片的内部 Segment。前端拥有自己的 `Order` 模型，并显式完成传输 DTO 到模型的映射。

Order Pinia Store 管理共享集合，并应用创建、更新、删除、批量创建和批量删除的成功结果；各 Feature 仍自行管理表单和对话框状态。Session Pinia Store 是认证方式与凭据的单一事实来源。

## 开发

环境要求：

- Node.js 22.12 或更高版本
- pnpm 11

安装并验证项目：

```powershell
pnpm install
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

`shared/api` 将非成功响应转换为 `ApiError`，并保留 RFC 9457 Problem Details 字段、验证错误和 `traceId`。Feature 与 Widget 显示适合用户查看的信息，同时保留 Trace 标识供支持人员关联排查。

## 当前范围

模板现已覆盖受保护路由、Basic 与 OAuth 登录、OAuth 刷新令牌轮换、本地退出、订单列表与详情、创建、更新、删除、批量创建、批量删除、客户端与服务端验证反馈、选择、确认对话框，以及聚焦的 Store、路由守卫和组件测试。客户选择、生成式 OpenAPI Client、刷新令牌撤销和自动化端到端浏览器测试仍留待后续阶段。
