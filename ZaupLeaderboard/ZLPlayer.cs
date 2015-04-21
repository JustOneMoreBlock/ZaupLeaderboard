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
using unturned.ROCKS.Uconomy;
using Uconomy_Essentials;
using UconomyBasicShop;


namespace ZaupLeaderboard
{
    public class ZLPlayer : RocketPlayerComponent
    {
        private RocketPlayerEvents rpe;
        private Votifier Votifier;
        private Uconomy Uconomy;
        private Uconomy_Essentials.Uconomy_Essentials UE;
        private UconomyBasicShop.UconomyBasicShop ZS;
        private Dictionary<string, uint> stats;
        private Dictionary<string, decimal> money;
        private Dictionary<string, uint> serversvotedon = new Dictionary<string, uint>();
        

        protected void Load() {
            this.Votifier = Votifier.Instance;
            this.Uconomy = Uconomy.Instance;
            this.UE = Uconomy_Essentials.Uconomy_Essentials.Instance;
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
            RocketServerEvents.OnPlayerConnected += new RocketServerEvents.PlayerConnected(this.onPlayerConnected);
            RocketServerEvents.OnPlayerDisconnected += new RocketServerEvents.PlayerDisconnected(this.onPlayerDisconnected);
            this.Votifier.OnPlayerVoted += new Votifier.PlayerVotedEvent(this.onPlayerVoted);
            this.rpe.OnDeath += new RocketPlayerEvents.PlayerDeath(this.onPlayerDeath);
            this.rpe.OnUpdateStat += new RocketPlayerEvents.PlayerUpdateStat(this.onUpdateStat);
            this.UE.OnPlayerExchange += new Uconomy_Essentials.Uconomy_Essentials.PlayerExchangeEvent(this.onPlayerExchange);
            this.UE.OnPlayerLoss += new Uconomy_Essentials.Uconomy_Essentials.PlayerLossEvent(this.onPlayerLoss);
            this.UE.OnPlayerPaid += new Uconomy_Essentials.Uconomy_Essentials.PlayerPaidEvent(this.onPlayerPaid);
            this.ZS.OnShopBuy += new UconomyBasicShop.UconomyBasicShop.PlayerShopBuy(this.onShopBuy);
            this.ZS.OnShopSell += new UconomyBasicShop.UconomyBasicShop.PlayerShopSell(this.onShopSell);
        }
        private void onPlayerConnected(RocketPlayer player)
        {
            byte success = ZaupLeaderboard.Instance.DatabaseMgr.onPlayerConnected(player.CSteamID, player.CharacterName);
            if (success <= 0)
            {
                Logger.Log("Could not add or update the leaderboard for " + player.CharacterName + ".");
            }
        }
        private void onPlayerDisconnected(RocketPlayer player)
        {
            string sql = ZaupLeaderboard.Instance.UpdatePlayedTimeSql;
            uint topstreak = ZaupLeaderboard.Instance.DatabaseMgr.GetPvpStreak(player.CSteamID);
            uint pvpstreak = this.stats["pvpstreak"];
            uint toppvpstreak = this.stats["toppvpstreak"];
            if (pvpstreak > 0 && pvpstreak > toppvpstreak)
            {
                this.stats["toppvpstreak"] = pvpstreak;
                toppvpstreak = pvpstreak;    
            }
            if (toppvpstreak > topstreak)
            {
                sql += ", `toppvpstreak`=" + toppvpstreak;
            }
            // Only keep the last streak.
            sql += ", `pvpstreak`=" + pvpstreak;
            // Now loop through the others and add to the sql update.
            foreach (string s in this.stats.Keys)
            {
                sql += ", `" + s + "`=`" + s + "` + " + this.stats[s];
            }
            foreach (string s2 in this.money.Keys)
            {
                sql += ", `" + s2 + "`=`" + s2 + "` + " + this.money[s2];
            }
            sql += " where `steamId`='" + player.CSteamID + "'";
            byte success = ZaupLeaderboard.Instance.DatabaseMgr.UpdateTable(player.CSteamID, sql);
            if (success < 1)
            {
                Logger.Log("There was problem saving the leaderboard info for " + player.CharacterName + ".");
            }
        }
        private void onPlayerVoted(RocketPlayer player, ServiceDefinition defintion)
        {
            if (this.serversvotedon.ContainsKey(defintion.Name))
            {
                this.serversvotedon[defintion.Name]++;
            }
            else
            {
                this.serversvotedon.Add(defintion.Name, 1);
            }
        }
        private void onPlayerDeath(RocketPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
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
            switch(stat) {
                case EPlayerStat.KILLS_ZOMBIES_NORMAL:
                    this.stats["zombiekills"]++;
                    break;
                case EPlayerStat.KILLS_ZOMBIES_MEGA:
                    this.stats["zombiekillsmega"]++;
                    break;
                case EPlayerStat.KILLS_PLAYERS:
                    this.stats["playerkills"]++;
                    break;
                case EPlayerStat.FOUND_ITEMS:
                    this.stats["founditems"]++;
                    break;
                case EPlayerStat.FOUND_RESOURCES:
                    this.stats["foundresources"]++;
                    break;
                case EPlayerStat.FOUND_EXPERIENCE:
                    this.stats["foundexperience"]++;
                    break;
            }
        }
        private void onPlayerExchange(RocketPlayer player, decimal currency, uint experience)
        {
            this.money["totalmoneyearned"] += currency;
            this.stats["totalxpexchanged"] += experience;
        }
        private void onPlayerLoss(RocketPlayer player, decimal amount)
        {
            this.money["totalmoneyearned"] -= amount;
            this.money["totalmoneylost"] += amount;
        }
        private void onPlayerPaid(RocketPlayer player, decimal amount)
        {
            this.money["totalmoneyearned"] += amount;
        }
        private void onShopBuy(RocketPlayer player, decimal currency, byte amtitems, ushort id, string type)
        {
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
        private void onShopSell(RocketPlayer player, decimal currency, byte amtitems, ushort id)
        {
            this.money["totalmoneyearned"] += currency;
            this.stats["totalitemsold"] += amtitems;
        }
    }
}
