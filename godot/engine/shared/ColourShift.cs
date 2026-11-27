using Godot;

namespace Engine.Shared;

/// <summary>
/// A colour added to, or taken from, every colour something shows: each of the three parts of a colour moved by so
/// much of its whole range, up or down, and held at the range's ends. It is what the cartridge's arithmetic on the
/// colours of the things that take part in it comes to: a room's dark taken from what lies behind it, a glow added.
/// </summary>
/// <param name="Red">How far red is moved: a part of its range, above nought added and below it taken.</param>
/// <param name="Green">How far green is moved.</param>
/// <param name="Blue">How far blue is moved.</param>
public readonly record struct ColourShift(float Red, float Green, float Blue)
{
    /// <summary>No shift: what takes back a shift asked for.</summary>
    public static ColourShift None => default;

    /// <summary>Whether it moves nothing.</summary>
    public bool IsNone => Red == 0 && Green == 0 && Blue == 0;

    /// <summary>The shift that takes <paramref name="colour"/> from every colour.</summary>
    public static ColourShift Taking(Color colour) => new(-colour.R, -colour.G, -colour.B);
}
