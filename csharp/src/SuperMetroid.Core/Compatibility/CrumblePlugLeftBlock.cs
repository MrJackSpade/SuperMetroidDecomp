namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The picture a crumbling plug leaves on its block as its setup makes the block air (#1806). The setup never chooses
/// what the block shows, so the block shows what the plug's spawn last held: a plug spawned at a fixed block of the room,
/// as the Lower Norfair statue's hand spawns the room's plug, leaves block 416 mirrored both ways; a plug spawned at the
/// block an enemy found, as the walking statue's footstep spawns one, leaves block zero, unmirrored. Nothing pictures
/// the block: the plug draws its first breakup picture over it in the room-object pass later in the same update. Until
/// then it is gameplay-observable, because a Wave shot's scan past the room's ceiling reads its block number and
/// mirroring (<see cref="StraddledTerrain{TTerrain}"/>), whose straddled terrain takes its collision kind from them and
/// keeps the block it read as the block to restore.
/// </summary>
/// <remarks>
/// <para>
/// Consulted once per plug spawn, by the plug's setup as the statue's room object is spawned: during the enemy pass for
/// the hand's trigger, and as the footstep's spawn request is applied right after the enemy pass. A static rule with no
/// state.
/// </para>
/// <para>
/// Evidence: the plug's original setup stores a value it never loads as the block's level word, keeping only its picture
/// bits, so the block shows whatever the spawn routine stored last. The spawn at a fixed block stores the empty draw
/// program as the object's draw last, read as block 416 mirrored both ways; the spawn at the block an enemy found stores
/// zero last. Both spawn routines were read from the retail cartridge's room-object spawns.
/// </para>
/// </remarks>
[CompatibilityService("a crumbling plug's block shows what its spawn last held: block 416 mirrored both ways after a spawn at a fixed block, block zero after a spawn at a found block")]
public static class CrumblePlugLeftBlock
{
    /// <summary>How a crumbling plug was spawned, which decides the picture its setup leaves.</summary>
    public enum Spawn
    {
        /// <summary>Spawned at a fixed block of the room, as the Lower Norfair statue's hand spawns the room's plug.</summary>
        AtFixedBlock,

        /// <summary>Spawned at the block an enemy found, as the walking statue's footstep spawns one.</summary>
        AtFoundBlock,
    }

    /// <summary>How the block a plug leaves is mirrored; only these two occur.</summary>
    public enum Mirroring
    {
        /// <summary>Shown as drawn.</summary>
        None,

        /// <summary>Reflected left to right and top to bottom.</summary>
        BothWays,
    }

    /// <summary>The picture a plug's block is left showing: a block of the room's block set and how it is mirrored.</summary>
    /// <param name="Number">The block shown, in the room's block set.</param>
    /// <param name="Mirrored">How the block is mirrored.</param>
    public readonly record struct Picture(int Number, Mirroring Mirrored);

    /// <summary>The block the empty draw program reads as, which a spawn at a fixed block leaves.</summary>
    private const int FixedBlockSpawnLeftNumber = 416;

    /// <summary>The picture a plug spawned by <paramref name="spawn"/> leaves on its block.</summary>
    /// <param name="spawn">How the plug was spawned.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="spawn"/> names no spawn.</exception>
    public static Picture LeftBy(Spawn spawn) => spawn switch
    {
        Spawn.AtFixedBlock => new(FixedBlockSpawnLeftNumber, Mirroring.BothWays),
        Spawn.AtFoundBlock => new(0, Mirroring.None),
        _ => throw new ArgumentOutOfRangeException(nameof(spawn), spawn, "Not a crumbling plug's spawn."),
    };
}
