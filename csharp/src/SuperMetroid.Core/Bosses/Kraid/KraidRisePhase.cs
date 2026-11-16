namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's body runs while he rises through the floor (<see cref="KraidBodyStage.Rising"/>).</summary>
public enum KraidRisePhase
{
    /// <summary>
    /// Restricts Samus to the room's first screen while the body's phase timer runs, then shows his body
    /// (<see cref="ShowBody"/>).
    /// </summary>
    RestrictSamusToFirstScreen,
    /// <summary>Shows his body whole at the start of the rise.</summary>
    ShowBody,
    /// <summary>Starts the rise's quake and its first rock wait, and requests the boss music.</summary>
    StartQuakeAndMusic,
    /// <summary>Requests rise rocks on sixteen-frame timer boundaries during the first pre-rise wait.</summary>
    RocksEvery16Frames,
    /// <summary>Requests rise rocks on eight-frame timer boundaries during the faster pre-rise wait.</summary>
    RocksEvery8Frames,
    /// <summary>Raises Kraid at half a pixel per update with horizontal shaking until first-phase combat starts.</summary>
    RaiseBody,
}
