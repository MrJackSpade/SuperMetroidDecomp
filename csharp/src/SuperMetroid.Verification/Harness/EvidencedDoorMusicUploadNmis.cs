using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Supplies a converted movie's evidenced accepted-NMI count for the door music upload of
/// the update being replayed. The count is timing evidence from the native input timeline,
/// like the wait's controller latch; no gameplay state is read from the reference.
/// </summary>
internal sealed class EvidencedDoorMusicUploadNmis : IDoorMusicUploadNmiSource
{
    /// <summary>Gameplay update whose following upload wait has native timing evidence.</summary>
    private int update;
    /// <summary>Accepted NMIs supplied by the converted movie for that upload wait.</summary>
    private int expected;
    /// <summary>Prevents one update's evidence from satisfying two upload calls.</summary>
    private bool consumed;

    /// <summary>Declares the native upload-wait NMIs that follow <paramref name="nextUpdate"/>.</summary>
    public void Expect(int nextUpdate, int count)
    {
        Ensure.AtLeastZero(count);
        update = nextUpdate;
        expected = count;
        consumed = false;
    }

    /// <summary>Consumes the evidenced accepted-NMI count once, rejecting an unexpected or duplicate upload.</summary>
    public int AcceptedNmisDuringUpload(int musicDataIndex)
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
