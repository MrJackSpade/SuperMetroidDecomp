namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What a cue of the escaping Dachora's animations does: one per kind of step its lists use.</summary>
internal enum EscapeDachoraCueKind
{
    /// <summary>Continues at the target once the rising acid is above Y 206.</summary>
    GoToIfAcidHigh,
    /// <summary>Continues at the target once the critters have escaped.</summary>
    GoToIfCrittersEscaped,
    /// <summary>Moves the Dachora six pixels left.</summary>
    MoveLeft,
    /// <summary>Moves the Dachora six pixels right.</summary>
    MoveRight,
}
