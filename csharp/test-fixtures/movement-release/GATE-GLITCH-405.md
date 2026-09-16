# Green Hill Zone Grapple/Speed Booster gate parity (#405)

## Reported technique

The gate-glitch reference page links two demonstrations:

- <https://vimeo.com/715803825> — “Gate Glitch with Grapple and Speed”
- <https://vimeo.com/715793416> — “Gate Glitch with Grapple, Space, and Speed”

Both use the blue-left downward gate in Green Hill Zone (`$8F:9E52`). Samus
retains leftward Speed Booster momentum, jumps beside the gate, and fires Grapple
left. The beam crosses the gate column and reaches the switch on its far side.

This is not permission for Grapple to ignore gates. The room's resident `$C82A`
PLM installs five solid shootable cells at X=100, rows 55–59, while the `$C836`
shot-block PLM installs the blue-left `$46` trigger at X=99, row 55.

## Cartridge diagnosis

`BlockCollGrappleBeam` at `$94:A85B` divides the authored 8.8 Grapple velocity
into four collision probes. The routine only returns early for carry+overflow,
which means a successful Grapple connection. Ordinary solid carry does **not**
end the loop. After all four probes, `$9B:C703` sees only the fourth reaction.

In the reproduced success, the endpoint sequence is:

| Gameplay frame | Samus X | Endpoint X | Result |
| ---: | ---: | ---: | --- |
| 3 | 1642 | 1633 | air |
| 4 | 1637 | 1616 | air |
| 5 | 1637 | 1599 | the four subprobes cross the one-cell gate; final probe is air |
| 6 | 1637 | 1587 | air |
| 7 | 1637 | 1575 | blue-left trigger wakes the gate |

The old C# loop returned on the first ordinary solid subprobe. On frame 5 it
stopped at X=1608 inside the gate, queued cancellation, and could never reach the
switch. The fix preserves all four native subprobes and makes the cancellation
decision from the final reaction, while still accepting carry+overflow grapple
blocks immediately.

## Native evidence

`native-green-gate-probe.h` runs the pinned retail ROM through the original 65816
subroutines. Install the header temporarily into the native harness, expose
`DiagnosticGreenGateGrapple`, and run:

```text
sm.exe --green-gate-grapple "Super Metroid.smc" green-gate-405-native.csv
```

The committed `green-gate-405-native.csv` contains two successful fixed-point
setups and three adjacent failures. Every candidate uses:

- Green Hill Zone `$8F:9E52`
- Samus Y=939, subpositions zero
- moving-left firing pose `$0C`
- Grapple and Speed Booster equipped, Grapple selected
- active boost stage `$0400`, extra run speed 4
- Left+Jump on frame 0, then Left+Jump+Shoot through frame 20

The C# verifier drives the same actual room and production runtime. It compares
the success flag, activation frame, final fixed-point Samus position, pose,
Grapple endpoint, and Grapple function state for every row. Successful rows also
assert that the bank-$86 gate actor moves upward, covering the visible gate
handoff rather than stopping at the PLM timer.

Run only this regression with:

```text
dotnet run --project csharp/src/SuperMetroid.Verification/SuperMetroid.Verification.csproj -c Release -- --green-hill-gate-glitch
```
