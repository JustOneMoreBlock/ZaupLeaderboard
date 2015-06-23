using System;
using System.Collections.Generic;
using Rocket.API;

namespace ZaupLeaderboard
{
    public class ZLConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;
        public IRocketPluginConfiguration DefaultConfiguration
        {
            get
            {
                return new ZLConfiguration
                {
                    DatabaseAddress = "localhost",
                    DatabaseUsername = "unturned",
                    DatabasePassword = "password",
                    DatabaseName = "unturned",
                    DatabaseTableName = "leaderboard",
                    DatabasePort = 3306
                };
            }
        }
    }
}
