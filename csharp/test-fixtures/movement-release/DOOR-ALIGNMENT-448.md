# #448: Door alignment timing and movement carry

Reference: https://wiki.supermetroid.run/Aligning_Doors (revision 9040).

## Cartridge evidence

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- CSV SHA256: `0BE25E6CEC03FD91BA8C2A322765D9F5DAE9BC2085523063B7F3745C759D9C21`.
- Two independent native captures were byte-identical.

`door-alignment-448-v1.csv` contains 1,072 calls into the original cartridge CPU at
`$82:E310`. The matrix covers all four door directions and low-byte camera offsets
`$00`, `$01`, `$02`, `$7F`, `$80`, `$FE`, and `$FF`. Time freeze is set only to suppress
the unrelated background-streaming child call; the tested coordinate branch and
coroutine handoff execute unmodified cartridge instructions.

Horizontal doors change only camera Y. Vertical doors change only camera X. Positive
signed low bytes decrement and negative signed low bytes increment, one pixel per call.
The first call that observes zero leaves the coordinate unchanged and advances the
coroutine from `$E310` to `$E353`. Therefore an offset of one costs one movement call
plus the common completion call; an already aligned door still takes that completion
call. This is the exact variable real-time delay described by the technique reference.

The frontend's door state never calls the gameplay clock step during any transition
phase. The four-direction production trajectory checks also retain both Samus
subposition words through visible scrolling and the destination handoff. That preserves
the movement state used by alignment-sensitive destination strategies instead of
rounding Samus to host pixels.

## Reproduction

Apply `native-door-alignment-entrypoint.patch` inside the pinned `upstream-sm` tree,
build Release x64, and run:

```text
sm.exe --diagnostic-door-alignment "Super Metroid.smc" door-alignment-448-v1.csv
SuperMetroid.Verification --door-alignment
```

The diagnostic output uses exclusive creation and refuses to overwrite an existing
capture. Reverse the temporary entrypoint patch after capture. The managed verifier
compares every camera word and completion seam against the CSV, then checks retained
subpixels in all four complete opening-scroll trajectories.

The wiki's quoted 42-frame normal transition is not asserted as a universal cartridge
constant: sound queues and processing can extend it, and the project's frontend keeps
those owners explicit. This fixture isolates only the alignment-dependent delay and the
movement carry claimed by the technique.
