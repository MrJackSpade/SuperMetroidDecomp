using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a PLM starts running (#1296): one of the families' typed programs. Families that choose among starts hold this, so
/// any family's program can stand where the original game names a list.
/// </summary>
public readonly record struct RoomObjectProgramStart
{
    /// <summary>Holds a typed program.</summary>
    private RoomObjectProgramStart(Enum typedProgram) => TypedProgram = typedProgram;

    /// <summary>The typed program, as its family's program enum.</summary>
    internal Enum TypedProgram { get; }

    /// <summary>The shared delete: the PLM deletes itself on its first pass.</summary>
    internal static RoomObjectProgramStart Delete { get; } = Typed(RoomObjectSharedProgram.Delete);

    /// <summary>One of the shared typed programs.</summary>
    internal static RoomObjectProgramStart Typed(RoomObjectSharedProgram program) => new(program);

    /// <summary>A blue door cap program.</summary>
    internal static RoomObjectProgramStart Typed(BlueDoorProgram program) => new(program);

    /// <summary>A scripted room program.</summary>
    internal static RoomObjectProgramStart Typed(ScriptedRoomObjectProgram program) => new(program);

    /// <summary>Starts this program on <paramref name="slot"/>; the caller keeps the slot's timer.</summary>
    internal void StartOn(RoomObjectSlot slot)
    {
        switch (TypedProgram)
        {
            case RoomObjectSharedProgram shared:
                slot.StartAnimation(new RoomObjectSharedAnimation(shared));
                break;
            case BlueDoorProgram blueDoor:
                slot.StartAnimation(new BlueDoorObjectAnimation(blueDoor));
                break;
            case ScriptedRoomObjectProgram scripted:
                slot.StartAnimation(new ScriptedRoomObjectAnimation(scripted));
                break;
            default:
                throw new InvalidOperationException($"PLM start names an unsupported typed program {TypedProgram}.");
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"typed {TypedProgram}";
}
