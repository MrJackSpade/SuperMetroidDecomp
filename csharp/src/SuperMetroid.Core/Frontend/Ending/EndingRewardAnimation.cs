namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The animations of the post-credits reward Samus's gesture and jump (#2197).</summary>
internal enum EndingRewardAnimation
{
    /// <summary>The suitless Samus letting her hair down, then handing off to her jump.</summary>
    HairRelease,
    /// <summary>The suitless Samus's legs while she lets her hair down.</summary>
    HairReleaseLegs,
    /// <summary>The suited body during the gesture.</summary>
    GestureBody,
    /// <summary>The suited arm's gesture, which hands off to the jump.</summary>
    GestureArm,
    /// <summary>The helmet during the gesture.</summary>
    GestureHelmet,
    /// <summary>The helmetless head during the gesture.</summary>
    GestureHead,
    /// <summary>The suitless Samus jumping, then in flight.</summary>
    SuitlessJump,
    /// <summary>The suited Samus jumping, then in flight.</summary>
    SuitedJump,
    /// <summary>The helmet's jump, then its flight.</summary>
    HelmetJump,
    /// <summary>The helmetless head's jump, then its flight.</summary>
    HeadJump,
    /// <summary>Samus falling.</summary>
    Falling,
    /// <summary>Samus landing and shooting.</summary>
    Landing,
}
