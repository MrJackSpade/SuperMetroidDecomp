using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One live actor of the scene over Ceres (#2489): a persistent actor behind the explosion
/// (<see cref="CeresDestructionActor"/>) or one of the station's blasts (<see cref="CeresBlast"/>), each moving its own way.
/// </summary>
/// <param name="sprite">The cinematic sprite showing the actor: its position and animation.</param>
internal abstract class CeresSceneActor(CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite)
{
    /// <summary>The cinematic sprite showing the actor: its position and animation.</summary>
    internal CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> Sprite { get; } = Ensure.NotNull(sprite);

    /// <summary>Whether the scene draws the actor after every other actor, so the others overlap it.</summary>
    internal abstract bool DrawnLast { get; }

    /// <summary>Moves the actor for this update, before its animation steps.</summary>
    internal abstract void Move();
}
