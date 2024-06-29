using Morefun.LockStep.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
using UnityEngine;

public class SyncCmd 
{
    public byte PlayerId;
    public ushort ClientFrameId;
    public ushort VKey;
    public byte[] Args;
    public int ArgLen => Args == null ? 0 : Args.Length;

    public void Reset(byte newPlayerId, ushort newClientFrameId, ushort newVKey, byte[] newArgs)
    {
        PlayerId = newPlayerId;
        ClientFrameId = newClientFrameId;
        VKey = newVKey;
        Args = newArgs;
    }

    public void WriteArgToBuffer(NetWriter buffer)
    {
        buffer.Write((ushort)(ArgLen));
        buffer.WriteBytesFullWithoutLen(Args);
    }
    public void WriteToBuffer(NetWriter buffer)
    {
        buffer.Write(PlayerId);
        buffer.Write(ClientFrameId);
        buffer.Write(VKey);
        if (VKey == 516)
        {
            ;
        }
        WriteArgToBuffer(buffer);
    }

    public void ReadArgFromBuffer(NetReader buffer)
    {
        var len = buffer.ReadUInt16();
        if (len == 0)
            Args = null;
        else
        {
            var bytes = buffer.ReadBytes(len);
            Args = bytes;
        }
    }

    public void ReadFromBuffer(NetReader buffer)
    {
        PlayerId = buffer.ReadByte();
        ClientFrameId = buffer.ReadUInt16();
        VKey = buffer.ReadUInt16();
        ReadArgFromBuffer(buffer);
    }
}
