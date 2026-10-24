using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Enemies.EnemyEffectsFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks missing boss-effect art failures and Crocomire melt document validation.</summary>
internal static class EnemyEffectResourcesVerification
{
    /// <summary>Confirms missing or invalid corpse and melt resources fail before mutating actors, WRAM, or VRAM.</summary>
    internal static void VerifyEnemyEffectResources()
    {
        // Mother Brain's corpse art is staged and rotted by the extraction (#1392), which fails on missing art; the
        // runtime reads none.
        CheckCrocomireResourceFailure(null, fixture => fixture.InitializeMap((int)CrocomireMeltingTilemapAddress.FirstTilemap),
            "missing melt tilemap");
        CheckCrocomireResourceFailure(null, fixture => fixture.Call(c => state => c.Melting.BeginCrocomireMeltPass(state, CrocomireMeltingIllustration.First)),
            "missing melt graphics");
        CrocomireMeltingArtwork art = MeltEffectArtwork();
        CheckCrocomireResourceFailure(art, fixture => fixture.InitializeMap(0), "unknown melt tilemap");
        Console.WriteLine("  Enemy effect resources: missing/unknown art fails before changing WRAM, phases, actors, graphics or VRAM.");
    }

    /// <summary>Checks that one Crocomire resource error leaves effect mechanics, band pictures, tilemap, and VRAM intact.</summary>
    private static void CheckCrocomireResourceFailure(CrocomireMeltingArtwork? art,
        Action<CrocomireEffectFixture> operation, string context)
    {
        var expected = new CrocomireEffectFixture(art);
        var actual = new CrocomireEffectFixture(art);
        AssertThrows<InvalidDataException>(() => operation(actual), context + " fails loudly");
        AssertCrocomireEffectMechanics(expected, actual, context);
        AssertTrue(expected.Effect.Bg2WorkingTilemap.SequenceEqual(actual.Effect.Bg2WorkingTilemap), context + " tilemap unchanged");
        AssertTrue(expected.Vram.ImageEquals(actual.Vram), context + " VRAM unchanged");
    }

    /// <summary>Accepts valid melt JSON casing and rejects unknown, repeated, or case-aliased fields.</summary>
    internal static void VerifyCrocomireMeltJson()
    {
        string valid = MeltEffectJson();
        _ = MeltEffectArtwork(firstJson: valid);
        _ = MeltEffectArtwork(firstJson: valid.Replace("\"version\"", "\"Version\"", StringComparison.Ordinal));
        foreach ((string name, string malformed) in new[]
        {
            new VerifyCrocomireMeltJsonCase("unknown root field", valid.Replace("\"version\":1", "\"version\":1,\"unknown\":0", StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("duplicate root field", valid.Replace("\"version\":1", "\"version\":1,\"version\":1", StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("case-aliased root field", valid.Replace("\"version\":1", "\"version\":1,\"VERSION\":1", StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("unknown cell field", valid.Replace("\"tileIndex\":0", "\"tileIndex\":0,\"unknown\":0", StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("duplicate cell field", valid.Replace("\"tileIndex\":0", "\"tileIndex\":0,\"tileIndex\":0", StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("case-aliased cell field", valid.Replace("\"tileIndex\":0", "\"tileIndex\":0,\"TILEINDEX\":0", StringComparison.Ordinal)),
        })
            AssertThrows<InvalidDataException>(() => MeltEffectArtwork(firstJson: malformed), name + " cannot be silently discarded");
        Console.WriteLine("  Crocomire melt JSON: valid schema/casing preserved; unknown, duplicate and case-aliased root/cell fields rejected.");
    }

    /// <summary>Runs missing enemy-effect resource checks.</summary>
    [VerificationCommand("--enemy-effect-resources")]
    internal static int EnemyEffectResourcesCommand()
    {
        Suite(nameof(VerifyEnemyEffectResources), () => VerifyEnemyEffectResources());
        return 0;
    }

    /// <summary>Runs Crocomire melt JSON schema checks.</summary>
    [VerificationCommand("--crocomire-melt-json")]
    internal static int CrocomireMeltJsonCommand()
    {
        Suite(nameof(VerifyCrocomireMeltJson), () => VerifyCrocomireMeltJson());
        return 0;
    }
}
