using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Enemies.EnemyEffectsFixtureVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks that replacement effect artwork changes pixels without changing Crocomire or Mother Brain timing.</summary>
internal static class EnemyEffectIsolationVerification
{
    /// <summary>
    /// Checks that replacement melt artwork changes neither Crocomire's mechanics nor which picture each band of his melt
    /// image shows on each update (#1406).
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyCrocomireEffectIsolation()
    {
        var baseline = new CrocomireEffectFixture(MeltEffectArtwork(0x55));
        var edited = new CrocomireEffectFixture(MeltEffectArtwork(0xaa, seed: 3));
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            int source = (int)(pass == CrocomireMeltingIllustration.First
                ? CrocomireMeltingTilemapAddress.FirstTilemap : CrocomireMeltingTilemapAddress.SecondTilemap);
            baseline.InitializeMap(source); edited.InitializeMap(source);
            AssertCrocomireEffectMechanics(baseline, edited, "melt map replacement");
            AssertTrue(!baseline.Effect.Bg2WorkingTilemap.SequenceEqual(edited.Effect.Bg2WorkingTilemap), "authored melt map reaches scratch");
            // Crocomire's body draws into the enemy BG2 image (#1353).
            AssertTrue(!baseline.Enemies.Presentation.RequireBg2Image().Publish().Words.SequenceEqual(
                edited.Enemies.Presentation.RequireBg2Image().Publish().Words), "authored melt map reaches the BG2 image");

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
            // The compiled slope starts at 256, grows by 384 to 20480, then completes on
            // the following call. This explicit bound comes from the source, not a probe.
            int calls = (20480 - 256 + 383) / 384 + 1;
            int initialSmokeObjects = baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive);
            for (int call = 1; call <= calls; call++)
            {
                baseline.Dissolve(); edited.Dissolve();
                AssertCrocomireEffectMechanics(baseline, edited, "melt dissolve call " + call);
                AssertEqual(call < calls, baseline.Effect.MeltingHdmaActive, "exact HDMA lifetime");
                AssertEqual(call == calls ? phase.Next() : phase, baseline.Actor.DeathSequenceIndex, "exact dissolve phase handoff");
                AssertEqual(Math.Min(call, CrocomireMeltingDefinitions.ColumnCount), baseline.Effect.MeltColumnsErased, "fixed erase-column cadence");
                AssertEqual(initialSmokeObjects + (call + 5) / 6,
                    baseline.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive), "fixed acid-smoke cadence");
                if (call <= CrocomireMeltingDefinitions.ColumnCount)
                    AssertEqual(CrocomireEffectFixture.MeltBandFrame(new CrocomireMeltBandPicture(pass, call), (call - 1) % bands), baseline.MeltBand((call - 1) % bands), "each step shows the next band");
            }
            baseline.Call(c => c.Melting.FinishCrocomireMeltingPass); edited.Call(c => c.Melting.FinishCrocomireMeltingPass);
            AssertCrocomireEffectMechanics(baseline, edited, "finished melt pass");
            AssertTrue(baseline.Effect.Bg2WorkingTilemap.SequenceEqual(edited.Effect.Bg2WorkingTilemap), "both visual variants clear the completed melt tilemap");
            AssertTrue(baseline.Vram.Bytes.Slice(EnemyBg2FrameLayout.VramBase * 2, 2048)
                .SequenceEqual(edited.Vram.Bytes.Slice(EnemyBg2FrameLayout.VramBase * 2, 2048)), "exact cleared BG2 upload");
        }
        Console.WriteLine("  Crocomire replacement: both maps/images, band selection per update, 54 calls per dissolve, erase/HDMA/smoke/phase timing unchanged.");
    }

    /// <summary>Compares two corpse images through all 118 rot calls, checking row completion and RAM-to-VRAM transfers.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyMotherBrainRotIsolation()
    {
        var baselineEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var editedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        EnemyStagingSurfaces baseline = baselineEnemies.Room.Staging;
        EnemyStagingSurfaces edited = editedEnemies.Room.Staging;
        var baselineRot = new MotherBrainCorpseRottingState();
        var editedRot = new MotherBrainCorpseRottingState();
        baselineRot.Initialize(baseline);
        editedRot.Initialize(edited);
        int finished = 0;
        // Entry i waits 2*i calls and needs max(1,ceil(i/2)) destructive moves. The last
        // entry therefore completes on 94+24=118; different pixels cannot alter that.
        for (int call = 1; call <= 118; call++)
        {
            ushort seed = unchecked((ushort)(call * 37));
            short[] rowYBefore = Enumerable.Range(0, MotherBrainCorpseRottingState.EntryCount)
                .Select(index => MotherBrainCorpseRottingState.ReadEntry(baseline, index).YOffset)
                .ToArray();
            var expected = baselineRot.Step(baseline, new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            var actual = editedRot.Step(edited, new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            AssertEqual(call < 118, expected.StillRotting, "exact corpse lifetime");
            AssertEqual(expected.StillRotting, actual.StillRotting, "corpse lifetime independent of pixels");
            AssertAnimationValues(baselineRot, editedRot, "corpse counters at call " + call);
            AssertEqual<int?>(call < 118 ? call : null, expected.ShownRotStep, "every active call shows the step just taken, completion shows none");
            AssertEqual<int?>(expected.ShownRotStep, actual.ShownRotStep, "same corpse showing schedule");
            AssertTrue(expected.DustRequests.SequenceEqual(actual.DustRequests), "same completion dust and audio");
            // Rows finished by this call, in table order: `$DBDF` marks a completed row
            // `$FFFF`, except the final row, which completes as the call returns carry clear.
            int[] finishedRows = Enumerable.Range(0, MotherBrainCorpseRottingState.EntryCount)
                .Where(index => rowYBefore[index] >= 0 &&
                    (MotherBrainCorpseRottingState.ReadEntry(baseline, index).YOffset < 0 ||
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
                AssertEqual(MotherBrainCorpseRottingState.ReadEntry(baseline, index),
                    MotherBrainCorpseRottingState.ReadEntry(edited, index), "every corpse row's Y/delay is independent of artwork");
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
