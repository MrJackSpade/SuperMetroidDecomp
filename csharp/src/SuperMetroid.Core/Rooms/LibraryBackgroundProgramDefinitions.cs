using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Immutable bank-$8F room-background command programs for every retail room state.
/// These are engine-owned transfer/decompression instructions, not replaceable art;
/// their referenced character sheets and tilemaps are installed separately.
/// </summary>
public static partial class LibraryBackgroundProgramDefinitions
{
    /// <summary>Distinct high-bank command lists selected by the 323 retail states.</summary>
    public const int RetailProgramCount = 68;

    /// <summary>All pinned program definitions, ordered by native list pointer.</summary>
    public static IReadOnlyList<LibraryBackgroundProgram> All { get; }

    /// <summary>Checks the 68 compiled lists cover exactly the high-bank commands selected by retail room states.</summary>
    static LibraryBackgroundProgramDefinitions()
    {
        All = Array.AsReadOnly(programs);
        ushort[] selected = RoomStateDefinitions.All
            .Select(state => state.BackgroundDataPointer)
            .Where(pointer => unchecked((short)pointer) < 0)
            .Distinct()
            .Order()
            .ToArray();
        if (programs.Length != RetailProgramCount ||
            !programs.Select(program => program.Pointer).SequenceEqual(selected) ||
            programs.Any(program => program.NativeByteCount < sizeof(ushort) ||
                program.Instructions.Count >= RoomAssetRomData.LibraryBackground.MaximumCommandsPerList))
        {
            throw new InvalidDataException(
                "Compiled library-background programs do not cover the retail room states.");
        }
    }

    /// <summary>Finds a compiled retail command list. Constructed programs can be supplied explicitly, without byte decoding.</summary>
    /// <param name="pointer">16-bit bank-$8F list identity; values absent from the pinned retail catalog simply fail lookup.</param>
    /// <param name="program">Shared compiled definition on success, or null on failure; lookup does not execute transfers or load artwork.</param>
    /// <returns>True when the exact pointer is cataloged; false otherwise.</returns>
    public static bool TryGet(ushort pointer, [NotNullWhen(true)] out LibraryBackgroundProgram? program)
    {
        int low = 0;
        int high = programs.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            int comparison = programs[middle].Pointer.CompareTo(pointer);
            if (comparison == 0)
            {
                program = programs[middle];
                return true;
            }
            if (comparison < 0) low = middle + 1;
            else high = middle - 1;
        }
        program = null;
        return false;
    }
}
