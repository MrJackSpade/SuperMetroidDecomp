namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The Skultera words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class SkulteraExtensionWords
{
    /// <summary>radius used to turn the sine phase into vertical bob distance.</summary>
    internal ushort Radius { get; set; }

    /// <summary>instruction flag consumed by main AI on the frame after a turn animation sleeps.</summary>
    internal ushort TurnFinishedFlag { get; set; }

    /// <summary>angle step, negated when a completed turn reverses the swim wave.</summary>
    internal ushort AngleDelta { get; set; }

    /// <summary>Previous sine Y offset used to move by the difference between adjacent phases.</summary>
    internal ushort PreviousYOffset { get; set; }

    /// <summary>Current sine Y offset retained after successful movement or vertical collision.</summary>
    internal ushort CurrentYOffset { get; set; }
}
