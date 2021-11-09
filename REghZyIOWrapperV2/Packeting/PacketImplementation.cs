using System;

namespace REghZyIOWrapperV2.Packeting {
    /// <summary>
    /// This attribute is used to mark packet implementations, so that they can be located and fully loaded
    /// during app startup (because C# doesn't load every possible class available during startup, only when needed)
    /// <para>
    /// Marking a packet implementation with this will guarantee that it is loaded
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PacketImplementation : Attribute {

    }
}