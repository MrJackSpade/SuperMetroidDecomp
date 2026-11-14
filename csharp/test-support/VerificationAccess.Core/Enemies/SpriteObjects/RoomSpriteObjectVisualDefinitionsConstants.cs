using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// The native bank-$B4 spritemap each room sprite-object picture draws, which only verification reads (#1817): an
/// enemy-sheet frame's is the record the extraction reads it from, and a hit burst's is the burst's own record.
/// </summary>
internal static class RoomSpriteObjectVisualDefinitionsConstants
{
    /// <summary>The bank of the room sprite objects' records.</summary>
    private const byte Bank = 0xb4;

    /// <summary>The bank-$B4 spritemap of each of Samus's hit-burst sprites the burst shows.</summary>
    private static readonly Dictionary<ProjectileSpriteId, ushort> HitBurstSpritemaps = new Dictionary<ProjectileSpriteId, ushort>
    {
        [HitBurstSpriteDefinitions.Tight] = 0xd857,
        [HitBurstSpriteDefinitions.Opening] = 0xd86d,
        [HitBurstSpriteDefinitions.Wide] = 0xd883,
        [HitBurstSpriteDefinitions.Scattered] = 0xd899,
    };

    /// <summary>The bank-$B4 spritemap <paramref name="picture"/> draws.</summary>
    /// <exception cref="InvalidOperationException">The picture's record is not one of the room sprite objects'.</exception>
    internal static ushort Spritemap(RoomSpriteObjectPicture picture)
    {
        if (picture.HitBurst is { } burst)
            return HitBurstSpritemaps[burst];
        SnesAddress location = EnemyFrameCartridgeLocationDefinitions.Locations[picture.RequireEnemyFrame()];
        return location.Bank == Bank
            ? location.Offset
            : throw new InvalidOperationException($"Room sprite-object picture {picture} is read from bank ${location.Bank:X2}, not ${Bank:X2}.");
    }

    /// <summary>Every picture the room sprite objects' animations show, each once.</summary>
    internal static IReadOnlySet<RoomSpriteObjectPicture> Pictures { get; } =
        RoomSpriteObjectAnimations.Segments.Frames.ToHashSet();
}
