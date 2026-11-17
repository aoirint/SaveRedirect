# SaveRedirect

SaveRedirect is a small BepInEx 5 Mono plugin for Lethal Company that redirects
Easy Save 3 file operations into an absolute, per-process directory selected by
the launcher. It is intended for disposable mod-development and automated-test
profiles where normal player saves must remain untouched.

The current static compatibility baseline is Lethal Company v81 on Windows with
BepInEx 5.4.21. Live in-game validation is still required before a stable
release. See [developer documentation](docs/README.md) for the evidence and
design boundary.

## Runtime contract

Set `SAVE_REDIRECT_ROOT` to an absolute directory before launching
`Lethal Company.exe`. On successful startup the plugin writes this marker to
`BepInEx/LogOutput.log`:

```text
[SAVEREDIRECT] ready
```

If the root is absent or invalid, the plugin logs `[SAVEREDIRECT] failed` and
requests process exit code 86. A launcher must wait for the ready marker and
terminate the process on timeout.

The patch applies only to Easy Save 3 file storage under
`PersistentDataPath`. PlayerPrefs, cache, resources, and other ES3 locations
keep their original behavior. The root policy is lexical confinement, not a
sandbox against another plugin executing in the same process.

## NuGet minimum publish age

Use .NET SDK 11.0.100 or a later stable SDK selected by `global.json`.
The configured NuGet sources use `minPublishAgeHours="168"` to delay
automatic version selection by `dotnet package add` and
`dotnet package update` for seven days. This NuGet 8 feature is the reason
for adopting the .NET 11 STS SDK; it does not change the mod's runtime target.

Explicit version requests bypass this filter. Ordinary restore, including
transitive dependencies and locked restore, does not validate package age.
Floating references with an applicable cooldown fail with NU1020; keep the
existing non-floating package versions and review lockfile changes.
Both nuget.org and the BepInEx feed expose registration `published` metadata.
If an evaluated version lacks its publish date, automatic selection fails
rather than assuming it is old enough. Source mappings and exceptions retain
their existing scope. Cooldown expiry does not establish package trust;
continue reviewing provenance, identity, hashes, and vulnerability signals.

See [NuGet minimum package publish age][nuget-minimum-publish-age] for the
supported operations and restore limitations.

## Build and test

Install the .NET 11 SDK selected by `global.json`.

```powershell
dotnet restore SaveRedirect.slnx --locked-mode
dotnet format SaveRedirect.slnx --no-restore --verify-no-changes
dotnet build SaveRedirect.slnx --configuration Release --no-restore --warnaserror
$version = (dotnet msbuild SaveRedirect/SaveRedirect.csproj `
  -getProperty:Version -nologo).Trim()
dotnet run --project SaveRedirect.Tests --configuration Release --no-build -- `
  SaveRedirect/bin/Release/netstandard2.1/com.aoirint.SaveRedirect.dll $version
```

As in the related BepInEx repositories, CI owns staging and ZIP creation rather
than the runtime plugin project. `SaveRedirect.Tests` validates the completed
CI archive and actual BepInEx assembly attributes. No Thunderstore or stable
GitHub Release publication is configured while live compatibility evidence
remains incomplete.

## Installation

Copy `com.aoirint.SaveRedirect.dll` to a dedicated directory below
`BepInEx/plugins`. Configure the launch environment and wait for the ready
marker before treating the game process as isolated.

## License

[MIT](LICENSE)

[nuget-minimum-publish-age]: https://learn.microsoft.com/en-us/nuget/consume-packages/minimum-package-publish-age
