namespace SuperMetroid.Core.Frontend;

/// <summary>Private instructions of the intro's confused baby Metroid actor.</summary>
internal enum ConfusedBabyInstruction : ushort
{
    /// <summary><c>Instruction_PlayBabyMetroid_Cry1</c> at $8B:A25B.</summary>
    PlayCry1 = 0xa25b,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry2</c> at $8B:A263.</summary>
    PlayCry2 = 0xa263,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry3</c> at $8B:A26B.</summary>
    PlayCry3 = 0xa26b,
}
