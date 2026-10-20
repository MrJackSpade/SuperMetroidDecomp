namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which choice a <see cref="TorizoCue.Choice"/> cue is. A two-way branch or call between two body programs.</summary>
internal enum TorizoChoiceKind
{
    /// <summary>Continues at the target when Bomb Torizo's face is blown up, else at the alternate for Golden Torizo (<c>Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden</c>, $AA:C2D9).</summary>
    GoToIfFaceBlownUpElseIfGolden,
    /// <summary>Calls one of Bomb Torizo's two attacks (<c>Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack</c>, $AA:C5A4).</summary>
    CallBombTorizoAttack,
    /// <summary>Calls one of Golden Torizo's two attacks (<c>Instruction_GoldenTorizo_CallY_OrY2_ForAttack</c>, $AA:D526).</summary>
    CallGoldenTorizoAttack,
}
