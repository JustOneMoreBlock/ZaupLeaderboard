using System;
using System.Collections.Generic;
using Rocket.Components;
using Rocket.RocketAPI.Events;
using Rocket.RocketAPI;
using Rocket.Logging;
using SDG;
using UnityEngine;
using Steamworks;
using unturned.ROCKS.Votifier;
using Newtonsoft.Json;


namespace ZaupLeaderboard
{
    public class ZLPlayer : RocketPlayerComponent
    {
        private RocketPlayerEvents rpe;
        private Votifier Votifier;
        private Dictionary<string, uint> stats;
        private Dictionary<string, decimal> money;
        private Dictionary<string, uint> serversvotedon = new Dictionary<string, uint>();
        private uint lastexperience;
        private CSteamID playerid;
        private bool disconnecteddone;

        public void Start() {
            this.playerid = this.PlayerInstance.CSteamID;
            this.Votifier = Votifier.Instance;
            this.rpe = base.gameObject.transform.GetComponent<RocketPlayerEvents>();
            this.stats = new Dictionary<string,uint>{
                {"zombiekills", 0},
                {"zombiekillsmega", 0},
                {"playerkills", 0},
                {"killedbyplayer", 0},
                {"killedbybleeding", 0},
                {"killedbybones", 0},
                {"killedbyfreezing", 0},
                {"killedbyfood", 0},
                {"killedbywater", 0},
                {"killedbygun", 0},
                {"killedbymelee", 0},
                {"killedbyzombie", 0},
                {"killedbysuicide", 0},
                {"killedbykill", 0},
                {"killedbyinfection", 0},
                {"killedbypunch", 0},
                {"killedbybreath", 0},
                {"killedbyroadkill", 0},
                {"killedbyvehicle", 0},
                {"killedbygrenade", 0},
                {"killedbyshred", 0},
                {"founditems", 0},
                {"foundresources", 0},
                {"foundexperience", 0},
                {"pvpstreak", 0},
                {"toppvpstreak", 0},
                {"totalxpexchanged", 0},
                {"totalitemsbought", 0},
                {"totalvehiclesbought", 0},
                {"totalitemsold", 0},
            };
            this.money = new Dictionary<string, decimal> {
                {"totalmoneyearned", 0.00m},
                {"totalmoneyspent", 0.00m},
                {"totalmoneylost", 0.00m}
            };
            RocketServerEvents.OnPlayerDisconnected += this.onPlayerDisconnected;
            RocketServerEvents.OnServerShutdown += this.onServerShutdown;
            this.Votifier.OnPlayerVoted += this.onPlayerVoted;
            this.rpe.OnDeath += this.onPlayerDeath;
            this.rpe.OnUpdateStat += this.onUpdateStat;
            this.rpe.OnUpdateExperience += this.onUpdateExperience;
            string votes = ZaupLeaderboard.Instance.DatabaseMgr.GetVotes(this.playerid);
            this.stats["toppvpstreak"] = ZaupLeaderboard.Instance.DatabaseMgr.GetPvpStreak(this.playerid);
            this.serversvotedon = JsonConvert.DeserializeObject<Dictionary<string, uint>>(votes);
            this.lastexperience = this.PlayerInstance.Experience;
        }
        private void onPlayerDisconnected(RocketPlayer player)
        {
            if (player.CSteamID != this.playerid) return;
            string sql = ZaupLeaderboard.Instance.UpdatePlayedTimeSql;
            uint lasttopstreak = ZaupLeaderboard.Instance.DatabaseMgr.GetPvpStreak(player.CSteamID);
            if (this.stats["pvpstreak"] > 0 && this.stats["pvpstreak"] > this.stats["toppvpstreak"]) 
                this.stats["toppvpstreak"] = this.stats["pvpstreak"];
            if (this.stats["toppvpstreak"] > lasttopstreak)
            {
                sql += ", `toppvpstreak`=" + this.stats["toppvpstreak"];
            }
            // Only keep the last and top streak.
            sql += ", `pvpstreak`=" + this.stats["pvpstreak"];
            string json = JsonConvert.SerializeObject(this.serversvotedon, Formatting.Indented);
            sql += ", `serversvotedon`='" + json + "'";
            // Now loop through the others and add to the sql update.
            foreach (string s in this.stats.Keys)
            {
                if (s != "pvpstreak" && s != "toppvpstreak")
                    sql += ", `" + s + "`=`" + s + "` + " + this.stats[s];
            }
            foreach (string s2 in this.money.Keys)
            {
                sql += ", `" + s2 + "`=`" + s2 + "` + " + this.money[s2];
            }
            sql += " where `steamId`='" + player.CSteamID + "'";
            byte success = ZaupLeaderboard.Instance.DatabaseMgr.UpdateTable(player.CSteamID, sql);
            this.disconnecteddone = true;
            if (success < 1)
            {
                Logger.Log("There was problem saving the leaderboard info for " + player.CharacterName + ".");
            }
        }
        private void onServerShutdown()
        {
            if (this.disconnecteddone) return;
            RocketPlayer player = this.PlayerInstance;
            this.onPlayerDisconnected(player);
        }
        private void onPlayerVoted(RocketPlayer player, ServiceDefinition defintion)
        {
            Logger.Log("onPlayerVoted called.");
            if (player.CSteamID != this.playerid) return;
            Logger.Log("checking if key exists");
            if (this.serversvotedon.ContainsKey(defintion.Name))
            {
                Logger.Log("key exists");
                this.serversvotedon[defintion.Name]++;
            }
            else
            {
                Logger.Log("key doesn't exist, adding new");
                this.serversvotedon.Add(defintion.Name, 1);
            }
        }
        private void onPlayerDeath(RocketPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (player.CSteamID != this.playerid) return;
            uint pvpstreak = this.stats["pvpstreak"];
            uint toppvpstreak = this.stats["toppvpstreak"];
            if (pvpstreak > 0)
            {
                if (pvpstreak > toppvpstreak)
                {
                    this.stats["toppvpstreak"] = pvpstreak;
                }
                this.stats["pvpstreak"] = 0;
            }
            switch(cause) {
                case EDeathCause.BLEEDING:
                    // Bled to death.
                    this.stats["killedbybleeding"]++;
                    break;
                case EDeathCause.BONES:
                    this.stats["killedbybones"]++;
                    break;
                case EDeathCause.FREEZING:
                    this.stats["killedbyfreezing"]++;
                    break;
                case EDeathCause.FOOD:
                    this.stats["killedbyfood"]++;
                    break;
                case EDeathCause.WATER:
                    this.stats["killedbywater"]++;
                    break;
                case EDeathCause.GUN:
                    this.stats["killedbygun"]++;
                    this.stats["killedbyplayer"]++;
                    break;
                case EDeathCause.MELEE:
                    this.stats["killedbymelee"]++;
                    this.stats["killedbyplayer"]++;
                    break;
                case EDeathCause.PUNCH:
                    this.stats["killedbypunch"]++;
                    this.stats["killedbyplayer"]++;
                    break;
                case EDeathCause.ROADKILL:
                    this.stats["killedbyroadkill"]++;
                    this.stats["killedbyplayer"]++;
                    break;
                case EDeathCause.ZOMBIE:
                    this.stats["killedbyzombie"]++;
                    break;
                case EDeathCause.SUICIDE:
                    this.stats["killedbysuicide"]++;
                    break;
                case EDeathCause.KILL:
                    this.stats["killedbykill"]++;
                    break;
                case EDeathCause.INFECTION:
                    this.stats["killedbyinfection"]++;
                    break;
                case EDeathCause.BREATH:
                    this.stats["killedbybreath"]++;
                    break;
                case EDeathCause.VEHICLE:
                    this.stats["killedbyvehicle"]++;
                    break;
                case EDeathCause.GRENADE:
                    this.stats["killedbygrenade"]++;
                    break;
                case EDeathCause.SHRED:
                    this.stats["killedbyshred"]++;
                    break;
            }
        }
        private void onUpdateStat(RocketPlayer player, EPlayerStat stat)
        {
            if (player.CSteamID != this.playerid) return;
            switch(stat) {
                case EPlayerStat.KILLS_ZOMBIES_NORMAL:
                    this.stats["zombiekills"]++;
                    break;
                case EPlayerStat.KILLS_ZOMBIES_MEGA:
                    this.stats["zombiekillsmega"]++;
                    break;
                case EPlayerStat.KILLS_PLAYERS:
                    this.stats["playerkills"]++;
                    this.stats["pvpstreak"]++;
                    break;
                case EPlayerStat.FOUND_ITEMS:
                    this.stats["founditems"]++;
                    break;
                case EPlayerStat.FOUND_RESOURCES:
                    this.stats["foundresources"]++;
                    break;
            }
        }
        private void onUpdateExperience(RocketPlayer player, uint experience)
        {
            if (player.CSteamID != this.playerid) return;
            if (experience > this.lastexperience)
            {
                uint gainedexperience = experience - this.lastexperience;
                this.stats["foundexperience"] += gainedexperience;
            }
            this.lastexperience = experience;
        }
        public void UEOnPlayerExchange(object[] vars) {
            if (vars.Length != 3) return; // This is an invalid send so ignore it.
            // Correct # of args now see if the player is this one.
            RocketPlayer player = (RocketPlayer)vars[0];
            if (player.CSteamID != this.playerid) return; // Not the player, so we are going to ignore the call.
            decimal currency = (decimal)vars[1];
            uint experience = (uint)vars[2];
            string type = (string)vars[3];
            switch (type)
            {
                case "experience":
                    this.money["totalmoneyearned"] += currency;
                    this.stats["totalxpexchanged"] += experience;
                    break;
                case "money":
                    this.money["totalmoneyspent"] += currency;
                    break;
            }
            
        }
        private void onPlayerExchange(RocketPlayer player, decimal currency, uint experience, string type)
        {
            if (player.CSteamID != this.playerid) return;
            this.money["totalmoneyearned"] += currency;
            this.stats["totalxpexchanged"] += experience;
        }
        public void UEOnPlayerLoss(object[] vars) {
             if (vars.Length != 2) return; // This is an invalid send so ignore it.
            // Correct # of args now see if the player is this one.
            RocketPlayer player = (RocketPlayer)vars[0];
            if (player.CSteamID != this.playerid) return; // Not the player, so we are going to ignore the call.
            decimal amount = (decimal)vars[1];
            this.money["totalmoneylost"] += amount;
        }
        private void onPlayerLoss(RocketPlayer player, decimal amount)
        {
            if (player.CSteamID != this.playerid) return;
            this.money["totalmoneylost"] += amount;
        }
        public void UEOnPlayerPaid(object[] vars)
        {
            if (vars.Length != 2) return; // This is an invalid send so ignore it.
            // Correct # of args now see if the player is this one.
            RocketPlayer player = (RocketPlayer)vars[0];
            if (player.CSteamID != this.playerid) return; // Not the player, so we are going to ignore the call.
            decimal amount = (decimal)vars[1];
            this.money["totalmoneyearned"] += amount;
        }
        private void onPlayerPaid(RocketPlayer player, decimal amount)
        {
            if (player.CSteamID != this.playerid) return;
                this.money["totalmoneyearned"] += amount;
        }
        public void ZaupShopOnBuy(object[] vars)
        {
            Logger.Log("ZaupShopOnBuy has been called.");
            if (vars.Length != 5) return; // This is an invalid send so ignore it.
            // Correct # of args now see if the player is this one.
            RocketPlayer player = (RocketPlayer)vars[0];
            if (player.CSteamID != this.playerid) return; // Not the player, so we are going to ignore the call.
            decimal currency = (decimal)vars[1];
            byte amtitems = (byte)vars[2];
            ushort id = (ushort)vars[3];
            string type = (string)vars[4];
            this.money["totalmoneyspent"] += currency;
            if (type == "vehicle")
            {
                this.stats["totalvehiclesbought"] += amtitems;
            }
            else
            {
                this.stats["totalitemsbought"] += amtitems;
            }
        }
        private void onShopBuy(RocketPlayer player, decimal currency, byte amtitems, ushort id, string type)
        {
            if (player.CSteamID != this.playerid) return;
            this.money["totalmoneyspent"] += currency;
            if (type == "vehicle")
            {
                this.stats["totalvehiclesbought"] += amtitems;
            }
            else
            {
                this.stats["totalitemsbought"] += amtitems;
            }
        }
        public void ZaupShopOnSell(object[] vars)
        {
            Logger.Log("ZaupShopOnSell has been called.");
            if (vars.Length != 4) return; // This is an invalid send so ignore it.
            // Correct # of args now see if the player is this one.
            RocketPlayer player = (RocketPlayer)vars[0];
            if (player.CSteamID != this.playerid) return; // Not the player, so we are going to ignore the call.
            decimal currency = (decimal)vars[1];
            byte amtitems = (byte)vars[2];
            ushort id = (ushort)vars[3];
            this.money["totalmoneyearned"] += currency;
            this.stats["totalitemsold"] += amtitems;
        }
        private void onShopSell(RocketPlayer player, decimal currency, byte amtitems, ushort id)
        {
            if (player.CSteamID != this.playerid) return;
            this.money["totalmoneyearned"] += currency;
            this.stats["totalitemsold"] += amtitems;
        }
    }
}
