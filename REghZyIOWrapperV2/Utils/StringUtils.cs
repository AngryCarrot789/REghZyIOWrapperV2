namespace REghZyIOWrapperV2.Utils {
    public static class StringUtils {
        public static ushort ByteCount(this string value) {
            return (ushort) (value.Length * 2);
        }
    }
}
