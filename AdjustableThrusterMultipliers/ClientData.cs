using ProtoBuf;
using System.Collections.Generic;

namespace AdjustableThrusterMultipliers
{
    [ProtoContract]
    public class ClientData
    {
        [ProtoMember(1)]
        public Dictionary<long, float> ThrustersToChange { get; set; } = new Dictionary<long, float>();
    }
}