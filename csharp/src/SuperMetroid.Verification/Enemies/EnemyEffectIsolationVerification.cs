using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Enemies.EnemyEffectsFixtureVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks that replacement effect artwork changes pixels without changing Crocomire or Mother Brain timing.</summary>
internal static class EnemyEffectIsolationVerification
{
    /// <summary>
    /// Rebuilds the cartridge's melt scroll table, as its loop builds it line by line, into <paramref name="scrolls"/> from the fixture's state after a
    /// dissolve step: from the line below the tongue, each line one row further behind, less the slope's carry, until the
    /// source row reaches the lowest; the lines after show the placement, and the lines before keep what they held.
    /// </summary>
    private static void BuildNativeMeltScrolls(ushort[] scrolls, CrocomireEffectFixture fixture)
    {
        CrocomireDeathState death = fixture.Effect;
        ushort placement = fixture.Enemies.CrocomireBehavior.BodyLayer.Placement.Y;
        int line = Math.Clamp((fixture.Actor.Tongue?.Position.Y.Whole ?? fixture.Actor.Body.Position.Y.Whole) - 72, 0, 256);
        int adjustedY = death.DistortionSourceRow, currentY = adjustedY, fraction = 0;
        while (line < scrolls.Length && adjustedY < death.LowestDistortionSourceRow)
        {
            scrolls[line++] = unchecked((ushort)(placement + adjustedY - currentY));
            fraction += death.DistortionStep;
            if (fraction < 65536) adjustedY++;
            else fraction -= 65536;
            currentY++;
        }
        if (line < scrolls.Length) scrolls.AsSpan(line).Fill(placement);
    }

