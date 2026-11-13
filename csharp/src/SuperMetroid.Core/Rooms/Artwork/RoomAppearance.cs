namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// What gameplay has shown over a room's loaded artwork (#1466): whether Kraid's backdrop shows. Each room builds its
/// own, so nothing shown in one room shows in the next; with the frames its animations selected, the display names the
/// pictures of the room's blocks, background cells and written cells by it. A collectible shows its kind's picture where
/// it lies, which depends on nothing shown before it (#1538).
/// </summary>
public sealed class RoomAppearance
{
    /// <summary>Whether Kraid's rise has shown his backdrop over the room's tiles.</summary>
    public bool KraidBackdropShown { get; private set; }

    /// <summary>Shows Kraid's backdrop over the room's tiles, as his rise does (#1352).</summary>
    public void ShowKraidBackdrop() => KraidBackdropShown = true;
}
