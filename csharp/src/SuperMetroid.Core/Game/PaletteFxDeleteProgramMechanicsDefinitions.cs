namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control words for the two standalone bank-$8D palette-FX deletion lists.
/// </summary>
/// <remarks>
/// <c>InstList_PaletteFXObject_Delete</c> at $8D:E192 is the shared cinematic
/// deletion target. <c>InstList_PaletteFXObject_Nothing</c> at $8D:E220 is the
/// complete program for room-effect definition $8D:F745. Both lists contain only
/// the native <c>delete</c> instruction and have no presentation payload.
/// </remarks>
public static class PaletteFxDeleteProgramMechanicsDefinitions
{
    /// <summary><c>InstList_PaletteFXObject_Delete</c> at $8D:E192.</summary>
    public const ushort CinematicDelete = 0xe192;

    /// <summary><c>InstList_PaletteFXObject_Nothing</c> at $8D:E220.</summary>
    public const ushort EmptyRoomEffect = 0xe220;

    /// <summary>Palette-FX definition $8D:F745, whose complete program is deletion.</summary>
    public const ushort EmptyRoomEffectDefinition = 0xf745;

    /// <summary>Resolves either standalone deletion list's sole mechanics word.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer is CinematicDelete or EmptyRoomEffect)
        {
            value = PaletteFxInstructionCodes.Delete;
            return true;
        }

        value = 0;
        return false;
    }
}
