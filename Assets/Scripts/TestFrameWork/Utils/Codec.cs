using System;
using System.Collections.Generic;
using LockStep.Network;
using LockStep;
public class Codec
{
    public const int MAX_ARG_COUNT = 10; // ����������
    public const int ARG_SIZE = sizeof(Byte);  //������С
    public const int SEND_BUFFER_SIZE = 100;

    public const int SEND_CMD_FIX_LEN =
        sizeof(Byte) + sizeof(UInt16) + sizeof(UInt16) + sizeof(UInt16); //CMDǰ��Ĺ̶����� playerId(1), clientFrameId(2),vkey(2),argCount(2)

    private NetWriter m_sendCmdCache = new NetWriter(new NetBuffer(SEND_BUFFER_SIZE));
    private NetWriter m_sendBuffer = new NetWriter(new NetBuffer(512));
    private NetReader m_sendCmdCacheReader = new NetReader();

    private readonly SyncCmd _tempCmd = new SyncCmd();
    private bool m_isSendBufferDirty = true;

    private Queue<int> m_sendCmdArgCountQueue =
    new Queue<int>(SEND_BUFFER_SIZE / (SEND_CMD_FIX_LEN + MAX_ARG_COUNT * ARG_SIZE));
    public void TransToFrameCmd(byte playerId, ushort vkey, ushort frameId, byte[] args)
    {
        byte[] argBytes = args;
        m_isSendBufferDirty = true;
        _tempCmd.Reset(playerId, frameId, vkey, argBytes);
        m_sendCmdArgCountQueue.Enqueue(_tempCmd.ArgLen);
        _tempCmd.WriteToBuffer(m_sendCmdCache);
    }

    public NetWriter ReadFromSendQueue(out bool isSendBufferChanged, bool is_packaged = false, ushort SEQ = 0, ushort ACK = 0, ushort SID = 0)
    {
        isSendBufferChanged = false;

        //�ع�SendBuffer
        lock (m_sendCmdCache)
            {
                //��ʼ����
                var numCmd = (byte)m_sendCmdArgCountQueue.Count;

                //���SendBuffer�����ӻ��߼���
                if (m_isSendBufferDirty)
                {
                    if (is_packaged)
                    {
                        m_sendBuffer.Write(SEQ);
                        m_sendBuffer.Write(ACK);
                        m_sendBuffer.Write(SID);
                    }
                    m_isSendBufferDirty = false;

                    //m_sendBuffer.Seek(SEND_HEAD_CMD_LEN_POS);
                    //m_sendBuffer.Write((byte)0); //д����ʱֵ���ø���buff�ڲ���ƫ��

                    m_sendCmdCacheReader.Read(m_sendCmdCache);
                    if (numCmd !=0 )
                        m_sendBuffer.Write(numCmd);
                    //var needAuth = IsAuthInfoInSendQueue();
                    //uint addAuth = 0;
                    uint sendCmdNum;
                    for (sendCmdNum = 0; sendCmdNum < numCmd; sendCmdNum++)
                    {
                        var cmd = new SyncCmd();
                        cmd.ReadFromBuffer(m_sendCmdCacheReader);
                        cmd.WriteToBuffer(m_sendBuffer);
                    }

                    isSendBufferChanged = true;
                }
            }
        m_sendCmdArgCountQueue.Clear();
        return m_sendBuffer;
    }

    public void WriteBufferHead()
    {
        m_sendBuffer.SeekZero();
    }
    public void ClearCache()
    {
        m_sendCmdCache.SeekZero();
        m_sendBuffer.SeekZero();
    }
}
