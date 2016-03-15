/*
    Copyright 2011 MCForge
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
*/
using System.Collections.Generic;
using System.Data;
using System.IO;
using MCGalaxy.Eco;
using MCGalaxy.SQL;

namespace MCGalaxy {
    public static class Economy {

        public static bool Enabled = false;
        
        public const string createTable =
            @"CREATE TABLE if not exists Economy (
                player         VARCHAR(20),
                money       INT UNSIGNED,
                total       INT UNSIGNED NOT NULL DEFAULT 0,
                purchase    VARCHAR(255) NOT NULL DEFAULT '%cNone',
                payment     VARCHAR(255) NOT NULL DEFAULT '%cNone',
                salary      VARCHAR(255) NOT NULL DEFAULT '%cNone',
                fine        VARCHAR(255) NOT NULL DEFAULT '%cNone',
                PRIMARY KEY(player)
            );";

        public struct EcoStats {
            public string playerName, purchase, payment, salary, fine;
            public int money, totalSpent;
            public EcoStats(string name, int mon, int tot, string pur, string pay, string sal, string fin) {
                playerName = name;
                money = mon;
                totalSpent = tot;
                purchase = pur;
                payment = pay;
                salary = sal;
                fine = fin;
            }
        }

        public static class Settings {
            //Maps
            public static bool Levels = false;
            public static List<Level> LevelsList = new List<Level>();
            public class Level {
                public int price;
                public string name;
                public string x;
                public string y;
                public string z;
                public string type;
            }
        }

        public static void LoadDatabase() {
        retry:
            Database.executeQuery(createTable); //create database
            DataTable eco = Database.fillData("SELECT * FROM Economy");
            try {
                DataTable players = Database.fillData("SELECT * FROM Players");
                if (players.Rows.Count == eco.Rows.Count) { } //move along, nothing to do here
                else if (eco.Rows.Count == 0) { //if first time, copy content from player to economy
                    Database.executeQuery("INSERT INTO Economy (player, money) SELECT Players.Name, Players.Money FROM Players");
                } else {
                    //this will only be needed when the server shuts down while it was copying content (or some other error)
                    Database.executeQuery("DROP TABLE Economy");
                    goto retry;
                }
                players.Dispose(); eco.Dispose();
            } catch { }
        }

        public static void Load() {
            /*if (loadDatabase) {
            retry:
                if (Server.useMySQL) MySQL.executeQuery(createTable); else SQLite.executeQuery(createTable); //create database on server loading
                string queryP = "SELECT * FROM Players"; string queryE = "SELECT * FROM Economy";
                DataTable eco = Server.useMySQL ? MySQL.fillData(queryE) : SQLite.fillData(queryE);
                try {
                    DataTable players = Server.useMySQL ? MySQL.fillData(queryP) : SQLite.fillData(queryP);
                    if (players.Rows.Count == eco.Rows.Count) { } //move along, nothing to do here
                    else if (eco.Rows.Count == 0) { //if first time, copy content from player to economy
                        string query = "INSERT INTO Economy (player, money) SELECT Players.Name, Players.Money FROM Players";
                        if (Server.useMySQL) MySQL.executeQuery(query); else SQLite.executeQuery(query);
                    } else {
                        //this will only be needed when the server shuts down while it was copying content (or some other error)
                        if (Server.useMySQL) MySQL.executeQuery("DROP TABLE Economy"); else SQLite.executeQuery("DROP TABLE Economy");
                        goto retry;
                    }
                    players.Dispose(); eco.Dispose();
                } catch { }
                return;
            }*/

            if (!File.Exists("properties/economy.properties")) { 
                Server.s.Log("Economy properties don't exist, creating"); 
                File.Create("properties/economy.properties").Close(); 
                Save(); 
            }
            using (StreamReader r = File.OpenText("properties/economy.properties")) {
                string line;
                while (!r.EndOfStream) {
                    line = r.ReadLine().ToLower().Trim();
                    string[] linear = line.ToLower().Trim().Split(':');
                    try {
                        switch (linear[0]) {
                            case "enabled":
                    		    Enabled = linear[1].CaselessEquals("true"); break;

                            case "level":
                                if (linear[1] == "enabled") {
                                    if (linear[2] == "true") { Settings.Levels = true; } else if (linear[2] == "false") { Settings.Levels = false; }
                                }
                                if (linear[1] == "levels") {
                                    Settings.Level lvl = new Settings.Level();
                                    if (FindLevel(linear[2]) != null) { lvl = FindLevel(linear[2]); Settings.LevelsList.Remove(lvl); }
                                    switch (linear[3]) {
                                        case "name":
                                            lvl.name = linear[4]; break;
                                        case "price":
                                            lvl.price = int.Parse(linear[4]); break;
                                        case "x":
                                            lvl.x = linear[4]; break;
                                        case "y":
                                            lvl.y = linear[4]; break;
                                        case "z":
                                            lvl.z = linear[4]; break;
                                        case "type":
                                            lvl.type = linear[4]; break;
                                    }
                                    Settings.LevelsList.Add(lvl);
                                }
                                break;
                             default:
                                if (linear.Length < 3) break;
                                Item item = GetItem(linear[0]);
                                if (item != null) item.Parse(line, linear);
                                break;
                        }
                    } catch { }
                }
                r.Close();
            }
            Save();
        }

