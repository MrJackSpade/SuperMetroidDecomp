using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Verification access: runs the enemy-format draw passes as gameplay does and appends the OAM image their whole-frame
/// draws stand for to a native-style OAM buffer, so OAM oracles keep comparing the same records.
/// </summary>
internal static class EnemyFormatDrawAccess
{
    /// <summary>
    /// What a hit burst draws in these enemy-format passes: no projectile sheets, so a burst drawn here fails loudly. Its
    /// picture is Samus's weapon effect on her projectile sheets (#1713), which no enemy-format record stands for.
    /// </summary>
    internal static SamusHitBurstArtwork NoHitBurstSheets => new(new ProjectileDrawArtwork(Sheets: null), SamusBeamCombination.Power);

    /// <summary>Appends the OAM image of a draw's published entries, in emission order, after the buffer's records.</summary>
    extension(OamBuffer target)
    {
        /// <summary>
        /// Lets <paramref name="draw"/> publish into a fresh frame and appends the OAM records its draws and OAM sprites
        /// stand for, wrapping at the 128th record as the native enemy writer does.
        /// </summary>
        internal void AppendDrawn(Action<SpriteFrameBuffer> draw)
        {
            var frame = new SpriteFrameBuffer();
            frame.BeginFrame();
            draw(frame);
            OamBuffer image = frame.FinalizeAsOam();
            // Which pictures were drawn where, so StandsFor can match each to the composition the native OAM uses (#1640).
            DrawnRuns.GetOrCreateValue(target).Add(new DrawnRun(target.NextByteOffset, [.. frame.Draws.Cast<ImageSpriteDraw>()]));
            for (int index = 0; index < image.LastFinalizedSpriteCount; index++)
            {
                int spriteIndex = target.NextByteOffset >> 2;
                image.LowTable.Slice(index * 4, 4).CopyTo(target.WritableLowTable()[target.NextByteOffset..]);
                OamEntry entry = image.GetEntry(index);
                target.SetHighTablePair(spriteIndex, SnesOamHighTablePair.FromSprite(unchecked(entry.Position.X), entry.IsLarge));
                target.NextByteOffset = (target.NextByteOffset + 4) % OamBuffer.LowTableByteCount;
            }
        }

        /// <summary>
        /// Whether the records drawn into this buffer stand for <paramref name="native"/>'s (#1640). A picture several
        /// compositions share (#1513, #1633, #1635) can stand for any of them: each graphics set loading its sheet composes it
        /// at its own tile base and palette row, and the cartridge's OAM is one of those. Each drawn picture, in emission
        /// order, is matched to the composition whose records equal the native records at its place; the buffer stands for
        /// the native OAM when every picture matches one and every record then agrees.
        /// </summary>
        /// <param name="native">The cartridge's OAM.</param>
        /// <param name="apartFromPriority">Whether the parts' priority bits are left out of the comparison.</param>
        internal bool StandsFor(OamBuffer native, bool apartFromPriority = false)
        {
            Ensure.NotNull(native);
            if (target.NextByteOffset != native.NextByteOffset || !target.HighTable.SequenceEqual(native.HighTable))
                return false;
            byte[] rebuilt = target.LowTable.ToArray();
            foreach (DrawnRun run in DrawnRuns.TryGetValue(target, out List<DrawnRun>? runs) ? runs : [])
            {
                int offset = run.Offset;
                foreach (ImageSpriteDraw drawn in run.Draws)
                {
                    byte[]? matched = null;
                    foreach (CharacterSpriteDraw candidate in SuperMetroid.TestSupport.RepositoryInstallation.Verification.SheetFrameSources.CharacterDrawCandidates(drawn))
                    {
                        byte[] records = SpriteFrameBufferAccess.Assemble([candidate], parkUnused: false).LowTable[..(candidate.Frame.Parts.Length * 4)].ToArray();
                        if (SameRecords(records, native.LowTable, offset, apartFromPriority))
                        {
                            matched = records;
                            break;
                        }
                    }
                    if (matched is null)
                        return false;
                    for (int index = 0; index < matched.Length; index++)
                        rebuilt[(offset + index) % OamBuffer.LowTableByteCount] = matched[index];
                    offset = (offset + matched.Length) % OamBuffer.LowTableByteCount;
                }
            }
            return SameRecords(rebuilt, native.LowTable, 0, apartFromPriority);
        }
    }

