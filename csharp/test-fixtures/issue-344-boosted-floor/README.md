# #344: accelerated running embeds Samus in the half-height floor

Run `dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --boost-floor-audit` from the repository root.

Reproduction uses unchanged retail room $8F:ADAD ($02/$1E), its enemies and PLMs. Place ordinary standing Samus at (80,420), equip Speed Booster and Varia, settle for 30 neutral frames, then hold Right + Dash for 240 frames. This reproduces normal accelerating running, not a shinespark.

Before correction, frame 69 moved Samus to (396,459), eight pixels below the half-height floor's standing center. Frame 70 reached (403,459), pose $89, and every remaining held-input frame stayed there. Correct movement keeps Y=451 across the half-height section (X=352..415), then reaches the real wall at X=475 with Y=443. The test asserts that floor trajectory and the true wall endpoint, not merely lack of a crash.

Native $94:8DBD sets `samus_pos_adjusted_by_slope_flag` after downward square-slope collision. $94:87F4 only sets the flag when aligning Y; it does not erase an existing one. $90:923F consumes that retained support flag to choose a one-pixel grounding probe instead of total X speed plus one. The next nonzero $94:9763 terrain entry clears it. The port omitted the square-slope write and overwrote the flag in horizontal alignment, allowing a fast probe to skip the eight-pixel floor.

Focused checks additionally assert set/preserve/clear behavior through downward square-floor contact, horizontal movement, zero vertical movement, and nonzero unobstructed terrain movement. Full verification and Windows build pass. Player confirmation remains required.
