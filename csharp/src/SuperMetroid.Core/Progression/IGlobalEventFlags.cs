namespace SuperMetroid.Core.Progression;

/// <summary>The story events that have happened, which room objects and enemies read and mark.</summary>
public interface IGlobalEventFlags
{
    /// <summary>Whether <paramref name="storyEvent"/> has happened.</summary>
    bool Has(StoryEvent storyEvent);

    /// <summary>Marks <paramref name="storyEvent"/> as having happened.</summary>
    void Set(StoryEvent storyEvent);
}
