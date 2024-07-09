using System;
using System.Text;

namespace LockStep.Network
{
    /// <summary>
    /// 网络二进制数据写入类
    /// Binary stream Writer. Supports simple types, buffers, arrays, structs, and nested types
    /// </summary>
    public class NetWriter
    {
        private NetBuffer m_Buffer;

        private const int k_MaxStringLength = 2048;
        private static byte[] s_StringWriteBuffer;
        private static Encoding s_Encoding;

        public NetWriter()
        {
            m_Buffer = new NetBuffer();
            if (s_Encoding == null)
            {
                s_Encoding = new UTF8Encoding();
                s_StringWriteBuffer = new byte[k_MaxStringLength];
            }
        }

        public NetWriter(NetBuffer buffer)
        {
            m_Buffer = buffer;
            if (s_Encoding == null)
            {
                s_Encoding = new UTF8Encoding();
                s_StringWriteBuffer = new byte[k_MaxStringLength];
            }
        }

        public NetWriter(byte[] buffer)
        {
            m_Buffer = new NetBuffer(buffer);
            if (s_Encoding == null)
            {
                s_Encoding = new UTF8Encoding();
                s_StringWriteBuffer = new byte[k_MaxStringLength];
            }
        }

        public uint Position => m_Buffer.Position;

        public int Capacity => m_Buffer.Capacity;

        public int BytesAvailable => m_Buffer.BytesAvailable;

        public byte[] ToArray()
        {
            return m_Buffer.ToArray();
        }

        //这里是整个数据，包括后面的空数组
        public byte[] AsArray()
        {
            return AsArraySegment().Array;
        }

