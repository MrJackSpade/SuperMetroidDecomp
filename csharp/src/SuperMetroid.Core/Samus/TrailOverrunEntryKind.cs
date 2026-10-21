namespace SuperMetroid.Core.Samus;

/// <summary>What one trail overrun entry in the installed overread file records.</summary>
internal enum TrailOverrunEntryKind : byte
{
    /// <summary>The read reaches direct-page scratch and draws no trail.</summary>
    Scratch,

    /// <summary>The read finds a placement on the cartridge, stored after the kind as four signed offsets.</summary>
    Placement,
}
