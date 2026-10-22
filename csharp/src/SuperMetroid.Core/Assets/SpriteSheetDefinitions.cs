namespace SuperMetroid.Core.Assets;

/// <summary>Each installed sprite sheet's file name.</summary>
public static class SpriteSheetDefinitions
{
    /// <summary>Every installed sprite sheet.</summary>
    public static IReadOnlyList<SpriteSheetId> Sheets { get; } = Enum.GetValues<SpriteSheetId>();

    /// <summary>The installed name of a sheet's image and manifest, without extension.</summary>
    public static string FileName(SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => "escape-timer",
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
