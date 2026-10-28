using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>Compares Puyo hop records and initial vertical-curve integration with retail bank-$A2 data.</summary>
internal static class PuyoHopDefinitionsVerification
{
    /// <summary>Checks seven native records and the six non-dropping integrations, then rejects invalid byte selectors.</summary>
    internal static void VerifyCompiledPuyoHops(SuperMetroidAddressSpace rom)
    {
        Action<PuyoEnemyState> calculate = PuyoBehavior.CalculateInitialPuyoHopSpeed;
        var state = new PuyoEnemyState();
        for (ushort index = 0; index < 56; index += 8)
        {
            var hop = PuyoHops.FromByteIndex(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a07 + index)), hop.Height, "Puyo native hop height");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a09 + index)), hop.XSpeed, "Puyo native horizontal speed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a0b + index)), hop.YIndexDelta, "Puyo native gravity/drop delta");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a0d + index)), PuyoNativeWords.PuyoAirborneFunctions.Word(hop.Function), "Puyo native airborne callback");
            if (hop.Function == PuyoAirborneFunction.Dropping)
                continue; // Dropping bypasses hop initialization and uses constant speed.
            ushort time = 0, distance = 0;
            ushort height = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a07 + index));
            ushort target = unchecked((ushort)((height << 8) | (LittleEndianBytes.HighByte(height))));
            do
            {
                time = unchecked((ushort)(time + CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a09 + index))));
                distance = unchecked((ushort)(distance + CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08390 + (LittleEndianBytes.HighByte(time)) * 8))));
            } while (unchecked((short)(target - distance)) >= 0);
            state.HopRecord = index / PuyoHopDefinitions.RecordSize;
            calculate(state);
            AssertEqual(time, state.YSpeedTableIndex, "Puyo real initial curve index without bus");
            AssertEqual(time / 2, state.InitialYSpeedTableIndexHalf, "Puyo half threshold");
            AssertEqual(time / 2 + time / 4, state.InitialYSpeedTableIndexThreeQuarters, "Puyo three-quarter threshold");
        }
        foreach (ushort invalid in new ushort[] { 1, 7, 49, 56, 65535 })
            AssertThrows<InvalidDataException>(() => PuyoHops.FromByteIndex(invalid), "Puyo invalid byte selector");
        Console.WriteLine("Puyo definitions: all seven native records and six real initial hop integrations match without a bus; dropping remains a separate constant-speed path.");
    }
}
