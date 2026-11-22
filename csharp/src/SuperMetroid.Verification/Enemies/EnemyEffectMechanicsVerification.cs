using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyEffectsFixtureVerification;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks Crocomire's melt and Mother Brain's corpse rot against the cartridge's timing and schedules.</summary>
internal static class EnemyEffectMechanicsVerification
{
    /// <summary>
    /// The camera Crocomire's room shows him with: the room is one screen tall, so its camera never leaves the room's top
    /// row.
    /// </summary>
    private static readonly Position16<RoomPixels> RoomCamera = Position16<RoomPixels>.Zero;

    /// <summary>
    /// How far above him the cartridge's body area lay (#1802, #2031): its rows are his body's rows reckoned from his position
    /// less this.
    /// </summary>
    private static int AreaTop => BossBodySourceDefinitions.AreaFromBoss[BossBodyOwner.Crocomire].Y;

    /// <summary>
    /// The cartridge's vertical placement of his body layer, the row of his body's area its room's camera row shows (#1802),
    /// which its melt scroll table holds: his area lay 67 pixels above him.
    /// </summary>
    private static ushort NativePlacement(CrocomireEffectFixture fixture) =>
        unchecked((ushort)(RoomCamera.Y - (fixture.Actor.Body.Position.Y.Whole + AreaTop)));

    /// <summary>
    /// Rebuilds the cartridge's melt scroll table, as its loop builds it line by line, into <paramref name="scrolls"/> from the fixture's state after a
    /// dissolve step: from the line below the tongue, each line one row further behind, less the slope's carry, until the
    /// source row reaches the lowest; the lines after show the placement, and the lines before keep what they held.
    /// </summary>
    private static void BuildNativeMeltScrolls(ushort[] scrolls, CrocomireEffectFixture fixture)
    {
        CrocomireDeathState death = fixture.Effect;
        ushort placement = NativePlacement(fixture);
        int line = Math.Clamp(fixture.Actor.Tongue.Position.Y.Whole - 72, 0, 256);
        int adjustedY = death.DistortionSourceRow, currentY = adjustedY, fraction = 0;
        while (line < scrolls.Length && adjustedY < death.LowestDistortionSourceRow)
        {
            scrolls[line++] = unchecked((ushort)(placement + adjustedY - currentY));
            fraction += checked((int)death.DistortionStep.RawValue);
            if (fraction < 65536) adjustedY++;
            else fraction -= 65536;
            currentY++;
        }
        if (line < scrolls.Length) scrolls.AsSpan(line).Fill(placement);
    }

