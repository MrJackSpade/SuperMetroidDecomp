using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>
/// Checks the terrain a Wave shot's scan past the room's ceiling finds halfway through a block (#1612) against the
/// level-word codec: the installed table, and the import's own decoding read back, give for every pair of blocks what
/// the original read of the word straddling their two stored words decodes to.
/// </summary>
internal static class StraddledTerrainVerification
{
    /// <summary>
    /// Pairs every level word, as the first block, with the word holding its two bytes exchanged, as the second, so the
    /// pairs reach every first block, every second block and every combination of the first's high byte with the
    /// second's low byte once. For each pair, the straddled terrain must be what the codec decodes from the word the
    /// original reads at the odd offset between them: the first word's high byte followed by the second word's low byte.
    /// The encoder must store each decoded first block back as its own word, so the pairs are the blocks the room holds.
    /// </summary>
    internal static void VerifyStraddledTerrain()
    {
        StraddledTerrain<RoomTerrainBlock> installed = InstalledFixtures.SamusOverreads.StraddledTerrain;
        StraddledTerrain<RoomTerrainBlock> decoded = StraddledTerrainFiles.Read(StraddledTerrainFiles.Decode());
        int pairs = 0;
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort firstWord = (ushort)value;
            ushort secondWord = (ushort)(((value & 0xFF) << 8) | (value >> 8));
            RoomTerrainBlock first = RoomLevelWordCodec.Decode(firstWord);
            RoomTerrainBlock second = RoomLevelWordCodec.Decode(secondWord);
            ushort encoded = RoomLevelWordEncoder.Encode(first);
            if (encoded != firstWord)
                AssertEqual(firstWord, encoded, $"The encoder stores {first} as the word it decodes from");
            // The original's unaligned read at the odd offset between two level words.
            RoomTerrainBlock expected = RoomLevelWordCodec.Decode((ushort)((firstWord >> 8) | ((secondWord & 0xFF) << 8)));
            RoomTerrainBlock fromInstallation = installed.Straddling(first, second);
            if (fromInstallation != expected)
                AssertEqual(expected, fromInstallation, $"Installed straddled terrain of {first} then {second}");
            RoomTerrainBlock fromImport = decoded.Straddling(first, second);
            if (fromImport != expected)
                AssertEqual(expected, fromImport, $"Imported straddled terrain of {first} then {second}");
            pairs++;
        }
        AssertEqual(ushort.MaxValue + 1, pairs, "Every level word is checked as a first block once");
        VerifyCollectibleBlocks(installed);
        Console.WriteLine($"PASS straddled terrain matches the level-word codec for {pairs} pairs of blocks.");
    }

    /// <summary>
    /// The blocks a collectible that shows its own picture writes under it, named by pair and look (#2354), straddle as
    /// the cartridge's numbered blocks they are stored as, whichever block they follow or lead: a read ending in the
    /// first pair's blocks collides as a solid block, one ending in the other three pairs' blocks as a door block.
    /// </summary>
    private static void VerifyCollectibleBlocks(StraddledTerrain<RoomTerrainBlock> installed)
    {
        foreach (CollectiblePairBlock block in CollectiblePairBlock.All)
        {
            RoomTerrainBlock written = CollectibleObjectDraws.BlockPairTerrain(
                CollectibleObjectDraws.VisibleFrame(InWorldCollectibleKind.Bombs, block.Look, block.Pair).Frame)
                ?? throw new InvalidOperationException($"The {block}'s frame writes no pair block.");
            AssertEqual(RoomBlockAppearance.Of(block), written.Appearance, $"The {block}'s frame writes the pair block");
            RoomTerrainBlock stored = RoomLevelWordCodec.Decode(RoomLevelWordEncoder.Encode(written));
            RoomCollisionType expectedCollision = block.Pair == CollectibleBlockPairs.Pair.Solid
                ? RoomCollisionType.SolidBlock : RoomCollisionType.DoorBlock;
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                RoomTerrainBlock other = RoomLevelWordCodec.Decode((ushort)value);
                RoomTerrainBlock ending = installed.Straddling(other, written);
                if (ending != installed.Straddling(other, stored))
                    AssertEqual(installed.Straddling(other, stored), ending, $"A read from {other} into the {block} straddles as its stored block");
                if (ending.Collision != expectedCollision)
                    AssertEqual(expectedCollision, ending.Collision, $"A read from {other} into the {block} collides as its pair's family");
                RoomTerrainBlock leading = installed.Straddling(written, other);
                if (leading != installed.Straddling(stored, other))
                    AssertEqual(installed.Straddling(stored, other), leading, $"A read from the {block} into {other} straddles as its stored block");
            }
        }
    }

    /// <summary>Runs the straddled-terrain check (#1612).</summary>
    [VerificationCommand("--straddled-terrain")]
    internal static int StraddledTerrainCommand()
    {
        Suite(nameof(VerifyStraddledTerrain), () => VerifyStraddledTerrain());
        return 0;
    }
}