    /// <summary>The attribute bits holding a part's priority, in the high byte of its OAM record's attribute word.</summary>
    private const byte PriorityBits = 0x30;

    /// <summary>Each OAM buffer's appended picture draws, by the byte offset their records start at.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<OamBuffer, List<DrawnRun>> DrawnRuns = new();

    /// <summary>Whether <paramref name="records"/> equal <paramref name="native"/>'s low-table bytes from <paramref name="offset"/>, wrapping, priority aside when asked.</summary>
    private static bool SameRecords(ReadOnlySpan<byte> records, ReadOnlySpan<byte> native, int offset, bool apartFromPriority)
    {
        for (int index = 0; index < records.Length; index++)
        {
            int at = (offset + index) % OamBuffer.LowTableByteCount;
            byte mask = apartFromPriority && at % 4 == 3 ? unchecked((byte)~PriorityBits) : (byte)0xFF;
            if ((records[index] & mask) != (native[at] & mask))
                return false;
        }
        return true;
    }

    /// <summary>One append's picture draws and the byte offset their records start at.</summary>
    /// <param name="Offset">The low-table byte offset of the run's first record.</param>
    /// <param name="Draws">The picture draws, in emission order.</param>
    private sealed record DrawnRun(int Offset, ImageSpriteDraw[] Draws);

    /// <summary>Runs one enemy layer range's advance and publish passes.</summary>
    extension(EnemyDrawList drawing)
    {
        /// <summary>Advances and draws an inclusive layer range, appending its OAM image to <paramref name="oam"/>.</summary>
        internal void DrawAsOam(OamBuffer oam, Position16<RoomPixels> camera)
        {
            drawing.Advance(camera);
            oam.AppendDrawn(frame => drawing.Draw(frame, camera));
        }
    }

    /// <summary>Draws room sprite objects into an OAM image.</summary>
    extension(RoomSpriteObjects pool)
    {
        /// <summary>Publishes the room sprite objects, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawRoomSpriteObjectsAsOam(OamBuffer oam, Position16<RoomPixels> camera) =>
            oam.AppendDrawn(frame => pool.DrawRoomSpriteObjects(frame, camera, NoHitBurstSheets));
    }

    /// <summary>Draws enemy projectiles into an OAM image.</summary>
    extension(EnemyProjectileEngine engine)
    {
        /// <summary>Publishes the projectiles at one depth relative to Samus, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawEnemyProjectilesAtAsOam(OamBuffer oam, Position16<RoomPixels> camera,
            EnemyProjectileDepth depth, bool timeIsFrozen) =>
            oam.AppendDrawn(frame => engine.DrawEnemyProjectilesAt(frame, camera, depth, timeIsFrozen));

        /// <summary>Publishes the room sprite objects and the projectiles in front of Samus, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawEnemyProjectilesInFrontOfSamusAsOam(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false) =>
            oam.AppendDrawn(frame => engine.DrawEnemyProjectilesInFrontOfSamus(frame, camera, NoHitBurstSheets, timeIsFrozen));

        /// <summary>Publishes the projectiles behind Samus, appending their OAM image to <paramref name="oam"/>.</summary>
        internal void DrawEnemyProjectilesBehindSamusAsOam(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false) =>
            oam.AppendDrawn(frame => engine.DrawEnemyProjectilesBehindSamus(frame, camera, timeIsFrozen));
    }

    /// <summary>Draws the Ceres elevator arrival actors into an OAM image.</summary>
    extension(CeresElevatorArrivalState arrival)
    {
        /// <summary>
        /// Publishes both arrival actors through <paramref name="presentation"/>, appending their OAM image to
        /// <paramref name="oam"/>.
        /// </summary>
        internal void DrawAsOam(OamBuffer oam, EnemyPresentation presentation, Position16<RoomPixels> camera) =>
            oam.AppendDrawn(frame => arrival.Draw(frame, presentation, camera));
    }
}
