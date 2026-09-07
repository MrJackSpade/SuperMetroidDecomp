# #347 investigation: repeated bombs and posture changes

Run `dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --shutter-repeat`.

The tool executes 216 room-local sequences using unchanged $01/$22, both shutter slots, valid centered offsets -3/0/+3, nine bomb intervals and four unmorph timings. It starts in Morph Ball on the platform and sends ordinary Shoot edges for 220 frames, then observes through frame 319. An optional single Up input is sent at frame 45, 75 or 105. No production fix is claimed by this tool.

Morph-only runs produced at most one pixel of overlap. With slot 0, offset -3, Shoot every two frames and Up at frame 45, overlap becomes two pixels at frame 141 (Samus 357,68; platform Y=119), then grows to 49 pixels by frame 176. The full sweep's worst recorded overlap was 52 pixels. This involves standing/landing/falling posture changes near the shaft ceiling, unlike the previous morph-only approach sweep.

This is a reproducible deep-overlap candidate, not yet evidence that the cartridge behaves differently. Native ceiling-contact and carry behavior still need comparison. The player has been asked whether their report involved unmorphing. Keep #347 open without awaiting-player-validation until diagnosis and appropriate verification establish a fix.
