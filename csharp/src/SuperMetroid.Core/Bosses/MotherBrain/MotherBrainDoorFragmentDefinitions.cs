namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled physical definitions for Mother Brain's eight escape-door fragments.</summary>
internal static class MotherBrainDoorFragmentDefinitions
{
    /// <summary>Eight authored fragments are selected by the projectile's zero-based spawn parameter.</summary>
    internal const int Count = 8;

    /// <summary>$86:C9B2-$86:C9D1, interleaved with the X velocity: the fragments' authored scatter speeds.</summary>
    internal static ReadOnlySpan<short> YVelocities => [-512, -256, -256, -128, -128, 128, -256, 512];

    /// <summary>Every fragment flies right at $0500.</summary>
    internal const int XVelocity = 1280;

    /// <summary>The first fragment's Y offset, 32 pixels above the door.</summary>
    internal const int FirstY = -32;

    /// <summary>Each following fragment starts this many pixels lower.</summary>
    internal const int YStep = 8;
}
