using SuperMetroid.Tooling;
using System.Reflection;

namespace SuperMetroid.Core.Game;

/// <summary>An instruction-program catalog whose compiled mechanics words can be enumerated.</summary>
internal interface IInstructionProgramCatalog
{
    /// <summary>Gets the number of native instruction words that describe mechanics rather than presentation.</summary>
    static abstract int MechanicsWordCount { get; }

    /// <summary>Returns the indexed mechanics word in the catalog's stable declaration order.</summary>
    /// <param name="index">Zero-based position below <see cref="MechanicsWordCount"/>.</param>
    /// <returns>The address/value pair consumed by mechanics audits and generators.</returns>
    static abstract InstructionMechanicsWord MechanicsWord(int index);
}

/// <summary>A catalog whose sprite-pointer presentation operands are an indexed list.</summary>
internal interface IPresentationOperandCatalog
{
    /// <summary>Gets the number of instruction operands that select sprite frames.</summary>
    static abstract int PresentationWordCount { get; }

    /// <summary>Returns the bank-local address of an indexed presentation operand.</summary>
    /// <param name="index">Zero-based position below <see cref="PresentationWordCount"/>.</param>
    /// <returns>The operand address used to look up its compiled frame identity.</returns>
    static abstract ushort PresentationWordAddress(int index);
}

/// <summary>A one-frame catalog whose program has a single presentation operand.</summary>
internal interface ISinglePresentationOperand
{
    /// <summary>Gets the sole instruction operand that selects the catalog's displayed frame.</summary>
    static abstract ushort PresentationWord { get; }
}

/// <summary>A catalog that can tell which bank bytes its compiled mechanics words own.</summary>
internal interface ICompiledMechanicsByteProbe
{
    /// <summary>Tests whether a bank address is occupied by a compiled mechanics word.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address falls inside mechanics data owned by the catalog.</returns>
    static abstract bool IsCompiledMechanicsByte(int address);
}

/// <summary>A catalog whose program bank is declared rather than probed.</summary>
internal interface IDeclaredProgramBank
{
    /// <summary>Gets the program bank declared by the catalog without probing individual bytes.</summary>
    static abstract int Bank { get; }
}

/// <summary>
/// Development-tool view of one instruction-program catalog: whichever of the catalog contracts
/// the type implements, read through typed generic helpers. Gameplay reads its own catalogs
/// directly; resource audits, the selector generator and layout dumps enumerate them here.
/// A shipped catalog type implements the contracts through its <see cref="ToolingForAttribute"/>
/// adapter; <see cref="Type"/> is always the shipped type.
/// </summary>
/// <param name="Type">Shipped catalog type whose development implementation is inspected.</param>
/// <param name="MechanicsWords">Indexed mechanics words when the catalog exposes that contract.</param>
/// <param name="PresentationOperands">Presentation operand addresses when the catalog exposes that contract.</param>
/// <param name="SinglePresentationOperand">The sole presentation address for single-frame catalogs, if present.</param>
/// <param name="CompiledMechanicsByteProbe">Address predicate for catalogs that can report compiled mechanics ownership.</param>
/// <param name="DeclaredBank">Declared bank for catalogs that expose a fixed program bank.</param>
internal sealed record InstructionProgramCatalog(
    Type Type,
    IReadOnlyList<InstructionMechanicsWord>? MechanicsWords,
    IReadOnlyList<ushort>? PresentationOperands,
    ushort? SinglePresentationOperand,
    Func<int, bool>? CompiledMechanicsByteProbe,
    int? DeclaredBank)
{
    /// <summary>Contracts that identify the supported instruction-program catalog capabilities.</summary>
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

    /// <summary>Builds a tooling view of a shipped catalog, with null values for contracts its implementation does not support.</summary>
    /// <param name="type">Shipped catalog type to inspect and preserve in the returned view.</param>
    /// <returns>Catalog metadata and delegates for each implemented tooling contract.</returns>
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

    /// <summary>Invokes the closed generic reader corresponding to a contract, or returns its default when unsupported.</summary>
    /// <typeparam name="T">Value supplied by the selected generic reader.</typeparam>
    /// <param name="helper">Name of the private static generic reader method.</param>
    /// <param name="contract">Capability interface that must be implemented.</param>
    /// <param name="type">Development implementation to close the helper over.</param>
    /// <returns>The bound contract value, or the default value when the type lacks that contract.</returns>
    private static T? Bind<T>(string helper, Type contract, Type type) =>
        contract.IsAssignableFrom(type)
            ? (T)typeof(InstructionProgramCatalog).GetMethod(helper, BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type).Invoke(null, null)!
            : default;

    /// <summary>Copies a catalog's indexed mechanics words into an immutable-by-convention tooling list.</summary>
    /// <typeparam name="TCatalog">Catalog type implementing the mechanics-word contract.</typeparam>
    /// <returns>Mechanics words in the order defined by the catalog.</returns>
    private static IReadOnlyList<InstructionMechanicsWord> MechanicsWordsOf<TCatalog>()
        where TCatalog : IInstructionProgramCatalog =>
        [.. Enumerable.Range(0, TCatalog.MechanicsWordCount).Select(index => TCatalog.MechanicsWord(index))];

    /// <summary>Copies a catalog's indexed presentation operand addresses into a tooling list.</summary>
    /// <typeparam name="TCatalog">Catalog type implementing the indexed presentation contract.</typeparam>
    /// <returns>Presentation addresses in the order defined by the catalog.</returns>
    private static IReadOnlyList<ushort> PresentationOperandsOf<TCatalog>()
        where TCatalog : IPresentationOperandCatalog =>
        [.. Enumerable.Range(0, TCatalog.PresentationWordCount).Select(index => TCatalog.PresentationWordAddress(index))];

    /// <summary>Reads the single presentation address exposed by a one-frame catalog.</summary>
    /// <typeparam name="TCatalog">Catalog type implementing the single-presentation contract.</typeparam>
    /// <returns>The catalog's sole presentation operand.</returns>
    private static ushort? SinglePresentationOperandOf<TCatalog>()
        where TCatalog : ISinglePresentationOperand => TCatalog.PresentationWord;

    /// <summary>Creates an address probe that delegates mechanics-byte ownership to the catalog.</summary>
    /// <typeparam name="TCatalog">Catalog type implementing the compiled mechanics byte-probe contract.</typeparam>
    /// <returns>A predicate forwarding addresses to the catalog's static ownership check.</returns>
    private static Func<int, bool> CompiledMechanicsByteProbeOf<TCatalog>()
        where TCatalog : ICompiledMechanicsByteProbe => address => TCatalog.IsCompiledMechanicsByte(address);

    /// <summary>Reads the fixed bank declared by a catalog that exposes that capability.</summary>
    /// <typeparam name="TCatalog">Catalog type implementing the declared-bank contract.</typeparam>
    /// <returns>The bank value provided by the catalog.</returns>
    private static int? DeclaredBankOf<TCatalog>()
        where TCatalog : IDeclaredProgramBank => TCatalog.Bank;
}
