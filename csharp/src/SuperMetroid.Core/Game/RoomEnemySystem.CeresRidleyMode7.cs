using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const int CeresRidleyMode7ZoomTable = 0xa6ae4d;
    private const int CeresRidleyMode7YVelocityTable = 0xa6af2f;
    private const int CeresRidleyMode7XVelocityTable = 0xa6b00f;

    /// <summary>
    /// Ports $A6:AA54. Every value below is a literal PPU/actor word installed by the retail
    /// setup routine; in particular B and C intentionally begin at $0100 before the first
    /// getaway-table entry replaces the matrix.
    /// </summary>
    private static void SetupCeresRidleyMode7(RidleyEnemyState state)
    {
        state.Mode7Active = true;
        state.Mode7Finished = false;
        state.Mode7TableByteIndex = 0;
        state.Mode7Angle = 1;
        state.Mode7HorizontalOffset = 0xff80;
        state.Mode7VerticalOffset = 0x0020;
        state.Mode7Zoom = 0x0800;
        state.Mode7MatrixA = 0x0100;
        state.Mode7MatrixB = 0x0100;
        state.Mode7MatrixC = 0x0100;
        state.Mode7MatrixD = 0x0100;
        state.Mode7CenterX = 0x0040;
        state.Mode7CenterY = 0x0040;
        state.Mode7BabyFrame = 0;
        state.Mode7WingFrame = 0;
    }

    /// <summary>
    /// Ports room-main function $A6:AABD through its $FFFF table terminator. This routine
    /// owns presentation only: ordinary Ridley has already been hidden, while Samus remains
    /// in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    private void TickCeresRidleyMode7Getaway(
        RidleyEnemyState state,
        SamusState? samus,
        ushort nmiFrameCounter)
    {
        ushort tableByteIndex = state.Mode7TableByteIndex;
        state.Mode7TableByteIndex = unchecked((ushort)(tableByteIndex + 2));

        // At byte index $D0, `$90:E119` replaces Samus's movement/hack handlers so the
        // rotating boss image cannot overlap her. Room main executes after Samus movement
        // natively; Request retains that one-frame boundary even though this actor currently
        // advances during the runtime's earlier EnemyMain phase.
        if (tableByteIndex == 0x00d0 && samus is not null)
            samus.CeresRidleyEjection.Request();

        ushort zoom = ReadWord(_bus!, CeresRidleyMode7ZoomTable + tableByteIndex);
        if (zoom == 0xffff)
        {
            // $A6:AB2E restores fake BGMODE=$09 and clears all Mode 7/scroll registers.
            // Preserve that clean handoff explicitly so the renderer cannot accidentally
            // keep sampling Mode 7 VRAM for the self-destruct text scene.
            state.Mode7Finished = true;
            state.Mode7Active = false;
            state.Mode7HorizontalOffset = 0;
            state.Mode7VerticalOffset = 0;
            state.Mode7MatrixA = 0;
            state.Mode7MatrixB = 0;
            state.Mode7MatrixC = 0;
            state.Mode7MatrixD = 0;
            state.Mode7CenterX = 0;
            state.Mode7CenterY = 0;
            // $A6:AB55 hands the same extended actor workspace to $A6:C04E. Samus's
            // special push/fall handler remains responsible for restoring normal input;
            // ending Mode 7 itself does not unlock her in the cartridge.
            state.Function = RidleyAiFunction.CeresActivateSelfDestruct;
            state.FunctionTimer = 0;
            return;
        }

        state.Mode7Zoom = zoom;
        state.Mode7VerticalOffset = unchecked((ushort)(
            state.Mode7VerticalOffset +
            ReadWord(_bus!, CeresRidleyMode7YVelocityTable + tableByteIndex)));
        state.Mode7HorizontalOffset = unchecked((ushort)(
            state.Mode7HorizontalOffset -
            ReadWord(_bus!, CeresRidleyMode7XVelocityTable + tableByteIndex)));

        UpdateCeresRidleyMode7Palette(zoom);
        state.Mode7Angle = unchecked((ushort)(state.Mode7Angle + 0x0030));
        UpdateCeresRidleyMode7Matrix(state);

        // $A6:ACBC advances the Baby capsule on every fourth NMI using 0,1,2,1. The transfer
        // lists patch only the two two-byte rows occupied by that animation in the map.
        if ((nmiFrameCounter & 3) == 0)
        {
            state.Mode7BabyFrame = unchecked((ushort)((state.Mode7BabyFrame + 1) & 3));
            ushort[] babyTransferPointers = [0xace2, 0xacf5, 0xad08, 0xacf5];
            ApplyMode7TransferList(babyTransferPointers[state.Mode7BabyFrame]);
        }

        // $A6:AD27 alternates the six sparse wing rows on every eighth NMI.
        if ((nmiFrameCounter & 7) == 0)
        {
            state.Mode7WingFrame = unchecked((ushort)((state.Mode7WingFrame + 1) & 1));
            ApplyMode7TransferList(state.Mode7WingFrame == 0 ? (ushort)0xad49 : (ushort)0xad80);
        }
    }

    private void UpdateCeresRidleyMode7Matrix(RidleyEnemyState state)
    {
        byte angle = unchecked((byte)(state.Mode7Angle >> 8));
        ushort diagonal = MultiplyCartridgeSinCos(state.Mode7Zoom, unchecked((byte)(angle + 64)));
        ushort offDiagonal = MultiplyCartridgeSinCos(state.Mode7Zoom, angle);
        state.Mode7MatrixA = diagonal;
        state.Mode7MatrixB = offDiagonal;
        state.Mode7MatrixC = unchecked((ushort)-(short)offDiagonal);
        state.Mode7MatrixD = diagonal;
    }

    private void UpdateCeresRidleyMode7Palette(ushort zoom)
    {
        // SetCeresRidleyPaletteAccordingToZoomLevel uses this wrapped bank-$A6 expression.
        // There are fifteen colors; palette entry zero remains the shared transparent color.
        ushort sourcePointer = unchecked((ushort)(((zoom >> 8) * 32) - 0x4ef9));
        _cgram!.LoadFromBus(_bus!, 0xa60000 | sourcePointer, colorCount: 15, destinationIndex: 0x00a2 / 2);
    }

    /// <summary>
    /// Executes one $80:8B4F Mode-7 DMA-list payload synchronously. The Ceres lists use only
    /// low-byte VRAM writes ($2118), but rejecting any other target keeps later additions
    /// from silently treating character or CGRAM data as a tile number.
    /// </summary>
    private void ApplyMode7TransferList(ushort pointer)
    {
        int cursor = 0xa60000 | pointer;
        for (int transferCount = 0; transferCount < 16; transferCount++)
        {
            byte control = _bus!.ReadByte(cursor);
            if (control == 0)
                return;
            if ((control & 0xc0) != 0x80)
            {
                throw new InvalidDataException(
                    $"Ceres Mode-7 transfer $A6:{cursor & 0xffff:X4} uses unsupported control ${control:X2}.");
            }

            int sourceAddress = _bus.ReadByte(AdvanceBankAddress(cursor, 1)) |
                (_bus.ReadByte(AdvanceBankAddress(cursor, 2)) << 8) |
                (_bus.ReadByte(AdvanceBankAddress(cursor, 3)) << 16);
            ushort byteCount = ReadWord(_bus, AdvanceBankAddress(cursor, 4));
            ushort destinationWord = ReadWord(_bus, AdvanceBankAddress(cursor, 6));
            byte incrementMode = _bus.ReadByte(AdvanceBankAddress(cursor, 8));
            if (incrementMode != 0)
            {
                throw new InvalidDataException(
                    $"Ceres Mode-7 transfer $A6:{cursor & 0xffff:X4} uses VMAIN ${incrementMode:X2}.");
            }

            byte[] source = new byte[byteCount];
            int sourceBank = sourceAddress & 0xff0000;
            int sourceOffset = sourceAddress & 0xffff;
            for (int index = 0; index < source.Length; index++)
                source[index] = _bus.ReadByte(sourceBank | ((sourceOffset + index) & 0xffff));
            _vram!.LoadMode7MapBytes(source, destinationWord);
            cursor = AdvanceBankAddress(cursor, 9);
        }

        throw new InvalidDataException("Ceres Mode-7 transfer list exceeded sixteen entries.");
    }

    private static int AdvanceBankAddress(int address, int byteCount) =>
        (address & 0xff0000) | ((address + byteCount) & 0xffff);
}
