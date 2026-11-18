using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One persistent actor behind Ceres's explosion (#1768, #2489): what it shows, its definition and the sprite showing it.
/// </summary>
/// <param name="role">What the actor shows, which selects its definition and installed placement.</param>
/// <param name="definition">The actor's animation, colouring and motion.</param>
/// <param name="sprite">The cinematic sprite showing the actor: its position and animation.</param>
internal sealed class CeresDestructionActor(
    CeresDestructionActorRole role,
    CeresDestructionActorDefinition definition,
    CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite) : CeresSceneActor(sprite)
{

    /// <summary>The actor's animation, colouring and motion.</summary>
    internal CeresDestructionActorDefinition Definition { get; } = definition;

    /// <inheritdoc/>
    internal override bool DrawnLast => Definition.DrawnLast;

    /// <summary>Drifts the actor round its loop, or leaves it standing still when it drifts round none.</summary>
    internal override void Move()
    {
        if (Definition.DriftsRoundLoop)
            Sprite.DriftRoundLoop(Definition.HorizontalDelta);
    }
}
