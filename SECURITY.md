# Security policy

Report vulnerabilities through GitHub private vulnerability reporting. Do not
publish save paths, personal data, credentials, or exploit details in an issue.

`SAVE_REDIRECT_ROOT` is trusted launcher input and must be absolute. SaveRedirect
lexically confines ES3 file paths under it and fails closed when the root is
invalid. It does not sandbox other BepInEx plugins, prevent junction or reparse
point attacks by trusted local code, or protect against an already compromised
game process.

Launchers should use a dedicated non-administrator Windows account, create a
fresh per-instance root, wait for `[SAVEREDIRECT] ready`, and terminate the game
if readiness is not observed.
