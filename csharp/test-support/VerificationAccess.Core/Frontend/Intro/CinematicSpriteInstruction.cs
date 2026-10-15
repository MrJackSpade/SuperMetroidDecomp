namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Shared bank-$8B cinematic-sprite interpreter instructions.</summary>
internal enum CinematicSpriteInstruction : ushort
{
    /// <summary><c>CinematicSpriteObject_Instruction_Delete</c> at $8B:9438.</summary>
    Delete = 0x9438,

    /// <summary><c>UNUSED_CinematicSpriteObject_Instruction_Sleep_8B9442</c> at $8B:9442.</summary>
    Sleep = 0x9442,

    /// <summary>Pre-instruction = [[Y]] at $8B:944C.</summary>
    SetPreInstruction = 0x944c,

    /// <summary><c>CinematicSpriteObject_Instruction_GotoY</c> at $8B:94BC.</summary>
    Goto = 0x94bc,

    /// <summary>Decrement timer and go to [[Y]] if non-zero at $8B:94C3.</summary>
    DecrementTimerAndGoto = 0x94c3,

    /// <summary>Timer = [[Y]] at $8B:94D6.</summary>
    SetTimer = 0x94d6,
}
