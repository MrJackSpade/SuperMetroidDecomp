using System.Reflection;

namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned instruction word at its native address.</summary>
internal readonly record struct InstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>An instruction-program catalog whose compiled mechanics words can be enumerated.</summary>
internal interface IInstructionProgramCatalog
{
    static abstract int MechanicsWordCount { get; }

    static abstract InstructionMechanicsWord MechanicsWord(int index);
}

/// <summary>A catalog whose sprite-pointer presentation operands are an indexed list.</summary>
internal interface IPresentationOperandCatalog
{
    static abstract int PresentationWordCount { get; }

    static abstract ushort PresentationWordAddress(int index);
}

/// <summary>A one-frame catalog whose program has a single presentation operand.</summary>
internal interface ISinglePresentationOperand
{
    static abstract ushort PresentationWord { get; }
}

/// <summary>A catalog that can tell which bank bytes its compiled mechanics words own.</summary>
internal interface ICompiledMechanicsByteProbe
{
    static abstract bool IsCompiledMechanicsByte(int address);
}

/// <summary>A catalog whose program bank is declared rather than probed.</summary>
internal interface IDeclaredProgramBank
{
    static abstract int Bank { get; }
}

/// <summary>
/// Development-tool view of one instruction-program catalog: whichever of the catalog contracts
/// the type implements, read through typed generic helpers. Gameplay reads its own catalogs
/// directly; resource audits, the selector generator and layout dumps enumerate them here.
/// </summary>
internal sealed record InstructionProgramCatalog(
    Type Type,
    IReadOnlyList<InstructionMechanicsWord>? MechanicsWords,
    IReadOnlyList<ushort>? PresentationOperands,
    ushort? SinglePresentationOperand,
    Func<int, bool>? CompiledMechanicsByteProbe,
    int? DeclaredBank)
{
    private static readonly Type[] Contracts =
    [
        typeof(IInstructionProgramCatalog), typeof(IPresentationOperandCatalog), typeof(ISinglePresentationOperand),
        typeof(ICompiledMechanicsByteProbe), typeof(IDeclaredProgramBank),
    ];

    /// <summary>Every Core type implementing a catalog contract, in ordinal name order.</summary>
    internal static IReadOnlyList<InstructionProgramCatalog> All() =>
        [.. typeof(InstructionProgramCatalog).Assembly.GetTypes()
            .Where(type => Contracts.Any(contract => contract.IsAssignableFrom(type) && type != contract))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .Select(Of)];

    /// <summary>The catalog view of a type, with null for each contract it does not implement.</summary>
    internal static InstructionProgramCatalog Of(Type type) => new(type,
        Bind<IReadOnlyList<InstructionMechanicsWord>>(nameof(MechanicsWordsOf), typeof(IInstructionProgramCatalog), type),
        Bind<IReadOnlyList<ushort>>(nameof(PresentationOperandsOf), typeof(IPresentationOperandCatalog), type),
        Bind<ushort?>(nameof(SinglePresentationOperandOf), typeof(ISinglePresentationOperand), type),
        Bind<Func<int, bool>>(nameof(CompiledMechanicsByteProbeOf), typeof(ICompiledMechanicsByteProbe), type),
        Bind<int?>(nameof(DeclaredBankOf), typeof(IDeclaredProgramBank), type));

    private static T? Bind<T>(string helper, Type contract, Type type) =>
        contract.IsAssignableFrom(type)
            ? (T)typeof(InstructionProgramCatalog).GetMethod(helper, BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type).Invoke(null, null)!
            : default;

    private static IReadOnlyList<InstructionMechanicsWord> MechanicsWordsOf<TCatalog>()
        where TCatalog : IInstructionProgramCatalog =>
        [.. Enumerable.Range(0, TCatalog.MechanicsWordCount).Select(index => TCatalog.MechanicsWord(index))];

    private static IReadOnlyList<ushort> PresentationOperandsOf<TCatalog>()
        where TCatalog : IPresentationOperandCatalog =>
        [.. Enumerable.Range(0, TCatalog.PresentationWordCount).Select(index => TCatalog.PresentationWordAddress(index))];

    private static ushort? SinglePresentationOperandOf<TCatalog>()
        where TCatalog : ISinglePresentationOperand => TCatalog.PresentationWord;

    private static Func<int, bool> CompiledMechanicsByteProbeOf<TCatalog>()
        where TCatalog : ICompiledMechanicsByteProbe => address => TCatalog.IsCompiledMechanicsByte(address);

    private static int? DeclaredBankOf<TCatalog>()
        where TCatalog : IDeclaredProgramBank => TCatalog.Bank;
}
