using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Where on a shared sprite sheet a sprite takes its characters, and what colours it draws in (#1464): what an enemy, or
/// a projectile or sprite object it spawns, carries so its compositions draw from the right characters and colours.
/// The default value is the cleared graphics word: character zero in the common sprite colours.
/// </summary>
public readonly record struct SpriteGraphics
{
    /// <summary>The last sheet character a tile base can name.</summary>
    public const int MaximumTileBase = 511;

    /// <summary>The colours given, or null for the cleared word's common colours.</summary>
    private readonly SpriteColouring? colouring;

    /// <summary>A validated tile base and colouring.</summary>
    /// <param name="tileBase">The sheet character its compositions' parts count from, 0 through <see cref="MaximumTileBase"/>.</param>
    /// <param name="colouring">What colours it draws in.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tile base is outside the sheet.</exception>
    public SpriteGraphics(int tileBase, SpriteColouring colouring)
    {
        TileBase = tileBase;
        this.colouring = Ensure.NotNull(colouring);
    }

    /// <summary>The sheet character its compositions' parts count from, 0 through <see cref="MaximumTileBase"/>.</summary>
    public int TileBase
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumTileBase, value);
    }

    /// <summary>What colours it draws in; the cleared word draws in the common sprite colours.</summary>
    public SpriteColouring Colouring
    {
        get => colouring ?? SharedSpriteColouring.Common;
        init => colouring = Ensure.NotNull(value);
    }

    /// <summary>What an emptied slot holds: character zero in the common sprite colours, as its cleared word selects.</summary>
    internal static SpriteGraphics Emptied => default;

    /// <summary>Character zero in <paramref name="colouring"/>.</summary>
    internal static SpriteGraphics CharacterZeroIn(SpriteColouring colouring) => new(0, colouring);

    /// <summary>The same characters drawn in <paramref name="colouring"/>.</summary>
    internal SpriteGraphics In(SpriteColouring colouring) => this with { Colouring = colouring };

    /// <summary>Whether both select the same characters and the same colours.</summary>
    public bool Equals(SpriteGraphics other) => TileBase == other.TileBase && Colouring == other.Colouring;

    /// <summary>Hashes the characters and colours <see cref="Equals(SpriteGraphics)"/> compares.</summary>
    public override int GetHashCode() => HashCode.Combine(TileBase, Colouring);
}
