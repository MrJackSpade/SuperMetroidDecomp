namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What a Botwoon projectile frame depicts (#1869), each with only the facts that tell its frames apart.</summary>
internal abstract record BotwoonProjectilePicture
{
    /// <summary>Only the pictures below exist.</summary>
    private BotwoonProjectilePicture()
    {
    }

    /// <summary>One of a body link's four cycling frames.</summary>
    /// <param name="Facing">The direction the link faces.</param>
    /// <param name="Phase">The frame's place in the link's cycle.</param>
    internal sealed record BodyLink(CompassDirection Facing, int Phase) : BotwoonProjectilePicture;

    /// <summary>The tail cap.</summary>
    /// <param name="Facing">The direction the tail faces.</param>
    internal sealed record TailCap(CompassDirection Facing) : BotwoonProjectilePicture;

    /// <summary>The empty frame of a segment inside the wall.</summary>
    internal sealed record HiddenSegment : BotwoonProjectilePicture
    {
        /// <summary>The one hidden frame.</summary>
        internal static HiddenSegment Instance { get; } = new();
    }

    /// <summary>One of the spit's five cycling frames.</summary>
    /// <param name="Phase">The frame's place in the spit's cycle.</param>
    internal sealed record SpitFrame(int Phase) : BotwoonProjectilePicture;
}
