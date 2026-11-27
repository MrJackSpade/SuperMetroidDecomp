using System.Text.Json;
using Engine.Actors;
using Engine.Core;
using Engine.Ui;
using Game.Data;
using Godot;

namespace Game.Assists;

/// <summary>
/// What a tester has asked of the game to make it easier to try: the old game's tester's options. None of it is the
/// cartridge's, and with all of it off the game plays as the cartridge does.
/// </summary>
public sealed class AssistSettings
{
    /// <summary>A new game begins on Ceres at once, with the opening left out.</summary>
    public bool SkipOpening { get; set; }

    /// <summary>
    /// Whatever hurts Samus takes its usual energy, but energy that would be gone is left at one; a shinespark does not
    /// end for want of energy and goes on costing her down to one.
    /// </summary>
    public bool Invincible { get; set; }

    /// <summary>Each kind of ammunition she can hold any of never stays at none.</summary>
    public bool AmmoNeverRunsOut { get; set; }

    /// <summary>She is given everything there is to collect, full, as a game begins or is loaded and as this is switched on.</summary>
    public bool GrantsAllEquipment { get; set; }

    /// <summary>The way down to Tourian is open in the statues' hall, with no boss counted beaten.</summary>
    public bool TourianOpen { get; set; }

    /// <summary>A saved game is loaded with its bosses, Shaktool's path and its eye doors as they were before she met them, outside Tourian.</summary>
    public bool ResetsBossesOnLoad { get; set; }

    /// <summary>An escape's countdown runs as usual down to one second and stays there.</summary>
    public bool CountdownHolds { get; set; }

    /// <summary>The time the ending takes the game to have been cleared in, in minutes, in place of the time played; none for the time played.</summary>
    public int? EndingMinutes { get; set; }

    /// <summary>What the maps show beyond what she has explored and found.</summary>
    public MapReveal MapReveal { get; set; }
}

/// <summary>
/// The tester's assists: the settings (<see cref="AssistSettings"/>), kept in a file of their own outside any save
/// and outside the game's options, and what each of them does that is not one line where it is asked. The title's
/// demos and the opening's scenes of play are played unassisted, whatever is set.
/// </summary>
public sealed class TesterAssists : ISessionService
{
    /// <summary>The energy, the reserve and the ammunition a full inventory holds.</summary>
    private static readonly (string Amount, int Most)[] FullAmounts =
    [
        (Amounts.Energy, 1499), (Amounts.ReserveEnergy, 400), (Amounts.Missiles, 230), (Amounts.SuperMissiles, 50), (Amounts.PowerBombs, 50),
    ];

    /// <summary>Everything a full inventory has, each switched on but the spazer, which the plasma beam shuts out.</summary>
    private static readonly string[] FullEquipment =
    [
        Equipment.VariaSuit, Equipment.GravitySuit, Equipment.MorphBall, Equipment.Bombs, Equipment.SpringBall, Equipment.ScrewAttack,
        Equipment.HiJumpBoots, Equipment.SpaceJump, Equipment.SpeedBooster, Equipment.GrappleBeam, Equipment.XrayScope,
        Equipment.ChargeBeam, Equipment.IceBeam, Equipment.WaveBeam, Equipment.Spazer, Equipment.PlasmaBeam, Equipment.ReserveTank,
    ];

    /// <summary>The kinds of ammunition kept from running out.</summary>
    private static readonly string[] Ammunition = [Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs];

    /// <summary>The eye doors a reset closes again, by the fact each one's eye is remembered destroyed under.</summary>
    private static readonly string[] EyeDoors =
    [
        "kraid_eye_door_eye_door_facing_left", "lower_norfair_farming_eye_door_facing_right", "basement_eye_door_facing_left",
        "the_precious_eye_door_facing_right", "tourian_eye_door_eye_door_facing_left",
    ];

    /// <summary>The least an escape's countdown is left at while it is held, in hundredths of a second.</summary>
    public const int CountdownHeldAt = 100;

    private static readonly JsonSerializerOptions Format = new() { WriteIndented = true };
    private static readonly AssistSettings Unassisted = new();
    private readonly string _file = ProjectSettings.GlobalizePath("user://debug.json");
    private readonly List<string> _lent = [];
    private bool _lending;

    /// <summary>The assists, with the settings last kept, or with everything off when none were.</summary>
    /// <exception cref="InvalidDataException">The file of settings cannot be read.</exception>
    public TesterAssists()
    {
        if (!File.Exists(_file))
            return;
        try
        {
            Settings = JsonSerializer.Deserialize<AssistSettings>(File.ReadAllText(_file))
                ?? throw new InvalidDataException($"The tester's settings in {_file} hold nothing that can be read.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The tester's settings in {_file} cannot be read: {exception.Message}", exception);
        }
    }

