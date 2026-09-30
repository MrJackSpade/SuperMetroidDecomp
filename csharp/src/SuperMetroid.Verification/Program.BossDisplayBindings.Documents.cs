using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Distinct authored poses let assertions detect using the physical frame by mistake.</summary>
    private sealed class BossDisplayDocuments
    {
        private readonly EnemyIdentityFixture json = new();
        internal EnemyExtendedFrameDocument Oam { get; }
        internal EnemyBg2FrameDocument Crocomire { get; }
        internal EnemyBg2FrameDocument Phantoon { get; }
        internal EnemyBg2FrameDocument Draygon { get; }

        internal BossDisplayDocuments(bool edited)
        {
            Oam = json.ExtendedDocument();
            foreach ((_, EnemyExtendedFrameDefinition[] family) in Families())
            for (int index = 0; index < family.Length; index++)
            {
                EnemyExtendedFrameDefinition frame = family[index];
                Oam.Frames[frame.Name] = !edited && EnemyExtendedFrameDefinitions.IsBg2Only(frame) ? [] :
                [ new() { OffsetX = edited ? 6 : 0, OffsetY = edited ? -3 : 0,
                    Parts = [new() { OffsetX = 2, OffsetY = 3, Size = index % 2 == 0 ? 16 : 8,
                        TileColumn = index % 16, TileRow = index / 16, Palette = 1, Priority = 2,
                        FlipX = edited, FlipY = edited }] } ];
                if (edited) Oam.DisplayFrames![frame.Name] = family[(index + 1) % family.Length].Name;
            }
            Crocomire = Bg2(CrocomireBg2FrameDefinitions.Frames, edited);
            Phantoon = Bg2(PhantoonBg2FrameDefinitions.Frames, edited);
            Draygon = Bg2(DraygonBg2FrameDefinitions.Frames, edited);
        }

        internal EnemyTileArtworkCatalog Build(bool extended = true, byte? omitBg2Bank = null) => new(
            new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
            extendedFrames: extended ? EnemyExtendedFrameCatalog.Load(json.Json(Oam)) : null,
            crocomireBg2Frames: omitBg2Bank == CrocomireBg2FrameDefinitions.Bank ? null : CrocomireBg2FrameCatalog.Load(json.Json(Crocomire)),
            phantoonBg2Frames: omitBg2Bank == PhantoonBg2FrameDefinitions.Bank ? null : PhantoonBg2FrameCatalog.Load(json.Json(Phantoon)),
            draygonBg2Frames: omitBg2Bank == DraygonBg2FrameDefinitions.Bank ? null : DraygonBg2FrameCatalog.Load(json.Json(Draygon)));

        internal EnemyExtendedFrameDefinition Selected(EnemyExtendedFrameDefinition native) =>
            EnemyExtendedFrameDefinitions.Frames.ToArray().Single(frame => frame.Name == Oam.DisplayFrames![native.Name]);

        internal EnemyBg2WriteDocument[] Writes(byte bank, ushort pointer)
        {
            (EnemyBg2FrameDocument Document, EnemyBg2FrameDefinition[] Definitions) family = bank switch
            {
                CrocomireBg2FrameDefinitions.Bank => (Crocomire, CrocomireBg2FrameDefinitions.Frames.ToArray()),
                PhantoonBg2FrameDefinitions.Bank => (Phantoon, PhantoonBg2FrameDefinitions.Frames.ToArray()),
                DraygonBg2FrameDefinitions.Bank => (Draygon, DraygonBg2FrameDefinitions.Frames.ToArray()),
                _ => throw new InvalidDataException("Unknown fixture family."),
            };
            foreach (EnemyBg2FrameDefinition frame in family.Definitions)
                if (frame.Pointer == pointer) return family.Document.Frames[frame.Name];
            return [];
        }

        internal static IEnumerable<(ushort Definition, EnemyExtendedFrameDefinition[] Frames)> Families()
        {
            EnemyExtendedFrameDefinition[] all = EnemyExtendedFrameDefinitions.Frames.ToArray();
            yield return (RoomEnemySystem.CrocomireDefinition, all.Where(frame =>
                frame.Bank == CrocomireBodyVisualDefinitions.Bank && frame.Name.StartsWith("crocomire_body_oam_", StringComparison.Ordinal)).ToArray());
            yield return (RoomEnemySystem.PhantoonBodyDefinition, all.Where(frame =>
                frame.Bank == PhantoonBg2FrameDefinitions.Bank && PhantoonBg2FrameDefinitions.IsFrame(frame.Pointer)).ToArray());
            yield return (DraygonEnemyDefinitionPointers.Body, all.Where(frame => frame.Bank == DraygonBg2FrameDefinitions.Bank &&
                (frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal) || frame.Name.StartsWith("draygon_bg2_", StringComparison.Ordinal))).ToArray());
        }

        private static EnemyBg2FrameDocument Bg2(ReadOnlySpan<EnemyBg2FrameDefinition> definitions, bool edited) => new()
        {
            Version = 1,
            Frames = definitions.ToArray().Select((frame, index) => KeyValuePair.Create(frame.Name,
                new EnemyBg2WriteDocument[]
                { new() { X = 3, Y = 2, Tiles = [100 + index, 200 + index] },
                  new() { X = 4, Y = 2, Tiles = [edited ? 500 + index : 300 + index, 400 + index] },
                  new() { X = 1, Y = edited ? 4 : 3, Tiles = [600 + index] } })).ToDictionary(),
        };
    }
}
