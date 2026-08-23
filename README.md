# Weather MCP Server

A minimal standalone [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server, built in C#, exposing a single tool: live current weather for any city.

Built as a learning exercise to demonstrate MCP alongside native Semantic Kernel plugins in the [AI Chat Portfolio](https://github.com/SarojaSaravanan/AIChatPortfolio) project — see that repo's README for how this server is consumed as an MCP client.

---

## What this is

Unlike a native Semantic Kernel plugin (a C# class living inside one app's process), this is a **completely separate, standalone program**. It exposes its tool through the open MCP standard rather than any one AI framework's proprietary plugin system — meaning any MCP-compatible client (Claude Desktop, a different Semantic Kernel app, a LangChain app, etc.) could use this exact same server, regardless of what language or framework that client is built in.

## Tool exposed

| Tool | Description |
|---|---|
| `get_current_weather` | Returns current temperature and wind speed for a given city, using the free [Open-Meteo](https://open-meteo.com/) API (no API key required) |

## Tech Stack

- .NET 8
- [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) — official C# MCP SDK
- Transport: **stdio** (the server is launched as a subprocess by its client and communicates over standard input/output)

## Running it standalone

```bash
dotnet run
```

The server will sit silently, waiting for an MCP client to connect over stdio — this is expected behavior, not a hang.

## How a client connects to it

A client (like the AI Chat Portfolio backend) launches this server automatically as a subprocess:

```csharp
var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "WeatherMcpServer",
    Command = "dotnet",
    Arguments = ["run", "--project", @"path\to\WeatherMcpServer"]
}));

var tools = await mcpClient.ListToolsAsync();
```

No manual startup required — the client process spawns and manages this server's lifecycle.

## Why this exists

Built specifically to demonstrate the practical difference between:
- **Native SK plugins** — simple, fast, but locked to one app's process and framework
- **MCP-exposed tools** — reusable across any MCP-compatible AI application, at the cost of slightly more setup (a separate process, protocol communication instead of a direct method call)

See the [main project README](https://github.com/SarojaSaravanan/AIChatPortfolio#mcp-integration) for a side-by-side comparison and a working demo screenshot.
