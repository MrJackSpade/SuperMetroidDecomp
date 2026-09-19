using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledLoadStationDefinitions()
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        int compared = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int count = LoadStationDefinitions.Count(area);
            for (int stationIndex = 0; stationIndex < count; stationIndex++)
            {
                var native = LoadStationEntry.Load(bus, area, checked((byte)stationIndex));
                var compiled = LoadStationDefinitions.Get(area, checked((byte)stationIndex));
                AssertEqual(native, compiled,
                    $"compiled {area} load-station {stationIndex} matches the cartridge");
                compared++;
            }

            AssertThrows<ArgumentOutOfRangeException>(
                () => LoadStationDefinitions.Get(area, checked((byte)count)),
                $"compiled {area} catalog rejects the first nonexistent station");
        }

        AssertEqual(134, compared, "all native load-station slots are compiled");
        AssertThrows<ArgumentOutOfRangeException>(
            () => LoadStationDefinitions.Count((AreaId)AreaIds.RetailCount),
            "compiled load-station catalog rejects unknown areas");

        var guardedRuntime = new SuperMetroidRuntime(new LoadStationReadGuard(bus));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertEqual(LoadStationDefinitions.Get(AreaId.Ceres, 0),
            guardedRuntime.ActiveLoadStation!,
            "production Ceres initialization consumes compiled load-station data");

        Console.WriteLine(
            "Load stations: 134 typed records match cartridge data; production Ceres entry rejects native table reads.");
    }

    private sealed class LoadStationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= LoadStationRomData.PointerTable && address < LoadStationRomData.DataEnd)
            {
                throw new InvalidOperationException(
                    $"Production runtime read native load-station data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
