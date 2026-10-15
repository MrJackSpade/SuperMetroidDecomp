namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What a cue of the Ceres door's programs does: one per bank-$A6 instruction its lists use.</summary>
internal enum CeresDoorCueKind
{
    /// <summary>Continues at the target while the area boss is alive (<c>GotoYIfAreaBossIsAlive</c>, $A6:F66A).</summary>
    GoToIfAreaBossAlive,
    /// <summary>Continues at the target until Ceres Ridley has escaped (<c>GotoYIfCeresRidleyHasNotEscaped</c>, $A6:F678).</summary>
    GoToIfRidleyNotEscaped,
    /// <summary>Continues at the target unless Samus is within 48 pixels on both axes (<c>GotoYIfSamusIsDistant</c>, $A6:F63E).</summary>
    GoToIfSamusDistant,
    /// <summary>Marks the door drawn by Ridley's scene (<c>SetDrawnByRidleyFlag</c>, $A6:F69F).</summary>
    HandDrawingToRidley,
    /// <summary>Stops colliding with Samus (<c>SetAsIntangible</c>, $A6:F68B).</summary>
    MakeIntangible,
    /// <summary>Hides the door (<c>SetAsInvisible</c>, $A6:F6A6).</summary>
    MakeInvisible,
    /// <summary>Collides with Samus again (<c>SetAsTangible</c>, $A6:F695).</summary>
    MakeTangible,
    /// <summary>Shows the door (<c>SetAsVisible</c>, $A6:F6B3).</summary>
    MakeVisible,
    /// <summary>Shows the door and clears the drawn-by-Ridley mark (<c>SetAsVisibleClearDrawnByRidleyFlag</c>, $A6:F6B0).</summary>
    MakeVisibleAndTakeDrawingBack,
    /// <summary>Queues the door's opening sound (<c>QueueOpeningSFX</c>, $A6:F6BD).</summary>
    PlayOpeningSound,
}
