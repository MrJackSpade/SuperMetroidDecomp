# X-ray live capture (#348)

`horizontal.png` and `horizontal.smframe` are captures from the translated runtime,
not emulator screenshots. The room is the retail X-ray Scope chamber ($8F:A2CE).
The fixture places standing Samus on its open floor at (128,139), then uses normal
Select/Run input to activate the Scope and widen the beam.

The decorative wall at screen pixel (200,120) is backed by an air block. Its native
X-ray table entry replaces it with blank metatile $00FF. The regression asserts
that visible wall art becomes the stage-eight backdrop inside the beam. It also
checks every captured BG2 word against the setup-owned WRAM map, aiming/release,
retained frame ownership and immediate-software entrypoint agreement.

The same test loads the two explicitly excluded rooms ($A66A/$CEFB), verifying
that their original VRAM is retained, OBJ color math is disabled, and only CEFB
removes BG2 from the main screen. Separate tests cover all boss exclusions and
the native CGADSUB values.

Regenerate with:

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --xray-input
```

Generated diagnostics go under `csharp/test-temp/issue-348-xray`. Compare this
retained packet on either backend with:

```powershell
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --compare csharp/test-fixtures/issue-348-xray/horizontal.smframe --device hardware
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --compare csharp/test-fixtures/issue-348-xray/horizontal.smframe --device warp
```

Both comparisons pass exactly. This verifies backend consistency, not an
independent emulator image comparison. The live Fireflea fixed-color producer
remains to be audited before declaring #348 complete.

The release regression now asserts each teardown step: the release edge retains
the last window, the next two frames keep X-ray ownership with a closed window
and zero backdrop while BG2 restores, then ordinary rendering resumes. This
failed before the teardown capture change (the release edge returned the normal
room immediately). The post-setup NoBeam phase also publishes an empty window.

The startup regression checks all eight setup calls against the instruction list
at $91:D223: blending first appears on call two, the beam stays closed while its
maps are copied, and only call eight installs the active backdrop. Before this
correction, blending did not appear until setup had finished.

The final teardown call also retains the blend selection for its own HDMA pass,
even though it releases gameplay time and deletes the object. The test asserts
that retained operation with zero fixed color, followed by normal rendering on
the next pass. This final-boundary assertion failed before the correction.
