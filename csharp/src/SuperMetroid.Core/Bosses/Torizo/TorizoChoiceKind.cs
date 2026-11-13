namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which choice a <see cref="TorizoCue.Choice"/> cue is. A two-way branch or call between two body animations.</summary>
internal enum TorizoChoiceKind
{
    /// <summary>Continues at the target when Bomb Torizo's face is blown up, else at the alternate for Golden Torizo.</summary>
    GoToIfFaceBlownUpElseIfGolden,
    /// <summary>Calls one of Bomb Torizo's two attacks.</summary>
    CallBombTorizoAttack,
    /// <summary>Calls one of Golden Torizo's two attacks.</summary>
    CallGoldenTorizoAttack,
}
