using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the room objects' visual catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class RoomObjectContentIdentity
{
    /// <summary>
    /// Kraid's ten draw roles: the three ceiling-crumble stages, the three ceiling background draws of Kraid's room
    /// sequence, the two floor-spike appearances, and the two room-clearing layouts that replace the ceiling blocks and
    /// the floor spikes after Kraid is defeated.
    /// </summary>
    private static readonly KraidRoomDraw[] KraidDraws =
    [
        KraidRoomDraw.CrumbleFirst, KraidRoomDraw.CrumbleSecond, KraidRoomDraw.CrumbleThird,
        KraidRoomDraw.CeilingBackground1, KraidRoomDraw.CeilingBackground2, KraidRoomDraw.CeilingBackground3,
        KraidRoomDraw.SpikeFirst, KraidRoomDraw.SpikeSecond, KraidRoomDraw.ClearCeiling, KraidRoomDraw.ClearSpikes,
    ];

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectBlueDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectBlueDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectBlueDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomObjectBlueDoorVisualFiles.Editable.OrderBy(item => RoomObjectBlueDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int row = 0; row < appearances.Length; row++) appearances[row] = visuals.GetAppearance(frame.Draw, row);
                content.AppendName("frame", RoomObjectBlueDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectBotwoonWallVisuals"/>.</summary>
    extension(InstalledRoomObjectBotwoonWallVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectBotwoonWallVisuals),
            content =>
            {
                Span<RoomBlockAppearance> selected = stackalloc RoomBlockAppearance[BotwoonWallObjectDrawDefinitions.BlockCount];
                for (int index = 0; index < selected.Length; index++)
                    selected[index] = visuals.GetAppearance(BotwoonWallObjectDrawDefinitions.Clear, 0, index);
                content.AppendAppearances("blocks", selected);
            });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectCollectibleVisuals"/>.</summary>
    extension(InstalledRoomObjectCollectibleVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectCollectibleVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[1];
            foreach (var frame in RoomObjectCollectibleVisualFiles.Editable.OrderBy(frame => frame.Id, StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.Id);
                content.Append("runs", 1);
                buffer[0] = visuals.GetAppearance(RoomObjectDraw.Of(frame.Draw));
                content.AppendAppearances("blocks", buffer);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectColoredDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectColoredDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectColoredDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomObjectColoredDoorVisualFiles.All.OrderBy(item => RoomObjectColoredDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int block = 0; block < appearances.Length; block++) appearances[block] = visuals.GetAppearance(frame.Draw, block);
                content.AppendName("frame", RoomObjectColoredDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectDownwardGateVisuals"/>.</summary>
    extension(InstalledRoomObjectDownwardGateVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectDownwardGateVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[5];
            foreach (var frame in RoomObjectDownwardGateVisualFiles.All.OrderBy(item => RoomObjectDownwardGateVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                DownwardGateObjectDraws.TryDescribe(frame.Draw, out var draw);
                content.AppendName("frame", RoomObjectDownwardGateVisualFiles.VisualId(frame.Draw));
                content.Append("runs", draw.RunCount);
                for (int run = 0; run < draw.RunCount; run++)
                {
                    int count = draw.BlockCount(run);
                    for (int cell = 0; cell < count; cell++) buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell);
                    content.AppendAppearances("blocks", buffer[..count]);
                }
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectEyeDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectEyeDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectEyeDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomObjectEyeDoorVisualFiles.Editable.OrderBy(item => RoomObjectEyeDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                EyeDoorDraws.TryDescribe(frame.Draw, out var shape);
                content.AppendName("frame", RoomObjectEyeDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                for (int cell = 0; cell < shape.BlockCount; cell++) buffer[cell] = visuals.GetAppearance(frame.Draw, cell);
                content.AppendAppearances("blocks", buffer[..shape.BlockCount]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectGrappleBlockVisuals"/>.</summary>
    extension(InstalledRoomObjectGrappleBlockVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectGrappleBlockVisuals), content =>
        {
            Span<RoomBlockAppearance> block = stackalloc RoomBlockAppearance[1];
            foreach (var draw in RoomObjectGrappleBlockVisualFiles.All.OrderBy(draw => RoomObjectGrappleBlockVisualFiles.VisualId(draw.Frame), StringComparer.Ordinal))
            {
                content.AppendName("frame", RoomObjectGrappleBlockVisualFiles.VisualId(draw.Frame));
                content.Append("runs", 1);
                block[0] = visuals.GetAppearance(RoomObjectDraw.Of(draw.Frame));
                content.AppendAppearances("blocks", block);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectGreyDoorVisuals"/>.</summary>
    extension(InstalledRoomObjectGreyDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectGreyDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomObjectGreyDoorVisualFiles.All.OrderBy(item => RoomObjectGreyDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int row = 0; row < appearances.Length; row++) appearances[row] = visuals.GetAppearance(frame.Draw, row);
                content.AppendName("frame", RoomObjectGreyDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectNoobTubeVisuals"/>.</summary>
    extension(InstalledRoomObjectNoobTubeVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectNoobTubeVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[48];
            foreach (var frame in RoomObjectNoobTubeVisualFiles.All.OrderBy(item => RoomObjectNoobTubeVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                NoobTubeObjectDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int cell = 0; cell < shape.BlockCount(run); cell++)
                        appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell);
                content.AppendName("frame", RoomObjectNoobTubeVisualFiles.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed tube artwork.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances[..count]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectSamusEaterVisuals"/>.</summary>
    extension(InstalledRoomObjectSamusEaterVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectSamusEaterVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[8];
            foreach (var draw in RoomObjectSamusEaterVisualFiles.All.OrderBy(item => RoomObjectSamusEaterVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                int index = 0;
                for (int run = 0; run < 3; run++)
                    for (int block = 0; block < SamusEaterObjectDraws.Draw.Count(run); block++)
                        appearances[index++] = visuals.GetAppearance(draw.Draw, run, block);
                content.AppendName("frame", RoomObjectSamusEaterVisualFiles.VisualId(draw.Draw));
                // Preserve the original flattened-frame hash, not the three physical runs.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectShotBlockVisuals"/>.</summary>
    extension(InstalledRoomObjectShotBlockVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectShotBlockVisuals), content =>
        {
            Span<RoomBlockAppearance> row = stackalloc RoomBlockAppearance[2];
            foreach (var draw in RoomObjectShotBlockVisualFiles.Calculated.OrderBy(RoomObjectShotBlockVisualFiles.VisualId, StringComparer.Ordinal))
            {
                content.AppendName("frame", RoomObjectShotBlockVisualFiles.VisualId(draw));
                content.Append("runs", draw.RunCount);
                for (int run = 0; run < draw.RunCount; run++)
                {
                    for (int block = 0; block < draw.BlocksPerRun; block++)
                        row[block] = visuals.GetAppearance(RoomObjectDraw.Of(draw), run, block);
                    content.AppendAppearances("blocks", row[..draw.BlocksPerRun]);
                }
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectStationVisuals"/>.</summary>
    extension(InstalledRoomObjectStationVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectStationVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[2];
            foreach (var frame in RoomObjectStationVisualFiles.All.OrderBy(item => RoomObjectStationVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                var shape = RoomObjectStationVisualFiles.Describe(frame.Draw);
                content.AppendName("frame", RoomObjectStationVisualFiles.VisualId(frame.Draw));
                content.Append("runs", shape.RunCount);
                for (int run = 0; run < shape.RunCount; run++)
                {
                    int count = shape.BlockCount(run);
                    for (int cell = 0; cell < count; cell++) buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell);
                    content.AppendAppearances("blocks", buffer[..count]);
                }
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectKraidVisuals"/>.</summary>
    extension(InstalledRoomObjectKraidVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectKraidVisuals), content =>
        {
            foreach (KraidRoomDraw draw in KraidDraws.OrderBy(draw => RoomObjectKraidVisualFiles.VisualId(RoomObjectDraw.Of(draw)), StringComparer.Ordinal))
                Append(draw, visuals.frames[draw]);

            void Append(KraidRoomDraw draw, RoomBlockAppearance[] appearances)
            {
                content.AppendName("frame", RoomObjectKraidVisualFiles.VisualId(RoomObjectDraw.Of(draw)));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectLinkedRestoreVisuals"/>.</summary>
    extension(InstalledRoomObjectLinkedRestoreVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectLinkedRestoreVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var draw in RoomObjectLinkedRestoreVisualFiles.All.OrderBy(item => RoomObjectLinkedRestoreVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                InstalledRoomObjectLinkedRestoreVisuals.Describe(draw.Draw, out int runs);
                for (int run = 0; run < runs; run++)
                    for (int block = 0; block < 2; block++)
                        appearances[run * 2 + block] = visuals.GetAppearance(draw.Draw, run, block);
                content.AppendName("frame", RoomObjectLinkedRestoreVisualFiles.VisualId(draw.Draw));
                // The original catalog hashes each flattened frame as one run.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances[..(runs * 2)]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectMaridiaElevatubeVisuals"/>.</summary>
    extension(InstalledRoomObjectMaridiaElevatubeVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectMaridiaElevatubeVisuals),
            content => content.AppendAppearances("block", [visuals.GetAppearance(MaridiaElevatubeObjectDefinitions.BlockDraw, 0, 0)]));
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomObjectSpeedBoosterVisuals"/>.</summary>
    extension(InstalledRoomObjectSpeedBoosterVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomObjectSpeedBoosterVisuals),
            content => content.AppendAppearances("block", [visuals.GetAppearance(SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw, 0, 0)]));
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectTourianAccessVisuals"/>.</summary>
    extension(RoomObjectTourianAccessVisuals visuals)
    {
        /// <summary>
        /// Canonical identity of the selected visual frames, excluding mechanics. The identity keeps the name it was
        /// introduced under, so installed content identities do not change.
        /// </summary>
        public string ContentIdentity => SelectedPresentationHash.Create("RoomObjectTourianAccessVisualCatalog", content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[24];
            foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>().OrderBy(RoomObjectTourianAccessVisualFiles.VisualId, StringComparer.Ordinal))
            {
                TourianAccessDraws.Describe(draw, out int rows, out _);
                for (int index = 0; index < rows * 4; index++)
                    appearances[index] = visuals.GetAppearance(RoomObjectDraw.Of(draw), index / 4, index % 4);
                content.AppendName("frame", RoomObjectTourianAccessVisualFiles.VisualId(draw));
                // Preserve the original flattened-frame hash, including the six-row clear.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances[..(rows * 4)]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectCrocomireVisualCatalog"/>.</summary>
    extension(RoomObjectCrocomireVisualCatalog visuals)
    {
        /// <summary>Hashes every arena layout's selected appearances, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectCrocomireVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[24];
                foreach (var draw in RoomObjectCrocomireVisualFiles.All.OrderBy(item => RoomObjectCrocomireVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    CrocomireArenaDraws.TryDescribe(draw.Draw, out var shape);
                    int count = shape.RunCount * shape.BlocksPerRun;
                    for (int index = 0; index < count; index++)
                        appearances[index] = visuals.GetAppearance(draw.Draw, index / shape.BlocksPerRun, index % shape.BlocksPerRun);
                    content.AppendName("frame", RoomObjectCrocomireVisualFiles.VisualId(draw.Draw));
                    // Preserve the flattened-frame identity used by existing visual installations.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectDraygonCannonVisualCatalog"/>.</summary>
    extension(RoomObjectDraygonCannonVisualCatalog visuals)
    {
        /// <summary>Hashes every cannon frame's selected appearances, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectDraygonCannonVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[4];
                foreach (var frame in RoomObjectDraygonCannonVisualFiles.All.OrderBy(item => RoomObjectDraygonCannonVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    DraygonCannonDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"{nameof(visuals)} lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomObjectDraygonCannonVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed cannon artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectEscapeGateVisualCatalog"/>.</summary>
    extension(RoomObjectEscapeGateVisualCatalog visuals)
    {
        /// <summary>Hashes every gate frame's selected appearances, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectEscapeGateVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[4];
                foreach (var frame in RoomObjectEscapeGateVisualFiles.All.OrderBy(item => RoomObjectEscapeGateVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    for (int row = 0; row < appearances.Length; row++)
                        appearances[row] = visuals.GetAppearance(frame.Draw, row) ??
                            throw new InvalidDataException($"Escape-gate visuals lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomObjectEscapeGateVisualFiles.VisualId(frame.Draw));
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectMotherBrainFakeDeathVisualCatalog"/>.</summary>
    extension(RoomObjectMotherBrainFakeDeathVisualCatalog visuals)
    {
        /// <summary>Hashes every layout's selected appearances as one flattened frame, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectMotherBrainFakeDeathVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[14];
                foreach (var draw in RoomObjectMotherBrainFakeDeathVisualFiles.All.OrderBy(item => RoomObjectMotherBrainFakeDeathVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    int index = 0;
                    for (int run = 0; run < draw.Runs.Length; run++)
                        for (int block = 0; block < draw.Runs.Span[run].Blocks.Length; block++)
                            appearances[index++] = visuals.GetAppearance(draw.Draw, run, block) ??
                                throw new InvalidDataException($"Mother Brain fake-death visuals lack draw {draw.Draw}.");
                    content.AppendName("frame", RoomObjectMotherBrainFakeDeathVisualFiles.VisualId(draw.Draw));
                    // Existing installations hash each draw as one flattened visual frame.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, index));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectMotherBrainGlassVisualCatalog"/>.</summary>
    extension(RoomObjectMotherBrainGlassVisualCatalog visuals)
    {
        /// <summary>Hashes every glass layout's selected appearances as one flattened frame, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectMotherBrainGlassVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[16];
                foreach (var frame in RoomObjectMotherBrainGlassVisualFiles.All.OrderBy(item => RoomObjectMotherBrainGlassVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    MotherBrainGlassDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"{nameof(visuals)} lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomObjectMotherBrainGlassVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed glass artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectSporeSpawnCeilingVisualCatalog"/>.</summary>
    extension(RoomObjectSporeSpawnCeilingVisualCatalog visuals)
    {
        /// <summary>Hashes every ceiling frame's four selected appearances as one flattened frame, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectSporeSpawnCeilingVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[4];
                foreach (var draw in RoomObjectSporeSpawnCeilingVisualFiles.All.OrderBy(item => RoomObjectSporeSpawnCeilingVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    for (int index = 0; index < appearances.Length; index++)
                        appearances[index] = visuals.GetAppearance(draw.Draw, index / 2, index % 2);
                    content.AppendName("frame", RoomObjectSporeSpawnCeilingVisualFiles.VisualId(draw.Draw));
                    // Preserve the original flattened frame identity.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectBombTorizoHandVisualCatalog"/>.</summary>
    extension(RoomObjectBombTorizoHandVisualCatalog visuals)
    {
        /// <summary>Hashes every hand frame's selected appearances, flattened into one run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectBombTorizoHandVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[16];
                foreach (var frame in RoomObjectBombTorizoHandVisualFiles.All.OrderBy(item => RoomObjectBombTorizoHandVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    BombTorizoHandDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell);
                    content.AppendName("frame", RoomObjectBombTorizoHandVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed hand artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectChozoStatueVisualCatalog"/>.</summary>
    extension(RoomObjectChozoStatueVisualCatalog visuals)
    {
        /// <summary>Hashes every frame's selected appearances, flattened into one run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectChozoStatueVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[27];
                foreach (var frame in RoomObjectChozoStatueVisualFiles.All.OrderBy(item => RoomObjectChozoStatueVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    ChozoStatueDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell);
                    content.AppendName("frame", RoomObjectChozoStatueVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed Chozo artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomObjectElevatorPlatformVisualCatalog"/>.</summary>
    extension(RoomObjectElevatorPlatformVisualCatalog visuals)
    {
        /// <summary>Hashes each frame's selected appearances run by run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomObjectElevatorPlatformVisualCatalog), content =>
            {
                var buffer = new RoomBlockAppearance[4];
                foreach (var frame in RoomObjectElevatorPlatformVisualFiles.DrawLists.OrderBy(item => RoomObjectElevatorPlatformVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    ElevatorPlatformDraws.TryDescribe(frame.Draw, out var shape);
                    content.AppendName("frame", RoomObjectElevatorPlatformVisualFiles.VisualId(frame.Draw));
                    content.Append("runs", shape.RunCount);
                    for (int run = 0; run < shape.RunCount; run++)
                    {
                        int count = shape.BlockCount(run);
                        for (int cell = 0; cell < count; cell++)
                            buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell);
                        content.AppendAppearances("blocks", buffer.AsSpan(0, count));
                    }
                }
            });
    }
}
