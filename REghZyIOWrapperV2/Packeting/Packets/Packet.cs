using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using REghZyIOWrapperV2.Connections;
using REghZyIOWrapperV2.Streams;

namespace REghZyIOWrapperV2.Packeting.Packets {
    /// <summary>
    /// This packet class is similar to minecraft's packet system, where known packets dont
    /// require sending the length, but custom ones are (aka Packet250CustomPayload)
    /// </summary>
    public abstract class Packet {
        /// <summary>
        /// An array of packet creators (taking the metadat and data input) and returns a packet instance
        /// <para>Index is the packet ID</para>
        /// </summary>
        private static readonly Func<IDataInput, Packet>[] PacketCreators;

        /// <summary>
        /// A dictionary that maps the ID of a packet to the type of packet (e.g 0 = typeof(Packet0HardwareInfo))
        /// </summary>
        private static readonly Dictionary<byte, Type> PacketIdToType;

        /// <summary>
        /// A dictionary that maps the type of packet to its ID (eg typeof(Packet0HardwareInfo) = 0)
        /// </summary>
        private static readonly Dictionary<Type, byte> PacketTypeToId;

        private static readonly Dictionary<byte, ushort> PacketIdToSize;

        static Packet() {
            PacketCreators = new Func<IDataInput, Packet>[255];
            PacketIdToType = new Dictionary<byte, Type>();
            PacketTypeToId = new Dictionary<Type, byte>();
            PacketIdToSize = new Dictionary<byte, ushort>();
        }

        /// <summary>
        /// Creates a packet instance
        /// </summary>
        protected Packet() {

        }

        /// <summary>
        /// Runs a packet's static constructor. Useful if you register your packet creator in there.
        /// This method should usually be run before you start using packets, or packet systems
        /// (e.g during app startup), otherwise the packets wont be registered :-)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RunPacketCtor<T>() {
            RunPacketCtor(typeof(T));
        }

        /// <summary>
        /// Runs a packet's static constructor. Useful if you register your packet creator in there.
        /// This method should usually be run before you start using packets, or packet systems
        /// (e.g during app startup), otherwise the packets wont be registered :-)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RunPacketCtor(Type type) {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }

        /// <summary>
        /// Finds all types that use the <see cref="PacketImplementation"/> attribute, and inherit the <see cref="Packet"/> class
        /// </summary>
        /// <param name="allowAbstract"></param>
        /// <param name="checkInheritance"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindPacketImplementations(bool allowAbstract = true, bool checkInheritance = true) {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    if (type.GetCustomAttribute(typeof(PacketImplementation), checkInheritance) != null) {
                        if (type.IsAbstract && !allowAbstract) {
                            continue;
                        }

                        if (type.IsSubclassOf(typeof(Packet))) {
                            yield return type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes the custom data in this packet to the given <see cref="IDataOutput"/> (ID and Meta are written automatically, so this is optional)
        /// </summary>
        public abstract void Write(IDataOutput writer);

        /// <summary>
        /// Registers a packet type with the given ID (and optionally a creator for it).
        /// Usually, you register creators in a packet class' static constructor
        /// </summary>
        /// <param name="id">The packet ID is being registered</param>
        /// <param name="size">The size of the packet (0-65535 bytes)</param>
        /// <param name="creator">The function that creates the packet, or null if one isn't needed</param>
        /// <typeparam name="T">The type of packet that is being registered</typeparam>
        protected static void RegisterPacket<T>(byte id, Func<IDataInput, T> creator) where T : Packet {
            Type type = typeof(T);
            if (type == typeof(Packet)) {
                throw new Exception("Packet type cannot be the base abstract Packet type");
            }

            // should only really need to check the first or second one (not third, considering it can be null)
            // but eh :)
            if (PacketIdToType.ContainsKey(id) || PacketTypeToId.ContainsKey(type) || PacketCreators[id] != null) {
                throw new Exception($"The packet (ID {id}) has already been registered");
            }

            if (creator != null) {
                PacketCreators[id] = creator;
            }

            PacketIdToType[id] = type;
            PacketTypeToId[type] = id;
            // PacketIdToSize[id] = size;
        }

        /// <summary>
        /// Gets the ID of the specific packet type
        /// </summary>
        /// <typeparam name="T">The type of packet to get the ID of</typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if the given packet type was not registered</exception>
        public static byte GetPacketID<T>() where T : Packet {
            if (PacketTypeToId.TryGetValue(typeof(T), out byte id)) {
                return id;
            }

            throw new Exception($"A packet (of type {typeof(T).Name}) was not registered");
        }

        /// <summary>
        /// Gets the ID of the specific packet type (an alternative to the generic method)
        /// </summary>
        /// <param name="packet">The packet instance</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if the given packet type was not registered</exception>
        public static byte GetPacketID(Packet packet) {
            if (PacketTypeToId.TryGetValue(packet.GetType(), out byte id)) {
                return id;
            }

            throw new Exception($"A packet (of type {packet.GetType().Name}) was not registered");
        }

        /// <summary>
        /// Gets the packet type from the given ID
        /// </summary>
        /// <param name="id">The packet ID</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if the given ID has not been used/registered with</exception>
        public static Type GetPacketType(byte id) {
            if (PacketIdToType.TryGetValue(id, out Type type)) {
                return type;
            }

            throw new Exception($"The packet ID {id} was not registered");
        }

        public static ushort GetPacketSize(byte packetId) {
            if (PacketIdToSize.TryGetValue(packetId, out ushort size)) {
                return size;
            }

            throw new Exception($"The packet ID {packetId} was not registered");
        }

        public static bool IsPacketRegistered(byte id) {
            return PacketIdToType.ContainsKey(id);
        }

        /// <summary>
        /// Writes the given packet's ID, and then calls the given packet's <see cref="Packet.Write(IDataOutput)"/> method
        /// </summary>
        /// <param name="output">The data output to write the packet data</param>
        /// <param name="packet">The packet that is to be written</param>
        /// <exception cref="InvalidDataException">If the packet's ID or Meta was invalid (below 0 or above 99)</exception>
        public static void WritePacket(Packet packet, IDataOutput output) {
            if (packet == null) {
                throw new ArgumentNullException(nameof(packet));
            }

            output.WriteByte(GetPacketID(packet));
            packet.Write(output);
        }

        public static Packet ReadPacket(IDataInput input) {
            byte id = input.ReadByte();
            Func<IDataInput, Packet> creator = PacketCreators[id];
            if (creator == null) {
                throw new Exception($"Missing packet creator for id {id}");
            }

            try {
                return creator(input);
            }
            catch (Exception e) {
                throw new Exception("Failed to create packet from creator", e);
            }
        }

        public static Packet ReadPacket(byte id, IDataInput input) {
            Func<IDataInput, Packet> creator = PacketCreators[id];
            if (creator == null) {
                throw new Exception($"Missing packet creator for id {id}");
            }

            try {
                return creator(input);
            }
            catch (Exception e) {
                throw new Exception("Failed to create packet from creator", e);
            }
        }
    }
}