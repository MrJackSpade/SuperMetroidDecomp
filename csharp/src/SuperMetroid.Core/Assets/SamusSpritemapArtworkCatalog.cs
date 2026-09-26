namespace SuperMetroid.Core.Assets;

/// <summary>Editable visual composition behind the bank-$92 Samus OAM pointer table.</summary>
/// <remarks>
/// The 253 pose selectors and 2,096 indexed pointers preserve native identity. Only
/// the five-byte sprite parts describe appearance; animation timing and collisions
/// remain in the simulation. A zero pointer is a native mutable-memory reference,
/// not an empty picture, and must still be resolved through the CPU address space.
/// </remarks>
public sealed class SamusSpritemapArtworkCatalog
{
    /// <summary>Native <c>$92:808D</c> word-pointer table, indexed by pose half/frame.</summary>
    public const int PointerTableAddress = 0x92808D;
    public const int PointerCount = 2096;
    /// <summary>Native <c>$92:9263</c> upper-half base-index table.</summary>
    public const int TopBaseAddress = 0x929263;
    /// <summary>Native <c>$92:945D</c> lower-half base-index table.</summary>
    public const int BottomBaseAddress = 0x92945D;

    private readonly ushort[] topBases;
    private readonly ushort[] bottomBases;
    private readonly ushort[] pointers;
    private readonly Dictionary<ushort, SamusSpritemapDefinition> definitions;

    public SamusSpritemapArtworkCatalog(ushort[] topBases, ushort[] bottomBases,
        ushort[] pointers, SamusSpritemapDefinition[] definitions)
    {
        ArgumentNullException.ThrowIfNull(topBases);
        ArgumentNullException.ThrowIfNull(bottomBases);
        ArgumentNullException.ThrowIfNull(pointers);
        ArgumentNullException.ThrowIfNull(definitions);
        if (topBases.Length != SamusBodyArtworkCatalog.PoseCount ||
            bottomBases.Length != SamusBodyArtworkCatalog.PoseCount ||
            pointers.Length != PointerCount)
            throw new InvalidDataException("Samus spritemap selector tables have an invalid length.");
        this.topBases = (ushort[])topBases.Clone();
        this.bottomBases = (ushort[])bottomBases.Clone();
        this.pointers = (ushort[])pointers.Clone();
        this.definitions = new Dictionary<ushort, SamusSpritemapDefinition>();
        foreach (SamusSpritemapDefinition definition in definitions)
        {
            if (definition is null || definition.Pointer < 0x90ED ||
                definition.Parts is null || definition.Parts.Length > 128 ||
                !this.definitions.TryAdd(definition.Pointer,
                    new SamusSpritemapDefinition(definition.Pointer,
                        (SamusSpritePart[])definition.Parts.Clone())))
                throw new InvalidDataException("Samus spritemap has an invalid or duplicate record.");
        }
        foreach (ushort pointer in this.pointers)
            if (pointer != 0 && !this.definitions.ContainsKey(pointer))
                throw new InvalidDataException($"Samus spritemap ${pointer:X4} is absent from installed art.");
        foreach (ushort index in this.topBases.Concat(this.bottomBases))
            if (index >= PointerCount)
                throw new InvalidDataException($"Samus spritemap base index {index} is outside the table.");
    }

    public ReadOnlySpan<ushort> TopBases => topBases;
    public ReadOnlySpan<ushort> BottomBases => bottomBases;
    public ReadOnlySpan<ushort> Pointers => pointers;
    public IReadOnlyCollection<SamusSpritemapDefinition> Definitions => definitions.Values;
    public ushort TopBase(byte pose) => topBases[pose];
    public ushort BottomBase(byte pose) => bottomBases[pose];

    /// <summary>Returns false only for a native zero pointer, which requires a bus read.</summary>
    public bool TryGet(ushort index, out SamusSpritemapDefinition? definition)
    {
        if (index >= PointerCount)
            throw new InvalidDataException($"Samus spritemap index {index} is outside extracted art.");
        ushort pointer = pointers[index];
        definition = pointer == 0 ? null : definitions[pointer];
        return definition is not null;
    }
}

/// <summary>A native bank-$92 spritemap record and its editable five-byte OAM parts.</summary>
public sealed record SamusSpritemapDefinition(ushort Pointer, SamusSpritePart[] Parts);

/// <summary>Encoded X/size word, signed-wrap Y byte, and complete OBJ attribute word.</summary>
public readonly record struct SamusSpritePart(ushort X, byte Y, ushort Attributes);
