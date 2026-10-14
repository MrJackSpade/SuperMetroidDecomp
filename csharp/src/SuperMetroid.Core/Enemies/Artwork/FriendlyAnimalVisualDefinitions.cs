using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Program-selected ordinary OAM compositions for the friendly animals and their
/// escape variants. Their route, jumps, echoes and escape timing remain mechanics.
/// </summary>
internal static class FriendlyAnimalVisualDefinitions
{
    /// <summary>Etecoon/Dachora ordinary animation and sprite bank $A7.</summary>
    internal const byte NormalBank = 0xa7;
    /// <summary>The rescue-sequence animals' ordinary animation and sprite bank $B3.</summary>
    internal const byte EscapeBank = 0xb3;
    /// <summary>Thirty-one selected Etecoon compositions at $A7:EEED..F20A.</summary>
    internal const int EtecoonFrameCount = 31;
    /// <summary>Twenty-nine selected Dachora body/echo compositions at $A7:F9C4..FF53.</summary>
    internal const int DachoraFrameCount = 29;
    /// <summary>Ten selected rescue Etecoon compositions at $B3:E736..E91F.</summary>
    internal const int EscapeEtecoonFrameCount = 10;
    /// <summary>Twelve selected rescue Dachora compositions at $B3:EB1B..ED3E.</summary>
    internal const int EscapeDachoraFrameCount = 12;

    /// <summary>Collects the Etecoon pose operands selected by its ordinary animation program.</summary>
    internal static ushort[] EtecoonOperands() => Enumerable.Range(0,
        EtecoonInstructionProgramDefinitions.PresentationWordCount)
        .Select(EtecoonInstructionProgramDefinitions.PresentationWordAddress).ToArray();
    /// <summary>Collects Dachora's body and echo pose operands from its ordinary program.</summary>
    internal static ushort[] DachoraOperands() => Enumerable.Range(0,
        DachoraInstructionProgramDefinitions.PresentationWordCount)
        .Select(DachoraInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Compiles the four animal frame groups with their respective native sprite banks.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
    [
        .. CompiledEnemyCompositionDefinitions.Frames(NormalBank, EtecoonOperands(),
            EtecoonFrameCount, "etecoon"),
        .. CompiledEnemyCompositionDefinitions.Frames(NormalBank, DachoraOperands(),
            DachoraFrameCount, "dachora"),
        // The rescue-sequence animals are typed programs (#641): their poses in first-shown order.
        .. CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(EscapeBank,
            EscapeEtecoonAnimations.Spritemaps(Enum.GetValues<EscapeEtecoonProgram>()), EscapeEtecoonFrameCount, "escape_etecoon"),
        .. CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(EscapeBank,
            EscapeDachoraAnimations.Spritemaps(Enum.GetValues<EscapeDachoraProgram>()), EscapeDachoraFrameCount, "escape_dachora"),
    ];
}
