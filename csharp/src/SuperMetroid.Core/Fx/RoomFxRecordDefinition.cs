using System.Collections.Immutable;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One room-FX record, its fields decoded at import into the effect, its blend, the visor's animation, the liquid it fills
/// the room with, the palette effects and animated tiles it starts, and the palette blend they select (#1415, #1526, #1695).
/// </summary>
/// <remarks>Its room's effect selection (<see cref="RoomFxSelection"/>) names the entry door that selects it, or holds it as its default (#1546).
/// <c>BaseYPosition</c> is the liquid surface's starting height,
/// or null when the record names none and the liquid has no surface; <c>TargetYPosition</c> is the height the
/// surface moves to, or null when the record names none and the surface never moves; <c>YVelocity</c> is the signed
/// speed it moves at. <c>PaletteEffects</c> and <c>AnimatedTiles</c> are the effects the record starts, in the order
/// they start. <c>VisorAnimation</c> says whether the room animates Samus's visor, and <c>EffectBlend</c> how the effect's
/// overlay blends with the room, or null when the effect shows no overlay (#1545).</remarks>
public sealed record RoomFxRecordDefinition(
    int? BaseYPosition,
    int? TargetYPosition,
    SignedEightEight YVelocity,
    int Timer,
    RoomFxType Type,
    RoomVisorAnimation VisorAnimation,
    RoomEffectBlend? EffectBlend,
    RoomLiquid? Liquid,
    ImmutableArray<PaletteFxObject> PaletteEffects,
    ImmutableArray<AnimatedTileObject> AnimatedTiles,
    RoomFxPaletteBlend PaletteBlend)
{
    /// <summary>
    /// The liquid the effect fills its room with: <see cref="RoomWater"/> for water and the statue effect,
    /// <see cref="RoomLavaOrAcid"/> for lava and acid, and null for every other effect (#1695).
    /// </summary>
    /// <exception cref="ArgumentException">The liquid is not the kind the record's effect has.</exception>
    public RoomLiquid? Liquid { get; init; } = RoomLiquid.OfEffect(Type, Liquid);
}
