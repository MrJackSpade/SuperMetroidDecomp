using SuperMetroid.Core.Input;

/// <summary>Exploratory input search only; candidates require independent cartridge replay.</summary>
internal static class BombTraversalSearch
{
    public static int Run(string rom, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        int candidates = 0;
        int freeCases = 0, freeCandidates = 0;
        foreach (bool ceiling in new[] { false, true })
        foreach (int interval in Enumerable.Range(24, 7).Concat(Enumerable.Range(48, 11)))
        for (int offset = 1; offset <= 8; offset++)
        foreach (int neutralBand in ceiling ? new[] { 3 } : Enumerable.Range(0, 6))
        foreach (int steeringStart in ceiling ? new[] { 170 } : new[] { 170, 175, 180, 185, 190, 195 })
        {
            if (!ceiling && interval is not (26 or 27 or 52 or 53 or 54)) continue;
            if (!ceiling) freeCases++;
            var runtime = BombTraversalFixture.Create(rom, ceiling);
            var samus = runtime.Samus!;
            int targetX = 128 + offset, launches = 0, floorReturns = 0, ceilingHits = 0, traversalLaunches = 0;
            var inputs = new List<ushort>();
            for (int frame = 0; frame < 600; frame++)
            {
                bool shoot = frame == 0 || frame >= 52 && (frame - 52) % interval == 0;
                if (shoot) targetX = samus.XPosition + (frame >= steeringStart ? offset : 0);
                // Aim beside the NEXT blast, not the newest bomb (which may still
                // have another half-cycle before it catches Samus). The dead band
                // avoids alternating left/right inertia overshoot around one pixel.
                // These reads choose controller input; they never alter game state.
                var nextBomb = runtime.BombProjectiles.Slots.Where(bomb => bomb.IsActive && bomb.BombTimer >= 9).OrderBy(bomb => bomb.BombTimer).FirstOrDefault();
                if (nextBomb is not null) targetX = nextBomb.XPosition + (frame >= steeringStart ? offset : 0);
                ushort input = shoot ? runtime.ControllerBindings.Shoot : (ushort)0;
                if (samus.XPosition < targetX - neutralBand) input |= (ushort)SnesButton.Right;
                if (samus.XPosition > targetX + neutralBand) input |= (ushort)SnesButton.Left;
                inputs.Add(input);
                runtime.StepFrame(input);
                if (runtime.LastBombJumpMovement is { Started: true })
                {
                    launches++;
                    if (frame > steeringStart) traversalLaunches++;
                }
                if (frame > steeringStart && samus.YPosition >= 249) floorReturns++;
                if (frame > steeringStart && (runtime.LastMorphBallMovement is { HitCeiling: true } || runtime.LastBombJumpMovement is { Vertical.Collided: true } && samus.YPosition == (ceiling ? 215 : 23))) ceilingHits++;
                // Stop rejected search branches before they leave the synthetic
                // room or regain floor support; do not turn an invalid room walk
                // into an accepted traversal or swallow a gameplay exception.
                if (!ceiling && (floorReturns != 0 || ceilingHits != 0 || samus.XPosition < 32 || samus.XPosition > 512)) break;
            }
            if (inputs.Count == 600 && launches >= 8 && traversalLaunches >= 6 && floorReturns == 0 && samus.XPosition > 160)
            {
                candidates++;
                if (!ceiling) freeCandidates++;
                string suffix = ceiling ? "" : $"-band-{neutralBand}-start-{steeringStart}";
                string path = Path.Combine(outputDirectory, $"bomb-traversal-ceiling-{ceiling}-interval-{interval}-offset-{offset}{suffix}.csv");
                using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                using var writer = new StreamWriter(stream);
                writer.WriteLine("frame,input");
                for (int frame = 0; frame < inputs.Count; frame++) writer.WriteLine($"{frame},{inputs[frame]:X4}");
                Console.WriteLine($"Candidate ceiling={ceiling} interval={interval} offset={offset}: x={samus.XPosition}, y={samus.YPosition}, launches={launches}, floor={floorReturns}, ceilingHits={ceilingHits}; {path}");
            }
        }
        Console.WriteLine($"Found {candidates} candidate sequences. These are not cartridge parity evidence.");
        Console.WriteLine($"Free-flight search: {freeCases} policies, {freeCandidates} sustained candidates; ceiling candidates do not satisfy that scope.");
        return candidates > 0 ? 0 : 1;
    }
}
