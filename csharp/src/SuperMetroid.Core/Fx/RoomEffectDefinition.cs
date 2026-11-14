using System.Collections.Immutable;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One room effect a room shows (#1415, #1526, #1695, #1740): its kind, how its overlay blends, the visor's animation, the
/// liquid it fills the room with and how that liquid's level moves, the colour effects and block animations it starts, and
/// the palette blend that tints its overlay.
/// </summary>
/// <remarks>Its room's effect selection (<see cref="RoomFxSelection"/>) names the entry door that selects it, or holds it as its default (#1546).
/// The liquid's levels are room lines from the room's top, so a rising liquid's level falls.</remarks>
/// <param name="LiquidStartLevel">The line the liquid's surface starts at, or null when the effect's liquid has no surface.</param>
/// <param name="LiquidTargetLevel">The line the liquid's surface rises or falls to, or null when its level never moves.</param>
/// <param name="LiquidLevelSpeed">How fast the liquid's level moves toward its target once it starts, signed: negative rises.</param>
/// <param name="LiquidMoveDelay">The updates the liquid waits, once its level has somewhere to go, before it starts to move.</param>
/// <param name="Type">The kind of effect the room shows.</param>
/// <param name="VisorAnimation">Whether the room animates Samus's visor.</param>
/// <param name="EffectBlend">How the effect's overlay blends with the room, or null when the effect shows no overlay (#1545).</param>
/// <param name="Liquid">The liquid the effect fills the room with, or null for an effect with none.</param>
/// <param name="PaletteEffects">The colour effects the room effect starts, in the order they start.</param>
/// <param name="BlockAnimations">The block animations the room effect starts, in the order they start.</param>
/// <param name="PaletteBlend">The palette blend that tints the effect's overlay and the HUD.</param>
public sealed record RoomEffectDefinition(
    int? LiquidStartLevel,
    int? LiquidTargetLevel,
    SignedEightEight LiquidLevelSpeed,
    int LiquidMoveDelay,
    RoomFxType Type,
    RoomVisorAnimation VisorAnimation,
    RoomEffectBlend? EffectBlend,
    RoomLiquid? Liquid,
    ImmutableArray<PaletteFxObject> PaletteEffects,
    ImmutableArray<BlockAnimation> BlockAnimations,
    RoomFxPaletteBlend PaletteBlend)
{
    /// <summary>
    /// The liquid the effect fills its room with: <see cref="RoomWater"/> for water and the statue effect,
    /// <see cref="RoomLavaOrAcid"/> for lava and acid, and null for every other effect (#1695).
    /// </summary>
    /// <exception cref="ArgumentException">The liquid is not the kind the effect has.</exception>
    public RoomLiquid? Liquid { get; init; } = RoomLiquid.OfEffect(Type, Liquid);
}
