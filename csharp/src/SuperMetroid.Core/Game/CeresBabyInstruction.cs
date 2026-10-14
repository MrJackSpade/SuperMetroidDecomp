namespace SuperMetroid.Core.Game;

/// <summary>The Ceres Baby Metroid cutscene animation-list instructions in bank $A6.</summary>
internal enum CeresBabyInstruction : ushort
{
    /// <summary><c>Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX</c> at $A6:BFC9.</summary>
    PlayCrySfxOrGoto = 0xbfc9,
    /// <summary><c>Instruction_BabyMetroidCutscene_UpdateColors</c> at $A6:BFE1.</summary>
    UpdateColors = 0xbfe1,
    /// <summary><c>Instruction_BabyMetroidCutscene_GotoXIfNotFalling</c> at $A6:BFF2.</summary>
    GotoIfNotFalling = 0xbff2,
    /// <summary><c>Instruction_BabyMetroidCutscene_GotoX</c> at $A6:BFF8.</summary>
    Goto = 0xbff8,
}
