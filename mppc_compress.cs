namespace mppc_cs
{
    public class MppcCompress
    {
        public static byte[] Compress(byte[] buffer)
        {
            byte[] array = new byte[16384];
            int num = buffer.Length;
            int destLen = (int)CompressBound((uint)num);
            if (!((num > 8192) ? (Compress2(array, ref destLen, buffer, num) == 0) : (Compress(array, ref destLen, buffer, num) == 0)))
            {
                return buffer;
            }
            return [.. array.Take(destLen)];
        }
        public static uint CompressBound(uint sourcelen)
        {
            return sourcelen * 9 / 8 + 1 + 2 + 3;
        }
        public static int Compress(byte[] dest, ref int destLen, byte[] source, int sourceLen)
        {
            int num = Mppc_compress(source, dest, sourceLen);
            if (num > 0 && num <= destLen)
            {
                destLen = num;
                return 0;
            }
            return -1;
        }
        public static int Compress2(byte[] dest, ref int destLen, byte[] source, int sourceLen)
        {
            int num = destLen;
            destLen = 0;
            uint num2 = 0u;
            uint num3 = 0u;
            while (sourceLen > 0 && num > 2)
            {
                int num4 = ((sourceLen > 8192) ? 8192 : sourceLen);
                int num5 = Mppc_compress(source, dest, num4, num2, num3 + 2);
                if (num5 > 0 && num5 < num4 && num5 <= num - 2)
                {
                    BitConverter.GetBytes((ushort)(num5 | 0x8000)).CopyTo(dest, num3);
                }
                else
                {
                    if (num4 <= 0 || num4 > num - 2)
                    {
                        return -1;
                    }
                    num5 = num4;
                    Array.Copy(source, num2, dest, num3 + 2, num5);
                    BitConverter.GetBytes((ushort)num5).CopyTo(dest, num3);
                }
                num2 += (uint)num4;
                sourceLen -= num4;
                num3 += (uint)(num5 + 2);
                num -= num5 + 2;
                destLen += num5 + 2;
            }
            if (sourceLen != 0)
            {
                return -1;
            }
            return 0;
        }
        private static void Putbits(byte[] buf, uint val, uint n, ref uint l, ref uint addr_buf)
        {
            l += n;
            BitConverter.GetBytes(Byteorder_32(val << (int)(32 - l)) | buf[addr_buf]).CopyTo(buf, addr_buf);
            addr_buf += l >> 3;
            l &= 7u;
        }
        private static void Putlit(byte[] buf, uint c, ref uint l, ref uint addr_buf)
        {
            if (c < 128)
            {
                Putbits(buf, c, 8u, ref l, ref addr_buf);
            }
            else
            {
                Putbits(buf, (c & 0x7F) | 0x100, 9u, ref l, ref addr_buf);
            }
        }
        private static void Putoff(byte[] buf, uint off, ref uint l, ref uint addr_buf)
        {
            if (off < 64)
            {
                Putbits(buf, 0x3C0 | off, 10u, ref l, ref addr_buf);
            }
            else if (off < 320)
            {
                Putbits(buf, 0xE00 | (off - 64), 12u, ref l, ref addr_buf);
            }
            else
            {
                Putbits(buf, 0xC000 | (off - 320), 16u, ref l, ref addr_buf);
            }
        }
        private static int Mppc_compress(byte[] ibuf, byte[] obuf, int isize, uint ptr_ibuf = 0u, uint ptr_obuf = 0u)
        {
            Dictionary<ushort, int> dictionary = [];
            uint num = ptr_obuf;
            uint num2 = ptr_ibuf;
            uint num3 = (uint)(ptr_ibuf + isize);
            uint num4 = ptr_ibuf;
            obuf[ptr_obuf] = 0;
            uint l = 0u;
            while (num3 - num4 > 2)
            {
                ushort key = BitConverter.ToUInt16(ibuf, (int)num4);
                if (!dictionary.TryGetValue(key, out int num5))
                {
                    num5 = -1;
                    dictionary.Add(key, num5);
                }

                dictionary[key] = (int)num4;
                if (num5 < num2)
                {
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    num4 = ptr_ibuf;
                    continue;
                }
                if (num5 >= num4)
                {
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    num4 = ptr_ibuf;
                    continue;
                }
                if (BitConverter.ToUInt16(ibuf, num5) != BitConverter.ToUInt16(ibuf, (int)num4++))
                {
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    continue;
                }
                if (ibuf[num5 += 2] != ibuf[++num4])
                {
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    num4 = ptr_ibuf;
                    continue;
                }
                num5++;
                for (num4++; num4 < num3 && ibuf[num5] == ibuf[num4]; num4++)
                {
                    num5++;
                }
                uint num6 = num4 - ptr_ibuf;
                ptr_ibuf = num4;
                Putoff(obuf, (uint)(num4 - num5), ref l, ref ptr_obuf);
                if (num6 < 4)
                {
                    Putbits(obuf, 0u, 1u, ref l, ref ptr_obuf);
                }
                else if (num6 < 8)
                {
                    Putbits(obuf, 8 | (num6 & 3), 4u, ref l, ref ptr_obuf);
                }
                else if (num6 < 16)
                {
                    Putbits(obuf, 0x30 | (num6 & 7), 6u, ref l, ref ptr_obuf);
                }
                else if (num6 < 32)
                {
                    Putbits(obuf, 0xE0 | (num6 & 0xF), 8u, ref l, ref ptr_obuf);
                }
                else if (num6 < 64)
                {
                    Putbits(obuf, 0x3C0 | (num6 & 0x1F), 10u, ref l, ref ptr_obuf);
                }
                else if (num6 < 128)
                {
                    Putbits(obuf, 0xF80 | (num6 & 0x3F), 12u, ref l, ref ptr_obuf);
                }
                else if (num6 < 256)
                {
                    Putbits(obuf, 0x3F00 | (num6 & 0x7F), 14u, ref l, ref ptr_obuf);
                }
                else if (num6 < 512)
                {
                    Putbits(obuf, 0xFE00 | (num6 & 0xFF), 16u, ref l, ref ptr_obuf);
                }
                else if (num6 < 1024)
                {
                    Putbits(obuf, 0x3FC00 | (num6 & 0x1FF), 18u, ref l, ref ptr_obuf);
                }
                else if (num6 < 2048)
                {
                    Putbits(obuf, 0xFF800 | (num6 & 0x3FF), 20u, ref l, ref ptr_obuf);
                }
                else if (num6 < 4096)
                {
                    Putbits(obuf, 0x3FF000 | (num6 & 0x7FF), 22u, ref l, ref ptr_obuf);
                }
                else if (num6 < 8192)
                {
                    Putbits(obuf, 0xFFE000 | (num6 & 0xFFF), 24u, ref l, ref ptr_obuf);
                }
            }
            switch (num3 - num4)
            {
                case 2u:
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    break;
                case 1u:
                    Putlit(obuf, ibuf[ptr_ibuf++], ref l, ref ptr_obuf);
                    break;
            }
            if (l != 0)
            {
                Putbits(obuf, 0u, 8 - l, ref l, ref ptr_obuf);
            }
            return (int)(ptr_obuf - num);
        }
        private static uint Byteorder_32(uint x)
        {
            return ((x & 0xFF) << 24) + (((x >> 8) & 0xFF) << 16) + (((x >> 16) & 0xFF) << 8) + ((x >> 24) & 0xFF);
        }
    }
}
