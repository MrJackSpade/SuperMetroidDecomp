# Player-confirmed posture: Morph Ball only

The player explicitly ruled out unmorphing. The README's earlier unmorph/ceiling case is not a reproduction of their report.

`--shutter-morph-approaches` runs 288 valid room-local Morph Ball-only sequences. It combines both platforms, centered and adjoining-passage starts, repeated bombs, rolling away and rolling back. Every frame asserts that Samus's collision height remains the Morph Ball radius (7). The sweep reaches 38 pixels of overlap.

`dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --shutter-morph-repro` selects a single sequence and deliberately fails on the first overlap exceeding one pixel. It is an unresolved diagnostic, not part of the passing default suite.

Sequence: unchanged X-ray Scope room, slot 0, begin centered on the platform in Morph Ball; one neutral setup frame; Shoot every eight frames through frame 219; Right on frames 75..78; Left on frames 79..82; otherwise no direction. No Up or unmorph input. At frame 161 Samus is (371,72), pose $32, platform Y=109, and the support gap is -2. Continuing the sweep grows the overlap beyond a tile.

This supplies the first failing edge-contact assertion for the correct posture. Native movement/carry/ceiling behavior still requires comparison before applying a production change. Do not confuse this with proof that the cartridge never permits such an overlap, or with player validation of a fix.
