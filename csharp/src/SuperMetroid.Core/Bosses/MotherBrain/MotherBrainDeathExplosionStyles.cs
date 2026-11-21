using SuperMetroid.Core.Enemies.Projectiles;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What each style of Mother Brain's death explosions does.</summary>
internal static partial class MotherBrainDeathExplosionStyles
{
    /// <summary>
    /// Each style: the smoky style bursts a group's smoky pair in smoke at its own interval; the mixed style bursts the
    /// whole group, each explosion's animation drawn at random, at the mixed interval.
    /// </summary>
    private static readonly IdentityTable<MotherBrainDeathExplosionStyle, MotherBrainDeathExplosionStyleDefinition> Styles =
        new(new Dictionary<MotherBrainDeathExplosionStyle, MotherBrainDeathExplosionStyleDefinition>
        {
            [MotherBrainDeathExplosionStyle.Smoky] = new(MotherBrainDeathExplosionDefinitions.SmokyIntervalUpdates,
                group => group.Smoky, _ => DustExplosionAnimation.Smoke),
            [MotherBrainDeathExplosionStyle.Mixed] = new(MotherBrainDeathExplosionDefinitions.MixedIntervalUpdates,
                group => group.Mixed, MotherBrainRandomDefinitions.MixedDeathExplosionAnimation.Choose),
        });

    /// <summary>The definition of <paramref name="style"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="style"/> is undefined.</exception>
    internal static MotherBrainDeathExplosionStyleDefinition Of(MotherBrainDeathExplosionStyle style) => Styles[style];
}
