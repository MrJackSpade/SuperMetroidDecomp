namespace Engine.World;

/// <summary>Something a trigger, a switch or a sequence can tell to do one of the things it knows how to do, by name.</summary>
public interface ITriggerTarget
{
    /// <summary>Does the named thing. A name the target does not know is an error.</summary>
    void Triggered(string what);
}
