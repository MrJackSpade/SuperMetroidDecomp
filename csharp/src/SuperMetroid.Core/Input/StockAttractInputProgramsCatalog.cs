namespace SuperMetroid.Core.Input;

/// <summary>Application-owned title demo controller definitions from bank $91. Addresses
/// are stable diagnostic/control-flow identities, never runtime ROM reads.</summary>
public static partial class StockAttractInputPrograms
{
    /// <summary>One native object header: initializer, pre-instruction and initial list.</summary>
    public readonly record struct ObjectDefinition(ushort Initializer, ushort PreInstruction, ushort Start);
    /// <summary>The three distinct operations reachable from shipped title-demo objects.</summary>
    public enum Operation { Input, Delete, Goto }
    /// <summary>A decoded input span or control transfer. Held and edge words remain independent.</summary>
    public readonly record struct Command(Operation Kind, ushort Next, ushort Duration = 0,
        SnesButton Held = SnesButton.None, SnesButton NewlyPressed = SnesButton.None);

    /// <summary>Resolves the six-byte stock object identity at $91:pointer.</summary>
    public static ObjectDefinition GetObject(ushort pointer) => pointer switch
    {
        0x9e52 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8ace),
        0x9e88 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x93cc),
        0x9eac => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x965a),
        0x9e5e => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8df0),
        0x9eb2 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x973a),
        0x9e58 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8c3e),
        0x9eb8 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x989e),
        0x9e94 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x946c),
        0x9ea0 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9560),
        0x9e76 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8fe4),
        0x9e9a => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x950a),
        0x9e64 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8e64),
        0x9e70 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8f3a),
        0x9e82 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.ShinesparkPreInstruction, 0x933c),
        0x9e7c => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9154),
        0x9e6a => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x8eb4),
        0x9e8e => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9464),
        0x9ed6 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9da6),
        0x9ebe => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x99ae),
        0x9ec4 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x99c8),
        0x9eca => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9af0),
        0x9ed0 => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9cd8),
        0x9edc => new(DemoInputRomData.Routines.NoOp, DemoInputRomData.Attract.CheckLeave, 0x9dae),
        _ => throw new InvalidDataException($"Unknown stock attract object $91:{pointer:X4}."),
    };

    /// <summary>Returns a typed stock operation; missing definitions fail loudly.</summary>
    public static Command GetCommand(ushort pointer) =>
        Commands.TryGetValue(pointer, out var command) ? command :
            throw new InvalidDataException($"Unknown stock attract command $91:{pointer:X4}.");

    private static readonly Dictionary<ushort, Command> Commands = CreateCommands();
    private static Dictionary<ushort, Command> CreateCommands()
    {
        var commands = new Dictionary<ushort, Command>();
        AddPart1(commands); AddPart2(commands); AddPart3(commands); AddPart4(commands);
        return commands;
    }
}
