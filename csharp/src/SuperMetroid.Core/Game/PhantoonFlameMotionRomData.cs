namespace SuperMetroid.Core.Game;

/// <summary>NTSC Japan/USA operands for bank-$86 Phantoon flame motion; PAL values differ.</summary>
internal static class PhantoonFlameMotionRomData
{
    /// <summary>$86:9885/$988D, enraged initializer: signed clockwise/counterclockwise angle step magnitude.</summary>
    internal const int RageAngleStep = 2;
    /// <summary>$86:9A49, enraged pre-instruction: radius increase per frame.</summary>
    internal const ushort RageRadiusStep = 4;
    /// <summary>$86:9ADE, spiral pre-instruction: radius increase per frame.</summary>
    internal const ushort SpiralRadiusStep = 2;
    /// <summary>$86:9AE8, spiral pre-instruction: clockwise angle increase per frame.</summary>
    internal const ushort SpiralAngleStep = 2;
}
