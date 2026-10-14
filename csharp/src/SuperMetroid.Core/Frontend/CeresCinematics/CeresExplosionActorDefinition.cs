using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
internal readonly record struct CeresExplosionActorDefinition(
    ushort PreInstruction,
    CinematicSpriteProgram Program);