    /// <summary>
    /// Checks that replacement melt artwork changes neither Crocomire's mechanics nor which picture each band of his melt
    /// image shows on each update (#1406).
    /// </summary>
    internal static void VerifyCrocomireEffectIsolation()
    {
        var baseline = new CrocomireEffectFixture(MeltEffectArtwork(0x55));
        var edited = new CrocomireEffectFixture(MeltEffectArtwork(0xaa, seed: 3));
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            CrocomireMeltingIllustration source = pass;
            baseline.InitializeMap(source); edited.InitializeMap(source);
            AssertCrocomireEffectMechanics(baseline, edited, "melt map replacement");
            // Both variants draw the installed melt image's parts on the same cells (#1466); its pictures are the import's.
            AssertTrue(BossBodyCanvasFixtures.ShowSame(baseline.Enemies.Presentation.RequireBossBody(), edited.Enemies.Presentation.RequireBossBody(),
                CrocomireEffectFixture.Sheets), "both visual variants show the melt image's parts on the same cells");

            baseline.Call(c => state => c.Melting.BeginCrocomireMeltPass(state, pass));
            edited.Call(c => state => c.Melting.BeginCrocomireMeltPass(state, pass));
            AssertCrocomireEffectMechanics(baseline, edited, "melt pass begins");
            int bands = CrocomireMelting.BandCount(pass);
            CrocomireDeathPhase phase = baseline.Actor.DeathSequenceIndex;
            for (int band = 0; band <= bands; band++)
            {
                baseline.Call(c => c.Melting.ShowNextCrocomireMeltBand); edited.Call(c => c.Melting.ShowNextCrocomireMeltBand);
                AssertCrocomireEffectMechanics(baseline, edited, "melt band " + band);
                AssertEqual(band == bands ? phase.Next() : phase, baseline.Actor.DeathSequenceIndex,
                    "only the update after the last band advances the phase");
                if (band < bands)
                    AssertEqual(CrocomireEffectFixture.MeltBandFrame(new CrocomireMeltBandPicture(pass, 0), band), baseline.MeltBand(band), "each band shows the unerased image on its own update");
            }

            baseline.Call(c => c.Melting.BeginCrocomireMelting); edited.Call(c => c.Melting.BeginCrocomireMelting);
            phase = baseline.Actor.DeathSequenceIndex;
            // The cartridge's BG2 vertical-scroll table, from the first gameplay line, levelled as the melt starts.
            var nativeScrolls = new ushort[256];
            Array.Fill(nativeScrolls, baseline.Enemies.CrocomireBehavior.BodyLayer.Placement.Y);
            AssertEqual(CrocomireMeltDistortion.Level(baseline.Enemies.CrocomireBehavior.BodyLayer.Placement.Y), baseline.Effect.MeltDistortion,
                "the melt starts levelled at his body layer's placement");
            // The compiled slope starts at 256, grows by 384 to 20480, then completes on
            // the following call. This explicit bound comes from the source, not a probe.
            int calls = (20480 - 256 + 383) / 384 + 1;
            int initialSmokeObjects = baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive);
            for (int call = 1; call <= calls; call++)
            {
                baseline.Dissolve(); edited.Dissolve();
                AssertCrocomireEffectMechanics(baseline, edited, "melt dissolve call " + call);
                AssertEqual(call < calls, baseline.Effect.MeltingScrollActive, "exact HDMA lifetime");
                if (call < calls)
                {
                    // The described stretch gives every gameplay line the scroll the cartridge's table gives it (#1468).
                    BuildNativeMeltScrolls(nativeScrolls, baseline);
                    for (int line = 0; line < BackgroundLineScrolls.GameplayLines; line++)
                        AssertEqual(nativeScrolls[line], ScrollShapeLines.StretchScroll(BossLineScrolls.Of(baseline.Effect.MeltDistortion), line + ScreenGeometry.GameplayHudHeightPixels),
                            $"melt dissolve call {call} stretch at gameplay line {line}");
                }
                AssertEqual(call == calls ? phase.Next() : phase, baseline.Actor.DeathSequenceIndex, "exact dissolve phase handoff");
                AssertEqual(Math.Min(call, CrocomireMeltingDefinitions.ColumnCount), baseline.Effect.MeltColumnsErased, "fixed erase-column cadence");
                AssertEqual(initialSmokeObjects + (call + 5) / 6,
                    baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive), "fixed acid-smoke cadence");
                if (call <= CrocomireMeltingDefinitions.ColumnCount)
                    AssertEqual(CrocomireEffectFixture.MeltBandFrame(new CrocomireMeltBandPicture(pass, call), (call - 1) % bands), baseline.MeltBand((call - 1) % bands), "each step shows the next band");
            }
            baseline.Call(c => c.Melting.FinishCrocomireMeltingPass); edited.Call(c => c.Melting.FinishCrocomireMeltingPass);
            AssertCrocomireEffectMechanics(baseline, edited, "finished melt pass");
            AssertTrue(BossBodyCanvasFixtures.ShowSame(baseline.Effect.WorkingBody!, edited.Effect.WorkingBody!, CrocomireEffectFixture.Sheets),
                "both visual variants clear the completed melt canvas");
            AssertTrue(baseline.Vram.Bytes.Slice(EnemyBg2FrameLayoutConstants.VramBase * 2, 2048)
                .SequenceEqual(edited.Vram.Bytes.Slice(EnemyBg2FrameLayoutConstants.VramBase * 2, 2048)), "exact cleared BG2 upload");
        }
        Console.WriteLine("  Crocomire replacement: both maps/images, band selection per update, 54 calls per dissolve, erase/HDMA/smoke/phase timing unchanged.");
    }

    /// <summary>Compares two corpses through all 118 rot calls, checking row falls, dust and the steps shown.</summary>
    internal static void VerifyMotherBrainRotIsolation()
    {
        var baselineRot = new MotherBrainCorpseRottingState();
        var editedRot = new MotherBrainCorpseRottingState();
        int finished = 0;
        // Entry i waits 2*i calls and needs max(1,ceil(i/2)) destructive moves. The last
        // entry therefore completes on 94+24=118; different pixels cannot alter that.
        for (int call = 1; call <= 118; call++)
        {
            ushort seed = unchecked((ushort)(call * 37));
            bool[] fallenBefore = [.. baselineRot.Schedule.Rows.Select(row => row.HasFallen)];
            var expected = baselineRot.Step(new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            var actual = editedRot.Step(new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            AssertEqual(call < 118, expected.StillRotting, "exact corpse lifetime");
            AssertEqual(expected.StillRotting, actual.StillRotting, "corpse lifetime independent of pixels");
            AssertAnimationValues(baselineRot, editedRot, "corpse counters at call " + call);
            AssertEqual<int?>(call < 118 ? call : null, expected.ShownRotStep, "every active call shows the step just taken, completion shows none");
            AssertEqual<int?>(expected.ShownRotStep, actual.ShownRotStep, "same corpse showing schedule");
            AssertTrue(expected.DustRequests.SequenceEqual(actual.DustRequests), "same completion dust and audio");
            // Rows fallen by this call, bottom row first: each is marked fallen, except the last
            // row, whose fall ends the rot.
            int[] finishedRows = Enumerable.Range(0, MotherBrainCorpseRottingState.EntryCount)
                .Where(index => !fallenBefore[index] &&
                    (baselineRot.Schedule.Rows[index].HasFallen ||
                        (!expected.StillRotting && index == MotherBrainCorpseRottingState.EntryCount - 1)))
                .ToArray();
            AssertEqual(finishedRows.Length, expected.DustRequests.Count, "one dust request per completed corpse row");
            for (int request = 0; request < finishedRows.Length; request++)
            {
                MotherBrainCorpseDustRequest dust = expected.DustRequests[request];
                int index = finishedRows[request];
                AssertEqual(2 * index + Math.Max(1, (index + 1) / 2), call, "exact per-row completion call");
                AssertEqual(unchecked((ushort)(100 + (seed & 31) - 16)), dust.Position.X, "native dust X and sampled RNG");
                AssertEqual((ushort)136, dust.Position.Y, "native dust Y");
                AssertEqual((call & 7) == 0, dust.SoundEffectQueued, "native dust sound cadence");
                finished++;
            }
            for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
            {
                CorpseRowFall baselineRow = baselineRot.Schedule.Rows[index], editedRow = editedRot.Schedule.Rows[index];
                AssertEqual(baselineRow.PixelRow, editedRow.PixelRow, "every corpse row's pixel row is independent of artwork");
                AssertEqual(baselineRow.DelayCalls, editedRow.DelayCalls, "every corpse row's delay is independent of artwork");
                AssertEqual(baselineRow.HasFallen, editedRow.HasFallen, "every corpse row's fall is independent of artwork");
            }
        }
        AssertEqual(MotherBrainCorpseRottingState.EntryCount, finished, "each of the 48 completion callbacks fires exactly once");
        AssertEqual(118u, baselineRot.ProcessCallCount, "fixed corpse completion clock");
        Console.WriteLine("  Mother Brain corpse: 118 exact calls, 48 row completions, 117 rot steps shown on every row, dust/audio timing unchanged.");
    }

    /// <summary>Runs the Crocomire melt and Mother Brain corpse artwork isolation checks.</summary>
    [VerificationCommand("--enemy-effect-isolation")]
    internal static int EnemyEffectIsolationCommand()
    {
        Suite(nameof(VerifyCrocomireEffectIsolation), () => VerifyCrocomireEffectIsolation());
        Suite(nameof(VerifyMotherBrainRotIsolation), () => VerifyMotherBrainRotIsolation());
        return 0;
    }
}
