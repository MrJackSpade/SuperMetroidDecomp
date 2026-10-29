using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
namespace SuperMetroid.Verification.Bosses;

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
            foreach ((_, EnemySpritemapDefinition[] family) in Families())
            for (int index = 0; index < family.Length; index++)
            {
                EnemySpritemapDefinition frame = family[index];
                Oam.Frames[frame.Name] = !edited && EnemyExtendedFrameBounds.IsBg2Only(frame) ? [] :
                [ new() { OffsetX = edited ? 6 : 0, OffsetY = edited ? -3 : 0,
                    Parts = [new() { OffsetX = 2, OffsetY = 3, Size = index % 2 == 0 ? 16 : 8,
                        TileColumn = index % 16, TileRow = index / 16, Palette = 1, Priority = 2,
                        FlipX = edited, FlipY = edited }] } ];
                if (edited) (Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires Oam.DisplayFrames."))[frame.Name] = family[(index + 1) % family.Length].Name;
            }
            Crocomire = Bg2(CrocomireBg2FrameDefinitions.Frames, CrocomireBg2FrameDefinitions.Version, edited);
            Phantoon = Bg2(PhantoonBg2FrameDefinitions.Frames, PhantoonBg2FrameDefinitions.Version, edited);
            Draygon = Bg2(DraygonBg2FrameDefinitions.Frames, DraygonBg2FrameDefinitions.Version, edited);
        }

        /// <summary>Loads fixture documents into an artwork catalog, optionally omitting one BG2 family.</summary>
        internal InstalledEnemyArtwork Build(bool extended = true, byte? omitBg2Bank = null)
        {
            // Bosses draw from sheets composed over the installed enemy artwork, so the fixture starts from it and
            // replaces only the extended frames it authors (#1446).
            EnemyTileSourceArtwork tiles = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadEnemyTiles().Enemies;
            tiles.ExtendedFrames = extended ? EnemyExtendedFrameDocuments.Load(json.Json(Oam)) : null;
            return new(tiles,
                new BossSourceArtwork { CrocomireBg2Frames = omitBg2Bank == CrocomireBg2FrameDefinitionsConstants.Bank ? null : CrocomireBg2FrameCatalog.Load(json.Json(Crocomire)), PhantoonBg2Frames = omitBg2Bank == PhantoonBg2FrameDefinitionsConstants.Bank ? null : PhantoonBg2FrameCatalog.Load(json.Json(Phantoon)), DraygonBg2Frames = omitBg2Bank == DraygonBg2FrameToolingDefinitions.Bank ? null : DraygonBg2FrameCatalog.Load(json.Json(Draygon)) },
                new AreaSourceArtwork());
        }

        /// <summary>Resolves the redirected display pose for a native frame in the fixture.</summary>
        internal EnemySpritemapDefinition Selected(EnemySpritemapDefinition native) =>
            EnemyExtendedFrameDefinitions.Frames.Single(frame => frame.Name == (Oam.DisplayFrames ?? throw new InvalidOperationException("Verification requires Oam.DisplayFrames."))[native.Name]);

        /// <summary>Locates authored BG2 writes by bank and native frame pointer.</summary>
        internal EnemyBg2WriteDocument[] Writes(byte bank, ushort pointer)
        {
            WritesCase family = bank switch
            {
                CrocomireBg2FrameDefinitionsConstants.Bank => new WritesCase(Crocomire, CrocomireBg2FrameDefinitions.Frames.ToArray()),
                PhantoonBg2FrameDefinitionsConstants.Bank => new WritesCase(Phantoon, PhantoonBg2FrameDefinitions.Frames.ToArray()),
                DraygonBg2FrameToolingDefinitions.Bank => new WritesCase(Draygon, DraygonBg2FrameDefinitions.Frames.ToArray()),
                _ => throw new InvalidDataException("Unknown fixture family."),
            };
            foreach (EnemySpritemapDefinition frame in family.Definitions)
                if (frame.Pointer == pointer) return family.Document.Frames[frame.Name];
            return [];
        }

        /// <summary>Groups Crocomire, Phantoon, and Draygon frames by their native enemy family.</summary>
        internal static IEnumerable<FamiliesResult> Families()
        {
            EnemySpritemapDefinition[] all = EnemyExtendedFrameDefinitions.Frames.ToArray();
            yield return new FamiliesResult(EnemyDefinitionId.Crocomire, all.Where(frame =>
                frame.Bank == CrocomireBodyVisualToolingDefinitions.Bank && frame.Name.StartsWith("crocomire_body_", StringComparison.Ordinal)).ToArray());
            yield return new FamiliesResult(EnemyDefinitionId.PhantoonBody, all.Where(frame =>
                frame.Bank == PhantoonBg2FrameDefinitionsConstants.Bank && PhantoonBg2FrameDefinitionsConstants.IsFrame(frame.Pointer)).ToArray());
            yield return new FamiliesResult(EnemyDefinitionId.DraygonBody, all.Where(frame => frame.Bank == DraygonBg2FrameToolingDefinitions.Bank &&
                frame.Name.StartsWith("draygon_", StringComparison.Ordinal)).ToArray());
        }

        /// <summary>Gives each BG2 frame distinctive tiles, authored by their tilemap entries, and relocates one write when edited.</summary>
        private static EnemyBg2FrameDocument Bg2(ImmutableArray<EnemySpritemapDefinition> definitions, int version, bool edited) => new()
        {
            Version = version,
            Frames = definitions.SelectToArray((frame, index) => KeyValuePair.Create(frame.Name,
                new EnemyBg2WriteDocument[]
                { new() { X = 3, Y = 2, Tiles = RoomBackgroundTilemapCellWords.Cells(100 + index, 200 + index) },
                  new() { X = 4, Y = 2, Tiles = RoomBackgroundTilemapCellWords.Cells(edited ? 500 + index : 300 + index, 400 + index) },
                  new() { X = 1, Y = edited ? 4 : 3, Tiles = RoomBackgroundTilemapCellWords.Cells(600 + index) } })).ToDictionary(),
        };
    }
}
