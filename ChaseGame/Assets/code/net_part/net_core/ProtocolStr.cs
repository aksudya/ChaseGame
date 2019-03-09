using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;


public class ProtocolStr : ProtocolBase
{
    public string str;

    public override ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        ProtocolStr protocol = new ProtocolStr();
        protocol.str = Encoding.UTF8.GetString(readbuff, start, length);
        return (ProtocolBase)protocol;
    }

    public override byte[] Encode()
    {
        byte[] b = Encoding.UTF8.GetBytes(str);
        return b;
    }

    public override string GetName()
    {
        if (str.Length == 0)
            return "";
        return str.Split(',')[0];
    }

    public override string GetDesc()
    {
        return str;
    }
}
