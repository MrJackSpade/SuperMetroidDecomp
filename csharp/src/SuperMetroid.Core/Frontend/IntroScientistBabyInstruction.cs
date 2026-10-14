namespace SuperMetroid.Core.Frontend;

/// <summary>Private instructions of the intro scientist-cutscene baby Metroid actor.</summary>
internal enum IntroScientistBabyInstruction : ushort
{
    /// <summary><c>Instruction_PlayBabyMetroid_Cry1</c> at $8B:A25B.</summary>
    PlayCry1 = 0xa25b,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry2</c> at $8B:A263.</summary>
    PlayCry2 = 0xa263,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry3</c> at $8B:A26B.</summary>
    PlayCry3 = 0xa26b,

    /// <summary><c>Instruction_StartIntroPage4</c> at $8B:B346.</summary>
    StartIntroPage4 = 0xb346,

    /// <summary><c>Instruction_StartIntroPage5</c> at $8B:B34E.</summary>
    StartIntroPage5 = 0xb34e,
}
