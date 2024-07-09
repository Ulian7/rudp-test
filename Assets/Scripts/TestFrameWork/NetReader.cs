using System;
using System.Text;

namespace LockStep.Network
{
    /// <summary>
    /// 网络二进制数据读取器
    /// bigendian
    /// </summary>
    public class NetReader
    {
        protected NetBuffer m_buf;

        private const int k_MaxStringLength = 2048;
        private const int k_InitialStringBufferSize = 64;
        private static byte[] s_StringReaderBuffer;
        private static Encoding s_Encoding;

        public byte[] Bytes => m_buf.Bytes;
        public int Capacity => m_buf.Capacity;

        public NetReader()
        {
            m_buf = new NetBuffer();
            Initialize();
        }

        public NetReader(NetWriter writer)
        {
            m_buf = new NetBuffer(writer.ToArray());
            Initialize();
        }

        public NetReader(byte[] buffer)
        {
            m_buf = new NetBuffer(buffer);
            Initialize();
        }

        static void Initialize()
        {
            if (s_Encoding == null)
            {
                s_StringReaderBuffer = new byte[k_InitialStringBufferSize];
                s_Encoding = new UTF8Encoding();
            }
        }

        public uint Position => m_buf.Position;

        public void Read(NetWriter writer)
        {
            Replace(writer.ToArray());
            SeekZero();
        }

        public void SeekZero()
        {
            m_buf.SeekZero();
        }

        public void Seek(uint pos)
        {
            m_buf.Seek(pos);
        }

        public void Skip(int count)
        {
            m_buf.Skip(count);
        }

        internal void SeekBack(uint count)
        {
            m_buf.SeekBack(count);
        }

        public void Replace(byte[] buffer)
        {
            m_buf.Replace(buffer);
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki
        // NOTE: big endian.

        //这两个值是小端？
        public UInt32 ReadPackedUInt32()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return (UInt32) (240 + 256 * (a0 - 241) + a1);
            }

            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return (UInt32) (2288 + 256 * a1 + a2);
            }

            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt32) a2) << 8) + (((UInt32) a3) << 16);
            }

            byte a4 = ReadByte();
            if (a0 >= 251)
            {
                return a1 + (((UInt32) a2) << 8) + (((UInt32) a3) << 16) + (((UInt32) a4) << 24);
            }

            return 0;
            //throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + a0);
        }

        //这两个值是小端？
        public UInt64 ReadPackedUInt64()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + 256 * (a0 - ((UInt64) 241)) + a1;
            }

            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return 2288 + (((UInt64) 256) * a1) + a2;
            }

            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt64) a2) << 8) + (((UInt64) a3) << 16);
            }

            byte a4 = ReadByte();
            if (a0 == 251)
            {
                return a1 + (((UInt64) a2) << 8) + (((UInt64) a3) << 16) + (((UInt64) a4) << 24);
            }


            byte a5 = ReadByte();
            if (a0 == 252)
            {
                return a1 + (((UInt64) a2) << 8) + (((UInt64) a3) << 16) + (((UInt64) a4) << 24) +
                       (((UInt64) a5) << 32);
            }


            byte a6 = ReadByte();
            if (a0 == 253)
            {
                return a1 + (((UInt64) a2) << 8) + (((UInt64) a3) << 16) + (((UInt64) a4) << 24) +
                       (((UInt64) a5) << 32) + (((UInt64) a6) << 40);
            }


            byte a7 = ReadByte();
            if (a0 == 254)
            {
                return a1 + (((UInt64) a2) << 8) + (((UInt64) a3) << 16) + (((UInt64) a4) << 24) +
                       (((UInt64) a5) << 32) + (((UInt64) a6) << 40) + (((UInt64) a7) << 48);
            }


            byte a8 = ReadByte();
            if (a0 == 255)
            {
                return a1 + (((UInt64) a2) << 8) + (((UInt64) a3) << 16) + (((UInt64) a4) << 24) +
                       (((UInt64) a5) << 32) + (((UInt64) a6) << 40) + (((UInt64) a7) << 48) + (((UInt64) a8) << 56);
            }

            //throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);

            return 0;
        }

        public byte ReadByte()
        {
            return m_buf.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return (sbyte) m_buf.ReadByte();
        }

        public short ReadInt16()
        {
            ushort value = 0;
            value |= (ushort) (m_buf.ReadByte() << 8);
            value |= m_buf.ReadByte();
            return (short) value;
        }

        public ushort ReadUInt16()
        {
            ushort value = 0;
            value |= (ushort) (m_buf.ReadByte() << 8);
            value |= m_buf.ReadByte();
            return value;
        }

        public int ReadInt32()
        {
            uint value = 0;
            value |= (uint) (m_buf.ReadByte() << 24);
            value |= (uint) (m_buf.ReadByte() << 16);
            value |= (uint) (m_buf.ReadByte() << 8);
            value |= m_buf.ReadByte();
            return (int) value;
        }

        public uint ReadUInt32()
        {
            uint value = 0;
            value |= (uint) (m_buf.ReadByte() << 24);
            value |= (uint) (m_buf.ReadByte() << 16);
            value |= (uint) (m_buf.ReadByte() << 8);
            value |= m_buf.ReadByte();
            return value;
        }

        public long ReadInt64()
        {
            ulong value = 0;

            ulong other = ((ulong) m_buf.ReadByte()) << 56;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 48;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 40;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 32;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 24;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 16;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 8;
            value |= other;

            other = m_buf.ReadByte();
            value |= other;

            return (long) value;
        }

        public ulong ReadUInt64()
        {
            ulong value = 0;

            ulong other = ((ulong) m_buf.ReadByte()) << 56;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 48;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 40;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 32;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 24;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 16;
            value |= other;

            other = ((ulong) m_buf.ReadByte()) << 8;
            value |= other;

            other = m_buf.ReadByte();
            value |= other;

            return value;
        }

        public string ReadString()
        {
            UInt16 numBytes = ReadUInt16();
            if (numBytes == 0)
                return "";

            if (numBytes >= k_MaxStringLength)
            {
                //throw new IndexOutOfRangeException("ReadString() too long: " + numBytes);
                return null;
            }

            while (numBytes > s_StringReaderBuffer.Length)
            {
                s_StringReaderBuffer = new byte[s_StringReaderBuffer.Length * 2];
            }

            m_buf.ReadBytes(s_StringReaderBuffer, numBytes);

            char[] chars = s_Encoding.GetChars(s_StringReaderBuffer, 0, numBytes);
            return new string(chars);
        }

        public char ReadChar()
        {
            return (char) m_buf.ReadByte();
        }

        public bool ReadBoolean()
        {
            int value = m_buf.ReadByte();
            return value == 1;
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                //throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
                return null;
            }

            byte[] value = new byte[count];
            m_buf.ReadBytes(value, (uint) count);
            return value;
        }

        public byte[] ReadBytesAndSize()
        {
            ushort sz = ReadUInt16();
            if (sz == 0)
                return null;

            return ReadBytes(sz);
        }

        public override string ToString()
        {
            return m_buf.ToString();
        }

        /*public string ToHexString()
        {
            return m_buf.ToHexString();
        }*/

        /// <summary>
        /// 去掉已经读完的字节
        /// 将 Pos 设置成0
        /// </summary>
        public void Arrangement(int pos = -1)
        {
            m_buf.Arrangement(pos);
        }
    }
}
