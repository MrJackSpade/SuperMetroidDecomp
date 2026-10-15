namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Whether the native updater requested a 16-block column or row.</summary>
public enum BackgroundUpdateAxis
{
    /// <summary>Transfers one vertical strip when horizontal scrolling crosses a 16-pixel boundary.</summary>
    Column,

    /// <summary>Transfers one horizontal strip when vertical scrolling crosses a 16-pixel boundary.</summary>
    Row,
}
