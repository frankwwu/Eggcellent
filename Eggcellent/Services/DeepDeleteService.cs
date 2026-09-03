using System.IO;
using System.Security.Cryptography;

namespace Eggcellent.Services
{
    public static class DeepDeleteService
    {
        /// <summary>
        /// Overwrites a file's contents with random data for the given number of passes,
        /// renames it to obscure the original name, then deletes it. Not a guarantee against
        /// forensic recovery on all storage types (notably SSDs with wear-leveling), but it
        /// matches standard "deep delete" behavior for secure file deletion.
        /// </summary>
        public static bool Shred(string filePath, int passes)
        {
            try
            {
                if (!File.Exists(filePath)) return false;

                long length = new FileInfo(filePath).Length;
                var attributes = File.GetAttributes(filePath);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);

                if (length > 0)
                {
                    using var rng = RandomNumberGenerator.Create();
                    var buffer = new byte[81920];

                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None);
                    for (int pass = 0; pass < passes; pass++)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        long remaining = length;
                        while (remaining > 0)
                        {
                            int chunk = (int)Math.Min(buffer.Length, remaining);
                            rng.GetBytes(buffer, 0, chunk);
                            stream.Write(buffer, 0, chunk);
                            remaining -= chunk;
                        }
                        stream.Flush(true);
                    }
                }

                // Rename to a random name before deleting, then delete.
                var directory = Path.GetDirectoryName(filePath) ?? "";
                var randomName = Path.Combine(directory, Path.GetRandomFileName());
                File.Move(filePath, randomName);
                File.Delete(randomName);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
