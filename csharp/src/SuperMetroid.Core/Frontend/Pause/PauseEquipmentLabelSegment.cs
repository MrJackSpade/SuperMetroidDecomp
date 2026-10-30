namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>A part of an equipment label the cartridge can write on its own (#1467).</summary>
internal enum PauseEquipmentLabelSegment
{
    /// <summary>The label's first five cells, which a beam-column button write covers.</summary>
    Head,
    /// <summary>The remaining cells of a nine-cell label.</summary>
    Rest,
}
