[English](README.md) | [简体中文](README.zh-CN.md)

# 后端 VSA：Web API 2 与 OWIN

一个可运行的 .NET Framework 4.8 参考模板，使用 ASP.NET Web API 2、Katana/OWIN 管道，以及通过 `HttpListener` 自托管的控制台进程。

> 本文是 [英文 README](README.md) 的简体中文翻译。如有内容不一致，以英文原文为准。

## 前置条件

- 安装了 .NET Framework 4.8 Developer Pack 的 Windows
- 能够构建 SDK 风格 `net48` 项目的 .NET SDK 10 或更高版本

本地 `NuGet.Config` 有意只使用 nuget.org，避免还原行为受到计算机级包源配置的影响。

## 架构

本文档是当前模板具体项目和目录布局、运行说明及技术栈细节的权威事实来源。

```text
src/
├── BuildingBlocks/                                  # 稳定、可复用、与业务无关的架构基础组件
│   ├── BackendVsaOwin.BuildingBlocks.WebApi/        # Web API 2 传输层基础组件
│   └── BackendVsaOwin.BuildingBlocks.Persistence/   # 共享 SQLite 连接和迁移基础设施
├── Host/                                            # 可执行宿主和组合根
│   └── BackendVsaOwin.Host/                         # 当前可执行宿主和组合根
└── Modules/                                         # 业务模块和垂直切片
    ├── BackendVsaOwin.Modules.Customers.Contracts/  # Customers 对外公开的最小契约
    ├── BackendVsaOwin.Modules.Customers/            # Customers 业务模块和垂直切片
    └── BackendVsaOwin.Modules.Orders/               # Orders 业务模块和垂直切片

tests/                                               # 单元测试和宿主集成测试
├── BackendVsaOwin.Modules.Customers.Tests/          # Customers 模块测试
├── BackendVsaOwin.Modules.Orders.Tests/             # Orders 模块测试
└── BackendVsaOwin.Host.IntegrationTests/            # HTTP、管道和 OpenAPI 集成测试
```

可执行 Host 按职责组织：

```text
BackendVsaOwin.Host/
├── Authentication/   # Basic/OAuth 集成和 SQLite Refresh Token
├── Composition/      # 应用身份和 Host-owned 模块描述符
├── OpenApi/          # NSwag 配置和文档处理器
├── Persistence/      # 数据库路径和应用迁移顺序
├── WebApi/           # Web API 配置、发现、DI 和异常处理
├── App.config
├── Program.cs
└── Startup.cs
```

Host 负责 `WebApp.Start`、OWIN 管道、Microsoft DI 容器、Web API 配置、NSwag、认证、数据库迁移编排和模块组合。Refresh Token Store 与迁移也归 Host 所有，因为令牌签发属于 Host 认证关注点，而不是业务模块。显式的 Host-owned 模块描述符在一个有序目录中统一声明各模块的程序集、服务注册委托和依赖。模块目录负责派生运行时 Controller 白名单与模块迁移顺序，并调用模块服务注册，因此未列入的程序集不会意外增加端点或模块迁移。模板不使用基于反射的模块自动发现。每个业务模块负责自身的 HTTP 动作、请求与响应模型、验证、处理器、领域对象、SQLite Store 和嵌入式迁移脚本。`BackendVsaOwin.BuildingBlocks.WebApi` 只包含共享 HTTP 传输层基础类型，包括 RFC 9457 错误契约，不包含领域结果或业务规则。`BackendVsaOwin.BuildingBlocks.Persistence` 包含可复用的 SQLite 连接工厂和 DbUp 执行器；专属 SQL 和 Store 保留在其所属的 Host 关注点或业务模块中。自定义 Web API 依赖解析器为每个请求创建并释放一个 `IServiceScope`。

Orders 只引用 `Customers.Contracts`，并通过 `ICustomerLookup` 查询客户；它无法访问 Customers 的领域或持久化类型。Customers 实现该契约，Host 在启动时完成连接。订单保存 `CustomerId` 和客户名称快照，因此当前客户名称的变化不会改写历史订单数据。

