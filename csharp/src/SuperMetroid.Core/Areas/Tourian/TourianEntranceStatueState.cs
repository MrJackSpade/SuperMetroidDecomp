using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One Tourian entrance statue's own state (#1476). Its setup starts the animation every time, so nothing it holds is
/// inherited from an earlier occupant of its slot.
/// </summary>
/// <param name="program">Where the statue's animation starts.</param>
internal sealed class TourianEntranceStatueState(AnimationProgramCursor<TourianEntranceStatueProgram> program)
{
    /// <summary>Where the statue's typed animation resumes (#641).</summary>
    internal AnimationProgramCursor<TourianEntranceStatueProgram> Program { get; set; } = program;
}
