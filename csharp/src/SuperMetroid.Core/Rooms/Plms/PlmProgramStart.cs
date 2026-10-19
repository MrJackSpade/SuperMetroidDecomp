using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What a PLM starts running (#1296): one of the families' typed programs. Families that choose among starts hold this, so
/// any family's program can stand where the cartridge names a list.
/// </summary>
public readonly record struct PlmProgramStart
{
    /// <summary>Holds a typed program.</summary>
    private PlmProgramStart(Enum typedProgram) => TypedProgram = typedProgram;

    /// <summary>The typed program, as its family's program enum.</summary>
    internal Enum TypedProgram { get; }

    /// <summary>The shared delete: the PLM deletes itself on its first pass.</summary>
    internal static PlmProgramStart Delete { get; } = Typed(RoomPlmSharedProgram.Delete);

    /// <summary>One of the shared typed programs.</summary>
    internal static PlmProgramStart Typed(RoomPlmSharedProgram program) => new(program);

    /// <summary>A Speed Booster block program.</summary>
    internal static PlmProgramStart Typed(SpeedBoosterBlockProgram program) => new(program);

    /// <summary>A blue door cap program.</summary>
    internal static PlmProgramStart Typed(BlueDoorProgram program) => new(program);

    /// <summary>A block-reaction program.</summary>
    internal static PlmProgramStart Typed(BlockReactionPlmProgram program) => new(program);

    /// <summary>A scripted room program.</summary>
    internal static PlmProgramStart Typed(ScriptedRoomPlmProgram program) => new(program);

    /// <summary>Starts this program on <paramref name="slot"/>; the caller keeps the slot's timer.</summary>
    internal void StartOn(PlmSlot slot)
    {
        switch (TypedProgram)
        {
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
            case ScriptedRoomPlmProgram scripted:
                slot.StartAnimation(new ScriptedRoomPlmAnimation(scripted));
                break;
            default:
                throw new InvalidOperationException($"PLM start names an unsupported typed program {TypedProgram}.");
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"typed {TypedProgram}";
}
