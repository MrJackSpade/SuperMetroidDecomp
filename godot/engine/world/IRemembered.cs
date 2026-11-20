namespace Engine.World;

/// <summary>Something placed in a room that progress may remember by a name.</summary>
public interface IRemembered
{
    /// <summary>The name it is remembered by in progress; empty when nothing about it is remembered. Unique across the whole game.</summary>
    string RememberedAs { get; }

    /// <summary>Whether this kind of thing makes no sense unless it is remembered, so that having no name is a mistake.</summary>
    bool MustBeRemembered { get; }
}
