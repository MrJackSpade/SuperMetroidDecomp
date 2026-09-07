# Issue #350: grounded grapple release floor clip

Preserved player debugger slot 0 on September 7, 2026. The live slot remains
untouched; use this copy for investigation after the player overwrites slot 0.

Player trigger: release the Grapple Beam while on the ground; Samus can clip
into the floor. Exact timing and root cause have not yet been investigated.

- Fixture: `slot-0.smstate` (2,272,543 bytes).
- SHA-256: `5927AE95EC79649C32E26213DAFD5D3EC8304E69A939E16625C2B821D2BC7033`.
- Repository HEAD at preservation: `f2a2a1545b6b1e29782ea488ebe7f20b7ae03800`.
  This is provenance, not proof that the running player's build matched HEAD.
- Source: `debug-states/SuperMetroid-debug-slot-0.smstate`.

Private debugger-state fixture; may contain cartridge-derived data. Do not
distribute publicly. No reproduction or fix is claimed by preserving this file.

`slot-0-named.smstate` is a schema-3 copy converted by the known-compatible
`61edf37` core using the production load/save path, without advancing frame 8187.
SHA-256: `B67621C554E56AD77A4357E071D522EA7D3B202D5C9E9E85D3673B0ADE0FC0E9`.
The original remains unchanged. This companion conversion was performed alongside
issue #353 to preserve both fixtures before compiler method tokens shifted.
