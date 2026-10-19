using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The TourianEntranceStatue words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class TourianEntranceStatueExtensionWords
{
    /// <summary>The statue record's typed animation (#641); every statue deletes its record at once.</summary>
    internal AnimationProgramCursor<TourianEntranceStatueProgram>? Program { get; set; }
}
