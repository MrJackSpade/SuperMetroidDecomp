using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks Mother Brain's Rinka spawn reservations against the original token table (#1699): over every set of held
/// points, every screen the camera can show of them and every spawn point a Rinka can hold (#2256), the point reserved,
/// where the Rinka is placed, and its release match the original's reservation and release by token.
/// </summary>
internal static class RinkaSpawnReservationsVerification
{
    /// <summary>The bank-$A2 spawn-point table: eleven records of centre X, centre Y and reservation token.</summary>
    private const int SpawnPointTable = 0xa2b75b;

    /// <summary>The table's record count.</summary>
    private const int SpawnPointCount = 11;

    /// <summary>
    /// Runs every reservation and release the reservations can reach: each of the 2048 sets of held points, each camera
    /// whose screen shows a different selection of the spawn places, and each spawn point the reserving Rinka holds,
    /// reserving and then releasing, compared with the original algorithm reading the cartridge's table at that point's
    /// centre, where the import places a Rinka that starts at it. A room-wide Rinka placed at no point's centre is refused
    /// at import.
    /// </summary>
    internal static void VerifyRinkaSpawnReservations(CartridgeImportAddressSpace rom)
    {
        SpawnRecord[] table = ReadTable(rom);
        Position16<RoomPixels>[] spawns = [.. table.Select(record => record.Centre)];
        Position16<RoomPixels>[] cameras = DistinctScreens(spawns);
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Rinka, PopulationWords.Parameters(1, 0), new(500, 100)),
            "a room-wide Rinka placed at no spawn point's centre is refused at import");
        AssertEqual<EnemySpawnSettings>(new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.CentreUpper),
            EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Rinka, PopulationWords.Parameters(1, 0), RinkaSpawnReservations.Centre(RinkaSpawnPoint.CentreUpper)),
            "a room-wide Rinka's spawn point is the point whose centre places it");
        // Each set of held points is one case with its own reservations and its own original table state, reading only
        // the immutable table, spawns, cameras and point definitions, so the 2048 sets run across the machine's cores
        // (#1757); a set writes only its own count, summed once every set has run.
        var casesByHeld = new int[1 << SpawnPointCount];
        ParallelSweep.For(0, 1 << SpawnPointCount, held =>
        {
            int cases = 0;
            var reservations = new RinkaSpawnReservations();
            var original = new TokenTableReservations(table);
            for (int index = 0; index < SpawnPointCount; index++)
            {
                if ((held & (1 << index)) == 0)
                    continue;
                // A point, seen from a camera at its centre, reserves itself.
                RinkaSpawnPoint point = RinkaSpawnPointDefinitions.SearchOrder[index].Point;
                AssertEqual(point, reservations.Reserve(point, table[index].Centre),
                    $"held set {held}: point {index} reserves itself");
                original.Occupied[index] = true;
            }
            AssertSameHeld(original, reservations, $"held set {held}");

            foreach (Position16<RoomPixels> camera in cameras)
            {
                for (int own = 0; own < SpawnPointCount; own++)
                {
                    cases++;
                    Position16<RoomPixels> spawn = spawns[own];
                    RinkaSpawnPoint? reserved = reservations.Reserve(RinkaSpawnPointDefinitions.SearchOrder[own].Point, camera);
                    TokenReservation expected = original.Reserve(spawn, camera);
                    if (reserved is not { } point)
                    {
                        // Every point held: neither reserves, and nothing is released.
                        AssertTrue(!expected.Reserved, $"held set {held}, camera {camera}, spawn {spawn}: the original reserves token {expected.Token}");
                        continue;
                    }
                    int token = MotherBrainNativeWords.RinkaSpawnPoints.Word(point);
                    Position16<RoomPixels> placed = RinkaSpawnReservations.Centre(point);
                    if (!expected.Reserved || expected.Token != token || expected.Position != placed || !reservations.IsReserved(point))
                    {
                        AssertEqual(expected, new TokenReservation(true, token, expected.Relocated, placed),
                            $"held set {held}, camera {camera}, spawn {spawn}: reservation (held after: {reservations.IsReserved(point)})");
                    }

                    reservations.Release(point);
                    original.Release();
                    if (reservations.IsReserved(point) || original.Occupied[TableIndex(table, token)])
                        AssertSameHeld(original, reservations, $"held set {held}, camera {camera}, spawn {spawn}: release");
                }
            }
            AssertSameHeld(original, reservations, $"held set {held} after every reservation and release");
            casesByHeld[held] = cases;
        });
        Console.WriteLine($"Rinka spawn reservations: {casesByHeld.Sum()} reservations and releases over {cameras.Length} screens match the original token table.");
    }

    /// <summary>Reads the eleven spawn-point records.</summary>
    private static SpawnRecord[] ReadTable(CartridgeImportAddressSpace rom)
    {
        var source = CartridgeImportSource.Require(rom);
        return [.. Enumerable.Range(0, SpawnPointCount).Select(index =>
        {
            int record = SpawnPointTable + 6 * index;
            return new SpawnRecord(
                new(source.ReadCartridgeWord(record), source.ReadCartridgeWord(record + 2)),
                source.ReadCartridgeWord(record + 4));
        })];
    }

    /// <summary>
    /// One camera for each distinct selection of <paramref name="spawns"/> a screen can show: cameras at and one pixel
    /// either side of every edge at which a spawn enters or leaves the screen, keeping the first with each selection.
    /// </summary>
    private static Position16<RoomPixels>[] DistinctScreens(Position16<RoomPixels>[] spawns)
    {
        int[] xs = [.. spawns.SelectMany(spawn => new[] { spawn.X - 256, spawn.X - 255, spawn.X, spawn.X + 1 }).Append(0).Distinct()];
        int[] ys = [.. spawns.SelectMany(spawn => new[] { spawn.Y - 224, spawn.Y - 223, spawn.Y, spawn.Y + 1 }).Append(0).Distinct()];
        var screens = new Dictionary<long, Position16<RoomPixels>>();
        foreach (int x in xs)
        {
            foreach (int y in ys)
            {
                Position16<RoomPixels> camera = Position16<RoomPixels>.Wrapped(x, y);
                long shown = 0;
                for (int index = 0; index < spawns.Length; index++)
                {
                    if (!TokenTableReservations.IsOutsideCamera(spawns[index], camera))
                        shown |= 1L << index;
                }
                screens.TryAdd(shown, camera);
            }
        }
        return [.. screens.Values];
    }

    /// <summary>The table index whose record carries <paramref name="token"/>.</summary>
    private static int TableIndex(SpawnRecord[] table, int token)
    {
        // A loop rather than a predicate: the check asks this of every case, and a capturing predicate allocates each time.
        for (int index = 0; index < table.Length; index++)
        {
            if (table[index].Token == token)
                return index;
        }
        return -1;
    }

    /// <summary>Asserts both hold the same points: each table index's point is held exactly when the original marks it occupied.</summary>
    private static void AssertSameHeld(TokenTableReservations original, RinkaSpawnReservations reservations, string context)
    {
        for (int index = 0; index < SpawnPointCount; index++)
        {
            RinkaSpawnPoint point = MotherBrainNativeWords.RinkaSpawnPoints.Decode(original.Table[index].Token, "Rinka spawn token");
            AssertEqual(original.Occupied[index], reservations.IsReserved(point), $"{context}: {point} held");
        }
    }

    /// <summary>One spawn-point record as the cartridge stores it.</summary>
    /// <param name="Centre">The point's centre in room pixels.</param>
    /// <param name="Token">The point's reservation token.</param>
    private readonly record struct SpawnRecord(Position16<RoomPixels> Centre, int Token);

    /// <summary>What the original reservation did.</summary>
    /// <param name="Reserved">Whether a point was reserved.</param>
    /// <param name="Token">The reserving Rinka's token afterwards.</param>
    /// <param name="Relocated">Whether the Rinka moved to the point's centre.</param>
    /// <param name="Position">Where the Rinka is placed afterwards.</param>
    private readonly record struct TokenReservation(bool Reserved, int Token, bool Relocated, Position16<RoomPixels> Position);

    /// <summary>
    /// The original reservation, transcribed from the port before #1699: an occupancy flag per table record, the reserving
    /// Rinka's token, a lookup of the population spawn's record that falls back to the first, and release by token.
    /// </summary>
    private sealed class TokenTableReservations(SpawnRecord[] table)
    {
        /// <summary>The cartridge's spawn-point records.</summary>
        internal SpawnRecord[] Table { get; } = table;

        /// <summary>Which records are occupied.</summary>
        internal bool[] Occupied { get; } = new bool[table.Length];

        /// <summary>The token of the point the reserving Rinka holds, or zero.</summary>
        private int _token;

        /// <summary>Reserves as the original did for a Rinka whose population spawn is <paramref name="spawn"/>.</summary>
        internal TokenReservation Reserve(Position16<RoomPixels> spawn, Position16<RoomPixels> camera)
        {
            int mapped = 0;
            for (int index = 0; index < Table.Length; index++)
            {
                if (Table[index].Centre == spawn)
                {
                    mapped = index;
                    break;
                }
            }
            bool mustRelocate = IsOutsideCamera(spawn, camera) || Occupied[mapped];
            Position16<RoomPixels> position = spawn;
            if (mustRelocate)
            {
                int selected = -1;
                for (int index = 0; index < Table.Length; index++)
                {
                    if (!IsOutsideCamera(Table[index].Centre, camera) && !Occupied[index])
                    {
                        selected = index;
                        break;
                    }
                }
                if (selected < 0)
                {
                    for (int index = 0; index < Table.Length; index++)
                    {
                        if (!Occupied[index])
                        {
                            selected = index;
                            break;
                        }
                    }
                }
                if (selected < 0)
                    return new TokenReservation(false, _token, false, spawn);
                mapped = selected;
                position = Table[selected].Centre;
            }
            Occupied[mapped] = true;
            _token = Table[mapped].Token;
            return new TokenReservation(true, _token, mustRelocate, position);
        }

        /// <summary>Releases the record whose token the Rinka holds.</summary>
        internal void Release()
        {
            if (_token == 0)
                return;
            for (int index = 0; index < Table.Length; index++)
            {
                if (Table[index].Token == _token)
                {
                    Occupied[index] = false;
                    break;
                }
            }
            _token = 0;
        }

        /// <summary>The original strict screen test for special spawns.</summary>
        internal static bool IsOutsideCamera(Position16<RoomPixels> position, Position16<RoomPixels> camera) =>
            CoordinateAxis.AsSigned(position.Y) < 0 ||
            CoordinateAxis.Difference(position.Y, camera.Y) is < 0 or >= 224 ||
            CoordinateAxis.Difference(position.X, camera.X) is < 0 or >= 256;
    }
}
