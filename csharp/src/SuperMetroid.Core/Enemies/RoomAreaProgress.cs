using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The story events, the Zebetite count and one area's defeated bosses, as a room's room objects and enemies read and write them.
/// </summary>
/// <param name="system">The session's progression owner.</param>
/// <param name="area">The room's area, whose defeated bosses these are.</param>
internal sealed class RoomAreaProgress(SharedGameState system, AreaId area) : IStoryEvents, IDefeatedBossesRecorder, IZebetiteProgress
{
    /// <summary>The Zebetites destroyed, kept by the session's progress.</summary>
    public int ZebetitesDestroyed
    {
        get => system.Progress.ZebetitesDestroyed;
        set => system.Progress.ZebetitesDestroyed = value;
    }

    /// <summary>Whether <paramref name="storyEvent"/> has happened, as the session's progress keeps it.</summary>
    public bool Has(StoryEvent storyEvent) => system.HasEvent(storyEvent);

    /// <summary>Marks <paramref name="storyEvent"/> as having happened in the session's progress.</summary>
    public void Set(StoryEvent storyEvent) => system.SetEvent(storyEvent);

    /// <summary>Whether any of <paramref name="bosses"/> is defeated in this room's area.</summary>
    public bool IsDefeated(DefeatedBosses bosses) => system.HasDefeatedAnyOf(area, bosses);

    /// <summary>Records <paramref name="bosses"/> as defeated in this room's area.</summary>
    public void SetDefeated(DefeatedBosses bosses) => system.RecordBossesDefeated(area, bosses);
}
