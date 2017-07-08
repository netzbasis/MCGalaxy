﻿/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MCGalaxy.Commands;

namespace MCGalaxy.Gui {

    public partial class PropertyWindow : Form {
        
        bool rankSupressEvents = false;
        
        void LoadRankProps() {
            GuiPerms.SetDefaultIndex(rank_cmbDefault, Group.standard.Permission);
            GuiPerms.SetDefaultIndex(rank_cmbOsMap, ServerConfig.OSPerbuildDefault);
            rank_cbTPHigher.Checked = ServerConfig.HigherRankTP;
            rank_cbSilentAdmins.Checked = ServerConfig.AdminsJoinSilently;
            rank_cbEmpty.Checked = ServerConfig.ListEmptyRanks;
        }
        
        void ApplyRankProps() {
            ServerConfig.DefaultRankName = rank_cmbDefault.SelectedItem.ToString();
            ServerConfig.OSPerbuildDefault = GuiPerms.GetPermission(rank_cmbOsMap, LevelPermission.Nobody);
            ServerConfig.HigherRankTP = rank_cbTPHigher.Checked;
            ServerConfig.AdminsJoinSilently = rank_cbSilentAdmins.Checked;
            ServerConfig.ListEmptyRanks = rank_cbEmpty.Checked;
        }
        
        
        List<Group> storedRanks = new List<Group>();
        void LoadRanks() {
            rank_list.Items.Clear();
            storedRanks.Clear();
            storedRanks.AddRange(Group.GroupList);
            foreach ( Group grp in storedRanks ) {
                rank_list.Items.Add(grp.trueName + " = " + (int)grp.Permission);
            }
            rank_list.SelectedIndex = 0;
        }
        
        void SaveRanks() {
            Group.saveGroups(storedRanks);
            Group.InitAll();
            LoadRanks();
        }
        
        
        void rank_btnColor_Click(object sender, EventArgs e) {
            chat_ShowColorDialog(rank_btnColor, storedRanks[rank_list.SelectedIndex].name + " rank color");
            storedRanks[rank_list.SelectedIndex].color = Colors.Parse(rank_btnColor.Text);
        }

        void rank_list_SelectedIndexChanged(object sender, EventArgs e) {
            if ( rankSupressEvents ) return;
            Group grp = storedRanks.Find(G => G.trueName == rank_list.Items[rank_list.SelectedIndex].ToString().Split('=')[0].Trim());
            if ( grp.Permission == LevelPermission.Nobody ) { rank_list.SelectedIndex = 0; return; }

            rank_txtName.Text = grp.trueName;
            rank_txtPerm.Text = ( (int)grp.Permission ).ToString();
            rank_txtLimit.Text = grp.maxBlocks.ToString();
            rank_txtUndo.Text = grp.maxUndo.ToString();
            chat_ParseColor(grp.color, rank_btnColor);
            rank_txtMOTD.Text = String.IsNullOrEmpty(grp.MOTD) ? String.Empty : grp.MOTD;
            rank_txtOSMaps.Text = grp.OverseerMaps.ToString();
            rank_txtPrefix.Text = grp.prefix;
        }

        void rank_txtName_TextChanged(object sender, EventArgs e) {
            if (rank_txtName.Text.IndexOf(' ') > 0) {
                rank_txtName.Text = rank_txtName.Text.Replace(" ", "");
                return;
            }
            
            if ( rank_txtName.Text != "" && rank_txtName.Text.ToLower() != "nobody" ) {
                storedRanks[rank_list.SelectedIndex].trueName = rank_txtName.Text;
                rankSupressEvents = true;
                rank_list.Items[rank_list.SelectedIndex] = rank_txtName.Text + " = " + (int)storedRanks[rank_list.SelectedIndex].Permission;
                rankSupressEvents = false;
            }
        }

