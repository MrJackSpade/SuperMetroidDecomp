using SuperMetroid.Tooling;
using System.Reflection;

namespace SuperMetroid.Core.Game;

/// <summary>An instruction-program catalog whose compiled mechanics words can be enumerated.</summary>
internal interface IInstructionProgramCatalog
{
    /// <summary>Gets mechanics word count.</summary>
    static abstract int MechanicsWordCount { get; }

    /// <summary>Mechanics word.</summary>
    static abstract InstructionMechanicsWord MechanicsWord(int index);
}

/// <summary>A catalog whose sprite-pointer presentation operands are an indexed list.</summary>
internal interface IPresentationOperandCatalog
{
    /// <summary>Gets presentation word count.</summary>
    static abstract int PresentationWordCount { get; }

    /// <summary>Presentation word address.</summary>
    static abstract ushort PresentationWordAddress(int index);
}

/// <summary>A one-frame catalog whose program has a single presentation operand.</summary>
internal interface ISinglePresentationOperand
{
    /// <summary>Gets presentation word.</summary>
    static abstract ushort PresentationWord { get; }
}

/// <summary>A catalog that can tell which bank bytes its compiled mechanics words own.</summary>
internal interface ICompiledMechanicsByteProbe
{
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    static abstract bool IsCompiledMechanicsByte(int address);
}

/// <summary>A catalog whose program bank is declared rather than probed.</summary>
internal interface IDeclaredProgramBank
{
    /// <summary>Gets bank.</summary>
    static abstract int Bank { get; }
}

/// <summary>
/// Development-tool view of one instruction-program catalog: whichever of the catalog contracts
/// the type implements, read through typed generic helpers. Gameplay reads its own catalogs
/// directly; resource audits, the selector generator and layout dumps enumerate them here.
/// A shipped catalog type implements the contracts through its <see cref="ToolingForAttribute"/>
/// adapter; <see cref="Type"/> is always the shipped type.
/// </summary>
internal sealed record InstructionProgramCatalog(
    Type Type,
    IReadOnlyList<InstructionMechanicsWord>? MechanicsWords,
    IReadOnlyList<ushort>? PresentationOperands,
    ushort? SinglePresentationOperand,
    Func<int, bool>? CompiledMechanicsByteProbe,
    int? DeclaredBank)
{
    /// <summary>Stores contracts.</summary>
    private static readonly Type[] Contracts =
    [
        typeof(IInstructionProgramCatalog), typeof(IPresentationOperandCatalog), typeof(ISinglePresentationOperand),
        typeof(ICompiledMechanicsByteProbe), typeof(IDeclaredProgramBank),
    ];

    /// <summary>Shipped catalog type to the development type implementing its contracts.</summary>
    private static readonly Dictionary<Type, Type> Implementations = typeof(InstructionProgramCatalog).Assembly.GetTypes()
        .Where(type => Contracts.Any(contract => contract.IsAssignableFrom(type) && type != contract))
        .ToDictionary(type => type.GetCustomAttribute<ToolingForAttribute>()?.Owner ?? type);

    /// <summary>Every catalog implementing a contract, in ordinal name order of the shipped type.</summary>
    internal static IReadOnlyList<InstructionProgramCatalog> All() =>
        [.. Implementations.Keys.OrderBy(type => type.Name, StringComparer.Ordinal).Select(Of)];

    /// <summary>The catalog view of a type, with null for each contract it does not implement.</summary>
    internal static InstructionProgramCatalog Of(Type type)
    {
        Type implementation = Implementations.GetValueOrDefault(type, type);
        return new(type,
            Bind<IReadOnlyList<InstructionMechanicsWord>>(nameof(MechanicsWordsOf), typeof(IInstructionProgramCatalog), implementation),
            Bind<IReadOnlyList<ushort>>(nameof(PresentationOperandsOf), typeof(IPresentationOperandCatalog), implementation),
            Bind<ushort?>(nameof(SinglePresentationOperandOf), typeof(ISinglePresentationOperand), implementation),
            Bind<Func<int, bool>>(nameof(CompiledMechanicsByteProbeOf), typeof(ICompiledMechanicsByteProbe), implementation),
            Bind<int?>(nameof(DeclaredBankOf), typeof(IDeclaredProgramBank), implementation));
    }

    /// <summary>Binds.</summary>
    private static T? Bind<T>(string helper, Type contract, Type type) =>
        contract.IsAssignableFrom(type)
            ? (T)typeof(InstructionProgramCatalog).GetMethod(helper, BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type).Invoke(null, null)!
            : default;

    /// <summary>Mechanics words of.</summary>
    private static IReadOnlyList<InstructionMechanicsWord> MechanicsWordsOf<TCatalog>()
        where TCatalog : IInstructionProgramCatalog =>
        [.. Enumerable.Range(0, TCatalog.MechanicsWordCount).Select(index => TCatalog.MechanicsWord(index))];

    /// <summary>Presentation operands of.</summary>
    private static IReadOnlyList<ushort> PresentationOperandsOf<TCatalog>()
        where TCatalog : IPresentationOperandCatalog =>
        [.. Enumerable.Range(0, TCatalog.PresentationWordCount).Select(index => TCatalog.PresentationWordAddress(index))];

    /// <summary>Single presentation operand of.</summary>
    private static ushort? SinglePresentationOperandOf<TCatalog>()
        where TCatalog : ISinglePresentationOperand => TCatalog.PresentationWord;

    /// <summary>Compiled mechanics byte probe of.</summary>
    private static Func<int, bool> CompiledMechanicsByteProbeOf<TCatalog>()
        where TCatalog : ICompiledMechanicsByteProbe => address => TCatalog.IsCompiledMechanicsByte(address);

    /// <summary>Declared bank of.</summary>
    private static int? DeclaredBankOf<TCatalog>()
        where TCatalog : IDeclaredProgramBank => TCatalog.Bank;
}
