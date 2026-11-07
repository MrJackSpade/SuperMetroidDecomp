namespace SuperMetroid.Core.Progression;

/// <summary>Global story-event bits.</summary>
public interface IGlobalEventFlags
{
    /// <summary>Whether <paramref name="eventNumber"/> is set.</summary>
    bool Has(EventNumber eventNumber);

    /// <summary>Sets <paramref name="eventNumber"/>.</summary>
    void Set(EventNumber eventNumber);
}
