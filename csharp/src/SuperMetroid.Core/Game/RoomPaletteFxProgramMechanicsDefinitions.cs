namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared resolver for immutable mechanics words in translated room palette-FX programs.
/// </summary>
public static class RoomPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Resolves one compiled bank-$8D program word across translated owners.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out value))
            return true;
        if (WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}
