# Save redirection boundary

This design consumes the [v81 save-path evidence](../domain/lethal-company-v81-save-paths.md).

## Boundary

The BepInEx entry point reads one trusted launcher input, normalizes it, creates
the root, and installs one Harmony patch. The prefix intercepts only
`ES3Settings.FullPath` reads where the location is `File` and the directory is
`PersistentDataPath`. All other getter calls continue to the base method.

`SavePathPolicy` is framework-free. It accepts only an absolute root and a
non-empty relative requested path whose canonical lexical path remains below
that root. Invalid requested paths resolve to a confined `blocked-path.es3`
fallback instead of reaching the normal save directory.

## Startup and failure policy

The launcher supplies `SAVE_REDIRECT_ROOT`. Successful initialization logs
`[SAVEREDIRECT] ready` with the plugin version and a one-way root hash. Failure
logs a bounded exception type and requests exit code 86. Logging failures are
contained separately so they cannot bypass process termination or escape a
Harmony callback.

The launcher remains responsible for readiness timeout, process termination,
and any outer normal-save directory transaction. SaveRedirect does not claim to
contain malicious code already executing inside the game process.
