# Underwater jump comparison (#307)

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
