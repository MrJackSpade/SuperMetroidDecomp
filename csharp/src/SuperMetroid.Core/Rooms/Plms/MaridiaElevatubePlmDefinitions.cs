namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Bounded bank-$84 instruction and physical draw data for the hardcoded
/// elevatube PLM spawned by Maridia's door setup.
/// </summary>
internal static class MaridiaElevatubePlmDefinitions
{
    /// <summary><c>$84:9367</c>: the one-block physical elevatube draw list.</summary>
    internal const ushort DrawPointer = 0x9367;

    /// <summary>84:9369: solid block 180, shared with Kraid's physical draw. Block 180 is the one named visual scalar.</summary>
    internal const ushort PhysicalWord = 0x8180;

    /// <summary>Temporary artwork description for export; gameplay draws the single physical word directly.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Draw =>
        new(DrawPointer,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(1, new ushort[] { PhysicalWord }, new(0, 0)),
            });

    /// <summary>Stable asset identity assigned to the elevatube's one-block artwork.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>Exposes the sole elevatube draw list to artwork export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllDraws
    {
        get { yield return Draw; }
    }

    /// <summary>Maps the native elevatube draw pointer to its asset identity and rejects unrelated pointers.</summary>
    internal static string DrawVisualId(ushort pointer) => pointer == DrawPointer
        ? VisualId
        : throw new InvalidDataException(
            $"Maridia elevatube draw ${pointer:X4} has no visual ID.");

    /// <summary>Resolves the extracted elevatube artwork by its stable asset identity.</summary>
    internal static bool TryGetDrawByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId([Draw], _ => VisualId, id, out draw);
}
