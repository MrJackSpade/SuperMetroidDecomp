namespace SuperMetroid.Core.Game;

/// <summary>Scroller distances used by vertical target selection at <c>$90:9666</c>.</summary>
public readonly record struct VerticalCameraContext(
    SamusVerticalDirection YDirection,
    ushort UpScroller,
    ushort DownScroller);
