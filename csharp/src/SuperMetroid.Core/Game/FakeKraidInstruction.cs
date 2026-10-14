namespace SuperMetroid.Core.Game;

/// <summary>Mini-Kraid's private bank-$A6 animation instructions.</summary>
internal enum FakeKraidInstruction : ushort
{
    /// <summary><c>Instruction_MiniKraid_Move</c> at $A6:9B26.</summary>
    Move = 0x9b26,

    /// <summary><c>Instruction_MiniKraid_ChooseAction</c> at $A6:9B74.</summary>
    ChooseAction = 0x9b74,

    /// <summary><c>Instruction_MiniKraid_PlayCrySFX</c> at $A6:9BB2.</summary>
    PlayCrySFX = 0x9bb2,

    /// <summary><c>Instruction_MiniKraid_FireSpitLeft</c> at $A6:9BC4.</summary>
    FireSpitLeft = 0x9bc4,

    /// <summary><c>Instruction_MiniKraid_FireSpitRight</c> at $A6:9C02.</summary>
    FireSpitRight = 0x9c02,
}
