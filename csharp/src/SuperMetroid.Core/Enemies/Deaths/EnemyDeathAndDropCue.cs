using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>One cue of an enemy's death explosion and drop animations: what it does, with the values it acts on.</summary>
internal abstract partial record EnemyDeathAndDropCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EnemyDeathAndDropCue()
    {
    }

    /// <summary>Spawns an effect actor at a random place within the spread around the explosion.</summary>
    /// <param name="EffectActor">The effect actor the cue spawns.</param>
    /// <param name="Spread">How far around the explosion it may land.</param>
    internal sealed partial record SpawnEffectActor(RoomEffectActorKind EffectActor, EnemyDeathSpriteSpread Spread) : EnemyDeathAndDropCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(EnemyDeathAndDropCueKind Kind) : EnemyDeathAndDropCue;
}
