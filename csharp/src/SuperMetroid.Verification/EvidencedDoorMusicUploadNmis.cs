using SuperMetroid.Core.Frontend;

/// <summary>
/// Supplies a converted movie's evidenced accepted-NMI count for the door music upload of
/// the update being replayed. The count is timing evidence from the native input timeline,
/// like the wait's controller latch; no gameplay state is read from the reference.
/// </summary>
internal sealed class EvidencedDoorMusicUploadNmis : IDoorMusicUploadNmiSource
{
    private int update;
    private int expected;
    private bool consumed;

    /// <summary>Declares the native upload-wait NMIs that follow <paramref name="nextUpdate"/>.</summary>
    public void Expect(int nextUpdate, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        update = nextUpdate;
        expected = count;
        consumed = false;
    }

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
