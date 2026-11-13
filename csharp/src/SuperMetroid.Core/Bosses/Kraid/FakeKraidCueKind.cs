namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What a cue of Mini-Kraid's animations does: choose its next step or spit, walk a step, cry, or spit a pair of projectiles left or right.</summary>
internal enum FakeKraidCueKind
{
    /// <summary>Continues at the step or spit animation the decision timer and facing select.</summary>
    ChooseAction,
    /// <summary>Walks one four-pixel step unless the reversal clock expires, then faces Samus.</summary>
    Move,
    /// <summary>Records the cry sound while Mini-Kraid is on screen.</summary>
    PlayCrySound,
    /// <summary>Spits a pair of projectiles to the left.</summary>
    SpitLeft,
    /// <summary>Spits a pair of projectiles to the right.</summary>
    SpitRight,
}
