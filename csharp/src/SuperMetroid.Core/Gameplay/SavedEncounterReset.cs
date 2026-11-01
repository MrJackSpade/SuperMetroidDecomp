using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The defeated encounters <see cref="GameplayAssists.EncounterResetOnLoad"/> returns to their undefeated state when a
/// regular save is loaded: the bosses, minibosses and Torizos of whole areas, story events, and the door flags that keep
/// a defeated door guardian away.
/// </summary>
public sealed record SavedEncounterReset
{
    /// <summary>The areas whose every boss, miniboss and Torizo defeat is forgotten.</summary>
    public required IReadOnlyList<AreaId> BossAreas { get; init => field = Ensure.NotNull(value); }

    /// <summary>The story events cleared, so the encounters they record happen again.</summary>
    public required IReadOnlyList<EventNumber> ClearedEvents { get; init => field = Ensure.NotNull(value); }

    /// <summary>The door flags cleared, numbered as the save's door flags are, so the door guardians they record return.</summary>
    public required IReadOnlyList<int> ClearedDoorFlags { get; init => field = Ensure.NotNull(value); }
}
