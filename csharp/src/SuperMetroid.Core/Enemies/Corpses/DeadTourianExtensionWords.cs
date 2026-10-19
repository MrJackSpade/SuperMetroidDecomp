using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The DeadTourian words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class DeadTourianExtensionWords
{
    /// <summary>The dead Zoomer, Ripper or Skree's typed corpse pose (#641).</summary>
    internal AnimationProgramCursor<DeadTourianProgram> Program { get; set; }
}
