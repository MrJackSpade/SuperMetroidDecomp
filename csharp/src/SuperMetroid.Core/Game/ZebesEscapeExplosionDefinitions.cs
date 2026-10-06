namespace SuperMetroid.Core.Game;

/// <summary>One random Zebes-escape explosion sprite and its optional sound.</summary>
internal readonly record struct ZebesEscapeExplosionDefinition(
    RoomSpriteObjectKind SpriteKind,
    byte SoundEffect);

/// <summary>Semantic weighted choices from the bank-$8F escape explosion selector.</summary>
internal static class ZebesEscapeExplosionDefinitions
{
    /// <summary>Smoke sprite object $0C in <c>ExplosionSpriteObjectIDs</c> at $8F:C1D6.</summary>
    private const RoomSpriteObjectKind Smoke = (RoomSpriteObjectKind)0x0c;
    /// <summary>Short big dust cloud $12 in <c>ExplosionSpriteObjectIDs</c> at $8F:C1D6.</summary>
    private const RoomSpriteObjectKind ShortBigDustCloud = (RoomSpriteObjectKind)0x12;
    /// <summary>Small-explosion library-two sound $24 from <c>ExplosionSoundEffects</c> at $8F:C1DE.</summary>
    private const byte SmallExplosionSound = 0x24;
    /// <summary>Smoke library-two sound $25 from <c>ExplosionSoundEffects</c> at $8F:C1DE.</summary>
    private const byte SmokeSound = 0x25;

    /// <summary>
    /// Decodes the native low-three-bit choice: two small explosions, one small dust
    /// cloud, two smoke clouds, two short big dust clouds and one big dust cloud.
    /// Only the first small-explosion and smoke choices carry a sound.
    /// </summary>
    // The bucket weights and sound-bearing choices are authored effect variety (see residualScalarInputsReview).
    internal static ZebesEscapeExplosionDefinition ForIndex(int index) => index switch
    {
        0 or 1 => new(RoomSpriteObjectKind.SporeSpawnDyingExplosion,
            index == 0 ? SmallExplosionSound : (byte)0),
        2 => new(RoomSpriteObjectKind.BotwoonSmallExplosion, 0),
        3 or 4 => new(Smoke, index == 3 ? SmokeSound : (byte)0),
        5 or 6 => new(ShortBigDustCloud, 0),
        7 => new(RoomSpriteObjectKind.DustCloud, 0),
        _ => throw new InvalidDataException(
            $"Zebes escape explosion index {index} is outside eight definitions."),
    };
}
