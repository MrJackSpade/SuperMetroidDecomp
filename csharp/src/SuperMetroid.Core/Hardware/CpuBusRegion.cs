namespace SuperMetroid.Core.Hardware;

/// <summary>Which CPU-visible store a <see cref="CpuBusCell"/> lives in.</summary>
public enum CpuBusRegion
{
    /// <summary>Work RAM, banks $7E/$7F and their low-window system-bank mirrors.</summary>
    WorkRam,

    /// <summary>Battery-backed save RAM, banks $70-$7D/$F0-$FF lower window.</summary>
    SaveRam,

    /// <summary>Cartridge ROM, read only by the asset importer.</summary>
    Cartridge,
}
