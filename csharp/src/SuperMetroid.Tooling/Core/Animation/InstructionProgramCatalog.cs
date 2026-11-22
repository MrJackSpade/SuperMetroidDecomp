using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Animation;

/// <summary>
/// Development-tool view of one instruction-program catalog: whichever of the catalog contracts its development adapter
/// implements, read through typed generic helpers. Gameplay reads its own catalogs directly; resource audits and layout
/// dumps enumerate them here. A shipped catalog type implements the contracts through its <see cref="ToolingForAttribute"/>
/// adapter, and <see cref="Registered"/> names each pairing (#2602); the program audit fails on an adapter missing from it,
/// since its owner then has no recognized shape. <see cref="Name"/> and <see cref="FullName"/> are the shipped type's.
/// </summary>
internal sealed record InstructionProgramCatalog(
    string Name,
    string FullName,
    IReadOnlyList<InstructionMechanicsWord>? MechanicsWords,
    IReadOnlyList<ushort>? PresentationOperands,
    ushort? SinglePresentationOperand,
    int? DeclaredBank)
{
    /// <summary>Every shipped program with a development adapter, each read through the adapter that implements its contracts.</summary>
    private static readonly InstructionProgramCatalog[] Registered =
    [
        Indexed<FakeKraidProjectileInstructionProgramDefinitionsTooling>(typeof(FakeKraidProjectileAnimation)),
        Indexed<MotherBrainTopTubeInstructionProgramDefinitionsTooling>(typeof(MotherBrainTopTubeInstructionProgramDefinitions)),
        Indexed<OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsTooling>(typeof(OldTourianEscapeShaftWallExplosionAnimationDefinitions)),
        Indexed<SpacePirateProjectileInstructionProgramDefinitionsTooling>(typeof(SpacePirateProjectileInstructionProgramDefinitions)),
        Indexed<TourianStatueProjectileInstructionProgramDefinitionsTooling>(typeof(TourianStatueProjectileInstructionProgramDefinitions)),
    ];

    /// <summary>Every registered catalog, in ordinal name order of the shipped type.</summary>
    internal static IReadOnlyList<InstructionProgramCatalog> All() =>
        [.. Registered.OrderBy(catalog => catalog.Name, StringComparer.Ordinal)];

    /// <summary>
    /// The catalog view of the shipped type named <paramref name="fullName"/>, with null for every contract when no adapter
    /// is registered for it.
    /// </summary>
    internal static InstructionProgramCatalog Of(string fullName) =>
        Registered.SingleOrDefault(catalog => catalog.FullName == fullName) ??
        new(fullName[(fullName.LastIndexOf('.') + 1)..], fullName, null, null, null, null);

    /// <summary>
    /// The view of <paramref name="owner"/> through its adapter <typeparamref name="TAdapter"/>, which enumerates the
    /// program's mechanics words and presentation operands and declares its bank.
    /// </summary>
    private static InstructionProgramCatalog Indexed<TAdapter>(Type owner)
        where TAdapter : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank =>
        new(owner.Name, owner.FullName ?? throw new InvalidOperationException($"{owner.Name} has no full name."),
            MechanicsWordsOf<TAdapter>(), PresentationOperandsOf<TAdapter>(), null, TAdapter.Bank);

    /// <summary>Copies a catalog's compiled mechanics words into an audit-friendly list.</summary>
    private static IReadOnlyList<InstructionMechanicsWord> MechanicsWordsOf<TCatalog>()
        where TCatalog : IInstructionProgramCatalog =>
        [.. Enumerable.Range(0, TCatalog.MechanicsWordCount).Select(index => TCatalog.MechanicsWord(index))];

    /// <summary>Copies all native sprite-pointer operand addresses from a catalog.</summary>
    private static IReadOnlyList<ushort> PresentationOperandsOf<TCatalog>()
        where TCatalog : IPresentationOperandCatalog =>
        [.. Enumerable.Range(0, TCatalog.PresentationWordCount).Select(index => TCatalog.PresentationWordAddress(index))];
}
