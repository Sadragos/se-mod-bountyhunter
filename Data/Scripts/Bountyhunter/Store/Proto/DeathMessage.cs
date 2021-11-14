using ProtoBuf;
using System;
using System.Xml.Serialization;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class DeathMessage
    {
        [ProtoMember(1)]
        [XmlAttribute]
        public string Player = "*";

        [ProtoMember(2)]
        [XmlAttribute]
        public string Message;

        [ProtoMember(3)]
        [XmlAttribute]
        public bool Enabled = true;

        public DeathMessage(string player, string message)
        {
            Player = player;
            Message = message;
        }

        public DeathMessage()
        {
        }
    }
}