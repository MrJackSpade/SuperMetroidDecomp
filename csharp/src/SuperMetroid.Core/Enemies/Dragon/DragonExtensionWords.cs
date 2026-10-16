namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The Dragon words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class DragonExtensionWords
{
    /// <summary>animation lists requested by Dragon AI before the instruction owner installs them.</summary>
    internal ushort RequestedInstructionList { get; set; }

    /// <summary>animation lists currently installed in the common enemy interpreter.</summary>
    internal ushort InstalledInstructionList { get; set; }

    /// <summary>callback flags signaling completion of Dragon's attack animation.</summary>
    internal ushort AnimationFinishedFlag { get; set; }
}
