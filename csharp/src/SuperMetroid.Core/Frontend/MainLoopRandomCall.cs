namespace SuperMetroid.Core.Frontend;

/// <summary>What MainGameLoop's RNG call does before a game state's dispatch.</summary>
internal enum MainLoopRandomCall
{
    /// <summary>The reset vector reseeds, then advances once: state zero is dispatched after the first call.</summary>
    Reseed,

    /// <summary>HDMA objects run first (lava swaps the live RNG bytes), then the RNG advances.</summary>
    NonGameplayLineEffectsThenAdvance,

    /// <summary>The RNG advances unless the update resumes the dispatch inside its own NMI wait.</summary>
    AdvanceUnlessResumingWait,

    /// <summary>The RNG advances once before the dispatch.</summary>
    Advance,

    /// <summary>The gameplay runtime makes its own call, or the state makes none.</summary>
    RuntimeOwned,
}
