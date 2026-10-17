namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom values kept per physical slot for the room rather than per occupant: values an initializer does not
/// write keep the previous Beetom's until room load. A fresh Beetom touched before it first chooses an action
/// therefore attaches facing the way the slot's previous Beetom last faced.
/// </summary>
internal sealed class BeetomExtensionWords
{
    /// <summary>The short hop's starting quadratic-speed record index.</summary>
    internal int InitialShortLeapYSpeedIndex { get; set; }

    /// <summary>The long hop's starting quadratic-speed record index.</summary>
    internal int InitialLongLeapYSpeedIndex { get; set; }

    /// <summary>The lunge's starting quadratic-speed record index.</summary>
    internal int InitialLungeYSpeedIndex { get; set; }

    /// <summary>Whether the current arc has passed its apex.</summary>
    internal bool Falling { get; set; }

    /// <summary>Whether the Beetom is attached to Samus.</summary>
    internal bool AttachedToSamus { get; set; }

    /// <summary>Whether the Beetom faces right rather than left.</summary>
    internal bool FacingRight { get; set; }

    /// <summary>The Samus-minus-Beetom offset captured when the Beetom first attaches.</summary>
    internal Offset16<RoomPixels> InitialAttachmentOffset { get; set; }
}