       void rank_txtPermission_TextChanged(object sender, EventArgs e) {
            if ( rank_txtPerm.Text != "" ) {
                int foundPerm;
                if (!int.TryParse(rank_txtPerm.Text, out foundPerm)) {
                    if ( rank_txtPerm.Text != "-" )
                        rank_txtPerm.Text = rank_txtPerm.Text.Remove(rank_txtPerm.Text.Length - 1);
                    return;
                }

                if ( foundPerm < -50 ) { rank_txtPerm.Text = "-50"; return; }
                else if ( foundPerm > 119 ) { rank_txtPerm.Text = "119"; return; }

                storedRanks[rank_list.SelectedIndex].Permission = (LevelPermission)foundPerm;
                rankSupressEvents = true;
                rank_list.Items[rank_list.SelectedIndex] = storedRanks[rank_list.SelectedIndex].trueName + " = " + foundPerm;
                rankSupressEvents = false;
            }
        }

       void rank_txtLimit_TextChanged(object sender, EventArgs e) {
            if (rank_txtLimit.Text != "") {
                int drawLimit;
                if (!int.TryParse(rank_txtLimit.Text, out drawLimit)) {
                    rank_txtLimit.Text = rank_txtLimit.Text.Remove(rank_txtLimit.Text.Length - 1);
                    return;
                }

                if ( drawLimit < 1 ) { rank_txtLimit.Text = "1"; return; }

                storedRanks[rank_list.SelectedIndex].maxBlocks = drawLimit;
            }
        }

       void txtMaxUndo_TextChanged(object sender, EventArgs e) {
            if (rank_txtUndo.Text != "") {
                long maxUndo;
                if (!long.TryParse(rank_txtUndo.Text, out maxUndo)) {
                    rank_txtUndo.Text = rank_txtUndo.Text.Remove(rank_txtUndo.Text.Length - 1);
                    return;
                }

                if ( maxUndo < -1 ) { rank_txtUndo.Text = "0"; return; }

                storedRanks[rank_list.SelectedIndex].maxUndo = maxUndo;
            }
        }
        
        void rank_txtOSMaps_TextChanged(object sender, EventArgs e) {
            if (rank_txtOSMaps.Text != "") {
                byte maxMaps;
                if (!byte.TryParse(rank_txtOSMaps.Text, out maxMaps)) {
                    rank_txtOSMaps.Text = rank_txtOSMaps.Text.Remove(rank_txtOSMaps.Text.Length - 1);
                    return;
                }
                storedRanks[rank_list.SelectedIndex].OverseerMaps = maxMaps;
            }
        }
        
        void rank_txtPrefix_TextChanged(object sender, EventArgs e) {
            storedRanks[rank_list.SelectedIndex].prefix = rank_txtPrefix.Text;
        }
        
        void rank_txtMOTD_TextChanged(object sender, EventArgs e) {
            if (rank_txtMOTD.Text != null) storedRanks[rank_list.SelectedIndex].MOTD = rank_txtMOTD.Text;
        }

        void rank_btnAdd_Click(object sender, EventArgs e) {
            // Find first free rank permission
            int freePerm = 5;
            for (int i = (int)LevelPermission.Guest; i <= (int)LevelPermission.Nobody; i++) {
                if (Group.findPermInt(i) != null) continue;
                
                freePerm = i; break;
            }
            
            Group newGroup = new Group((LevelPermission)freePerm, 600, 30, "CHANGEME", '1', "", null);
            storedRanks.Add(newGroup);
            rank_list.Items.Add(newGroup.trueName + " = " + (int)newGroup.Permission);
        }

        void rank_btnDel_Click(object sender, EventArgs e) {
            if (rank_list.Items.Count <= 1) return;
            
            storedRanks.RemoveAt(rank_list.SelectedIndex);
            rankSupressEvents = true;
            rank_list.Items.RemoveAt(rank_list.SelectedIndex);
            rankSupressEvents = false;

            rank_list.SelectedIndex = 0;
        }
    }
}
