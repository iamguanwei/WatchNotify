# WatchNotify

Windows 系统通知监测工具，用于实时监测 Windows 系统通知并将通知内容转发到指定的 HTTP/HTTPS 地址。

## 功能特性

- 实时监测 Windows 系统 Toast 通知
- 支持按发送者名称过滤通知
- 将通知内容转发到指定的 HTTP/HTTPS URL
- 支持 URL 占位符替换消息内容
- 支持开机自启动
- 支持日志记录
- 提供命令行和图形界面两种运行方式

## 项目结构

```
WatchNotify/
├── NotificationMonitor/           # 命令行监测工具
│   ├── Program.cs                 # 程序入口
│   ├── NotificationListener.cs    # 通知监听服务
│   ├── NotificationProcessor.cs   # 通知处理器
│   └── HttpForwarder.cs           # HTTP转发器
├── NotificationMonitor.Core/      # 核心类库
│   ├── Entity/                    # 实体类
│   ├── EventArgs/                 # 事件参数
│   ├── Interface/                 # 接口定义
│   └── Common/                    # 通用组件
└── WatchNotifyUi/                 # 图形界面应用
    ├── MainForm.cs                # 主窗体
    └── Helper/                    # 辅助工具
```

## 系统要求

- Windows 10 22000 及以上版本
- .NET 9.0 Runtime

## 使用方法

### 图形界面方式

1. 运行 `WatchNotifyUi.exe`
2. 在配置页面设置消息发送地址和监视清单
3. 点击"启动"按钮开始监测
4. 程序会最小化到系统托盘继续运行

### 命令行方式

```bash
NotificationMonitor.exe [选项]
```

#### 命令行参数

| 参数 | 简写 | 说明 |
|------|------|------|
| --sender | -s | 指定允许的发送者，可多次使用 |
| --url | -u | 指定转发URL，支持{0}占位符替换消息内容 |
| --log | -l | 启用日志记录到文件 |
| --test | | 测试转发地址，不指定则使用--url参数的地址 |
| --help | -h | 显示帮助信息 |

#### 使用示例

```bash
# 监测系统通知并转发
NotificationMonitor.exe --sender "系统" --url https://example.com/api

# 监测多个发送者，启用日志
NotificationMonitor.exe -s 系统 -s 邮件应用 -u https://example.com/msg?text={0} -l

# 测试转发地址是否正常
NotificationMonitor.exe --test https://example.com/api
```

## 配置说明

### 消息发送地址

支持两种格式：

1. **普通URL**：消息内容作为 POST 请求体发送
   ```
   https://example.com/api/notify
   ```

2. **带占位符URL**：`{0}` 会被替换为实际消息内容
   ```
   https://example.com/msg?text={0}
   ```

### 监视清单

指定需要监测的通知发送者名称，每行一个。支持模糊匹配，例如：

```
系统
邮件应用
微信
```

## 开机启动

图形界面程序支持开机自启动功能：

- 通过托盘图标右键菜单 -> "开机启动" 进行设置
- 或在主界面中进行配置

## 权限说明

程序首次运行时需要获取系统通知访问权限：

1. 程序会自动请求权限
2. 如果权限被拒绝，请在 Windows 设置中手动开启：
   - 设置 -> 隐私和安全性 -> 通知 -> 允许应用访问通知

## 日志文件

启用日志功能后，日志文件保存在程序目录下：

```
notification_monitor.log
```

## 许可证

详见 [LICENSE.txt](LICENSE.txt)
