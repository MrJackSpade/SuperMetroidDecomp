using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>What a save station, checkpoint or save editor hands a save slot to keep: Samus, options and progress.</summary>
public sealed record SuperMetroidSaveSnapshot
{
    /// <summary>The seven configurable actions as a validated permutation of SNES button masks.</summary>
    public ControllerBindings ControllerBindings { get; init; } = ControllerBindings.Default;

    /// <summary>Whether the options menu's moonwalk behavior is enabled.</summary>
    public bool MoonwalkEnabled { get; init; }
    /// <summary>The checksummed cartridge debug value, carried unchanged; a new file holds one.</summary>
    public int DebugFlag { get; init; } = 1;
    /// <summary>The checksummed new-file marker, carried unchanged; a new file holds one.</summary>
    public int NewFileMarker { get; init; } = 1;

    /// <summary>Whether door transitions automatically clear the selected HUD item.</summary>
    public bool IconCancelEnabled { get; init; }

    /// <summary>The persistent excess-missile accumulator written to WRAM <c>$09D8</c>.</summary>
    public int ReserveMissiles { get; init; }

    /// <summary>Whether the alternate Japanese text setting written to WRAM <c>$09E2</c> is enabled.</summary>
    public bool JapaneseText { get; init; }

    /// <summary>
    /// The cumulative item-PLM setup count. Gameplay wraps it to zero after <see cref="SaveFieldValue.MaximumValue"/>;
    /// the snapshot only carries the value it reached.
    /// </summary>
    public int LoadedItemCount { get; init; }

    /// <summary>Bit mask of collected items currently enabled on Samus.</summary>
    public SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Bit mask of item upgrades Samus has collected, including unequipped upgrades.</summary>
    public SamusEquipmentFlags CollectedItems { get; init; }

    /// <summary>Bit mask of collected beam upgrades currently enabled on Samus.</summary>
    public SamusBeamFlags EquippedBeams { get; init; }

    /// <summary>Bit mask of all beam upgrades Samus has collected.</summary>
    public SamusBeamFlags CollectedBeams { get; init; }

    /// <summary>The native reserve-tank mode word; zero is unset, one is auto, and two is manual.</summary>
    public ReserveTankMode ReserveMode { get; init; }

    /// <summary>Samus's current energy in energy-point units.</summary>
    public int Health { get; init; } = 99;

    /// <summary>Samus's energy capacity in energy-point units.</summary>
    public int MaxHealth { get; init; } = 99;

    /// <summary>The current ordinary missile count.</summary>
    public int Missiles { get; init; }

    /// <summary>The acquired ordinary missile capacity.</summary>
    public int MaxMissiles { get; init; }

    /// <summary>The current super missile count.</summary>
    public int SuperMissiles { get; init; }

    /// <summary>The acquired super missile capacity.</summary>
    public int MaxSuperMissiles { get; init; }

    /// <summary>The current power bomb count.</summary>
    public int PowerBombs { get; init; }

    /// <summary>The acquired power bomb capacity.</summary>
    public int MaxPowerBombs { get; init; }

    /// <summary>The native HUD selection index: beam, missile, super missile, power bomb, grapple, or X-ray.</summary>
    public HudItem HudItem { get; init; }

    /// <summary>The acquired reserve-tank capacity in energy-point units.</summary>
    public int MaxReserveEnergy { get; init; }

    /// <summary>The energy-point total currently stored in reserve tanks.</summary>
    public int ReserveEnergy { get; init; }

    /// <summary>The sub-second gameplay counter, normally in the inclusive range 0 through 59.</summary>
    public int GameTimeFrames { get; init; }

    /// <summary>The gameplay clock's seconds component, normally in the inclusive range 0 through 59.</summary>
    public int GameTimeSeconds { get; init; }

    /// <summary>The gameplay clock's minutes component, normally in the inclusive range 0 through 59.</summary>
    public int GameTimeMinutes { get; init; }

    /// <summary>The gameplay clock's hours component, saturated by gameplay at 99.</summary>
    public int GameTimeHours { get; init; }

    /// <summary>The startup dispatcher route, carried unchanged including values the dispatcher does not name.</summary>
    public int LoadingGameState { get; init; } = SaveLoadingGameStateCodec.Word(SaveLoadingGameState.MainGame);

    /// <summary>The zero-based load-station index within <see cref="Area"/>.</summary>
    public int SaveStation { get; init; }

    /// <summary>The retail area index that owns <see cref="SaveStation"/>.</summary>
    public int Area { get; init; }

    /// <summary>The persistent progress to save; the snapshot owns this copy.</summary>
    public PersistentProgress Progress { get; init; } = new();

    /// <summary>Captures the translated gameplay owners into a detached snapshot.</summary>
    /// <param name="samus">Source of player inventory, ammunition, energy, and reserve state.</param>
    /// <param name="system">Source of persistent events, bosses, room bits, maps, and load state.</param>
    /// <param name="context">The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</param>
    /// <returns>A detached copy safe to pass to <see cref="SaveSlotStore.SaveSlot"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> or <paramref name="system"/> is <see langword="null"/>.</exception>
    public static SuperMetroidSaveSnapshot Capture(SamusState samus, SharedGameState system, SaveSnapshotContext context)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(system);
        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = (context.ControllerBindings ?? ControllerBindings.Default)
                .RequireRetailPermutation(),
            MoonwalkEnabled = context.MoonwalkEnabled,
            IconCancelEnabled = context.IconCancelEnabled,
            EquippedItems = samus.EquippedItems,
            CollectedItems = samus.CollectedItems,
            EquippedBeams = samus.EquippedBeams,
            CollectedBeams = samus.CollectedBeams,
            ReserveMode = samus.ReserveTankMode,
            Health = samus.Health,
            MaxHealth = samus.MaxHealth,
            Missiles = samus.Missiles,
            MaxMissiles = samus.MaxMissiles,
            SuperMissiles = samus.SuperMissiles,
            MaxSuperMissiles = samus.MaxSuperMissiles,
            PowerBombs = samus.PowerBombs,
            MaxPowerBombs = samus.MaxPowerBombs,
            HudItem = samus.SelectedHudItem,
            MaxReserveEnergy = samus.MaxReserveEnergy,
            ReserveEnergy = samus.ReserveEnergy,
            ReserveMissiles = samus.ReserveMissiles,
            JapaneseText = context.JapaneseText,
            LoadedItemCount = system.LoadedItemCount,
            GameTimeFrames = context.GameTime?.Frames ?? 0,
            GameTimeSeconds = context.GameTime?.Seconds ?? 0,
            GameTimeMinutes = context.GameTime?.Minutes ?? 0,
            GameTimeHours = context.GameTime?.Hours ?? 0,
            LoadingGameState = system.SavedLoadingGameState,
            SaveStation = context.SaveStation,
            Area = AreaIds.ToIndex(context.Area),
            Progress = system.Progress.Copy(),
        };
    }
}
