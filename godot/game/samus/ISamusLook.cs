namespace Game;

/// <summary>A state of Samus's own that says which of her timelines shows it.</summary>
public interface ISamusLook
{
    /// <summary>The name of the look: the first part of the name of the timeline that shows the state.</summary>
    string Look { get; }
}
