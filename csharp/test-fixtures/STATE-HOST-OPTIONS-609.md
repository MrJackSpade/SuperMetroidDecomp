# Active host options after debugger restoration (#609)

Affected version: v0.3.1.

The reported executable-directory INI and current AppData slot 0 reproduced
the mismatch: INI Invincibility/InfiniteAmmo true, restored runtime false/false.
The captured room was B283. The state and INI were read without modification.

Both the frontend options and runtime guard properties were serialized into the
old graph. Interactive hosts replaced the graph without rebinding current policy.
The state store now accepts an explicit optional host-options override. Windows
and Android provide their active session options. Exact diagnostic/replay callers
omit it and retain captured behavior. Rebinding changes policy, not resources,
cartridge state, or the retained display. Future runtime creation uses the same
rebound frontend options.

The actual v0.3.1 state also uses room callback compiler ordinal 463. Adding runtime
members renumbered it. The existing narrow logical callback migration now accepts
that known legacy ordinal, checking the unchanged runtime/room capture layout and
method signatures. The v0.3.1/current RoomLoading diff adds only the Shaktool setup
call, not changes to captured callbacks. A regression covers that identity alongside
the previously supported 443/461 identities; unknown closures are not guessed.

Run IntegrationVerification with:

`--state-host-options <SuperMetroid-debug-slot-0.smstate> <SuperMetroid.ini>`

The INI must enable both protections. Assertions cover unchanged initial resources,
frontend/runtime option agreement, zero-to-one energy and each unlocked ammo type
after the real runtime frame, actual HUD digit tile words, locked ammo remaining
zero, and removal of protection when options are disabled. This is a deliberately
constructed resource-boundary test on the player's loaded room, not a claim to
have replayed the player's combat input.

Verification: reported-state regression, full core suite, host-independent Android
session/state integration suite, and Windows Release build. Player confirmation
remains outstanding. No private state, ROM, INI, or imagery is published here.
