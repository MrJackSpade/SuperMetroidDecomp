namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's action choice, step and spit animations (#641), in authored order; see <see cref="FakeKraidAnimations"/>.
/// </summary>
internal enum FakeKraidSegment
{
    /// <summary>
    /// Fake Kraid chooses his next action (#2214): every step and spit ends here, and the choice always continues at the
    /// step or spit it picks.
    /// </summary>
    ChooseAction,
    /// <summary>Fake Kraid steps forwards, facing left.</summary>
    StepForwardsFacingLeft,
    /// <summary>Fake Kraid steps backwards, facing left.</summary>
    StepBackwardsFacingLeft,
    /// <summary>Fake Kraid fires his spit, facing left.</summary>
    FireSpitFacingLeft,
    /// <summary>Fake Kraid steps forwards, facing right.</summary>
    StepForwardsFacingRight,
    /// <summary>Fake Kraid steps backwards, facing right.</summary>
    StepBackwardsFacingRight,
    /// <summary>Fake Kraid fires his spit, facing right.</summary>
    FireSpitFacingRight,
}