```text
Host -> Customers -> Customers.Contracts
Host -> Orders    -> Customers.Contracts
Host、Customers、Orders -> BuildingBlocks.WebApi
Host、Customers、Orders -> BuildingBlocks.Persistence
Orders -X-> Customers 内部实现
```

每个用例仍保留在自己的 feature 目录中。Web API 2 会先选择 Controller，再评估动作级 HTTP 方法约束，因此共享 `/api/orders` 的动作必须属于同一个 Controller 类型。各切片的动作文件由此组成一个 `partial OrdersController`；这是传输层的兼容性选择，并非共享的业务 Service 层。

模板将语言版本固定为 C# 14。纯输出 DTO 使用 `required init` 属性和具名对象初始化器；请求 DTO 保留 setter 和显式 Validator，领域对象则保留用于维护不变量的构造函数或工厂。PolySharp 为 `net48` 提供 `required`、`init` 等特性所需的编译器支持类型；它是私有的编译期依赖，不会增加需要部署的运行时程序集。

CLR 类型和属性遵循 .NET 的 PascalCase 规范，HTTP JSON 契约则对请求、响应和错误属性统一使用 camelCase。Web API 的 Newtonsoft Formatter 负责该命名策略，NSwag 复用同一个序列化设置，避免运行时载荷与 OpenAPI Schema 漂移。API 错误采用 RFC 9457 Problem Details，并使用 `application/problem+json` 媒体类型。标准 `type` URI 是唯一的机器可读问题标识，不再并行输出 `code` 属性。规范允许的 `traceId` 扩展用于标识一次具体请求，并与 `X-Trace-Id` 响应头及服务端结构化日志保持一致。

全局 `ModelStateValidationFilter` 在 Web API 完成参数绑定之后、Action 执行之前运行。它会把参数绑定和类型转换错误转换为统一的验证 Problem Details 响应，并且不会暴露 Formatter 异常详情。只有传输层绑定成功后才会执行 Feature Validator；后者负责金额必须为正、批量大小限制、跨模块引用等用例规则。缺少请求体由 Controller 边界处理，因此 Feature Validator 接收非空请求对象，也不依赖 Web API 的 `ModelState` 类型。

三个测试项目都在 Microsoft Testing Platform 上使用 xUnit.net v3 编程模型，使后续现代 .NET 模板可以继续采用相同的测试风格。模板级 `global.json` 为 .NET SDK 10 及更高版本的 `dotnet test` 选择 MTP 运行器。

## 开发

请在当前目录运行以下命令：

```powershell
dotnet restore BackendVsaOwin.sln
dotnet build BackendVsaOwin.sln --no-restore --configuration Release
dotnet test BackendVsaOwin.sln --no-build --configuration Release
dotnet run --project src/Host/BackendVsaOwin.Host -- http://localhost:5088/
```

使用 `Ctrl+C` 停止 Host。可以通过第一个命令行参数指定其他 URL；默认值也保存在 `src/Host/BackendVsaOwin.Host/App.config` 中。

## 持久化

Customers 与 Orders 使用同一个 SQLite 数据库。默认相对路径以 Host 可执行文件目录为基准解析，可在 `App.config` 中修改：

```xml
<add key="DatabasePath" value="data/backend-vsa-owin.db" />
```

Host 会创建父目录和应用日志管道，启用并验证持久化的 SQLite WAL 模式，然后在开始接受流量前执行嵌入式 DbUp 迁移。DbUp 会通过应用所用的同一个 JSON 日志 Provider 输出迁移发现、执行和无需更新等诊断信息。Host 认证迁移会先创建 Refresh Token 存储；随后模块目录验证每项依赖都已在前面声明，并按该顺序确定性迁移：先 Customers，后 Orders。所有迁移所有者共享数据库默认的 `SchemaVersions` 日志表，同时分别拥有 `Migrations/` 下的编号 SQL 脚本。已执行脚本按资源名称跟踪，不应修改；每次 Schema 变更都应新增一个编号脚本。

