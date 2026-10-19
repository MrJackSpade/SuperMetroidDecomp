using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The DeadSidehopper words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class DeadSidehopperExtensionWords
{
    /// <summary>The dead Sidehopper's typed animation (#641), started before its corpse state exists.</summary>
    internal AnimationProgramCursor<DeadTourianProgram> Program { get; set; }
}
