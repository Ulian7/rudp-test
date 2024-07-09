using System;
using LockStep.Network;
using System.Collections.Generic;

[Serializable]
public class SyncFrame
{
    public ushort FrameId;
    public readonly List<SyncCmd> CmdList = new List<SyncCmd>();
    
    public void WriteToBuffer(NetWriter writer)
    {
        writer.Write(FrameId);
        var count = (byte)(CmdList?.Count ?? 0);
        writer.Write(count);

        for (var i = 0; i < count; i++)
        {
            var cmd = CmdList[i];
            cmd.WriteToBuffer(writer);
        } 
    }

    public void ReadFromBuffer(NetReader reader)
    {
        FrameId = reader.ReadUInt16();
        int count = reader.ReadByte();
        if (CmdList.Count != 0)
            CmdList.Clear();

        for (var i = 0; i < count; i++)
        {
            SyncCmd cmd = new SyncCmd();
            cmd.ReadFromBuffer(reader);
            CmdList.Add(cmd);
        }
    }

    public override string ToString()
    {
        string temp = "\n";
        temp += "ServerFrame " + FrameId.ToString() + "\n";
        foreach (SyncCmd cmd in CmdList)
        {
            temp += "Player" + cmd.PlayerId.ToString() + " Input Vkey " + cmd.VKey + " in viewFrame " + cmd.ClientFrameId.ToString() + "\n";
        }
        return temp;
    }
}