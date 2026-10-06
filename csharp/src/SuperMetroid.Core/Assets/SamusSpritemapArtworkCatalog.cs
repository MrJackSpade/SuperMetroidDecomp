using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable visual composition behind the bank-$92 Samus OAM pointer table.</summary>
/// <remarks>
/// The 253 pose selectors and 2,096 indexed pointers preserve native identity;
/// the selected compositions and their five-byte sprite parts describe appearance; timing and collisions
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

    private readonly Dictionary<int, ushort> topBases;
    private readonly Dictionary<int, ushort> bottomBases;
    private readonly Dictionary<int, ushort> pointers;
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
        this.topBases = Enumerable.Range(0, topBases.Length)
            .Where(pose => topBases[pose] != SamusSpritemapPoseDefinitions.TopBase((byte)pose))
            .ToDictionary(pose => pose, pose => topBases[pose]);
        this.bottomBases = Enumerable.Range(0, bottomBases.Length)
            .Where(pose => bottomBases[pose] != SamusSpritemapPoseDefinitions.BottomBase((byte)pose))
            .ToDictionary(pose => pose, pose => bottomBases[pose]);
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
        foreach (ushort pointer in pointers)
            if (pointer != 0 && !this.definitions.ContainsKey(pointer))
                throw new InvalidDataException($"Samus spritemap ${pointer:X4} is absent from installed art.");
        this.pointers = Enumerable.Range(0, pointers.Length)
            .Where(index => SamusSpritemapFrameDefinitions.SourceIndex(index) != index
                ? pointers[index] != pointers[SamusSpritemapFrameDefinitions.SourceIndex(index)]
                : !SamusSpritemapFrameDefinitions.TryPointer(index, this.definitions, out ushort calculated) || pointers[index] != calculated)
            .ToDictionary(index => index, index => pointers[index]);
        foreach (ushort index in topBases.Concat(bottomBases))
            if (index >= PointerCount)
                throw new InvalidDataException($"Samus spritemap base index {index} is outside the table.");
    }

    /// <summary>SHA-256 of selected pose bases, frame pointers and ordered OBJ composition parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusSpritemapArtworkCatalog), content =>
    {
        content.AppendWords("top bases", TopBases);
        content.AppendWords("bottom bases", BottomBases);
        content.AppendWords("pointers", Pointers);
        foreach ((ushort pointer, SamusSpritemapDefinition definition) in this.definitions.OrderBy(pair => pair.Key))
        {
            content.Append("pointer", pointer);
            content.Append("part count", definition.Parts.Length);
            foreach (SamusSpritePart part in definition.Parts)
            {
                content.Append("x", part.X);
                content.Append("y", part.Y);
                content.Append("attributes", part.Attributes);
            }
        }
    });

    public ReadOnlySpan<ushort> TopBases => Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount).Select(pose => TopBase((byte)pose)).ToArray();
    public ReadOnlySpan<ushort> BottomBases => Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount).Select(pose => BottomBase((byte)pose)).ToArray();
    public ReadOnlySpan<ushort> Pointers => Enumerable.Range(0, PointerCount).Select(Pointer).ToArray();
    private ushort Pointer(int index) => pointers.TryGetValue(index, out ushort value)
        ? value : SamusSpritemapFrameDefinitions.SourceIndex(index) != index
            ? Pointer(SamusSpritemapFrameDefinitions.SourceIndex(index))
            : SamusSpritemapFrameDefinitions.TryPointer(index, definitions, out ushort calculated)
                ? calculated : throw new InvalidDataException("Installed OAM allocation no longer supplies its selected identity.");
    public IReadOnlyCollection<SamusSpritemapDefinition> Definitions => definitions.Values;
    public ushort TopBase(byte pose)
    {
        if (pose >= SamusBodyArtworkCatalog.PoseCount) throw new IndexOutOfRangeException();
        return topBases.TryGetValue(pose, out ushort value) ? value : SamusSpritemapPoseDefinitions.TopBase(pose);
    }
    public ushort BottomBase(byte pose)
    {
        if (pose >= SamusBodyArtworkCatalog.PoseCount) throw new IndexOutOfRangeException();
        return bottomBases.TryGetValue(pose, out ushort value) ? value : SamusSpritemapPoseDefinitions.BottomBase(pose);
    }

    /// <summary>Returns false only for a native zero pointer, which requires a bus read.</summary>
    public bool TryGet(ushort index, out SamusSpritemapDefinition? definition)
    {
        if (index >= PointerCount)
            throw new InvalidDataException($"Samus spritemap index {index} is outside extracted art.");
        ushort pointer = Pointer(index);
        definition = pointer == 0 ? null : definitions[pointer];
        return definition is not null;
    }
}

/// <summary>A native bank-$92 spritemap record and its editable five-byte OAM parts.</summary>
public sealed record SamusSpritemapDefinition(ushort Pointer, SamusSpritePart[] Parts);

/// <summary>Encoded X/size word, signed-wrap Y byte, and complete OBJ attribute word.</summary>
public readonly record struct SamusSpritePart(
    [property: JsonRequired] ushort X,
    [property: JsonRequired] byte Y,
    [property: JsonRequired] ushort Attributes);
