using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Holds the spawned-object sheets' common art once (#1513, owner flags 20 and 28). Every graphics set's spawned-object
/// sheets picture the explosions, drops, projectiles and sprite objects its spawners show, so the common ones were pictured
/// again on every set's sheet. Each laid-out sheet is painted and cut into its pictures; a picture is known by its pixels and
/// by its sheet's colouring over the colour groups it uses (<see cref="SheetPictureColouring"/>), as the rows a picture
/// never shows say nothing about it. A picture two or more sheets hold is common: it is held once, on a common sheet, under
/// every frame name those sheets show it as. Common pictures whose colourings agree on every group they share are held on
/// one common sheet, so there is one common sheet for each colouring the common art is drawn in. A sheet keeps only the
/// pictures no other sheet holds, and has no sheet of its own when it holds none.
/// </summary>
internal static class SpawnedObjectSheetSharing
{
    /// <summary>The installed sheets <paramref name="layouts"/>, each a distinct laid-out spawned-object sheet, share out to.</summary>
    /// <exception cref="InvalidDataException">A sheet's pictures cannot be cut out, or a sheet does not fit on one image.</exception>
    internal static SharedSpawnedSheets Share(List<SpriteSheetLayout> layouts)
    {
        Ensure.NotNull(layouts);
        var painted = new PaintedSheet[layouts.Count];
        Parallel.For(0, layouts.Count, () => new SpriteSheetCanvas(), (index, _, canvas) =>
        {
            painted[index] = PaintedSheet.Paint(layouts[index], canvas);
            return canvas;
        }, _ => { });
        // Each distinct picture, in the order the sheets first hold it, and each sheet's pictures by frame name.
        var pictures = new List<HeldPicture>();
        var pictureOf = new Dictionary<string, HeldPicture>(StringComparer.Ordinal);
        var held = new Dictionary<string, HeldPicture>[painted.Length];
        for (int sheet = 0; sheet < painted.Length; sheet++)
        {
            held[sheet] = new Dictionary<string, HeldPicture>(StringComparer.Ordinal);
            foreach ((string name, SheetPicture picture) in painted[sheet].Frames)
            {
                SheetPictureColouring colouring = SheetPictureColouring.Of(painted[sheet].Document, picture.Rows);
                string identity = $"{picture.Content}/{colouring.Key}";
                if (!pictureOf.TryGetValue(identity, out HeldPicture? shown))
                {
                    pictureOf.Add(identity, shown = new HeldPicture(picture, colouring));
                    pictures.Add(shown);
                }
                shown.HeldBy(sheet, name, colouring);
                held[sheet].Add(name, shown);
            }
        }
        // Each common picture joins the first common sheet whose colouring agrees with its own, or starts the next.
        var common = new List<PictureSheetPlan>();
        foreach (HeldPicture picture in pictures.Where(picture => picture.IsCommon))
        {
            int sheet = common.FindIndex(plan => plan.TryTake(picture));
            if (sheet < 0)
            {
                sheet = common.Count;
                common.Add(new PictureSheetPlan(picture, $"Spawned-Common {sheet}"));
            }
            picture.CommonSheet = sheet;
        }
        // Each sheet keeps the pictures no other sheet holds, in its own colouring.
        var own = new PictureSheetPlan?[painted.Length];
        for (int sheet = 0; sheet < painted.Length; sheet++)
        {
            foreach (HeldPicture picture in held[sheet].Values.Distinct().Where(picture => !picture.IsCommon))
            {
                if (own[sheet] is { } plan)
                    plan.Take(picture);
                else
                    own[sheet] = new PictureSheetPlan(picture, $"spawned-object sheet {sheet}");
            }
        }
        PictureSheetPlan[] plans = [.. common, .. own.OfType<PictureSheetPlan>()];
        var encoded = new EncodedSpriteSheet[plans.Length];
        Parallel.For(0, plans.Length, () => new SpriteSheetEncoder(), (index, _, encoder) =>
        {
            encoded[index] = encoder.Encode(plans[index].Layout());
            return encoder;
        }, encoder => encoder.Dispose());
        var encodedOf = new Dictionary<PictureSheetPlan, EncodedSpriteSheet>(ReferenceEqualityComparer.Instance);
        for (int index = 0; index < plans.Length; index++)
            encodedOf.Add(plans[index], encoded[index]);
        var shares = new SpawnedSheetShare[painted.Length];
        for (int sheet = 0; sheet < painted.Length; sheet++)
        {
            shares[sheet] = new SpawnedSheetShare(own[sheet] is { } plan ? encodedOf[plan] : null,
                held[sheet].Where(frame => frame.Value.IsCommon)
                    .ToDictionary(frame => frame.Key, frame => frame.Value.CommonSheet, StringComparer.Ordinal));
        }
        return new SharedSpawnedSheets([.. common.Select(plan => encodedOf[plan])], shares);
    }

