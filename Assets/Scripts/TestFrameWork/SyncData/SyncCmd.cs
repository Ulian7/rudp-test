using LockStep.Network;

public class SyncCmd 
{
    public byte PlayerId;
    public ushort ClientFrameId = 0;
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
    public void WriteToBufferS2C(NetWriter buffer)
    {
        buffer.Write(PlayerId);
        buffer.Write(VKey);
        if (VKey == 516)
        {
            ;
        }
        WriteArgToBuffer(buffer);
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
    
    public void ReadFromBufferS2C(NetReader buffer)
    {
        PlayerId = buffer.ReadByte();
        VKey = buffer.ReadUInt16();
        ReadArgFromBuffer(buffer);
    }

    public override string ToString()
    {
        string temp = "Player" + PlayerId.ToString() + " Input Vkey " + VKey + " in viewFrame " + ClientFrameId.ToString() + "\n";
        return temp;
    }
}
