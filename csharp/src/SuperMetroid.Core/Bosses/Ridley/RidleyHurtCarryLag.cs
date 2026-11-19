namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The lag a held Samus builds behind Lower Norfair Ridley's claw as he is hurt (#2561): each hurt update builds it by
/// <see cref="NorfairRidleyMotionDefinitions.HurtCarryLagRise"/> up to <see cref="NorfairRidleyMotionDefinitions.HurtCarryLag"/>,
/// and each ordinary update takes <see cref="NorfairRidleyMotionDefinitions.HurtCarryLagRecovery"/> off it, never below
/// none. Once it is full, a hurt update also carries a grabbed Samus.
/// </summary>
public sealed class RidleyHurtCarryLag
{
    /// <summary>The lag built, none through <see cref="NorfairRidleyMotionDefinitions.HurtCarryLag"/>.</summary>
    [IntegerQuantity("the hurt carry lag built")]
    public int Built { get; private set; }

    /// <summary>Whether the lag is full, so the hurt update carries a grabbed Samus.</summary>
    internal bool IsFull => Built >= NorfairRidleyMotionDefinitions.HurtCarryLag;

    /// <summary>Takes an ordinary update's recovery off the lag.</summary>
    internal void RecoverOnOrdinaryUpdate() => Built = Math.Max(0, Built - NorfairRidleyMotionDefinitions.HurtCarryLagRecovery);

    /// <summary>Builds the lag by a hurt update's rise, up to full.</summary>
    internal void BuildOnHurtUpdate() =>
        Built = Math.Min(NorfairRidleyMotionDefinitions.HurtCarryLag, Built + NorfairRidleyMotionDefinitions.HurtCarryLagRise);
}
