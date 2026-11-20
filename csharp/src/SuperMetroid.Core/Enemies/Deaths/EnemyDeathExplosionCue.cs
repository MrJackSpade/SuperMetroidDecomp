using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>One cue of an enemy's death explosion animations (#2554): what it does, with the values it acts on.</summary>
internal abstract record EnemyDeathExplosionCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EnemyDeathExplosionCue()
    {
    }

    /// <summary>Spawns an effect actor at a random place within the spread around the explosion.</summary>
    /// <param name="EffectActor">The effect actor the cue spawns.</param>
    /// <param name="Spread">How far around the explosion it may land.</param>
    internal sealed record SpawnEffectActor(RoomEffectActorKind EffectActor, EnemyDeathSpriteSpread Spread) : EnemyDeathExplosionCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(EnemyDeathExplosionCueKind Kind) : EnemyDeathExplosionCue;
}
