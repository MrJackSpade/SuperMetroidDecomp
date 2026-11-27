using Engine.Actors;
using Engine.Core;
using Game.Data;

namespace Game.Tests.Movies;

/// <summary>
/// What of the game is held against the cartridge's record of an update: results a player can observe, each read
/// from a member the game has for its own use. Nothing of the cartridge's own workings is compared (its poses, its
/// handlers, its places for things), and nothing is added to the game to be read here.
/// </summary>
/// <remarks>
/// <para>
/// Compared on every update: the random number. Compared on every update the cartridge is in play: that the game is
/// in play too, the room, Samus's place to the part of a pixel, her energy, reserves and ammunition with their
/// capacities, her equipment and beams owned and switched on, the view's place, and the places of the enemies whose
/// kinds are decided (<see cref="CartridgeEnemies"/>).
/// </para>
/// <para>
/// Not compared yet, to be added as the movies need them: the escape's time, what enemies have launched, enemies'
/// health, and anything outside play beyond the random number (a door's scroll, the pause, the menus).
/// </para>
/// </remarks>
public sealed class MovieCheck
{
    /// <summary>The cartridge's bits for equipment, with the game's name for each.</summary>
    private static readonly (int Bit, string Name)[] EquipmentBits =
    [
        (0x0001, Equipment.VariaSuit), (0x0002, Equipment.SpringBall), (0x0004, Equipment.MorphBall), (0x0008, Equipment.ScrewAttack),
        (0x0020, Equipment.GravitySuit), (0x0100, Equipment.HiJumpBoots), (0x0200, Equipment.SpaceJump), (0x1000, Equipment.Bombs),
        (0x2000, Equipment.SpeedBooster), (0x4000, Equipment.GrappleBeam), (0x8000, Equipment.XrayScope),
    ];

    /// <summary>The cartridge's bits for beams, with the game's name for each.</summary>
    private static readonly (int Bit, string Name)[] BeamBits =
    [
        (0x0001, Equipment.WaveBeam), (0x0002, Equipment.IceBeam), (0x0004, Equipment.Spazer), (0x0008, Equipment.PlasmaBeam),
        (0x1000, Equipment.ChargeBeam),
    ];

    /// <summary>The cartridge's amounts, each with its capacity, and the game's name for each.</summary>
    private static readonly (int Amount, int Capacity, string Name)[] AmountCells =
    [
        (Cartridge.Energy, Cartridge.EnergyCapacity, Amounts.Energy),
        (Cartridge.Reserve, Cartridge.ReserveCapacity, Amounts.ReserveEnergy),
        (Cartridge.Missiles, Cartridge.MissileCapacity, Amounts.Missiles),
        (Cartridge.SuperMissiles, Cartridge.SuperMissileCapacity, Amounts.SuperMissiles),
        (Cartridge.PowerBombs, Cartridge.PowerBombCapacity, Amounts.PowerBombs),
    ];

    private readonly MovieTrace _trace;
    private readonly SortedSet<int> _undecided = [];

    /// <summary>A check against <paramref name="trace"/>.</summary>
    public MovieCheck(MovieTrace trace)
    {
        ArgumentNullException.ThrowIfNull(trace);
        _trace = trace;
    }

    /// <summary>The kinds of enemy the cartridge has had in its list, in play, that <see cref="CartridgeEnemies"/> does not decide.</summary>
    public IReadOnlyCollection<int> UndecidedEnemyKinds => _undecided;

    /// <summary>Everything the game's equipment and beams are named by, and the cartridge's bits owned and switched on for them, for a start made from a snapshot.</summary>
    public static IEnumerable<(string Name, bool Owned, bool On)> EquipmentOf(int equipmentOn, int equipmentOwned, int beamsOn, int beamsOwned) =>
        EquipmentBits.Select(each => (each.Name, (equipmentOwned & each.Bit) != 0, (equipmentOn & each.Bit) != 0))
            .Concat(BeamBits.Select(each => (each.Name, (beamsOwned & each.Bit) != 0, (beamsOn & each.Bit) != 0)));

    /// <summary>The cartridge's cells for each amount and its capacity, with the game's name for it.</summary>
    public static IReadOnlyList<(int Amount, int Capacity, string Name)> AmountsKept => AmountCells;

    /// <summary>The game as it stands, in a line, for a report.</summary>
    public static string Describe(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        string line = $"{ModeNames(session.Mode)}; random {RandomOf(session):X4}";
        if (PlayOf(session) is not { } playing)
            return line;
        Player samus = session.Player;
        return $"{line}; {playing.Room.SceneFilePath}; Samus {Place(samus.Position.X)},{Place(samus.Position.Y)}; view {session.Camera.Left},{session.Camera.Top}";
    }