        public static void Save() {
            if (!File.Exists("properties/economy.properties")) { Server.s.Log("Economy properties don't exist, creating"); }
            //Thread.Sleep(2000);
            File.Delete("properties/economy.properties");
            //Thread.Sleep(2000);
            using (StreamWriter w = File.CreateText("properties/economy.properties")) {
                //enabled
                w.WriteLine("enabled:" + Enabled);              
                foreach (Item item in Items) {
                    w.WriteLine();
                    item.Serialise(w);                    
                }

                //maps
                w.WriteLine();
                w.WriteLine("level:enabled:" + Settings.Levels);
                foreach (Settings.Level lvl in Settings.LevelsList) {
                    w.WriteLine();
                    w.WriteLine("level:levels:" + lvl.name + ":name:" + lvl.name);
                    w.WriteLine("level:levels:" + lvl.name + ":price:" + lvl.price);
                    w.WriteLine("level:levels:" + lvl.name + ":x:" + lvl.x);
                    w.WriteLine("level:levels:" + lvl.name + ":y:" + lvl.y);
                    w.WriteLine("level:levels:" + lvl.name + ":z:" + lvl.z);
                    w.WriteLine("level:levels:" + lvl.name + ":type:" + lvl.type);
                }
                w.Close();
            }
        }

        public static Settings.Level FindLevel(string name) {
            foreach (Settings.Level lvl in Settings.LevelsList) {
                try {
                    if (lvl.name.CaselessEquals(name)) return lvl;
                } catch { }
            }
            return null;
        }

        public static EcoStats RetrieveEcoStats(string playername) {
            EcoStats es;
            es.playerName = playername;
            DatabaseParameterisedQuery query = DatabaseParameterisedQuery.Create();
            query.AddParam("@Name", playername);
            using (DataTable eco = Database.fillData(query, "SELECT * FROM Economy WHERE player=@Name")) {
                if (eco.Rows.Count >= 1) {
                    es.money = int.Parse(eco.Rows[0]["money"].ToString());
                    es.totalSpent = int.Parse(eco.Rows[0]["total"].ToString());
                    es.purchase = eco.Rows[0]["purchase"].ToString();
                    es.payment = eco.Rows[0]["payment"].ToString();
                    es.salary = eco.Rows[0]["salary"].ToString();
                    es.fine = eco.Rows[0]["fine"].ToString();
                } else {
                    es.money = 0;
                    es.totalSpent = 0;
                    es.purchase = "%cNone";
                    es.payment = "%cNone";
                    es.salary = "%cNone";
                    es.fine = "%cNone";
                }
            }
            return es;
        }

        public static void UpdateEcoStats(EcoStats es) {
            DatabaseParameterisedQuery query = DatabaseParameterisedQuery.Create();
            query.AddParam("@Name", es.playerName);
            query.AddParam("@Money", es.money);
            query.AddParam("@Total", es.totalSpent);
            query.AddParam("@Purchase", es.purchase);
            query.AddParam("@Payment", es.payment);
            query.AddParam("@Salary", es.salary);
            query.AddParam("@Fine", es.fine);
            Database.executeQuery(query, string.Format("{0} Economy (player, money, total, purchase, payment, salary, fine) VALUES " +
                                                       "(@Name, @Money, @Total, @Purchase, @Payment, @Salary, @Fine)", (Server.useMySQL ? "REPLACE INTO" : "INSERT OR REPLACE INTO")));
        }
        
        public static Item[] Items = { new ColorItem(), new TitleColorItem(), new TitleItem(), new RankItem() };
        
        public static Item GetItem(string name) {
            foreach (Item item in Items) {
                if (name.CaselessEquals(item.Name)) return item;
            }
            return null;
        }
        
        public static SimpleItem Color { get { return (SimpleItem)Items[0]; } }
        public static SimpleItem TitleColor { get { return (SimpleItem)Items[1]; } }
        public static SimpleItem Title { get { return (SimpleItem)Items[2]; } }
        public static RankItem Ranks { get { return (RankItem)Items[3]; } }
    }
}