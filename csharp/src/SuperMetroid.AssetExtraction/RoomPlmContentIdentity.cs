using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the room objects' visual catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class RoomPlmContentIdentity
{
    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmBlueDoorVisuals"/>.</summary>
    extension(InstalledRoomPlmBlueDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmBlueDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomPlmBlueDoorVisualFiles.Editable.OrderBy(item => RoomPlmBlueDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int row = 0; row < appearances.Length; row++) appearances[row] = visuals.GetAppearance(frame.Draw, row);
                content.AppendName("frame", RoomPlmBlueDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmBotwoonWallVisuals"/>.</summary>
    extension(InstalledRoomPlmBotwoonWallVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmBotwoonWallVisuals),
            content =>
            {
                Span<RoomBlockAppearance> selected = stackalloc RoomBlockAppearance[BotwoonWallPlmDrawDefinitions.BlockCount];
                for (int index = 0; index < selected.Length; index++)
                    selected[index] = visuals.GetAppearance(BotwoonWallPlmDrawDefinitions.Clear, 0, index);
                content.AppendAppearances("blocks", selected);
            });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmCollectibleVisuals"/>.</summary>
    extension(InstalledRoomPlmCollectibleVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmCollectibleVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[1];
            foreach (var frame in RoomPlmCollectibleVisualFiles.All.OrderBy(frame => frame.Id, StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.Id);
                content.Append("runs", 1);
                buffer[0] = visuals.GetAppearance(PlmDraw.Of(frame.Draw));
                content.AppendAppearances("blocks", buffer);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmColoredDoorVisuals"/>.</summary>
    extension(InstalledRoomPlmColoredDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmColoredDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomPlmColoredDoorVisualFiles.All.OrderBy(item => RoomPlmColoredDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int block = 0; block < appearances.Length; block++) appearances[block] = visuals.GetAppearance(frame.Draw, block);
                content.AppendName("frame", RoomPlmColoredDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmDownwardGateVisuals"/>.</summary>
    extension(InstalledRoomPlmDownwardGateVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmDownwardGateVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[5];
            foreach (var frame in RoomPlmDownwardGateVisualFiles.All.OrderBy(item => RoomPlmDownwardGateVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                DownwardGatePlmDraws.TryDescribe(frame.Draw, out var draw);
                content.AppendName("frame", RoomPlmDownwardGateVisualFiles.VisualId(frame.Draw));
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

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmEyeDoorVisuals"/>.</summary>
    extension(InstalledRoomPlmEyeDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmEyeDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomPlmEyeDoorVisualFiles.Editable.OrderBy(item => RoomPlmEyeDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                EyeDoorDraws.TryDescribe(frame.Draw, out var shape);
                content.AppendName("frame", RoomPlmEyeDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                for (int cell = 0; cell < shape.BlockCount; cell++) buffer[cell] = visuals.GetAppearance(frame.Draw, cell);
                content.AppendAppearances("blocks", buffer[..shape.BlockCount]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmGrappleBlockVisuals"/>.</summary>
    extension(InstalledRoomPlmGrappleBlockVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmGrappleBlockVisuals), content =>
        {
            Span<RoomBlockAppearance> block = stackalloc RoomBlockAppearance[1];
            foreach (var draw in RoomPlmGrappleBlockVisualFiles.All.OrderBy(draw => RoomPlmGrappleBlockVisualFiles.VisualId(draw.Frame), StringComparer.Ordinal))
            {
                content.AppendName("frame", RoomPlmGrappleBlockVisualFiles.VisualId(draw.Frame));
                content.Append("runs", 1);
                block[0] = visuals.GetAppearance(PlmDraw.Of(draw.Frame));
                content.AppendAppearances("blocks", block);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmGreyDoorVisuals"/>.</summary>
    extension(InstalledRoomPlmGreyDoorVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmGreyDoorVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var frame in RoomPlmGreyDoorVisualFiles.All.OrderBy(item => RoomPlmGreyDoorVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int row = 0; row < appearances.Length; row++) appearances[row] = visuals.GetAppearance(frame.Draw, row);
                content.AppendName("frame", RoomPlmGreyDoorVisualFiles.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmNoobTubeVisuals"/>.</summary>
    extension(InstalledRoomPlmNoobTubeVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmNoobTubeVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[48];
            foreach (var frame in RoomPlmNoobTubeVisualFiles.All.OrderBy(item => RoomPlmNoobTubeVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                NoobTubePlmDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int cell = 0; cell < shape.BlockCount(run); cell++)
                        appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell);
                content.AppendName("frame", RoomPlmNoobTubeVisualFiles.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed tube artwork.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances[..count]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmSamusEaterVisuals"/>.</summary>
    extension(InstalledRoomPlmSamusEaterVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmSamusEaterVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[8];
            foreach (var draw in RoomPlmSamusEaterVisualFiles.All.OrderBy(item => RoomPlmSamusEaterVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                int index = 0;
                for (int run = 0; run < 3; run++)
                    for (int block = 0; block < SamusEaterPlmDraws.Draw.Count(run); block++)
                        appearances[index++] = visuals.GetAppearance(draw.Draw, run, block);
                content.AppendName("frame", RoomPlmSamusEaterVisualFiles.VisualId(draw.Draw));
                // Preserve the original flattened-frame hash, not the three physical runs.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmShotBlockVisuals"/>.</summary>
    extension(InstalledRoomPlmShotBlockVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmShotBlockVisuals), content =>
        {
            Span<RoomBlockAppearance> row = stackalloc RoomBlockAppearance[2];
            foreach (var draw in RoomPlmShotBlockVisualFiles.Calculated.OrderBy(RoomPlmShotBlockVisualFiles.VisualId, StringComparer.Ordinal))
            {
                content.AppendName("frame", RoomPlmShotBlockVisualFiles.VisualId(draw));
                content.Append("runs", draw.RunCount);
                for (int run = 0; run < draw.RunCount; run++)
                {
                    for (int block = 0; block < draw.BlocksPerRun; block++)
                        row[block] = visuals.GetAppearance(PlmDraw.Of(draw), run, block);
                    content.AppendAppearances("blocks", row[..draw.BlocksPerRun]);
                }
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmStationVisuals"/>.</summary>
    extension(InstalledRoomPlmStationVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmStationVisuals), content =>
        {
            Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[2];
            foreach (var frame in RoomPlmStationVisualFiles.All.OrderBy(item => RoomPlmStationVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                var shape = RoomPlmStationVisualFiles.Describe(frame.Draw);
                content.AppendName("frame", RoomPlmStationVisualFiles.VisualId(frame.Draw));
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

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmKraidVisuals"/>.</summary>
    extension(InstalledRoomPlmKraidVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmKraidVisuals), content =>
        {
            foreach (KraidRoomDraw draw in InstalledRoomPlmKraidVisuals.ContentOrder.OrderBy(draw => RoomPlmKraidVisualFiles.VisualId(PlmDraw.Of(draw)), StringComparer.Ordinal))
                Append(draw, visuals.frames[draw]);

            void Append(KraidRoomDraw draw, RoomBlockAppearance[] appearances)
            {
                content.AppendName("frame", RoomPlmKraidVisualFiles.VisualId(PlmDraw.Of(draw)));
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmLinkedRestoreVisuals"/>.</summary>
    extension(InstalledRoomPlmLinkedRestoreVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmLinkedRestoreVisuals), content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[4];
            foreach (var draw in RoomPlmLinkedRestoreVisualFiles.All.OrderBy(item => RoomPlmLinkedRestoreVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
            {
                InstalledRoomPlmLinkedRestoreVisuals.Describe(draw.Draw, out int runs, out _);
                for (int run = 0; run < runs; run++)
                    for (int block = 0; block < 2; block++)
                        appearances[run * 2 + block] = visuals.GetAppearance(draw.Draw, run, block);
                content.AppendName("frame", RoomPlmLinkedRestoreVisualFiles.VisualId(draw.Draw));
                // The original catalog hashes each flattened frame as one run.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances[..(runs * 2)]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmMaridiaElevatubeVisuals"/>.</summary>
    extension(InstalledRoomPlmMaridiaElevatubeVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmMaridiaElevatubeVisuals),
            content => content.AppendAppearances("block", [visuals.GetAppearance(MaridiaElevatubePlmDefinitions.BlockDraw, 0, 0)]));
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomPlmSpeedBoosterVisuals"/>.</summary>
    extension(InstalledRoomPlmSpeedBoosterVisuals visuals)
    {
        /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmSpeedBoosterVisuals),
            content => content.AppendAppearances("block", [visuals.GetAppearance(SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw, 0, 0)]));
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmTourianAccessVisuals"/>.</summary>
    extension(RoomPlmTourianAccessVisuals visuals)
    {
        /// <summary>
        /// Canonical identity of the selected visual frames, excluding mechanics. The identity keeps the name it was
        /// introduced under, so installed content identities do not change.
        /// </summary>
        public string ContentIdentity => SelectedPresentationHash.Create("RoomPlmTourianAccessVisualCatalog", content =>
        {
            Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[24];
            foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>().OrderBy(RoomPlmTourianAccessVisualFiles.VisualId, StringComparer.Ordinal))
            {
                TourianAccessDraws.Describe(draw, out int rows, out _);
                for (int index = 0; index < rows * 4; index++)
                    appearances[index] = visuals.GetAppearance(PlmDraw.Of(draw), index / 4, index % 4);
                content.AppendName("frame", RoomPlmTourianAccessVisualFiles.VisualId(draw));
                // Preserve the original flattened-frame hash, including the six-row clear.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances[..(rows * 4)]);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmCrocomireVisualCatalog"/>.</summary>
    extension(RoomPlmCrocomireVisualCatalog visuals)
    {
        /// <summary>Hashes every arena layout's selected appearances, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmCrocomireVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[24];
                foreach (var draw in RoomPlmCrocomireVisualFiles.All.OrderBy(item => RoomPlmCrocomireVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    CrocomireArenaDraws.TryDescribe(draw.Draw, out var shape);
                    int count = shape.RunCount * shape.BlocksPerRun;
                    for (int index = 0; index < count; index++)
                        appearances[index] = visuals.GetAppearance(draw.Draw, index / shape.BlocksPerRun, index % shape.BlocksPerRun) ??
                            throw new InvalidDataException($"Crocomire visuals lack draw {draw.Draw}.");
                    content.AppendName("frame", RoomPlmCrocomireVisualFiles.VisualId(draw.Draw));
                    // Preserve the flattened-frame identity used by existing visual installations.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmDraygonCannonVisualCatalog"/>.</summary>
    extension(RoomPlmDraygonCannonVisualCatalog visuals)
    {
        /// <summary>Hashes every cannon frame's selected appearances, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmDraygonCannonVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[4];
                foreach (var frame in RoomPlmDraygonCannonVisualFiles.All.OrderBy(item => RoomPlmDraygonCannonVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    DraygonCannonDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"{nameof(visuals)} lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomPlmDraygonCannonVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed cannon artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmEscapeGateVisualCatalog"/>.</summary>
    extension(RoomPlmEscapeGateVisualCatalog visuals)
    {
        /// <summary>Hashes every gate frame's selected appearances, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmEscapeGateVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[4];
                foreach (var frame in RoomPlmEscapeGateVisualFiles.All.OrderBy(item => RoomPlmEscapeGateVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    for (int row = 0; row < appearances.Length; row++)
                        appearances[row] = visuals.GetAppearance(frame.Draw, row) ??
                            throw new InvalidDataException($"Escape-gate visuals lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomPlmEscapeGateVisualFiles.VisualId(frame.Draw));
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmMotherBrainFakeDeathVisualCatalog"/>.</summary>
    extension(RoomPlmMotherBrainFakeDeathVisualCatalog visuals)
    {
        /// <summary>Hashes every layout's selected appearances as one flattened frame, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainFakeDeathVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[14];
                foreach (var draw in RoomPlmMotherBrainFakeDeathVisualFiles.All.OrderBy(item => RoomPlmMotherBrainFakeDeathVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    int index = 0;
                    for (int run = 0; run < draw.Runs.Length; run++)
                        for (int block = 0; block < draw.Runs.Span[run].Blocks.Length; block++)
                            appearances[index++] = visuals.GetAppearance(draw.Draw, run, block) ??
                                throw new InvalidDataException($"Mother Brain fake-death visuals lack draw {draw.Draw}.");
                    content.AppendName("frame", RoomPlmMotherBrainFakeDeathVisualFiles.VisualId(draw.Draw));
                    // Existing installations hash each draw as one flattened visual frame.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, index));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmMotherBrainGlassVisualCatalog"/>.</summary>
    extension(RoomPlmMotherBrainGlassVisualCatalog visuals)
    {
        /// <summary>Hashes every glass layout's selected appearances as one flattened frame, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainGlassVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[16];
                foreach (var frame in RoomPlmMotherBrainGlassVisualFiles.All.OrderBy(item => RoomPlmMotherBrainGlassVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    MotherBrainGlassDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"{nameof(visuals)} lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomPlmMotherBrainGlassVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed glass artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmSporeSpawnCeilingVisualCatalog"/>.</summary>
    extension(RoomPlmSporeSpawnCeilingVisualCatalog visuals)
    {
        /// <summary>Hashes every ceiling frame's four selected appearances as one flattened frame, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmSporeSpawnCeilingVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[4];
                foreach (var draw in RoomPlmSporeSpawnCeilingVisualFiles.All.OrderBy(item => RoomPlmSporeSpawnCeilingVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    for (int index = 0; index < appearances.Length; index++)
                        appearances[index] = visuals.GetAppearance(draw.Draw, index / 2, index % 2) ??
                            throw new InvalidDataException($"Spore Spawn ceiling visuals lack frame {draw.Draw}.");
                    content.AppendName("frame", RoomPlmSporeSpawnCeilingVisualFiles.VisualId(draw.Draw));
                    // Preserve the original flattened frame identity.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmBombTorizoHandVisualCatalog"/>.</summary>
    extension(RoomPlmBombTorizoHandVisualCatalog visuals)
    {
        /// <summary>Hashes every hand frame's selected appearances, flattened into one run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmBombTorizoHandVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[16];
                foreach (var frame in RoomPlmBombTorizoHandVisualFiles.All.OrderBy(item => RoomPlmBombTorizoHandVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    BombTorizoHandDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"{nameof(visuals)} lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomPlmBombTorizoHandVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed hand artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmChozoStatueVisualCatalog"/>.</summary>
    extension(RoomPlmChozoStatueVisualCatalog visuals)
    {
        /// <summary>Hashes every frame's selected appearances, flattened into one run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmChozoStatueVisualCatalog), content =>
            {
                var appearances = new RoomBlockAppearance[27];
                foreach (var frame in RoomPlmChozoStatueVisualFiles.All.OrderBy(item => RoomPlmChozoStatueVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    ChozoStatueDraws.TryDescribe(frame.Draw, out var shape);
                    int count = 0;
                    for (int run = 0; run < shape.RunCount; run++)
                        for (int cell = 0; cell < shape.BlockCount(run); cell++)
                            appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"Chozo statue visuals lack frame {frame.Draw}.");
                    content.AppendName("frame", RoomPlmChozoStatueVisualFiles.VisualId(frame.Draw));
                    // Preserve the flattened single-run framing of installed Chozo artwork.
                    content.Append("runs", 1);
                    content.AppendAppearances("blocks", appearances.AsSpan(0, count));
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="RoomPlmElevatorPlatformVisualCatalog"/>.</summary>
    extension(RoomPlmElevatorPlatformVisualCatalog visuals)
    {
        /// <summary>Hashes each frame's selected appearances run by run, in visual-identity order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create(nameof(RoomPlmElevatorPlatformVisualCatalog), content =>
            {
                var buffer = new RoomBlockAppearance[4];
                foreach (var frame in RoomPlmElevatorPlatformVisualFiles.DrawLists.OrderBy(item => RoomPlmElevatorPlatformVisualFiles.VisualId(item.Draw), StringComparer.Ordinal))
                {
                    ElevatorPlatformDraws.TryDescribe(frame.Draw, out var shape);
                    content.AppendName("frame", RoomPlmElevatorPlatformVisualFiles.VisualId(frame.Draw));
                    content.Append("runs", shape.RunCount);
                    for (int run = 0; run < shape.RunCount; run++)
                    {
                        int count = shape.BlockCount(run);
                        for (int cell = 0; cell < count; cell++)
                            buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell) ??
                                throw new InvalidDataException($"Elevator-platform visuals lack frame {frame.Draw}.");
                        content.AppendAppearances("blocks", buffer.AsSpan(0, count));
                    }
                }
            });
    }
}
