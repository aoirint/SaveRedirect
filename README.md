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

## Build and test

```powershell
dotnet restore SaveRedirect.slnx --locked-mode
dotnet format SaveRedirect.slnx --no-restore --verify-no-changes
dotnet build SaveRedirect.slnx --configuration Release --no-restore --warnaserror
$version = (dotnet msbuild SaveRedirect/SaveRedirect.csproj `
  -getProperty:Version -nologo).Trim()
dotnet msbuild SaveRedirect/SaveRedirect.csproj `
  -target:PackageSaveRedirect -property:Configuration=Release -nologo
dotnet run --project SaveRedirect.Tests --configuration Release --no-build -- `
  SaveRedirect/bin/Release/netstandard2.1/com.aoirint.SaveRedirect.dll `
  "artifacts/SaveRedirect-$version.zip" $version
```

`PackageSaveRedirect` belongs to the plugin project and produces the
host-neutral development ZIP. `SaveRedirect.Tests` validates the completed
archive and actual BepInEx assembly attributes. No Thunderstore or stable
GitHub Release publication is configured while live compatibility evidence
remains incomplete.

## Installation

Copy `com.aoirint.SaveRedirect.dll` to a dedicated directory below
`BepInEx/plugins`. Configure the launch environment and wait for the ready
marker before treating the game process as isolated.

## License

[MIT](LICENSE)
