using SuperMetroid.Core.Frontend;

/// <summary>
/// Supplies a converted movie's evidenced accepted-NMI count for the door music upload of
/// the update being replayed. The count is timing evidence from the native input timeline,
/// like the wait's controller latch; no gameplay state is read from the reference.
/// </summary>
internal sealed class EvidencedDoorMusicUploadNmis : IDoorMusicUploadNmiSource
{
    /// <summary>Movie update whose native upload-wait evidence is currently armed.</summary>
    private int update;
    /// <summary>Accepted NMIs expected during that update's music upload, from the converted input timeline.</summary>
    private int expected;
    /// <summary>Whether the armed upload evidence has already been consumed.</summary>
    private bool consumed;

    /// <summary>Declares the native upload-wait NMIs that follow <paramref name="nextUpdate"/>.</summary>
    public void Expect(int nextUpdate, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        update = nextUpdate;
        expected = count;
        consumed = false;
    }

    /// <summary>Consumes the recorded NMI count for the current update's music upload exactly once.</summary>
    /// <param name="musicDataIndex">Native music data index being uploaded; used to identify unexpected uploads.</param>
    /// <returns>The accepted NMI count evidenced for the armed update.</returns>
    /// <exception cref="InvalidDataException">No upload wait was expected or the evidence was already consumed.</exception>
    public int AcceptedNmisDuringUpload(byte musicDataIndex)
    {
        if (expected == 0)
            throw new InvalidDataException(
                $"Update {update} uploaded music set ${musicDataIndex:X2}, but native recorded no upload wait.");
        if (consumed)
            throw new InvalidDataException($"Update {update} uploaded music data twice.");
        consumed = true;
        return expected;
    }

    /// <summary>Fails when native waited on an upload that the port did not perform.</summary>
    public void AssertSettled()
    {
        if (expected != 0 && !consumed)
            throw new InvalidDataException(
                $"Native update {update} waited {expected} NMIs on a music upload the port did not perform.");
    }
}
