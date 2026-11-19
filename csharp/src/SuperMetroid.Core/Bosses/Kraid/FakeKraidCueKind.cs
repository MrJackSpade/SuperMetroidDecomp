namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What a signalling cue of Fake Kraid's animations does: choose its next step or spit, walk a step, or cry.</summary>
internal enum FakeKraidCueKind
{
    /// <summary>Continues at the step or spit animation his wait between spits and his facing select.</summary>
    ChooseAction,
    /// <summary>Walks one four-pixel step unless the walk's last step turns him round, then faces Samus.</summary>
    Move,
    /// <summary>Records the cry sound while Fake Kraid is on screen.</summary>
    PlayCrySound,
}
