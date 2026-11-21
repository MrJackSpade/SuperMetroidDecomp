using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A live ending actor with its role and its own motion: a picture actor (<see cref="EndingPictureSprite"/>) or a typed
/// text (<see cref="EndingTextSprite"/>), both running animations that publish the escape scenes' cues (#2195, #2197).
/// </summary>
internal abstract partial class EndingSprite
{
    /// <summary>Creates an actor of <paramref name="role"/>, starting the motion its role names (#2489).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The role is undefined.</exception>
    protected EndingSprite(EndingSpriteRole role)
    {
        Role = role;
        Definition = EndingSpriteRoleDefinitions.Roles[role];
        Motion = Definition.Motion();
    }

    /// <summary>What the actor shows.</summary>
    public EndingSpriteRole Role { get; }

    /// <summary>What the actor's role draws from, the sprite layer it shows on and the motion it starts with.</summary>
    public EndingSpriteRoleDefinition Definition { get; }

    /// <summary>How the actor moves, with whatever state its motion keeps (#2489); an actor with no motion of its own stands still (#2541).</summary>
    internal EndingActorMotion Motion { private get; set; }

    /// <summary>The actor: where it stands, whether it is alive, and its animation's next update.</summary>
    public abstract ICinematicActor<EndingSceneCue> Sprite { get; }

    /// <summary>Runs the actor's own motion for this update in <paramref name="scene"/>, before its animation steps (#2489).</summary>
    internal void StepMotion(IEndingActorScene scene) => Motion.Step(Sprite, scene);

    /// <summary>
    /// Draws what the actor shows from <paramref name="art"/> without advancing it: a picture from its family's
    /// presentation, or a typed text's letters.
    /// </summary>
    internal abstract void Draw(ScreenSprites screenSprites, EndingSpriteArt art);
}