每次 Store 操作都会独立打开并释放连接，且每个连接都启用 `Foreign Keys=True` 和显式的 30 秒锁等待超时；对延迟有更严格要求的部署可以在构造连接工厂时覆盖该超时。模块 Store 只使用 Dapper 执行参数化运行时 SQL 和物化行对象。其私有持久化行模型把 SQLite GUID 保持为规范字符串并显式转换为领域类型，同时把金额 `TEXT` 直接映射成 `decimal`；不可变领域对象不依赖 Dapper。`orders.customer_id` 通过 `ON DELETE RESTRICT` 和 `ON UPDATE RESTRICT` 引用 `customers.id`；通过 `ICustomerLookup` 完成的应用验证仍是面向用户的检查，外键是最终的数据完整性保护。Microsoft.Data.Sqlite 把订单 `decimal` 值保存为 `TEXT`，避免 SQLite `REAL` 导致完整精度在往返后丢失。批量新增和批量删除均在显式控制的短事务中执行。

订单更新也会在同一事务内读取更新后的快照再提交，避免并发更新在写入和回读之间改变响应内容。

## HTTP 端点

| 方法 | 路径 | 用途 |
| --- | --- | --- |
| `POST` | `/api/customers` | 验证并创建一个客户。 |
| `GET` | `/api/customers/{id}` | 根据 ID 获取一个客户。 |
| `POST` | `/api/orders` | 验证客户引用并创建一个订单。 |
| `POST` | `/api/orders/batch` | 验证客户引用并原子地批量创建订单。 |
| `GET` | `/api/orders` | 按 ID 确定性排序列出所有订单。 |
| `GET` | `/api/orders/{id}` | 根据 ID 获取一个订单。 |
| `PUT` | `/api/orders/{id}` | 验证并替换订单的可变字段。 |
| `DELETE` | `/api/orders/{id}` | 删除一个订单。 |
| `POST` | `/api/orders/batch-delete` | 验证并批量删除订单。 |
| `POST` | `/oauth/token` | 使用资源所有者凭据签发演示用 OAuth2 Bearer Token。 |
| `GET` | `/swagger/v1/swagger.json` | 返回生成的 OpenAPI 3.0 文档。 |
| `GET` | `/swagger` | 打开 Swagger UI。 |

## 认证

模板为同一组受保护的 Web API 操作提供两种可选认证方案：Katana Basic 认证和 OAuth2 Bearer 认证。Swagger UI 及其 OpenAPI 文档注册在认证中间件之前，因此保持公开；`/oauth/token` 由 OAuth 授权服务器中间件处理，不需要 Basic 认证。Web API 的全局 `AuthorizeAttribute` 要求每个 API 端点都具备已认证身份。生成的 OpenAPI 文档将两种方案声明为独立的安全要求，表示 Basic 或 OAuth2 二选一，因此 Swagger UI 会显示两种认证方案的锁形图标，并使用应用名称配置 OAuth2 客户端设置。

`BasicAuthenticationHandler` 负责解析 HTTP 请求头、创建 Katana 认证票据，并在选择 Basic 方案时添加 Basic challenge。`OAuthAuthorizationProvider` 对 OAuth2 资源所有者密码授权复用相同的 `ICredentialValidator`，并签发不透明 Bearer Token；它也会在校验公开客户端的 `client_id`（若提供）与原始票据一致后接受 Refresh Token Grant。默认的 `ConfiguredCredentialValidator` 会对照从 `App.config` 读取的单一凭据执行固定时间比较，因此更换凭据来源时无需修改任一传输组件。`BasicAuthenticationOptions` 只保存 Basic 方案元数据和 Realm，不保存凭据。

演示凭据配置在 `src/Host/BackendVsaOwin.Host/App.config` 中：

```xml
<add key="Username" value="admin" />
<add key="Password" value="password" />
<add key="DataProtectionKey" value="" />
```

`ApplicationIdentity` 定义编译期的 `ApplicationName`、`OpenApiTitle` 和 `BasicRealm` 常量。克隆模板用于其他应用时修改这些常量即可；它们有意在所有环境中保持一致。`Username` 和 `Password` 仍然是由 Basic 与 OAuth2 演示流程共享的运行时配置。

