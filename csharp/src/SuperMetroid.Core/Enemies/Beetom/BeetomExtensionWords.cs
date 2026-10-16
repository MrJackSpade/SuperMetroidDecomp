namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class BeetomExtensionWords
{
    /// <summary>speed indices for the short hop's target height.</summary>
    internal ushort InitialShortLeapYSpeedIndex { get; set; }

    /// <summary>speed indices for the long hop's target height.</summary>
    internal ushort InitialLongLeapYSpeedIndex { get; set; }

    /// <summary>speed indices for the lunge arc.</summary>
    internal ushort InitialLungeYSpeedIndex { get; set; }

    /// <summary>Native rising/falling phase flags for active Beetom arcs.</summary>
    internal ushort FallingFlag { get; set; }

    /// <summary>Native flags indicating which Beetoms currently drain Samus.</summary>
    internal ushort AttachedFlag { get; set; }

    /// <summary>travel directions, also used for post-drain throws.</summary>
    internal ushort Direction { get; set; }

    /// <summary>offset captured when Beetom first attaches to Samus.</summary>
    internal Offset16<RoomPixels> InitialAttachmentOffset { get; set; }
}
