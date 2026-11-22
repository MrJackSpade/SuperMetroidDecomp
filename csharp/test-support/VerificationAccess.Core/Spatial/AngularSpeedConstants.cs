namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="AngularSpeed"/> that only verification reads.</summary>
internal static class AngularSpeedConstants
{
    /// <summary>
    /// A speed of <paramref name="fractionsOfStep"/> 256ths of a step an update, -32768 through 32767: the precision a
    /// rotation angle turns at.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    public static AngularSpeed FromFractionsOfStep(int fractionsOfStep) => new global::SuperMetroid.Core.Spatial.AngularSpeed(fractionsOfStep);
}