Host 先创建结构化日志，再调用 `app.SetDataProtectionProvider`，并且该调用仍位于 OAuth 中间件之前。`AesDataProtectionProvider` 使用 `LoadOptions.PreserveWhitespace` 加载程序可执行文件的配置文件。如果 `DataProtectionKey` 不存在或为空，就生成 32 个密码学安全随机字节，将其 Base64 表示写入运行时 `.exe.config`，并立即使用这组字节；如果已有值，则进行 Base64 解码、记录其 SHA-256 指纹，并要求解码结果严格为 32 字节。Provider 按 purpose 派生独立的加密与认证子密钥，并使用 AES-CBC 加 HMAC-SHA256 保护 Katana 票据。要让令牌跨重启继续有效，生成后的配置文件必须持久且可写；需要验证同一令牌的节点必须使用相同密钥。生产环境应优先通过受保护的部署配置注入稳定密钥，不要依赖首次启动自动生成。

使用标准请求头发送凭据（例如将 `admin:password` 编码为 Base64）：

```text
Authorization: Basic YWRtaW46cGFzc3dvcmQ=
```

缺少凭据或凭据无效时，请求会保持匿名；Web API 全局授权过滤器返回 HTTP 401，`SchemeAwareAuthenticationFilter` 根据请求中的认证凭据选择 challenge。既没有 `Authorization` 请求头也没有 `access_token` 查询参数时，同时声明 Basic 和 Bearer；Basic 请求只声明 Basic，无效 Bearer 请求头或非空 Query Token 只声明 Bearer。认证失败不是应用层 Problem Details 响应。集成测试项目使用独立的 `test-user` / `test-password` 配置。

使用 Password Grant 请求演示用 Bearer Token。模板有意将客户端视为公开客户端，但仍要求配置的资源所有者凭据：

```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=public-client&username=admin&password=password
```

成功响应包含有效期一小时的 `access_token` 和有效期 30 天的 `refresh_token`。在同一端点兑换刷新令牌；如果初次请求提供了 `client_id`，刷新时必须保持一致：

```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token&client_id=public-client&refresh_token=<refresh_token>
```

每次成功兑换都会消费旧刷新令牌并返回新令牌。重复使用已消费令牌会撤销整个令牌族，包括最新的替代令牌。刷新令牌是随机、不透明的句柄；SQLite 只保存其 SHA-256 哈希和受 Katana 保护的票据。在同一数据库与保护上下文下重启 Host 后，刷新令牌仍然可用。

将返回的 `access_token` 用于访问同样接受 Basic 的 API：

```text
Authorization: Bearer <access_token>
```

对于无法设置 `Authorization` 请求头的受限客户端或协议握手，Bearer 中间件也接受 Query String Token：

```http
GET /api/orders?access_token=<access_token>
```

两种形式同时存在时，`Authorization` 请求头优先；即使请求头中的 Bearer Token 无效，也不会回退使用 Query Token。Query String Token 可能被服务器和代理日志、浏览器历史、监控遥测及 `Referer` 请求头记录。此兼容路径只能通过 HTTPS 使用，日志和遥测必须对 `access_token` 脱敏，普通 HTTP 客户端应优先使用 Bearer 请求头。

`Microsoft.Owin.Security.OAuth` 使用 Katana 票据格式和 Host 的 AES-256 数据保护提供器。令牌端点、Password Grant、Refresh Token 轮换和共享演示凭据仍然不是生产级身份系统。公开的 `client_id` 只能标识客户端，不能认证客户端。部署时应使用 HTTPS、合适的机密客户端或 PKCE 流程、真实的用户与客户端存储、保护密钥管理与轮换、管理端撤销与清理，以及符合部署威胁模型的授权模式。刷新令牌重放会阻止后续刷新，但已经签发的自包含 Access Token 仍会有效到一小时后过期。

当前自托管演示有意允许 HTTP 和 App.config 中的明文凭据。部署服务必须使用 HTTPS、安全的机密存储、凭据轮换，以及符合威胁模型的认证方案；不要继续使用示例密码。

先创建客户：

```json
{
  "displayName": "Ada Lovelace"
}
```

然后使用返回的客户 ID 创建订单：

```json
{
  "customerId": "9fc75e91-bc60-469d-98a6-d677e6760cd9",
  "totalAmount": 42.50
}
```

