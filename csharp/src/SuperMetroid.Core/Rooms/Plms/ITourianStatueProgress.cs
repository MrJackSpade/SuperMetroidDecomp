namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Tourian entrance statue progress that grey-door conditions read (#633).</summary>
public interface ITourianStatueProgress
{
    /// <summary>Whether the entrance statue sequence has finished.</summary>
    bool IsFinished { get; }
}
