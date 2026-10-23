using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>How a session turns a gameplay update into the published display.</summary>
internal interface IGameplayFramePublication
{
    /// <summary>Returns the display that follows a gameplay update.</summary>
    /// <param name="runtime">The gameplay runtime that just advanced.</param>
    /// <param name="current">The display currently published.</param>
    /// <param name="nextIdentity">Takes the identity for a newly published frame.</param>
    DisplayPacket Publish(SuperMetroidRuntime runtime, DisplayPacket current, Func<RenderFrameIdentity> nextIdentity);
}
