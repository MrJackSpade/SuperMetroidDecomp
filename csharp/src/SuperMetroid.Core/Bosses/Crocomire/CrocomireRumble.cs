namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Resolves a cursor into Crocomire's hidden-wall rumble schedule to the target it names.</summary>
internal static class CrocomireRumble
{
    /// <summary>Rejects the timing operands while resolving every native target cursor.</summary>
    internal static CrocomireRumbleDefinition AtOffset(int tableOffset)
    {
        var all = CrocomireRumbleDefinitions.All;
        if (tableOffset is 0 or 2) return all[tableOffset / 2];
        int terminatorOffset = CrocomireRumbleDefinitions.FirstPhaseOffset +
            CrocomireRumbleDefinitions.PhaseCount * CrocomireRumbleDefinitions.PhaseBytes;
        if (tableOffset == terminatorOffset || tableOffset == terminatorOffset + 2)
            return all[all.Length - 2 + (tableOffset - terminatorOffset) / 2];
        int offset = tableOffset - CrocomireRumbleDefinitions.FirstPhaseOffset;
        if (offset >= 0 && offset < CrocomireRumbleDefinitions.PhaseCount * CrocomireRumbleDefinitions.PhaseBytes &&
            offset % CrocomireRumbleDefinitions.PhaseBytes is 0 or 2)
            return all[2 + offset / CrocomireRumbleDefinitions.PhaseBytes * 2 + offset % CrocomireRumbleDefinitions.PhaseBytes / 2];
        throw new InvalidDataException("Crocomire rumble offset $" + tableOffset.ToString("X4") + " is not an authored target word.");
    }
}
