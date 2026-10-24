using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Escape;

/// <summary>Semantic weighted choices from the bank-$8F escape explosion selector.</summary>
internal static class ZebesEscapeExplosionDefinitions
{
    /// <summary>The small-explosion choice's sound, from <c>ExplosionSoundEffects</c> at $8F:C1DE.</summary>
    private static readonly SoundEffectId SmallExplosionSound = Library2Sound.SmallExplosion;
    /// <summary>The smoke choice's sound, from <c>ExplosionSoundEffects</c> at $8F:C1DE.</summary>
    private static readonly SoundEffectId SmokeSound = Library2Sound.BigExplosion;

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
            index == 0 ? SmallExplosionSound : null),
        2 => new(RoomSpriteObjectKind.BotwoonSmallExplosion, null),
        3 or 4 => new(RoomSpriteObjectKind.Smoke, index == 3 ? SmokeSound : null),
        5 or 6 => new(RoomSpriteObjectKind.ShortBigDustCloud, null),
        7 => new(RoomSpriteObjectKind.DustCloud, null),
        _ => throw new InvalidDataException(
            $"Zebes escape explosion index {index} is outside eight definitions."),
    };
}
