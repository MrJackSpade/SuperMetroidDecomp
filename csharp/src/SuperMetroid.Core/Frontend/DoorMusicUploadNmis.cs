namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Supplies how many accepted NMIs the cartridge takes while <c>SendAPUData</c> ($80:8028)
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
    int AcceptedNmisDuringUpload(byte musicDataIndex);
}

/// <summary>
/// The port's normal-play policy: the upload is instantaneous, so no NMI elapses during it.
/// </summary>
public sealed class LagFreeDoorMusicUploadNmis : IDoorMusicUploadNmiSource
{
    public static LagFreeDoorMusicUploadNmis Instance { get; } = new();

    private LagFreeDoorMusicUploadNmis() { }

    public int AcceptedNmisDuringUpload(byte musicDataIndex) => 0;
}
