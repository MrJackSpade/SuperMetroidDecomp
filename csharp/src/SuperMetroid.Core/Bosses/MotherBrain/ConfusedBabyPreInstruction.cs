namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Pre-instructions of the intro's confused baby Metroid actor.</summary>
internal enum ConfusedBabyPreInstruction : ushort
{
    /// <summary><c>PreInstruction_CinematicSpriteObject_ConfusedBabyMetroid</c> at $8B:BA5E.</summary>
    WaitingForHatch = 0xba5e,

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Hatched</c> at $8B:BA73.</summary>
    Hatched = 0xba73,

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Idling</c> at $8B:BB0D.</summary>
    Idling = 0xbb0d,

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Dancing</c> at $8B:BB24.</summary>
    Dancing = 0xbb24,
}
