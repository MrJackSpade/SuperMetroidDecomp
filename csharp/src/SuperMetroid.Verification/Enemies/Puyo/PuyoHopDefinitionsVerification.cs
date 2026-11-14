using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>Compares Puyo hop records and initial vertical-curve integration with retail bank-$A2 data.</summary>
internal static class PuyoHopDefinitionsVerification
{
    /// <summary>Checks the seven hops against their native records and the six non-dropping integrations.</summary>
    internal static void VerifyCompiledPuyoHops(CartridgeImportAddressSpace rom)
    {
        Action<PuyoEnemyState> calculate = PuyoBehavior.CalculateInitialPuyoHopSpeed;
        var state = new PuyoEnemyState();
        foreach (PuyoHopType hopType in Enum.GetValues<PuyoHopType>())
        {
            int index = PuyoNativeWords.HopRecordOffsets.Word(hopType);
            var hop = PuyoHopDefinitions.Hops[hopType];
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a07 + index)), hop.JumpHeight, "Puyo native hop height");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a09 + index)), hop.XSpeed, "Puyo native horizontal speed");
            // The record's third value is the hop's curve step, or for the drop its fall speed (#1820).
            AssertEqual((int)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a0b + index)), hop.Movement == PuyoAirborneMovement.Dropping
                ? PuyoHopDefinitions.DropSpeed.RawValue >> 8
                : hop.CurveStep.InTwoHundredFiftySixthsOfMark, "Puyo native curve step or drop speed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a0d + index)), PuyoNativeWords.PuyoAirborneMovements.Word(hop.Movement), "Puyo native airborne callback");
            if (hop.Movement == PuyoAirborneMovement.Dropping)
                continue; // Dropping bypasses hop initialization and uses constant speed.
            ushort time = 0, distance = 0;
            ushort height = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a07 + index));
            ushort target = unchecked((ushort)((height << 8) | (LittleEndianBytes.HighByte(height))));
            do
            {
                time = unchecked((ushort)(time + CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa29a09 + index))));
                distance = unchecked((ushort)(distance + CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08390 + (LittleEndianBytes.HighByte(time)) * 8))));
            } while (unchecked((short)(target - distance)) >= 0);
            state.CurrentHop = hopType;
            calculate(state);
            AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(time), state.YSpeedCurvePlace, "Puyo real initial curve index without bus");
            AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(time / 2), state.InitialYSpeedCurvePlaceHalf, "Puyo half threshold");
            AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(time / 2 + time / 4), state.InitialYSpeedCurvePlaceThreeQuarters, "Puyo three-quarter threshold");
        }
        Console.WriteLine("Puyo definitions: all seven native records and six real initial hop integrations match without a bus; dropping remains a separate constant-speed path.");
    }
}
