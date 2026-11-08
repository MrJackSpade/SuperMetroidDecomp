using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.SamusEaterObjectDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How each Samus Eater draw is described.</summary>
internal static class SamusEaterObjectDraws
{
    /// <summary>Decodes one of eight plant draw pointers into its orientation and chew phase.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        if (request is not RoomObjectFrameDraw<SamusEaterDraw>)
        {
            draw = default;
            return false;
        }
        SamusEaterDraw list = RoomObjectDraw.FrameOf<SamusEaterDraw>(request, "SamusEaterDraw");
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

    /// <summary>Floor or ceiling orientation and one of four mouth animation phases for a draw list.</summary>
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
        /// <summary>The runs a plant draw writes: its two mouth runs, then its foliage.</summary>
        internal const int RunCount = 3;
        /// <summary>
        /// Where a run starts, in blocks from the plant's origin: the second mouth run two blocks left of the first, and
        /// the foliage two blocks left and one row toward the plant body: up for a floor plant, down for a ceiling one.
        /// </summary>
        internal Offset16<RoomBlocks> Start(int run) => run switch
        {
            0 => Offset16<RoomBlocks>.Zero,
            1 => new(-2, 0),
            2 => new(-2, Ceiling ? 1 : -1),
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
                // Right mouth half precedes the left half in draw order.
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
