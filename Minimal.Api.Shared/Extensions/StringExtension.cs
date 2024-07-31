using System.Security.Cryptography;

namespace Minimal.Api.Shared.Extensions;
public static class StringExtension
{
    public static string GenerateRandomString(int length)
    {
        string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*()_=-+";
        RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        var stringChars = new char[length];
        var byteBuffer = new byte[sizeof(uint)];

        for (int i = 0; i < length; i++)
        {
            Rng.GetBytes(byteBuffer);
            uint num = BitConverter.ToUInt32(byteBuffer, 0);
            stringChars[i] = Chars[(int)(num % (uint)Chars.Length)];
        }

        return (new string(stringChars)).Shuffle(10);
    }

    public static string Shuffle(this string str, int times = 0)
    {
        char[] array = str.ToCharArray();
        Random rng = new Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        if (times > 0)
            return Shuffle(new string(array), times - 1);

        return new string(array);
    }
}
