namespace Utils {
    public static class StringUtility {
        public static string ToKFormat(float f) {
            return (f / 1000).ToString("0") + "K";
        }
    }
}