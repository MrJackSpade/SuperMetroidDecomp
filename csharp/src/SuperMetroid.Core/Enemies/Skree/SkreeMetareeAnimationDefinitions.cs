namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Fixed animation selectors for Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitions
{
    /// <summary>$A3:894E: select the Metaree program for its named animation phase.</summary>
    internal static SkreeMetareeProgram MetareeProgram(SkreeMetareeAnimationPhase phase) => phase switch
    {
        SkreeMetareeAnimationPhase.Idling => SkreeMetareeProgram.MetareeIdling,
        SkreeMetareeAnimationPhase.PreparingAttack => SkreeMetareeProgram.MetareePreparingAttack,
        SkreeMetareeAnimationPhase.Diving => SkreeMetareeProgram.MetareeDiving,
        SkreeMetareeAnimationPhase.StopAnimating => SkreeMetareeProgram.MetareeStopAnimating,
        _ => throw InvalidPhase(phase, "Metaree"),
    };

    /// <summary>$A3:C69C: select the Skree program for its named animation phase.</summary>
    internal static SkreeMetareeProgram SkreeProgram(SkreeMetareeAnimationPhase phase) => phase switch
    {
        SkreeMetareeAnimationPhase.Idling => SkreeMetareeProgram.SkreeIdling,
        SkreeMetareeAnimationPhase.PreparingAttack => SkreeMetareeProgram.SkreePreparingAttack,
        SkreeMetareeAnimationPhase.Diving => SkreeMetareeProgram.SkreeDiving,
        SkreeMetareeAnimationPhase.StopAnimating => SkreeMetareeProgram.SkreeStopAnimating,
        _ => throw InvalidPhase(phase, "Skree"),
    };

    /// <summary>Reports an out-of-table animation selector with its enemy name and native word value.</summary>
    private static InvalidDataException InvalidPhase(SkreeMetareeAnimationPhase phase, string enemyName) =>
        new($"{enemyName} animation phase ${(int)phase:X4} exceeds its four-entry table.");
}
