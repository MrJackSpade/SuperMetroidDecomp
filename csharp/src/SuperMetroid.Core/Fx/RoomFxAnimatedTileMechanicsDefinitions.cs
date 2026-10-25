using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable engine-owned header and control data for the simple retail room-FX
/// animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// Frame source pointers deliberately are not stored here. They identify replaceable
/// installed character artwork. This catalog owns
/// only the instruction cursor, transfer geometry, duration, and loop control required
/// to schedule that artwork.
/// </remarks>
public static class RoomFxAnimatedTileMechanicsDefinitions
{
    /// <summary>Direct object-to-header dispatch; unknown bank87 identities return null.</summary>
    /// <remarks>Seven named spike/sand/liquid/weather objects select native instruction starts,
    /// transfer sizes and encoded VRAM destinations. Each field and the full ushort
    /// identity domain are independently verified against NTSC J/U v1.0 and pinned
    /// bank_87.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Frame schedules use the separately verified arithmetic view; neither headers
    /// nor frames are retained in a generated table.</remarks>
    private static RoomFxAnimatedTileObjectDefinition? SelectObject(AnimatedTileObject animatedObject) => animatedObject switch
    {
        AnimatedTileObject.HorizontalSpikes => new(AnimatedTileObject.HorizontalSpikes,
            frameByteCount: 128,
            frameCount: 4, frameDuration: 8, imageCount: 3),
        AnimatedTileObject.MaridiaSandCeiling => new(AnimatedTileObject.MaridiaSandCeiling,
            frameByteCount: 64,
            frameCount: 4, frameDuration: 10, imageCount: 4),
        AnimatedTileObject.MaridiaSandFalling => new(AnimatedTileObject.MaridiaSandFalling,
            frameByteCount: 32,
            frameCount: 4, frameDuration: 10, imageCount: 4),
        AnimatedTileObject.Lava => new(AnimatedTileObject.Lava,
            frameByteCount: 64,
            frameCount: 5, frameDuration: 13, imageCount: 5),
        AnimatedTileObject.Acid => new(AnimatedTileObject.Acid,
            frameByteCount: 64,
            frameCount: 5, frameDuration: 10, imageCount: 5),
        AnimatedTileObject.Rain => new(AnimatedTileObject.Rain,
            frameByteCount: 80,
            frameCount: 5, frameDuration: 10, imageCount: 5),
        AnimatedTileObject.Spores => new(AnimatedTileObject.Spores,
            frameByteCount: 48,
            frameCount: 3, frameDuration: 10, imageCount: 3),
        AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or
            AnimatedTileObject.TourianStatueRidley or AnimatedTileObject.TourianStatueKraid or
            AnimatedTileObject.TourianStatueDraygon or AnimatedTileObject.Empty or
            AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or
            AnimatedTileObject.UnusedCrateriaLava or AnimatedTileObject.BrinstarPlant or
            AnimatedTileObject.WreckedShipScreen or AnimatedTileObject.WreckedShipTreadmillRightwards or
            AnimatedTileObject.WreckedShipTreadmillLeftwards => null,
        _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {animatedObject}."),
    };

    /// <summary>The definition of an object this catalog supports.</summary>
    internal static RoomFxAnimatedTileObjectDefinition Require(AnimatedTileObject animatedObject) => SelectObject(animatedObject) ??
        throw new InvalidOperationException($"{animatedObject} has no definition in this catalog.");

    /// <summary>Enumerates the seven supported objects without stored headers.</summary>
    public static IEnumerable<RoomFxAnimatedTileObjectDefinition> All
    {
        get
        {
            yield return Require(AnimatedTileObject.HorizontalSpikes);
            yield return Require(AnimatedTileObject.MaridiaSandCeiling);
            yield return Require(AnimatedTileObject.MaridiaSandFalling);
            yield return Require(AnimatedTileObject.Lava);
            yield return Require(AnimatedTileObject.Acid);
            yield return Require(AnimatedTileObject.Rain);
            yield return Require(AnimatedTileObject.Spores);
        }
    }

    /// <summary>Resolves a supported header; unknown identities preserve false and null output.</summary>
    public static bool TryResolve(AnimatedTileObject animatedObject, [NotNullWhen(true)] out RoomFxAnimatedTileObjectDefinition? definition)
    {
        definition = SelectObject(animatedObject);
        return definition is not null;
    }
}
