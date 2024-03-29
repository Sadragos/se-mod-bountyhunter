﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bountyhunter.Data.Proto
{
    [ProtoContract]
    public class ClientServerMessage
    {
        [ProtoMember(1)]
        public string Sender;

        [ProtoMember(2)]
        public string Message;

        [ProtoMember(3)]
        public string Type;

        [ProtoMember(4)]
        public ulong SteamId;

        [ProtoMember(5)]
        public string DialogTitle;

        [ProtoMember(6)]
        public int Delay;

        [ProtoMember(7)]
        public string Font;
    }
}
