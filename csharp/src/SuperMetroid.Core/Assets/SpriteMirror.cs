namespace SuperMetroid.Core.Assets;

/// <summary>How a sprite image is mirrored about its draw's origin; the two directions combine.</summary>
[Flags]
public enum SpriteMirror
{
    /// <summary>Drawn as stored.</summary>
    None = 0,

    /// <summary>Mirrored left to right.</summary>
    Horizontal = 1,

    /// <summary>Mirrored top to bottom.</summary>
    Vertical = 2,
}
