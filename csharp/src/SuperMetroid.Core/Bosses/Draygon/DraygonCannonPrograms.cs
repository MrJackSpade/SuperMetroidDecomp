namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Chooses Draygon's cannon programs by facing and tells which facing a running program belongs to.</summary>
internal static class DraygonCannonPrograms
{
    /// <summary>Whether <paramref name="program"/> belongs to the right-facing cannon.</summary>
    internal static bool FacesRight(DraygonCannonProgram program) =>
        DraygonCannonPlmProgramDefinitions.Orientations.TryGetValue(program, out DraygonCannonOrientation orientation)
            ? orientation == DraygonCannonOrientation.Right
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonProgram)} {program}.");

    /// <summary>The first program of an intact cannon facing <paramref name="orientation"/>.</summary>
    internal static DraygonCannonProgram StartProgram(DraygonCannonOrientation orientation) =>
        DraygonCannonPlmProgramDefinitions.StartPrograms.TryGetValue(orientation, out DraygonCannonProgram program)
            ? program
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonOrientation)} {orientation}.");

    /// <summary>The first program of a cannon facing <paramref name="orientation"/> that spawns already destroyed.</summary>
    internal static DraygonCannonProgram DestroyProgram(DraygonCannonOrientation orientation) =>
        DraygonCannonPlmProgramDefinitions.DestroyPrograms.TryGetValue(orientation, out DraygonCannonProgram program)
            ? program
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonOrientation)} {orientation}.");
}
