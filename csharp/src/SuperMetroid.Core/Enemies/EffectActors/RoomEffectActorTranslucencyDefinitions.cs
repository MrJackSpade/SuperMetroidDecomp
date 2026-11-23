namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// The room effect actors the game flickers on and off every other update to show them see-through, which the port draws
/// steadily at half strength instead (#2642): a Metroid's shell, whose flicker a display whose refresh does not match the
/// game's showed as the shell pulsing in and out of sight. Their animations still run as the game's do.
/// </summary>
internal static partial class RoomEffectActorTranslucencyDefinitions
{
    /// <summary>The effect actors drawn translucent.</summary>
    internal static IReadOnlySet<RoomEffectActorKind> Translucent { get; } = new HashSet<RoomEffectActorKind> { RoomEffectActorKind.MetroidShell };
}
