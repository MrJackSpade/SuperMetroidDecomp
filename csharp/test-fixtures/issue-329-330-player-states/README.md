# Player fixtures for #329 / #330

Captured from the player's live slots on 2026-09-06 without loading, advancing,
replacing or rebuilding the running game. These reports have only been logged;
no reproduction or diagnosis is claimed.

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
