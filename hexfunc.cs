namespace mppc_cs
{
    public static class HEXFunc
    {
        public static byte[] ToBytesFromHexString(this string hexString)
        {
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("16进制字符串长度必须是偶数。");

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
  