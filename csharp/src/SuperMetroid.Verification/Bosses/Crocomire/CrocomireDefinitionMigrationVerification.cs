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
        AssertEqual(0xa49c79, CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.First), "first melting tilemap source");
        AssertEqual(0xa49e7b, CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.Second), "second melting tilemap source");
        AssertEqual(0xada600, CrocomireSourceDefinitions.SkeletonChunk(0), "first skeleton chunk source");
        AssertEqual(0xada600 + 5 * 0x200, CrocomireSourceDefinitions.SkeletonChunk(5), "last skeleton chunk source");

        // The volley's authored gradients, for every shot before the last.
        for (int shot = 1; shot < CrocomireProjectileDefinitions.VolleyShotCount; shot++)
        {
            int expected = shot switch
            {
                3 or 6 => -16,
                1 or 4 or 7 => 0,
                _ => 32,
            };
            AssertEqual(expected, CrocomireVolley.AuthoredGradient(shot), $"volley gradient for shot {shot}");
        }
        AssertEqual(4, CrocomireProjectileDefinitions.VelocityMultiplier, "volley velocity factor");

        // The damage-reaction latch is a bit of the cartridge's body variable B, whose mouth-hit count nothing in the fight
        // reads, nor the claw-touch latch only the unreachable claw attack reads, nor the engagement mark nothing reads.
        AssertTrue(CrocomireNativeWords.DamageReactionPending(0x4801), "latch of a recorded word");
        AssertTrue(!CrocomireNativeWords.DamageReactionPending(0x8000), "the engagement mark is no latch");

        AssertEqual(0xc65e, CrocomireNativeWords.TongueFrames.Word(CrocomireTongueFrame.Fight0), "first tongue frame");
        AssertEqual(0xcaf6, CrocomireNativeWords.TongueFrames.Word(CrocomireTongueFrame.MeltingPose4), "last tongue frame");
        AssertEqual(0xcbb3, CrocomireNativeWords.TongueHitboxLists.Word(CrocomireTongueHitboxList.Fight), "fight tongue hitbox list");

        // Since #1476 the rumble loads no end marker: its last negative target ends it, so like a return target it
        // carries no next phase.
        AssertEqual(0, CrocomireRumbleDefinitions.All[^1].Cooldown, "the rumble's last target loads no next cooldown");
        AssertEqual(0, CrocomireRumbleDefinitions.All[^1].Delta, "the rumble's last target loads no next delta");
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
