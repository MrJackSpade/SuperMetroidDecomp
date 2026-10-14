namespace SuperMetroid.Core.Game;

/// <summary>One native callback word and its translated semantic identity, when supported.</summary>
internal readonly record struct SamusBeamCallbackDefinition(
    ushort NativePointer,
    SamusProjectilePreInstruction? Translated);
