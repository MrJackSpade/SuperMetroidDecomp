namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One cue of Ridley's body programs, with the operands its native instruction carries.</summary>
internal abstract record RidleyBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private RidleyBodyCue()
    {
    }

    /// <summary>
    /// In Ceres, when Samus has under 30 energy, shortens the hover to eight updates and continues at the cue's target
    /// (<c>Instruction_Ridley_GotoYIfNotNorfairAndSamusHasLowEnergy</c>, $A6:E4D2).
    /// </summary>
    /// <param name="Target">The program a goto cue continues at.</param>
    internal sealed record GoToIfCeresAndSamusLowEnergy(RidleyBodyProgram Target) : RidleyBodyCue;

    /// <summary>Continues at the cue's target unless Ridley faces left (<c>Instruction_Ridley_GotoYIfNotFacingLeft</c>, $A6:E517).</summary>
    /// <param name="Target">The program a goto cue continues at.</param>
    internal sealed record GoToUnlessFacingLeft(RidleyBodyProgram Target) : RidleyBodyCue;

    /// <summary>Moves Ridley by the cue's displacement (<c>Instruction_Ridley_MoveRidleyWithArgsInY</c>, $A6:E51F).</summary>
    /// <param name="Displacement">How far a <see cref="Move"/> cue moves Ridley.</param>
    internal sealed record Move(Offset16<RoomPixels> Displacement) : RidleyBodyCue;

    /// <summary>Selects the feet-distance table index (<c>Instruction_RidleyCeres_RidleyFeetDistanceIndexInY</c>, $A6:E501).</summary>
    /// <param name="FeetDistanceIndex">The claw-height pose a <see cref="SetFeetDistance"/> cue selects: zero standing, one legs half extended, two legs extended.</param>
    internal sealed record SetFeetDistance(int FeetDistanceIndex) : RidleyBodyCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(RidleyBodyCueKind Kind) : RidleyBodyCue;
}
