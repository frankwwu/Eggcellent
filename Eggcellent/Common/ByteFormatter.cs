namespace Eggcellent.Common
{
    public static class ByteFormatter
    {
        public static string Format(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }
            return bytes <= 0 ? "0 B" : $"{size:0.##} {units[unit]}";
        }
    }
}
