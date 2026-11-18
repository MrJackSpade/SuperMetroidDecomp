namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// What a signalling cue of the ninja Space Pirates' animations does: stop the spin jump's speed, face Samus, or start a divekick.
/// </summary>
internal enum NinjaSpacePirateCueKind
{
    /// <summary>Starts the spin jump with no horizontal speed (#2453).</summary>
    ResetSpinSpeed,
    /// <summary>Continues at the active stance facing Samus.</summary>
    StartActive,
    /// <summary>Starts the divekick's jump, either way.</summary>
    StartDivekick,
}
