using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How a block that grows back shows its picture again (#2557): a single block restores its saved terrain, and a larger
/// one draws its family's restored picture of its arrangement for one update.
/// </summary>
internal sealed class BlockRestore
{
    /// <summary>The family's restored picture of a larger arrangement.</summary>
    private readonly Func<BlockArrangement, RoomObjectDraw> larger;

    /// <summary>A restore drawing <paramref name="larger"/> for an arrangement of more than one block.</summary>
    private BlockRestore(Func<BlockArrangement, RoomObjectDraw> larger) => this.larger = larger;

    /// <summary>A shot block's restore: the shot-block family's restored picture.</summary>
    internal static BlockRestore ShotBlock { get; } = new(arrangement => ShotBlockObjectDraws.Restoration(arrangement));

    /// <summary>A crumble block's restore: the contact crumble's restored picture.</summary>
    internal static BlockRestore ContactCrumble { get; } =
        new(arrangement => LinkedRestoreObjectFamily.ContactCrumbleDraws.Of(new ContactCrumbleRestoreDraw(arrangement)));

    /// <summary>A bomb block's restore: the bomb block's restored picture.</summary>
    internal static BlockRestore BombBlock { get; } =
        new(arrangement => LinkedRestoreObjectFamily.BombBlockDraws.Of(new BombBlockRestoreDraw(arrangement)));

    /// <summary>The image that shows a block of <paramref name="arrangement"/> again.</summary>
    /// <param name="arrangement">The blocks the reaction broke.</param>
    internal BlockReactionFrame Of(BlockArrangement arrangement) =>
        arrangement == BlockArrangement.Single ? BlockReactionFrame.RestoredBlock : new(larger(arrangement), 1);
}
