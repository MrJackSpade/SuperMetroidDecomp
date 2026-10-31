using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The animal-rescue wall of the Zebes escape: where it stands and the blocks its setup and reaction write.</summary>
public static class EscapeAnimalWallDefinitions
{
    /// <summary>The block column the room's setup spawns the rescue wall at.</summary>
    public const byte WallX = 15;

    /// <summary>The block row the room's setup spawns the rescue wall at.</summary>
    public const byte WallY = 10;

    /// <summary>The block behaviour of the rescue wall's shootable origin block, which selects the critters-escape reaction.</summary>
    public static readonly RoomBlockBehavior ReactionBehavior = new(79);

    /// <summary>The collision type the setup gives the wall's origin block, keeping its artwork.</summary>
    public const RoomCollisionType OriginCollisionType = RoomCollisionType.ShootableBlock;

    /// <summary>The collision type of the two blocks below the origin, which extend it.</summary>
    public const RoomCollisionType ExtensionCollisionType = RoomCollisionType.VerticalExtension;

    /// <summary>The extension blocks' behaviour: a displacement of one block up, onto the block above.</summary>
    public static readonly RoomBlockBehavior ExtensionBehavior = RoomBlockBehavior.OfExtensionOffset(-1);

    /// <summary>The block the reaction shows in place of the wall's artwork, unmirrored.</summary>
    public static readonly RoomBlockAppearance ReactionAppearance = RoomBlockAppearance.Of(159);
}
