namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>What a cue of Zoa's programs does: one per kind of step its lists use.</summary>
internal enum ZoaCueKind
{
    /// <summary>Selects the third, two-pixel launch speed.</summary>
    SetSpeedStage3,
    /// <summary>Selects the first, half-pixel launch speed.</summary>
    SetSpeedStage1,
    /// <summary>Selects the second, five-eighths-pixel launch speed.</summary>
    SetSpeedStage2,
}