    /// <summary>What the tester has set, which the menu changes.</summary>
    public AssistSettings Settings { get; } = new();

    /// <summary>The session's assists.</summary>
    public static TesterAssists Of(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Service<TesterAssists>();
    }

    /// <summary>
    /// What holds now: the tester's settings, or nothing at all while one of the title's demos or the opening is
    /// played, which are played unassisted.
    /// </summary>
    public AssistSettings Now(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        for (GameMode? mode = session.Mode; mode is not null; mode = mode.Keeps)
        {
            if (mode is Menus.AttractDemoMode or Cinematics.Intro)
                return Unassisted;
        }

        return Settings;
    }

    /// <summary>Keeps the settings as they now are.</summary>
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_file) ?? ".");
        File.WriteAllText(_file, JsonSerializer.Serialize(Settings, Format));
    }

    /// <summary>
    /// Gives Samus everything there is to collect, full, and counts every item's place as taken, so that nothing of it
    /// lies in the rooms to be taken twice.
    /// </summary>
    public static void GrantEquipment(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        Inventory inventory = session.Player.Inventory;
        foreach (string equipment in FullEquipment)
        {
            inventory.Grant(equipment);
            inventory.Switch(equipment, on: equipment != Equipment.Spazer);
        }

        foreach ((string amount, int most) in FullAmounts)
        {
            inventory.SetCapacity(amount, most);
            inventory.Fill(amount);
        }

        foreach (string location in ItemLocations.All)
            session.Progress.Record(location);
    }

    /// <summary>What a game newly begun is given: the full inventory, when that is asked.</summary>
    public void AfterNewGame(GameSession session)
    {
        if (Settings.GrantsAllEquipment)
            GrantEquipment(session);
    }

    /// <summary>
    /// What a saved game just loaded is given, before any room is made from it: its bosses back, when that is asked,
    /// and the full inventory, when that is.
    /// </summary>
    public void AfterLoad(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (Settings.ResetsBossesOnLoad)
        {
            // Every boss but Tourian's, Shaktool's path and the five eye doors.
            foreach (string boss in Bosses.All)
            {
                if (boss != Bosses.MotherBrain)
                    session.Progress.Forget(Bosses.Defeated(boss));
            }

            session.Progress.Forget(StoryEvents.ShaktoolClearedPath);
            foreach (string door in EyeDoors)
                session.Progress.Forget(door);
        }

        if (Settings.GrantsAllEquipment)
            GrantEquipment(session);
    }

    /// <summary>
    /// As Samus's update begins, keeps her ammunition from being found at none within it: a kind she can hold that
    /// stands at none is put at one, and a kind at one is lent a second, so that firing her last does not put the
    /// item away. What was lent is taken back as her update ends (<see cref="TakeAmmoBack"/>).
    /// </summary>
    public void LendAmmo(GameSession session)
    {
        // Whatever an update that did not end as usual left lent is taken back first.
        TakeAmmoBack(session);
        _lending = Now(session).AmmoNeverRunsOut;
        if (!_lending)
            return;
        Inventory inventory = session.Player.Inventory;
        foreach (string ammo in Ammunition)
        {
            if (inventory.Capacity(ammo) == 0)
                continue;
            if (inventory.Amount(ammo) == 0)
            {
                inventory.Restore(ammo, 1);
            }
            else if (inventory.Amount(ammo) == 1)
            {
                inventory.Restore(ammo, 2);
                _lent.Add(ammo);
            }
        }
    }

    /// <summary>As Samus's update ends, takes back what was lent as it began, and leaves no kind she can hold at none.</summary>
    public void TakeAmmoBack(GameSession session)
    {
        if (!_lending)
            return;
        _lending = false;
        Inventory inventory = session.Player.Inventory;
        foreach (string ammo in _lent)
            inventory.Lose(ammo, 1);
        _lent.Clear();
        foreach (string ammo in Ammunition)
        {
            if (inventory.Capacity(ammo) > 0 && inventory.Amount(ammo) == 0)
                inventory.Restore(ammo, 1);
        }
    }

    /// <summary>The settings last the program: nothing of them begins again with a game.</summary>
    public void BeginAgain()
    {
    }

    /// <summary>
    /// Invincible, energy that the update has taken to nothing is left at one, before it is asked whether she is saved
    /// or dies.
    /// </summary>
    public void Step(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!Now(session).Invincible)
            return;
        Player samus = session.Player;
        if (!samus.IsDying && samus.Inventory.Amount(Amounts.Energy) == 0)
            samus.Inventory.Restore(Amounts.Energy, 1);
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
