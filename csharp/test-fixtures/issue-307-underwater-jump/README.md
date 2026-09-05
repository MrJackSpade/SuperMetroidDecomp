# Underwater jump comparison (#307)

## Corrected native-emulator save

Use `Maridia Jump Test.srm` with a ROM named `Maridia Jump Test.smc`.
FILE C in the original raw export below incorrectly enters the intro: the
managed save writer leaves loading_game_state at payload offset $0154 zero.
Cartridge $81:8085 accepts its checksum, then $82:EEB4 chooses state $001E.
The corrected fixture explicitly sets FILE C's entry point to $0005 and
updates both checksum/complement pairs. All other bytes remain unchanged.
The general managed save writer has not been changed by this export repair.

Export command: `--export-replay-sram <recording> <rom> <new-output> 2`.
The optional zero-based slot argument requests main-game entry explicitly;
omitting it still exports the original bytes without modification.

Verified with real 65816 ROM subroutine execution, not just managed decoding:

```
Original:  ROM LOAD carry=0 loading=0000 area=0004 station=0000 equipment=3105
           ROM START game_state=001E
Corrected: ROM LOAD carry=0 loading=0005 area=0004 station=0000 equipment=3105
           ROM START game_state=0005
```

`native-save-load-probe.patch` preserves the temporary headless probe for
reproduction against upstream-sm. Build and invoke its executable with
`--save-load-probe <rom> <sram>`; it does not initialize SDL or open a window.
The patch is not left applied to the upstream working tree.
`Verify-Export.ps1` checks a fresh export byte-for-byte against this verified
fixture and asserts that nothing beyond the entry point/checksums changes.

Corrected SRAM SHA-256:
`10C4EC00EA3683E545A278F4B3D453A144EC11711CB19DB87797C6FFCB01FF8D`.

## Original raw export (retained as failing fixture)

`Super Metroid.srm` is the unchanged 8192-byte initial SRAM from
`input-recordings/SuperMetroid-input-20260905-174826-322.smrec`.
Choose FILE C: Maridia save room $CED2, equipped items $3105 (including
Hi-Jump, excluding Gravity Suit), health 405/599. Walk left to the broken
tube room for the player's standing, held-jump comparison.

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
SRAM SHA-256: `1822A42432106E16BA63B7DFB1B8801D72C8F7A261D041BF7F9A6BE2E9AABE5C`.

In SNES9X 1.60 Windows, place the ROM beside the executable and SRAM in
its default `Saves` directory with the same basename. Load the ROM and
select FILE C normally; this is a battery save, not an emulator freeze state.

Re-export with DebugRunner:

```
--export-replay-sram <recording.smrec> <matching-rom.smc> <new-output.srm>
```

The exporter validates the recording, ROM digest and cartridge slot checksums,
and refuses to overwrite an existing file.

Investigation remains open: a temporary headless 65816/managed comparison
matched 100 frames of position, fractional velocity, animation and spritemap
origin in a constructed empty water room. This does not reproduce or disprove
the player's roughly one-tile difference relative to the real-room flowers.
No jump-height adjustment has been made from that limited comparison.

## Standing-turn investigation (unresolved)

Player confirmed this local ROM matches the managed jump height. Their other
emulator setup differs in jump height, but its cause/revision is not established.
Both emulator setups reportedly return Samus to the same spot after an underwater
standing turn; the managed game instead moves her far enough to leave a ledge.

`--underwater-turn-probe <rom>` records both a stationary single-tap turn on
the lower sloped floor in CEFB and mirrored isolated turns on a constructed flat
floor. The flat case moves 5.25 pixels over the turn. The headless cartridge
probe in `native-turn-probe.patch` also moves 5.25 pixels with this constructed
setup (see `native-turn-trace.txt`). It includes input, movement, animation,
block-collision transitions and pose transitions. Thus this is NOT a reproduction
of the player's native/managed difference, nor a passing regression/fix claim.

Requested next evidence: a SNES9X freeze state while stationary on the player's
actual ledge before turning. Do not introduce a compensating offset on the basis
of this limited fixture or close the turnaround report.
