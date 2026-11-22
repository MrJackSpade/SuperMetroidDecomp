using System.Collections.Frozen;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The room state each sound is heard in that plays an instrument only a music data set defines (#2603). The game plays
/// these sounds in that state alone, so their clips render with the instruments its music data set loads, as the
/// cartridge plays them: the shared sounds leave those instruments empty, and a render from them alone is silent where
/// they play. Every other sound renders from the shared sounds alone (#2057).
/// </summary>
internal static partial class SoundEffectRoomStateDefinitions
{
    /// <summary>
    /// Each such sound's room state: Phantoon appears only in his room before he is defeated, and Crocomire cries and melts
    /// only in his room before he is defeated.
    /// </summary>
    internal static readonly FrozenDictionary<SoundEffectId, RoomStateSettings> PlayedIn = new Dictionary<SoundEffectId, RoomStateSettings>
    {
        [EnemySound.PhantoonAppearsMiddle] = WreckedShipRoomStateDefinitions.PhantoonDefault,
        [EnemySound.PhantoonAppearsLow] = WreckedShipRoomStateDefinitions.PhantoonDefault,
        [EnemySound.PhantoonAppearsHigh] = WreckedShipRoomStateDefinitions.PhantoonDefault,
        [EnemySound.CrocomireCry] = NorfairRoomStateDefinitions.CrocomireDefault,
        [EnemySound.CrocomireMeltingCry] = NorfairRoomStateDefinitions.CrocomireDefault,
    }.ToFrozenDictionary();
}
