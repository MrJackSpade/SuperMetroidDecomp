
namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values and helpers of <see cref="Layer3Plane"/> that only verification reads.</summary>
internal static class Layer3PlaneConstants
{
    /// <summary>The rows at the top of the plane the HUD's images cover.</summary>
    public const int HudRows = 4;

    /// <summary>The tile word the cartridge clears the effect area to, which the plane's cleared picture pictures (#1466).</summary>
    public static SuperMetroid.Core.Assets.BackgroundTile ClearedCell => SuperMetroid.AssetExtraction.Fx.Layer3ClearedCellDefinitions.ClearedCell;
}
