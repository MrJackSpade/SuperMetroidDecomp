using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The global gameplay freeze word (WRAM <c>$0A78</c>) and the effective freeze including X-ray's.
/// </summary>
internal sealed class GameplayTimeFreeze
{
    private readonly SamusFrameState _player;

    internal GameplayTimeFreeze(SamusFrameState player)
    {
        _player = player;
    }

    /// <summary>
    /// Host ownership of shared WRAM <c>$0A78</c> outside X-ray. Reserve auto-refill and
    /// fatal-damage states set this while still calling portions of state-eight gameplay;
    /// every enemy/projectile/scroll consumer must observe the same native word.
    /// </summary>
    public bool GameplayTimeFrozen { get; set; }

    /// <summary>The effective global freeze word, including X-ray's translated owner.</summary>
    public bool TimeIsFrozen => GameplayTimeFrozen || (_player.Samus?.Xray.TimeIsFrozen ?? false);
}
