namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bounded bank-$84 instruction and physical draw data for the hardcoded
/// elevatube PLM spawned by Maridia's door setup.
/// </summary>
internal static class MaridiaElevatubePlmDefinitions
{
    /// <summary><c>$84:B8F0</c>: sixteen-frame delay and one-block draw.</summary>
    internal const ushort InstructionList = RoomPlmInstructionLists.MaridiaElevatube;
    /// <summary><c>$84:9367</c>: the one-block physical elevatube draw list.</summary>
    internal const ushort DrawPointer = 0x9367;
    /// <summary><c>$84:B8F6</c>: library-two elevatube sound operand.</summary>
    internal const byte SoundId = 0x15;

    internal static readonly RoomPlmShotBlockDrawDefinitions.DrawList Draw =
        new(DrawPointer,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(1, new ushort[] { 0x8180 }, 0, 0),
            });

    internal const string VisualId = "elevatube-block";

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws =>
        [Draw];

    internal static string DrawVisualId(ushort pointer) => pointer == DrawPointer
        ? VisualId
        : throw new InvalidDataException(
            $"Maridia elevatube draw ${pointer:X4} has no visual ID.");

    internal static bool TryGetDrawByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (string.Equals(id, VisualId, StringComparison.Ordinal))
        {
            draw = Draw;
            return true;
        }
        draw = default;
        return false;
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = address switch
        {
            InstructionList => 16,
            InstructionList + 2 => DrawPointer,
            InstructionList + 4 => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
            InstructionList + 7 => RoomPlmInstructionCodes.Delete,
            _ => 0,
        };
        return address is InstructionList or InstructionList + 2 or
            InstructionList + 4 or InstructionList + 7;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == InstructionList + 6)
        {
            value = SoundId;
            return true;
        }
        value = 0;
        return false;
    }
}
