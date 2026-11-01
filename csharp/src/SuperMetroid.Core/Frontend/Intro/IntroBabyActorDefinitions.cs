namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for the SR388 egg and baby-Metroid scenes.</summary>
internal static class IntroBabyActorDefinitions
{

    /// <summary>The intact Metroid egg at (112,155).</summary>
    public static IntroBabyActorDefinition Egg =>
        new(CinematicSpriteFrameBehavior.MetroidEgg, CinematicSpriteProgram.MetroidEgg, new(112, 155), ScreenColourGroup.IntroDiscoveryActors);

    /// <summary>The delivered baby Metroid at (84,139).</summary>
    public static IntroBabyActorDefinition DeliveredBaby =>
        new(CinematicSpriteFrameBehavior.BabyMetroidBeingDelivered, CinematicSpriteProgram.BabyMetroidBeingDelivered, new(84, 139), ScreenColourGroup.IntroCrossFadeObjects);

    /// <summary>The examined baby Metroid at (112,111).</summary>
    public static IntroBabyActorDefinition ExaminedBaby =>
        new(CinematicSpriteFrameBehavior.BabyMetroidBeingExamined, CinematicSpriteProgram.BabyMetroidBeingExamined, new(112, 111), ScreenColourGroup.IntroCrossFadeObjects);

    /// <summary>The newly hatched confused baby at (112,155).</summary>
    public static IntroBabyActorDefinition ConfusedBaby =>
        new(CinematicSpriteFrameBehavior.ConfusedBabyWaitingForHatch, CinematicSpriteProgram.ConfusedBabyMetroid, new(112, 155), ScreenColourGroup.IntroDiscoveryActors);
}
