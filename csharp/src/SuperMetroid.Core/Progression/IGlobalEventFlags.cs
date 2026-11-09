namespace SuperMetroid.Core.Progression;

/// <summary>Global story-event bits.</summary>
public interface IGlobalEventFlags
{
    /// <summary>Whether <paramref name="eventNumber"/> is set.</summary>
    bool Has(StoryEvent eventNumber);

    /// <summary>Sets <paramref name="eventNumber"/>.</summary>
    void Set(StoryEvent eventNumber);
}
