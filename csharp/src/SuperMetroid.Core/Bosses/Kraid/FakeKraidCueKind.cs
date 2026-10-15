namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What a cue of Mini-Kraid's programs does: one per bank-$A6 instruction its lists use.</summary>
internal enum FakeKraidCueKind
{
    /// <summary>Continues at the step or spit animation the decision timer and facing select (<c>Instruction_MiniKraid_ChooseAction</c>, $A6:9B74).</summary>
    ChooseAction,
    /// <summary>Walks one four-pixel step unless the reversal clock expires, then faces Samus (<c>Instruction_MiniKraid_Move</c>, $A6:9B26).</summary>
    Move,
    /// <summary>Records the cry sound while Mini-Kraid is on screen (<c>Instruction_MiniKraid_PlayCrySFX</c>, $A6:9BB2).</summary>
    PlayCrySound,
    /// <summary>Spits a pair of projectiles to the left (<c>Instruction_MiniKraid_FireSpitLeft</c>, $A6:9BC4).</summary>
    SpitLeft,
    /// <summary>Spits a pair of projectiles to the right (<c>Instruction_MiniKraid_FireSpitRight</c>, $A6:9C02).</summary>
    SpitRight,
}
