namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What a PLM starts running (#1296): a bank-$84 list the native interpreter still runs, or one of the shared typed
/// programs. Families that choose among such starts hold this instead of a raw list address, so a typed program can
/// stand where a native list did.
/// </summary>
public readonly record struct PlmProgramStart
{
    /// <summary>Holds a native list or a shared program; exactly one is set.</summary>
    private PlmProgramStart(ushort nativeList, RoomPlmSharedProgram? sharedProgram)
    {
        NativeList = nativeList;
        SharedProgram = sharedProgram;
    }

    /// <summary>The native list, or zero when the start is a typed program.</summary>
    internal ushort NativeList { get; }

    /// <summary>The shared typed program, or null when the start is a native list.</summary>
    internal RoomPlmSharedProgram? SharedProgram { get; }

    /// <summary>The shared delete: the PLM deletes itself on its first pass.</summary>
    internal static PlmProgramStart Delete { get; } = new(0, RoomPlmSharedProgram.Delete);

    /// <summary>A native bank-$84 list the interpreter runs; zero is not a list.</summary>
    internal static PlmProgramStart Native(ushort list) =>
        list != 0 ? new(list, null) : throw new InvalidDataException("A native PLM start needs a list.");

    /// <summary>Starts this program on <paramref name="slot"/>; the caller keeps the slot's timer.</summary>
    internal void StartOn(PlmSlot slot)
    {
        if (SharedProgram is { } program)
            slot.StartAnimation(new RoomPlmSharedAnimation(program));
        else
            slot.InstructionPointer = NativeList;
    }

    /// <inheritdoc/>
    public override string ToString() => SharedProgram is { } program ? $"shared {program}" : $"$84:{NativeList:X4}";
}
