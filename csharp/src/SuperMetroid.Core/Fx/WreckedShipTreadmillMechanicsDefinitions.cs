using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable control skeletons for the two Wrecked Ship treadmill animated-tile objects.
/// </summary>
/// <remarks>
/// The four frame-source operands are presentation identities resolved to installed art.
/// This catalog owns the object header, boss-wait command, durations, and loop control.
/// </remarks>
public static class WreckedShipTreadmillMechanicsDefinitions
{
    /// <summary>The frames in each treadmill loop.</summary>
    public const int FrameCount = 4;

    /// <summary>The bytes of characters every treadmill frame shows: two characters.</summary>
    public const ushort FrameByteCount = 32;

    /// <summary>The passes each treadmill frame is shown for: every pass shows the next frame.</summary>
    public const int FrameDuration = 1;

    /// <summary>Dispatches the two physical conveyor directions to their native headers.</summary>
    public static WreckedShipTreadmillObjectDefinition ForDirection(
        WreckedShipTreadmillDirection direction) => direction switch
    {
        WreckedShipTreadmillDirection.Rightwards => new(direction,
            AnimatedTileObject.WreckedShipTreadmillRightwards),
        WreckedShipTreadmillDirection.Leftwards => new(direction,
            AnimatedTileObject.WreckedShipTreadmillLeftwards),
        _ => throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}."),
    };

    /// <summary>Resolves either named header; all other bank87 identities return false/null.</summary>
    public static bool TryResolve(AnimatedTileObject animatedObject, [NotNullWhen(true)] out WreckedShipTreadmillObjectDefinition? definition)
    {
        definition = animatedObject switch
        {
            AnimatedTileObject.WreckedShipTreadmillRightwards => ForDirection(WreckedShipTreadmillDirection.Rightwards),
            AnimatedTileObject.WreckedShipTreadmillLeftwards => ForDirection(WreckedShipTreadmillDirection.Leftwards),
            AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or
                AnimatedTileObject.TourianStatueRidley or AnimatedTileObject.TourianStatueKraid or
                AnimatedTileObject.TourianStatueDraygon or AnimatedTileObject.Empty or
                AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.VerticalSpikes or
                AnimatedTileObject.CrateriaLake or AnimatedTileObject.UnusedCrateriaLava or
                AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen or
                AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling or
                AnimatedTileObject.Lava or AnimatedTileObject.Acid or AnimatedTileObject.Rain or
                AnimatedTileObject.Spores => null,
            _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}."),
        };
        return definition is not null;
    }
}
