using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// The native bank-$B4 spritemap each room sprite-object picture draws, which only verification reads (#1817): an
/// enemy-sheet frame's is the bank's record of it, the empty frame's the bank's record that draws nothing, and a hit
/// burst's is the burst's own record.
/// </summary>
internal static class RoomEffectActorVisualDefinitionsConstants
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
    internal static ushort Spritemap(RoomEffectActorPicture picture)
    {
        if (picture.HitBurst is { } burst)
            return HitBurstSpritemaps[burst];
        return EnemyFrameCartridgeLookupTooling.WordIn(Bank, picture.EnemyFrame
            ?? throw new InvalidOperationException("The picture draws neither a hit burst nor an enemy frame."));
    }

    /// <summary>Every picture the room sprite objects' animations show, each once.</summary>
    internal static IReadOnlySet<RoomEffectActorPicture> Pictures { get; } =
        RoomEffectActorAnimations.Segments.Frames.ToHashSet();
}
