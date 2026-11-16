namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What a branching cue of the escaping Dachora's animations waits for: the rising acid or the critters' escape.</summary>
internal enum EscapeDachoraCueKind
{
    /// <summary>Continues at the target once the rising acid is above Y 206.</summary>
    GoToIfAcidHigh,
    /// <summary>Continues at the target once the critters have escaped.</summary>
    GoToIfCrittersEscaped,
}
