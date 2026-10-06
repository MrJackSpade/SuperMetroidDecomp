# Ridley death, flight and tail audit (#1172)

## Report and affected version

The player reported a crash during Ridley's death while holding Samus, with odd
flight and tail motion during attacks. Affected version: **0.4.2+719e01e72669478823123e3fea2926af0691c4f1**.
The supplied diagnostic records `Enemy $E1BF instruction mechanics pointer
$A6:CA9B has no compiled owner`. No state or input recording accompanied it.
The user requested source auditing without further visual clarification.

## Findings and changes

* **Death crash:** the real death routine releases Samus and spawns twelve breakup
  actors, but their 29 instruction lists had no compiled mechanics owner or visual
  selectors. Eight body-piece compositions were also absent. Added the complete
  native list family, routing, selectors and ordinary artwork. The exact CA9B
  exception was reproduced before the fix; the artwork check then reproduced the
  missing ED29 composition before that omission was fixed.
* **Swoop flight:** the shared velocity helper generated a sine wave with amplitude
  127 instead of the native signed table's 256 peak. This produced velocity 635
  instead of 1280 for a downward swoop at speed 1280. Replaced it with the existing
  exact integer table. Angle clamping now preserves the native word subtraction's
  N flag, including the right-facing recovery target at 8000 rather than snapping
  the angle through a host signed minimum.
* **Pogo tail and bounce:** only neutral tail control was implemented. Restored
  live pointed-down, normal pogo, stab-setup and stabbing controllers and the
  body's setup/bounce/recovery handoffs. Restored hover/pogo extension and stagger
  settings, bounce distances and native horizontal direction selection.
* **Tail aiming:** restored current-seed random fling admission without advancing
  RNG, and native priority for a nearby missile/super missile in the first five
  projectile slots. Proximity flings, requested whips, and their additional-angle
  parameters retain native ordering.

These are cartridge translations, not visual tuning. Unused tail dispatcher
entries 7/8 are not introduced. The native pogo activation timer is always zero
in live code; no synthetic nonzero timer behavior is added.

## Native evidence and scope

Checked against supported J/U NTSC 1.0 ROM SHA256
`12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72`
and the pinned InsaneFirebat disassembly at
`362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Relevant [bank A6 source](https://github.com/InsaneFirebat/sm_disassembly/blob/362be646929cf8e483f692b73a6561cfc2dc1d0d/src/bank_A6.asm):
CA47..CAF3 breakup lists; B335 attack choice; B493..B5BD swoop phases;
B5C4..B858 hover/pogo handoffs; B8A9 bounce direction; CB33..CE64 tail control;
D19D..D2A9 aim selection; D526..D61E acceleration; D800 swoop velocities.
The velocity multiplier is bank 86:C26C/C272, using A0:B443 samples.
Cross-reference: [Patrick Johnston bank A6](https://patrickjohnston.org/bank/A6).

Attack-table selection and the inspected swoop phase parameters/timers agree with
the native source. This audit addresses the reported flight/tail and death paths;
it is not a claim of complete Ridley audiovisual or whole-game parity.

## Confirmation and installation

`--ridley-breakup-programs` executes the real death/grab handoff and all twelve
spawned actors with a bus that rejects cartridge access. It checks all 29 programs'
mechanics, native frame selection, sleep, and exact low/high OAM against independent
ROM decoding. This is a faithful synthetic reproduction, not a recorded playthrough.

`--ridley-flight-tail` reproduces the pre-fix velocity error, checks both velocity
components against all 256 native samples, and confirms the angle boundary.
First-pass tail fixes have focused assertions for production setup, exact angles
and root position, stop/stab/recovery transitions, missile aiming, random fling
admission, and bounce direction/boundaries without RNG advancement.

Ordinary composition and enemy bundle schema advance to 69. Schema-68 artwork
overrides retain edits and inherit the eight new stock compositions; incomplete
old stock requests refresh through existing installation handling. The regression
confirms both inheritance and rejection of incomplete stock.

The static visual audit now covers 170 owners and 5,829 references. Its dependency
check caught all eight missing compositions once the previously absent program
owner was added. Its proof remains coverage of declared compiled programs, not
proof that every cartridge actor already has a compiled owner. Reviewed routing
and bundle-schema fingerprints were updated. PLM, queued-VRAM-DMA and enemy-visual
build gates also run for this change.

Player validation remains pending. Ticket closure follows the user's explicit
session instruction; no release tag is created by this fix.
