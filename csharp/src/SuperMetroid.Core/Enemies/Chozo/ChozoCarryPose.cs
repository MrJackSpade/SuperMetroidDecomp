namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The poses a Chozo statue carries Samus through: taking her into its hand, holding her, and the four steps of the
/// stride it walks with her. Each pose has its own statue motion and its own hold on Samus.
/// </summary>
public enum ChozoCarryPose
{
    /// <summary>The open hand Samus is placed in: the statue stands still, holding her up at its side.</summary>
    Reaching,

    /// <summary>The hand closing on Samus: the statue stands still, drawing her in toward its grip.</summary>
    Closing,

    /// <summary>The hand gripping Samus: the statue stands still, holding her in its grip.</summary>
    Gripping,

    /// <summary>The statue holding Samus before it walks or lets her go: still, Samus in its grip.</summary>
    Holding,

    /// <summary>The stride's first step: the statue shifts its weight on, Samus held in its grip.</summary>
    StrideWeightShift,

    /// <summary>The stride's second step: the statue moves as far as its support foot does, Samus bobbing higher.</summary>
    StrideLift,

    /// <summary>The stride's third step: the statue moves as far as its support foot does, Samus bobbing higher still.</summary>
    StrideSwing,

    /// <summary>
    /// The stride's last step: the statue moves as far as its support foot does and slides a little more as the foot
    /// plants, Samus bobbing back down.
    /// </summary>
    StridePlant,
}
