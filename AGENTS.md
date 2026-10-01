# Waitingway

## Build and tests

- Requires .NET 10 SDK and Dalamud API 15 development files for the plugin.
- Plugin build: `dotnet build Waitingway/Waitingway.csproj`.
- Full solution build: `dotnet build Waitingway.sln`.
- Run all tests: `dotnet run --project Waitingway.Tests/Waitingway.Tests.csproj`.
- Tests are a custom console executable that links selected timer sources; do not use `dotnet test`, and no test filter exists.
- No repository lint, format, code-generation command, or CI workflow exists. Preserve `Waitingway/packages.lock.json` and ignore `bin/`, `obj/`, `.vs/`, and user files.

## Architecture

- `Waitingway/Plugin.cs` owns DTR entries, the one-second update loop, commands, notifications, windows, and integration lifecycle.
- `Timers/` contains mostly pure scheduling, aggregation, and display logic; `Waitingway.Tests/Program.cs` supplies hand-written console assertions.
- `Integrations/AutoRetainerDeployableProvider.cs` is the optional AutoRetainer IPC boundary.
- `Configuration.cs` stores persisted plugin settings; `Windows/` owns configuration and completion popup UI.

Preserve timer semantics: Ocean Fishing registration is `[HH:00:00, HH:15:00)` on even UTC hours, tooltip times use local time, an Eorzea timer day is 70 real minutes, and custom `Now` lasts one real minute. AutoRetainer entries hide when IPC or data is unavailable. Plugin lifecycle, IPC, UI, and persistence still need live validation.
