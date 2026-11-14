using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One brain frame (#1747): the whole or damaged head at each mouth stage and the corpse, named for the installed frame
/// <see cref="MotherBrainVisualDefinitions"/> shows for it. Its values are arbitrary.
/// </summary>
public enum MotherBrainHeadFrame
{
    /// <summary>Initial.</summary>
    Initial,
    /// <summary>Neutral phase two second.</summary>
    NeutralPhaseTwoSecond,
    /// <summary>Stretching phase two first.</summary>
    StretchingPhaseTwoFirst,
    /// <summary>Stretching phase two second.</summary>
    StretchingPhaseTwoSecond,
    /// <summary>Stretching phase two third.</summary>
    StretchingPhaseTwoThird,
    /// <summary>Decapitated loop.</summary>
    DecapitatedLoop,
    /// <summary>Decapitated second.</summary>
    DecapitatedSecond,
    /// <summary>Stretching phase three first.</summary>
    StretchingPhaseThreeFirst,
    /// <summary>Stretching phase three second.</summary>
    StretchingPhaseThreeSecond,
    /// <summary>Stretching phase three third.</summary>
    StretchingPhaseThreeThird,
    /// <summary>Corpse fourth.</summary>
    CorpseFourth,
    /// <summary>Corpse loop.</summary>
    CorpseLoop,
}
