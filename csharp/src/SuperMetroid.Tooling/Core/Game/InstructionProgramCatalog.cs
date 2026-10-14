using SuperMetroid.Tooling;
using System.Reflection;

namespace SuperMetroid.Core.Game;

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
    /// <summary>The contracts recognized when discovering development-tool adapters.</summary>
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

    /// <summary>Invokes a typed enumerator only when the implementation supports its contract.</summary>
    [ReflectionAudit("catalog discovery: static abstract contract members are reached for types found by assembly scan")]
    private static T? Bind<T>(string helper, Type contract, Type type) =>
        contract.IsAssignableFrom(type)
            ? (T?)(typeof(InstructionProgramCatalog).GetMethod(helper, BindingFlags.NonPublic | BindingFlags.Static) ??
                throw new MissingMethodException(nameof(InstructionProgramCatalog), helper))
                .MakeGenericMethod(type).Invoke(null, null)
            : default;

    /// <summary>Copies a catalog's compiled mechanics words into an audit-friendly list.</summary>
    private static IReadOnlyList<InstructionMechanicsWord> MechanicsWordsOf<TCatalog>()
        where TCatalog : IInstructionProgramCatalog =>
        [.. Enumerable.Range(0, TCatalog.MechanicsWordCount).Select(index => TCatalog.MechanicsWord(index))];

    /// <summary>Copies all native sprite-pointer operand addresses from a catalog.</summary>
    private static IReadOnlyList<ushort> PresentationOperandsOf<TCatalog>()
        where TCatalog : IPresentationOperandCatalog =>
        [.. Enumerable.Range(0, TCatalog.PresentationWordCount).Select(index => TCatalog.PresentationWordAddress(index))];

    /// <summary>Boxes a catalog's sole sprite-pointer operand for the optional view.</summary>
    private static ushort? SinglePresentationOperandOf<TCatalog>()
        where TCatalog : ISinglePresentationOperand => TCatalog.PresentationWord;

    /// <summary>Adapts a catalog's static ownership check to a callable bank-address predicate.</summary>
    private static Func<int, bool> CompiledMechanicsByteProbeOf<TCatalog>()
        where TCatalog : ICompiledMechanicsByteProbe => address => TCatalog.IsCompiledMechanicsByte(address);

    /// <summary>Boxes the native program bank for the optional development-tool view.</summary>
    private static int? DeclaredBankOf<TCatalog>()
        where TCatalog : IDeclaredProgramBank => TCatalog.Bank;
}
