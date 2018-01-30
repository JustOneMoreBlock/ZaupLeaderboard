using System;
using System.Collections.Generic;

using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using Steamworks;
using SDG.Unturned;

namespace ZaupLeaderboard
{
    public class ZaupLeaderboard : RocketPlugin<ZLConfiguration>
    {
        public static ZaupLeaderboard Instance;
        public ZLDatabaseManager DatabaseMgr;
        public string UpdatePlayedTimeSql;
        public delegate void OnChangeStats(CSteamID steamID, string WhichStat);
        public OnChangeStats OnChangeStatsEvent;

        protected override void Load()
        {
            ZaupLeaderboard.Instance = this;
            this.DatabaseMgr = new ZLDatabaseManager();
            this.UpdatePlayedTimeSql = "update `"
                    + ZaupLeaderboard.Instance.Configuration.Instance.DatabaseTableName
                    + "` set `lastdisconn`=current_timestamp, `timeplayed`=`timeplayed`+ TIMESTAMPDIFF(SECOND,`"
                    + ZaupLeaderboard.Instance.Configuration.Instance.DatabaseTableName
                    + "`.`lastconn`,CURRENT_TIMESTAMP())";
            U.Events.OnPlayerConnected += this.OnPlayerConnected;
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            byte success = ZaupLeaderboard.Instance.DatabaseMgr.OnPlayerConnected(player.CSteamID, player.CharacterName);
            if (success <= 0)
            {
                Logger.Log("Could not add or update the leaderboard for " + player.CharacterName + ".");
            }
        }
    }
}
