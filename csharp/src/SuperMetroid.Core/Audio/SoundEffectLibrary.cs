namespace SuperMetroid.Core.Audio;

/// <summary>
/// The three sound libraries, each with its own request queue and its own sound playing at a time.
/// A sound belongs to exactly one library; the values are not combinable flags.
/// </summary>
/// <remarks>
/// Values are arbitrary and nothing reads them; they start above zero only so that the default sound identity names no
/// library. The libraries' queue order is <see cref="SoundEffectLibraries.InQueueOrder"/>, and the import pipeline alone
/// knows the game's numbering of them.
/// </remarks>
public enum SoundEffectLibrary
{
    /// <summary>Samus's library (<see cref="SamusSound"/>): her weapons, grapple, spins and scope, and the menus.</summary>
    Samus = 1,
    /// <summary>The enemies' library (<see cref="EnemySound"/>): enemies, bosses, drops, impacts and terrain.</summary>
    Enemies,
    /// <summary>The room's library (<see cref="RoomSound"/>): doors, gates and elevators, Samus's footing, and the low-health beep.</summary>
    Room,
}
