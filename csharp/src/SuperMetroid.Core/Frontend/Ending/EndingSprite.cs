using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A live ending actor with its role and its own motion: a picture actor (<see cref="EndingPictureSprite"/>) or a typed
/// text (<see cref="EndingTextSprite"/>), both running animations that publish the escape scenes' cues (#2195, #2197).
/// </summary>
internal abstract class EndingSprite
{
    /// <summary>Creates an actor of <paramref name="role"/>, starting the motion its role names (#2489).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The role is undefined.</exception>
    protected EndingSprite(EndingSpriteRole role)
    {
        Role = role;
        Definition = EndingSpriteRoleDefinitions.Roles.TryGetValue(role, out EndingSpriteRoleDefinition? definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(role), role, "Undefined ending sprite role.");
        Motion = Definition.Motion?.Invoke();
    }

    /// <summary>What the actor shows.</summary>
    public EndingSpriteRole Role { get; }

    /// <summary>What the actor's role draws from, the sprite layer it shows on and the motion it starts with.</summary>
    public EndingSpriteRoleDefinition Definition { get; }

    /// <summary>How the actor moves, with whatever state its motion keeps (#2489), or null for one with no motion of its own.</summary>
    public EndingActorMotion? Motion { get; }

    /// <summary>The actor: where it stands, whether it is alive, and its animation's next update.</summary>
    public abstract ICinematicActor<EndingSceneCue> Sprite { get; }

    /// <summary>
    /// Draws what the actor shows from <paramref name="art"/> without advancing it: a picture from its family's
    /// presentation, or a typed text's letters.
    /// </summary>
    internal abstract void Draw(ScreenSprites screenSprites, EndingSpriteArt art);
}
