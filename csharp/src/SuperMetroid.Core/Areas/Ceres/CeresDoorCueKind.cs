namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What a cue of the Ceres door's programs does: one per kind of step its lists use.</summary>
internal enum CeresDoorCueKind
{
    /// <summary>Continues at the target while the area boss is alive.</summary>
    GoToIfAreaBossAlive,
    /// <summary>Continues at the target until Ceres Ridley has escaped.</summary>
    GoToIfRidleyNotEscaped,
    /// <summary>Continues at the target unless Samus is within 48 pixels on both axes.</summary>
    GoToIfSamusDistant,
    /// <summary>Marks the door drawn by Ridley's scene.</summary>
    HandDrawingToRidley,
    /// <summary>Stops colliding with Samus.</summary>
    MakeIntangible,
    /// <summary>Hides the door.</summary>
    MakeInvisible,
    /// <summary>Collides with Samus again.</summary>
    MakeTangible,
    /// <summary>Shows the door.</summary>
    MakeVisible,
    /// <summary>Shows the door and clears the drawn-by-Ridley mark.</summary>
    MakeVisibleAndTakeDrawingBack,
    /// <summary>Queues the door's opening sound.</summary>
    PlayOpeningSound,
}
