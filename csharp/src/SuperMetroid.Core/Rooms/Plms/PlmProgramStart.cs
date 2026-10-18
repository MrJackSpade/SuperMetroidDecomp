using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What a PLM starts running (#1296): a bank-$84 list the native interpreter still runs, or a typed program. Families
/// that choose among such starts hold this instead of a raw list address, so a typed program can stand where a native
/// list did.
/// </summary>
public readonly record struct PlmProgramStart
{
    /// <summary>Holds a native list or a typed program; exactly one is set.</summary>
    private PlmProgramStart(ushort nativeList, Enum? typedProgram)
    {
        NativeList = nativeList;
        TypedProgram = typedProgram;
    }

    /// <summary>The native list, or zero when the start is a typed program.</summary>
    internal ushort NativeList { get; }

    /// <summary>The typed program, as its family's program enum, or null when the start is a native list.</summary>
    internal Enum? TypedProgram { get; }

    /// <summary>The shared delete: the PLM deletes itself on its first pass.</summary>
    internal static PlmProgramStart Delete { get; } = Typed(RoomPlmSharedProgram.Delete);

    /// <summary>A native bank-$84 list the interpreter runs; zero is not a list.</summary>
    internal static PlmProgramStart Native(ushort list) =>
        list != 0 ? new(list, null) : throw new InvalidDataException("A native PLM start needs a list.");

    /// <summary>One of the shared typed programs.</summary>
    internal static PlmProgramStart Typed(RoomPlmSharedProgram program) => new(0, program);

    /// <summary>A Speed Booster block program.</summary>
    internal static PlmProgramStart Typed(SpeedBoosterBlockProgram program) => new(0, program);

    /// <summary>A blue door cap program.</summary>
    internal static PlmProgramStart Typed(BlueDoorProgram program) => new(0, program);

    /// <summary>A block-reaction program.</summary>
    internal static PlmProgramStart Typed(BlockReactionPlmProgram program) => new(0, program);

    /// <summary>Starts this program on <paramref name="slot"/>; the caller keeps the slot's timer.</summary>
    internal void StartOn(PlmSlot slot)
    {
        switch (TypedProgram)
        {
            case null:
                slot.InstructionPointer = NativeList;
                break;
            case RoomPlmSharedProgram shared:
                slot.StartAnimation(new RoomPlmSharedAnimation(shared));
                break;
            case SpeedBoosterBlockProgram speedBlock:
                slot.StartAnimation(new SpeedBoosterBlockPlmAnimation(speedBlock));
                break;
            case BlueDoorProgram blueDoor:
                slot.StartAnimation(new BlueDoorPlmAnimation(blueDoor));
                break;
            case BlockReactionPlmProgram blockReaction:
                slot.StartAnimation(new BlockReactionPlmAnimation(blockReaction));
                break;
            default:
                throw new InvalidOperationException($"PLM start names an unsupported typed program {TypedProgram}.");
        }
    }

    /// <inheritdoc/>
    public override string ToString() => TypedProgram is { } program ? $"typed {program}" : $"$84:{NativeList:X4}";
}
