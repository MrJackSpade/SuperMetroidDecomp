namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// X-ray's closing (#1793): once the beam's step reads Run released, the beam spends two more updates of frozen time
/// closing, and only the update after them gives Samus back her controls and the room its time. The beam's window and
/// the X-ray backdrop still show after the release and close once the first closing update is spent. The game spent the
/// two updates putting back the room pictures the X-ray had covered, work this simulation does not need, but the count
/// is gameplay-observable: each closing update is a whole update of frozen gameplay with its own random draw, and Samus
/// is released only once it is over.
/// </summary>
/// <remarks>
/// Started when the beam's step reads Run released, spent once by each of the beam's updates until it is over, after
/// which the beam's last step releases Samus; read by the frame's display for whether the beam's window and the X-ray
/// backdrop still show. A beam that is not closing holds the closing that is over. Evidence: the X-ray's beam states three
/// and four each put back half of what the X-ray covered and advance to the next state, one update apiece (each would
/// wait for room to send its half, which there always is then); state five releases Samus on the update after them.
/// </remarks>
[CompatibilityService("X-ray's beam spends two updates of frozen time closing after Run is released before Samus is released, and its window closes after the first of them")]
public readonly record struct XrayClosing
{
    /// <summary>The updates the closing lasts.</summary>
    public const int Updates = 2;

    /// <summary>A closing with <paramref name="updatesLeft"/> updates still to spend.</summary>
    private XrayClosing(int updatesLeft) => UpdatesLeft = updatesLeft;

    /// <summary>The closing Run's release starts, none of its updates spent yet.</summary>
    public static XrayClosing Started { get; } = new(Updates);

    /// <summary>The closing that is over: the beam's next step releases Samus.</summary>
    public static XrayClosing Over => default;

    /// <summary>The closing's updates still to spend, from <see cref="Updates"/> down to zero.</summary>
    public int UpdatesLeft { get; }

    /// <summary>Whether the closing is over.</summary>
    public bool IsOver => UpdatesLeft == 0;

    /// <summary>Whether the beam's window and the X-ray backdrop still show: until the closing's first update is spent.</summary>
    public bool ShowsBeam => UpdatesLeft == Updates;

    /// <summary>The closing after one more of its updates is spent.</summary>
    /// <exception cref="InvalidOperationException">The closing is already over.</exception>
    public XrayClosing Spend() => IsOver
        ? throw new InvalidOperationException("X-ray's closing is already over.")
        : new(UpdatesLeft - 1);
}
