namespace SuperMetroid.Core.Earthquakes;

/// <summary>
/// The side of its resting place a shaking room shows on a frame (#2421): its shake's displacement, or the far side opposite
/// it. Its values are arbitrary.
/// </summary>
public enum EarthquakeSide
{
    /// <summary>The room shows its shake's displacement.</summary>
    Near,

    /// <summary>The room shows the side opposite its shake's displacement: the displacement negated.</summary>
    Far,
}
