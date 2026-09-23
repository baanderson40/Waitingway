# Waitingway

Dalamud plugin that displays useful FFXIV timers in the server info bar.

## Current timer

Ocean Fishing registration opens every two hours at even server-time hours and lasts 15 minutes. The timer uses the exact half-open interval `[HH:00:00, HH:15:00)` internally; tooltip times are converted to the computer's local time zone.

Deployables tracks all AutoRetainer-known airships and submersibles with a return timestamp. It reports ready vessels first, or the next return countdown when none are ready.

## Commands

- `/waitingway` opens settings.
- `/waitingway toggle` toggles the Ocean Fishing entry.
- `/waitingway toggle deployables` toggles the deployables entry.
- `/waitingway toggle retainers` toggles the retainer ventures entry.

Deployables and retainer ventures are read through AutoRetainer's IPC when AutoRetainer is installed and available. Waitingway tracks all airships and submersibles with a recorded return time; idle vessels are ignored. Retainer tracking includes every active venture. Each entry hides when AutoRetainer is unavailable or no matching data exists.

## Build

```text
dotnet build Waitingway/Waitingway.csproj
dotnet run --project Waitingway.Tests/Waitingway.Tests.csproj
```
