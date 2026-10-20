using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Areas.Escape;

/// <summary>Semantic weighted choices from the bank-$8F escape explosion selector.</summary>
internal static class ZebesEscapeExplosionDefinitions
{
    /// <summary>Small-explosion library-two sound $24 from <c>ExplosionSoundEffects</c> at $8F:C1DE.</summary>
    private const byte SmallExplosionSound = 0x24;
    /// <summary>Smoke library-two sound $25 from <c>ExplosionSoundEffects</c> at $8F:C1DE.</summary>
    private const byte SmokeSound = 0x25;

    /// <summary>
    /// Decodes the native low-three-bit choice: two small explosions, one small dust
    /// cloud, two smoke clouds, two short big dust clouds and one big dust cloud.
    /// Only the first small-explosion and smoke choices carry a sound.
    /// </summary>
    /// <remarks>The bucket weights and sound-bearing choices are authored effect variety (see
    /// residualScalarInputsReview).</remarks>
    internal static ZebesEscapeExplosionDefinition ForIndex(int index) => index switch
    {
        0 or 1 => new(RoomSpriteObjectKind.SporeSpawnDyingExplosion,
            index == 0 ? SmallExplosionSound : (byte)0),
        2 => new(RoomSpriteObjectKind.BotwoonSmallExplosion, 0),
        3 or 4 => new(RoomSpriteObjectKind.Smoke, index == 3 ? SmokeSound : (byte)0),
        5 or 6 => new(RoomSpriteObjectKind.ShortBigDustCloud, 0),
        7 => new(RoomSpriteObjectKind.DustCloud, 0),
        _ => throw new InvalidDataException(
            $"Zebes escape explosion index {index} is outside eight definitions."),
    };
}
