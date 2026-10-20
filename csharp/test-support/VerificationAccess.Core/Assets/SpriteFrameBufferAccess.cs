using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Verification access: the native OAM image a sprite frame stands for, to compare with cartridge OAM oracles.</summary>
internal static class SpriteFrameBufferAccess
{
    /// <summary>Draws into a fresh frame and reads the OAM image it stands for.</summary>
    extension(SpriteFrameBuffer)
    {
        /// <summary>Begins a fresh frame, lets <paramref name="draw"/> publish into it, and returns its finalized OAM image.</summary>
        internal static OamBuffer DrawnOam(Action<SpriteFrameBuffer> draw)
        {
            var frame = new SpriteFrameBuffer();
            frame.BeginFrame();
            draw(frame);
            return frame.FinalizeAsOam();
        }
    }

    /// <summary>Rebuilds native OAM from a frame's whole-frame draws.</summary>
    extension(SpriteFrameBuffer sprites)
    {
        /// <summary>
        /// Finalizes the frame and returns the OAM image the cartridge builds for it: its draws' parts in emission
        /// order. Each part names OBJ character artwork-base + character, its coordinates wrap
        /// to OAM's nine-bit X and eight-bit Y, and 16-pixel parts are large. Unused records stay zero, as in a native
        /// buffer that was appended to but not finalized.
        /// </summary>
        internal OamBuffer FinalizeAsOam()
        {
            sprites.FinalizeFrame();
            return sprites.AssembledOam(parkUnused: false);
        }

        /// <summary>
        /// The OAM image an already finalized frame stands for, with unused records parked at Y=$F0 as `$80:896E`
        /// leaves them; compare it with a native buffer that was finalized.
        /// </summary>
        internal OamBuffer FinalizedAsOam() => sprites.AssembledOam(parkUnused: true);

        /// <summary>Assembles the finalized frame's draw parts in emission order.</summary>
        [PpuLayoutBoundary("exact hardware test: rebuilds native OAM tables from whole-frame draws")]
        internal OamBuffer AssembledOam(bool parkUnused)
        {
            var low = new byte[SnesPpuLayoutConstants.OamLowTableByteCount];
            var high = new byte[SnesPpuLayoutConstants.OamUploadByteCount - SnesPpuLayoutConstants.OamLowTableByteCount];
            int slot = 0;
            foreach (SpriteDraw entry in sprites.Draws)
            {
                int baseCharacter = FirstObjCharacter(entry.Artwork.Id.Kind);
                for (int part = 0; part < entry.Frame.Parts.Length; part++)
                {
                    SpriteScreenPart placed = entry.Place(part);
                    bool large = placed.Size.X switch
                    {
                        8 => false,
                        16 => true,
                        _ => throw new InvalidDataException($"{entry} has a {placed.Size.X}-pixel part, not a gameplay OBJ size."),
                    };
                    var flips = (placed.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (placed.FlipY ? SnesTileFlipFlags.Vertical : 0);
                    Write(placed.Position.X, placed.Position.Y,
                        SnesObjAttributeWord.Create(baseCharacter + placed.Character, placed.Palette, placed.Priority, flips), large);
                }
            }
            if (parkUnused)
                for (int unused = slot; unused < OamBuffer.SpriteCount; unused++)
                    low[unused * 4 + 1] = ParkedY;
            var result = new OamBuffer();
            result.LoadUploadPayload([.. low, .. high], slot);
            return result;

            void Write(int x, int y, SnesObjAttributeWord attributes, bool large)
            {
                if (slot == OamBuffer.SpriteCount)
                    throw new InvalidOperationException($"The frame stands for more than {OamBuffer.SpriteCount} OAM sprites.");
                ushort wrappedX = unchecked((ushort)(x & SnesSpritemapXWord.OffsetFieldMaximum));
                low[slot * 4] = unchecked((byte)wrappedX);
                low[slot * 4 + 1] = unchecked((byte)y);
                low[slot * 4 + 2] = SnesLittleEndian.LowByte(attributes.Raw);
                low[slot * 4 + 3] = SnesLittleEndian.HighByte(attributes.Raw);
                high[slot / 4] |= (byte)(SnesOamHighTablePair.FromSprite(wrappedX, large).Raw << (slot % 4 * 2));
                slot++;
            }
        }
    }

    /// <summary>The Y at which `$80:896E` parks unused OAM records, below the visible screen.</summary>
    private const byte ParkedY = 240;

    /// <summary>OBJ tile 31 ($1F), where `$90:C7DF` uploads the arm-cannon cover the cannon's sprite names.</summary>
    private const int ArmCannonCoverCharacter = 31;

    /// <summary>The OBJ character an artwork's character 0 stands for in the gameplay OBJ table.</summary>
    internal static int FirstObjCharacter(SpriteArtworkKind kind) => kind switch
    {
        SpriteArtworkKind.EscapeTimerTiles => SuperMetroid.Core.Areas.Escape.Artwork.EscapeTimerPresentationDefinitions.FirstCharacter,
        SpriteArtworkKind.SamusArmCannonCover => ArmCannonCoverCharacter,
        SpriteArtworkKind.GrappleTiles => SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions.FirstCharacter,
        _ => 0,
    };
}
