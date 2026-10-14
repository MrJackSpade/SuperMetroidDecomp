namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Private instructions of the Ceres-destruction PLANET ZEBES title actor.</summary>
internal enum ZebesTitleInstruction : ushort
{
    /// <summary><c>Instruction_FadeInPlanetZebesText</c> at $8B:C9A5.</summary>
    FadeInText = 0xc9a5,

    /// <summary><c>Instruction_SpawnPlanetZebesSubtitleIfNeeded</c> at $8B:C9AF.</summary>
    SpawnJapaneseTextIfNeeded = 0xc9af,

    /// <summary><c>Instruction_FadeOutPlanetZebesText</c> at $8B:C9BD.</summary>
    FadeOutText = 0xc9bd,

    /// <summary><c>Instruction_StartFlyingToZebes</c> at $8B:C9C7.</summary>
    StartFlyingToZebes = 0xc9c7,
}
