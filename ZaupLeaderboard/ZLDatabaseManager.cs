﻿using System;
using System.Collections.Generic;
using System.Linq;
using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.RocketAPI;
using Rocket.Logging;
using Steamworks;
using System.Text;
using System.Threading.Tasks;

namespace ZaupLeaderboard
{
    public class ZLDatabaseManager
    {
        public ZLDatabaseManager()
		{
			new CP1250();
			this.CheckSchema();
		}
        internal void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = "show tables like '"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName + "'";
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = "CREATE TABLE `"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName
                    + "` (`steamId` varchar(32) NOT NULL, "
                    + "`name` varchar(32) NOT NULL, "
                    + "`firstconn` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, "
                    + "`lastconn` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, "
                    + "`lastdisconn` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, "
                    + "`timeplayed` int(11) NOT NULL DEFAULT 0, "
                    + "`zombiekills` int(8) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`zombiekillsmega` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`playerkills` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyplayer` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbybleeding` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbybones` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyfreezing` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyfood` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbywater` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbygun` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbymelee` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyzombie` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbysuicide` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbykill` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyinfection` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbypunch` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbybreath` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyroadkill` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyvehicle` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbygrenade` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`killedbyshred` int(6) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`founditems` int(8) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`foundresources` int(8) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`foundexperience` int(8) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`pvpstreak` int(3) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`toppvpstreak` int(3) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`serversvotedon` varchar(255) NULL, "
                    + "`totalmoneyearned` decimal(15,2) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`totalmoneyspent` decimal(15,2) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`totalmoneylost` decimal(15,2) UNSIGNED NOT NULL DEFAULT 0.00, "
                    + "`totalxpexchanged` int(8) UNSIGNED NOT NULL DEFAULT 0, "
                    + "`totalitemsbought` int(8) UNSIGNTED NOT NULL DEFAULT 0, "
                    + "`totalvehiclesbought` int(8) UNSIGNTED NOT NULL DEFAULT 0, "
                    + "`totalitemsold` int(8) UNSIGNED NOT NULL DEFAULT 0, primary key ('steamId'))";
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        private MySqlConnection createConnection()
		{
			MySqlConnection result = null;
			try
			{
				if (ZaupLeaderboard.Instance.Configuration.DatabasePort == 0)
				{
					ZaupLeaderboard.Instance.Configuration.DatabasePort = 3306;
				}
				result = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[]
				{
					ZaupLeaderboard.Instance.Configuration.DatabaseAddress,
					ZaupLeaderboard.Instance.Configuration.DatabaseName,
					ZaupLeaderboard.Instance.Configuration.DatabaseUsername,
					ZaupLeaderboard.Instance.Configuration.DatabasePassword,
					ZaupLeaderboard.Instance.Configuration.DatabasePort
				}));
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}
			return result;
		}
        public byte onPlayerConnected(CSteamID id, string name)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = "insert into '"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName
                    + "' ('streamId', 'name') VALUES ('"
                    + id.ToString() + "', '"
                    + name + "') on duplicate key update `"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName
                    + "` set `name`='" 
                    + name + "', `lastconn`=current_timestamp where `steamId`='" 
                    + id.ToString() + "'";
                mySqlConnection.Open();
                byte success = (byte)mySqlCommand.ExecuteScalar();
                mySqlConnection.Close();
                return success;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return 0;
            }
        }
        public byte UpdateTable(CSteamID id, string sql)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = String.Format(sql, ZaupLeaderboard.Instance.Configuration.DatabaseTableName);
                mySqlConnection.Open();
                byte success = (byte)mySqlCommand.ExecuteScalar();
                mySqlConnection.Close();
                return success;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return 0;
            }
        }
        public uint GetPvpStreak(CSteamID id)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = "select `toppvpstreak` from '"
                    + ZaupLeaderboard.Instance.Configuration.DatabaseTableName
                    + "' where `steamId`='"
                    + id.ToString() + "'";
                mySqlConnection.Open();
                uint success = (uint)mySqlCommand.ExecuteScalar();
                mySqlConnection.Close();
                return success;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return 0;
            }
        }
    }
}
