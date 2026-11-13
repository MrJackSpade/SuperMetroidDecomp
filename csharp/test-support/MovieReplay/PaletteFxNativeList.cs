using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>One segment's native list (#1600): each beat's native address, and the control instructions between its beats.</summary>
/// <param name="BeatAddresses">The native address of each of the segment's beats, in order.</param>
/// <param name="Controls">The list's control instructions, in order.</param>
internal sealed record PaletteFxNativeList(ushort[] BeatAddresses, PaletteFxNativeControl[] Controls);

/// <summary>What a native control instruction of a colour effect's list does (#1600).</summary>
internal enum PaletteFxNativeControlKind
{
    /// <summary>Sets the object's timer, the plays the next list's counted run makes.</summary>
    SetCounter,

    /// <summary>Counts the timer down and, while it is not zero, replays its own list from the start.</summary>
    Repeat,

    /// <summary>Continues at another list.</summary>
    Goto,

    /// <summary>Runs on into the list authored next.</summary>
    RunOnIntoNext,

    /// <summary>Deletes the object.</summary>
    Delete,
}

/// <summary>One native control instruction of a colour effect's list (#1600).</summary>
/// <param name="Kind">What it does.</param>
/// <param name="AfterBeat">The number of the list's beats before it.</param>
/// <param name="Address">Its native address; for a run on into the next list, that list's entry.</param>
/// <param name="Target">The list it continues at or replays, where it names one.</param>
/// <param name="Count">The timer it sets, where it sets one.</param>
internal sealed record PaletteFxNativeControl(PaletteFxNativeControlKind Kind, int AfterBeat, ushort Address, PaletteFxSegment? Target, int? Count);

/// <summary>Where a live object resumes (#1600): the animation it plays, named by its first segment, and its place in it.</summary>
/// <param name="Animation">The first segment of the animation it plays.</param>
/// <param name="Place">Its place in that animation.</param>
internal readonly record struct PaletteFxImportedPlace(PaletteFxSegment Animation, SuperMetroid.Core.Animation.AnimationPlace Place);
