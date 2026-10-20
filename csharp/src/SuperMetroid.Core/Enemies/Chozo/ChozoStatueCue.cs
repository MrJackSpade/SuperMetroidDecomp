namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's programs, with the operands its native instruction carries.</summary>
internal abstract record ChozoStatueCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ChozoStatueCue()
    {
    }

    /// <summary>Clears spikes under the operand's footstep offset (<c>Instruction_Chozo_SpawnChozoSpikeClearingFootstepProjectile</c>, $AA:E58F).</summary>
    /// <param name="Offset">Where a <see cref="Footstep"/> cue probes for spikes, relative to the statue; only its X is meaningful.</param>
    internal sealed record Footstep(Offset16<RoomPixels> Offset) : ChozoStatueCue;

    /// <summary>Moves the statue by the operand's carry-motion entry and carries Samus (<c>Instruction_Chozo_Movement_IndexInY</c>, $AA:E5D8).</summary>
    /// <param name="CarryRecord">The carry-motion record, 0 through 31, a <see cref="Move"/> cue moves by.</param>
    internal sealed record Move(int CarryRecord) : ChozoStatueCue;

    /// <summary>Sets the pre-instruction main AI runs (<c>Instruction_CommonAA_Enemy0FB2_InY</c>, $AA:806B).</summary>
    /// <param name="PreInstruction">The cue's PreInstruction operand.</param>
    internal sealed record SetPreInstruction(ChozoStatuePreInstruction PreInstruction) : ChozoStatueCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(ChozoStatueCueKind Kind) : ChozoStatueCue;
}
