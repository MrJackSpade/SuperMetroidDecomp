namespace SuperMetroid.Core.Assets;

/// <summary>The two native escape-timer character uploads.</summary>
public enum EscapeTimerTilePage
{
    /// <summary>Characters 0..15 from $B0:C000 to VRAM word $7E00.</summary>
    First,
    /// <summary>Characters 16..24 from $B0:C200 to VRAM word $7F00.</summary>
    Second,
}
