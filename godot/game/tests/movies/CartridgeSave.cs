using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.World;
using Engine.World.Objects;
using Game.Data;

namespace Game.Tests.Movies;

/// <summary>
/// One of the cartridge's saved games as the game's own saved game: what a movie's cartridge already holds when it is
/// switched on, and the progress of a cartridge a snapshot was taken of. The oracle's side: the game reads nothing
/// here, and is given the result as it is given any saved game.
/// </summary>
/// <remarks>
/// <para>
/// The cartridge keeps a saved game as a run of its memory (from <c>$7E:D7C0</c>) copied into its kept memory, with its
/// explored maps packed after it. What Samus carries, her controls and the special settings lie at the run's start
/// (in memory, at <c>$09A2</c> on); the bits of progress follow (<c>$D820</c> on). The game keeps the same things as
/// what she owns and has switched on, amounts, named facts, explored cells, a room and an entry point.
/// </para>
/// <para>
/// What the cartridge keeps and the game has no way to hold is listed in <see cref="NotCarried"/>, never dropped
/// silently: each is either something the game needs and lacks, or something nothing depends on.
/// </para>
/// </remarks>
public sealed class CartridgeSave
{
    private const int SavedBytes = 0x2000, SlotBytes = 0x065c, FirstSlot = 0x0010, Slots = 3;
    private const int ChecksumAt = 0x0000, ComplementAt = 0x0008, BackupChecksumAt = 0x1ff0, BackupComplementAt = 0x1ff8, ChosenSlotAt = 0x1fec;

    // Where things lie in a saved game's run of memory.
    private const int EquipmentOnAt = 0x00, EquipmentOwnedAt = 0x02, BeamsOnAt = 0x04, BeamsOwnedAt = 0x06, ButtonsAt = 0x10, ReserveModeAt = 0x1e,
        AmountsAt = 0x20, ReserveCapacityAt = 0x32, ReserveAt = 0x34, TimeFramesAt = 0x38, JapaneseAt = 0x40, MoonwalkAt = 0x42, IconCancelAt = 0x48,
        EventsAt = 0x60, BossesAt = 0x68, ItemsAt = 0xb0, DoorsAt = 0xf0, StationsUsedAt = 0x138, MapsBoughtAt = 0x148, StationAt = 0x156, AreaAt = 0x158,
        PackedMapsAt = 0x15c, PackedMapsBytes = 0x500, ProgressBytes = 0x15c;

    // Where the same things lie in the cartridge's memory while a game is played.
    private const int CarriedInMemory = 0x09a2, CarriedBytes = 0x60, ProgressInMemory = 0xd820, AreaInMemory = 0x079f,
        LiveMapInMemory = 0x07f7, MapsInMemory = 0xcd52;

    private const int PlaneBytes = 256, PageBytes = 128, PageColumns = 32, RunCells = 8, PackedAreas = 6;

    /// <summary>The cartridge's button of each control, by its bit of the controller word.</summary>
    private static readonly Dictionary<int, string> ControlOfButton = new()
    {
        [0x0080] = Controls.A, [0x8000] = Controls.B, [0x0040] = Controls.X, [0x4000] = Controls.Y,
        [0x2000] = Controls.Select, [0x0020] = Controls.L, [0x0010] = Controls.R,
    };

    /// <summary>The actions of play in the order the cartridge keeps their buttons.</summary>
    private static readonly string[] ActionsInOrder =
    [
        GameAction.Attack.Name, GameAction.Jump.Name, Actions.Run.Name, Actions.ItemCancel.Name, Actions.ItemSelect.Name,
        Actions.AngleDown.Name, Actions.AngleUp.Name,
    ];

    private CartridgeSave(SaveData data, IReadOnlyList<string> notCarried) => (Data, NotCarried) = (data, notCarried);

    /// <summary>The saved game as the game keeps one.</summary>
    public SaveData Data { get; }

    /// <summary>What the cartridge's saved game holds that the game has no way to hold.</summary>
    public IReadOnlyList<string> NotCarried { get; }

    /// <summary>
    /// Whether slot <paramref name="slot"/> (from nought) of the cartridge's kept memory holds a saved game, by the
    /// cartridge's own rule: its contents match either of its two pairs of a checksum and its complement.
    /// </summary>
    public static bool Holds(ReadOnlySpan<byte> kept, int slot)
    {
        ReadOnlySpan<byte> run = RunOf(kept, slot);
        int sum = 0;
        for (int at = 0; at < SlotBytes; at += 2)
            sum = (sum + Word(run, at)) & 0xffff;
        int complement = ~sum & 0xffff;
        return (Word(kept, ChecksumAt + slot * 2) == sum && Word(kept, ComplementAt + slot * 2) == complement)
            || (Word(kept, BackupChecksumAt + slot * 2) == sum && Word(kept, BackupComplementAt + slot * 2) == complement);
    }

