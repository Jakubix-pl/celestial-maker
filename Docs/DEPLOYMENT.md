# Local operation — free PC expansion

Unity project: `Game`, pinned editor 6000.6.3f1. No public deployment or production signing is authorized.

```powershell
./Tools/validate.ps1
./Tools/build-expansion.ps1
node Website/build.mjs
./Tools/start-website.ps1
node --test Backend/test/free-game.test.ts
```

The build script uses sequential `-batchmode -nographics` processes and refuses a locked project. Use an isolated copy or defer; never close an interactive editor. It runs no graphical player or Play Mode. The simulator uses the editor's bundled .NET SDK. Logs are under `Logs/Expansion`.

`Website/dist-free` is the active local site. `Website/dist`, `Builds/Web` and previous Windows packages are historical. The free PC ZIP is included when built. An updated Web package is not claimed: the active site displays its unavailable status instead of loading a historical monetized player. Web source, IndexedDB persistence and `Tools/build-web.ps1` remain available for a future authorized build. Android source is retained; SDK/NDK acceptance remains deferred.

The optional Node service listens on loopback 4311. It needs no keys to start and normal gameplay never contacts it. `Backend/.env.example` contains only origins, database path and optional OIDC settings. Existing authentication infrastructure is preserved. All checkout, activation, webhook, receipt and purchase verification routes have been removed. Do not configure Stripe or Google Play billing.

Back up an existing SQLite file before migration; `/profile` reads historical transaction rows without rewriting them. New service databases contain account/session tables only. Historical SQL migrations are retained as reference for old databases, not required at new startup.

A future hosting operation requires separate authorization. Preserve old immutable site-release paths and IndexedDB data when planning a deployment. No remote service was changed in this task.
