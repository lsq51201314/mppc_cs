using System.Runtime.InteropServices;

namespace mppc_cs
{
    public class MppcDecompress
    {
        public static unsafe byte[]? Decompress(byte[] buffer, int dataSize)
        {
            void* ptr = NativeMemory.Alloc(16384u);
            void* ptr2 = NativeMemory.Alloc(102400u);
            Marshal.Copy(buffer, 0, (nint)ptr, buffer.Length);
            int sourceLen = buffer.Length;
            int num = dataSize;
            int num2 = ((num <= 8192) ? Uncompress((byte*)ptr2, &num, (byte*)ptr, sourceLen) : Uncompress2((byte*)ptr2, &num, (byte*)ptr, sourceLen));
            NativeMemory.Free(ptr);
            if (num2 == -1)
            {
                NativeMemory.Free(ptr2);
                return null;
            }
            byte[] array = new byte[num];
            Marshal.Copy((nint)ptr2, array, 0, array.Length);
            NativeMemory.Free(ptr2);
            return array;
        }
        public unsafe static int Uncompress(byte* dest, int* destLen, byte* source, int sourceLen)
        {
            int num = Mppc_decompress(source, dest, sourceLen, *destLen);
            if (num > 0 && num <= *destLen)
            {
                *destLen = num;
                return 0;
            }
            return -1;
        }
        public unsafe static int Uncompress2(byte* dest, int* destLen, byte* source, int sourceLen)
        {
            int num = *destLen;
            *destLen = 0;
            while (sourceLen > 2 && num > 0)
            {
                int num2 = *(ushort*)source & 0x7FFF;
                if (num2 <= 0 || num2 + 2 > sourceLen || num2 > 8192)
                {
                    return -1;
                }
                int num3;
                if ((*(ushort*)source & 0x8000) != 0)
                {
                    num3 = Mppc_decompress(source + 2, dest, num2, num);
                    if (num3 <= 0 || num3 > num || num3 > 8192)
                    {
                        return -1;
                    }
                }
                else
                {
                    num3 = num2;
                    if (num3 > num)
                    {
                        return -1;
                    }
                    Buffer.MemoryCopy(source + 2, dest, num3, num3);
                }
                source += num2 + 2;
                sourceLen -= num2 + 2;
                dest += num3;
                num -= num3;
                *destLen += num3;
            }
            if (sourceLen != 0)
            {
                return -1;
            }
            return 0;
        }
        private unsafe static int Mppc_decompress(byte* ibuf, byte* obuf, int isize, int osize)
        {
            if (isize > 9217)
            {
                return -1;
            }
            byte* ptr = (byte*)NativeMemory.Alloc(16384u, 1u);
            Memcpy(ptr, ibuf, isize);
            ibuf = ptr;
            byte* ptr2 = obuf;
            byte* ptr3 = obuf + osize;
            uint num = (uint)(isize * 8);
            uint l = 0u;
            uint blen = 7u;
            while (num > blen)
            {
                uint num2 = Fetch(ref ibuf, ref l);
                if (num2 < 2147483648u)
                {
                    if (obuf >= ptr3)
                    {
                        NativeMemory.Free(ptr);
                        return -1;
                    }
                    *(obuf++) = (byte)(num2 >> 24);
                    Passbits(8u, ref l, ref blen);
                    continue;
                }
                if (num2 < 3221225472u)
                {
                    if (obuf >= ptr3)
                    {
                        NativeMemory.Free(ptr);
                        return -1;
                    }
                    *(obuf++) = (byte)(((num2 >> 23) | 0x80) & 0xFF);
                    Passbits(9u, ref l, ref blen);
                    continue;
                }
                uint num3;
                uint num4;
                if (num2 >= 4026531840u)
                {
                    num3 = (num2 >> 22) & 0x3F;
                    num2 <<= 10;
                    if (num2 < 2147483648u)
                    {
                        num4 = 3u;
                        Passbits(11u, ref l, ref blen);
                    }
                    else if (num2 < 3221225472u)
                    {
                        num4 = 4 | ((num2 >> 28) & 3);
                        Passbits(14u, ref l, ref blen);
                    }
                    else if (num2 < 3758096384u)
                    {
                        num4 = 8 | ((num2 >> 26) & 7);
                        Passbits(16u, ref l, ref blen);
                    }
                    else if (num2 < 4026531840u)
                    {
                        num4 = 0x10 | ((num2 >> 24) & 0xF);
                        Passbits(18u, ref l, ref blen);
                    }
                    else if (num2 < 4160749568u)
                    {
                        num4 = 0x20 | ((num2 >> 22) & 0x1F);
                        Passbits(20u, ref l, ref blen);
                    }
                    else if (num2 < 4227858432u)
                    {
                        num4 = 0x40 | ((num2 >> 20) & 0x3F);
                        Passbits(22u, ref l, ref blen);
                    }
                    else if (num2 < 4261412864u)
                    {
                        num4 = 0x80 | ((num2 >> 18) & 0x7F);
                        Passbits(24u, ref l, ref blen);
                    }
                    else
                    {
                        Passbits(10u, ref l, ref blen);
                        num2 = Fetch(ref ibuf, ref l);
                        if (num2 < 4278190080u)
                        {
                            num4 = 0x100 | ((num2 >> 16) & 0xFF);
                            Passbits(16u, ref l, ref blen);
                        }
                        else if (num2 < 4286578688u)
                        {
                            num4 = 0x200 | ((num2 >> 14) & 0x1FF);
                            Passbits(18u, ref l, ref blen);
                        }
                        else if (num2 < 4290772992u)
                        {
                            num4 = 0x400 | ((num2 >> 12) & 0x3FF);
                            Passbits(20u, ref l, ref blen);
                        }
                        else if (num2 < 4292870144u)
                        {
                            num4 = 0x800 | ((num2 >> 10) & 0x7FF);
                            Passbits(22u, ref l, ref blen);
                        }
                        else
                        {
                            if (num2 >= 4293918720u)
                            {
                                NativeMemory.Free(ptr);
                                return -1;
                            }
                            num4 = 0x1000 | ((num2 >> 8) & 0xFFF);
                            Passbits(24u, ref l, ref blen);
                        }
                    }
                }
                else if (num2 >= 3758096384u)
                {
                    num3 = ((num2 >> 20) & 0xFF) + 64;
                    num2 <<= 12;
                    if (num2 < 2147483648u)
                    {
                        num4 = 3u;
                        Passbits(13u, ref l, ref blen);
                    }
                    else if (num2 < 3221225472u)
                    {
                        num4 = 4 | ((num2 >> 28) & 3);
                        Passbits(16u, ref l, ref blen);
                    }
                    else if (num2 < 3758096384u)
                    {
                        num4 = 8 | ((num2 >> 26) & 7);
                        Passbits(18u, ref l, ref blen);
                    }
                    else if (num2 < 4026531840u)
                    {
                        num4 = 0x10 | ((num2 >> 24) & 0xF);
                        Passbits(20u, ref l, ref blen);
                    }
                    else if (num2 < 4160749568u)
                    {
                        num4 = 0x20 | ((num2 >> 22) & 0x1F);
                        Passbits(22u, ref l, ref blen);
                    }
                    else if (num2 < 4227858432u)
                    {
                        num4 = 0x40 | ((num2 >> 20) & 0x3F);
                        Passbits(24u, ref l, ref blen);
                    }
                    else
                    {
                        Passbits(12u, ref l, ref blen);
                        num2 = Fetch(ref ibuf, ref l);
                        if (num2 < 4261412864u)
                        {
                            num4 = 0x80 | ((num2 >> 18) & 0x7F);
                            Passbits(14u, ref l, ref blen);
                        }
                        else if (num2 < 4278190080u)
                        {
                            num4 = 0x100 | ((num2 >> 16) & 0xFF);
                            Passbits(16u, ref l, ref blen);
                        }
                        else if (num2 < 4286578688u)
                        {
                            num4 = 0x200 | ((num2 >> 14) & 0x1FF);
                            Passbits(18u, ref l, ref blen);
                        }
                        else if (num2 < 4290772992u)
                        {
                            num4 = 0x400 | ((num2 >> 12) & 0x3FF);
                            Passbits(20u, ref l, ref blen);
                        }
                        else if (num2 < 4292870144u)
                        {
                            num4 = 0x800 | ((num2 >> 10) & 0x7FF);
                            Passbits(22u, ref l, ref blen);
                        }
                        else
                        {
                            if (num2 >= 4293918720u)
                            {
                                NativeMemory.Free(ptr);
                                return -1;
                            }
                            num4 = 0x1000 | ((num2 >> 8) & 0xFFF);
                            Passbits(24u, ref l, ref blen);
                        }
                    }
                }
                else
                {
                    num3 = ((num2 >> 16) & 0x1FFF) + 320;
                    num2 <<= 16;
                    if (num2 < 2147483648u)
                    {
                        num4 = 3u;
                        Passbits(17u, ref l, ref blen);
                    }
                    else if (num2 < 3221225472u)
                    {
                        num4 = 4 | ((num2 >> 28) & 3);
                        Passbits(20u, ref l, ref blen);
                    }
                    else if (num2 < 3758096384u)
                    {
                        num4 = 8 | ((num2 >> 26) & 7);
                        Passbits(22u, ref l, ref blen);
                    }
                    else if (num2 < 4026531840u)
                    {
                        num4 = 0x10 | ((num2 >> 24) & 0xF);
                        Passbits(24u, ref l, ref blen);
                    }
                    else
                    {
                        Passbits(16u, ref l, ref blen);
                        num2 = Fetch(ref ibuf, ref l);
                        if (num2 < 4160749568u)
                        {
                            num4 = 0x20 | ((num2 >> 22) & 0x1F);
                            Passbits(10u, ref l, ref blen);
                        }
                        else if (num2 < 4227858432u)
                        {
                            num4 = 0x40 | ((num2 >> 20) & 0x3F);
                            Passbits(12u, ref l, ref blen);
                        }
                        else if (num2 < 4261412864u)
                        {
                            num4 = 0x80 | ((num2 >> 18) & 0x7F);
                            Passbits(14u, ref l, ref blen);
                        }
                        else if (num2 < 4278190080u)
                        {
                            num4 = 0x100 | ((num2 >> 16) & 0xFF);
                            Passbits(16u, ref l, ref blen);
                        }
                        else if (num2 < 4286578688u)
                        {
                            num4 = 0x200 | ((num2 >> 14) & 0x1FF);
                            Passbits(18u, ref l, ref blen);
                        }
                        else if (num2 < 4290772992u)
                        {
                            num4 = 0x400 | ((num2 >> 12) & 0x3FF);
                            Passbits(20u, ref l, ref blen);
                        }
                        else if (num2 < 4292870144u)
                        {
                            num4 = 0x800 | ((num2 >> 10) & 0x7FF);
                            Passbits(22u, ref l, ref blen);
                        }
                        else
                        {
                            if (num2 >= 4293918720u)
                            {
                                NativeMemory.Free(ptr);
                                return -1;
                            }
                            num4 = 0x1000 | ((num2 >> 8) & 0xFFF);
                            Passbits(24u, ref l, ref blen);
                        }
                    }
                }
                if (obuf - num3 < ptr2 || obuf + num4 > ptr3)
                {
                    NativeMemory.Free(ptr);
                    return -1;
                }
                Lamecopy(obuf, obuf - num3, num4);
                obuf += num4;
            }
            NativeMemory.Free(ptr);
            return (int)(obuf - ptr2);
        }
        private static void Passbits(uint n, ref uint l, ref uint blen)
        {
            l += n;
            blen += n;
        }
        private unsafe static uint Fetch(ref byte* buf, ref uint l)
        {
            buf += l >> 3;
            l &= 7u;
            return Byteorder_32(*(uint*)buf) << (int)l;
        }
        private unsafe static void Lamecopy(byte* dst, byte* src, uint len)
        {
            if (dst - src > 3)
            {
                while (len > 3)
                {
                    *(int*)dst = *(int*)src;
                    dst += 4;
                    src += 4;
                    len -= 4;
                }
            }
            while (len-- != 0)
            {
                *(dst++) = *(src++);
            }
        }
        private unsafe static void Memcpy(void* dst, void* src, int count)
        {
            byte[] array = new byte[4096];
            byte* ptr = (byte*)dst;
            byte* ptr2 = (byte*)src;
            int num = 0;
            while (num < count)
            {
                int num2 = count - num;
                if (num2 > 4096)
                {
                    num2 = 4096;
                }
                Marshal.Copy(new IntPtr(ptr2), array, 0, num2);
                Marshal.Copy(array, 0, new IntPtr(ptr), num2);
                num += num2;
                ptr += num2;
                ptr2 += num2;
            }
        }
        private static uint Byteorder_32(uint x)
        {
            return ((x & 0xFF) << 24) + (((x >> 8) & 0xFF) << 16) + (((x >> 16) & 0xFF) << 8) + ((x >> 24) & 0xFF);
        }
    }
}