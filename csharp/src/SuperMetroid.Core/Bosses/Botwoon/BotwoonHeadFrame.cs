namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A drawn head pose; <see cref="Visible"/> false is the empty frame of the hidden head.</summary>
[IndependentFlag("independent facts of one head frame: whether it is drawn and whether the mouth is open")]
internal readonly record struct BotwoonHeadFrame(bool Visible, BotwoonFacing Facing, bool MouthOpen)
{
    /// <summary>The empty frame.</summary>
    internal static BotwoonHeadFrame Hidden => new(false, BotwoonFacing.Up, false);

    /// <summary>The closed mouth facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadFrame Closed(BotwoonFacing facing) => new(true, facing, false);

    /// <summary>The open mouth facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadFrame Open(BotwoonFacing facing) => new(true, facing, true);
}
