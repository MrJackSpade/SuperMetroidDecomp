namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the ninja Space Pirates' animations does: one per kind of step its lists use.</summary>
internal enum NinjaSpacePirateCueKind
{
    /// <summary>Resets the speed.</summary>
    ResetSpeed,
    /// <summary>Continues at the active stance facing Samus.</summary>
    StartActive,
    /// <summary>Starts the leftward divekick jump.</summary>
    StartLeftDivekick,
    /// <summary>Starts the rightward divekick jump.</summary>
    StartRightDivekick,
}
