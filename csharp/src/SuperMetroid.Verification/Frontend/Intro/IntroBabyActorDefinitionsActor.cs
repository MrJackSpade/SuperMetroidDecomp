using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The actor and definition pointer of one intro baby actor definitions actor.</summary>
/// <param name="Actor">The actor.</param>
/// <param name="DefinitionPointer">The definition pointer.</param>
internal readonly record struct IntroBabyActorDefinitionsActor(IntroBabyActorDefinition Actor, ushort DefinitionPointer);
