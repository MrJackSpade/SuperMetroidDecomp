using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One live actor of the Ceres approach's rear view: what it shows and the sprite showing it.</summary>
/// <param name="Role">What the actor shows, which selects its definition and installed placement.</param>
/// <param name="Sprite">The cinematic sprite showing the actor: its position and animation.</param>
internal sealed record CeresRearViewActor(CeresRearViewActorRole Role, IntroDiscoverySprite Sprite);
