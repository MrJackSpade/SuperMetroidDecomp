namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive animation phases shared by Skree and Metaree. Native AI advances
/// through the first three values; the fourth selector is an unused authored list.
/// </summary>
public enum SkreeMetareeAnimationPhase : ushort
{
    Idling = 0,
    PreparingAttack = 1,
    Diving = 2,
    StopAnimating = 3,
}

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

    private static InvalidDataException InvalidPhase(SkreeMetareeAnimationPhase phase, string enemyName) =>
        new($"{enemyName} animation phase ${(int)phase:X4} exceeds its four-entry table.");
}