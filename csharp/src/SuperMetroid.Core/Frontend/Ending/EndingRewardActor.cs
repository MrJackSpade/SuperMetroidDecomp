namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ten reward gesture and jump actors (#1416: the values are arbitrary; the bank-$8B cinematic-object definition
/// words that named them are verification's <c>EndingRewardNativeWords</c>).
/// </summary>
internal enum EndingRewardActor
{
    /// <summary>The suitless upper hair-release actor the gesture spawns.</summary>
    SuitlessUpper = 1,
    /// <summary>The suitless lower hair-release actor the gesture spawns.</summary>
    SuitlessLower,
    /// <summary>The suitless jumping actor the jump spawns.</summary>
    JumpSuitlessBody,
    /// <summary>The suited gesture body.</summary>
    SuitedBody,
    /// <summary>The suited moving thumbs-up arm.</summary>
    SuitedArm,
    /// <summary>The helmeted gesture head.</summary>
    HelmetedHead,
    /// <summary>The helmetless gesture head.</summary>
    HelmetlessHead,
    /// <summary>The suited jump body the jump spawns.</summary>
    JumpSuitedBody,
    /// <summary>The helmeted jump head the jump spawns.</summary>
    JumpHelmetedHead,
    /// <summary>The helmetless jump head the jump spawns.</summary>
    JumpHelmetlessHead,
}
