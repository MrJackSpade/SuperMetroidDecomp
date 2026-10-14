namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>What a cue of Skultera's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum SkulteraCueKind
{
    /// <summary>Draws the fish on layer six, above the matching scenery (<c>SetLayerTo6</c>, $A3:9096).</summary>
    DrawAboveScenery,
    /// <summary>Draws the fish on layer two, below the matching scenery (<c>SetLayerTo2</c>, $A3:90A0).</summary>
    DrawBelowScenery,
    /// <summary>Tells main AI the turn is complete (<c>SetTurnFinishedFlag</c>, $A3:90AA).</summary>
    FinishTurn,
}
