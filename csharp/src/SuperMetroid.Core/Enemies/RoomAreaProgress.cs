using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The persistent event bits, the Zebetite count and one area's defeated bosses, as a room's room objects and enemies read and write them.
/// </summary>
/// <param name="system">The session's progression owner.</param>
/// <param name="area">The room's area, whose defeated bosses these are.</param>
internal sealed class RoomAreaProgress(SharedGameState system, AreaId area) : IGlobalEventFlags, IAreaBossFlags, IZebetiteProgress
{
    /// <summary>The Zebetites destroyed, kept by the session's progress.</summary>
    public int ZebetitesDestroyed
    {
        get => system.Progress.ZebetitesDestroyed;
        set => system.Progress.ZebetitesDestroyed = value;
    }

    /// <summary>Checks a persistent event bit from the system owner.</summary>
    public bool Has(StoryEvent eventNumber) => system.HasEvent(eventNumber);

    /// <summary>Publishes an event bit through the system owner.</summary>
    public void Set(StoryEvent eventNumber) => system.SetEvent(eventNumber);

    /// <summary>Whether any of <paramref name="bosses"/> is defeated in this room's area.</summary>
    public bool IsDefeated(DefeatedBosses bosses) => system.HasDefeatedAnyOf(area, bosses);

    /// <summary>Records <paramref name="bosses"/> as defeated in this room's area.</summary>
    public void SetDefeated(DefeatedBosses bosses) => system.RecordBossesDefeated(area, bosses);
}
