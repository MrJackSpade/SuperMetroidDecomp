using SuperMetroid.Core.Enemies.EffectActors;

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
        new(RoomEffectActorKind.SmallExplosion, SmallExplosionSound);

    /// <summary>A small explosion that makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SilentSmallExplosion =
        new(RoomEffectActorKind.SmallExplosion, null);

    /// <summary>A small dust cloud, which makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SmallDustCloud =
        new(RoomEffectActorKind.SmokePuff, null);

    /// <summary>A smoke cloud that sounds the big-explosion sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SoundingSmoke =
        new(RoomEffectActorKind.Smoke, SmokeSound);

    /// <summary>A smoke cloud that makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition SilentSmoke =
        new(RoomEffectActorKind.Smoke, null);

    /// <summary>A short big dust cloud, which makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition ShortBigDustCloud =
        new(RoomEffectActorKind.ShortBigDustCloud, null);

    /// <summary>A big dust cloud, which makes no sound.</summary>
    internal static readonly ZebesEscapeExplosionDefinition BigDustCloud =
        new(RoomEffectActorKind.BigDustCloud, null);

    /// <summary>
    /// The explosion's draw (#2499): its sixteen equally likely choices (<see cref="ZebesEscapeDefinitions.ExplosionChoice"/>)
    /// name, in order, two small explosions, one small dust cloud, two smoke clouds, two short big dust clouds and one big dust
    /// cloud, of which only the first small explosion and the first smoke cloud sound, and then eight that leave the kind to
    /// the explosion's place.
    /// </summary>
    internal static readonly WeightedRandomChoice<ZebesEscapeExplosionDraw> Draw = new(ZebesEscapeDefinitions.ExplosionChoice,
        new(1, new ZebesEscapeExplosionDraw.Named(SoundingSmallExplosion)),
        new(1, new ZebesEscapeExplosionDraw.Named(SilentSmallExplosion)),
        new(1, new ZebesEscapeExplosionDraw.Named(SmallDustCloud)),
        new(1, new ZebesEscapeExplosionDraw.Named(SoundingSmoke)),
        new(1, new ZebesEscapeExplosionDraw.Named(SilentSmoke)),
        new(1, new ZebesEscapeExplosionDraw.Named(ShortBigDustCloud)),
        new(1, new ZebesEscapeExplosionDraw.Named(ShortBigDustCloud)),
        new(1, new ZebesEscapeExplosionDraw.Named(BigDustCloud)),
        new(8, ZebesEscapeExplosionDraw.LeftOpen.Instance));

    /// <summary>The kind an indoor explosion shows when its draw leaves the kind open: a small explosion.</summary>
    internal static readonly RoomEffectActorKind IndoorKindLeftOpen = SilentSmallExplosion.SpriteKind;

    /// <summary>
    /// The kinds an outdoor explosion shows when its draw leaves the kind open, by the column phase of the block it goes off
    /// over, the phases repeating every four columns: a small explosion, a small dust cloud, a smoke cloud and a short big
    /// dust cloud.
    /// </summary>
    internal static readonly RoomEffectActorKind[] OutdoorKindsLeftOpenByColumnPhase =
    [
        SilentSmallExplosion.SpriteKind,
        SmallDustCloud.SpriteKind,
        SilentSmoke.SpriteKind,
        ShortBigDustCloud.SpriteKind,
    ];
}