    /// <summary>
    /// The slot, from nought, the cartridge's choice of saved game begins on: the one last chosen, by its own rule (a
    /// number of a slot that shares no bit with the word kept beside it; otherwise the first).
    /// </summary>
    public static int ChosenSlot(ReadOnlySpan<byte> kept)
    {
        int slot = Word(kept, ChosenSlotAt);
        return slot < Slots && (slot & Word(kept, ChosenSlotAt + 2)) == 0 ? slot : 0;
    }

    /// <summary>The saved game in slot <paramref name="slot"/> (from nought) of the cartridge's kept memory.</summary>
    public static CartridgeSave OfSlot(ReadOnlySpan<byte> kept, int slot, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(content);
        ReadOnlySpan<byte> run = RunOf(kept, slot);

        // The explored maps are packed: of each of the six areas of Zebes in turn, only the runs of eight cells that
        // hold a cell of the area's map, a byte a run, in the order they lie in the area's whole map.
        var planes = new byte[Areas.All.Count][];
        int packed = PackedMapsAt;
        for (int area = 0; area < planes.Length; area++)
        {
            planes[area] = new byte[PlaneBytes];
            if (area >= PackedAreas)
                continue;
            Area map = MapOf(content, area);
            for (int at = 0; at < PlaneBytes; at++)
            {
                (int column, int row) = FirstCellOf(at);
                if (!Enumerable.Range(column, RunCells).Any(across => map.CellAt(across, row) is not null))
                    continue;
                if (packed >= PackedMapsAt + PackedMapsBytes)
                    throw new InvalidDataException("The areas' maps hold more runs of cells than a saved game has room for.");
                planes[area][at] = run[packed++];
            }
        }

        return Build(run[..ProgressBytes], planes);
    }

    /// <summary>
    /// The saved game the cartridge would make of <paramref name="memory"/>, its whole memory mid-game: what she
    /// carries, her progress and her maps as they stand, saved where she last saved.
    /// </summary>
    public static CartridgeSave OfMemory(ReadOnlySpan<byte> memory)
    {
        var run = new byte[ProgressBytes];
        memory.Slice(CarriedInMemory, CarriedBytes).CopyTo(run);
        memory.Slice(ProgressInMemory, ProgressBytes - EventsAt).CopyTo(run.AsSpan(EventsAt));

        // The maps of the areas she is not in are kept apart; the map of the one she is in is the one being marked.
        var planes = new byte[Areas.All.Count][];
        int current = Word(memory, AreaInMemory);
        for (int area = 0; area < planes.Length; area++)
            planes[area] = memory.Slice(area == current ? LiveMapInMemory : MapsInMemory + area * PlaneBytes, PlaneBytes).ToArray();
        return Build(run, planes);
    }

