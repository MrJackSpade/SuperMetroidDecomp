namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Private instructions of the intro Metroid egg actor.</summary>
internal enum IntroEggInstruction : ushort
{
    /// <summary><c>Instruction_SpawnMetroidEggParticles</c> at $8B:A918.</summary>
    SpawnParticles = 0xa918,

    /// <summary><c>Instruction_StartIntroPage3</c> at $8B:B33E.</summary>
    StartIntroPage3 = 0xb33e,
}
