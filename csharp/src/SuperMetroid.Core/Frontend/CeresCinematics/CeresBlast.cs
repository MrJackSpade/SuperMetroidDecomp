using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One of the station's blasts (#2489): a wave's small or large blast or the station blast, drifting with the fleeing
/// gunship until its animation ends it.
/// </summary>
/// <param name="sprite">The cinematic sprite showing the blast: its position and animation.</param>
internal sealed class CeresBlast(CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite) : CeresSceneActor(sprite)
{
    /// <inheritdoc/>
    internal override bool DrawnLast => false;

    /// <summary>Drifts the blast with the fleeing gunship's velocity.</summary>
    internal override void Move() => Sprite.Position = new(
        Sprite.Position.X + CeresDestructionSceneDefinitions.Motion.GunshipXVelocity,
        Sprite.Position.Y + CeresDestructionSceneDefinitions.Motion.GunshipYVelocity);
}
