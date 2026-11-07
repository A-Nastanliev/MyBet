using System.Security.Cryptography;

namespace MyBet.Application.Import
{
    public static class FileHash
    {
        public static string Compute(byte[] content) => Convert.ToHexString(SHA256.HashData(content));
    }
}
