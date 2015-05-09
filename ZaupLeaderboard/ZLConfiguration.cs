using System;
using System.Collections.Generic;
using Rocket.RocketAPI;

namespace ZaupLeaderboard
{
    public class ZLConfiguration : IRocketConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;
        public IRocketConfiguration DefaultConfiguration
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
