# Murder Beam original-CPU probe (#399)

Temporarily include `native-release-probe.h` followed by
`native-murder-beam-probe.h` in `upstream-sm/src/sm_rtl.c`, expose
`DiagnosticMurderBeam` from `upstream-sm/src/main.c`, and invoke it before SDL
startup. Build the x64 Release target, run it against the project's pinned retail
ROM, then restore both entrypoint files and rebuild the normal executable. The
probe changes only synthetic WRAM and does not write save data.

The bounded setup places Samus at `(128,128)`, facing left, with raw equipped-beam
word `$100F`, then calls native `FireChargedBeam` at `$90:B986`. Its first line is:

```text
MURDER fire count=1 cooldown=0 type=901F damage=200 dir=7 xy=116/123 pre=A4AA list=0000 radius=0/0 speed=0000/0000 exit=0004/0000/003C
```

Sixteen subsequent calls to `HandleProjectile` at `$90:AECE` preserve every
reported field. The zero instruction-list pointer makes the projectile handler
skip animation and callback execution; it does not deallocate the slot. The
nonzero type and damage remain available to bank-$A0 enemy collision. This is why
the safe left-facing Murder Beam is invisible, stationary, persistent, and still
damaging. `$90:A4AA` is the charged callback-table overread for combination 15,
but it is dormant in this safe direction because the instruction pointer is zero.

The C# verification is:

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification/SuperMetroid.Verification.csproj -c Release -- --murder-beam
```

It drives the real pause-menu same-frame Boots Left+A setup after charging, checks
the cartridge values above, and uses the production ordinary-enemy collision path
to prove repeated 200-point damage and Ice lethal-hit substitution. It also
asserts that an unsafe right-facing setup does not silently become an ordinary
beam. The probe intentionally does not run that unsafe native callback: the
original CPU leaves the bounded firing routine for unrelated code and does not
provide a stable gameplay result suitable for a normal session.