创建成功时返回 HTTP 201，并通过 `Location` 响应头提供新资源地址。Orders 通过 Customers 的公开契约解析当前客户名称并将其保存为 `customerName`；调用方不能直接提交该快照。不存在的 `customerId` 会在订单写入前返回 HTTP 400。`PUT` 只修改 `totalAmount`，并保留客户标识和最初的名称快照。验证失败时返回 HTTP 400、Problem Details 类型 `urn:backend-vsa-owin:problem:validation-failed`，以及按 JSON 字段路径组织的 `errors` 扩展。查询、更新或删除不存在的订单 ID 时返回 HTTP 404 和类型 `urn:backend-vsa-owin:problem:order-not-found`；客户查询失败使用 `urn:backend-vsa-owin:problem:customer-not-found`。每个问题都包含 `type`、`title`、`status`、`detail`、`instance` 和 `traceId`；未处理异常会转换为安全 HTTP 500 响应，Host 则使用同一 Trace 标识记录完整异常。合法的上游 W3C `traceparent` 会被继续传播，否则由服务器启动新的 Trace。删除成功时返回 HTTP 204。

批量新增接受 1 到 100 个订单：

```json
{
  "orders": [
    {
      "customerId": "9fc75e91-bc60-469d-98a6-d677e6760cd9",
      "totalAmount": 42.50
    },
    {
      "customerId": "d7231233-8af5-4e3c-ad61-38a78015bfec",
      "totalAmount": 99.95
    }
  ]
}
```

所有条目和客户引用都会在写入存储之前完成验证。客户 ID 通过一次批量契约调用完成解析。任一客户无效或不存在时返回 HTTP 400，并使用 `orders[1].customerId` 这样的索引字段，且不会写入任何订单。有效批次会原子提交，并返回 HTTP 201、`createdCount` 和 `items`。由于响应代表多个资源，因此不提供单一 `Location` 响应头。

批量删除接受 1 到 100 个互不重复且非空的订单 ID：

```json
{
  "ids": [
    "b9c359d4-6e94-41ca-aa3c-0af90409d2a1",
    "51c67b21-883e-47cb-8b5f-bc5a7fc23067"
  ]
}
```

有效的批量请求返回 HTTP 200，响应包含 `requestedCount`、`deletedCount` 和 `missingIds`。不存在的 ID 不会导致请求失败，也不会回滚已经删除的订单。该命令使用 POST，是因为不同 Web API 2 客户端和中间组件对 DELETE 请求体的支持不一致。

## 依赖选择

