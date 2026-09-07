# #349: West Ocean BG2 page split

Run `dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --ocean-sky` from the repository root.

The unchanged retail room $8F:93FE ($00/$05), state $940B, selects main $C11B and setup $91CE. Native setup $88:A800 writes BG2SC=$4A (32x64); main $88:AF99 passes table $88:ADA6 to the shared sky updater. The port formerly admitted only the land wrappers, omitted ocean streaming, and interpreted the map as 64x32.

The regression failed before the fix: the actual capture selected 64x32 and `ScrollingSky` was null. It now checks every visible tile word against ocean ROM pointers through camera Y=256..768 in eight-pixel increments, checks the native vertical-page raster, and verifies that an ordinary runtime frame queues the ocean source table.

`background.png` is the isolated corrected BG2 at camera (128,768), after the downward streaming sweep. `wrong-layout.png` deliberately reinterprets that same retained memory and HDMA with the former 64x32 layout: the lower page appears alongside the upper page, breaking the cloud bands vertically. This comparison isolates addressing, not enemy/terrain art or player movement. These are port-generated diagnostics, not emulator captures. The test asserts the incorrect interpretation actually changes visible pixels.

Full verification and the Windows build passed. Player confirmation remains required.
