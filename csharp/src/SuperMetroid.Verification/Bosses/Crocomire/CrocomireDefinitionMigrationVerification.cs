using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire;

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
        AssertEqual(0xa49c79, CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.Flesh), "first melting tilemap source");
        AssertEqual(0xa49e7b, CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.BareSkull), "second melting tilemap source");
        AssertEqual(0xada600, CrocomireSourceDefinitions.SkeletonChunk(0), "first skeleton chunk source");
        AssertEqual(0xada600 + 5 * 0x200, CrocomireSourceDefinitions.SkeletonChunk(5), "last skeleton chunk source");

        // The volley's authored gradients, for every shot before the last, in firing order.
        AssertTrue(CrocomireProjectileDefinitions.AuthoredGradients.SequenceEqual([0, 32, -16, 0, 32, -16, 0, 32]), "volley gradients in firing order");
        AssertEqual(4, CrocomireProjectileDefinitions.VolleySpeed, "volley speed");

        // The damage-reaction latch is a bit of the cartridge's body variable B, whose mouth-hit count nothing in the fight
        // reads, nor the claw-touch latch only the unreachable claw attack reads, nor the engagement mark nothing reads.
        AssertTrue(CrocomireNativeWords.DamageReactionPending(0x4801), "latch of a recorded word");
        AssertTrue(!CrocomireNativeWords.DamageReactionPending(0x8000), "the engagement mark is no latch");

        AssertEqual(0xc65e, CrocomireNativeWords.TongueFrames.Word(EnemyFrameId.CrocomireTongueCurlingDown), "first tongue frame");
        AssertEqual(0xcaf6, CrocomireNativeWords.TongueFrames.Word(EnemyFrameId.CrocomireArmAloneReturning), "last tongue frame");

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