    /// <summary>
    /// How <paramref name="session"/> differs from the cartridge's record last read (<see cref="MovieTrace.ReadTo"/>),
    /// each difference in a line with both values; none when they agree.
    /// </summary>
    public IReadOnlyList<string> Differences(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var differences = new List<string>();
        int random = _trace.Word(Cartridge.Random);
        if (random != RandomOf(session))
            differences.Add($"the random number: the cartridge has {random:X4}, the game {RandomOf(session):X4}");
        if (_trace.Word(Cartridge.GameState) != Cartridge.InPlay)
            return differences;

        if (PlayOf(session) is not { } playing)
        {
            differences.Add($"the cartridge is in play; the game is in {ModeNames(session.Mode)}");
            return differences;
        }

        string room = CartridgeRooms.SceneOf(_trace.Word(Cartridge.Room));
        if (room != playing.Room.SceneFilePath)
        {
            // Everything else of play is of another room: only the room is reported.
            differences.Add($"the room: the cartridge is in {room}, the game in {playing.Room.SceneFilePath}");
            return differences;
        }

        Player samus = session.Player;
        Compare(differences, "Samus's place across", $"{_trace.Word(Cartridge.SamusX)}.{_trace.Word(Cartridge.SamusXPart):X4}", Place(samus.Position.X));
        Compare(differences, "Samus's place down", $"{_trace.Word(Cartridge.SamusY)}.{_trace.Word(Cartridge.SamusYPart):X4}", Place(samus.Position.Y));
        foreach ((int amount, int capacity, string name) in AmountCells)
        {
            Compare(differences, $"the amount \"{name}\"", _trace.Word(amount), samus.Inventory.Amount(name));
            Compare(differences, $"the capacity of \"{name}\"", _trace.Word(capacity), samus.Inventory.Capacity(name));
        }

        foreach ((string name, bool owned, bool on) in EquipmentOf(
            _trace.Word(Cartridge.EquipmentOn), _trace.Word(Cartridge.EquipmentOwned), _trace.Word(Cartridge.BeamsOn), _trace.Word(Cartridge.BeamsOwned)))
        {
            Compare(differences, $"whether \"{name}\" is owned", owned, samus.Inventory.Owns(name));
            Compare(differences, $"whether \"{name}\" is switched on", on, samus.Inventory.IsOn(name));
        }

        Compare(differences, "the view's left edge", _trace.Word(Cartridge.ViewX), session.Camera.Left);
        Compare(differences, "the view's top edge", _trace.Word(Cartridge.ViewY), session.Camera.Top);
        CompareEnemies(differences, playing);
        return differences;
    }

    /// <summary>The enemies of the kinds decided, as sets of places: the cartridge's entries of each type against the room's living enemies of it.</summary>
    private void CompareEnemies(List<string> differences, PlayingMode playing)
    {
        var kept = new SortedDictionary<string, List<(int X, int Y)>>(StringComparer.Ordinal);
        foreach (CartridgeEnemies.Kind kind in CartridgeEnemies.Kinds.Values)
        {
            if (kind.GameType is { } type)
                kept.TryAdd(type, []);
        }

        for (int place = 0; place < Cartridge.EnemyPlaces; place++)
        {
            int at = Cartridge.Enemies + place * Cartridge.EnemyStride;
            int kind = _trace.Word(at + Cartridge.EnemyKind);
            if (kind == 0)
                continue;
            if (!CartridgeEnemies.Kinds.TryGetValue(kind, out CartridgeEnemies.Kind? how))
                _undecided.Add(kind);
            else if (how.GameType is { } type)
                kept[type].Add((_trace.Word(at + Cartridge.EnemyX), _trace.Word(at + Cartridge.EnemyY)));
        }

        foreach ((string type, List<(int X, int Y)> places) in kept)
        {
            var played = playing.Room.Enemies.Where(enemy => !enemy.IsDead && enemy.GetType().Name == type)
                .Select(enemy => (X: enemy.Position.X.Whole & 0xffff, Y: enemy.Position.Y.Whole & 0xffff)).Order().ToList();
            places.Sort();
            if (!places.SequenceEqual(played))
                differences.Add($"the places of the enemies \"{type}\": the cartridge has [{string.Join(" ", places)}], the game [{string.Join(" ", played)}]");
        }
    }

    private static void Compare<T>(List<string> differences, string what, T cartridge, T game)
        where T : notnull
    {
        if (!cartridge.Equals(game))
            differences.Add($"{what}: the cartridge has {cartridge}, the game {game}");
    }

    /// <summary>A place as the cartridge writes one: whole pixels, a point, and the part of a pixel in four hexadecimal digits.</summary>
    private static string Place(Engine.Shared.Pixels place) => $"{place.Whole & 0xffff}.{place.Fraction:X4}";

    /// <summary>The random number as the game has it: the sixteen bits the cartridge keeps.</summary>
    private static int RandomOf(GameSession session) => (int)(session.Random.Last.Number & 0xffff);

    /// <summary>The play the session is in or keeps beneath the mode in control; none when it is not in play.</summary>
    public static PlayingMode? PlayOf(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        for (GameMode? each = session.Mode; each is not null; each = each.Keeps)
        {
            if (each is PlayingMode playing)
                return playing;
        }

        return null;
    }

    private static string ModeNames(GameMode mode)
    {
        var names = new List<string>();
        for (GameMode? each = mode; each is not null; each = each.Keeps)
            names.Add(each.GetType().Name);
        return string.Join(" over ", names);
    }
}
