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

        // The cannon's terrain is the collision type and behaviour of the block words the cartridge writes.
        foreach ((DraygonCannonTerrain terrain, ushort word, string name) in new (DraygonCannonTerrain, ushort, string)[]
        {
            (DraygonCannonDefinitions.LiveCannon, 0xc044, "live cannon"),
            (DraygonCannonDefinitions.LiveCannonExtension, 0xd0ff, "live cannon extension"),
            (DraygonCannonDefinitions.DestroyedCannon, 0xa003, "destroyed cannon"),
        })
        {
            AssertEqual(new RoomLevelWord(word).CollisionType, terrain.Type, $"{name} collision type");
            AssertEqual(word & 0xff, terrain.Behavior.Value, $"{name} behaviour");
        }
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
