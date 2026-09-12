namespace SuperMetroid.Core.Game;

/// <summary>Fixed growing-shutter dispatch ordering and 16.16 growth increments.</summary>
public static class GrowingShutterDefinitions
{
    /// <summary>$A2:EA4E, InitAI_ShutterGrowing.functionPointers: down timer/proximity, then up timer/proximity.</summary>
    public static GrowingShutterFunction InitialFunction(int index) => index switch
    {
        0 => GrowingShutterFunction.WaitToGrowDownForTimer,
        1 => GrowingShutterFunction.WaitToGrowDownForProximity,
        2 => GrowingShutterFunction.WaitToGrowUpForTimer,
        3 => GrowingShutterFunction.WaitToGrowUpForProximity,
        _ => throw new InvalidDataException($"Growing shutter initial selector {index} exceeds its four-entry definition."),
    };

    /// <summary>
    /// $A2:EA56, InitAI_ShutterGrowing.YSpeed/YSubSpeed: 24 records from 0.0625 to 1.5 pixels/frame.
    /// Only parameter two's low byte selects the record; its high byte remains unrelated state.
    /// </summary>
    public static (short Whole, ushort Fraction) Speed(ushort parameter)
    {
        int index = (byte)parameter;
        if (index >= 24)
            throw new InvalidDataException($"Growing shutter speed index {index} exceeds its 24-entry definition.");
        int value = (index + 1) * 0x1000;
        return ((short)(value >> 16), (ushort)value);
    }
}
