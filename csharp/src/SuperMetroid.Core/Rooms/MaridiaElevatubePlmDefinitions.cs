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

    /// <summary>84:9369: solid block 180, shared with Kraid's physical draw. Block 180 is the one named visual scalar.</summary>
    internal const ushort PhysicalWord = 0x8180;

    // Temporary artwork DTO only. Gameplay draws the single physical word directly.
    /// <summary>One-block physical draw description used to identify the elevatube's replaceable artwork.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Draw =>
        new(DrawPointer,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(1, new ushort[] { PhysicalWord }, 0, 0),
            });

    /// <summary>Stable artwork key associated with the elevatube draw list.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>Enumerates the single physical draw list exported for the elevatube.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws
    {
        get { yield return Draw; }
    }

    /// <summary>Returns the artwork key for the elevatube's native draw-list pointer.</summary>
    /// <param name="pointer">Bank-local PLM draw pointer to resolve.</param>
    /// <returns>The stable key consumed by the extracted artwork catalog.</returns>
    /// <exception cref="InvalidDataException">The pointer is not the elevatube draw list.</exception>
    internal static string DrawVisualId(ushort pointer) => pointer == DrawPointer
        ? VisualId
        : throw new InvalidDataException(
            $"Maridia elevatube draw ${pointer:X4} has no visual ID.");

    /// <summary>Resolves the temporary draw description for the elevatube artwork key.</summary>
    /// <param name="id">Stable visual key being looked up.</param>
    /// <param name="draw">Receives the draw description on success, or the default value otherwise.</param>
    /// <returns><see langword="true"/> when <paramref name="id"/> matches <see cref="VisualId"/>.</returns>
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

    /// <summary>Reads a compiled instruction or operand word from the elevatube's instruction sequence.</summary>
    /// <param name="address">Bank-local address queried by the PLM interpreter.</param>
    /// <param name="value">Receives the compiled word when recognized, or zero otherwise.</param>
    /// <returns><see langword="true"/> when the address contains a word-sized instruction or operand.</returns>
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

    /// <summary>Reads the byte-sized library-two sound identifier embedded in the elevatube command.</summary>
    /// <param name="address">Bank-local address queried by the PLM interpreter.</param>
    /// <param name="value">Receives the sound identifier when recognized, or zero otherwise.</param>
    /// <returns><see langword="true"/> only for the sound operand address.</returns>
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