- Web API Core 5.3.0 有意解析到 Web API Client 6.0.0。
- `Microsoft.Owin.Hosting` 和 `Microsoft.Owin.Host.HttpListener` 提供控制台自托管能力；这不是 IIS 项目。
- `Microsoft.Owin.Security` 和 `Microsoft.Owin.Security.OAuth` 提供 Katana 原生的 Basic 与 OAuth2 认证 Options、中间件、每请求 Handler、认证票据、Refresh Token Provider 和 challenge 生命周期。`SchemeAwareAuthenticationFilter` 根据请求方案选择 OWIN challenge，Web API 全局 `AuthorizeAttribute` 则独立负责强制认证。
- Host 将 Web API 默认程序集解析器替换为显式的 Customers/Orders 白名单；集成测试使用独立的测试启动配置加入测试异常 Controller，而不会让测试程序集进入生产发现范围。
- NSwag 提供 API 描述和 UI；其 Newtonsoft Schema 生成器与 Web API Formatter 共用设置，使文档属性名与运行时 JSON 一致。XML 文档与显式 Web API 响应元数据用于完善操作、模型和响应契约，Controller 摘要也会作为 OpenAPI 标签描述。文档后处理器发布相对的 `/` Server URL，使文档能够跟随当前 Host 和代理环境。文档处理器负责声明 Basic 与 OAuth2 安全方案，并将由 OWIN 处理的 `/oauth/token` 操作加入公开的 `Authentication` 标签；令牌操作文档描述 OAuth 表单字段和标准 JSON 错误，同时排除在 Problem Details 媒体类型改写之外。全局操作处理器则为每个生成的 Web API 操作自动追加共享的 `500` `ProblemDetailsResponse` 契约，除非该操作已经显式声明了该响应；OWIN 令牌操作则有意保留 OAuth 错误契约。其静态文件依赖保持为传递依赖。
- 小型 `BackendVsaOwin.BuildingBlocks.WebApi` 项目负责将 RFC 9457 Problem Details 适配到 Web API 2。它避免让此 .NET Framework 模板耦合 ASP.NET Core MVC 包，同时保持跨模块错误序列化和媒体类型一致。
- `ProblemTypeUris` 集中管理 `urn:backend-vsa-owin:problem` 命名空间和共享的验证错误类型。`order-not-found`、`customer-not-found` 等模块专属类型仍由各自模块负责；命名空间继续使用现有的冒号分隔契约格式。
- 其中的全局 `ModelStateValidationFilter` 会在 Action 执行前处理传输层绑定错误；Feature Validator 继续负责用例规则，不依赖 Web API 的 `ModelState`。
- `BackendVsaOwin.BuildingBlocks.Persistence` 只负责可复用的 SQLite 连接创建和 DbUp 编排。Customers 与 Orders 继续拥有各自的 Store 实现和嵌入式 SQL 迁移。
- `Microsoft.Data.Sqlite` 提供 SQLite ADO.NET Provider，Dapper 则只在 Store 内消除 Command 和 Reader 样板代码，不负责连接、事务、迁移或领域模型。DbUp 执行只向前演进的嵌入式脚本，将记录写入一个共享的 `SchemaVersions` 日志表，并通过 Host 现有的 Microsoft Extensions Logging Provider 输出迁移事件。Host 认证迁移最先执行，随后按显式模块顺序迁移，使 Orders 能在 Customers 创建被引用表之后添加外键。
- `System.Diagnostics.DiagnosticSource` 在 .NET Framework 4.8 上建立 W3C 请求 Activity。Microsoft Extensions Logging 将 JSON 结构化异常记录写入控制台；生产部署可以替换日志 Provider，而无需修改异常边界。
- Microsoft DI 10.0.10 作为依赖注入容器，兼容性支持包保持为传递依赖。
- PolySharp 1.13.0 以私有方式为 `net48` 提供现代 C# 语法所需的内部编译器 Polyfill；生成类型不会公开，且 `required` 不会取代 HTTP 请求验证。
- 基于 Microsoft Testing Platform 的 xUnit.net v3 覆盖针对性切片测试，以及使用隔离临时 SQLite 文件的 OWIN TestServer 集成测试，不需要单独的测试适配器。
- `AddManyAsync` 负责批量新增的原子边界，SQLite 实现通过数据库事务落实这一保证。
- `ICustomerLookup` 是有意保持精简的跨模块 API；Orders 不与 Customers 共享仓储、实体，也不通过本机 HTTP 回调 Customers。
- 自定义 Basic 和 OAuth2 方案有意共享从 `App.config` 读取的一个凭据；`ICredentialValidator` 将凭据验证与 Katana 请求处理分离，但没有提前引入用户存储抽象。Swagger 和 `/oauth/token` 保持公开，后续 Web API 请求接受 Basic 或 Bearer。SQLite 支持的 Refresh Token 轮换用于演示一次性兑换和重放时的令牌族撤销；用户存储、机密客户端认证、Authorization Code + PKCE、Access Token 撤销以及角色或策略授权仍不在此最小模板范围内。
- `ApplicationIdentity` 集中管理编译期的应用名称、OpenAPI 标题和 Basic 认证 Realm，为克隆模板提供统一的应用身份来源。程序集名称和命名空间属于编译期项目身份，不作为运行时配置。

## 当前限制

单个 SQLite 文件提供持久化本地存储，但不提供复制、高可用或多进程写入协调。批量删除仍把不存在的 ID 视为成功的尽力处理结果，但其写入会在一个事务中提交。Customers 有意暂未提供更新和删除；数据库当前通过 `RESTRICT` 保护已被引用的客户，因此更完整的引用生命周期策略仍不在此最小模板范围内。持久化日志聚合、分布式 Trace 导出、备份自动化、过期 Refresh Token 清理和仓库级自动化同样不在范围内；部署环境必须备份并保护数据库文件。即使增加了 Refresh Token 轮换，共享 Basic 凭据和 OAuth Password Grant 仍只是演示用认证边界。
