using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The pointer and definition of one intro mother brain definitions actor.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct IntroMotherBrainDefinitionsActor(ushort Pointer, IntroMotherBrainActorDefinition Definition);
