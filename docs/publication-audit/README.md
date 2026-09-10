# Publication audit and history migration

Audit baseline: `93ba20a034fe3e0fe7972d792d73a326b1967259` (2026-09-10).
This repository stays at its existing URL. The migration does not change visibility.

## Removal inventory

`inventory.json` lists every historical path selected for removal and whether it
was present at the audit baseline. `purge-paths.txt` contains the same paths in
git-filter-repo's literal-path format. The migration regenerates this inventory
from the exact published tip before rewriting. Renames are included explicitly.

| Category | Disposition and evidence |
| --- | --- |
| Retail ROM | Remove `Super Metroid.smc`; its SHA-256 is `12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72`. |
| Extracted assets | Remove both `standalone-assets/` and `csharp/standalone-assets/`, including raw cartridge chunks, PNGs, WAV samples and SPC upload streams. |
| Debugger states | Remove all `.smstate` paths. All 15 distinct historical state blobs were decompressed and verified to contain the complete 3 MiB retail ROM, byte for byte. |
| Game captures | Remove fixture PNG/WAV/SPCU files. These include rendered game graphics and audio. |
| Player data | Remove tracked SRAM and controller recordings, including the otherwise ignored `.srm` and `.smrec` files. Local copies remain available. |
| Build/debug artifacts | Remove tracked DLL, EXE, PDB and `csharp/test-temp/` files. These are generated files, not the maintained source. |
| CSV trace archives | Retain. The audit examined all 98 distinct ZIP blobs at the earlier audit tip: 201 CSV members and no bundled ROMs/images/executables. Subsequent archives remain subject to the same review. |

At this baseline, 5,629 distinct historical paths are selected, including 2,946
currently tracked paths. No source implementation or authored CSV trace archive is
selected except disposable source under the already-ignored `csharp/test-temp/`.
`.gitignore` prevents normal re-addition; it does not prevent deliberate `git add -f`.

## Other findings and limits

Gitleaks 8.30.1 scanned historical patches, the checkout, historical archives,
decoded state/binary strings, and 509 issue bodies plus 922 comments. No credentials
were confirmed. Ten historical scanner findings were controller-variable
assignments, not API keys. A checkout archive-detection error on one C# file was
covered by a separate plain-file scan. This is a point-in-time scan, not proof that
no unknown or encoded secret can exist.

The pinned snesrev source has an MIT license (retained in the vendored directory).
The pinned disassembly has a Zero-Clause BSD license. These do not establish rights
to redistribute Nintendo's ROM/assets. The repository lacks a top-level license;
translated source provenance, attribution, and distribution notices still need
review before a public release. Removing assets is not legal clearance for all code.

Android packaging explicitly includes the local ROM and extracted audio; desktop
packaging copies extracted audio. Do not publish locally built packages without
changing those packaging paths. Fresh clones need privately supplied inputs and
regenerated assets; local development copies are intentionally preserved.

Thirty-one issue bodies and three comments contain local filesystem paths. One
non-noreply author email appears in Git metadata. Error reporting includes exception
text and recording paths and is enabled in the tracked INI. This migration preserves
issue history and commit identity by the owner's instruction. Review future public
error reporting and personal-data disclosure separately. No releases, Actions runs,
or Actions artifacts existed at the audit; Pages and wiki were disabled.

## Migration commands

Use Python 3.11+ and git-filter-repo 2.47.0. Keep other commits, pushes and Discord
posting paused. Run from a clean isolated checkout at the published main tip.
First commit/push the migration tooling and drain the normal Discord queue.

```text
python tools/publication-migration.py prepare
python tools/publication-migration.py preflight
python tools/publication-migration.py rewrite --filter-repo /private/path/git-filter-repo.py
python tools/publication-migration.py publish
python tools/publication-migration.py edit
python tools/publication-migration.py reconcile
pwsh -File tools/discord-updates.ps1 next
```

`--repository PATH` selects the isolated checkout. `preflight` and `edit` accept
`--limit N` for bounded batches. `status` returns counts without message bodies.
Resume the same command and state after a failure; never initialize a fresh tracker.

Recovery material is in `<git-common-directory>/publication-migration/`: a verified
original Git bundle, original tracker/messages, old-to-new `commit-map`, mapped
messages, verification results, local index backups, and atomic progress checkpoints.
This directory contains private original material. Keep it private and back it up;
never commit or upload it to the public repository.

The rewrite disables empty/degenerate commit pruning. Every commit must have exactly
one replacement. Verification compares every retained path, mode and blob identity
in every commit and verifies author/committer identities, dates, full messages, and
mapped parent topology. A lease prevents overwriting an unexpected remote update.

Discord preflight verifies every original message. Editing changes only the version
heading and terminal commit link, preserves the summary and message ID, disables
mentions, and verifies both the PATCH receipt and subsequent GET. An uncertain edit
is reconciled by GET on resume, without reposting. The normal queue stays blocked
until every edit, tracker migration and local checkout reconciliation has completed.

Local reconciliation updates indexes without deleting the removed files from disk,
preserves pre-existing unstaged edits, and checks their hashes. Staged work or
unindexed local commits cause a stop instead of a reset. The main checkout is then
fast-forwarded to the sanitized published tip.

Force-pushing sanitized branches does not guarantee immediate deletion of old GitHub
objects/cached commit pages. GitHub-side cleanup may require support. Do not assume
that changing repository visibility makes unreachable original objects safe.
See [GitHub's removal guidance](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository).

## Verification

```text
FILTER_REPO_SCRIPT=/private/path/git-filter-repo.py python tests/test_publication_migration.py -v
pwsh -File tests/discord-updates.Tests.ps1
```

The offline suite exercises a real synthetic rewrite (including a ROM-only commit,
a renamed file, and a merge), strict tree verification, message-edit receipt loss,
external message changes, missing mappings, wrong destinations, and queue locking.
