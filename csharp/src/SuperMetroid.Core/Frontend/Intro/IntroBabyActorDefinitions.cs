using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-actor definitions for the SR388 egg and baby-Metroid scenes.</summary>
internal static partial class IntroBabyActorDefinitions
{

    /// <summary>The intact Metroid egg in its place on the floor.</summary>
    public static IntroBabyActorDefinition Egg =>
        new(IntroBabyAnimation.Egg, new(112, 155), IntroSpriteColouring.DiscoveryActors);

    /// <summary>The delivered baby Metroid in its container.</summary>
    public static IntroBabyActorDefinition DeliveredBaby =>
        new(IntroBabyAnimation.DeliveredBaby, new(84, 139), IntroSpriteColouring.CrossFadeActors);

    /// <summary>The examined baby Metroid, held up for study.</summary>
    public static IntroBabyActorDefinition ExaminedBaby =>
        new(IntroBabyAnimation.ExaminedBaby, new(112, 111), IntroSpriteColouring.CrossFadeActors);

    /// <summary>The newly hatched confused baby, where the egg lay.</summary>
    public static IntroBabyActorDefinition ConfusedBaby =>
        new(IntroBabyAnimation.ConfusedBaby, new(112, 155), IntroSpriteColouring.DiscoveryActors);
}
