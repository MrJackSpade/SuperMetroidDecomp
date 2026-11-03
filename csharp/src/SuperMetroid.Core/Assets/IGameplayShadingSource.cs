namespace SuperMetroid.Core.Assets;

/// <summary>
/// An owner of a colour effect gameplay's background pictures show (#1512): a room's palette effects, its layer-3
/// effect's colours, a door's fade or a boss's colours. It holds the effect as typed state and, as a frame is published,
/// names the shades and effects that state shows.
/// </summary>
public interface IGameplayShadingSource
{
    /// <summary>Lays the shades and effects the owner's state shows over the pictures it colours.</summary>
    /// <param name="frame">The frame's shading as it is gathered.</param>
    void ShadeGameplay(GameplayShadingBuilder frame);
}
