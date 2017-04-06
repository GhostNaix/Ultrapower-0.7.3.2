using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class SystemMessageGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public SystemMessageGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(1);
        }

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 1)
                {
                    var message = string.Join(" ", m_vArgs.Skip(1));
                    var avatar = level.Avatar;
                    var mail = new AllianceMailStreamEntry();
                    mail.SetId((int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                    mail.SenderId = avatar.UserId;
                    mail.SetSenderAvatarId(avatar.UserId);
                    mail.SetSenderName(avatar.AvatarName);
                    mail.SetIsNew(2);
                    mail.AllianceId = 0;
                    mail.AllianceBadgeData = 1526735450;
                    mail.AllianceName = "Administrator";
                    mail.Message = message;
                    mail.SetSenderLevel(avatar.m_vAvatarLevel);
                    mail.SetSenderLeagueId(avatar.m_vLeagueId);

                    foreach (var onlinePlayer in ResourcesManager.m_vOnlinePlayers)
                    {
                        var p = new AvatarStreamEntryMessage(onlinePlayer.Client);
                        p.SetAvatarStreamEntry(mail);
                        Processor.Send(p);
                    }
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
