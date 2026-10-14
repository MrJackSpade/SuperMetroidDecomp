namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B pre-instructions owned by the reward jump; the actors otherwise run the shared no-op or none.</summary>
internal enum EndingRewardJumpPreInstruction : ushort
{
    /// <summary>$8B:F528, body flight pre-instruction, switches to the falling sheet above Y=-80.</summary>
    BodyFlight = 0xf528,
    /// <summary>$8B:F57F, head flight pre-instruction, deletes the old head above Y=-80.</summary>
    HeadFlight = 0xf57f,
    /// <summary>$8B:F5DD, returning body pre-instruction, queues graphics and lands at Y=136.</summary>
    Landing = 0xf5dd,
}
