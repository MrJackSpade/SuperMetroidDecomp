namespace Engine.World;

/// <summary>Something placed in a room's scene, which may be there only when a condition holds.</summary>
public interface IPlaced
{
    /// <summary>The condition under which the thing is present when its room is set up; null when it always is.</summary>
    Condition? PresentWhen { get; }
}
