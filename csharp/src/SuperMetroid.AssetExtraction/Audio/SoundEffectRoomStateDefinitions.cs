using System.Collections.Frozen;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The room state each sound is heard in that plays an instrument a music data set defines (#2603) or redefines (#2627).
/// The game plays these sounds in that state alone, so their clips render with the instruments its music data set loads,
/// as the cartridge plays them: the shared sounds leave such an instrument empty, so a render from them alone is silent,
/// or hold another instrument in its place, so a render from them alone plays the wrong one. Every other sound renders
/// from the shared sounds alone (#2057).
/// </summary>
internal static partial class SoundEffectRoomStateDefinitions
{
    /// <summary>
    /// Each such sound's room state: Phantoon appears only in his room before he is defeated, Crocomire cries and melts
    /// only in his room before he is defeated, and Spore Spawn opens and is hit only in his room before he is defeated,
    /// whose music data set redefines the instrument his sound plays.
    /// </summary>
    internal static readonly FrozenDictionary<SoundEffectId, RoomStateSettings> PlayedIn = new Dictionary<SoundEffectId, RoomStateSettings>
    {
        [EnemySound.PhantoonAppearsMiddle] = WreckedShipRoomStateDefinitions.PhantoonDefault,
        [EnemySound.PhantoonAppearsLow] = WreckedShipRoomStateDefinitions.PhantoonDefault,
        [EnemySound.PhantoonAppearsHigh] = WreckedShipRoomStateDefinitions.PhantoonDefault,
        [EnemySound.CrocomireCry] = NorfairRoomStateDefinitions.CrocomireDefault,
        [EnemySound.CrocomireMeltingCry] = NorfairRoomStateDefinitions.CrocomireDefault,
        [EnemySound.ShotSporeSpawn] = BrinstarRoomStateDefinitions.SporeSpawnDefault,
    }.ToFrozenDictionary();
}
