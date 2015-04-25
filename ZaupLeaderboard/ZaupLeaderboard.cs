using System;
using System.Collections.Generic;
using Rocket.RocketAPI;
using Rocket.Logging;
using Rocket.RocketAPI.Events;

namespace ZaupLeaderboard
{
    public class ZaupLeaderboard : RocketPlugin<ZLConfiguration>
    {
        public static ZaupLeaderboard Instance;
        public ZLDatabaseManager DatabaseMgr;
        public string UpdatePlayedTimeSql;

        protected override void Load()
        {
            ZaupLeaderboard.Instance = this;
            this.DatabaseMgr = new ZLDatabaseManager();
            this.UpdatePlayedTimeSql = "update `"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName
                    + "` set `lastdisconn`=current_timestamp, `timeplayed`=`timeplayed`+ TIMESTAMPDIFF(SECOND,`"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName
                    + "`.`lastconn`,CURRENT_TIMESTAMP())";
            RocketServerEvents.OnPlayerConnected += this.onPlayerConnected;
        }
        private void onPlayerConnected(RocketPlayer player)
        {
            byte success = ZaupLeaderboard.Instance.DatabaseMgr.onPlayerConnected(player.CSteamID, player.CharacterName);
            if (success <= 0)
            {
                Logger.Log("Could not add or update the leaderboard for " + player.CharacterName + ".");
            }
        }
    }
}
