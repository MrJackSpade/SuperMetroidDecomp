namespace SuperMetroid.Core.Assets;

/// <summary>A set of the eight OBJ palette rows: the rows a pre-coloured frame's pixels were coloured from.</summary>
[Flags]
public enum SpritePaletteRows : byte
{
    /// <summary>No row.</summary>
    None = 0,

    /// <summary>Row 0.</summary>
    Row0 = 1 << 0,

    /// <summary>Row 1.</summary>
    Row1 = 1 << 1,

    /// <summary>Row 2.</summary>
    Row2 = 1 << 2,

    /// <summary>Row 3.</summary>
    Row3 = 1 << 3,

    /// <summary>Row 4.</summary>
    Row4 = 1 << 4,

    /// <summary>Row 5.</summary>
    Row5 = 1 << 5,

    /// <summary>Row 6.</summary>
    Row6 = 1 << 6,

    /// <summary>Row 7.</summary>
    Row7 = 1 << 7,
}
