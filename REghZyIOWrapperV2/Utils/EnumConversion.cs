using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REghZyIOWrapperV2.Utils {
    /// <summary>
    /// A class for converting between enum and primitives (int/short/etc)
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>

    // original post is https://stackoverflow.com/a/52914567/11034928
    // i just added in the ToSByte, ToInt16, etc, methods for extra speed
    // atleast i hope it speeds up... its only like microseconds though so
    public static class EnumConversion<TEnum> where TEnum : unmanaged, Enum {
        public static sbyte ToSByte(TEnum value) {
            unsafe {
                sbyte v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static byte ToByte(TEnum value) {
            unsafe {
                byte v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static short ToInt16(TEnum value) {
            unsafe {
                short v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static ushort ToUInt16(TEnum value) {
            unsafe {
                ushort v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static int ToInt32(TEnum value) {
            unsafe {
                int v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static uint ToUInt32(TEnum value) {
            unsafe {
                uint v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static long ToInt64(TEnum value) {
            unsafe {
                long v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static ulong ToUInt64(TEnum value) {
            unsafe {
                ulong v = default;
                *((TEnum*) &v) = value;
                return v;
            }
        }

        public static TResult ToAny<TResult>(TEnum value) where TResult : unmanaged {
            unsafe {
                if (sizeof(TResult) > sizeof(TEnum)) {
                    TResult v = default;
                    *((TEnum*) &v) = value;
                    return v;
                }
                else {
                    return *(TResult*) &value;
                }
            }
        }

        public static TEnum FromSByte(sbyte value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromByte(byte value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromInt16(short value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromUInt16(ushort value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromInt32(int value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromUInt32(uint value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromInt64(long value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromUInt64(ulong value) { unsafe { return *(TEnum*) &value; } }
        public static TEnum FromAny<TSource>(TSource value) where TSource : unmanaged {
            unsafe {
                if (sizeof(TEnum) > sizeof(TSource)) {
                    TEnum o = default;
                    *((TSource*) &o) = value;
                    return o;
                }
                else {
                    return *(TEnum*) &value;
                }
            }
        }
    }
}
