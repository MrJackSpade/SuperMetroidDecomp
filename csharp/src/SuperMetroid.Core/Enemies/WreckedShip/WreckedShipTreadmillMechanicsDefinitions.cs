using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// Immutable control skeletons for the two Wrecked Ship treadmill animated-tile objects.
/// </summary>
/// <remarks>
/// The four frame-source operands are presentation identities resolved to installed art.
/// This catalog owns the object header, boss-wait command, durations, and loop control.
/// </remarks>
public static class WreckedShipTreadmillMechanicsDefinitions
{
    /// <summary>Dispatches the two physical conveyor directions to their native headers.</summary>
    public static WreckedShipTreadmillObjectDefinition ForDirection(
        WreckedShipTreadmillDirection direction) => direction switch
    {
        WreckedShipTreadmillDirection.Rightwards => new(direction,
            AnimatedTileObject.WreckedShipTreadmillRightwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillRightwardsWait),
        WreckedShipTreadmillDirection.Leftwards => new(direction,
            AnimatedTileObject.WreckedShipTreadmillLeftwards,
            AnimatedTileInstructionListPointers.WreckedShipTreadmillLeftwardsWait),
        _ => throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}."),
    };

    /// <summary>Resolves either named header; all other bank87 identities return false/null.</summary>
    public static bool TryResolve(AnimatedTileObject objectPointer, [NotNullWhen(true)] out WreckedShipTreadmillObjectDefinition? definition)
    {
        definition = objectPointer switch
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
            _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {objectPointer}."),
        };
        return definition is not null;
    }
}
