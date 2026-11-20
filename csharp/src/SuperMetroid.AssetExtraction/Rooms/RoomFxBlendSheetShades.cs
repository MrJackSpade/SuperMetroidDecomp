using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The shades of each room-FX palette blend (#1512), which the import pre-colours a tileset's layer-3 pictures in: a
/// blend loads its three colours over the HUD's effect colours' tenth through twelfth, and the empty blend clears only
/// the twelfth, as <c>LoadFXHeader</c> copies them.
/// </summary>
internal static class RoomFxBlendSheetShades
{
    /// <summary>The room effects' tints in the order the blend sheet lists their shades (#2567).</summary>
    private static readonly IReadOnlyList<RoomEffectTint> TintOrder = Ensure.EveryMemberOnce<RoomEffectTint>(
    [
        RoomEffectTint.None,
        RoomEffectTint.Lava,
        RoomEffectTint.LandingSiteRain,
        RoomEffectTint.IndigoMaridiaWater,
        RoomEffectTint.WaterAndAcid,
        RoomEffectTint.Fog,
        RoomEffectTint.TealMaridiaWater,
        RoomEffectTint.GreenMaridiaWater,
        RoomEffectTint.PurpleMaridiaWater,
    ]);

    /// <summary>The first blend ink of a tileset's layer-3 picture.</summary>
    private const int FirstInk = 25;

    /// <summary>The ink the empty blend clears: the third blend ink.</summary>
    private const int ClearedInk = FirstInk + 2;

    /// <summary>Every blend's shade, with the colours <paramref name="blends"/> installs for it.</summary>
    internal static IReadOnlyList<SheetShade> Of(RoomFxBlendInks blends)
    {
        Ensure.NotNull(blends);
        return [.. TintOrder.Select(blend => SheetShade.Run(RoomEffectTintShading.Of(blend), FirstInkOf(blend), [.. blends.Inks(blend)]))];
    }

    /// <summary>
    /// The first ink <paramref name="blend"/>'s colours write: each blend's colour run starts at the first blend ink, but the
    /// empty blend's, which only clears its cleared ink.
    /// </summary>
    private static int FirstInkOf(RoomEffectTint blend) => InkRunsApart.TryGetValue(blend, out int first) ? first : FirstInk;

    /// <summary>The blends whose colour run starts elsewhere than the first blend ink, with the ink it starts at: the empty blend's.</summary>
    private static readonly IReadOnlyDictionary<RoomEffectTint, int> InkRunsApart = new Dictionary<RoomEffectTint, int>
    {
        [RoomEffectTint.None] = ClearedInk,
    };
}
