# Player fixtures for #329 / #330

Captured from the player's live slots on 2026-09-06 without loading, advancing,
replacing or rebuilding the running game. These reports have only been logged;
The original captures remain unchanged. The #329 investigation below records subsequent
reproduction and diagnosis; #330 still requires investigation.

- #329: `slot-0.smstate`, Crocomire sprite corruption.
- #330: `slot-1.smstate`, Grapple Beam not working.

The preserved Core/Desktop DLLs and PDBs came from
`src/SuperMetroid.Game/bin/Debug/net10.0-windows`. Both state headers match:

- Core MVID: `9a74c4a3-aa03-48e7-b3c6-56fdfbadd7d7`
- Desktop MVID: `956e95ad-62bd-4b05-b7fa-b0fa3a9c65f0`

State SHA-256:

- Slot 0: `E5E27B4DE14B4692A3CCF9327966F0EC1E19D2514BBD7A00426D1F4E4437E137`
- Slot 1: `313876866D8E5CEEB19A00B2F2ECB3B94AEDBF2E7B06C695AA9DC9BDDAD01958`

Use a disposable load directory and the exact-state harness with these assembly
references. Keep its normal MVID/ROM checks enabled. Do not overwrite the live
slots or rebuild these pinned assemblies. The state graph includes private game
data and belongs only in this private repository.

## #329 reproduction and fix

The `inspect` console project references only the pinned assemblies. It keeps the real
store's MVID/ROM/payload checks, copies the state to a disposable temporary directory,
and captures paused and unpaused PNGs without changing player saves:

```powershell
dotnet run --project csharp/test-fixtures/issue-329-330-player-states/inspect -c Release -- csharp/test-fixtures/issue-329-330-player-states 0 'Super Metroid.smc' csharp/test-temp/issue-329-inspection
```

Slot 0 is paused in room A98D, Samus (1052,155), camera (956,0), Crocomire at
(1152,120), death phase zero. Unpausing reproduced the detached upper body.
Current production reproduced the same cause on fight frame zero: BG2VOFS FFCB was
overridden by the allocated but inactive all-zero melting HDMA table.

The fix tracks the native object lifetime: spawn at A4:9555 and channel disable at
A4:95CE. Both the software path and display capture use scanline overrides only while
that object is active. Scratch-buffer allocation no longer means the effect is enabled.

`--crocomire-bg2-audit` in DebugRunner checks 60 fresh fight frames and writes the
corrected current frame. RenderVerification `--retail-crocomire` checks 60 fight frames,
HDMA lifetime at every death frame, and 180 GPU/software comparisons across 42 death
phases on both hardware and WARP. Fresh-runtime frames are not exact-state comparisons:
their purpose is to reproduce the same faulty register selection and verify the new code.
The old capture and corrected example were visually inspected; player validation remains
required before closing #329.
