using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// #1513: the import, which alone knows where a graphics set's members land among the sprite characters, lays a set out
/// as the cartridge stages it: ordinary members follow one another from 64 tiles into the enemy tile area, a member whose
/// header says so stages at its fixed block, each member's character index counts every earlier member's tiles, and an
/// enemy that fixes or copies its index draws from the index it fixes or copies.
/// </summary>
internal static class GraphicsSetCharacterLayoutVerification
{
    /// <summary>The bytes of one enemy tile.</summary>
    private const int TileBytes = 32;

    /// <summary>Checks the layout of retail sets with an ordinary and a fixed-block member, and the rejected shapes.</summary>
    internal static void VerifyGraphicsSetCharacterLayout()
    {
        // Ceres Ridley's set: Ridley, then the Ceres door at its header's fixed block.
        GraphicsSetCharacterLayout ceres = GraphicsSetCharacterLayout.Of(RoomEnemyGraphicsSetDefinitions.CeresRidleyDefault);
        AssertEqual(2, ceres.Members.Count, "Ceres Ridley's set lays out both members");
        GraphicsSetMemberCharacters ridley = ceres.Members[0];
        GraphicsSetMemberCharacters door = ceres.Members[1];
        AssertEqual(EnemyDefinitionId.RidleyCeres, ridley.DefinitionId, "Ridley is the set's first member");
        AssertEqual(64 * TileBytes, ridley.StagingOffset, "an ordinary first member stages 64 tiles into the enemy tile area");
        AssertEqual(0, ridley.TileIndexBase, "the first member's pictures count from the set's first character");
        AssertEqual(EnemyTileSetDefinitions.ByteCounts[EnemyDefinitionId.RidleyCeres], ridley.TileByteCount, "Ridley stages his whole tile set");
        AssertEqual(EnemyTileSetDefinitions.FixedBlocks[EnemyDefinitionId.CeresDoor] * 512, door.StagingOffset,
            "the Ceres door stages at its fixed block");
        AssertEqual(ridley.TileByteCount / TileBytes, door.TileIndexBase, "the second member's index counts the first member's tiles");
        AssertEqual(0, ceres.TileBaseOf(EnemyDefinitionId.CeresDoor), "the Ceres door draws from the characters it fixes");
        AssertEqual(0, ceres.TileBaseOf(EnemyDefinitionId.Kraid), "an enemy the set does not load draws from the first character");

        // An enemy copying its owner's index draws where its owner's set member does.
        RoomEnemyGraphicsSetDefinition turtles = RoomEnemyGraphicsSetLists.DrawnUnder
            .First(set => set.Records.ToArray().Any(member => member.DefinitionId == EnemyDefinitionId.MamaTurtle));
        GraphicsSetCharacterLayout turtleLayout = GraphicsSetCharacterLayout.Of(turtles);
        AssertEqual(turtleLayout.TileBaseOf(EnemyDefinitionId.MamaTurtle), turtleLayout.TileBaseOf(EnemyDefinitionId.BabyTurtle),
            $"a Baby Turtle in {turtles.Name} draws from its Mama's characters");

        AssertThrows<InvalidDataException>(() => GraphicsSetCharacterLayout.Of(new RoomEnemyGraphicsSetDefinition("five members",
            [.. Enumerable.Repeat(new RoomEnemyGraphicsSetMember(EnemyDefinitionId.Beetom), 5)])),
            "a set of more than four members is rejected");
        AssertThrows<InvalidDataException>(() => GraphicsSetCharacterLayout.Of(new RoomEnemyGraphicsSetDefinition("after a fixed block",
            [new(EnemyDefinitionId.CeresDoor), new(EnemyDefinitionId.Beetom)])),
            "a member after one staged at a fixed block is rejected");
        Console.WriteLine("  Graphics-set layout: ordinary and fixed-block members land where the cartridge stages them.");
    }

    /// <summary>Runs the graphics-set character layout check.</summary>
    [VerificationCommand("--graphics-set-character-layout")]
    internal static int GraphicsSetCharacterLayoutCommand()
    {
        Suite(nameof(VerifyGraphicsSetCharacterLayout), VerifyGraphicsSetCharacterLayout);
        return 0;
    }
}
