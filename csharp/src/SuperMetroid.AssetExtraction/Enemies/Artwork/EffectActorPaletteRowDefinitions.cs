using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The palette row the game ORs into every part of an effect actor's frames where its spawn names one (#2641); every other
/// effect actor's parts show their own rows.
/// </summary>
internal static partial class EffectActorPaletteRowDefinitions
{
    /// <summary>
    /// Draygon's sprite row, which his sprite colours, white flash and opening's colours are loaded into: the row he creates
    /// his Evirs in, in his opening and at his burial ($A5:A096-$A5:A137 pass palette index $0E00).
    /// </summary>
    private const int DraygonSpriteRow = 7;

    /// <summary>The row each effect actor kind's spawn ORs into its frames' parts.</summary>
    internal static IReadOnlyDictionary<RoomEffectActorKind, int> Rows { get; } = new Dictionary<RoomEffectActorKind, int>
    {
        [RoomEffectActorKind.EvirFacingLeft] = DraygonSpriteRow,
        [RoomEffectActorKind.EvirFacingRight] = DraygonSpriteRow,
    };
}
