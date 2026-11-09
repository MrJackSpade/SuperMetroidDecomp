using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Physical draw layouts selected by Mother Brain's glass PLM: how each frame's cells collide. The glass's draws
/// hold the frames' signed run offsets from the PLM origin, its typed object owns damage thresholds, shards and timing,
/// and what each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainGlassObjectDrawDefinitions
{
    /// <summary>
    /// How the cells of the eleven glass states collide, one row per draw run: the shootable glass block at the origin,
    /// its extensions down the pane, the solid shifted pane, and the air the shatter leaves.
    /// </summary>
    internal static readonly Dictionary<MotherBrainGlassDraw, RoomCollisionType[][]> Collisions = new()
    {
        [MotherBrainGlassDraw.Initial] =
        [
            [RoomCollisionType.ShootableBlock],
        ],
        [MotherBrainGlassDraw.PaneDamage1] =
        [
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
        ],
        [MotherBrainGlassDraw.PaneDamage2] =
        [
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
        ],
        [MotherBrainGlassDraw.PaneTransition] =
        [
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane1] =
        [
            [RoomCollisionType.ShootableBlock],
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane2] =
        [
            [RoomCollisionType.ShootableBlock],
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        ],
        [MotherBrainGlassDraw.ShiftedPane3] =
        [
            [RoomCollisionType.ShootableBlock],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
        ],
        [MotherBrainGlassDraw.Shatter1] =
        [
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.HorizontalExtension,
            ],
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
        ],
        [MotherBrainGlassDraw.Shatter2] =
        [
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.HorizontalExtension,
            ],
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
        ],
        [MotherBrainGlassDraw.Shatter3] =
        [
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
        ],
        [MotherBrainGlassDraw.Cleared] =
        [
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
        ],
    };
}
