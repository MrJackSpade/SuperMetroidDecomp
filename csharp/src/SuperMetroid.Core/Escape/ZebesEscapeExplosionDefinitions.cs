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
    /// The native low-three-bit choices, in index order: two small explosions, one small dust
    /// cloud, two smoke clouds, two short big dust clouds and one big dust cloud.
    /// Only the first small-explosion and smoke choices carry a sound.
    /// </summary>
    /// <remarks>The bucket weights and sound-bearing choices are authored effect variety (see
    /// residualScalarInputsReview).</remarks>
    internal static readonly ZebesEscapeExplosionDefinition[] Choices =
    [
        new(RoomSpriteObjectKind.SporeSpawnDyingExplosion, SmallExplosionSound),
        new(RoomSpriteObjectKind.SporeSpawnDyingExplosion, null),
        new(RoomSpriteObjectKind.BotwoonSmallExplosion, null),
        new(RoomSpriteObjectKind.Smoke, SmokeSound),
        new(RoomSpriteObjectKind.Smoke, null),
        new(RoomSpriteObjectKind.ShortBigDustCloud, null),
        new(RoomSpriteObjectKind.ShortBigDustCloud, null),
        new(RoomSpriteObjectKind.DustCloud, null),
    ];
}
