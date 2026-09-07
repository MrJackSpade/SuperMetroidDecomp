# Player-supplied independent visual reference

Preserved September 7, 2026 from the two screenshots attached by the player to
their underwater turnaround/ledge report. These are unchanged image files, not
regenerated software-renderer output.

| File | Provenance | SHA-256 |
| --- | --- | --- |
| `maridia-snes9x-player.png` | Original clipboard attachment `codex-clipboard-IeEz7n.png`; visible title is “Maridia Jump Test - Snes9x 1.60” | `AACB98C4C20D4767ACEE0AC80CCED4CA842CBCC873FED64150B55EB65D0D02D0` |
| `maridia-managed-player.png` | Original clipboard attachment `codex-clipboard-zD9o0v.png`; visible managed window identifies room $8F:CEFB, state $8F:CF27, $04/$01 | `B56946EE5AD5B48DA7801AAD93C7BC51CBC3386B56AC456080C49F0F73333C39` |

The player described standing against the ledge, jumping and pressing left: both
versions land on the ledge, but the managed version lands farther onto it. This
pair was supplied as evidence for that movement discrepancy, not confirmation of
renderer correctness or a fix. Preserve the report's meaning.

The local emulator setup is documented in
[`issue-307-underwater-jump`](../issue-307-underwater-jump/README.md), including the
ROM and exported SRAM hashes. The screenshot alone does not independently prove
which ROM bytes or exact emulator freeze state were active at capture, so those
setup hashes must not be treated as cryptographic capture provenance.

## Permitted use and limits

- Independent reference for the visible room appearance and original player report.
- Not an exact golden: native frame, input timing and emulator state are not saved
  alongside these images. The HUD inventory, player position and animation differ.
- Images include window chrome and scaled game output; no native-resolution pixel
  equality is asserted or inferred from them.
- Does not establish cartridge correctness for Ceres, Kraid, doors, elevators or
  other historical regression scenes. Those references remain separately needed.
- Private ROM-derived imagery: keep with the private repository, not public assets.
