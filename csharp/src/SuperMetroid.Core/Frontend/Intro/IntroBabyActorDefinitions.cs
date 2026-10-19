using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for the SR388 egg and baby-Metroid scenes.</summary>
internal static class IntroBabyActorDefinitions
{

    /// <summary><c>$8B:CE5B/$A8D5</c>, the intact Metroid egg at (112,155).</summary>
    public static IntroBabyActorDefinition Egg =>
        new(CinematicSpritePreInstruction.MetroidEgg, CinematicSpriteProgram.MetroidEgg, new(0x0070, 0x009b), 0x0e00);

    /// <summary><c>$8B:CE61/$AD55</c>, the delivered baby Metroid at (84,139).</summary>
    public static IntroBabyActorDefinition DeliveredBaby =>
        new(CinematicSpritePreInstruction.BabyMetroidBeingDelivered, CinematicSpriteProgram.BabyMetroidBeingDelivered, new(0x0054, 0x008b), 0x0c00);

    /// <summary><c>$8B:CE67/$AD93</c>, the examined baby Metroid at (112,111).</summary>
    public static IntroBabyActorDefinition ExaminedBaby =>
        new(CinematicSpritePreInstruction.BabyMetroidBeingExamined, CinematicSpriteProgram.BabyMetroidBeingExamined, new(0x0070, 0x006f), 0x0c00);

    /// <summary><c>$8B:CE79/$BA4B</c>, the newly hatched confused baby at (112,155).</summary>
    public static IntroBabyActorDefinition ConfusedBaby =>
        new(CinematicSpritePreInstruction.ConfusedBabyWaitingForHatch, CinematicSpriteProgram.ConfusedBabyMetroid, new(0x0070, 0x009b), 0x0e00);
}
