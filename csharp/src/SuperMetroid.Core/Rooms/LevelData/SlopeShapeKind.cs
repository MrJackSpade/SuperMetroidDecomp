namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>How Samus, her shots and enemies read a slope shape (<see cref="SlopeShape"/>). Its values are arbitrary.</summary>
public enum SlopeShapeKind
{
    /// <summary>The shape is made of solid quarters, read through the square slopes.</summary>
    Square,

    /// <summary>The shape is a height profile, read column by column.</summary>
    Profile,
}
