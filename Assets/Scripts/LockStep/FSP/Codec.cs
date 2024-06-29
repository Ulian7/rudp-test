using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtobufSerialize;
using Morefun.LockStep.Network;
using Morefun.LockStep;
using System.Security.Cryptography;
public class Codec
{
    public const int MAX_ARG_COUNT = 10; // 最大参数个数
    public const int ARG_SIZE = sizeof(Byte);  //参数大小
    public const int SEND_BUFFER_SIZE = 100;

    public const int SEND_CMD_FIX_LEN =
        sizeof(Byte) + sizeof(UInt16) + sizeof(UInt16) + sizeof(UInt16); //CMD前面的固定长度 playerId(1), clientFrameId(2),vkey(2),argCount(2)

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

    public NetWriter ReadFromSendQueue(out bool isSendBufferChanged)
    {
        isSendBufferChanged = false;

        //重构SendBuffer
        lock (m_sendCmdCache)
            {
                //开始处理
                var numCmd = (uint)m_sendCmdArgCountQueue.Count;

                //如果SendBuffer有增加或者减少
                if (m_isSendBufferDirty)
                {
                    m_isSendBufferDirty = false;

                    //m_sendBuffer.Seek(SEND_HEAD_CMD_LEN_POS);
                    //m_sendBuffer.Write((byte)0); //写入临时值，好更新buff内部的偏移

                    m_sendCmdCacheReader.Read(m_sendCmdCache);

                    //var needAuth = IsAuthInfoInSendQueue();
                    //uint addAuth = 0;
                    uint sendCmdNum;
                    for (sendCmdNum = 0; sendCmdNum < numCmd; sendCmdNum++)
                    {
                        var cmd = new SyncCmd();
                        cmd.ReadFromBuffer(m_sendCmdCacheReader);

                        var numArgs = cmd.ArgLen;
                        var vKey = cmd.VKey;

                        cmd.WriteToBuffer(m_sendBuffer);
                    }

                    isSendBufferChanged = true;
                }
            }
        m_sendCmdArgCountQueue.Dequeue();
        return m_sendBuffer;
    }

    public void ClearCache()
    {
        m_sendCmdCache.SeekZero();
        m_sendBuffer.SeekZero();
    }
}
