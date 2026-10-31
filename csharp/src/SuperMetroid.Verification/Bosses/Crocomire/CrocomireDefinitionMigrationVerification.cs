using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Confirms #1413's Crocomire migration: each catalog that left its cartridge encoding still produces, for one member of
/// each kind, what the deleted catalog did, and the import's sources and native words are the cartridge's.
/// </summary>
internal static class CrocomireDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, sources and native words with the values the deleted catalogs held.</summary>
    internal static void VerifyCrocomireDefinitionMigration()
    {
        AssertEqual(17, CrocomireColorDefinitions.InitialWallCount, "initial wall colours");
        AssertEqual(0xa4b89d, CrocomireSourceDefinitions.FightBodyColors, "fight body source");
        AssertEqual(0xa4b91d, CrocomireSourceDefinitions.WallSpikesColors, "wall spikes source");
        AssertEqual(0xa49c79, CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.First), "first melting tilemap source");
        AssertEqual(0xa49e7b, CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.Second), "second melting tilemap source");
        AssertEqual(0xada600, CrocomireSourceDefinitions.SkeletonChunk(0), "first skeleton chunk source");
        AssertEqual(0xada600 + 5 * 0x200, CrocomireSourceDefinitions.SkeletonChunk(5), "last skeleton chunk source");

        // The volley's gradients are the authored table and its final overread, for every selector the table covered.
        for (int parameter = 0; parameter < 20; parameter++)
        {
            short expected = (parameter >> 1) switch
            {
                0 or 3 or 6 => -16,
                1 or 4 or 7 => 0,
                2 or 5 or 8 => 32,
                _ => unchecked((short)0xb620),
            };
            AssertEqual(expected, CrocomireVolley.GradientForSpawnParameter(parameter), $"volley gradient for selector {parameter}");
        }
        AssertEqual(4, CrocomireProjectileDefinitions.VelocityMultiplier, "volley velocity factor");

        // The fight latches and the hit count pack into the cartridge's body variable B.
        AssertEqual(0x8003, CrocomireNativeWords.FightWord(CrocomireFightFlags.Engaged, 3), "engaged with three hits");
        AssertEqual(CrocomireFightFlags.ClawTouchedSamus | CrocomireFightFlags.DamageReactionPending,
            CrocomireNativeWords.FightFlags(0x4801), "latches of a recorded word");
        AssertEqual(1, CrocomireNativeWords.HitCount(0x4801), "hit count of a recorded word");
        AssertEqual(15, CrocomireMouthHits.Next(15), "the hit count saturates");

        AssertEqual(0xc65e, CrocomireNativeWords.TongueFrames.Word(CrocomireTongueFrame.Fight0), "first tongue frame");
        AssertEqual(0xcaf6, CrocomireNativeWords.TongueFrames.Word(CrocomireTongueFrame.MeltingPose4), "last tongue frame");
        AssertEqual(0xcbb3, CrocomireNativeWords.TongueHitboxLists.Word(CrocomireTongueHitboxList.Fight), "fight tongue hitbox list");

        // The last oscillation's timing is still the end-marker word the cartridge's table ends with.
        AssertEqual(0x8080, CrocomireRumbleDefinitions.All[^1].Cooldown, "rumble end marker cooldown");
        AssertEqual(0x8080, CrocomireRumbleDefinitions.All[^1].Delta, "rumble end marker delta");
        Console.WriteLine("Crocomire definition migration: colours, melting, skeleton, volley, latches and tongue identities keep their values.");
    }

    /// <summary>Runs the focused Crocomire definition-migration check.</summary>
    [VerificationCommand("--crocomire-definition-migration")]
    internal static int CrocomireDefinitionMigrationCommand()
    {
        VerifyCrocomireDefinitionMigration();
        return 0;
    }
}
