namespace SuperMetroid.Core.Assets;

/// <summary>The member of <see cref="SpritePaletteRows"/> each row number stands for.</summary>
public static class SpritePaletteRowSet
{
    /// <summary>Every row in order, so a row's number is its position.</summary>
    public static IReadOnlyList<SpritePaletteRows> Rows { get; } =
    [
        SpritePaletteRows.Row0, SpritePaletteRows.Row1, SpritePaletteRows.Row2, SpritePaletteRows.Row3,
        SpritePaletteRows.Row4, SpritePaletteRows.Row5, SpritePaletteRows.Row6, SpritePaletteRows.Row7,
    ];
}
