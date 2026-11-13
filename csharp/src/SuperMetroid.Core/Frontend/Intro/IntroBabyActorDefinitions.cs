using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for the SR388 egg and baby-Metroid scenes.</summary>
internal static class IntroBabyActorDefinitions
{

    /// <summary>The intact Metroid egg at (112,155).</summary>
    public static IntroBabyActorDefinition Egg =>
        new(CinematicSpriteFrameBehavior.MetroidEgg, CinematicSpriteSegment.MetroidEgg, new(112, 155), IntroSpriteColouring.DiscoveryActors);

    /// <summary>The delivered baby Metroid at (84,139).</summary>
    public static IntroBabyActorDefinition DeliveredBaby =>
        new(CinematicSpriteFrameBehavior.BabyMetroidBeingDelivered, CinematicSpriteSegment.BabyMetroidBeingDelivered, new(84, 139), IntroSpriteColouring.CrossFadeObjects);

    /// <summary>The examined baby Metroid at (112,111).</summary>
    public static IntroBabyActorDefinition ExaminedBaby =>
        new(CinematicSpriteFrameBehavior.BabyMetroidBeingExamined, CinematicSpriteSegment.BabyMetroidBeingExamined, new(112, 111), IntroSpriteColouring.CrossFadeObjects);

    /// <summary>The newly hatched confused baby at (112,155).</summary>
    public static IntroBabyActorDefinition ConfusedBaby =>
        new(CinematicSpriteFrameBehavior.ConfusedBabyWaitingForHatch, CinematicSpriteSegment.ConfusedBabyMetroid, new(112, 155), IntroSpriteColouring.DiscoveryActors);
}
