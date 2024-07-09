using System;

namespace LockStep
{
    /// <summary>
    /// 网络数据缓冲类
    ///
    /// from unitynetwork
    /// A growable buffer class used by NetReader and NetWriter.
    /// this is used instead of MemoryStream and BinaryReader/BinaryWriter to avoid allocations.
    /// </summary>
    public class NetBuffer
    {
        private byte[] m_Buffer;
        private uint m_Pos;
        private const int k_InitialSize = 4; //默认长度
        private const float k_GrowthFactor = 1.5f;
        private const int k_BufferSizeWarning = 1024 * 1024 * 128;

        public byte[] Bytes => m_Buffer;

        public uint Position => m_Pos;

        public int Capacity => m_Buffer.Length;

        public int BytesAvailable => (int) (Capacity - m_Pos);

        public NetBuffer() : this(k_InitialSize)
        {
        }

        public NetBuffer(int size) : this(new byte[size])
        {
        }

        // this does NOT copy the buffer
        public NetBuffer(byte[] buffer)
        {
            m_Buffer = buffer;
        }

        public byte ReadByte()
        {
            if (m_Pos >= m_Buffer.Length)
            {
                //throw new IndexOutOfRangeException("NetReader:ReadByte out of range:" + ToString());
                m_Pos++;
                return 0;
            }

            return m_Buffer[m_Pos++];
        }

        public void ReadBytes(byte[] buffer, uint count)
        {
            if (m_Pos + count > m_Buffer.Length)
            {
                //throw new IndexOutOfRangeException("NetReader:ReadBytes out of range: (" + count + ") " + ToString());
            }

            for (uint i = 0; i < count; i++)
            {
                buffer[i] = m_Buffer[m_Pos + i];
            }

            m_Pos += count;
        }

        public void ReadChars(char[] buffer, uint count)
        {
            if (m_Pos + count > m_Buffer.Length)
            {
                //throw new IndexOutOfRangeException("NetReader:ReadChars out of range: (" + count + ") " + ToString());
            }

            for (uint i = 0; i < count; i++)
            {
                buffer[i] = (char) m_Buffer[m_Pos + i];
            }

            m_Pos += count;
        }
        
        internal ArraySegment<byte> AsArraySegment()
        {
            return new ArraySegment<byte>(m_Buffer, 0, (int) m_Pos);
        }

        internal byte[] ToArray()
        {
            var newArray = new byte[m_Pos];
            Array.Copy(m_Buffer, newArray, (int) m_Pos);
            return newArray;
        }

        public void WriteByte(byte value)
        {
            WriteCheckForSpace(1);
            m_Buffer[m_Pos] = value;
            m_Pos += 1;
        }

        public void WriteByte2(byte value0, byte value1)
        {
            WriteCheckForSpace(2);
            m_Buffer[m_Pos] = value0;
            m_Buffer[m_Pos + 1] = value1;
            m_Pos += 2;
        }

        public void WriteByte4(byte value0, byte value1, byte value2, byte value3)
        {
            WriteCheckForSpace(4);
            m_Buffer[m_Pos] = value0;
            m_Buffer[m_Pos + 1] = value1;
            m_Buffer[m_Pos + 2] = value2;
            m_Buffer[m_Pos + 3] = value3;
            m_Pos += 4;
        }

        public void WriteByte8(byte value0, byte value1, byte value2, byte value3, byte value4, byte value5,
            byte value6, byte value7)
        {
            WriteCheckForSpace(8);
            m_Buffer[m_Pos] = value0;
            m_Buffer[m_Pos + 1] = value1;
            m_Buffer[m_Pos + 2] = value2;
            m_Buffer[m_Pos + 3] = value3;
            m_Buffer[m_Pos + 4] = value4;
            m_Buffer[m_Pos + 5] = value5;
            m_Buffer[m_Pos + 6] = value6;
            m_Buffer[m_Pos + 7] = value7;
            m_Pos += 8;
        }

        // every other Write() function in this class writes implicitly at the end-marker m_Pos.
        // this is the only Write() function that writes to a specific location within the buffer
        public void WriteBytesAtOffset(byte[] buffer, uint targetOffset, uint count)
        {
            uint newEnd = count + targetOffset;

            WriteCheckForSpace(newEnd);

            if (targetOffset == 0 && count == buffer.Length)
            {
                buffer.CopyTo(m_Buffer, m_Pos);
            }
            else
            {
                //CopyTo doesnt take a count :(
                for (int i = 0; i < count; i++)
                {
                    m_Buffer[targetOffset + i] = buffer[i];
                }
            }

            // although this writes within the buffer, it could move the end-marker
            if (newEnd > m_Pos)
            {
                m_Pos = newEnd;
            }
        }

        public void WriteBytes(byte[] buffer, uint count)
        {
            WriteCheckForSpace(count);

            if (count == buffer.Length)
            {
                buffer.CopyTo(m_Buffer, m_Pos);
            }
            else
            {
                //CopyTo doesnt take a count :(
                for (int i = 0; i < count; i++)
                {
                    m_Buffer[m_Pos + i] = buffer[i];
                }
            }

            m_Pos += count;
        }

        void WriteCheckForSpace(uint count)
        {
            if (m_Pos + count < m_Buffer.Length)
                return;

            int newLen = (int) (m_Buffer.Length * k_GrowthFactor);
            while (m_Pos + count >= newLen)
            {
                newLen = (int) (newLen * k_GrowthFactor);
            }

            // only do the copy once, even if newLen is increased multiple times
            byte[] tmp = new byte[newLen];
            m_Buffer.CopyTo(tmp, 0);
            m_Buffer = tmp;
        }

        public void SeekZero()
        {
            m_Pos = 0;
        }

        public void Seek(uint pos)
        {
            SeekZero();
            WriteCheckForSpace(pos);
            m_Pos = pos;
        }

        public void SeekBack(uint count)
        {
            if (count <= m_Pos)
            {
                m_Pos -= count;
            }
            else
            {
                throw new Exception(string.Format("SeedBack count({0}) is bigger than m_Pos({1})", count, m_Pos));
            }
        }

        /// <summary>
        /// 去掉已经读完的字节
        /// 并将 Position 设置成0
        /// </summary>
        public void Arrangement(int pos = -1)
        {
            pos = pos == -1 ? (int) m_Pos : pos;
            var len = m_Buffer.Length;
            if (pos > len)
            {
                pos = len;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            var size = 0;
            if (pos < len)
            {
                size = len - pos;
            }

            Buffer.BlockCopy(m_Buffer, pos, m_Buffer, 0, size);
            m_Pos = 0;
        }

        public void Replace(byte[] buffer)
        {
            m_Buffer = buffer;
            m_Pos = 0;
        }

        public void Skip(int count)
        {
            for (var i = 0; i < count; i++)
                ReadByte();
        }

        public override string ToString()
        {
            return String.Format("NetBuf sz:{0} pos:{1}", m_Buffer.Length, m_Pos);
        }

        /*public string ToHexString()
        {
            return NetEncoding.BytesToHex(m_Buffer, (int) m_Pos);
        }*/
    }
}