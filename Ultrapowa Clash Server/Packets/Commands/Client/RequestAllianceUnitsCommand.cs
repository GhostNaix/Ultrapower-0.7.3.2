using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.StreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Commands.Client
{
    // Packet 511
    internal class RequestAllianceUnitsCommand : Command
    {
        public RequestAllianceUnitsCommand(Reader reader, Device client, int id) : base(reader, client, id)
        {
        }

        internal override void Decode()
        {
            this.Reader.ReadInt32();
            this.FlagHasRequestMessage = this.Reader.ReadByte();
            this.Message = this.Reader.ReadString();
            this.Message2 = this.Reader.ReadString();
        }

        internal override async void Process()
        {
            try
            {
                ClientAvatar player = this.Device.Player.Avatar;
                player.TroopRequestMessage = this.Message;
                Alliance all = ObjectManager.GetAlliance(player.AllianceId);
                TroopRequestStreamEntry cm = new TroopRequestStreamEntry();
                cm.SetSender(player);
                cm.Message = this.Message;
                cm.ID = all.m_vChatMessages.Count + 1;
                cm.SetMaxTroop(player.GetAllianceCastleTotalCapacity());
                cm.m_vDonatedTroop = player.GetAllianceCastleUsedCapacity();

                StreamEntry s = all.m_vChatMessages.Find(c => c.SenderID == this.Device.Player.Avatar.UserId && c.GetStreamEntryType() == 1);
                if (s != null)
                {
                    all.m_vChatMessages.RemoveAll(t => t == s);
                    all.AddChatMessage(cm);
                }
                else
                {
                    all.AddChatMessage(cm);
                }

                foreach (AllianceMemberEntry op in all.GetAllianceMembers())
                {
                    Level aplayer = await ResourcesManager.GetPlayer(op.AvatarId);
                    if (aplayer.Client != null)
                    {
                        AllianceStreamEntryMessage p = new AllianceStreamEntryMessage(aplayer.Client);
                        p.SetStreamEntry(cm);
                        p.Send();

                        if (s != null)
                        {
                            new AllianceStreamEntryRemovedMessage(aplayer.Client, s.ID).Send();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public byte FlagHasRequestMessage;
        public string Message;
        public int MessageLength;
        public string Message2;
    }
}
