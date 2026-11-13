using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One pose's installed arm-cannon placement in <c>samus-arm-cannon.json</c> (#1638).</summary>
public sealed record SamusArmCannonPoseDocument
{
    /// <summary>Whether the pose draws the cover, and before or after Samus's body: <c>Hidden</c>, <c>BeforeBody</c> or <c>AfterBody</c>.</summary>
    public required SamusArmCannonDrawOrder DrawOrder { get; init; }
    /// <summary>The aim direction whose cover picture the pose shows at animation frame zero, or at every frame without a later direction.</summary>
    public required SamusProjectileDirection Direction { get; init; }
    /// <summary>The aim direction whose cover picture the pose shows at every animation frame after zero, or none.</summary>
    public SamusProjectileDirection? LaterDirection { get; init; }
    /// <summary>
    /// The cover's displacement from Samus, in pixels, at each animation frame the pose shows (0 through 255), keyed by that
    /// animation frame: right and down are positive, and her pose's art origin is applied after it.
    /// </summary>
    public required Dictionary<int, Offset16<RoomPixels>> Displacements { get; init; }
}
