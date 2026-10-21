using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The actor and native pointer of one intro rinka definitions actor.</summary>
/// <param name="Actor">The actor.</param>
/// <param name="NativePointer">The native pointer.</param>
internal readonly record struct IntroRinkaDefinitionsActor(CinematicActorDefinition Actor, ushort NativePointer);
