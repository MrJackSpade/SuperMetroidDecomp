using static SuperMetroid.Core.Enemies.QuadraticSpeedCurveDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>The shared quadratic enemy speed curve: the authored speed at each of its marks.</summary>
public static class QuadraticSpeedCurve
{
    /// <summary>The speed at mark <paramref name="mark"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="mark"/> is not one of the authored marks.</exception>
    public static QuadraticCurveSpeed AtMark(int mark) => mark >= 0 && mark < Speeds.Length
        ? Speeds[mark]
        : throw new InvalidDataException($"Quadratic speed curve mark {mark} is outside the {MarkCount} authored marks.");
}
