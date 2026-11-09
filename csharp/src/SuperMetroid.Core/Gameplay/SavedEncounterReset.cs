using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The defeated encounters <see cref="GameplayAssists.EncounterResetOnLoad"/> returns to their undefeated state when a
/// regular save is loaded: the bosses, minibosses and Torizos of whole areas, story events, and the opened doors that keep
/// a defeated door guardian away.
/// </summary>
public sealed record SavedEncounterReset
{
    /// <summary>The areas whose every boss, miniboss and Torizo defeat is forgotten.</summary>
    public required IReadOnlyList<AreaId> BossAreas { get; init => field = Ensure.NotNull(value); }

    /// <summary>The story events cleared, so the encounters they record happen again.</summary>
    public required IReadOnlyList<StoryEvent> ClearedEvents { get; init => field = Ensure.NotNull(value); }

    /// <summary>The doors closed again, so the door guardians their opening records return.</summary>
    public required IReadOnlyList<LockedDoor> ClearedDoors { get; init => field = Ensure.NotNull(value); }
}
