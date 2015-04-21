using System;
using System.Collections.Generic;
using Rocket.RocketAPI;

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
        }
    }
}
