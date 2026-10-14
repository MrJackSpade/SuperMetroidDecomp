namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed animation-program selectors for Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitions
{
    /// <summary>$A3:894E: select the Metaree program for its named animation phase.</summary>
    internal static ushort MetareeInstructionList(SkreeMetareeAnimationPhase phase) => phase switch
    {
        SkreeMetareeAnimationPhase.Idling => SkreeMetareeInstructionProgramDefinitions.MetareeIdling,
        SkreeMetareeAnimationPhase.PreparingAttack => SkreeMetareeInstructionProgramDefinitions.MetareePreparingAttack,
        SkreeMetareeAnimationPhase.Diving => SkreeMetareeInstructionProgramDefinitions.MetareeDiving,
        SkreeMetareeAnimationPhase.StopAnimating => SkreeMetareeInstructionProgramDefinitions.MetareeStopAnimating,
        _ => throw InvalidPhase(phase, "Metaree"),
    };

    /// <summary>$A3:C69C: select the Skree program for its named animation phase.</summary>
    internal static ushort SkreeInstructionList(SkreeMetareeAnimationPhase phase) => phase switch
    {
        SkreeMetareeAnimationPhase.Idling => SkreeMetareeInstructionProgramDefinitions.SkreeIdling,
        SkreeMetareeAnimationPhase.PreparingAttack => SkreeMetareeInstructionProgramDefinitions.SkreePreparingAttack,
        SkreeMetareeAnimationPhase.Diving => SkreeMetareeInstructionProgramDefinitions.SkreeDiving,
        SkreeMetareeAnimationPhase.StopAnimating => SkreeMetareeInstructionProgramDefinitions.SkreeStopAnimating,
        _ => throw InvalidPhase(phase, "Skree"),
    };

    /// <summary>Reports an out-of-table animation selector with its enemy name and native word value.</summary>
    private static InvalidDataException InvalidPhase(SkreeMetareeAnimationPhase phase, string enemyName) =>
        new($"{enemyName} animation phase ${(int)phase:X4} exceeds its four-entry table.");
}
