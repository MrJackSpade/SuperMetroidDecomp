namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The programs of a Tourian boss statue's animated tiles, in authored order. Each statue runs the same four programs
/// with its own operands; see <see cref="TourianStatueTileAnimations"/>.
/// </summary>
internal enum TourianStatueTileProgram
{
    /// <summary>The spawn: marks the statue animating, skips to <see cref="AlreadyGrey"/> once its grey event exists, and shows the first idle frame.</summary>
    Entry,

    /// <summary>The idle swing, which branches to <see cref="Release"/> once the statue's boss is dead.</summary>
    Oscillate,

    /// <summary>The release: waits its turn, blacks out three colours, glows the eye, frees the soul and sets the grey event.</summary>
    Release,

    /// <summary>A statue released on an earlier visit: clears its state, installs its grey colours and stops.</summary>
    AlreadyGrey,
}