    /// <summary>One distinct picture the spawned-object sheets hold, and which of them hold it under which frame names.</summary>
    private sealed class HeldPicture
    {
        /// <summary>The sheets holding the picture, by their place among the layouts.</summary>
        private readonly HashSet<int> holders = [];

        /// <summary>The frame names the sheets show the picture as, each once.</summary>
        private readonly HashSet<string> nameSet = new(StringComparer.Ordinal);

        /// <summary>Starts a picture first held in <paramref name="colouring"/>.</summary>
        internal HeldPicture(SheetPicture picture, SheetPictureColouring colouring)
        {
            Picture = picture;
            Colouring = colouring;
        }

        /// <summary>The picture.</summary>
        internal SheetPicture Picture { get; }

        /// <summary>
        /// Its colouring over the groups it uses, with every shade any sheet holding it names, so every shade it can be drawn
        /// in stays installed beside it.
        /// </summary>
        internal SheetPictureColouring Colouring { get; private set; }

        /// <summary>The frame names the sheets show the picture as, in the order they first do.</summary>
        internal List<string> Names { get; } = [];

        /// <summary>Whether two or more sheets hold the picture, so it is held once on a common sheet.</summary>
        internal bool IsCommon => holders.Count > 1;

        /// <summary>The common sheet the picture is held on, by its place among the common sheets.</summary>
        internal int CommonSheet { get; set; } = -1;

        /// <summary>Records that sheet <paramref name="sheet"/>, coloured as <paramref name="colouring"/>, shows the picture as frame <paramref name="name"/>.</summary>
        /// <exception cref="InvalidOperationException">The colouring disagrees with the picture's, which its identity rules out.</exception>
        internal void HeldBy(int sheet, string name, SheetPictureColouring colouring)
        {
            holders.Add(sheet);
            if (nameSet.Add(name))
                Names.Add(name);
            Colouring = Colouring.MergedWith(colouring)
                ?? throw new InvalidOperationException($"Spawned-object frame {name} is coloured two ways under one colouring key.");
        }
    }

    /// <summary>The pictures one installed sheet will hold, in order, and the colouring they agree on.</summary>
    private sealed class PictureSheetPlan
    {
        /// <summary>The pictures, in order.</summary>
        private readonly List<HeldPicture> pictures = [];

        /// <summary>Every frame name the pictures are shown as.</summary>
        private readonly HashSet<string> names = new(StringComparer.Ordinal);

        /// <summary>Names the sheet in failures.</summary>
        private readonly string sheet;

        /// <summary>The colouring every picture taken so far agrees on.</summary>
        private SheetPictureColouring colouring;

        /// <summary>Starts a sheet with <paramref name="first"/>.</summary>
        internal PictureSheetPlan(HeldPicture first, string sheet)
        {
            this.sheet = sheet;
            colouring = first.Colouring;
            Add(first);
        }

        /// <summary>
        /// Takes <paramref name="picture"/> when its colouring agrees with the sheet's on every group both cover and none of
        /// its frame names already shows another picture of the sheet.
        /// </summary>
        internal bool TryTake(HeldPicture picture)
        {
            if (picture.Names.Any(names.Contains) || colouring.MergedWith(picture.Colouring) is not { } merged)
                return false;
            colouring = merged;
            Add(picture);
            return true;
        }

        /// <summary>Takes <paramref name="picture"/>, which one sheet's colouring painted like the sheet's others.</summary>
        /// <exception cref="InvalidOperationException">The picture's colouring or frame names clash with the sheet's.</exception>
        internal void Take(HeldPicture picture)
        {
            if (!TryTake(picture))
                throw new InvalidOperationException($"Frame {picture.Names[0]} of {sheet} clashes with the sheet's other pictures.");
        }

        /// <summary>The sheet's layout.</summary>
        internal PictureSheetLayout Layout() =>
            new([.. pictures.Select(picture => new NamedSheetPicture(picture.Picture, picture.Names))], colouring, sheet);

        /// <summary>Adds a picture and its frame names.</summary>
        private void Add(HeldPicture picture)
        {
            pictures.Add(picture);
            names.UnionWith(picture.Names);
        }
    }
}
