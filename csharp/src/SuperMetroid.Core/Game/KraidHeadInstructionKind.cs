namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive command forms used by Kraid's private head interpreter.</summary>
internal enum KraidHeadInstructionKind
{
    /// <summary>Timed pose selecting a tilemap and mouth collision shapes.</summary>
    Frame,
    /// <summary>Normal roar sound command inserted before a fully opened mouth.</summary>
    RoarSound,
    /// <summary>Dying-roar sound command inserted in the slower opening sequence.</summary>
    DyingSound,
    /// <summary>End cursor with no displayed frame.</summary>
    Terminate,
}
