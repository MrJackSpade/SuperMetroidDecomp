namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>What a cue of Zoa's animations does: select one of its three launch speeds.</summary>
internal enum ZoaCueKind
{
    /// <summary>Selects the third, two-pixel launch speed.</summary>
    SetSpeedStage3,
    /// <summary>Selects the first, half-pixel launch speed.</summary>
    SetSpeedStage1,
    /// <summary>Selects the second, five-eighths-pixel launch speed.</summary>
    SetSpeedStage2,
}
