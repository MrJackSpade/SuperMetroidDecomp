namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the walking Space Pirates' programs does: one per bank-$B2 instruction its lists use.</summary>
internal enum WalkingSpacePirateCueKind
{
    /// <summary>Continues at the walk or laser animation Samus's side and height select (<c>Instruction_PirateWalking_ChooseAMovement</c>, $B2:FCC8).</summary>
    ChooseMovement,
}
