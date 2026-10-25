using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The Ripper words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class RipperExtensionWords
{
    /// <summary>GRipper's population-defined left patrol endpoint.</summary>
    internal int MinimumXPosition { get; set; }

    /// <summary>GRipper's population-defined right patrol endpoint.</summary>
    internal int MaximumXPosition { get; set; }

    /// <summary>The Ripper-family typed flying loop (#641).</summary>
    internal AnimationProgramCursor<RipperProgram> Program { get; set; }
}
