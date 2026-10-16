namespace SuperMetroid.Core.Hardware;

/// <summary>Which CPU-visible store a <see cref="CpuBusCell"/> lives in.</summary>
public enum CpuBusRegion
{
    /// <summary>Work RAM, banks $7E/$7F and their low-window system-bank mirrors.</summary>
    WorkRam,


    /// <summary>Cartridge ROM, read only by the asset importer.</summary>
    Cartridge,
}
