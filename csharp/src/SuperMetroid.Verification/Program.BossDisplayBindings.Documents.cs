using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Distinct authored poses let assertions detect using the physical frame by mistake.</summary>
    private sealed class BossDisplayDocuments
    {
        /// <summary>Fixture JSON helper that serializes generated OAM and BG2 documents for catalog loading.</summary>
        private readonly EnemyIdentityFixture json = new();

        /// <summary>Extended OAM document containing the fixture's distinct boss poses and optional display-frame remapping.</summary>
        internal EnemyExtendedFrameDocument Oam { get; }

        /// <summary>Generated BG2 write document for Crocomire's frame family.</summary>
        internal EnemyBg2FrameDocument Crocomire { get; }

        /// <summary>Generated BG2 write document for Phantoon's frame family.</summary>
        internal EnemyBg2FrameDocument Phantoon { get; }

        /// <summary>Generated BG2 write document for Draygon's frame family.</summary>
        internal EnemyBg2FrameDocument Draygon { get; }

        /// <summary>Creates distinct generated poses and BG2 writes, optionally changing them to identify selected edits.</summary>
        /// <param name="edited">Whether to vary fixture tiles, offsets, flips, and display-frame selections from their baseline values.</param>
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

        /// <summary>Loads the generated documents into a verification catalog, optionally excluding extended OAM or one BG2 family.</summary>
        /// <param name="extended">Whether the extended OAM document should be loaded.</param>
        /// <param name="omitBg2Bank">Bank whose BG2 catalog should be omitted, or null to load all three families.</param>
        /// <returns>A catalog backed only by the fixture's generated artwork documents.</returns>
        internal EnemyTileArtworkCatalog Build(bool extended = true, byte? omitBg2Bank = null) => EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
            extendedFrames: extended ? EnemyExtendedFrameCatalog.Load(json.Json(Oam)) : null,
            crocomireBg2Frames: omitBg2Bank == CrocomireBg2FrameDefinitions.Bank ? null : CrocomireBg2FrameCatalog.Load(json.Json(Crocomire)),
            phantoonBg2Frames: omitBg2Bank == PhantoonBg2FrameDefinitions.Bank ? null : PhantoonBg2FrameCatalog.Load(json.Json(Phantoon)),
            draygonBg2Frames: omitBg2Bank == DraygonBg2FrameDefinitions.Bank ? null : DraygonBg2FrameCatalog.Load(json.Json(Draygon)));

        /// <summary>Resolves a native OAM frame through the fixture's display-frame selection map.</summary>
        /// <param name="native">Native frame whose authored display selection is requested.</param>
        /// <returns>The extended-frame definition named by the OAM document's display mapping.</returns>
        internal EnemyExtendedFrameDefinition Selected(EnemyExtendedFrameDefinition native) =>
            EnemyExtendedFrameDefinitions.Frames.ToArray().Single(frame => frame.Name == Oam.DisplayFrames![native.Name]);

        /// <summary>Finds the fixture BG2 writes associated with a frame pointer in one of the three boss banks.</summary>
        /// <param name="bank">Bank identifying Crocomire, Phantoon, or Draygon BG2 definitions.</param>
        /// <param name="pointer">Native frame pointer whose fixture writes are requested.</param>
        /// <returns>The selected frame's write records, or an empty array when the pointer is not in that family.</returns>
        /// <exception cref="InvalidDataException">The bank is not one of the supported fixture families.</exception>
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

        /// <summary>Enumerates the boss identities and extended OAM frame groups used to generate the fixture document.</summary>
        /// <returns>Crocomire, Phantoon, and Draygon definition/frame pairs in fixture construction order.</returns>
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

        /// <summary>Builds a BG2 document with three deterministic tile writes for every frame in a boss family.</summary>
        /// <param name="definitions">Frame identities that determine document keys and per-frame tile values.</param>
        /// <param name="edited">Whether the generated writes use the alternate tile and Y-offset values.</param>
        /// <returns>A version-one document keyed by native frame names.</returns>
        private static EnemyBg2FrameDocument Bg2(EnemyBg2FrameDefinitionSequence definitions, bool edited) => new()
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
