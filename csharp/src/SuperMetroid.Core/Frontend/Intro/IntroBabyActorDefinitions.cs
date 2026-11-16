using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-actor definitions for the SR388 egg and baby-Metroid scenes.</summary>
internal static class IntroBabyActorDefinitions
{

    /// <summary>The intact Metroid egg at (112,155).</summary>
    public static IntroBabyActorDefinition Egg =>
        new(IntroBabyAnimation.Egg, new(112, 155), IntroSpriteColouring.DiscoveryActors);

    /// <summary>The delivered baby Metroid at (84,139).</summary>
    public static IntroBabyActorDefinition DeliveredBaby =>
        new(IntroBabyAnimation.DeliveredBaby, new(84, 139), IntroSpriteColouring.CrossFadeActors);

    /// <summary>The examined baby Metroid at (112,111).</summary>
    public static IntroBabyActorDefinition ExaminedBaby =>
        new(IntroBabyAnimation.ExaminedBaby, new(112, 111), IntroSpriteColouring.CrossFadeActors);

    /// <summary>The newly hatched confused baby at (112,155).</summary>
    public static IntroBabyActorDefinition ConfusedBaby =>
        new(IntroBabyAnimation.ConfusedBaby, new(112, 155), IntroSpriteColouring.DiscoveryActors);
}
