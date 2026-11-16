namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One cue of the Yapping Maw's animations: what it does, with the aim it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Aim">The aim whose mouth offset a <see cref="YappingMawCueKind.HoldSamus"/> cue holds Samus at (#2071).</param>
internal readonly record struct YappingMawCue(YappingMawCueKind Kind, YappingMawAim? Aim = null);
