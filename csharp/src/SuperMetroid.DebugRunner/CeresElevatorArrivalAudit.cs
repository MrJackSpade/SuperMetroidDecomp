using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Reproduces issue #61 through the production frontend and guards the exact first-visible-
/// frame failure: the orange level-data elevator pad must remain concealed while the real
/// bank-$86 elevator arrives from above.
/// </summary>
internal static class CeresElevatorArrivalAudit
{
    // The stationary `$86:A395` concealer uses the four small tile-$20 entries authored by
    // spritemap `$8D:846D`. Their screen-space coordinates cover the landing pad's top lip.
    private const int ConcealerFirstX = 112;
    private const int ConcealerY = 93;
    private const int ConcealerTile = 0x20;
    private const int ConcealerPalette = 5;
    private const int ConcealerPriority = 3;
    private const int ConcealerEntryCount = 4;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });

        FrontendFrame frame = FrontendAuditDriver.EnterSelectedSlot(game);
        if (frame.GameState != SuperMetroidGameState.MainGameplayFadeIn ||
            game.GameplayActiveAreaIndex != AreaId.Ceres ||
            game.GameplayActiveRoomIndex != 0)
        {
            throw new InvalidDataException(
                $"Fresh Ceres setup skipped native state-seven fade: state={frame.GameState}, " +
                $"room={game.GameplayActiveAreaIndex}/{game.GameplayActiveRoomIndex}.");
        }
        AssertBlack(frame.Pixels, "state-seven setup frame");

        bool observedVisibleFadeFrame = false;
        int fadeFrames = 0;
        while (frame.GameState == SuperMetroidGameState.MainGameplayFadeIn)
        {
            frame = game.Step(0);
            fadeFrames++;
            if (IsBlack(frame.Pixels))
                continue;

            observedVisibleFadeFrame = true;
            SuperMetroidRuntime runtime = game.RuntimeForVerification
                ?? throw new InvalidOperationException("Ceres fade has no gameplay runtime.");
            AssertConcealerPublished(runtime.DisplayedOam);
            AssertNoOrangePhantom(frame.Pixels);
        }

        if (!observedVisibleFadeFrame)
            throw new InvalidDataException("Ceres state-seven fade never produced a visible frame.");
        if (frame.GameState != SuperMetroidGameState.MadeItToCeresElevator)
        {
            throw new InvalidDataException(
                $"Ceres fade ended in {frame.GameState}, not the cartridge elevator state.");
        }
        if (game.RuntimeForVerification?.CeresElevatorArrival is not { IsComplete: false })
            throw new InvalidDataException("Ceres elevator objects disappeared during fade-in.");

        Console.WriteLine(
            $"Ceres elevator arrival audit passed: {fadeFrames} state-seven frames, " +
            "first visible frame contained the native four-tile concealer and no orange phantom pad.");
        return 0;
    }

    private static void AssertConcealerPublished(OamBuffer oam)
    {
        var seenEntries = new bool[ConcealerEntryCount];
        for (int spriteIndex = 0; spriteIndex < OamBuffer.SpriteCount; spriteIndex++)
        {
            OamEntry entry = oam.GetEntry(spriteIndex);
            bool alignedX = entry.X >= ConcealerFirstX &&
                entry.X < ConcealerFirstX + ConcealerEntryCount * 8 &&
                (entry.X - ConcealerFirstX) % 8 == 0;
            if (alignedX && entry.Y == ConcealerY &&
                entry.TileNumber == ConcealerTile &&
                entry.Palette == ConcealerPalette &&
                entry.Priority == ConcealerPriority &&
                !entry.IsLarge)
            {
                int expectedIndex = (entry.X - ConcealerFirstX) / 8;
                if (seenEntries[expectedIndex])
                {
                    throw new InvalidDataException(
                        $"First visible Ceres frame duplicated concealer OBJ X={entry.X}.");
                }
                seenEntries[expectedIndex] = true;
            }
        }

        int matchingEntries = seenEntries.Count(seen => seen);
        if (matchingEntries != ConcealerEntryCount)
        {
            throw new InvalidDataException(
                $"First visible Ceres frame published {matchingEntries}/{ConcealerEntryCount} " +
                "stationary elevator-concealer OBJ entries.");
        }
    }

    private static void AssertNoOrangePhantom(ReadOnlySpan<Rgba32> pixels)
    {
        // Limit the test to the top lip covered by the concealer so unrelated warm colors
        // in the room, Samus, or HUD cannot satisfy the predicate.
        const int firstX = 100;
        const int lastX = 155;
        const int firstY = 96;
        const int lastY = 100;
        for (int y = firstY; y <= lastY; y++)
        {
            for (int x = firstX; x <= lastX; x++)
            {
                Rgba32 pixel = pixels[y * FrontendFrame.Width + x];
                if (pixel.R > pixel.B * 2 && pixel.R > pixel.G && pixel.G > pixel.B)
                {
                    throw new InvalidDataException(
                        $"Orange phantom elevator remained visible at ({x},{y}): " +
                        $"rgba=({pixel.R},{pixel.G},{pixel.B},{pixel.A}).");
                }
            }
        }
    }

    private static void AssertBlack(ReadOnlySpan<Rgba32> pixels, string description)
    {
        if (!IsBlack(pixels))
            throw new InvalidDataException($"Ceres {description} was not forced black.");
    }

    private static bool IsBlack(ReadOnlySpan<Rgba32> pixels)
    {
        foreach (Rgba32 pixel in pixels)
        {
            if (pixel.R != 0 || pixel.G != 0 || pixel.B != 0 || pixel.A != byte.MaxValue)
                return false;
        }
        return true;
    }
}
