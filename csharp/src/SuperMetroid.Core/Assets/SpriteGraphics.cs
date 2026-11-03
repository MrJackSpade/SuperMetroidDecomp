namespace SuperMetroid.Core.Assets;

/// <summary>
/// Where on a shared sprite sheet a sprite takes its characters, and how its picture is shaded (#1464, #1512): what an
/// enemy, or a projectile or sprite object it spawns, carries so its compositions draw from the right characters in the
/// right colours. The default value is the cleared graphics: character zero in the picture's standard colours.
/// </summary>
public readonly record struct SpriteGraphics
{
    /// <summary>The last sheet character a tile base can name.</summary>
    public const int MaximumTileBase = 511;

    /// <summary>The shading given, or null for the cleared graphics' standard colours.</summary>
    private readonly PictureShading? shading;

    /// <summary>A validated tile base and shading.</summary>
    /// <param name="tileBase">The sheet character its compositions' parts count from, 0 through <see cref="MaximumTileBase"/>.</param>
    /// <param name="shading">The shades and colour effects its pictures show.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tile base is outside the sheet.</exception>
    public SpriteGraphics(int tileBase, PictureShading shading)
    {
        TileBase = tileBase;
        this.shading = Ensure.NotNull(shading);
    }

    /// <summary>The sheet character its compositions' parts count from, 0 through <see cref="MaximumTileBase"/>.</summary>
    public int TileBase
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumTileBase, value);
    }

    /// <summary>The shades and colour effects its pictures show; the cleared graphics show their standard colours.</summary>
    public PictureShading Shading
    {
        get => shading ?? PictureShading.Standard;
        init => shading = Ensure.NotNull(value);
    }

    /// <summary>What an emptied slot holds: character zero in its standard colours.</summary>
    internal static SpriteGraphics Emptied => default;

    /// <summary>Character zero under <paramref name="shading"/>.</summary>
    internal static SpriteGraphics CharacterZeroIn(PictureShading shading) => new(0, shading);

    /// <summary>The same characters under <paramref name="shading"/>.</summary>
    internal SpriteGraphics In(PictureShading shading) => this with { Shading = shading };

    /// <summary>Whether both select the same characters and the same shading.</summary>
    public bool Equals(SpriteGraphics other) => TileBase == other.TileBase && Shading == other.Shading;

    /// <summary>Hashes the characters and shading <see cref="Equals(SpriteGraphics)"/> compares.</summary>
    public override int GetHashCode() => HashCode.Combine(TileBase, Shading);
}
