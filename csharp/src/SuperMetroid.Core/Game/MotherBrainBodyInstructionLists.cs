namespace SuperMetroid.Core.Game;

/// <summary>Retail bank-$A9 body posture programs, distinct from static pose frames.</summary>
public static class MotherBrainBodyInstructionLists
{
    /// <summary>$A9:9A0A, InstList_MotherBrainBody_Crouch_Slow: lowers the body 38 pixels before publishing the crouched pose.</summary>
    public const ushort CrouchSlow = 0x9a0a;
    /// <summary>$A9:98C6, InstList_MotherBrainBody_WalkingBackwards_Fast; the complete balanced backward gait.</summary>
    public const ushort WalkBackwardsFast = 0x98c6;
    /// <summary>$A9:97A4, InstList_MotherBrainBody_WalkingForwards_Medium; the complete forward gait.</summary>
    public const ushort WalkForwardsMedium = 0x97a4;
    /// <summary>$A9:C6F3 masks the shared random word to twelve bits before the forward-walk gate.</summary>
    public const ushort ForwardWalkRandomMask = 0x0fff;
    /// <summary>$A9:C6F6 admits the top 64 values of the masked random word for the low-X forward-walk branch.</summary>
    public const ushort ForwardWalkRandomThreshold = 0x0fc0;
}
