namespace Pepersistence
{
    public static class XorEncryption
    {
        public static string EncryptDecrypt(string text, string key)
        {
            char[] data = text.ToCharArray();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (char)(data[i] ^ key[i % key.Length]);
            }
            return new string(data);
        }
    }
}