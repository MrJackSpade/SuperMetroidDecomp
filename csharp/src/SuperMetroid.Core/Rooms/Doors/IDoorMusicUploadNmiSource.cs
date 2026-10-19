namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Supplies how many accepted NMIs the cartridge takes while <c>UploadToAPU</c> ($80:8028)
/// blocks a door transition's music-data upload.
/// </summary>
/// <remarks>
/// During a door transition the IRQ handler keeps requesting NMI, so every NMI that arrives
/// while the main loop is stalled in the upload is accepted: it advances <c>$05B5/$05B6</c>
/// and reads the controller, although no gameplay dispatch runs. The lag-free port spends
/// no updates on that stall, but those counters are gameplay state (their parity selects,
/// for example, the block-scan order of <c>$94:9763</c>).
/// </remarks>
public interface IDoorMusicUploadNmiSource
{
    /// <summary>Accepted NMIs elapsed while uploading music data set <paramref name="musicDataIndex"/>.</summary>
    /// <param name="musicDataIndex">Native room music-data selector identifying the upload whose accepted-NMI count is requested.</param>
    /// <returns>The nonnegative count to apply to native NMI counters, without running extra gameplay updates or delaying the port.</returns>
    int AcceptedNmisDuringUpload(int musicDataIndex);
}