    private static CartridgeSave Build(ReadOnlySpan<byte> run, byte[][] planes)
    {
        var notCarried = new List<string>();
        var data = new SaveData();

        // What she carries.
        foreach ((string name, bool owned, bool on) in MovieCheck.EquipmentOf(
            Word(run, EquipmentOnAt), Word(run, EquipmentOwnedAt), Word(run, BeamsOnAt), Word(run, BeamsOwnedAt)))
        {
            if (owned)
                data.Owned.Add(name);
            if (on)
                data.SwitchedOn.Add(name);
        }

        // The reserve tanks' mode: none yet, refilling by themselves, or by hand.
        switch (Word(run, ReserveModeAt))
        {
            case 0:
                break;
            case 1:
                data.Owned.Add(Equipment.ReserveTank);
                data.SwitchedOn.Add(Equipment.ReserveTank);
                break;
            case 2:
                data.Owned.Add(Equipment.ReserveTank);
                break;
            default:
                throw new InvalidDataException($"The saved game's reserve mode is {Word(run, ReserveModeAt)}, which is none of the cartridge's three.");
        }

        string[] amounts = [Amounts.Energy, Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs];
        for (int index = 0; index < amounts.Length; index++)
        {
            data.Amounts[amounts[index]] = Word(run, AmountsAt + index * 4);
            data.Capacities[amounts[index]] = Word(run, AmountsAt + index * 4 + 2);
        }

        data.Amounts[Amounts.ReserveEnergy] = Word(run, ReserveAt);
        data.Capacities[Amounts.ReserveEnergy] = Word(run, ReserveCapacityAt);

        // Her controls and the special settings, which the cartridge keeps with each file.
        for (int index = 0; index < ActionsInOrder.Length; index++)
        {
            int button = Word(run, ButtonsAt + index * 2);
            data.Controls[ActionsInOrder[index]] = ControlOfButton.TryGetValue(button, out string? control)
                ? control
                : throw new InvalidDataException($"The saved game puts \"{ActionsInOrder[index]}\" on the button {button:X4}, which is no button an action can be on.");
        }

        data.Language = Word(run, JapaneseAt) != 0 ? "japanese" : "english";
        data.Toggles[Toggles.Moonwalk] = Word(run, MoonwalkAt) != 0;
        // The cartridge's setting is "icon cancel", on when the beam is selected again at every door; the game's is its opposite.
        data.Toggles[Toggles.ManualItemCancel] = Word(run, IconCancelAt) == 0;

        // The time played: frames, seconds, minutes and hours, in updates.
        data.PlayUpdates = Word(run, TimeFramesAt) + 60L * (Word(run, TimeFramesAt + 2) + 60L * (Word(run, TimeFramesAt + 4) + 60L * Word(run, TimeFramesAt + 6)));

        // Where it was saved.
        int savedArea = Word(run, AreaAt), station = Word(run, StationAt);
        if (!CartridgeProgress.Stations.TryGetValue((savedArea, station), out (string Room, string EntryPoint) place))
            throw new InvalidDataException($"The saved game was saved at place {station} of area {savedArea}, which is none of the cartridge's.");
        (data.Room, data.EntryPoint) = place;

        // Progress: each bit the fact the game records for it.
        var facts = new SortedSet<string>(StringComparer.Ordinal);
        foreach (int number in BitsSet(run, EventsAt, 8))
        {
            if (CartridgeProgress.Events.TryGetValue(number, out string? fact))
                facts.Add(fact);
            else
                notCarried.Add($"story event {number}, which the cartridge gives no meaning");
        }

        for (int area = 0; area < Areas.All.Count; area++)
        {
            foreach (int bit in BitsSet(run, BossesAt + area, 1))
            {
                if (CartridgeProgress.Bosses.TryGetValue((area, 1 << bit), out string? fact))
                    facts.Add(fact);
                else
                    notCarried.Add($"boss bit {1 << bit} of the area \"{Areas.All[area]}\", which names no boss");
            }

            // The places a game has been saved at in the area, and whether its map has been bought.
            foreach (int number in BitsSet(run, StationsUsedAt + area * 2, 2))
            {
                if (CartridgeProgress.Stations.TryGetValue((area, number), out (string Room, string EntryPoint) used))
                    facts.Add(SaveData.SavedAtFact(used.EntryPoint));
                else
                    notCarried.Add($"saved-at place {number} of the area \"{Areas.All[area]}\", which is none of the cartridge's");
            }

            if (run[MapsBoughtAt + area] != 0)
                facts.Add(Station.MapRevealedFact(Areas.All[area]));
        }

        foreach (int number in BitsSet(run, ItemsAt, DoorsAt - ItemsAt))
        {
            if (CartridgeProgress.Items.TryGetValue(number, out string? fact))
                facts.Add(fact);
            else
                notCarried.Add($"item bit {number}, which is no item's place");
        }

        foreach (int number in BitsSet(run, DoorsAt, StationsUsedAt - DoorsAt))
        {
            if (CartridgeProgress.DoorsNotKept.Contains(number))
                notCarried.Add($"door {number} opened (\"{CartridgeProgress.Doors[number]}\"): the game keeps no fact for it");
            else if (CartridgeProgress.Doors.TryGetValue(number, out string? fact))
                facts.Add(fact);
            else
                notCarried.Add($"door bit {number}, which is no door");
        }

        data.Facts = [.. facts];

        // The explored maps: a bit a cell, most significant first, a row of each page of thirty-two columns in four bytes.
        for (int area = 0; area < planes.Length; area++)
        {
            for (int at = 0; at < PlaneBytes; at++)
            {
                (int column, int row) = FirstCellOf(at);
                for (int cell = 0; cell < RunCells; cell++)
                {
                    if ((planes[area][at] & (0x80 >> cell)) != 0)
                        data.Explored.Add($"{Areas.All[area]}|{column + cell}|{row}");
                }
            }
        }

        data.Explored.Sort(StringComparer.Ordinal);
        return new CartridgeSave(data, notCarried);
    }

    /// <summary>The first cell of the run of eight that byte <paramref name="at"/> of an area's map holds.</summary>
    private static (int Column, int Row) FirstCellOf(int at)
    {
        int page = at / PageBytes, local = at % PageBytes;
        return (page * PageColumns + local % (PageColumns / RunCells) * RunCells, local / (PageColumns / RunCells));
    }

    private static Area MapOf(ContentLibrary content, int area) => Area.Read(content.ReadData($"areas/{Areas.All[area]}.json"));

    /// <summary>The numbers of the bits set in <paramref name="bytes"/> bytes from <paramref name="at"/>, the least significant bit of each byte first.</summary>
    private static List<int> BitsSet(ReadOnlySpan<byte> run, int at, int bytes)
    {
        var set = new List<int>();
        for (int index = 0; index < bytes * 8; index++)
        {
            if ((run[at + index / 8] & (1 << (index % 8))) != 0)
                set.Add(index);
        }

        return set;
    }

    private static ReadOnlySpan<byte> RunOf(ReadOnlySpan<byte> kept, int slot)
    {
        if (kept.Length < SavedBytes)
            throw new InvalidDataException("The cartridge's kept memory is shorter than the cartridge's.");
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, Slots);
        return kept.Slice(FirstSlot + slot * SlotBytes, SlotBytes);
    }

    private static int Word(ReadOnlySpan<byte> bytes, int at) => bytes[at] | (bytes[at + 1] << 8);
}
