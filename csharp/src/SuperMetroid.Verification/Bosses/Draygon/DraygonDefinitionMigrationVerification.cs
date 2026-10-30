using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Confirms #1413's Draygon migration: each catalog that left its cartridge encoding still produces, for one member of
/// each kind, what the deleted catalog did, and the import's sources and native words are the cartridge's.
/// </summary>
internal static class DraygonDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, sources and native words with the values the deleted catalogs held.</summary>
    internal static void VerifyDraygonDefinitionMigration()
    {
        AssertEqual(25, DraygonColorDefinitions.IntroCount, "intro colours");
        AssertEqual(4, DraygonColorDefinitions.HealthBandColorCount, "health band colours");
        AssertEqual(0xa5a217, DraygonSourceDefinitions.IntroColors, "intro source");
        AssertEqual(0xa596af, DraygonSourceDefinitions.HealthBandColors, "health band source");

        // The cannon's terrain is the collision kind and behaviour the cartridge writes for each cannon state.
        AssertEqual(RoomCollisionType.ShootableBlock, DraygonCannonDefinitions.LiveCannon.Type, "live cannon collision kind");
        AssertEqual(new RoomBlockBehavior(0x44), DraygonCannonDefinitions.LiveCannon.Behavior, "live cannon behaviour");
        AssertEqual(RoomCollisionType.VerticalExtension, DraygonCannonDefinitions.LiveCannonExtension.Type, "live cannon extension collision kind");
        AssertEqual(new RoomBlockBehavior(0xff), DraygonCannonDefinitions.LiveCannonExtension.Behavior, "live cannon extension behaviour");
        AssertEqual(RoomCollisionType.SpikeBlock, DraygonCannonDefinitions.DestroyedCannon.Type, "destroyed cannon collision kind");
        AssertEqual(new RoomBlockBehavior(3), DraygonCannonDefinitions.DestroyedCannon.Behavior, "destroyed cannon behaviour");
        AssertEqual(0x77, DraygonCannonDefinitions.SuperMissileHitCounterSeed, "Super Missile hit counter");
        AssertEqual(3, DraygonCannonDefinitions.HitsToDestroy, "hits to destroy");

        AssertEqual(0x804b, DraygonNativeWords.EyeFunctions.Word(DraygonEyeFunction.Inert), "inert eye routine");
        AssertEqual(0xc513, DraygonNativeWords.EyeFunctions.Word(DraygonEyeFunction.FacingRight), "right-facing eye routine");
        Console.WriteLine("Draygon definition migration: colours, cannon terrain and eye identities keep their values.");
    }

    /// <summary>Runs the focused Draygon definition-migration check.</summary>
    [VerificationCommand("--draygon-definition-migration")]
    internal static int DraygonDefinitionMigrationCommand()
    {
        VerifyDraygonDefinitionMigration();
        return 0;
    }
}
