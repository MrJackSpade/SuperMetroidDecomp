namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The poses a Chozo statue carries Samus through: taking her into its hand, holding her, and the four steps of the
/// stride it walks with her. Each pose has its own statue motion and its own hold on Samus.
/// </summary>
public enum ChozoCarryPose
{
    /// <summary>The open hand Samus is placed in: the statue stands still, holding her 32 pixels up and 28 to its side.</summary>
    Reaching,

    /// <summary>The hand closing on Samus: the statue stands still, holding her 25 pixels up and 30 to its side.</summary>
    Closing,

    /// <summary>The hand gripping Samus: the statue stands still, holding her 23 pixels up and 32 to its side.</summary>
    Gripping,

    /// <summary>The statue holding Samus before it walks or lets her go: still, 23 pixels up and 32 to its side.</summary>
    Holding,

    /// <summary>The stride's first step: the statue shifts its weight two pixels on, Samus held 23 pixels up.</summary>
    StrideWeightShift,

    /// <summary>The stride's second step: the statue moves as far as its support foot does, Samus bobbing one pixel higher.</summary>
    StrideLift,

    /// <summary>The stride's third step: the statue moves as far as its support foot does, Samus bobbing two pixels higher.</summary>
    StrideSwing,

    /// <summary>
    /// The stride's last step: the statue moves as far as its support foot does and slides one pixel more as the foot
    /// plants, Samus bobbing one pixel higher.
    /// </summary>
    StridePlant,
}
