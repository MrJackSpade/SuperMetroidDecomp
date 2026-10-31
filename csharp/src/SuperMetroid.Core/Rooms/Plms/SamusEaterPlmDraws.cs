using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.SamusEaterPlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>How each Samus Eater draw is described, found and named.</summary>
internal static class SamusEaterPlmDraws
{
    /// <summary>Decodes one of eight bank-$84 plant draw pointers into its orientation and chew phase.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        if (request is not PlmFrameDraw<SamusEaterDraw>)
        {
            draw = default;
            return false;
        }
        SamusEaterDraw list = PlmDraw.FrameOf<SamusEaterDraw>(request, "SamusEaterDraw");
        draw = list switch
        {
            SamusEaterDraw.FloorIdle => new(false, 0),
            SamusEaterDraw.FloorChew1 => new(false, 1),
            SamusEaterDraw.FloorChew2 => new(false, 2),
            SamusEaterDraw.FloorChew3 => new(false, 3),
            SamusEaterDraw.CeilingIdle => new(true, 0),
            SamusEaterDraw.CeilingChew1 => new(true, 1),
            SamusEaterDraw.CeilingChew2 => new(true, 2),
            SamusEaterDraw.CeilingChew3 => new(true, 3),
            _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {list}."),
        };
        return true;
    }

    /// <summary>Maps a validated native draw pointer to the artwork catalog's orientation and phase ID.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<SamusEaterDraw>(request, "Samus Eater draw with a visual ID"));

    /// <summary>Names a typed draw list by its floor or ceiling orientation and idle or chew phase.</summary>
    internal static string VisualId(SamusEaterDraw frame) => frame switch
    {
        SamusEaterDraw.FloorIdle => "floor-idle",
        SamusEaterDraw.FloorChew1 => "floor-chew-1",
        SamusEaterDraw.FloorChew2 => "floor-chew-2",
        SamusEaterDraw.FloorChew3 => "floor-chew-3",
        SamusEaterDraw.CeilingIdle => "ceiling-idle",
        SamusEaterDraw.CeilingChew1 => "ceiling-chew-1",
        SamusEaterDraw.CeilingChew2 => "ceiling-chew-2",
        SamusEaterDraw.CeilingChew3 => "ceiling-chew-3",
        _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {frame}."),
    };

    /// <summary>Finds the native layout matching an artwork catalog ID by searching the eight supported lists.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the three native signed runs for a supported plant draw pointer from its phase and orientation.</summary>
    internal static bool TryGet(PlmDraw request,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!TryDescribe(request, out Draw draw))
        {
            list = default;
            return false;
        }
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[3];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[Draw.Count(run)];
            for (int block = 0; block < blocks.Length; block++) blocks[block] = draw.BlockAt(run, block);
            runs[run] = new(PlmRunDirection.Horizontal, blocks, new(Draw.NextX(run), draw.NextY(run)));
        }
        list = new(request, runs);
        return true;
    }

    /// <summary>Materializes the eight native plant layouts as temporary DTOs for artwork import and export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (SamusEaterDraw frame in Enum.GetValues<SamusEaterDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Floor or ceiling orientation and one of four mouth animation phases for a native draw list.</summary>
    [IndependentFlag("whether the eater draw hangs from the ceiling, a fact of the decoded draw list")]
    internal readonly record struct Draw(bool Ceiling, [property: IntegerQuantity("animation frame index within the eater's draw cycle")] int Phase)
    {
        /// <summary>The mouth runs each contain two blocks; the foliage run contains four.</summary>
        internal static int Count(int run) => run switch
        {
            0 or 1 => 2,
            2 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(run)),
        };
        /// <summary>Moves left between mouth blocks while keeping the foliage run in its original column.</summary>
        internal static int NextX(int run) => run switch
        {
            0 or 1 => -2,
            2 => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(run)),
        };
        /// <summary>Offsets the second mouth run toward the plant body, reversing the step for ceiling plants.</summary>
        internal int NextY(int run) => run switch
        {
            0 or 2 => 0,
            1 => Ceiling ? 1 : -1,
            _ => throw new ArgumentOutOfRangeException(nameof(run)),
        };
        /// <summary>The phase block, collision kind, and orientation mirroring of a single mouth or foliage block.</summary>
        internal RoomTerrainBlock BlockAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new ArgumentOutOfRangeException(nameof(block));
            int tile;
            RoomCollisionType collision;
            BlockMirroring flips;
            if (run == 2)
            {
                // Four-wide foliage: two tiles followed by their mirrored pair.
                tile = FoliageFirstBlock + Phase * 2 + Math.Min(block, 3 - block);
                collision = RoomCollisionType.SpikeAir;
                flips = block >= 2 ? BlockMirroring.Horizontal : BlockMirroring.None;
            }
            else
            {
                // Right mouth half precedes the left half in native draw order.
                tile = MouthFirstBlock + Phase * 2 + (run == 0 ? 1 - block : block);
                flips = run == 0 ? BlockMirroring.Horizontal : BlockMirroring.None;
                collision = new SamusEaterBlock(run, block) switch
                {
                    (0, 1) or (1, 0) => RoomCollisionType.SolidBlock,
                    (0, 0) when Phase == 0 => RoomCollisionType.SpecialAir,
                    (1, 1) when Phase == 0 => RoomCollisionType.HorizontalExtension,
                    _ => RoomCollisionType.Air,
                };
            }
            return RoomTerrainBlock.Of(tile, flips | (Ceiling ? BlockMirroring.Vertical : BlockMirroring.None), collision);
        }
    }
}
