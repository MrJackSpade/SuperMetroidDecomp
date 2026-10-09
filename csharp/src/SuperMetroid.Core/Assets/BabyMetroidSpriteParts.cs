using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Exact bilateral composition of the three native Baby poses; independent half-artwork and drawing order remain required.</summary>
internal sealed class BabyMetroidSpriteParts : EnemySpritemapParts
{
    /// <summary>Unreflected half-sprite records owned by this pose.</summary>
    private readonly EnemySpritemapPart[] halfParts;
    /// <summary>Signed one-based half-part indices in draw order; a negative entry requests horizontal reflection.</summary>
    private readonly sbyte[] drawingOrder;
    /// <summary>Optional first-pose component used to share unchanged dome parts across poses.</summary>
    private readonly BabyMetroidSpriteParts? sharedBody;
    /// <summary>Optional signed one-based references selecting local or shared half-parts and their reflection.</summary>
    private readonly sbyte[]? halfSelections;

    /// <summary>Creates a pose from owned half-parts, draw-order references, and optional shared-body selections.</summary>
    /// <param name="halfParts">Unreflected parts stored locally by this pose.</param>
    /// <param name="drawingOrder">Signed one-based indices that expand half-parts into the complete ordered spritemap.</param>
    /// <param name="sharedBody">Earlier pose supplying reusable half-parts, when this pose shares components.</param>
    /// <param name="halfSelections">Optional signed mapping from half-part indices to local or shared storage.</param>
    private BabyMetroidSpriteParts(EnemySpritemapPart[] halfParts, sbyte[] drawingOrder,
        BabyMetroidSpriteParts? sharedBody = null, sbyte[]? halfSelections = null)
    {
        this.halfParts = halfParts;
        this.drawingOrder = drawingOrder;
        this.sharedBody = sharedBody;
        this.halfSelections = halfSelections;
    }
    /// <summary>Gets the number of complete sprite records emitted by the bilateral composition.</summary>
    public override int Count => drawingOrder.Length;

    /// <summary>Gets a sprite record in draw order, reflecting it when its order entry has a negative sign.</summary>
    /// <param name="index">Zero-based index in the expanded spritemap.</param>
    /// <returns>The selected local or shared half-part with its requested horizontal orientation.</returns>
    public override EnemySpritemapPart this[int index]
    {
        get
        {
            int selected = drawingOrder[index];
            EnemySpritemapPart part = HalfPart(Math.Abs(selected) - 1);
            return selected < 0 ? Reflect(part) : part;
        }
    }

    /// <summary>Builds a compact bilateral representation for a recognized native Baby pose, sharing unchanged body parts when possible.</summary>
    /// <param name="identity">Spritemap identity used to select one of the three native poses.</param>
    /// <param name="supplied">Complete supplied records in native drawing order.</param>
    /// <param name="bodyTemplate">First-pose component reused for matching unchanged dome records.</param>
    /// <returns>A compact composition for a recognized symmetric pose, or ordinary owned records when it cannot be compiled.</returns>
    internal static EnemySpritemapParts Compile(int identity, EnemySpritemapPart[] supplied, ref BabyMetroidSpriteParts? bodyTemplate)
    {
        int offset = identity - BabyMetroidCompositionDefinitions.FirstPose;
        if (offset < 0 || offset % BabyMetroidCompositionDefinitions.PoseStride != 0 || offset / BabyMetroidCompositionDefinitions.PoseStride >= 3 || supplied.Length != BabyMetroidCompositionDefinitions.NativePartCount)
            return FromOwnedArray(supplied);
        var halves = new List<EnemySpritemapPart>(BabyMetroidCompositionDefinitions.NativePartCount / 2);
        var order = new sbyte[supplied.Length];
        for (int index = 0; index < supplied.Length; index++)
        {
            if (order[index] != 0) continue;
            EnemySpritemapPart reflected = Reflect(supplied[index]);
            int opposite = -1;
            for (int candidate = index + 1; candidate < supplied.Length; candidate++)
                if (order[candidate] == 0 && supplied[candidate] == reflected)
                { opposite = candidate; break; }
            if (opposite < 0) return FromOwnedArray(supplied);
            halves.Add(supplied[index]);
            order[index] = (sbyte)halves.Count;
            order[opposite] = (sbyte)-halves.Count;
        }
        if (bodyTemplate is null)
        {
            bodyTemplate = new BabyMetroidSpriteParts(halves.ToArray(), order);
            return bodyTemplate;
        }
        var local = new List<EnemySpritemapPart>();
        var selections = new sbyte[halves.Count];
        for (int index = 0; index < halves.Count; index++)
        {
            EnemySpritemapPart part = halves[index];
            // The native negative-Y dome has nine identical half-parts across all
            // three poses. Match complete supplied records, including orientation;
            // edited parts remain local and never mutate another pose's component.
            if (unchecked((sbyte)part.Y) < 0)
            {
                for (int candidate = 0; candidate < bodyTemplate.halfParts.Length; candidate++)
                {
                    EnemySpritemapPart shared = bodyTemplate.halfParts[candidate];
                    if (part == shared) { selections[index] = (sbyte)-(2 * candidate + 1); break; }
                    if (part == Reflect(shared)) { selections[index] = (sbyte)-(2 * candidate + 2); break; }
                }
            }
            if (selections[index] != 0) continue;
            local.Add(part);
            selections[index] = (sbyte)local.Count;
        }
        return new BabyMetroidSpriteParts(local.ToArray(), order, bodyTemplate, selections);
    }

    /// <summary>Resolves a half-part selection from local storage or the shared body template.</summary>
    /// <param name="index">Zero-based index into the half-part selection map.</param>
    /// <returns>The selected part, reflecting a shared template entry when its selection encodes the opposite orientation.</returns>
    private EnemySpritemapPart HalfPart(int index)
    {
        if (halfSelections is null) return halfParts[index];
        int selection = halfSelections[index];
        if (selection > 0) return halfParts[selection - 1];
        int shared = -selection - 1;
        EnemySpritemapPart part = sharedBody!.halfParts[shared / 2];
        return (shared & 1) == 0 ? part : Reflect(part);
    }

    /// <summary>Mirrors a sprite record horizontally, accounting for its tile width and toggling its horizontal flip bit.</summary>
    /// <param name="part">Unreflected sprite record to mirror.</param>
    /// <returns>A record with a reflected X position and horizontal-flip attribute.</returns>
    private static EnemySpritemapPart Reflect(EnemySpritemapPart part)
    {
        int size = part.X.IsLarge ? 16 : 8;
        int x = -part.X.SignedOffset - size;
        return part with
        {
            X = new SnesSpritemapXWord((ushort)((part.X.Raw & ~0x1ff) | (x & 0x1ff))),
            Attributes = new SnesObjAttributeWord((ushort)(part.Attributes.Raw ^ (ushort)SnesTileFlipFlags.Horizontal)),
        };
    }
}
