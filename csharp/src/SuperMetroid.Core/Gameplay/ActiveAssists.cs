using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The assists a gameplay runtime plays under (<see cref="Current"/>), and the owner of the two that change saved
/// progress: the granted loadout and the encounter reset applied when a save is loaded.
/// </summary>
internal sealed class ActiveAssists
{
    /// <summary>The current Samus, who receives the granted loadout.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The item, event, boss and door progress the loadout and the encounter reset change.</summary>
    private readonly SharedGameState _system;
    /// <summary>The Samus already granted the loadout, so later ammunition use and damage are not refilled.</summary>
    private SamusState? _loadoutRecipient;

    /// <summary>Starts a runtime's assists over its player and progress.</summary>
    /// <param name="player">The current Samus's owner.</param>
    /// <param name="system">The session's saved progress.</param>
    /// <param name="assists">The assists the runtime starts with.</param>
    internal ActiveAssists(SamusFrameState player, SharedGameState system, GameplayAssists assists)
    {
        _player = Ensure.NotNull(player);
        _system = Ensure.NotNull(system);
        Current = Ensure.NotNull(assists);
    }

    /// <summary>The assists the runtime currently plays under.</summary>
    public GameplayAssists Current { get; private set; }

    /// <summary>
    /// Changes the assists in the middle of play. A loadout newly granted reaches the current Samus at once, even if an
    /// earlier grant already reached her; nothing else about the game changes.
    /// </summary>
    /// <param name="assists">The assists to play under from now on.</param>
    internal void Rebind(GameplayAssists assists)
    {
        Ensure.NotNull(assists);
        if (Current.GrantedLoadout is null && assists.GrantedLoadout is not null)
            _loadoutRecipient = null;
        Current = assists;
        GrantLoadoutToNewSamus();
    }

    /// <summary>
    /// Forgets the saved boss, event and door-guardian defeats <see cref="GameplayAssists.EncounterResetOnLoad"/> names.
    /// Runs after a save's progress is restored and before any room, enemy or room object is chosen from it; room
    /// transitions never run it.
    /// </summary>
    internal void ResetSavedEncounters()
    {
        if (Current.EncounterResetOnLoad is not { } reset) return;
        foreach (AreaId area in reset.BossAreas)
            _system.ForgetBossesDefeated(area, DefeatedBossKinds.AllKnown);
        foreach (StoryEvent eventNumber in reset.ClearedEvents)
            _system.ClearEvent(eventNumber);
        foreach (LockedDoor door in reset.ClearedDoors)
            _system.Progress.OpenedDoors.Remove(door);
    }

    /// <summary>Carries the grant over to <paramref name="replacement"/> when it replaces the Samus already granted, so the grant is not repeated.</summary>
    internal void CarryLoadoutGrant(SamusState carried, SamusState replacement)
    {
        if (ReferenceEquals(_loadoutRecipient, carried))
            _loadoutRecipient = replacement;
    }

    /// <summary>Grants the loadout to the current Samus once; a Samus already granted keeps her later damage, ammunition use and equipment choices.</summary>
    internal void GrantLoadoutToNewSamus()
    {
        if (_player.Samus is { } samus && !ReferenceEquals(_loadoutRecipient, samus))
            GrantLoadout(samus);
    }

    /// <summary>Grants the loadout to the current Samus whose inventory a save has just restored, every time.</summary>
    internal void GrantLoadoutToRestoredSamus()
    {
        if (_player.Samus is { } samus)
            GrantLoadout(samus);
    }

    /// <summary>Fills <paramref name="samus"/>'s inventory from the granted loadout, if one is granted, and loads her suit's colours.</summary>
    private void GrantLoadout(SamusState samus)
    {
        if (Current.GrantedLoadout is not { } loadout) return;
        _loadoutRecipient = samus;
        samus.CollectedItems = samus.EquippedItems = loadout.Equipment;
        samus.CollectedBeams = loadout.CollectedBeams;
        samus.EquippedBeams = loadout.EquippedBeams;
        samus.Health = samus.MaxHealth = loadout.Energy;
        samus.ReserveEnergy = samus.MaxReserveEnergy = loadout.ReserveEnergy;
        samus.ReserveTankMode = loadout.ReserveTankMode;
        samus.Missiles = samus.MaxMissiles = loadout.Missiles;
        samus.SuperMissiles = samus.MaxSuperMissiles = loadout.SuperMissiles;
        samus.PowerBombs = samus.MaxPowerBombs = loadout.PowerBombs;
        if (loadout.CollectsEveryItemLocation)
            _system.Progress.CollectedItems.AddAll();
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, samus.EquippedItems);
    }
}
