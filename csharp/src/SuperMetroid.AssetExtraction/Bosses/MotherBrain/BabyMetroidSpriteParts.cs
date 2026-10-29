using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Exact bilateral composition of the three native Baby poses; independent half-artwork and drawing order remain required.</summary>
internal sealed class BabyMetroidSpriteParts : EnemySpritemapParts
{
    /// <summary>Unreflected halves owned by this pose; shared dome halves are excluded from later poses.</summary>
    internal readonly SpritePart[] halfParts;
    /// <summary>Original OAM part order, with a negative selection requesting horizontal reflection.</summary>
    private readonly sbyte[] drawingOrder;
    /// <summary>First pose's reusable dome artwork, consulted only by later poses.</summary>
    internal readonly BabyMetroidSpriteParts? sharedBody;
    /// <summary>Maps later-pose halves to local parts or reflected/unreflected shared dome parts.</summary>
    private readonly sbyte[]? halfSelections;

    /// <summary>Captures one pose's half artwork and its original bilateral drawing order.</summary>
    private BabyMetroidSpriteParts(SpritePart[] halfParts, sbyte[] drawingOrder,
        BabyMetroidSpriteParts? sharedBody = null, sbyte[]? halfSelections = null)
    {
        this.halfParts = halfParts;
        this.drawingOrder = drawingOrder;
        this.sharedBody = sharedBody;
        this.halfSelections = halfSelections;
    }
    /// <summary>Includes both sides of every paired part in native OAM order.</summary>
    public override int Count => drawingOrder.Length;
    /// <summary>Expands a selected half into its original or horizontally reflected OAM part.</summary>
    public override SpritePart this[int index]
    {
        get
        {
            int selected = drawingOrder[Ensure.BetweenInclusive(0, drawingOrder.Length - 1, index)];
            SpritePart part = HalfPart(Math.Abs(selected) - 1);
            return selected < 0 ? Reflect(part) : part;
        }
    }

    /// <summary>Pairs reflected parts for the three native poses and shares identical negative-Y dome parts with the first pose; otherwise retains the supplied composition.</summary>
    internal static EnemySpritemapParts Compile(EnemyFrameId frame, SpritePart[] supplied, ref BabyMetroidSpriteParts? bodyTemplate)
    {
        if (!BabyMetroidCompositionDefinitions.Poses.Contains(frame) || supplied.Length != BabyMetroidCompositionDefinitions.PartCount)
            return FromOwnedArray(supplied);
        var halves = new List<SpritePart>(BabyMetroidCompositionDefinitions.PartCount / 2);
        var order = new sbyte[supplied.Length];
        for (int index = 0; index < supplied.Length; index++)
        {
            if (order[index] != 0) continue;
            SpritePart reflected = Reflect(supplied[index]);
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
        var local = new List<SpritePart>();
        var selections = new sbyte[halves.Count];
        for (int index = 0; index < halves.Count; index++)
        {
            SpritePart part = halves[index];
            // The native negative-Y dome has nine identical half-parts across all
            // three poses. Match complete supplied records, including orientation;
            // edited parts remain local and never mutate another pose's component.
            if (part.Offset.Y < 0)
            {
                for (int candidate = 0; candidate < bodyTemplate.halfParts.Length; candidate++)
                {
                    SpritePart shared = bodyTemplate.halfParts[candidate];
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

    /// <summary>Resolves a pose half from local storage or the first pose's shared dome, including reflected shared selections.</summary>
    private SpritePart HalfPart(int index)
    {
        if (halfSelections is null) return halfParts[index];
        int selection = halfSelections[index];
        if (selection > 0) return halfParts[selection - 1];
        int shared = -selection - 1;
        BabyMetroidSpriteParts sharedParts = sharedBody ?? throw new InvalidDataException(
            "A shared Baby Metroid half selection requires the shared body parts.");
        SpritePart part = sharedParts.halfParts[shared / 2];
        return (shared & 1) == 0 ? part : Reflect(part);
    }

    /// <summary>Mirrors an 8- or 16-pixel piece across the actor's vertical axis, wrapping across the sprite plane, and toggles its horizontal mirroring.</summary>
    private static SpritePart Reflect(SpritePart part)
    {
        int size = part.Large ? 16 : 8;
        int x = SpritePart.WrapX(-part.Offset.X - size);
        return part.At(Offset16<ScreenPixels>.Of(x, part.Offset.Y)) with { Flip = part.Flip ^ TileFlip.Horizontal };
    }
}
