using Engine.Shared;

namespace Engine.World;

/// <summary>
/// Something that can be latched onto, as by a grappling line: it says where the line holds, and is told when it is
/// taken hold of, with where the line's end then was, and when it is let go. Whatever latches asks this; it never
/// asks what kind of thing it has hold of.
/// </summary>
public interface ILatchable
{
    /// <summary>Whether it can be latched onto right now.</summary>
    bool CanBeLatched { get; }

    /// <summary>Where a line latched onto it holds, in the room.</summary>
    PixelPosition LatchPoint { get; }

    /// <summary>Tells it a line has taken hold of it, and where in the room the line's end was as it did.</summary>
    void Latched(PixelPosition at);

    /// <summary>Tells it the line has let go.</summary>
    void Unlatched();
}
