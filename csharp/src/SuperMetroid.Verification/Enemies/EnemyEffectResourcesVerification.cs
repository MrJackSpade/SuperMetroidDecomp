using SuperMetroid.Core.Enemies;
using static SuperMetroid.Verification.Enemies.EnemyEffectsFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks missing boss-effect art failures and Crocomire melt document validation.</summary>
internal static class EnemyEffectResourcesVerification
{
    /// <summary>Confirms missing or invalid corpse and melt resources fail before mutating actors, WRAM, or VRAM.</summary>
    internal static void VerifyEnemyEffectResources()
    {
        // Mother Brain's corpse art is staged and rotted by the extraction (#1392), which fails on missing art; the
        // runtime reads none. Crocomire's melt images are composed into his body sheet at installation (#1466), which
        // fails on missing melt art; gameplay reads only his installed body sheet.
        AssertThrows<InvalidDataException>(() => BossBodySheetComposer.Frames(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BossArtwork, BossBodyOwner.Crocomire),
            "missing melt art fails the installation");
        CrocomireMeltingArtwork art = MeltEffectArtwork();
        // An illustration that is neither of his two melt images names no frame of his body sheet: the frame-name table
        // rejects it as an argument outside the illustrations (#2401), before any stage, actor or body changes.
        CheckCrocomireResourceFailure<ArgumentOutOfRangeException>(art,
            fixture => fixture.InitializeMap((CrocomireMeltingIllustration)(-1)), "unknown melt image");
        Console.WriteLine("  Enemy effect resources: missing/unknown art fails before changing WRAM, phases, actors, graphics or VRAM.");
    }

    /// <summary>
    /// Checks that one Crocomire melt failure, thrown as <typeparamref name="TException"/>, leaves effect mechanics, band
    /// pictures, tilemap, and VRAM intact.
    /// </summary>
    private static void CheckCrocomireResourceFailure<TException>(CrocomireMeltingArtwork? art,
        Action<CrocomireEffectFixture> operation, string context)
        where TException : Exception
    {
        var expected = new CrocomireEffectFixture(art);
        var actual = new CrocomireEffectFixture(art);
        AssertThrows<TException>(() => operation(actual), context + " fails loudly");
        AssertCrocomireEffectMechanics(expected, actual, context);
        AssertTrue(BossBodyCanvasFixtures.ShowSame(expected.Enemies.Presentation.RequireBossBody(), actual.Enemies.Presentation.RequireBossBody()),
            context + " shown body unchanged");
        AssertTrue(expected.Vram.ImageEquals(actual.Vram), context + " VRAM unchanged");
    }

    /// <summary>Accepts valid melt JSON casing and rejects unknown, repeated, or case-aliased fields.</summary>
    internal static void VerifyCrocomireMeltJson()
    {
        string valid = MeltEffectJson();
        string version = $"\"version\":{CrocomireMeltingArtworkFormat.TilemapVersion}";
        _ = MeltEffectArtwork(firstJson: valid);
        _ = MeltEffectArtwork(firstJson: valid.Replace("\"version\"", "\"Version\"", StringComparison.Ordinal));
        foreach ((string name, string malformed) in new[]
        {
            new VerifyCrocomireMeltJsonCase("unknown root field", valid.Replace(version, version + ",\"unknown\":0", StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("duplicate root field", valid.Replace(version, version + "," + version, StringComparison.Ordinal)),
            new VerifyCrocomireMeltJsonCase("case-aliased root field", valid.Replace(version, version + "," + version.Replace("version", "VERSION", StringComparison.Ordinal), StringComparison.Ordinal)),
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
