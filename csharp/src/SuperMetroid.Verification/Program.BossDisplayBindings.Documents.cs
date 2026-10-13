using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using static EnemyContentIdentityFixtureVerification;

/// <summary>Provides editable boss OAM and BG2 documents for display-binding checks.</summary>
internal static class BossDisplayBindingsDocumentsVerification
{
    /// <summary>Distinct authored poses let assertions detect using the physical frame by mistake.</summary>
    internal sealed class BossDisplayDocuments
    {
        /// <summary>Serializes fixture documents through the same loader path as installed artwork.</summary>
        private readonly EnemyIdentityFixture json = new();
        /// <summary>Authored extended OAM frames, optionally redirected to a different pose.</summary>
        internal EnemyExtendedFrameDocument Oam { get; }
        /// <summary>Crocomire body BG2 writes used by the binding fixture.</summary>
        internal EnemyBg2FrameDocument Crocomire { get; }
        /// <summary>Phantoon BG2 writes used by the binding fixture.</summary>
        internal EnemyBg2FrameDocument Phantoon { get; }
        /// <summary>Draygon BG2 writes used by the binding fixture.</summary>
        internal EnemyBg2FrameDocument Draygon { get; }

        /// <summary>Creates distinct poses and tile writes so wrong physical-frame selection is observable.</summary>
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
                if (edited) (Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires Oam.DisplayFrames."))[frame.Name] = family[(index + 1) % family.Length].Name;
            }
            Crocomire = Bg2(CrocomireBg2FrameDefinitions.Frames, edited);
            Phantoon = Bg2(PhantoonBg2FrameDefinitions.Frames, edited);
            Draygon = Bg2(DraygonBg2FrameDefinitions.Frames, edited);
        }

        /// <summary>Loads fixture documents into an artwork catalog, optionally omitting one BG2 family.</summary>
        internal EnemyTileArtworkCatalog Build(bool extended = true, byte? omitBg2Bank = null) => EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), ExtendedFrames = extended ? EnemyExtendedFrameCatalog.Load(json.Json(Oam)) : null, CrocomireBg2Frames = omitBg2Bank == CrocomireBg2FrameDefinitions.Bank ? null : CrocomireBg2FrameCatalog.Load(json.Json(Crocomire)), PhantoonBg2Frames = omitBg2Bank == PhantoonBg2FrameDefinitions.Bank ? null : PhantoonBg2FrameCatalog.Load(json.Json(Phantoon)), DraygonBg2Frames = omitBg2Bank == DraygonBg2FrameDefinitions.Bank ? null : DraygonBg2FrameCatalog.Load(json.Json(Draygon)) });

        /// <summary>Resolves the redirected display pose for a native frame in the fixture.</summary>
        internal EnemyExtendedFrameDefinition Selected(EnemyExtendedFrameDefinition native) =>
            EnemyExtendedFrameDefinitions.Frames.ToArray().Single(frame => frame.Name == (Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires Oam.DisplayFrames."))[native.Name]);

        /// <summary>Locates authored BG2 writes by bank and native frame pointer.</summary>
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

        /// <summary>Groups Crocomire, Phantoon, and Draygon frames by their native enemy family.</summary>
        internal static IEnumerable<(EnemyDefinitionId Definition, EnemyExtendedFrameDefinition[] Frames)> Families()
        {
            EnemyExtendedFrameDefinition[] all = EnemyExtendedFrameDefinitions.Frames.ToArray();
            yield return (EnemyDefinitionId.Crocomire, all.Where(frame =>
                frame.Bank == CrocomireBodyVisualDefinitions.Bank && frame.Name.StartsWith("crocomire_body_oam_", StringComparison.Ordinal)).ToArray());
            yield return (EnemyDefinitionId.PhantoonBody, all.Where(frame =>
                frame.Bank == PhantoonBg2FrameDefinitions.Bank && PhantoonBg2FrameDefinitions.IsFrame(frame.Pointer)).ToArray());
            yield return (EnemyDefinitionId.DraygonBody, all.Where(frame => frame.Bank == DraygonBg2FrameDefinitions.Bank &&
                (frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal) || frame.Name.StartsWith("draygon_bg2_", StringComparison.Ordinal))).ToArray());
        }

        /// <summary>Gives each BG2 frame distinctive tile values and relocates one write when edited.</summary>
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
