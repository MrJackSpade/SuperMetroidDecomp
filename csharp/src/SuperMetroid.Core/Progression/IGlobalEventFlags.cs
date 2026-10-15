namespace SuperMetroid.Core.Progression;

/// <summary>Global cartridge event bits.</summary>
public interface IGlobalEventFlags
{
    /// <summary>Whether <paramref name="eventNumber"/> is set.</summary>
    bool Has(EventNumber eventNumber);

    /// <summary>Sets <paramref name="eventNumber"/>.</summary>
    void Set(EventNumber eventNumber);

    /// <summary>Clears <paramref name="eventNumber"/>.</summary>
    void Clear(EventNumber eventNumber);
}
