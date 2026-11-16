namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="RotationAngle"/> that only verification reads.</summary>
internal static class RotationAngleConstants
{
    /// <summary>The angle of exactly <paramref name="rawValue"/> 256ths of a step, 0 through 65535.</summary>
    public static RotationAngle FromRaw(int rawValue) => new(checked((ushort)rawValue));
}
