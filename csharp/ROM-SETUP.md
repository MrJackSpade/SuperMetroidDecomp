# Install game data from your ROM

Desktop and Android use the same `SuperMetroid.AssetExtraction` library. Application
packages contain no ROM or extracted audio. Graphics and room data are read directly
from the installed ROM; setup extracts the audio upload streams, catalog, and 112 PCM
WAVs used by the managed audio renderer. No upstream disassembly checkout is needed.

Supported image: Super Metroid Japan/USA NTSC v1.0, 3 MiB, SHA-256
`12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72`.
A 512-byte copier header is accepted and removed from the installed copy. Other revisions,
patched images, truncated files, and oversized files are rejected before installation.
The selected source file is copied, never moved or deleted.

## Desktop

Run `SuperMetroid.Game` and choose a `.smc` or `.sfc` file when prompted. You can also
pass one ROM path or set `SUPERMETROID_ROM`. Setup installs into
`%LOCALAPPDATA%/SuperMetroid/`; later launches reuse that installation, so the original
file no longer needs to be available. Setup errors leave the picker available to retry.

An existing `SuperMetroid.ini` beside the executable takes precedence over the AppData
copy (the executable directory, not the shell's working directory). This is a complete
configuration override, not a merge. Removing it restores AppData settings; it never
overwrites them. Startup prints the selected source and full path on separate lines.

The Windows ZIP includes `SuperMetroid.defaults.ini` beside the executable, listing
every supported setting and its default. On first use, the game copies this template
to `%LOCALAPPDATA%/SuperMetroid/SuperMetroid.ini` if that active file does not exist.
You may edit the template before first launch. Afterward, edit the active file and
restart the game; the startup console prints its full path. Existing settings always
take precedence over the defaults template. Replacing the application ZIP/template never resets player settings.
Legacy/developer callers without an installation root use the INI beside their ROM.
The template contains no developer cheats or enabled automatic GitHub reporting.

## Android

Install the APK and tap **Choose ROM**. Use Android's document picker to select your
ROM from local storage or a document provider. The app reads the selected stream once,
validates it, and installs a private copy under its `files/` directory. Broad storage
permission and a permanently accessible source URI are unnecessary. Cancelling the
picker returns to setup. Updating the APK with `adb install -r` preserves app data.

Existing installations with a private ROM but no installation receipt automatically
extract and validate their audio on startup. No ROM is copied out of the APK.

## Storage and recovery

Within either platform's application-data root:

- `game/SuperMetroid.smc`: validated, unheadered ROM copy.
- `game/audio/`: extracted audio streams, WAVs, and metadata catalog.
- `game/installation.json`: extraction format version and ROM identity.
- `SuperMetroid.ini`, `SuperMetroid.save.json`, `debug-states/`, and
  `input-recordings/`: player data, outside the replaceable game directory.

Setup stages and validates all generated content before publishing it. A complete
installation is reused, including valid custom sample replacements. Missing or damaged
audio is regenerated from the installed ROM; this restores stock audio. Player data is
preserved. An interrupted directory swap is recovered on the next launch. Concurrent
installers cannot modify the same installation.

## Command line and verification

From the repository root:

```powershell
# Same installation used by the desktop application; optional final argument overrides its root.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets install "C:\ROMs\Super Metroid.smc"
# Extract just runtime audio into a chosen directory.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets audio-rom "C:\ROMs\Super Metroid.smc" "C:\private\audio"
# Synthetic validation, or full integration when a private ROM is supplied.
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import "C:\ROMs\Super Metroid.smc"
```

The integration verifier accepts an optional second argument containing reference audio
to compare every extracted file byte-for-byte. It also tests non-seekable document streams,
header normalization, invalid input, cancellation, asset repair, interrupted publication,
save preservation, and booting the production Android session from the installed layout.
Legacy raw-data/PNG/map extraction commands remain developer tools; normal setup does not
require their input directories. Keep all ROMs and generated game resources out of Git.

## Verified build

On September 10, 2026, the Release AOT APK was installed as the isolated package
`org.supermetroid.csharp.romimporttest` on a Retroid Pocket Classic. First-run setup
accepted the supplied ROM and reached the game menus; player testing confirmed the
game runs with audio. The existing game package and its saves were left intact.
The import integration suite and full Core verification suite passed using private
fixtures. The APK and desktop publish output were inspected and contained no ROM,
SPC upload streams, audio manifest, or WAV samples.
