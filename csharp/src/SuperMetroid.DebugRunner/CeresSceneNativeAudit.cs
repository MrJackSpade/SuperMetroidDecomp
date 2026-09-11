using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Compares the live scene's phase timing and complete OAM population with original CPU output.</summary>
internal static class CeresSceneNativeAudit
{
    public static int Run(string romPath, string nativeCsv)
    {
        var scene = new CeresDestructionCinematicState(SuperMetroidAddressSpace.LoadRetailRom(romPath));
        var prepare = typeof(CeresDestructionCinematicState).GetMethod("PrepareRenderOam", BindingFlags.Instance | BindingFlags.NonPublic)!;
        int frames = 0, timingFailures = 0, drawFailures = 0, populationFailures = 0;
        foreach (string line in File.ReadLines(nativeCsv).Skip(1))
        {
            string[] row = line.Split(',');
            scene.Step();
            CeresDestructionPhase expectedPhase = (CeresSceneAuditFunction)int.Parse(row[1]) switch
            {
                CeresSceneAuditFunction.Wait => CeresDestructionPhase.WaitForMusicQueue,
                CeresSceneAuditFunction.Fade => CeresDestructionPhase.FadeInAndDrift,
                CeresSceneAuditFunction.Approach => CeresDestructionPhase.ApproachExplosion,
                CeresSceneAuditFunction.Departure => CeresDestructionPhase.FlyingAwayFromExplosion,
                CeresSceneAuditFunction.Hold => CeresDestructionPhase.HoldAfterExplosion,
                _ => throw new InvalidDataException($"Unexpected native Ceres phase {row[1]}.")
            };
            if (scene.Phase != expectedPhase || scene.Zoom != int.Parse(row[2]) || scene.BackgroundX != int.Parse(row[3]) ||
                scene.BackgroundY != int.Parse(row[4]) || scene.Brightness != int.Parse(row[5]))
            {
                if (timingFailures++ == 0) Console.WriteLine($"First phase/camera mismatch at {frames}: native {string.Join(',', row.Take(6))}; managed {scene.Zoom}/{scene.BackgroundX}/{scene.BackgroundY}/{scene.Brightness}.");
            }
            var oam = (OamBuffer)prepare.Invoke(scene, null)!;
            if (!Components(oam.LowTable[..(oam.LastFinalizedSpriteCount * 4)].ToArray(), oam.HighTable.ToArray())
                .SequenceEqual(Components(Convert.FromHexString(row[6]), Convert.FromHexString(row[7]))))
            {
                if (populationFailures++ == 0) Console.WriteLine($"First unordered component mismatch at {frames}.");
            }
            if (!oam.LowTable[..(oam.LastFinalizedSpriteCount * 4)].SequenceEqual(Convert.FromHexString(row[6])) ||
                !oam.HighTable.SequenceEqual(Convert.FromHexString(row[7])))
            {
                if (drawFailures++ == 0) Console.WriteLine($"First scene OAM mismatch at {frames}: managed sprites {oam.LastFinalizedSpriteCount}; native {row[6].Length / 8}.");
            }
            frames++;
        }
        Console.WriteLine($"Ceres scene: {frames} frames, {timingFailures} phase/camera mismatches, {drawFailures} OAM mismatches, {populationFailures} unordered population mismatches.");
        if (frames == 0 || timingFailures != 0 || drawFailures != 0)
            throw new InvalidDataException("Ceres scene differs from the original-CPU phase/population trace.");
        return 0;
    }

    private static IEnumerable<string> Components(byte[] low, byte[] high) =>
        Enumerable.Range(0, low.Length / 4).Select(index =>
            Convert.ToHexString(low.AsSpan(index * 4, 4)) + ((high[index / 4] >> (index % 4 * 2)) & 3))
            .OrderBy(value => value, StringComparer.Ordinal);
}

/// <summary>Native cinematic function identities present in the bounded Ceres scene trace.</summary>
internal enum CeresSceneAuditFunction
{
    /// <summary>$8B:C2E4, wait for queued cinematic music.</summary>
    Wait = 0xc2e4,
    /// <summary>$8B:C2F1, initial drift and fade-in.</summary>
    Fade = 0xc2f1,
    /// <summary>$8B:C345, station approach before the final explosion.</summary>
    Approach = 0xc345,
    /// <summary>$8B:C5CA, gunship flying away from the explosion.</summary>
    Departure = 0xc5ca,
    /// <summary>$8B:C610, hold after the gunship flight.</summary>
    Hold = 0xc610
}
