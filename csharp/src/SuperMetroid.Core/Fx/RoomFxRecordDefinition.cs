using System.Collections.Immutable;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One room-FX record, its fields decoded at import into the effect, blends, liquid options, the palette effects and
/// animated tiles it starts, and the palette blend they select (#1415, #1526).
/// </summary>
/// <remarks><c>Door</c> is the door the record applies to, or null for the list's default record or its terminator;
/// <c>Terminates</c> ends the list without selecting FX. <c>BaseYPosition</c> is the liquid surface's starting height,
/// or null when the record names none and the surface sits below every room; <c>TargetYPosition</c> is the height the
/// surface moves to, or null when the record names none and the surface never moves; <c>YVelocity</c> is the signed
/// speed it moves at. <c>PaletteEffects</c> and <c>AnimatedTiles</c> are the effects the record starts, in the order
/// they start. Two records are equal when every field is, the effect lists compared in order.</remarks>
public sealed record RoomFxRecordDefinition(
    DoorDefinition? Door,
    bool Terminates,
    int? BaseYPosition,
    int? TargetYPosition,
    SignedEightEight YVelocity,
    int Timer,
    RoomFxType Type,
    LayerBlendingConfiguration DefaultLayerBlend,
    LayerBlendingConfiguration EffectLayerBlend,
    RoomLiquidOptions LiquidOptions,
    ImmutableArray<PaletteFxObject> PaletteEffects,
    ImmutableArray<AnimatedTileObject> AnimatedTiles,
    RoomFxPaletteBlend PaletteBlend)
{
    /// <summary>The palette effects the record starts, in the order they start.</summary>
    /// <exception cref="ArgumentException">The list is uninitialized.</exception>
    public ImmutableArray<PaletteFxObject> PaletteEffects { get; init; } = PaletteEffects.IsDefault
        ? throw new ArgumentException("A room-FX record's palette effects must be a list.", nameof(PaletteEffects))
        : PaletteEffects;

    /// <summary>The animated-tile objects the record starts, in the order they start.</summary>
    /// <exception cref="ArgumentException">The list is uninitialized.</exception>
    public ImmutableArray<AnimatedTileObject> AnimatedTiles { get; init; } = AnimatedTiles.IsDefault
        ? throw new ArgumentException("A room-FX record's animated tiles must be a list.", nameof(AnimatedTiles))
        : AnimatedTiles;

    /// <summary>Whether both records hold the same fields, their effect lists in the same order.</summary>
    public bool Equals(RoomFxRecordDefinition? other) =>
        other is not null && Equals(Door, other.Door) && Terminates == other.Terminates &&
        BaseYPosition == other.BaseYPosition && TargetYPosition == other.TargetYPosition &&
        YVelocity.Equals(other.YVelocity) && Timer == other.Timer && Type == other.Type &&
        DefaultLayerBlend == other.DefaultLayerBlend && EffectLayerBlend == other.EffectLayerBlend &&
        LiquidOptions == other.LiquidOptions && PaletteEffects.SequenceEqual(other.PaletteEffects) &&
        AnimatedTiles.SequenceEqual(other.AnimatedTiles) && PaletteBlend == other.PaletteBlend;

    /// <summary>Hashes what <see cref="Equals(RoomFxRecordDefinition)"/> compares.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Door);
        hash.Add(Terminates);
        hash.Add(BaseYPosition);
        hash.Add(TargetYPosition);
        hash.Add(YVelocity);
        hash.Add(Timer);
        hash.Add(Type);
        hash.Add(DefaultLayerBlend);
        hash.Add(EffectLayerBlend);
        hash.Add(LiquidOptions);
        foreach (PaletteFxObject effect in PaletteEffects)
            hash.Add(effect);
        foreach (AnimatedTileObject tiles in AnimatedTiles)
            hash.Add(tiles);
        hash.Add(PaletteBlend);
        return hash.ToHashCode();
    }
}
