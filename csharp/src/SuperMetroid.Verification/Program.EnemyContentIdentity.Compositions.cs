using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private sealed partial class EnemyIdentityFixture
    {
        private SpriteVisualPart[] Parts(string family, bool mutate)
        {
            if (mutate)
                foreach (string component in new[] { "x", "y", "size", "tile-column", "tile-row", "palette", "priority", "flip-x", "flip-y", "order", "count" })
                    Register(family + "-" + component);
            var first = new SpriteVisualPart
            {
                OffsetX = 0, OffsetY = 0, Size = 8, TileColumn = 0, TileRow = 0,
                Palette = 1, Priority = 1, FlipX = false, FlipY = false,
            };
            var second = first with { OffsetX = 1, TileColumn = 1, Palette = 2 };
            string? change = mutate && edit?.StartsWith(family + "-", StringComparison.Ordinal) == true
                ? edit[(family.Length + 1)..] : null;
            second = change switch
            {
                "x" => second with { OffsetX = 2 }, "y" => second with { OffsetY = 1 },
                "size" => second with { Size = 16 }, "tile-column" => second with { TileColumn = 2 },
                "tile-row" => second with { TileRow = 1 }, "palette" => second with { Palette = 3 },
                "priority" => second with { Priority = 2 }, "flip-x" => second with { FlipX = true },
                "flip-y" => second with { FlipY = true }, _ => second,
            };
            return change switch { "order" => [second, first], "count" => [first], _ => [first, second] };
        }

        public EnemySpritemapDocument OamDocument()
        {
            var definitions = EnemySpritemapDefinitions.Frames.ToArray();
            Register("oam-binding");
            return new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.Version,
                Frames = Ordered(definitions.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, Parts("oam", index == definitions.Length - 1)))),
                DisplayFrames = Ordered(definitions.Select((frame, index) => KeyValuePair.Create(frame.Name,
                    index == 0 && edit == "oam-binding"
                        ? definitions.First(other => other.Bank == frame.Bank && other.Name != frame.Name).Name : frame.Name))),
            };
        }
        public EnemySpritemapCatalog Oam() => EnemySpritemapCatalog.Load(Json(OamDocument()));

        public EnemyExtendedFrameDocument ExtendedDocument()
        {
            var definitions = EnemyExtendedFrameDefinitions.Frames.ToArray();
            foreach (string component in new[] { "x", "y", "order", "count", "binding" }) Register("extended-" + component);
            var frames = definitions.Select((frame, index) =>
            {
                bool last = index == definitions.Length - 1;
                var first = new EnemyExtendedVisualComponent { OffsetX = 0, OffsetY = 0, Parts = Parts("extended-parts", false) };
                var second = new EnemyExtendedVisualComponent
                {
                    OffsetX = last && edit == "extended-x" ? 2 : 1,
                    OffsetY = last && edit == "extended-y" ? 1 : 0,
                    Parts = Parts("extended-parts", last),
                };
                EnemyExtendedVisualComponent[] components = last && edit == "extended-order" ? [second, first]
                    : last && edit == "extended-count" ? [first] : [first, second];
                return KeyValuePair.Create(frame.Name, components);
            });
            return new EnemyExtendedFrameDocument
            {
                Version = EnemyExtendedFrameDefinitions.Version, Frames = Ordered(frames),
                DisplayFrames = Ordered(definitions.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, index == 0 && edit == "extended-binding" ? definitions[1].Name : frame.Name))),
            };
        }
        public EnemyExtendedFrameCatalog Extended() => EnemyExtendedFrameCatalog.Load(Json(ExtendedDocument()));

        public EnemyProjectileSpritemapDocument ProjectileDocument()
        {
            var frames = EnemyProjectileSpritemapDefinitions.Frames.ToArray();
            var programs = EnemyProjectilePresentationFrameDefinitions.All.ToArray();
            return new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.Version,
                Frames = Ordered(frames.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, Parts("projectile", index == frames.Length - 1)))),
                ProgramFrames = Ordered(programs.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, Parts("projectile-program", index == programs.Length - 1)))),
            };
        }
        public EnemyProjectileSpritemapCatalog Projectiles() => EnemyProjectileSpritemapCatalog.Load(Json(ProjectileDocument()));

        private MemoryStream Bg2(string family, ReadOnlySpan<EnemyBg2FrameDefinition> definitions)
        {
            foreach (string component in new[] { "tiles", "x", "y", "order", "count", "run-boundary" }) Register(family + "-" + component);
            var frames = definitions.ToArray().Select((frame, index) =>
            {
                bool first = index == 0;
                var a = new EnemyBg2WriteDocument { X = 0, Y = 0, Tiles = [1, 2] };
                var b = new EnemyBg2WriteDocument
                {
                    X = first && edit == family + "-x" ? 2 : 1,
                    Y = first && edit == family + "-y" ? 2 : 1,
                    Tiles = first && edit == family + "-tiles" ? [3, 5] : [3, 4],
                };
                EnemyBg2WriteDocument[] writes = first && edit == family + "-order" ? [b, a]
                    : first && edit == family + "-count" ? [a]
                    // Same ordered tile payload, different command boundaries and destinations.
                    : first && edit == family + "-run-boundary" ? [a with { Tiles = [1] }, a with { X = 1, Tiles = [2] }, b]
                    : [a, b];
                return KeyValuePair.Create(frame.Name, writes);
            });
            return Json(new EnemyBg2FrameDocument { Version = 1, Frames = Ordered(frames) });
        }
    }
}
