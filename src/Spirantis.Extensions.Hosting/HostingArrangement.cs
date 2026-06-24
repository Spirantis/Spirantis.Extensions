namespace Spirantis.Extensions.Hosting;

/// <summary>
/// The start/stop priority tier of an <see cref="IArrangeableHostedService"/>. Services start
/// in ascending order (<see cref="Superior"/> first) and stop in the reverse order.
/// </summary>
public enum HostingArrangement
{
    /// <summary>Starts before <see cref="Normal"/> services (and stops after them).</summary>
    Superior,

    /// <summary>The default tier.</summary>
    Normal,

    /// <summary>Starts after <see cref="Normal"/> services (and stops before them).</summary>
    Inferior,
}
