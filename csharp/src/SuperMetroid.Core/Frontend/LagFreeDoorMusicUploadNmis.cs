namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The port's normal-play policy: the upload is instantaneous, so no NMI elapses during it.
/// </summary>
public sealed class LagFreeDoorMusicUploadNmis : IDoorMusicUploadNmiSource
{
    /// <summary>Shared stateless policy for normal lag-free play; no upload incurs an emulated NMI stall.</summary>
    public static LagFreeDoorMusicUploadNmis Instance { get; } = new();

    /// <summary>Restricts the stateless normal-play upload policy to its shared instance.</summary>
    private LagFreeDoorMusicUploadNmis() { }

    /// <summary>Reports zero accepted NMIs for every instantaneous music-data upload.</summary>
    /// <param name="musicDataIndex">Native music-data selector; this policy does not distinguish uploads.</param>
    /// <returns>Zero, leaving the door transition's native NMI counters unchanged by the upload.</returns>
    public int AcceptedNmisDuringUpload(byte musicDataIndex) => 0;
}