    /// <summary>
    /// Runs both melt passes and checks the band showing phases, the melt stretch against the cartridge's scroll table,
    /// the HDMA lifetime, and the erase-column, acid-smoke and phase-handoff cadences of every dissolve call.
    /// </summary>
    internal static void VerifyCrocomireMeltMechanics()
    {
        var fixture = new CrocomireEffectFixture(MeltEffectArtwork());
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            fixture.InitializeMap(pass);
            fixture.Call(_ => state => CrocomireMelting.BeginCrocomireMeltPass(state, pass));
            int bands = CrocomireMelting.BandRound(pass);
            ListStep<CrocomireDeathStage> phase = fixture.Actor.DeathStage;
            for (int band = 0; band <= bands; band++)
            {
                fixture.Call(_ => CrocomireMelting.ShowNextCrocomireMeltBand);
                AssertEqual(band == bands ? phase.Following : phase, fixture.Actor.DeathStage,
                    "only the update after the last band advances the phase");
                if (band < CrocomireMeltingDefinitions.MeltImageBandCount)
                    AssertEqual(0, fixture.MeltBandStep(band), "each band shows the unerased image on its own update");
            }

            fixture.Call(_ => CrocomireMelting.BeginCrocomireMelting);
            phase = fixture.Actor.DeathStage;
            // The cartridge's BG2 vertical-scroll table, from the first gameplay line, levelled as the melt starts.
            var nativeScrolls = new ushort[256];
            Array.Fill(nativeScrolls, NativePlacement(fixture));
            AssertEqual(CrocomireMeltDistortion.Level(fixture.Actor.Body.Position.Y.Whole), fixture.Effect.MeltDistortion,
                "the melt starts levelled where he lies");
            // The compiled slope starts at 256, grows by 384 to 20480, then completes on
            // the following call. This explicit bound comes from the source, not a probe.
            int calls = (20480 - 256 + 383) / 384 + 1;
            int initialSmokeObjects = fixture.Enemies.RoomEffectActors.Count(slot => slot.IsActive);
            for (int call = 1; call <= calls; call++)
            {
                fixture.Dissolve();
                AssertEqual(call < calls, fixture.Effect.MeltingScrollActive, "exact HDMA lifetime");
                if (call < calls)
                {
                    // The described stretch gives every gameplay line the row the screen's top shows under the camera row the
                    // cartridge's table gives it (#1468, #1514), his body's rows reckoned from his position (#2031).
                    BuildNativeMeltScrolls(nativeScrolls, fixture);
                    for (int line = 0; line < BackgroundLineScrolls.GameplayLines; line++)
                        AssertEqual(unchecked((ushort)RoomView.ShownRow(unchecked((ushort)(nativeScrolls[line] + AreaTop)))), ScrollShapeLines.StretchScroll(BossLineScrolls.Of(fixture.Effect.MeltDistortion, RoomCamera), line + ScreenGeometry.GameplayHudHeightPixels),
                            $"melt dissolve call {call} stretch at gameplay line {line}");
                }
                AssertEqual(call == calls ? phase.Following : phase, fixture.Actor.DeathStage, "exact dissolve phase handoff");
                AssertEqual(Math.Min(call, CrocomireMeltingDefinitions.ColumnCount), fixture.Effect.MeltColumnsErased, "fixed erase-column cadence");
                AssertEqual(initialSmokeObjects + (call + 5) / 6,
                    fixture.Enemies.RoomEffectActors.Count(slot => slot.IsActive), "fixed acid-smoke cadence");
                // A step on the round's place past the image's six bands shows none (#2573).
                if (call <= CrocomireMeltingDefinitions.ColumnCount && (call - 1) % bands < CrocomireMeltingDefinitions.MeltImageBandCount)
                    AssertEqual(call, fixture.MeltBandStep((call - 1) % bands), "each step shows the next band");
                // His body shows the melt image, stored once, hiding what the steps its bands last showed have erased (#1652).
                string meltImage = CrocomireMeltingFrameNames.Of(pass);
                AssertTrue(fixture.Enemies.Presentation.RequireBossBody().Publish().Frames.Any(frame => frame.Name == meltImage && frame.Hidden is not null),
                    $"melt dissolve call {call} shows his body as {meltImage}, through the dissolve's hidden columns");
            }
            fixture.Call(c => CrocomireMelting.FinishCrocomireMeltingPass);
        }
        Console.WriteLine("  Crocomire melt: band selection per update, 54 calls per dissolve, erase/HDMA/smoke/phase timing pass.");
    }

    /// <summary>Runs a corpse through all 118 rot calls, checking row falls and dust.</summary>
    internal static void VerifyMotherBrainCorpseRot()
    {
        var rot = new MotherBrainCorpseRottingState();
        int finished = 0;
        // Entry i waits 2*i calls and needs max(1,ceil(i/2)) destructive moves. The last
        // entry therefore completes on 94+24=118.
        for (int call = 1; call <= 118; call++)
        {
            ushort seed = unchecked((ushort)(call * 37));
            bool[] fallenBefore = [.. rot.Schedule.Rows.Select(row => row.HasFallen)];
            var step = rot.Step(new Position16<RoomPixels>(100, 120), MotherBrainRandomSample.FromGenerator(seed), (ushort)call);
            AssertEqual(call < 118, step.StillRotting, "exact corpse lifetime");
            // Rows fallen by this call, bottom row first: each is marked fallen, except the last
            // row, whose fall ends the rot.
            int[] finishedRows = Enumerable.Range(0, MotherBrainCorpseRottingState.RowCount)
                .Where(index => !fallenBefore[index] &&
                    (rot.Schedule.Rows[index].HasFallen ||
                        (!step.StillRotting && index == MotherBrainCorpseRottingState.RowCount - 1)))
                .ToArray();
            AssertEqual(finishedRows.Length, step.DustRequests.Count, "one dust request per completed corpse row");
            for (int request = 0; request < finishedRows.Length; request++)
            {
                MotherBrainCorpseDustRequest dust = step.DustRequests[request];
                int index = finishedRows[request];
                AssertEqual(2 * index + Math.Max(1, (index + 1) / 2), call, "exact per-row completion call");
                AssertEqual(unchecked((ushort)(100 + (seed & 31) - 16)), dust.Position.X, "native dust X and sampled RNG");
                AssertEqual((ushort)136, dust.Position.Y, "native dust Y");
                AssertEqual((call & 7) == 0, dust.Sound is not null, "native dust sound cadence");
                finished++;
            }
        }
        AssertEqual(MotherBrainCorpseRottingState.RowCount, finished, "each of the 48 completion callbacks fires exactly once");
        Console.WriteLine("  Mother Brain corpse: 118 exact calls, 48 row completions, dust/audio timing pass.");
    }

    /// <summary>Runs the Crocomire melt and Mother Brain corpse rot mechanics checks.</summary>
    [VerificationCommand("--enemy-effect-mechanics")]
    internal static int EnemyEffectMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireMeltMechanics), () => VerifyCrocomireMeltMechanics());
        Suite(nameof(VerifyMotherBrainCorpseRot), () => VerifyMotherBrainCorpseRot());
        return 0;
    }
}
