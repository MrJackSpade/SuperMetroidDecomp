namespace SuperMetroid.Core.Frontend;

/// <summary>What the random generator does before a game state's dispatch each update (#1420).</summary>
internal enum DispatchRandomStep
{
    /// <summary>The generator is reseeded at power-on, then advances once: state zero is dispatched after the first advance.</summary>
    Reseed,

    /// <summary>The per-line effects run first (lava swaps the generator's live bytes), then the generator advances.</summary>
    NonGameplayLineEffectsThenAdvance,

    /// <summary>The generator advances unless the update resumes the dispatch inside its own frame wait.</summary>
    AdvanceUnlessResumingWait,

    /// <summary>The generator advances once before the dispatch.</summary>
    Advance,

    /// <summary>The gameplay runtime advances it itself, or the state does not advance it.</summary>
    RuntimeOwned,
}
