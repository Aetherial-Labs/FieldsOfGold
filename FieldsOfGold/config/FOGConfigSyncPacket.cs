using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace FieldsOfGold.config
{
    [ProtoContract]
    internal class FOGConfigSyncPacket
    {      
            [ProtoMember(1)]
            public byte[] configData;       
    }
}
