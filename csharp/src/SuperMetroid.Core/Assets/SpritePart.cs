namespace SuperMetroid.Core.Assets;

/// <summary>
/// One authored piece of a sprite: a small or large square of characters from the sprite's character sheet, placed
/// relative to the sprite's origin. It becomes a <see cref="SpriteFramePart"/> once the artwork it reads and the
/// screen's sprite sizes are known.
/// </summary>
/// <param name="Offset">Top-left corner relative to the sprite origin: X from <see cref="MinimumX"/> through
/// <see cref="MaximumX"/> and Y from <see cref="MinimumY"/> through <see cref="MaximumY"/> pixels.</param>
/// <param name="Large">Whether the piece draws at its screen's large sprite size instead of the small one.</param>
/// <param name="Character">The piece's first character on the sheet, 0 through <see cref="MaximumCharacter"/>; a
/// larger piece reads the sixteen-wide grid from it.</param>
/// <param name="Flip">The piece's own mirroring.</param>
/// <param name="Palette">The piece's palette row, 0 through <see cref="MaximumPalette"/>.</param>
/// <param name="Priority">The piece's priority against the backgrounds, 0 through <see cref="MaximumPriority"/>.</param>
public readonly record struct SpritePart(
    Offset16<ScreenPixels> Offset,
    bool Large,
    int Character,
    TileFlip Flip,
    int Palette,
    int Priority)
{
    /// <summary>The furthest left a piece may start, in pixels from the sprite origin.</summary>
    public const int MinimumX = -256;
    /// <summary>The furthest right a piece may start, in pixels from the sprite origin.</summary>
    public const int MaximumX = 255;
    /// <summary>The furthest up a piece may start, in pixels from the sprite origin.</summary>
    public const int MinimumY = -128;
    /// <summary>The furthest down a piece may start, in pixels from the sprite origin.</summary>
    public const int MaximumY = 127;
    /// <summary>The last character a sprite's character sheet holds.</summary>
    public const int MaximumCharacter = 511;
    /// <summary>The last of the eight sprite palette rows.</summary>
    public const int MaximumPalette = 7;
    /// <summary>The highest of the four priorities against the backgrounds.</summary>
    public const int MaximumPriority = 3;
    /// <summary>The width of the horizontal plane sprites are placed on: the screen and the hidden band beside it.</summary>
    public const int PlaneWidth = MaximumX - MinimumX + 1;

    /// <summary>A validated piece.</summary>
    /// <param name="offset">Top-left corner relative to the sprite origin: X from <see cref="MinimumX"/> through
    /// <see cref="MaximumX"/> and Y from <see cref="MinimumY"/> through <see cref="MaximumY"/> pixels.</param>
    /// <param name="large">Whether the piece draws at the screen's large sprite size.</param>
    /// <param name="character">First character on the sheet, 0 through <see cref="MaximumCharacter"/>.</param>
    /// <param name="palette">Palette row, 0 through <see cref="MaximumPalette"/>.</param>
    /// <param name="priority">Priority against the backgrounds, 0 through <see cref="MaximumPriority"/>.</param>
    /// <param name="flip">The piece's own mirroring.</param>
    /// <exception cref="ArgumentOutOfRangeException">A field is outside its range.</exception>
    public static SpritePart Create(Offset16<ScreenPixels> offset, bool large, int character, int palette, int priority,
        TileFlip flip = TileFlip.None)
    {
        Ensure.BetweenInclusive(MinimumX, MaximumX, offset.X);
        Ensure.BetweenInclusive(MinimumY, MaximumY, offset.Y);
        Ensure.BetweenInclusive(0, MaximumCharacter, character);
        Ensure.BetweenInclusive(0, MaximumPalette, palette);
        Ensure.BetweenInclusive(0, MaximumPriority, priority);
        if ((flip & ~(TileFlip.Horizontal | TileFlip.Vertical)) != TileFlip.None)
            throw new ArgumentOutOfRangeException(nameof(flip));
        return new(offset, large, character, flip, palette, priority);
    }

    /// <summary>A single small piece at the sprite origin.</summary>
    /// <param name="character">The piece's character on the sheet, 0 through <see cref="MaximumCharacter"/>.</param>
    /// <param name="palette">Palette row, 0 through <see cref="MaximumPalette"/>.</param>
    /// <param name="priority">Priority against the backgrounds, 0 through <see cref="MaximumPriority"/>.</param>
    /// <param name="flip">The piece's own mirroring.</param>
    /// <exception cref="ArgumentOutOfRangeException">A field is outside its range.</exception>
    public static SpritePart Single(int character, int palette, int priority, TileFlip flip = TileFlip.None) =>
        Create(Offset16<ScreenPixels>.Zero, false, character, palette, priority, flip);

    /// <summary>Whether the piece's pixels are drawn in reverse horizontal order.</summary>
    public bool FlipsHorizontally => (Flip & TileFlip.Horizontal) != TileFlip.None;

    /// <summary>Whether the piece's rows are drawn in reverse vertical order.</summary>
    public bool FlipsVertically => (Flip & TileFlip.Vertical) != TileFlip.None;

    /// <summary>
    /// <paramref name="x"/> wrapped into the horizontal range a piece may start at: a piece placed past either end of
    /// that range shows as far in from the other end, as the reflected Ceres stars place their mirror images.
    /// </summary>
    public static int WrapX(int x) => ((x - MinimumX) % PlaneWidth + PlaneWidth) % PlaneWidth + MinimumX;

    /// <summary>The piece moved to start at <paramref name="offset"/>, validated as <see cref="Create"/> validates.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The new corner is outside the range a piece may start at.</exception>
    public SpritePart At(Offset16<ScreenPixels> offset) => Create(offset, Large, Character, Palette, Priority, Flip);

    /// <summary>The piece drawn in palette row <paramref name="palette"/> instead of its own.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside 0 through <see cref="MaximumPalette"/>.</exception>
    public SpritePart InPalette(int palette) => this with { Palette = Ensure.BetweenInclusive(0, MaximumPalette, palette) };

    /// <summary>
    /// The piece as a whole-frame part over artwork whose character 0 is sheet character <paramref name="firstCharacter"/>,
    /// on a screen whose sprites draw at <paramref name="sizes"/>.
    /// </summary>
    /// <param name="firstCharacter">The sheet character the bound artwork starts at.</param>
    /// <param name="sizes">The screen's small and large sprite sizes.</param>
    /// <param name="inheritPalette">Whether the part takes the draw's palette instead of its own.</param>
    public SpriteFramePart ToFramePart(int firstCharacter, ScreenObjectSizes sizes, bool inheritPalette = false) =>
        new(Offset, Large ? sizes.Large : sizes.Small, Character - firstCharacter, Flip,
            inheritPalette ? null : Palette, Priority);

    /// <summary>Hashes the piece's visual fields in a fixed order.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.Append("x", Offset.X);
        content.Append("y", Offset.Y);
        content.Append("large", Large ? 1 : 0);
        content.Append("character", Character);
        content.Append("flip-horizontal", FlipsHorizontally ? 1 : 0);
        content.Append("flip-vertical", FlipsVertically ? 1 : 0);
        content.Append("palette", Palette);
        content.Append("priority", Priority);
    }
}
