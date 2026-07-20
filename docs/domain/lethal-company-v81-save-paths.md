# Lethal Company v81 save-path evidence

## Scope

This static evidence applies only to the Steam Windows build identified by the
authorized evidence roots ending in
`v81_1966720_22825947_6423525044216269478`. The managed-code export and asset
export were inspected on 2026-07-20. No proprietary game source is committed.

## Trace ledger

| Evidence | SHA-256 | Established fact |
| --- | --- | --- |
| `GameNetworkManager.cs` | `7AF7DEAFAB2EA2511F3EAA9AEFFFEC212DF3D4E4D01B219C9FAC1DD7D3AC787D` | Vanilla selects `LCGeneralSaveData` and `LCSaveFile1`-`3` and passes them to ES3 load/save operations. |
| `DeleteFileButton.cs` | `5F9DF31AC28E9CDC53496D2C14C9F1361BB8742D36ED93666792449CB03279FD` | Vanilla deletion calls `ES3.FileExists` and `ES3.DeleteFile` with a selected save filename. |
| `SaveFileUISlot.cs` | `384CCBEF5990806CE6FF5C74CDDED9DFBB83322ED637F3553E03A07DBFFBECD8` | Save-slot UI reads the same filenames through ES3. |
| `ES3Settings.cs` | `44EEC6688186CCDA7EA52D05C256479E33BC312E66E4C800B6A09AA76D7FA4B8` | `FullPath` combines `ES3IO.persistentDataPath` and the requested relative path for file storage in `PersistentDataPath`. |
| `ES3Internal/ES3IO.cs` | `13F11B4C28EFABB2C9F9298471AE0740E7B6C9D6BA3748856E54543273646609` | `persistentDataPath` is initialized from `Application.persistentDataPath`. |
| `ProjectSettings.asset` | `B44D603925916F38C37BA43FE03837219076435C7DE0C6CD8DFD1D38C0337952` | Company and product names select the Lethal Company persistent-data identity. |
| `ES3Defaults.asset` | `70219F13C889B8CCD2893CECE19E1F1828F44FC36A09403D5755895E61E0EA82` | Default ES3 settings select file location and `PersistentDataPath`. |

The getter is present and statically reachable through the listed ES3 calls.
Execution and Harmony ordering are not yet runtime-observed for SaveRedirect.

## LCBetterSaves 1.7.3

The inspected `Pooble-LCBetterSaves-1.7.3` package ZIP has SHA-256
`502C75B79C3A89CCCE484893DF020ADCDB8EADE9D3A10EA39F74110EFE77B5A6`;
its plugin DLL has SHA-256
`659694A858A91C96BF007224A4812A6CC8EFA299A44AB23D607037963343D911`.
Static Mono.Cecil inspection established these path families:

- `LCSaveFileN` enumeration and alias read/write;
- `LGU_N.json` auxiliary files;
- `TempFileN` and `LGUTempFileN` rename intermediates; and
- `ES3.GetFiles`, `FileExists`, `RenameFile`, `Load`, `Save`, and `DeleteFile`.

In this v81 ES3 build, those operations resolve file paths through
`ES3Settings.FullPath`. Patching that getter therefore preserves LCBetterSaves'
file families while changing only their root. A live extra-slot create, rename,
load, and delete scenario remains the runtime acceptance test.
