namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How one of Draygon's body rectangles reacts when Samus touches it.</summary>
internal enum DraygonHitboxTouch
{
    /// <summary>The vulnerable eye rectangle: touching it does nothing.</summary>
    Harmless = 1,
    /// <summary>The claws, tail and shell: ordinary contact damage followed by Draygon's common reaction.</summary>
    Body,
}
