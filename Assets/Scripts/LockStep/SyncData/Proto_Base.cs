using System;
using System.Threading.Tasks;
public class Proto_Base
{
    public virtual void Update(in DateTimeOffset time)
    {
        
    }

    public virtual void Send(byte[] bytes)
    {
        
    }

    public virtual async ValueTask<Byte[]> Receive(float interval)
    {
        return null;
    }
}