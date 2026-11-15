namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks how Samus's shot count drifts from the places her shots hold (#2051).</summary>
internal static class SamusShotCountVerification
{
    /// <summary>
    /// A fired shot counts one, a refused one is taken back, a special attack sets the count to its particles whatever was
    /// counted before, a deletion takes one off but leaves zero at zero, and clearing Samus's shots counts nothing.
    /// </summary>
    internal static void VerifySamusShotCount()
    {
        var count = new SamusShotCount();
        AssertEqual(0, count.Count, "a new pool counts no shots");
        count.CountShot();
        count.CountShot();
        AssertEqual(2, count.Count, "each fired shot counts one");
        count.TakeBackRefusedShot();
        AssertEqual(1, count.Count, "a refused shot is taken back");
        count.CountSpecialAttack(4);
        AssertEqual(4, count.Count, "a special attack sets the count to its particles");
        count.CountDeletion();
        AssertEqual(3, count.Count, "a deletion takes one off");
        count.Clear();
        count.CountDeletion();
        AssertEqual(0, count.Count, "a deletion at zero leaves zero");
        Console.WriteLine("Samus shot count: firing, refusal, a special attack, deletion at zero and clearing move the count as the game's does.");
    }
}
