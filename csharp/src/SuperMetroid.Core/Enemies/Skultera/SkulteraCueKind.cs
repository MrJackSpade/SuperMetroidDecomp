namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>What a cue of Skultera's programs does: one per instruction its lists use.</summary>
internal enum SkulteraCueKind
{
    /// <summary>Draws the fish on layer six, above the matching scenery.</summary>
    DrawAboveScenery,
    /// <summary>Draws the fish on layer two, below the matching scenery.</summary>
    DrawBelowScenery,
    /// <summary>Tells main AI the turn is complete.</summary>
    FinishTurn,
}
