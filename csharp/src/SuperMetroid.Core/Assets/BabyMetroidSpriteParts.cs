using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Exact bilateral composition of the three native Baby poses; independent half-artwork and drawing order remain required.</summary>
internal sealed class BabyMetroidSpriteParts : EnemySpritemapParts
{
    /// <summary>$A9:F9A8/FA40/FAD8: thirty five-byte records in each Baby Metroid pose, arranged as fifteen reflected pairs.</summary>
    private const int NativePartCount = 30;
    /// <summary>$A9:F9A8, Spritemap_BabyMetroid_0; following two counted records have the same thirty-part extent.</summary>
    private const int FirstPose = 0xa9f9a8;
    /// <summary>Native two-byte count followed by thirty five-byte OBJ records.</summary>
    private const int PoseStride = 2 + NativePartCount * 5;
    private readonly EnemySpritemapPart[] halfParts;
    private readonly sbyte[] drawingOrder;
    private readonly BabyMetroidSpriteParts? sharedBody;
    private readonly sbyte[]? halfSelections;

    private BabyMetroidSpriteParts(EnemySpritemapPart[] halfParts, sbyte[] drawingOrder,
        BabyMetroidSpriteParts? sharedBody = null, sbyte[]? halfSelections = null)
    {
        this.halfParts = halfParts;
        this.drawingOrder = drawingOrder;
        this.sharedBody = sharedBody;
        this.halfSelections = halfSelections;
    }
    public override int Count => drawingOrder.Length;
    public override EnemySpritemapPart this[int index]
    {
        get
        {
            int selected = drawingOrder[index];
            EnemySpritemapPart part = HalfPart(Math.Abs(selected) - 1);
            return selected < 0 ? Reflect(part) : part;
        }
    }

    internal static EnemySpritemapParts Compile(int identity, EnemySpritemapPart[] supplied, ref BabyMetroidSpriteParts? bodyTemplate)
    {
        int offset = identity - FirstPose;
        if (offset < 0 || offset % PoseStride != 0 || offset / PoseStride >= 3 || supplied.Length != NativePartCount)
            return FromOwnedArray(supplied);
        var halves = new List<EnemySpritemapPart>(NativePartCount / 2);
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

    private EnemySpritemapPart HalfPart(int index)
    {
        if (halfSelections is null) return halfParts[index];
        int selection = halfSelections[index];
        if (selection > 0) return halfParts[selection - 1];
        int shared = -selection - 1;
        EnemySpritemapPart part = sharedBody!.halfParts[shared / 2];
        return (shared & 1) == 0 ? part : Reflect(part);
    }

    private static EnemySpritemapPart Reflect(EnemySpritemapPart part)
    {
        int size = part.X.IsLarge ? 16 : 8;
        int x = -part.X.SignedOffset - size;
        return part with
        {
            X = new SnesSpritemapXWord((ushort)((part.X.Raw & ~0x1ff) | (x & 0x1ff))),
            Attributes = new SnesObjAttributeWord((ushort)(part.Attributes.Raw ^ 0x4000)),
        };
    }
}