        internal ArraySegment<byte> AsArraySegment()
        {
            return m_Buffer.AsArraySegment();
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki

        public void WritePackedUInt32(UInt32 value)
        {
            if (value <= 240)
            {
                Write((byte) value);
                return;
            }

            if (value <= 2287)
            {
                Write((byte) ((value - 240) / 256 + 241));
                Write((byte) ((value - 240) % 256));
                return;
            }

            if (value <= 67823)
            {
                Write((byte) 249);
                Write((byte) ((value - 2288) / 256));
                Write((byte) ((value - 2288) % 256));
                return;
            }

            if (value <= 16777215)
            {
                Write((byte) 250);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                return;
            }

            // all other values of uint
            Write((byte) 251);
            Write((byte) (value & 0xFF));
            Write((byte) ((value >> 8) & 0xFF));
            Write((byte) ((value >> 16) & 0xFF));
            Write((byte) ((value >> 24) & 0xFF));
        }

        public void WritePackedUInt64(UInt64 value)
        {
            if (value <= 240)
            {
                Write((byte) value);
                return;
            }

            if (value <= 2287)
            {
                Write((byte) ((value - 240) / 256 + 241));
                Write((byte) ((value - 240) % 256));
                return;
            }

            if (value <= 67823)
            {
                Write((byte) 249);
                Write((byte) ((value - 2288) / 256));
                Write((byte) ((value - 2288) % 256));
                return;
            }

            if (value <= 16777215)
            {
                Write((byte) 250);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                return;
            }

            if (value <= 4294967295)
            {
                Write((byte) 251);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                Write((byte) ((value >> 24) & 0xFF));
                return;
            }

            if (value <= 1099511627775)
            {
                Write((byte) 252);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                Write((byte) ((value >> 24) & 0xFF));
                Write((byte) ((value >> 32) & 0xFF));
                return;
            }

            if (value <= 281474976710655)
            {
                Write((byte) 253);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                Write((byte) ((value >> 24) & 0xFF));
                Write((byte) ((value >> 32) & 0xFF));
                Write((byte) ((value >> 40) & 0xFF));
                return;
            }

            if (value <= 72057594037927935)
            {
                Write((byte) 254);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                Write((byte) ((value >> 24) & 0xFF));
                Write((byte) ((value >> 32) & 0xFF));
                Write((byte) ((value >> 40) & 0xFF));
                Write((byte) ((value >> 48) & 0xFF));
                return;
            }

            // all others
            {
                Write((byte) 255);
                Write((byte) (value & 0xFF));
                Write((byte) ((value >> 8) & 0xFF));
                Write((byte) ((value >> 16) & 0xFF));
                Write((byte) ((value >> 24) & 0xFF));
                Write((byte) ((value >> 32) & 0xFF));
                Write((byte) ((value >> 40) & 0xFF));
                Write((byte) ((value >> 48) & 0xFF));
                Write((byte) ((value >> 56) & 0xFF));
            }
        }

        public void Write(char value)
        {
            m_Buffer.WriteByte((byte) value);
        }

        public void Write(byte value)
        {
            m_Buffer.WriteByte(value);
        }

        public void Write(sbyte value)
        {
            m_Buffer.WriteByte((byte) value);
        }

        public void Write(short value)
        {
            m_Buffer.WriteByte2((byte) ((value >> 8) & 0xff), (byte) (value & 0xff));
        }

        public void Write(ushort value)
        {
            m_Buffer.WriteByte2((byte) ((value >> 8) & 0xff), (byte) (value & 0xff));
        }

        public void Write(int value)
        {
            // little endian...
            m_Buffer.WriteByte4(
                (byte) ((value >> 24) & 0xff),
                (byte) ((value >> 16) & 0xff),
                (byte) ((value >> 8) & 0xff),
                (byte) (value & 0xff));
        }

        public void Write(uint value)
        {
            m_Buffer.WriteByte4(
                (byte) ((value >> 24) & 0xff),
                (byte) ((value >> 16) & 0xff),
                (byte) ((value >> 8) & 0xff),
                (byte) (value & 0xff));
        }

        public void Write(long value)
        {
            m_Buffer.WriteByte8(
                (byte) ((value >> 56) & 0xff),
                (byte) ((value >> 48) & 0xff),
                (byte) ((value >> 40) & 0xff),
                (byte) ((value >> 32) & 0xff),
                (byte) ((value >> 24) & 0xff),
                (byte) ((value >> 16) & 0xff),
                (byte) ((value >> 8) & 0xff),
                (byte) (value & 0xff));
        }

        public void Write(ulong value)
        {
            m_Buffer.WriteByte8(
                (byte) ((value >> 56) & 0xff),
                (byte) ((value >> 48) & 0xff),
                (byte) ((value >> 40) & 0xff),
                (byte) ((value >> 32) & 0xff),
                (byte) ((value >> 24) & 0xff),
                (byte) ((value >> 16) & 0xff),
                (byte) ((value >> 8) & 0xff),
                (byte) (value & 0xff));
        }

        public void Write(string value)
        {
            if (value == null)
            {
                m_Buffer.WriteByte2(0, 0);
                return;
            }

            int len = s_Encoding.GetByteCount(value);

            if (len >= k_MaxStringLength)
            {
                throw new IndexOutOfRangeException("Serialize(string) too long: " + value.Length);
            }

            Write((ushort) (len));
            int numBytes = s_Encoding.GetBytes(value, 0, value.Length, s_StringWriteBuffer, 0);
            m_Buffer.WriteBytes(s_StringWriteBuffer, (ushort) numBytes);
        }

        public void Write(bool value)
        {
            if (value)
                m_Buffer.WriteByte(1);
            else
                m_Buffer.WriteByte(0);
        }

        public void Write(byte[] buffer, int count)
        {
            m_Buffer.WriteBytes(buffer, (uint) count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            m_Buffer.WriteBytesAtOffset(buffer, (ushort) offset, (ushort) count);
        }

        public void WriteBytesAndSize(byte[] buffer, int count)
        {
            if (buffer == null || count == 0)
            {
                Write((UInt16) 0);
                return;
            }

            Write((UInt16) count);
            m_Buffer.WriteBytes(buffer, (UInt16) count);
        }

        //NOTE: this will write the entire buffer.. including trailing empty space!
        public void WriteBytesFull(byte[] buffer)
        {
            if (buffer == null)
            {
                Write((UInt16) 0);
                return;
            }

            Write((UInt16) buffer.Length);
            m_Buffer.WriteBytes(buffer, (UInt16) buffer.Length);
        }
        
        public void WriteBytesFullWithoutLen(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            m_Buffer.WriteBytes(buffer, (uint) buffer.Length);
        }


        public void SeekZero()
        {
            m_Buffer.SeekZero();
        }

        public void Seek(uint pos)
        {
            m_Buffer.Seek(pos);
        }

        /// <summary>
        /// 去掉已经读完的字节
        /// 将 Pos 设置成0
        /// </summary>
        public void Arrangement(int pos = -1)
        {
            m_Buffer.Arrangement(pos);
        }
    }
}
