namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// What a signalling cue of the ninja Space Pirates' animations does: reset the speed, face Samus, or start a divekick.
/// </summary>
internal enum NinjaSpacePirateCueKind
{
    /// <summary>Resets the speed.</summary>
    ResetSpeed,
    /// <summary>Continues at the active stance facing Samus.</summary>
    StartActive,
    /// <summary>Starts the divekick's jump, either way.</summary>
    StartDivekick,
}
