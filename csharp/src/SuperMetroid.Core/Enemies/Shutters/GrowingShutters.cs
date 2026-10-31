using static SuperMetroid.Core.Enemies.Shutters.GrowingShutterDefinitions;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>A growing shutter's starting behaviour and growth speed, selected by its population parameters.</summary>
public static class GrowingShutters
{
    /// <summary>The function initial selector <paramref name="index"/> starts the shutter with.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is outside the four selectors.</exception>
    public static GrowingShutterFunction InitialFunction(int index) =>
        (uint)index < (uint)InitialFunctions.Length
            ? InitialFunctions[index]
            : throw new InvalidDataException($"Growing shutter initial selector {index} exceeds its four-entry definition.");

    /// <summary>
    /// $A2:EA56, InitAI_ShutterGrowing.YSpeed.Whole/YSubSpeed: 24 records from 0.0625 to 1.5 pixels/frame.
    /// Only parameter two's low byte selects the record; its high byte remains unrelated state.
    /// </summary>
    /// <remarks>
    /// #625 / #642 exact NTSC J/U v1.0 algorithm: for low-byte index i in 0..23, the unsigned
    /// 16.16 increment is (i + 1) * $1000; split its high and low words in native order.
    /// All 24 pairs at $A2:EA56-$EAB5 match the pinned ROM and bank-A2 disassembly.
    /// The initializer masks parameter two to a byte, multiplies it by four, then reads
    /// both words; index 24 would read the following code and is outside this definition.
    /// VerifyCompiledGrowingShutters covers all 24 values through the real initializer,
    /// every high byte, all four dispatch selectors, and the invalid index boundary.
    /// </remarks>
    public static SignedSixteenSixteen Speed(int parameter)
    {
        int index = WordWrap.LowByte(parameter);
        if (index >= SpeedCount)
            throw new InvalidDataException($"Growing shutter speed index {index} exceeds its 24-entry definition.");
        return SignedSixteenSixteen.FromRaw((index + 1) * SpeedStep);
    }
}
