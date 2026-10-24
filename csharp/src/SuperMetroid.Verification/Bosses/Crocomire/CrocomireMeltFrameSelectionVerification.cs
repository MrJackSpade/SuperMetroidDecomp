using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks that Crocomire's melt selects the picture each band of his melt image shows, at the update the cartridge
/// uploaded that band (#1406).
/// </summary>
internal static class CrocomireMeltFrameSelectionVerification
{
    /// <summary>
    /// Runs both melt passes through their band showing and dissolve updates and, after every update, compares the picture
    /// each of the seven bands shows with the cartridge's schedule: one band an update at the unerased image, then the
    /// update after the last band advancing the phase; then each dissolve update erasing one column and showing the next
    /// band, cycling, at the new step, until all forty-nine columns are erased, after which no band changes. A band keeps
    /// the first pass's picture until the second pass shows its own.
    /// </summary>
    internal static void VerifyCrocomireMeltFrameSelection()
    {
        var fixture = new EnemyEffectsFixtureVerification.CrocomireEffectFixture(EnemyEffectsFixtureVerification.MeltEffectArtwork());
        var expected = new CrocomireMeltBandPicture?[CrocomireMeltingDefinitions.SecondPassBandCount];
        int updates = 0;
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            int bands = pass == CrocomireMeltingIllustration.First
                ? CrocomireMeltingDefinitions.FirstPassBandCount : CrocomireMeltingDefinitions.SecondPassBandCount;
            fixture.InitializeMap((int)(pass == CrocomireMeltingIllustration.First
                ? CrocomireMeltingTilemapAddress.FirstTilemap : CrocomireMeltingTilemapAddress.SecondTilemap));
            fixture.Call(c => state => c.Melting.BeginCrocomireMeltPass(state, pass));
            AssertBands(fixture, expected, $"{pass} pass begins");

            CrocomireDeathPhase showing = fixture.Actor.DeathSequenceIndex;
            for (int update = 1; update <= bands + 1; update++)
            {
                fixture.Call(c => c.Melting.ShowNextCrocomireMeltBand);
                updates++;
                if (update <= bands)
                    expected[update - 1] = new CrocomireMeltBandPicture(pass, 0);
                AssertEqual(update <= bands ? showing : showing.Next(), fixture.Actor.DeathSequenceIndex,
                    $"{pass} band update {update} phase");
                AssertBands(fixture, expected, $"{pass} band update {update}");
            }

            fixture.Call(c => c.Melting.BeginCrocomireMelting);
            CrocomireDeathPhase dissolving = fixture.Actor.DeathSequenceIndex;
            int step = 0;
            while (fixture.Actor.DeathSequenceIndex == dissolving)
            {
                fixture.Dissolve();
                updates++;
                if (step < CrocomireMeltingDefinitions.ColumnCount)
                {
                    step++;
                    expected[(step - 1) % bands] = new CrocomireMeltBandPicture(pass, step);
                }
                AssertEqual(step, fixture.Effect.MeltColumnsErased, $"{pass} dissolve update {step} columns erased");
                AssertBands(fixture, expected, $"{pass} dissolve update");
            }
            AssertEqual(CrocomireMeltingDefinitions.ColumnCount, step, $"{pass} dissolve erases every column before it ends");
            fixture.Call(c => c.Melting.FinishCrocomireMeltingPass);
            AssertBands(fixture, expected, $"{pass} pass finishes");
        }
        Console.WriteLine($"  Crocomire melt: {updates} updates over both passes select each band's picture on the cartridge's schedule.");
    }

    /// <summary>Asserts every band of <paramref name="fixture"/>'s melt shows the picture <paramref name="expected"/> holds for it.</summary>
    private static void AssertBands(EnemyEffectsFixtureVerification.CrocomireEffectFixture fixture,
        CrocomireMeltBandPicture?[] expected, string context)
    {
        for (int band = 0; band < expected.Length; band++)
            AssertEqual(EnemyEffectsFixtureVerification.CrocomireEffectFixture.MeltBandFrame(expected[band], band), fixture.MeltBand(band),
                $"{context}: band {band} picture");
    }

    /// <summary>Runs the Crocomire melt frame selection check.</summary>
    [VerificationCommand("--crocomire-melt-frames")]
    internal static int CrocomireMeltFramesCommand()
    {
        Suite(nameof(VerifyCrocomireMeltFrameSelection), () => VerifyCrocomireMeltFrameSelection());
        return 0;
    }
}
