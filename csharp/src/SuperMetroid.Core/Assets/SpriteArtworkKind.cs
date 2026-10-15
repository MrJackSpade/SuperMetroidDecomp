namespace SuperMetroid.Core.Assets;

/// <summary>Families of owned sprite artwork that whole-frame draw entries can bind.</summary>
/// <remarks>Append-only: numeric values appear in serialized display packets and debugger state.</remarks>
public enum SpriteArtworkKind
{
    /// <summary>The 25 escape-timer characters (digits, separators and the TIME label).</summary>
    EscapeTimerTiles,
}
