using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Escape;

/// <summary>The Zebes escape's explosions: the ones a random draw names, and the kinds a draw that names none leaves to the explosion's place.</summary>
internal static class ZebesEscapeExplosionDefinitions
{
    /// <summary>The small-explosion choice's sound, the shared small-explosion sound.</summary>
    private static readonly SoundEffectId SmallExplosionSound = EnemySound.SmallExplosion;
    /// <summary>The smoke choice's sound, the shared big-explosion sound.</summary>
    private static readonly SoundEffectId SmokeSound = EnemySound.BigExplosion;

    /// <summary>A small explosion that sounds the small-explosion sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SoundingSmallExplosion =
        new(RoomSpriteObjectKind.SporeSpawnDyingExplosion, SmallExplosionSound);

    /// <summary>A small explosion that makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SilentSmallExplosion =
        new(RoomSpriteObjectKind.SporeSpawnDyingExplosion, null);

    /// <summary>A small dust cloud, which makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SmallDustCloud =
        new(RoomSpriteObjectKind.BotwoonSmallExplosion, null);

    /// <summary>A smoke cloud that sounds the big-explosion sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SoundingSmoke =
        new(RoomSpriteObjectKind.Smoke, SmokeSound);

    /// <summary>A smoke cloud that makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SilentSmoke =
        new(RoomSpriteObjectKind.Smoke, null);

    /// <summary>A short big dust cloud, which makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition ShortBigDustCloud =
        new(RoomSpriteObjectKind.ShortBigDustCloud, null);

    /// <summary>A big dust cloud, which makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition BigDustCloud =
        new(RoomSpriteObjectKind.DustCloud, null);

    /// <summary>
    /// The eight equally likely explosions a draw that names one chooses from, in the draw's order: two small explosions,
    /// one small dust cloud, two smoke clouds, two short big dust clouds and one big dust cloud. Only the first small
    /// explosion and the first smoke cloud sound.
    /// </summary>
    /// <remarks>The bucket weights and sound-bearing choices are authored effect variety (see the residual scalar
    /// inputs review).</remarks>
    internal static readonly ZebesEscapeExplosionDefinition[] Drawn =
    [
        SoundingSmallExplosion,
        SilentSmallExplosion,
        SmallDustCloud,
        SoundingSmoke,
        SilentSmoke,
        ShortBigDustCloud,
        ShortBigDustCloud,
        BigDustCloud,
    ];

    /// <summary>The kind an indoor explosion shows when its draw leaves the kind open: a small explosion.</summary>
    internal static readonly RoomSpriteObjectKind IndoorKindLeftOpen = SilentSmallExplosion.SpriteKind;

    /// <summary>
    /// The kinds an outdoor explosion shows when its draw leaves the kind open, by the column phase of the block it goes off
    /// over, the phases repeating every four columns: a small explosion, a small dust cloud, a smoke cloud and a short big
    /// dust cloud.
    /// </summary>
    internal static readonly RoomSpriteObjectKind[] OutdoorKindsLeftOpenByColumnPhase =
    [
        SilentSmallExplosion.SpriteKind,
        SmallDustCloud.SpriteKind,
        SilentSmoke.SpriteKind,
        ShortBigDustCloud.SpriteKind,
    ];
}
