﻿using Heluo;
using Heluo.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;
using UnityEngine;
using MoonSharp.Interpreter;
using System.Collections;
using Heluo.Manager;
using static 侠之道存档修改器.EnumData;
using Heluo.Flow;
using Heluo.Tree;
using System.Text.RegularExpressions;

namespace 侠之道存档修改器
{
    public partial class Form1 : Form
    {
        private string saveFilesPath = "saveFilesPath.txt";
        private string FlagRemarkFilePath = "FlagRemark.txt";
        private string logPath = "log.txt";
        private GameData gameData = new GameData();
        private PathOfWuxiaSaveHeader pathOfWuxiaSaveHeader = new PathOfWuxiaSaveHeader();
        private DataManager Data = new DataManager();
        private bool saveFileIsSelected = false;
        private bool isSaveFileSelecting = false;

        private Dictionary<string, ComboBoxItem> dcbi = new Dictionary<string, ComboBoxItem>();
        public Form1()
        {
            try
            {
                InitializeComponent();

                Game.Data = Data;
                getConfigDatas();

                if (File.Exists(saveFilesPath))
                {
                    StreamReader sr = new StreamReader(saveFilesPath);
                    string line;

                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        SaveFilesPathTextBox.Text = line;
                    }

                    getSaveFiles();
                    sr.Close();
                }
            }
            catch(Exception ex)
            {
                if (File.Exists(logPath))
                {
                    StreamWriter sr = new StreamWriter(logPath);

                    sr.WriteLine(ex.StackTrace);
                    sr.Close();
                }
            }
            


        }

        public string getBaseFlowGraphStr(BaseFlowGraph bfg)
        {
            string str = "";

            if (bfg != null)
            {
                for (int i = 1; i < bfg.nodes.Count; i++)
                {
                    str += bfg.nodes[i] + ",";
                }
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }

        private void selectsaveFilesPathButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择存档文件夹，路径为Steam\\userdata\\xxxxxxxx\\1189630\\remote";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveFilesPathTextBox.Text = dialog.SelectedPath;
                StreamWriter sw = new StreamWriter(saveFilesPath);
                sw.WriteLine(dialog.SelectedPath);
                sw.Close();

                getSaveFiles();
            }
        }

        private void getSaveFiles()
        {
            try
            {
                DirectoryInfo folder = new DirectoryInfo(SaveFilesPathTextBox.Text);
                SaveFileListBox.Items.Clear();
                FileInfo[] fileList = folder.GetFiles().OrderBy(f => int.Parse(Regex.Match(f.Name, @"\d+").Value)).ToArray();
                for (int i = 0;i < fileList.Length;i++)
                {
                    FileInfo file = fileList[i];
                    if (file.Name.Contains("PathOfWuxia") && file.Name.Contains("save"))
                    {
                        SaveFileListBox.Items.Add(file.Name);
                    }
                }
                messageLabel.Text = "";
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
            }

        }

        private void saveFileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSaveFileSelecting = true;
            string saveFilePath = SaveFilesPathTextBox.Text + "\\" + SaveFileListBox.SelectedItem.ToString();

            FileStream readstream = File.OpenRead(saveFilePath);
            StreamReader sr = new StreamReader(readstream);

            byte[] array = new byte[17];
            sr.BaseStream.Read(array, 0, array.Length);
            if (array[0] == 239 && array[1] == 187 && array[2] == 191)
            {
                sr.BaseStream.Position = 3L;
                sr.BaseStream.Read(array, 0, array.Length);
            }

            string @string = Encoding.ASCII.GetString(array);
            if (@string == "WUXIASCHOOL_B_1_0")
            {
                pathOfWuxiaSaveHeader = LZ4MessagePackSerializer.Deserialize<PathOfWuxiaSaveHeader>(sr.BaseStream, HeluoResolver.Instance, true);
            }
            readstream.Close();


            readstream = File.OpenRead(saveFilePath);
            sr = new StreamReader(readstream);

            array = new byte[17];
            sr.BaseStream.Read(array, 0, array.Length);
            if (array[0] == 239 && array[1] == 187 && array[2] == 191)
            {
                sr.BaseStream.Position = 3L;
                sr.BaseStream.Read(array, 0, array.Length);
            }

            @string = Encoding.ASCII.GetString(array);
            if (@string == "WUXIASCHOOL_B_1_0")
            {

                LZ4MessagePackSerializer.Deserialize<PathOfWuxiaSaveHeader>(sr.BaseStream, HeluoResolver.Instance, true);
                gameData = LZ4MessagePackSerializer.Deserialize<GameData>(sr.BaseStream, HeluoResolver.Instance, true);

                Game.GameData = gameData;


                readDatas();

                saveFileIsSelected = true;
            }
            readstream.Close();

            isSaveFileSelecting = false;

            CharacterListView.Enabled = true;
        }

        private bool checkSaveFileIsSelected()
        {
            if (!saveFileIsSelected)
            {
                MessageBox.Show("请先选择一个存档");
            }
            return saveFileIsSelected;
        }

        private void getConfigDatas()
        {

            readAllMap();
            readAllRound();
            readAllGameLevel();
            readAllCharacter();
            readAllExterior();
            readAllElement();
            readAllSpecialSkill();
            readAllEquip();
            readAllSkill();
            readAllInventory();
            readAllTrait();
            readAllMantra();
            readAllGender();
            readAllQuestState();
            readShowAllQuest();
            readAllBook();
        }

        private void readAllInventory()
        {
            foreach (KeyValuePair<string, Props> kv in Data.Get<Props>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add(((EnumData.PropsType)kv.Value.PropsType).ToString());
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.PropsCategory));
                lvi.SubItems.Add(kv.Value.Price.ToString());
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.CanDeals));
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.IsShow));
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.UseTime));
                lvi.SubItems.Add(kv.Value.PropsEffectDescription.ToString());

                string canUseIds = "";
                if (kv.Value.CanUseID != null)
                {
                    for (int i = 0; i < kv.Value.CanUseID.Count; i++)
                    {
                        string canUseId = kv.Value.CanUseID[i];
                        if (canUseId == "Player")
                        {
                            canUseIds += "玩家,";
                        }
                        else
                        {
                            canUseIds += Data.Get<Npc>(canUseId).Name + ",";
                        }
                    }
                }
                else
                {
                    canUseIds = ",";
                }
                canUseIds = canUseIds.Substring(0, canUseIds.Length - 1);
                lvi.SubItems.Add(canUseIds);

                PropsListView.Items.Add(lvi);
            }

            PropsListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        private void readAllMap()
        {
            CurrentMapComboBox.DisplayMember = "value";
            CurrentMapComboBox.ValueMember = "key";
            foreach (KeyValuePair<string, Map> kv in Data.Get<Map>())
            {
                ComboBoxItem cbi = new ComboBoxItem(kv.Value.Id, kv.Value.Name);
                CurrentMapComboBox.Items.Add(cbi);
                dcbi.Add(cbi.key, cbi);
            }
        }

        private void readAllRound()
        {
            for (int i = 1; i <= 3; i++)
            {
                CurrentYearComboBox.Items.Add((EnumData.Year)i);
            }
            for (int i = 1; i <= 12; i++)
            {
                CurrentMonthComboBox.Items.Add((EnumData.Month)i);
            }
            for (int i = 1; i <= 5; i++)
            {
                CurrentRoundOfMonthComboBox.Items.Add((EnumData.RoundOfMonth)i);
            }
            for (int i = 1; i <= 2; i++)
            {
                CurrentTimeComboBox.Items.Add((EnumData.Time)i);
            }
        }

        private void readAllGameLevel()
        {
            for (int i = 1; i <= 4; i++)
            {
                GameLevelComboBox.Items.Add((EnumData.GameLevel)i);
            }
        }

        private void readAllCharacter()
        {
            foreach (KeyValuePair<string, CharacterInfo> kv in Data.Get<CharacterInfo>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                if (lvi.Text == "in0101")
                {
                    lvi.Text = "Player";
                }
                lvi.SubItems.Add(kv.Value.Remark);

                CharacterListView.Items.Add(lvi);
            }
        }

        private void readAllExterior()
        {
            foreach (KeyValuePair<string, CharacterExterior> kv in Data.Get<CharacterExterior>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                if (lvi.Text == "in0101")
                {
                    lvi.Text = "Player";
                }
                lvi.SubItems.Add(kv.Value.Remark);

                CharacterExteriorListView.Items.Add(lvi);
            }
        }

        private void readAllSkill()
        {
            SkillListView.Items.Clear();
            foreach (KeyValuePair<string, Skill> kv in Data.Get<Skill>())
            {
                if (WeaponComboBox.SelectedIndex != -1)
                {
                    string weaponId = ((ComboBoxItem)WeaponComboBox.SelectedItem).key;
                    Props prop = Data.Get<Props>(weaponId);

                    if (kv.Value.Type != prop.PropsCategory)
                    {
                        continue;
                    }
                }

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add("");
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.RequireAttribute));
                lvi.SubItems.Add(kv.Value.RequireValue.ToString());
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.Type));
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.DamageType));
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.TargetType));
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.TargetArea));
                lvi.SubItems.Add(kv.Value.MinRange + "-" + kv.Value.MaxRange);
                lvi.SubItems.Add(kv.Value.AOE.ToString());
                lvi.SubItems.Add(kv.Value.RequestMP.ToString());
                lvi.SubItems.Add(kv.Value.MaxCD.ToString());
                lvi.SubItems.Add(kv.Value.PushDistance == -1 ? "抓取" : kv.Value.PushDistance.ToString());
                if (kv.Value.Summonid != "0" && kv.Value.Summonid != string.Empty)
                {
                    string[] summonids = kv.Value.Summonid.Split(',');
                    string summonName = "";
                    for (int i = 0; i < summonids.Length; i++)
                    {
                        summonName += Data.Get<Npc>(summonids[i]).Name + ",";
                    }
                    summonName = summonName.Substring(0, summonName.Length - 1);
                    lvi.SubItems.Add(summonName);
                }
                else
                {
                    lvi.SubItems.Add("");
                }

                lvi.SubItems.Add(kv.Value.Description.ToString());

                SkillListView.Items.Add(lvi);
            }
        }

        private void readAllElement()
        {
            for (int i = 0; i <= 5; i++)
            {
                ElementComboBox.Items.Add(EnumData.GetDisplayName((Element)i));
            }
        }

        private void readAllSpecialSkill()
        {
            SpecialSkillComboBox.DisplayMember = "value";
            SpecialSkillComboBox.ValueMember = "key";
            foreach (KeyValuePair<string, Skill> skill in Data.Get<Skill>())
            {
                if (skill.Value.Id.Contains("specialskill"))
                {
                    ComboBoxItem cbi = new ComboBoxItem(skill.Value.Id, skill.Value.Name);
                    SpecialSkillComboBox.Items.Add(cbi);
                    dcbi.Add(cbi.key, cbi);
                }
            }
        }

        private void readAllEquip()
        {
            WeaponComboBox.DisplayMember = "value";
            WeaponComboBox.ValueMember = "key";
            WeaponComboBox2.DisplayMember = "value";
            WeaponComboBox2.ValueMember = "key";

            ClothComboBox.DisplayMember = "value";
            ClothComboBox.ValueMember = "key";

            JewelryComboBox.DisplayMember = "value";
            JewelryComboBox.ValueMember = "key";
            foreach (KeyValuePair<string, Props> props in Data.Get<Props>())
            {
                ComboBoxItem cbi = new ComboBoxItem(props.Value.Id, props.Value.Name + "-" + props.Value.PropsEffectDescription);
                if (props.Value.PropsType == Heluo.Data.PropsType.Weapon)
                {
                    WeaponComboBox.Items.Add(cbi);
                    WeaponComboBox2.Items.Add(cbi);
                    dcbi.Add(cbi.key, cbi);
                }
                else if (props.Value.PropsType == Heluo.Data.PropsType.Armor)
                {
                    ClothComboBox.Items.Add(cbi);
                    dcbi.Add(cbi.key, cbi);
                }
                else if (props.Value.PropsType == Heluo.Data.PropsType.Accessories)
                {
                    JewelryComboBox.Items.Add(cbi);
                    dcbi.Add(cbi.key, cbi);
                }
            }
        }

        private void readAllTrait()
        {
            foreach (KeyValuePair<string, Trait> kv in Data.Get<Trait>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add(kv.Value.Description);

                TraitListView.Items.Add(lvi);
            }
        }

        private void readAllMantra()
        {
            foreach (KeyValuePair<string, Mantra> kv in Data.Get<Mantra>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.RequireAttribute));
                lvi.SubItems.Add(kv.Value.RequireValue.ToString());

                String MantraRunEffectDescription = "";
                for (int i = 0; i < kv.Value.MantraRunEffectDescription.Count; i++)
                {
                    MantraRunEffectDescription += kv.Value.MantraRunEffectDescription[i].EffectDescription + ";";
                }
                lvi.SubItems.Add(MantraRunEffectDescription);

                MantraListView.Items.Add(lvi);
            }
        }

        private void readAllGender()
        {
            GenderComboBox.DisplayMember = "value";
            GenderComboBox.ValueMember = "key";
            foreach (Gender gender in Enum.GetValues(typeof(Gender)))
            {

                ComboBoxItem cbi = new ComboBoxItem(((int)gender).ToString(), EnumData.GetDisplayName(gender));
                GenderComboBox.Items.Add(cbi);
            }
        }

        private void readAllQuestState()
        {
            QuestStateComboBox.DisplayMember = "value";
            QuestStateComboBox.ValueMember = "key";
            foreach (QuestState questState in Enum.GetValues(typeof(QuestState)))
            {

                ComboBoxItem cbi = new ComboBoxItem(((int)questState).ToString(), questState.ToString());
                QuestStateComboBox.Items.Add(cbi);
            }
        }

        private void readShowAllQuest()
        {
            ShowAllQuestComboBox.DisplayMember = "value";
            ShowAllQuestComboBox.ValueMember = "key";
            foreach (showAllQuest showAllQuest in Enum.GetValues(typeof(showAllQuest)))
            {

                ComboBoxItem cbi = new ComboBoxItem(((int)showAllQuest).ToString(), showAllQuest.ToString());
                ShowAllQuestComboBox.Items.Add(cbi);
            }
            ShowAllQuestComboBox.SelectedIndex = 0;
        }

        private void readAllBook()
        {
            foreach (KeyValuePair<string, Book> kv in Data.Get<Book>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.BookTab));
                lvi.SubItems.Add(kv.Value.MaxReadTime.ToString());
                lvi.SubItems.Add(kv.Value.ReadConditionDescription);
                lvi.SubItems.Add(getBaseFlowGraphStr(kv.Value.ShowCondition));


                BookListView.Items.Add(lvi);
            }
        }

        private void readDatas()
        {
            initDatas();

            readCommonData();
            readInventory();
            readAllSkill();
            readCommunity();
            readParty();
            readFlag();
            readFlagLove();
            readQuest();
            readElective();
            readNurturanceOrder();
            readBook();
            readAlchemy();
            readForge();
            readShop();
        }

        private void initDatas()
        {
            HavingInventoryListView.SelectedItems.Clear();
            CharacterListView.SelectedItems.Clear();
            ElementComboBox.SelectedIndex = -1;
            SpecialSkillComboBox.SelectedIndex = -1;
            GrowthFactorTextBox.Text = "";
            HpTextBox.Text = "";
            MaxHpTextBox.Text = "";
            MpTextBox.Text = "";
            MaxMpTextBox.Text = "";
            AttackTextBox.Text = "";
            DefenseTextBox.Text = "";
            HitTextBox.Text = "";
            MoveTextBox.Text = "";
            DodgeTextBox.Text = "";
            ParryTextBox.Text = "";
            CriticalTextBox.Text = "";
            CounterTextBox.Text = "";
            AffiliationStrTextBox.Text = "";
            AffiliationTextBox.Text = "";
            WeaponComboBox.SelectedIndex = -1;
            WeaponComboBox2.SelectedIndex = -1;
            ClothComboBox.SelectedIndex = -1;
            JewelryComboBox.SelectedIndex = -1;
            StrTextBox.Text = "";
            VitTextBox.Text = "";
            DexTextBox.Text = "";
            SpiTextBox.Text = "";
            VibrantTextBox.Text = "";
            CultivatedTextBox.Text = "";
            ResoluteTextBox.Text = "";
            BraveTextBox.Text = "";
            ZitherTextBox.Text = "";
            ChessTextBox.Text = "";
            CalligraphyTextBox.Text = "";
            PaintingTextBox.Text = "";

            HavingSkillListView.Items.Clear();
            EquipSkill1Label.Text = "无";
            EquipSkill2Label.Text = "无";
            EquipSkill3Label.Text = "无";
            EquipSkill4Label.Text = "无";

            TraitListView.SelectedItems.Clear();
            HavingTraitListView.Items.Clear();

            MantraListView.SelectedItems.Clear();
            HavingMantraListView.Items.Clear();
            WorkMantraLabel.Text = "无";
            MantraCurrentLevelTextBox.Text = "";
            MantraMaxLevelTextBox.Text = "";

            CharacterExteriorListView.SelectedItems.Clear();
            SurNameTextBox.Text = "";
            NameTextBox.Text = "";
            NicknameTextBox.Text = "";
            ProtraitTextBox.Text = "";
            ModelTextBox.Text = "";
            DescriptionTextBox.Text = "";

            CommunityListView.SelectedItems.Clear();
            CommunityMaxLevelTextBox.Text = "";
            CommunityLevelTextBox.Text = "";
            CommunityExpTextBox.Text = "";
            CommunityIsOpenCheckBox.Checked = false;

            PartyListView.SelectedItems.Clear();

            FlagListView.SelectedItems.Clear();

            ctb_MasterLoveTextBox.Text = "";
            dxl_MasterLoveTextBox.Text = "";
            dh_MasterLoveTextBox.Text = "";
            ht_MasterLoveTextBox.Text = "";
            fxlh_MasterLoveTextBox.Text = "";
            lxp_MasterLoveTextBox.Text = "";
            ncc_MasterLoveTextBox.Text = "";
            tsz_MasterLoveTextBox.Text = "";
            mrx_MasterLoveTextBox.Text = "";
            j_MasterLoveTextBox.Text = "";
            xx_NpcLoveTextBox.Text = "";

            QuestListView.SelectedItems.Clear();
            QuestStateComboBox.SelectedIndex = -1;

            ElectiveListView.SelectedItems.Clear();
            CurrentElectiveLabel.Text = "";

            NurturanceOrderListView.SelectedItems.Clear();

            BookListView.SelectedItems.Clear();
            HavingBookListView.SelectedItems.Clear();

            AlchemyListView.SelectedItems.Clear();

            ForgeFightListView.SelectedItems.Clear();
            ForgeBladeAndSwordListView.SelectedItems.Clear();
            ForgeLongAndShortListView.SelectedItems.Clear();
            ForgeQimenListView.SelectedItems.Clear();
            ForgeArmorListView.SelectedItems.Clear();

            ShopListView.SelectedItems.Clear();

        }

        private void readCommonData()
        {
            GameVersionTextBox.Text = gameData.GameVersion;
            SaveTimeDateTimePicker.Text = pathOfWuxiaSaveHeader.SaveTime.ToString();
            CurrentMapComboBox.SelectedIndex = CurrentMapComboBox.Items.IndexOf(dcbi[gameData.MapId]);
            PlayerPostioionTextBox.Text = gameData.PlayerPostioion.ToString();
            PlayerForwardTextBox.Text = gameData.PlayerForward.ToString();
            CurrentYearComboBox.SelectedIndex = gameData.Round.CurrentYear - 1;
            CurrentMonthComboBox.SelectedIndex = gameData.Round.CurrentMonth - 1;
            CurrentRoundOfMonthComboBox.SelectedIndex = gameData.Round.CurrentRoundOfMonth - 1;
            CurrentTimeComboBox.SelectedIndex = gameData.Round.CurrentTime - 1;
            CurrentRoundTextBox.Text = gameData.Round.CurrentRound.ToString();
            EmotionTextBox.Text = gameData.emotion.ToString();
            MoneyTextBox.Text = gameData.Money.ToString();
            GameLevelComboBox.SelectedIndex = (int)gameData.GameLevel - 1;
        }

        private void readInventory()
        {
            HavingInventoryListView.Items.Clear();

            foreach (KeyValuePair<string, InventoryData> kv in gameData.Inventory)
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;

                lvi.SubItems.Add(Data.Get<Props>(kv.Key).Name);

                lvi.SubItems.Add(kv.Value.Count.ToString());

                HavingInventoryListView.Items.Add(lvi);
            }

            HavingInventoryListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        private void readCommunity()
        {
            CommunityListView.Items.Clear();

            foreach (KeyValuePair<string, CommunityData> kv in gameData.Community)
            {
                if (kv.Key == "Player")
                {
                    continue;
                }

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;

                lvi.SubItems.Add(gameData.Exterior[kv.Key].FullName());

                CommunityListView.Items.Add(lvi);
            }

            CommunityListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        private void readParty()
        {
            PartyListView.Items.Clear();

            foreach (string id in gameData.Party)
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = id;

                lvi.SubItems.Add(gameData.Exterior[id].FullName());

                PartyListView.Items.Add(lvi);
            }

            PartyListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        private void readFlag()
        {
            FlagListView.Items.Clear();

            foreach (KeyValuePair<string, int> kv in gameData.Flag)
            {
                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;

                lvi.SubItems.Add(kv.Value.ToString());

                FlagListView.Items.Add(lvi);
            }

            FlagListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 

            readFlagRemark();
        }

        private void readFlagRemark()
        {
            if (File.Exists(FlagRemarkFilePath))
            {
                StreamReader sr = new StreamReader(FlagRemarkFilePath);
                string line;

                // 从文件读取并显示行，直到文件的末尾 
                while ((line = sr.ReadLine()) != null)
                {
                    string[] flag = line.Split(':');
                    ListViewItem lvi = FlagListView.FindItemWithText(flag[0]);
                    if (lvi != null)
                    {
                        if (lvi.SubItems.Count < 3)
                        {
                            lvi.SubItems.Add(flag[1]);
                        }
                        else
                        {
                            lvi.SubItems[2].Text = flag[1];
                        }
                    }
                }

                sr.Close();
            }
        }

        private void readFlagLove()
        {
            ctb_MasterLoveTextBox.Text = Game.GameData.Flag["fg0201_MasterLove"].ToString();
            dxl_MasterLoveTextBox.Text = Game.GameData.Flag["fg0202_MasterLove"].ToString();
            dh_MasterLoveTextBox.Text = Game.GameData.Flag["fg0203_MasterLove"].ToString();
            lxp_MasterLoveTextBox.Text = Game.GameData.Flag["fg0204_MasterLove"].ToString();
            ht_MasterLoveTextBox.Text = Game.GameData.Flag["fg0205_MasterLove"].ToString();
            tsz_MasterLoveTextBox.Text = Game.GameData.Flag["fg0206_MasterLove"].ToString();
            fxlh_MasterLoveTextBox.Text = Game.GameData.Flag["fg0207_MasterLove"].ToString();
            ncc_MasterLoveTextBox.Text = Game.GameData.Flag["fg0208_MasterLove"].ToString();
            mrx_MasterLoveTextBox.Text = Game.GameData.Flag["fg0209_MasterLove"].ToString();
            j_MasterLoveTextBox.Text = Game.GameData.Flag["fg0210_MasterLove"].ToString();
            xx_NpcLoveTextBox.Text = Game.GameData.Flag["fg0301_NpcLove"].ToString();
        }

        private void readQuest()
        {
            QuestListView.Items.Clear();

            List<Quest> list = this.Data.Get<Quest>().Values.ToList();
            if (ShowAllQuestComboBox.SelectedIndex == 0)
            {
                list = list.FindAll((Quest x) => x.Type == QuestType.Teacher || x.Type == QuestType.EveryDay || x.Type == QuestType.Emergency || x.Type == QuestType.Working || x.Type == QuestType.Invitation);
            }


            foreach (Quest quest in list)
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = quest.Id;
                lvi.SubItems.Add(quest.Name);
                lvi.SubItems.Add(quest.Brief);
                lvi.SubItems.Add(getBaseFlowGraphStr(quest.ShowCondition));
                lvi.SubItems.Add(getBaseFlowGraphStr(quest.PickUpCondition));
                lvi.SubItems.Add(quest.DeadLine);
                lvi.SubItems.Add(((QuestSchedule)quest.Schedule).ToString());

                String EvaluationReward = "";
                if (quest.EvaluationReward != null)
                {
                    foreach (KeyValuePair<EvaluationLevel, EvaluationReward> kv in quest.EvaluationReward)
                    {
                        if (kv.Value != null)
                        {
                            if (kv.Value.Id == "Money")
                            {
                                EvaluationReward += kv.Value.Count + "钱,";
                            }
                            else if (kv.Value.Id != "")
                            {
                                EvaluationReward += Data.Get<Props>(kv.Value.Id).Name + "*" + kv.Value.Count + ",";
                            }
                        }
                    }
                    EvaluationReward = EvaluationReward.Substring(0, Math.Max(0, EvaluationReward.Length - 1));
                }
                lvi.SubItems.Add(EvaluationReward);

                QuestListView.Items.Add(lvi);
            }
        }

        private void readElective()
        {
            ElectiveListView.Items.Clear();

            foreach (KeyValuePair<string, Elective> kv in Data.Get<Elective>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Value.Id;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.Grade));
                lvi.SubItems.Add(kv.Value.ConditionDescription);
                lvi.SubItems.Add((Game.GameData.Elective.Triggered.Contains(kv.Value.Id) ? ElectiveState.已进修 : ElectiveState.未进修).ToString());


                ElectiveListView.Items.Add(lvi);
            }

            if (!string.IsNullOrEmpty(Game.GameData.Elective.Id))
            {
                string[] electives = Game.GameData.Elective.Id.Split('_');
                string electiveStr = "";
                for(int i = 0;i < electives.Length; i++)
                {
                    electiveStr += Data.Get<Elective>(electives[i]).Name+",";
                }
                electiveStr = electiveStr.Substring(0, electiveStr.Length - 1);
                CurrentElectiveLabel.Text = electiveStr;
            }
            else
            {
                CurrentElectiveLabel.Text = "无";
            }
        }

        private void readNurturanceOrder()
        {
            NurturanceOrderListView.Items.Clear();

            foreach (KeyValuePair<string, Nurturance> kv in Data.Get<Nurturance>())
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Value.Id;
                lvi.SubItems.Add(kv.Value.Name);
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.Fuction));
                lvi.SubItems.Add(EnumData.GetDisplayName(kv.Value.UIType));
                lvi.SubItems.Add(kv.Value.Emotion.ToString());
                lvi.SubItems.Add(getBaseFlowGraphStr(kv.Value.ShowCondition));
                lvi.SubItems.Add(getBaseFlowGraphStr(kv.Value.OpenCondition));
                lvi.SubItems.Add(getBaseFlowGraphStr(kv.Value.AdditionCondition));
                lvi.SubItems.Add(kv.Value.AdditionValue.ToString() + "%");


                NurturanceOrderListView.Items.Add(lvi);
            }

            Game.GameData.NurturanceOrder.CteateDevelopOrderTree();
        }
        private void readBook()
        {
            HavingBookListView.Items.Clear();

            foreach (KeyValuePair<string, BookData> kv in Game.GameData.ReadBookManager)
            {

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Value.Id;
                lvi.SubItems.Add(kv.Value.Item.Name);
                lvi.SubItems.Add(kv.Value.IsReadFinish ? "是" : "否");
                lvi.SubItems.Add(Mathf.Max(0, kv.Value.Item.MaxReadTime - kv.Value.CurrentReadTime).ToString());


                HavingBookListView.Items.Add(lvi);
            }
        }

        private void readAlchemy()
        {
            AlchemyListView.Items.Clear();

            foreach (KeyValuePair<string, Alchemy> kv in Data.Get<Alchemy>())
            {
                Props prop = Data.Get<Props>(kv.Value.PropsId);

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(prop.Name);
                lvi.SubItems.Add(prop.PropsEffectDescription);
                lvi.SubItems.Add(Game.GameData.Alchemy.Learned.Contains(kv.Key) ? "是" : "否");


                AlchemyListView.Items.Add(lvi);
            }
        }

        private void readForge()
        {
            ForgeFightListView.Items.Clear();
            ForgeBladeAndSwordListView.Items.Clear();
            ForgeLongAndShortListView.Items.Clear();
            ForgeQimenListView.Items.Clear();
            ForgeArmorListView.Items.Clear();

            foreach (KeyValuePair<string, Forge> kv in Data.Get<Forge>())
            {
                Props prop = Data.Get<Props>(kv.Value.PropsId);

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(prop.Name);
                lvi.SubItems.Add(EnumData.GetDisplayName(prop.PropsCategory));
                lvi.SubItems.Add(prop.PropsEffectDescription);
                lvi.SubItems.Add(Game.GameData.Forge.Opened.Contains(kv.Key) ? "是" : "否");
                lvi.SubItems.Add(kv.Value.OpenRound.ToString());


                switch (prop.PropsCategory)
                {
                    case Heluo.Data.PropsCategory.Fist:
                    case Heluo.Data.PropsCategory.Leg:
                        ForgeFightListView.Items.Add(lvi);
                        break;
                    case Heluo.Data.PropsCategory.Sword:
                    case Heluo.Data.PropsCategory.Blade:
                        ForgeBladeAndSwordListView.Items.Add(lvi);
                        break;
                    case Heluo.Data.PropsCategory.Long:
                    case Heluo.Data.PropsCategory.Short:
                        ForgeLongAndShortListView.Items.Add(lvi);
                        break;
                    case Heluo.Data.PropsCategory.DualWielding:
                    case Heluo.Data.PropsCategory.Special:
                        ForgeQimenListView.Items.Add(lvi);
                        break;
                    default:
                        ForgeArmorListView.Items.Add(lvi);
                        break;
                }
            }
        }

        private void readShop()
        {
            ShopListView.Items.Clear();

            foreach (KeyValuePair<string, Shop> kv in Data.Get<Shop>())
            {
                Props prop = Data.Get<Props>(kv.Value.PropsId);

                ListViewItem lvi = new ListViewItem();

                lvi.Text = kv.Key;
                lvi.SubItems.Add(prop.Name);
                lvi.SubItems.Add(prop.PropsEffectDescription);
                lvi.SubItems.Add(getBaseFlowGraphStr(kv.Value.Condition));
                lvi.SubItems.Add(kv.Value.IsRepeat ? "是" : "否");

                string ShopPeriods = "";
                for(int i = 0;i < kv.Value.ShopPeriods.Count; i++)
                {
                    ShopPeriods += kv.Value.ShopPeriods[i].OpenRound + "-" + (kv.Value.ShopPeriods[i].CloseRound-1) + ";";
                }
                ShopPeriods = ShopPeriods.Substring(0, ShopPeriods.Length - 1);
                lvi.SubItems.Add(ShopPeriods);

                if (kv.Value.ShopPeriods != null && kv.Value.ShopPeriods.Count != 0)
                {
                    for (int j = 0; j < kv.Value.ShopPeriods.Count; j++)
                    {
                        ShopPeriod shopPeriod = kv.Value.ShopPeriods[j];
                        if (shopPeriod.CheckInPeriod(Game.GameData.Round.CurrentRound))
                        {
                            bool isSholOut = Game.GameData.Shop.CheckIsSoldOut(kv.Value.Id, shopPeriod);
                                lvi.SubItems.Add(isSholOut && !kv.Value.IsRepeat ? "是" : "否");
                                break;
                        }
                    }
                }

                ShopListView.Items.Add(lvi);
            }
        }

            private void listView1_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            if (saveFileIsSelected)
            {
                InventoryAdd1button.Enabled = true;
                InventoryAdd10button.Enabled = true;
                InventorySub1button.Enabled = false;
                InventorySub10button.Enabled = false;
            }
        }

        private void listView2_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            if (saveFileIsSelected)
            {
                InventoryAdd1button.Enabled = false;
                InventoryAdd10button.Enabled = false;
                InventorySub1button.Enabled = true;
                InventorySub10button.Enabled = true;
            }
        }

        private void InventoryAdd1button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in PropsListView.SelectedItems)  //选中项遍历  
            {
                ListViewItem havinglvi = HavingInventoryListView.FindItemWithText(lvi.Text);

                if (havinglvi == null)
                {
                    havinglvi = new ListViewItem();

                    havinglvi.Text = lvi.Text;

                    havinglvi.SubItems.Add(Data.Get<Props>(lvi.Text).Name);
                    havinglvi.SubItems.Add(1.ToString());

                    this.HavingInventoryListView.Items.Add(havinglvi);
                }
                else
                {
                    ListViewSubItem si = havinglvi.SubItems[2];
                    int num = int.Parse(si.Text) + 1;
                    if (num > 99)
                    {
                        num = 99;
                    }
                    si.Text = num.ToString();
                }

                gameData.Inventory.Add(lvi.Text);
                HavingInventoryListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
                HavingInventoryListView.EnsureVisible(havinglvi.Index);
            }
        }

        private void InventoryAdd10button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in PropsListView.SelectedItems)  //选中项遍历  
            {
                ListViewItem havinglvi = HavingInventoryListView.FindItemWithText(lvi.Text);

                if (havinglvi == null)
                {
                    havinglvi = new ListViewItem();

                    havinglvi.Text = lvi.Text;

                    havinglvi.SubItems.Add(Data.Get<Props>(lvi.Text).Name);
                    havinglvi.SubItems.Add(10.ToString());

                    HavingInventoryListView.Items.Add(havinglvi);
                }
                else
                {
                    ListViewSubItem si = havinglvi.SubItems[2];
                    int num = int.Parse(si.Text) + 10;
                    if (num > 99)
                    {
                        num = 99;
                    }
                    si.Text = num.ToString();
                }

                gameData.Inventory.Add(lvi.Text, 10);
                HavingInventoryListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
                HavingInventoryListView.EnsureVisible(havinglvi.Index);
            }
        }

        private void InventorySub1button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in HavingInventoryListView.SelectedItems)  //选中项遍历  
            {
                ListViewSubItem si = lvi.SubItems[2];
                int num = int.Parse(si.Text) - 1;
                if (num < 0)
                {
                    num = 0;
                }

                si.Text = num.ToString();
                gameData.Inventory.Remove(lvi.Text);
                HavingInventoryListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。
                HavingInventoryListView.EnsureVisible(lvi.Index);
            }
        }

        private void InventorySub10button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in HavingInventoryListView.SelectedItems)  //选中项遍历  
            {
                ListViewSubItem si = lvi.SubItems[2];
                int num = int.Parse(si.Text) - 10;
                if (num < 0)
                {
                    num = 0;
                }

                si.Text = num.ToString();
                gameData.Inventory.Remove(lvi.Text, 10);
                HavingInventoryListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
                HavingInventoryListView.EnsureVisible(lvi.Index);
            }
        }

        private void createFormula(CharacterInfoData cid)
        {
            cid.CreateFormula();
            if (gameData.Community != null && gameData.Community.ContainsKey(cid.Id))
            {
                if (cid.status_coefficient_of_community == null)
                {
                    cid.status_coefficient_of_community = Game.Data.Get<GameFormula>("status_coefficient_of_community_" + cid.Id);

                    if (cid.status_coefficient_of_community != null)
                    {
                        if (cid.status_coefficient_of_community.Formula._script == null)
                        {
                            cid.status_coefficient_of_community.Formula._script = new Script(CoreModules.Math);
                        }
                        cid.status_coefficient_of_community.Formula._script.DoString(string.Concat(new string[]
                                   {
                                            "function ",
                                            cid.status_coefficient_of_community.Id,
                                            "() return ",
                                            cid.status_coefficient_of_community.Formula.Expression,
                                            " end"
                                   }), null, null);
                    }
                }
            }
            Dictionary<string, int> baseFormulaProperty = cid.GetBaseFormulaProperty();

            foreach (object obj in Enum.GetValues(typeof(CharacterProperty)))
            {
                CharacterProperty index = (CharacterProperty)obj;
                string key = string.Format("basic_{0}", index.ToString().ToLower());
                if (CharacterInfoData.PropertyFormula.ContainsKey(key))
                {
                    GameFormula gameFormula = CharacterInfoData.PropertyFormula[key];


                    if (gameFormula.Formula._script == null)
                    {
                        gameFormula.Formula._script = new Script(CoreModules.Math);
                    }
                    foreach (KeyValuePair<string, int> keyValuePair in baseFormulaProperty)
                    {
                        gameFormula.Formula._script.Globals.Set(keyValuePair.Key, DynValue.NewNumber(keyValuePair.Value));
                    }
                    gameFormula.Formula._script.DoString(string.Concat(new string[]
                       {
                                            "function ",
                                            gameFormula.Id,
                                            "() return ",
                                            gameFormula.Formula.Expression,
                                            " end"
                       }), null, null);
                }
            }
            int level = Game.GameData.Community[cid.Id].Favorability.Level;
            if (cid.CommunityFormulaProperty == null)
            {
                cid.CommunityFormulaProperty = new Dictionary<string, int>
                    {
                        {
                            "community_lv",
                            level
                        }
                    };
            }
            else if (cid.CommunityFormulaProperty.ContainsKey("community_lv"))
            {
                cid.CommunityFormulaProperty["community_lv"] = level;
            }
            else
            {
                cid.CommunityFormulaProperty.Add("community_lv", level);
            }
        }

        private void characterListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            if (checkSaveFileIsSelected())
            {
                try
                {
                    foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                    {  //选中项遍历 
                        string id = lvi.Text;
                        if (id == "in0101")
                        {
                            id = "Player";
                        }
                        CharacterInfoData cid = new CharacterInfoData();
                        if (!gameData.Character.ContainsKey(id))
                        {
                            CharacterInfo characterInfo = Data.Get<CharacterInfo>(id);
                            if (characterInfo != null)
                            {
                                cid = new CharacterInfoData(characterInfo);

                                createFormula(cid);

                                cid.OnRoundChange(gameData.Round.CurrentRound, false);
                                gameData.Character.Add(id, cid);
                            }
                        }
                        else
                        {
                            cid = gameData.Character[id];
                        }
                        createFormula(cid);
                        readSelectCharacterData(cid);
                    }
                }
                catch (Exception ex)
                {
                    messageLabel.Text = ex.Message;
                }
            }
        }

        public void readSelectCharacterData(CharacterInfoData cid)
        {
            readCharacterInfoData(cid);
            readCharacterSkillData(cid);
            updateSkillPredictionDamage(cid);
            readCharacterEquipSkillData(cid);
            readCharacterTraitData(cid);
            readCharacterMantraData(cid);
            readCharacterWorkMantraData(cid);

            SkillCurrentLevelTextBox.Text = "";
            SkillMaxLevelTextBox.Text = "";
            MantraCurrentLevelTextBox.Text = "";
            MantraMaxLevelTextBox.Text = "";
        }

        public void readCharacterInfoData(CharacterInfoData cid)
        {
            try
            {
                readCharacterProperty(cid);

                ElementComboBox.SelectedIndex = (int)cid.Element;
                if (cid.SpecialSkill != null && cid.SpecialSkill != string.Empty)
                {
                    SpecialSkillComboBox.SelectedIndex = SpecialSkillComboBox.Items.IndexOf(dcbi[cid.SpecialSkill]);
                }
                else
                {
                    SpecialSkillComboBox.SelectedIndex = -1;
                }
                GrowthFactorTextBox.Text = cid.GrowthFactor.ToString();

                StrTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Str].Value.ToString();
                VitTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Vit].Value.ToString();
                DexTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Dex].Value.ToString();
                SpiTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Spi].Value.ToString();
                VibrantTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Vibrant].Value.ToString();
                CultivatedTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Cultivated].Value.ToString();
                ResoluteTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Resolute].Value.ToString();
                BraveTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Brave].Value.ToString();
                ZitherTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Zither].Value.ToString();
                ChessTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Chess].Value.ToString();
                CalligraphyTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Calligraphy].Value.ToString();
                PaintingTextBox.Text = cid.UpgradeableProperty[CharacterUpgradableProperty.Painting].Value.ToString();

                //weaponComboBox.SelectedIndex = -1;
                if (cid.Equip[EquipType.Weapon] != null && cid.Equip[EquipType.Weapon] != string.Empty)
                {
                    WeaponComboBox.SelectedIndex = WeaponComboBox.Items.IndexOf(dcbi[cid.Equip[EquipType.Weapon]]);
                }
                else
                {
                    WeaponComboBox.SelectedIndex = -1;
                }
                //weaponComboBox2.SelectedIndex = -1;
                /*if (cid.Equip[EquipType.Weapon] != null && cid.Equip[EquipType.Weapon] != string.Empty)
                {
                    weaponComboBox2.SelectedIndex = weaponComboBox2.Items.IndexOf(dcbi[cid.Equip[EquipType.Weapon]]);
                }*/

                ClothComboBox.SelectedIndex = -1;
                if (cid.Equip[EquipType.Cloth] != null && cid.Equip[EquipType.Cloth] != string.Empty)
                {
                    ClothComboBox.SelectedIndex = ClothComboBox.Items.IndexOf(dcbi[cid.Equip[EquipType.Cloth]]);
                }

                JewelryComboBox.SelectedIndex = -1;
                if (cid.Equip[EquipType.Jewelry] != null && cid.Equip[EquipType.Jewelry] != string.Empty)
                {
                    JewelryComboBox.SelectedIndex = JewelryComboBox.Items.IndexOf(dcbi[cid.Equip[EquipType.Jewelry]]);
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
            }
        }

        private void readCharacterProperty(CharacterInfoData cid)
        {
            HpTextBox.Text = cid.HP.ToString();
            MaxHpTextBox.Text = cid.Property[CharacterProperty.Max_HP].Value.ToString();
            MpTextBox.Text = cid.MP.ToString();
            MaxMpTextBox.Text = cid.Property[CharacterProperty.Max_MP].Value.ToString();


            AttackTextBox.Text = cid.Property[CharacterProperty.Attack].Value.ToString();
            DefenseTextBox.Text = cid.Property[CharacterProperty.Defense].Value.ToString();
            HitTextBox.Text = cid.Property[CharacterProperty.Hit].Value.ToString();
            MoveTextBox.Text = cid.Property[CharacterProperty.Move].Value.ToString();
            DodgeTextBox.Text = cid.Property[CharacterProperty.Dodge].Value.ToString();
            ParryTextBox.Text = cid.Property[CharacterProperty.Parry].Value.ToString();
            CriticalTextBox.Text = cid.Property[CharacterProperty.Critical].Value.ToString();
            CounterTextBox.Text = cid.Property[CharacterProperty.Counter].Value.ToString();
            AffiliationTextBox.Text = cid.Property[CharacterProperty.Affiliation].Value.ToString();
            if (cid.Property[CharacterProperty.Affiliation].Value > 0)
            {
                AffiliationStrTextBox.Text = "楚天碧";
            }
            else if (cid.Property[CharacterProperty.Affiliation].Value < 0)
            {
                AffiliationStrTextBox.Text = "段霄烈";
            }
            else
            {
                AffiliationStrTextBox.Text = "无";
            }
        }

        public void readCharacterSkillData(CharacterInfoData cid)
        {
            HavingSkillListView.Items.Clear();
            foreach (KeyValuePair<string, SkillData> kv in cid.Skill)
            {
                if (kv.Key != "")
                {
                    if (cid.Equip[EquipType.Weapon] == null || cid.Equip[EquipType.Weapon] == "" || Data.Get<Props>(cid.Equip[EquipType.Weapon]).PropsCategory == kv.Value.Item.Type)
                    {
                        ListViewItem lvi = new ListViewItem();

                        lvi.Text = kv.Key;

                        lvi.SubItems.Add(kv.Value.Item.Name);

                        HavingSkillListView.Items.Add(lvi);
                    }
                }
            }

            HavingSkillListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        public void updateSkillPredictionDamage(CharacterInfoData cid)
        {
            foreach (ListViewItem lvi in SkillListView.Items)
            {
                Skill skill = Data.Get<Skill>(lvi.Text);

                ListViewSubItem ivsi = lvi.SubItems[2];
                //createFormula(cid);
                Dictionary<string, int> formulaProperty = cid.GetFormulaProperty();

                float coefficient = GetCoefficient(formulaProperty, skill, 10);

                int result = 0;
                switch (skill.DamageType)
                {
                    case DamageType.Damage:
                        result = Calculate(skill.Algorithm, (float)cid.Property[CharacterProperty.Attack].Value, coefficient);
                        break;
                    case DamageType.Heal:
                    case DamageType.MpRecover:
                        result = Calculate(skill.Algorithm, 0f, coefficient);
                        break;
                    case DamageType.Summon:
                        result = 0;
                        break;
                    case DamageType.Buff:
                        result = 0;
                        break;
                    case DamageType.Debuff:
                        result = 0;
                        break;
                    case DamageType.Throwing:
                        result = Calculate(skill.Algorithm, (float)cid.Property[CharacterProperty.Attack].Value, coefficient);
                        break;
                }

                ivsi.Text = result.ToString();
            }

            SkillListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        public float GetCoefficient(Dictionary<string, int> dict, Skill skill, int skill_level = 0)
        {
            float result = 0f;
            GameFormula gameFormula = null;
            gameFormula = Game.Data.Get<GameFormula>(skill.Damage);
            if (gameFormula == null)
            {
                return 0f;
            }
            int value;
            value = skill_level;
            try
            {
                if (dict.ContainsKey("slv"))
                {
                    dict["slv"] = value;
                }
                else
                {
                    dict.Add("slv", value);
                }
                if (gameFormula.Formula._script == null)
                {
                    gameFormula.Formula._script = new Script(CoreModules.Math);
                }
                foreach (KeyValuePair<string, int> keyValuePair in dict)
                {
                    gameFormula.Formula._script.Globals.Set(keyValuePair.Key, DynValue.NewNumber(keyValuePair.Value));
                }
                gameFormula.Formula._script.DoString(string.Concat(new string[]
                   {
                                            "function ",
                                            gameFormula.Id,
                                            "() return ",
                                            gameFormula.Formula.Expression,
                                            " end"
                   }), null, null);
                result = gameFormula.Evaluate(dict);
            }
            catch (Exception)
            {
                return 0f;
            }
            return result;
        }

        public int Calculate(Algorithm algorithm, float value, float coefficient)
        {
            switch (algorithm)
            {
                case Algorithm.Addition:
                    return (int)(value + coefficient);
                case Algorithm.Subtraction:
                    return (int)(value - coefficient);
                case Algorithm.Multiplication:
                    return (int)(value * coefficient);
                case Algorithm.Division:
                    return (int)(value / coefficient);
                default:
                    return 0;
            }
        }

        public void readCharacterEquipSkillData(CharacterInfoData cid)
        {
            try
            {
                string[] equipSkills = cid.GetEquipSkill();
                for (int i = 0; i < equipSkills.Length; i++)
                {
                    switch (i)
                    {
                        case 0: EquipSkill1Label.Text = equipSkills[i] == null || equipSkills[i] == string.Empty ? "无" : Data.Get<Skill>(equipSkills[i]).Name; break;
                        case 1: EquipSkill2Label.Text = equipSkills[i] == null || equipSkills[i] == string.Empty ? "无" : Data.Get<Skill>(equipSkills[i]).Name; break;
                        case 2: EquipSkill3Label.Text = equipSkills[i] == null || equipSkills[i] == string.Empty ? "无" : Data.Get<Skill>(equipSkills[i]).Name; break;
                        case 3: EquipSkill4Label.Text = equipSkills[i] == null || equipSkills[i] == string.Empty ? "无" : Data.Get<Skill>(equipSkills[i]).Name; break;
                    }
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            string saveFilePath = SaveFilesPathTextBox.Text + "\\" + SaveFileListBox.SelectedItem.ToString();


            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            FileStream writestream = new FileStream(saveFilePath, FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(writestream);
            try
            {

                if (GameConfig.GameDataHeader == "WUXIASCHOOL_B_1_0")
                {
                    byte[] bytes = Encoding.ASCII.GetBytes("WUXIASCHOOL_B_1_0");
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        memoryStream.Write(bytes, 0, bytes.Length);
                        LZ4MessagePackSerializer.Serialize(memoryStream, pathOfWuxiaSaveHeader, HeluoResolver.Instance);
                        LZ4MessagePackSerializer.Serialize(memoryStream, gameData, HeluoResolver.Instance);

                        sr.BaseStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                    }
                }
                messageLabel.Text = "保存成功";
                sr.Close();
                writestream.Close();
            }
            catch (Exception ex)
            {
                messageLabel.Text = "保存失败。" + ex.Message;
            }
            finally
            {
                sr.Close();
                writestream.Close();
            }

        }

        private void saveTimeDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            pathOfWuxiaSaveHeader.SaveTime = DateTime.Parse(SaveTimeDateTimePicker.Text);
        }

        private void currentMapComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            if (!isSaveFileSelecting)
            {
                Map map = Data.Get<Map>(((ComboBoxItem)CurrentMapComboBox.SelectedItem).key);

                gameData.SetMap(map.Id);
                gameData.PlayerPostioion = map.DefaultPosition;
                PlayerPostioionTextBox.Text = map.DefaultPosition.ToString();
            }
        }

        private void playerPostioionTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";

            PlayerPostioionTextBox.Tag = PlayerPostioionTextBox.Text;
        }

        private void playerPostioionTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {
                gameData.PlayerPostioion = stringToVector3(PlayerPostioionTextBox.Text);
                PlayerPostioionTextBox.Text = gameData.PlayerPostioion.ToString();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
                PlayerPostioionTextBox.Text = PlayerPostioionTextBox.Tag.ToString();
            }
        }

        private void playerForwardTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            PlayerForwardTextBox.Tag = PlayerForwardTextBox.Text;
        }

        private void playerForwardTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                messageLabel.Text = "";
                gameData.PlayerForward = stringToVector3(PlayerForwardTextBox.Text);
                PlayerForwardTextBox.Text = gameData.PlayerForward.ToString();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
                PlayerForwardTextBox.Text = PlayerForwardTextBox.Tag.ToString();
            }
        }

        private Vector3 stringToVector3(string str)
        {
            str = str.Replace("(", "").Replace(")", "");
            string[] s = str.Split(',');
            Vector3 v = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
            return v;
        }

        private void emotionTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            EmotionTextBox.Tag = EmotionTextBox.Text;
        }

        private void emotionTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {
                int emotion = Mathf.Clamp(int.Parse(EmotionTextBox.Text), 0, 100);
                gameData.Emotion = emotion;
                EmotionTextBox.Text = gameData.Emotion.ToString();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                EmotionTextBox.Text = EmotionTextBox.Tag.ToString();
            }
        }

        private void moneyTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            MoneyTextBox.Tag = MoneyTextBox.Text;
        }

        private void moneyTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {
                int money = Mathf.Clamp(int.Parse(MoneyTextBox.Text), 0, int.MaxValue);
                gameData.Money = money;
                MoneyTextBox.Text = gameData.Money.ToString();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MoneyTextBox.Text = MoneyTextBox.Tag.ToString();
            }
        }

        private void gameLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {
                gameData.GameLevel = (Heluo.Data.GameLevel)GameLevelComboBox.SelectedIndex + 1;
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
            }
        }

        private void refreshSaveListButton_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            getSaveFiles();
        }

        private void elementComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];
                cid.Element = (Element)ElementComboBox.SelectedIndex;
            }
        }

        private void specialSkillComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SpecialSkillComboBox.SelectedIndex != -1)
            {
                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.SpecialSkill = ((ComboBoxItem)SpecialSkillComboBox.SelectedItem).key;
                }
            }
        }

        private string oldWeaponComboBoxKey = "";
        private void weaponComboBox_TextChanged(object sender, EventArgs e)
        {
            WeaponComboBox2.SelectedIndex = WeaponComboBox.SelectedIndex;

            Props oldWeapon = Data.Get<Props>(oldWeaponComboBoxKey);
            Props newWeapon = new Props();

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                if (oldWeaponComboBoxKey != "")
                {
                    DettachPropsEffect(Data.Get<Props>(oldWeaponComboBoxKey), cid);
                }

                if (WeaponComboBox.SelectedIndex != -1)
                {
                    oldWeaponComboBoxKey = ((ComboBoxItem)WeaponComboBox.SelectedItem).key;
                    cid.Equip[EquipType.Weapon] = oldWeaponComboBoxKey;
                    AttachPropsEffect(Data.Get<Props>(oldWeaponComboBoxKey), cid);

                    newWeapon = Data.Get<Props>(oldWeaponComboBoxKey);
                }
                else
                {
                    oldWeaponComboBoxKey = "";
                    cid.Equip[EquipType.Weapon] = "";
                }

                //createFormula(cid);
                cid.UpgradeProperty(false);
                readCharacterProperty(cid);
                if (oldWeapon == null || oldWeapon.Id == "" || newWeapon == null || newWeapon.Id == "" || (oldWeapon.Id != "" && newWeapon.Id != "" && oldWeapon.PropsCategory != newWeapon.PropsCategory))
                {
                    readAllSkill();
                    updateSkillPredictionDamage(cid);
                    readCharacterSkillData(cid);
                }
                readCharacterEquipSkillData(cid);
            }
        }
        private void weaponComboBox2_TextChanged(object sender, EventArgs e)
        {
            WeaponComboBox.SelectedIndex = WeaponComboBox2.SelectedIndex;
        }

        private void AttachPropsEffect(Props porp, CharacterInfoData user)
        {
            if (porp.PropsEffect == null)
            {
                return;
            }
            for (int i = 0; i < porp.PropsEffect.Count; i++)
            {
                porp.PropsEffect[i].AttachPropsEffect(user);
            }
        }

        private void DettachPropsEffect(Props porp, CharacterInfoData user)
        {
            if (porp.PropsEffect == null)
            {
                return;
            }
            for (int i = 0; i < porp.PropsEffect.Count; i++)
            {
                porp.PropsEffect[i].DettachPropsEffect(user);
            }
        }

        private string oldClothComboBoxKey = "";
        private void clothComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];


                if (oldClothComboBoxKey != "")
                {
                    DettachPropsEffect(Data.Get<Props>(oldClothComboBoxKey), cid);
                }
                if (ClothComboBox.SelectedIndex != -1)
                {
                    oldClothComboBoxKey = ((ComboBoxItem)ClothComboBox.SelectedItem).key;
                    cid.Equip[EquipType.Cloth] = ((ComboBoxItem)ClothComboBox.SelectedItem).key;

                    AttachPropsEffect(Data.Get<Props>(oldClothComboBoxKey), cid);

                }
                else
                {
                    oldClothComboBoxKey = "";
                }

                //createFormula(cid);
                cid.UpgradeProperty(false);
                readCharacterProperty(cid);
            }
        }

        private string oldJewelryComboBoxKy = "";
        private void jewelryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                if (oldJewelryComboBoxKy != "")
                {
                    DettachPropsEffect(Data.Get<Props>(oldJewelryComboBoxKy), cid);
                }
                if (JewelryComboBox.SelectedIndex != -1)
                {
                    oldJewelryComboBoxKy = ((ComboBoxItem)JewelryComboBox.SelectedItem).key;
                    cid.Equip[EquipType.Jewelry] = ((ComboBoxItem)JewelryComboBox.SelectedItem).key;
                    AttachPropsEffect(Data.Get<Props>(oldJewelryComboBoxKy), cid);

                }
                else
                {
                    oldJewelryComboBoxKy = "";
                }

                //createFormula(cid);
                cid.UpgradeProperty(false);
                readCharacterProperty(cid);
            }
        }

        private void GrowthFactorTextBox_GotFocus(object sender, EventArgs e)
        {
            GrowthFactorTextBox.Tag = GrowthFactorTextBox.Text;
        }

        private void GrowthFactorTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (GrowthFactorTextBox.Text == string.Empty)
                {
                    GrowthFactorTextBox.Text = "0";
                }

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.GrowthFactor = float.Parse(GrowthFactorTextBox.Text);

                    GrowthFactorTextBox.Text = cid.GrowthFactor.ToString();
                }

            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                GrowthFactorTextBox.Text = GrowthFactorTextBox.Tag.ToString();
            }
        }

        private void hpTextBox_GotFocus(object sender, EventArgs e)
        {

            HpTextBox.Tag = HpTextBox.Text;
        }

        private void hpTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (HpTextBox.Text == string.Empty)
                {
                    HpTextBox.Text = "1";
                }

                int hp = Mathf.Clamp(int.Parse(HpTextBox.Text), 1, int.Parse(MaxHpTextBox.Text));

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.HP = hp;
                }
                HpTextBox.Text = hp.ToString();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                HpTextBox.Text = HpTextBox.Tag.ToString();
            }
        }

        private void maxHpTextBox_GotFocus(object sender, EventArgs e)
        {
            MaxHpTextBox.Tag = MaxHpTextBox.Text;
        }

        private void maxHpTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                int maxHp = 1;

                if (MaxHpTextBox.Text == string.Empty)
                {
                    maxHp = 1;
                }

                maxHp = Mathf.Clamp(int.Parse(MaxHpTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Max_HP].Base = maxHp - cid.Property[CharacterProperty.Max_HP].Equip_Attach - cid.Property[CharacterProperty.Max_HP].Four_Attribute_Attach;

                    int hp = Mathf.Clamp(cid.HP, 1, maxHp);
                    cid.HP = hp;

                    MaxHpTextBox.Text = maxHp.ToString();
                    HpTextBox.Text = hp.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MaxHpTextBox.Text = MaxHpTextBox.Tag.ToString();
            }
        }

        private void AffiliationTextBox_GotFocus(object sender, EventArgs e)
        {
            AffiliationTextBox.Tag = AffiliationTextBox.Text;
        }

        private void AffiliationTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (AffiliationTextBox.Text == string.Empty)
                {
                    AffiliationTextBox.Text = "0";
                }

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    int affiliation = int.Parse(AffiliationTextBox.Text);

                    cid.Property[CharacterProperty.Affiliation].Base = affiliation;

                    AffiliationTextBox.Text = affiliation.ToString();

                    if (cid.Property[CharacterProperty.Affiliation].Value > 0)
                    {
                        AffiliationStrTextBox.Text = "楚天碧";
                    }
                    else if (cid.Property[CharacterProperty.Affiliation].Value < 0)
                    {
                        AffiliationStrTextBox.Text = "段霄烈";
                    }
                    else
                    {
                        AffiliationStrTextBox.Text = "无";
                    }
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                AffiliationTextBox.Text = AffiliationTextBox.Tag.ToString();
            }
        }

        private void mpTextBox_GotFocus(object sender, EventArgs e)
        {

            MpTextBox.Tag = MpTextBox.Text;
        }

        private void mpTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (MpTextBox.Text == string.Empty)
                {
                    MpTextBox.Text = "1";
                }

                int mp = Mathf.Clamp(int.Parse(MpTextBox.Text), 1, int.Parse(MaxMpTextBox.Text));

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.MP = mp;
                }
                MpTextBox.Text = mp.ToString();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MpTextBox.Text = MpTextBox.Tag.ToString();
            }
        }

        private void maxMpTextBox_GotFocus(object sender, EventArgs e)
        {
            MaxMpTextBox.Tag = MaxMpTextBox.Text;
        }

        private void maxMpTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (MaxMpTextBox.Text == string.Empty)
                {
                    MaxMpTextBox.Text = "1";
                }

                int maxMp = Mathf.Clamp(int.Parse(MaxMpTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Max_MP].Base = maxMp - cid.Property[CharacterProperty.Max_MP].Equip_Attach - cid.Property[CharacterProperty.Max_MP].Four_Attribute_Attach;

                    int mp = Mathf.Clamp(cid.MP, 1, maxMp);
                    cid.MP = mp;

                    MaxMpTextBox.Text = maxMp.ToString();
                    MpTextBox.Text = mp.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MaxMpTextBox.Text = MaxMpTextBox.Tag.ToString();
            }
        }

        private void strTextBox_GotFocus(object sender, EventArgs e)
        {
            StrTextBox.Tag = StrTextBox.Text;
        }

        private void strTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (StrTextBox.Text == string.Empty)
                {
                    StrTextBox.Text = "0";
                }

                int str = Mathf.Clamp(int.Parse(StrTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Str].Level = str;

                    StrTextBox.Text = str.ToString();

                    //createFormula(cid);
                    cid.UpgradeProperty(false);
                    readCharacterProperty(cid);
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                StrTextBox.Text = StrTextBox.Tag.ToString();
            }
        }

        private void vitTextBox_GotFocus(object sender, EventArgs e)
        {
            VitTextBox.Tag = VitTextBox.Text;
        }

        private void vitTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (VitTextBox.Text == string.Empty)
                {
                    VitTextBox.Text = "0";
                }

                int vit = Mathf.Clamp(int.Parse(VitTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Vit].Level = vit;

                    VitTextBox.Text = vit.ToString();

                    //createFormula(cid);
                    cid.UpgradeProperty(false);
                    readCharacterProperty(cid);
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                VitTextBox.Text = VitTextBox.Tag.ToString();
            }
        }

        private void dexTextBox_GotFocus(object sender, EventArgs e)
        {
            DexTextBox.Tag = DexTextBox.Text;
        }

        private void dexTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (DexTextBox.Text == string.Empty)
                {
                    DexTextBox.Text = "0";
                }

                int dex = Mathf.Clamp(int.Parse(DexTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Dex].Level = dex;

                    DexTextBox.Text = dex.ToString();

                    //createFormula(cid);
                    cid.UpgradeProperty(false);
                    readCharacterProperty(cid);
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                DexTextBox.Text = DexTextBox.Tag.ToString();
            }
        }

        private void spiTextBox_GotFocus(object sender, EventArgs e)
        {
            SpiTextBox.Tag = SpiTextBox.Text;
        }

        private void spiTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (SpiTextBox.Text == string.Empty)
                {
                    SpiTextBox.Text = "0";
                }

                int spi = Mathf.Clamp(int.Parse(SpiTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Spi].Level = spi;

                    SpiTextBox.Text = spi.ToString();

                    //createFormula(cid);
                    cid.UpgradeProperty(false);
                    readCharacterProperty(cid);
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                SpiTextBox.Text = SpiTextBox.Tag.ToString();
            }
        }

        private void attackTextBox_GotFocus(object sender, EventArgs e)
        {
            AttackTextBox.Tag = AttackTextBox.Text;
        }

        private void attackTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (AttackTextBox.Text == string.Empty)
                {
                    AttackTextBox.Text = "0";
                }

                int attack = Mathf.Clamp(int.Parse(AttackTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Attack].Base = attack - cid.Property[CharacterProperty.Attack].Equip_Attach - cid.Property[CharacterProperty.Attack].Four_Attribute_Attach;

                    AttackTextBox.Text = attack.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                AttackTextBox.Text = AttackTextBox.Tag.ToString();
            }
        }

        private void defenseTextBox_GotFocus(object sender, EventArgs e)
        {
            DefenseTextBox.Tag = DefenseTextBox.Text;
        }

        private void defenseTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (DefenseTextBox.Text == string.Empty)
                {
                    DefenseTextBox.Text = "0";
                }

                int defense = Mathf.Clamp(int.Parse(DefenseTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Defense].Base = defense - cid.Property[CharacterProperty.Defense].Equip_Attach - cid.Property[CharacterProperty.Defense].Four_Attribute_Attach;

                    DefenseTextBox.Text = defense.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                DefenseTextBox.Text = DefenseTextBox.Tag.ToString();
            }
        }

        private void hitTextBox_GotFocus(object sender, EventArgs e)
        {
            HitTextBox.Tag = HitTextBox.Text;
        }

        private void hitTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (HitTextBox.Text == string.Empty)
                {
                    HitTextBox.Text = "0";
                }

                int hit = Mathf.Clamp(int.Parse(HitTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Hit].Base = hit - cid.Property[CharacterProperty.Hit].Equip_Attach - cid.Property[CharacterProperty.Hit].Four_Attribute_Attach;

                    HitTextBox.Text = hit.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                HitTextBox.Text = HitTextBox.Tag.ToString();
            }
        }

        private void moveTextBox_GotFocus(object sender, EventArgs e)
        {
            MoveTextBox.Tag = MoveTextBox.Text;
        }

        private void moveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (MoveTextBox.Text == string.Empty)
                {
                    MoveTextBox.Text = "0";
                }

                int move = Mathf.Clamp(int.Parse(MoveTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Move].Base = move - cid.Property[CharacterProperty.Move].Equip_Attach - cid.Property[CharacterProperty.Move].Four_Attribute_Attach;

                    MoveTextBox.Text = move.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MoveTextBox.Text = MoveTextBox.Tag.ToString();
            }
        }

        private void dodgeTextBox_GotFocus(object sender, EventArgs e)
        {
            DodgeTextBox.Tag = DodgeTextBox.Text;
        }

        private void dodgeTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (DodgeTextBox.Text == string.Empty)
                {
                    DodgeTextBox.Text = "0";
                }

                int dodge = Mathf.Clamp(int.Parse(DodgeTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Dodge].Base = dodge - cid.Property[CharacterProperty.Dodge].Equip_Attach - cid.Property[CharacterProperty.Dodge].Four_Attribute_Attach;

                    DodgeTextBox.Text = dodge.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                DodgeTextBox.Text = DodgeTextBox.Tag.ToString();
            }
        }

        private void parryTextBox_GotFocus(object sender, EventArgs e)
        {
            ParryTextBox.Tag = ParryTextBox.Text;
        }

        private void parryTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ParryTextBox.Text == string.Empty)
                {
                    ParryTextBox.Text = "0";
                }

                int parry = Mathf.Clamp(int.Parse(ParryTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Parry].Base = parry - cid.Property[CharacterProperty.Parry].Equip_Attach - cid.Property[CharacterProperty.Parry].Four_Attribute_Attach;

                    ParryTextBox.Text = parry.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ParryTextBox.Text = ParryTextBox.Tag.ToString();
            }
        }

        private void criticalTextBox_GotFocus(object sender, EventArgs e)
        {
            CriticalTextBox.Tag = CriticalTextBox.Text;
        }

        private void criticalTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CriticalTextBox.Text == string.Empty)
                {
                    CriticalTextBox.Text = "0";
                }

                int critical = Mathf.Clamp(int.Parse(CriticalTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Critical].Base = critical - cid.Property[CharacterProperty.Critical].Equip_Attach - cid.Property[CharacterProperty.Critical].Four_Attribute_Attach;

                    CriticalTextBox.Text = critical.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CriticalTextBox.Text = CriticalTextBox.Tag.ToString();
            }
        }

        private void counterTextBox_GotFocus(object sender, EventArgs e)
        {
            CounterTextBox.Tag = CounterTextBox.Text;
        }

        private void counterTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CounterTextBox.Text == string.Empty)
                {
                    CounterTextBox.Text = "0";
                }

                int counter = Mathf.Clamp(int.Parse(CounterTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.Property[CharacterProperty.Counter].Base = counter - cid.Property[CharacterProperty.Counter].Equip_Attach - cid.Property[CharacterProperty.Counter].Four_Attribute_Attach;

                    CounterTextBox.Text = counter.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CounterTextBox.Text = CounterTextBox.Tag.ToString();
            }
        }

        private void VibrantTextBox_GotFocus(object sender, EventArgs e)
        {
            VibrantTextBox.Tag = VibrantTextBox.Text;
        }

        private void VibrantTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (VibrantTextBox.Text == string.Empty)
                {
                    VibrantTextBox.Text = "0";
                }

                int vibrant = Mathf.Clamp(int.Parse(VibrantTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Vibrant].Level = vibrant;

                    VibrantTextBox.Text = vibrant.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                VibrantTextBox.Text = VibrantTextBox.Tag.ToString();
            }
        }

        private void CultivatedTextBox_GotFocus(object sender, EventArgs e)
        {
            CultivatedTextBox.Tag = CultivatedTextBox.Text;
        }

        private void CultivatedTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CultivatedTextBox.Text == string.Empty)
                {
                    CultivatedTextBox.Text = "0";
                }

                int Cultivated = Mathf.Clamp(int.Parse(CultivatedTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Cultivated].Level = Cultivated;

                    CultivatedTextBox.Text = Cultivated.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CultivatedTextBox.Text = CultivatedTextBox.Tag.ToString();
            }
        }

        private void ResoluteTextBox_GotFocus(object sender, EventArgs e)
        {
            ResoluteTextBox.Tag = ResoluteTextBox.Text;
        }

        private void ResoluteTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ResoluteTextBox.Text == string.Empty)
                {
                    ResoluteTextBox.Text = "0";
                }

                int Resolute = Mathf.Clamp(int.Parse(ResoluteTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Resolute].Level = Resolute;

                    ResoluteTextBox.Text = Resolute.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ResoluteTextBox.Text = ResoluteTextBox.Tag.ToString();
            }
        }

        private void BraveTextBox_GotFocus(object sender, EventArgs e)
        {
            BraveTextBox.Tag = BraveTextBox.Text;
        }

        private void BraveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (BraveTextBox.Text == string.Empty)
                {
                    BraveTextBox.Text = "0";
                }

                int Brave = Mathf.Clamp(int.Parse(BraveTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Brave].Level = Brave;

                    BraveTextBox.Text = Brave.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                BraveTextBox.Text = BraveTextBox.Tag.ToString();
            }
        }

        private void ZitherTextBox_GotFocus(object sender, EventArgs e)
        {
            ZitherTextBox.Tag = ZitherTextBox.Text;
        }

        private void ZitherTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ZitherTextBox.Text == string.Empty)
                {
                    ZitherTextBox.Text = "0";
                }

                int Zither = Mathf.Clamp(int.Parse(ZitherTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Zither].Level = Zither;

                    ZitherTextBox.Text = Zither.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ZitherTextBox.Text = ZitherTextBox.Tag.ToString();
            }
        }

        private void ChessTextBox_GotFocus(object sender, EventArgs e)
        {
            ChessTextBox.Tag = ChessTextBox.Text;
        }

        private void ChessTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ChessTextBox.Text == string.Empty)
                {
                    ChessTextBox.Text = "0";
                }

                int Chess = Mathf.Clamp(int.Parse(ChessTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Chess].Level = Chess;

                    ChessTextBox.Text = Chess.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ChessTextBox.Text = ChessTextBox.Tag.ToString();
            }
        }

        private void CalligraphyTextBox_GotFocus(object sender, EventArgs e)
        {
            CalligraphyTextBox.Tag = CalligraphyTextBox.Text;
        }

        private void CalligraphyTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CalligraphyTextBox.Text == string.Empty)
                {
                    CalligraphyTextBox.Text = "0";
                }

                int Calligraphy = Mathf.Clamp(int.Parse(CalligraphyTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Calligraphy].Level = Calligraphy;

                    CalligraphyTextBox.Text = Calligraphy.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CalligraphyTextBox.Text = CalligraphyTextBox.Tag.ToString();
            }
        }

        private void PaintingTextBox_GotFocus(object sender, EventArgs e)
        {
            PaintingTextBox.Tag = PaintingTextBox.Text;
        }

        private void PaintingTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (PaintingTextBox.Text == string.Empty)
                {
                    PaintingTextBox.Text = "0";
                }

                int Painting = Mathf.Clamp(int.Parse(PaintingTextBox.Text), 0, int.MaxValue);

                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    cid.UpgradeableProperty[CharacterUpgradableProperty.Painting].Level = Painting;

                    PaintingTextBox.Text = Painting.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                PaintingTextBox.Text = PaintingTextBox.Tag.ToString();
            }
        }

        private void skillListView_GotFocus(object sender, EventArgs e)
        {
            LearnSkillButton.Enabled = true;
            AbolishSkillButton.Enabled = false;
            SetSkill1Button.Enabled = false;
            SetSkill2Button.Enabled = false;
            SetSkill3Button.Enabled = false;
            SetSkill4Button.Enabled = false;
            SkillCurrentLevelTextBox.Enabled = false;
            SkillMaxLevelTextBox.Enabled = false;
        }

        private void havingSkillListView_GotFocus(object sender, EventArgs e)
        {
            LearnSkillButton.Enabled = false;
            AbolishSkillButton.Enabled = true;
            SetSkill1Button.Enabled = true;
            SetSkill2Button.Enabled = true;
            SetSkill3Button.Enabled = true;
            SetSkill4Button.Enabled = true;
            SkillCurrentLevelTextBox.Enabled = true;
            SkillMaxLevelTextBox.Enabled = true;
        }

        private void learnSkillButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem skillLvi in SkillListView.SelectedItems)
                {
                    cid.LearnSkill(skillLvi.Text);
                }
                readCharacterSkillData(cid);
                readCharacterEquipSkillData(cid);
            }
        }

        private void abolishSkillButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                {
                    cid.ReplaceSkill(skillLvi.Text, "");
                }
                readCharacterSkillData(cid);
                readCharacterEquipSkillData(cid);
            }
        }

        private void havingSkillListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                {
                    SkillData sd = cid.Skill[skillLvi.Text];

                    SkillCurrentLevelTextBox.Text = sd.Level.ToString();
                    SkillMaxLevelTextBox.Text = sd.MaxLevel.ToString();
                }
            }
        }

        private void setSkill1Button_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                Props weapon = cid.Equip.GetEquip(EquipType.Weapon);
                if (weapon == null)
                {
                    if (MessageBox.Show("未选择武器的情况下，将默认设定为拳法的技能", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                        {
                            cid.SetEquipSkill(SkillColumn.Skill01, skillLvi.Text);
                            readCharacterEquipSkillData(cid);
                        }
                    }
                }

            }
        }

        private void setSkill2Button_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                Props weapon = cid.Equip.GetEquip(EquipType.Weapon);
                if (weapon == null)
                {
                    if (MessageBox.Show("未选择武器的情况下，将默认设定为拳法的技能", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                        {
                            cid.SetEquipSkill(SkillColumn.Skill02, skillLvi.Text);
                            readCharacterEquipSkillData(cid);
                        }
                    }
                }
            }
        }

        private void setSkill3Button_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                Props weapon = cid.Equip.GetEquip(EquipType.Weapon);
                if (weapon == null)
                {
                    if (MessageBox.Show("未选择武器的情况下，将默认设定为拳法的技能", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                        {
                            cid.SetEquipSkill(SkillColumn.Skill03, skillLvi.Text);
                            readCharacterEquipSkillData(cid);
                        }
                    }
                }
            }
        }

        private void setSkill4Button_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                Props weapon = cid.Equip.GetEquip(EquipType.Weapon);
                if (weapon == null)
                {
                    if (MessageBox.Show("未选择武器的情况下，将默认设定为拳法的技能", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                        {
                            cid.SetEquipSkill(SkillColumn.Skill04, skillLvi.Text);
                            readCharacterEquipSkillData(cid);
                        }
                    }
                }
            }
        }
        private void skillCurrentLevelTextBox_GotFocus(object sender, EventArgs e)
        {
            SkillCurrentLevelTextBox.Tag = SkillCurrentLevelTextBox.Text;
        }

        private void skillCurrentLevelTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (SkillCurrentLevelTextBox.Text == string.Empty)
                {
                    SkillCurrentLevelTextBox.Text = "1";
                }
                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                    {
                        int level = Mathf.Clamp(int.Parse(SkillCurrentLevelTextBox.Text), 1, int.MaxValue);

                        cid.GetSkill(skillLvi.Text).Level = level;

                        SkillCurrentLevelTextBox.Text = cid.GetSkill(skillLvi.Text).Level.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                SkillCurrentLevelTextBox.Text = SkillCurrentLevelTextBox.Tag.ToString();
            }
        }

        private void skillMaxLevelTextBox_GotFocus(object sender, EventArgs e)
        {
            SkillMaxLevelTextBox.Tag = SkillMaxLevelTextBox.Text;
        }

        private void skillMaxLevelTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (SkillMaxLevelTextBox.Text == string.Empty)
                {
                    SkillMaxLevelTextBox.Text = "1";
                }
                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    foreach (ListViewItem skillLvi in HavingSkillListView.SelectedItems)
                    {
                        int MaxLevel = Mathf.Clamp(int.Parse(SkillMaxLevelTextBox.Text), 1, int.MaxValue);

                        cid.GetSkill(skillLvi.Text).MaxLevel = MaxLevel;

                        SkillMaxLevelTextBox.Text = cid.GetSkill(skillLvi.Text).MaxLevel.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                SkillMaxLevelTextBox.Text = SkillMaxLevelTextBox.Tag.ToString();
            }
        }

        private void AddTraitButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem traitLvi in TraitListView.SelectedItems)
                {
                    cid.LearnTrait(traitLvi.Text);
                }
                readCharacterTraitData(cid);
            }
        }

        private void AbolishTraitButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem traitLvi in HavingTraitListView.SelectedItems)
                {
                    cid.AbolishTrait(traitLvi.Text);
                }
                readCharacterTraitData(cid);
            }
        }

        public void readCharacterTraitData(CharacterInfoData cid)
        {
            HavingTraitListView.Items.Clear();
            foreach (KeyValuePair<string, TraitData> kv in cid.Trait)
            {
                if (kv.Key != "")
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Text = kv.Key;

                    lvi.SubItems.Add(kv.Value.Item.Name);

                    HavingTraitListView.Items.Add(lvi);
                }
            }

            HavingTraitListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }

        private void TraitListView_GotFocus(object sender, EventArgs e)
        {
            AddTraitButton.Enabled = true;
            AbolishTraitButton.Enabled = false;
        }

        private void HavingTraitListView_GotFocus(object sender, EventArgs e)
        {
            AddTraitButton.Enabled = false;
            AbolishTraitButton.Enabled = true;
        }
        public void readCharacterMantraData(CharacterInfoData cid)
        {
            HavingMantraListView.Items.Clear();
            foreach (KeyValuePair<string, MantraData> kv in cid.Mantra)
            {
                if (kv.Key != "")
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Text = kv.Key;

                    lvi.SubItems.Add(kv.Value.Item.Name);

                    HavingMantraListView.Items.Add(lvi);
                }
            }

            HavingMantraListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
        }
        public void readCharacterWorkMantraData(CharacterInfoData cid)
        {
            try
            {
                String WorkMantra = cid.WorkMantra;
                WorkMantraLabel.Text = WorkMantra == null || WorkMantra == string.Empty ? "无" : Data.Get<Mantra>(WorkMantra).Name;
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;
            }
        }

        private void MantraListView_GotFocus(object sender, EventArgs e)
        {
            LearnMantraButton.Enabled = true;
            AbolishMantraButton.Enabled = false;
        }

        private void HavingMantraListView_GotFocus(object sender, EventArgs e)
        {
            LearnMantraButton.Enabled = false;
            AbolishMantraButton.Enabled = true;
        }

        private void LearnMantraButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem mantraLvi in MantraListView.SelectedItems)
                {
                    cid.LearnMantra(mantraLvi.Text);
                }
                readCharacterMantraData(cid);
            }
        }

        private void AbolishMantraButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem mantraLvi in HavingMantraListView.SelectedItems)
                {
                    cid.AbolishMantra(mantraLvi.Text);
                }
                readCharacterMantraData(cid);
            }
        }

        private void SetWorkMantraButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem mantraLvi in HavingMantraListView.SelectedItems)
                {
                    cid.WorkMantra = mantraLvi.Text;
                    readCharacterWorkMantraData(cid);
                }

            }
        }

        private void HavingMantraListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CharacterListView.SelectedItems)
            {
                CharacterInfoData cid = gameData.Character[lvi.Text];

                foreach (ListViewItem mantraLvi in HavingMantraListView.SelectedItems)
                {
                    MantraData md = cid.Mantra[mantraLvi.Text];

                    MantraCurrentLevelTextBox.Text = md.Level.ToString();
                    MantraMaxLevelTextBox.Text = md.MaxLevel.ToString();
                }
            }
        }

        private void MantraCurrentLevelTextBox_GotFocus(object sender, EventArgs e)
        {
            MantraCurrentLevelTextBox.Tag = MantraCurrentLevelTextBox.Text;
        }

        private void MantraCurrentLevelTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (MantraCurrentLevelTextBox.Text == string.Empty)
                {
                    MantraCurrentLevelTextBox.Text = "1";
                }
                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    foreach (ListViewItem mantraLvi in HavingMantraListView.SelectedItems)
                    {
                        int level = Mathf.Clamp(int.Parse(MantraCurrentLevelTextBox.Text), 1, int.MaxValue);
                        cid.GetMantra(mantraLvi.Text).Level = level;

                        MantraCurrentLevelTextBox.Text = cid.GetMantra(mantraLvi.Text).Level.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MantraCurrentLevelTextBox.Text = MantraCurrentLevelTextBox.Tag.ToString();
            }
        }

        private void MantraMaxLevelTextBox_GotFocus(object sender, EventArgs e)
        {
            MantraMaxLevelTextBox.Tag = MantraMaxLevelTextBox.Text;
        }

        private void MantraMaxLevelTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (MantraMaxLevelTextBox.Text == string.Empty)
                {
                    MantraMaxLevelTextBox.Text = "1";
                }
                foreach (ListViewItem lvi in CharacterListView.SelectedItems)
                {
                    CharacterInfoData cid = gameData.Character[lvi.Text];

                    foreach (ListViewItem mantraLvi in HavingMantraListView.SelectedItems)
                    {
                        int MaxLevel = Mathf.Clamp(int.Parse(MantraMaxLevelTextBox.Text), 1, int.MaxValue);
                        cid.GetMantra(mantraLvi.Text).MaxLevel = MaxLevel;

                        MantraMaxLevelTextBox.Text = cid.GetMantra(mantraLvi.Text).MaxLevel.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                MantraMaxLevelTextBox.Text = MantraMaxLevelTextBox.Tag.ToString();
            }
        }

        private void characterExteriorListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            if (checkSaveFileIsSelected())
            {
                try
                {
                    foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                    {  //选中项遍历 
                        string id = lvi.Text;
                        if (id == "in0101")
                        {
                            id = "Player";
                        }
                        CharacterExteriorData ced = new CharacterExteriorData();
                        if (!gameData.Exterior.ContainsKey(id))
                        {
                            CharacterExterior characterExterior = Data.Get<CharacterExterior>(id);
                            if (characterExterior != null)
                            {
                                ced = new CharacterExteriorData(characterExterior);
                                gameData.Exterior.Add(id, ced);
                            }
                        }
                        else
                        {
                            ced = gameData.Exterior[id];
                        }
                        readSelectCharacterExteriorData(ced);
                    }
                }
                catch (Exception ex)
                {
                    messageLabel.Text = ex.Message;
                }
            }
        }

        public void readSelectCharacterExteriorData(CharacterExteriorData ced)
        {
            SurNameTextBox.Text = ced.SurName;
            NameTextBox.Text = ced.Name;
            NicknameTextBox.Text = ced.Nickname;
            ProtraitTextBox.Text = ced.Protrait;
            ModelTextBox.Text = ced.Model;
            GenderComboBox.SelectedIndex = (int)ced.Gender;
            DescriptionTextBox.Text = ced.Description;
        }

        private void SurNameTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            SurNameTextBox.Tag = SurNameTextBox.Text;
        }

        private void SurNameTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {

                foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                {
                    CharacterExteriorData ced = gameData.Exterior[lvi.Text];

                    ced.SurName = SurNameTextBox.Text;
                    SurNameTextBox.Text = ced.SurName;
                }

                readCommunity();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                SurNameTextBox.Text = SurNameTextBox.Tag.ToString();
            }
        }

        private void NameTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            NameTextBox.Tag = NameTextBox.Text;
        }

        private void NameTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {

                foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                {
                    CharacterExteriorData ced = gameData.Exterior[lvi.Text];

                    ced.Name = NameTextBox.Text;
                    NameTextBox.Text = ced.Name;
                }
                readCommunity();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                NameTextBox.Text = NameTextBox.Tag.ToString();
            }
        }

        private void NicknameTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            NicknameTextBox.Tag = NicknameTextBox.Text;
        }

        private void NicknameTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {

                foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                {
                    CharacterExteriorData ced = gameData.Exterior[lvi.Text];

                    ced.Nickname = NicknameTextBox.Text;
                    NicknameTextBox.Text = ced.Nickname;
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                NicknameTextBox.Text = NicknameTextBox.Tag.ToString();
            }
        }

        private void ProtraitTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            ProtraitTextBox.Tag = ProtraitTextBox.Text;
        }

        private void ProtraitTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {

                foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                {
                    CharacterExteriorData ced = gameData.Exterior[lvi.Text];

                    ced.Protrait = ProtraitTextBox.Text;
                    ProtraitTextBox.Text = ced.Protrait;
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ProtraitTextBox.Text = ProtraitTextBox.Tag.ToString();
            }
        }

        private void ModelTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            ModelTextBox.Tag = ModelTextBox.Text;
        }

        private void ModelTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            if (string.IsNullOrEmpty(ModelTextBox.Text))
            {
                messageLabel.Text = "模型编号不可为空";

                ModelTextBox.Text = ModelTextBox.Tag.ToString();
                return;
            }
            try
            {

                foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                {
                    CharacterExteriorData ced = gameData.Exterior[lvi.Text];

                    ced.Model = ModelTextBox.Text;
                    ModelTextBox.Text = ced.Model;

                    foreach (KeyValuePair<string, CharacterExterior> kv in Data.Get<CharacterExterior>())
                    {
                        if (kv.Value.Model == ced.Model)
                        {
                            ced.Gender = kv.Value.Gender;
                            GenderComboBox.SelectedIndex = (int)ced.Gender;
                            break;
                        }
                    }
                    messageLabel.Text = "未找到该模型编号";

                    ModelTextBox.Text = ModelTextBox.Tag.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ModelTextBox.Text = ModelTextBox.Tag.ToString();
            }
        }

        private void DescriptionTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            DescriptionTextBox.Tag = DescriptionTextBox.Text;
        }

        private void DescriptionTextBox_LostFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            try
            {

                foreach (ListViewItem lvi in CharacterExteriorListView.SelectedItems)
                {
                    CharacterExteriorData ced = gameData.Exterior[lvi.Text];

                    ced.Description = DescriptionTextBox.Text;
                    DescriptionTextBox.Text = ced.Description;
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                DescriptionTextBox.Text = DescriptionTextBox.Tag.ToString();
            }
        }

        private void CommunityListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CommunityListView.SelectedItems)
            {
                CommunityData cd = gameData.Community[lvi.Text];

                CommunityLevelTextBox.Text = cd.Favorability.Level.ToString();
                CommunityMaxLevelTextBox.Text = cd.Favorability.MaxLevel.ToString();
                CommunityExpTextBox.Text = cd.Favorability.Exp.ToString();
                CommunityIsOpenCheckBox.Checked = cd.isOpen;
            }
        }

        private void CommunityListView_GotFocus(object sender, EventArgs e)
        {
            AddPartyButton.Enabled = true;
            RemovePartyButton.Enabled = false;
        }

        private void PartyListView_GotFocus(object sender, EventArgs e)
        {
            AddPartyButton.Enabled = false;
            RemovePartyButton.Enabled = true;
        }

        private void CommunityMaxLevelTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            CommunityMaxLevelTextBox.Tag = CommunityMaxLevelTextBox.Text;
        }

        private void CommunityMaxLevelTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CommunityMaxLevelTextBox.Text == string.Empty)
                {
                    CommunityMaxLevelTextBox.Text = "1";
                }

                int maxLevel = Mathf.Clamp(int.Parse(CommunityMaxLevelTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CommunityListView.SelectedItems)
                {
                    CommunityData cd = gameData.Community[lvi.Text];

                    cd.Favorability.MaxLevel = maxLevel;
                    CommunityMaxLevelTextBox.Text = cd.Favorability.MaxLevel.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CommunityMaxLevelTextBox.Text = CommunityMaxLevelTextBox.Tag.ToString();
            }
        }

        private void CommunityLevelTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            CommunityLevelTextBox.Tag = CommunityLevelTextBox.Text;
        }

        private void CommunityLevelTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CommunityLevelTextBox.Text == string.Empty)
                {
                    CommunityLevelTextBox.Text = "1";
                }

                int Level = Mathf.Clamp(int.Parse(CommunityLevelTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CommunityListView.SelectedItems)
                {
                    CommunityData cd = gameData.Community[lvi.Text];

                    cd.Favorability.Level = Level;
                    CommunityLevelTextBox.Text = cd.Favorability.Level.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CommunityLevelTextBox.Text = CommunityLevelTextBox.Tag.ToString();
            }
        }

        private void CommunityExpTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            CommunityExpTextBox.Tag = CommunityExpTextBox.Text;
        }

        private void CommunityExpTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (CommunityExpTextBox.Text == string.Empty)
                {
                    CommunityExpTextBox.Text = "1";
                }

                int Exp = Mathf.Clamp(int.Parse(CommunityExpTextBox.Text), 1, int.MaxValue);

                foreach (ListViewItem lvi in CommunityListView.SelectedItems)
                {
                    CommunityData cd = gameData.Community[lvi.Text];

                    cd.Favorability.Exp = Exp;
                    CommunityExpTextBox.Text = cd.Favorability.Exp.ToString();
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                CommunityExpTextBox.Text = CommunityExpTextBox.Tag.ToString();
            }
        }

        private void CommunityIsOpenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CommunityListView.SelectedItems)
            {
                CommunityData cd = gameData.Community[lvi.Text];

                cd.isOpen = CommunityIsOpenCheckBox.Checked;

                if (CommunityIsOpenCheckBox.Checked)
                {
                    Game.GameData.NurturanceOrder.OpenCommunityOrder(lvi.Text);
                }
                else
                {
                    Game.GameData.NurturanceOrder.CloseCommunityOrder(lvi.Text);
                }
            }
        }

        private void AddPartyButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CommunityListView.SelectedItems)
            {
                Game.GameData.Party.AddParty(lvi.Text, false);
            }
            readParty();
        }

        private void RemovePartyButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in PartyListView.SelectedItems)
            {
                Game.GameData.Party.RemoveParty(lvi.Text);
            }
            readParty();
        }

        private void PartyListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in PartyListView.SelectedItems)
            {
                if (lvi.Text == "Player")
                {
                    RemovePartyButton.Enabled = false;
                }
                else
                {
                    RemovePartyButton.Enabled = true;
                }
            }
        }

        private void FlagAdd1Button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in FlagListView.SelectedItems)  //选中项遍历  
            {
                Game.GameData.Flag[lvi.Text] += 1;
                lvi.SubItems[1].Text = Game.GameData.Flag[lvi.Text].ToString();
            }
            FlagListView.EndUpdate();
            readFlagLove();
        }

        private void FlagAdd10Button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in FlagListView.SelectedItems)  //选中项遍历  
            {
                Game.GameData.Flag[lvi.Text] += 10;
                lvi.SubItems[1].Text = Game.GameData.Flag[lvi.Text].ToString();
            }
            FlagListView.EndUpdate();
            readFlagLove();
        }

        private void FlagSub1Button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in FlagListView.SelectedItems)  //选中项遍历  
            {
                Game.GameData.Flag[lvi.Text] -= 1;
                lvi.SubItems[1].Text = Game.GameData.Flag[lvi.Text].ToString();
            }
            FlagListView.EndUpdate();
            readFlagLove();
        }

        private void FlagSub10Button_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            foreach (ListViewItem lvi in FlagListView.SelectedItems)  //选中项遍历  
            {
                Game.GameData.Flag[lvi.Text] -= 10;
                lvi.SubItems[1].Text = Game.GameData.Flag[lvi.Text].ToString();
            }
            FlagListView.EndUpdate();
            readFlagLove();
        }

        private void ctb_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            ctb_MasterLoveTextBox.Tag = ctb_MasterLoveTextBox.Text;
        }

        private void ctb_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ctb_MasterLoveTextBox.Text == string.Empty)
                {
                    ctb_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(ctb_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0201_MasterLove"] = love;

                ctb_MasterLoveTextBox.Text = Game.GameData.Flag["fg0201_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ctb_MasterLoveTextBox.Text = ctb_MasterLoveTextBox.Tag.ToString();
            }
        }

        private void dxl_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            dxl_MasterLoveTextBox.Tag = dxl_MasterLoveTextBox.Text;
        }

        private void dxl_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (dxl_MasterLoveTextBox.Text == string.Empty)
                {
                    dxl_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(dxl_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0202_MasterLove"] = love;

                dxl_MasterLoveTextBox.Text = Game.GameData.Flag["fg0202_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                dxl_MasterLoveTextBox.Text = dxl_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void dh_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            dh_MasterLoveTextBox.Tag = dh_MasterLoveTextBox.Text;
        }

        private void dh_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (dh_MasterLoveTextBox.Text == string.Empty)
                {
                    dh_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(dh_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0203_MasterLove"] = love;

                dh_MasterLoveTextBox.Text = Game.GameData.Flag["fg0203_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                dh_MasterLoveTextBox.Text = dh_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void lxp_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            lxp_MasterLoveTextBox.Tag = lxp_MasterLoveTextBox.Text;
        }

        private void lxp_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (lxp_MasterLoveTextBox.Text == string.Empty)
                {
                    lxp_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(lxp_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0204_MasterLove"] = love;

                lxp_MasterLoveTextBox.Text = Game.GameData.Flag["fg0204_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                lxp_MasterLoveTextBox.Text = lxp_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void ht_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            ht_MasterLoveTextBox.Tag = ht_MasterLoveTextBox.Text;
        }

        private void ht_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ht_MasterLoveTextBox.Text == string.Empty)
                {
                    ht_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(ht_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0205_MasterLove"] = love;

                ht_MasterLoveTextBox.Text = Game.GameData.Flag["fg0205_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ht_MasterLoveTextBox.Text = ht_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void tsz_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            tsz_MasterLoveTextBox.Tag = tsz_MasterLoveTextBox.Text;
        }

        private void tsz_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (tsz_MasterLoveTextBox.Text == string.Empty)
                {
                    tsz_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(tsz_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0206_MasterLove"] = love;

                tsz_MasterLoveTextBox.Text = Game.GameData.Flag["fg0206_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                tsz_MasterLoveTextBox.Text = tsz_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void fxlh_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            fxlh_MasterLoveTextBox.Tag = fxlh_MasterLoveTextBox.Text;
        }

        private void fxlh_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (fxlh_MasterLoveTextBox.Text == string.Empty)
                {
                    fxlh_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(fxlh_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0207_MasterLove"] = love;

                fxlh_MasterLoveTextBox.Text = Game.GameData.Flag["fg0207_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                fxlh_MasterLoveTextBox.Text = fxlh_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void ncc_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            ncc_MasterLoveTextBox.Tag = ncc_MasterLoveTextBox.Text;
        }

        private void ncc_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (ncc_MasterLoveTextBox.Text == string.Empty)
                {
                    ncc_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(ncc_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0208_MasterLove"] = love;

                ncc_MasterLoveTextBox.Text = Game.GameData.Flag["fg0208_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                ncc_MasterLoveTextBox.Text = ncc_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void mrx_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            mrx_MasterLoveTextBox.Tag = mrx_MasterLoveTextBox.Text;
        }

        private void mrx_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (mrx_MasterLoveTextBox.Text == string.Empty)
                {
                    mrx_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(mrx_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0209_MasterLove"] = love;

                mrx_MasterLoveTextBox.Text = Game.GameData.Flag["fg0209_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                mrx_MasterLoveTextBox.Text = mrx_MasterLoveTextBox.Tag.ToString();
            }

        }

        private void j_MasterLoveTextBox_GotFocus(object sender, EventArgs e)
        {

            messageLabel.Text = "";
            j_MasterLoveTextBox.Tag = j_MasterLoveTextBox.Text;
        }

        private void j_MasterLoveTextBox_LostFocus(object sender, EventArgs e)
        {

            try
            {
                if (j_MasterLoveTextBox.Text == string.Empty)
                {
                    j_MasterLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(j_MasterLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0210_MasterLove"] = love;

                j_MasterLoveTextBox.Text = Game.GameData.Flag["fg0210_MasterLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                j_MasterLoveTextBox.Text = j_MasterLoveTextBox.Tag.ToString();
            }
        }

        private void xx_NpcLoveTextBox_GotFocus(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            xx_NpcLoveTextBox.Tag = xx_NpcLoveTextBox.Text;

        }

        private void xx_NpcLoveTextBox_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (xx_NpcLoveTextBox.Text == string.Empty)
                {
                    xx_NpcLoveTextBox.Text = "0";
                }

                int love = Mathf.Clamp(int.Parse(xx_NpcLoveTextBox.Text), 0, int.MaxValue);

                Game.GameData.Flag["fg0301_NpcLove"] = love;

                xx_NpcLoveTextBox.Text = Game.GameData.Flag["fg0301_NpcLove"].ToString();

                readFlag();
            }
            catch (Exception ex)
            {
                messageLabel.Text = ex.Message;

                xx_NpcLoveTextBox.Text = xx_NpcLoveTextBox.Tag.ToString();
            }

        }

        private void QuestListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in QuestListView.SelectedItems)
            {
                if (Game.GameData.Quest.IsInProgress(lvi.Text))
                {
                    QuestStateComboBox.SelectedIndex = 1;
                }
                else if (Game.GameData.Quest.IsPassed(lvi.Text))
                {
                    QuestStateComboBox.SelectedIndex = 2;
                }
                else
                {
                    QuestStateComboBox.SelectedIndex = 0;
                }
            }
        }

        private void ShowAllQuestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isSaveFileSelecting)
            {
                readQuest();
            }
        }

        private void QuestChangeState1Button_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in QuestListView.SelectedItems)
            {
                if (Game.GameData.Quest.TrackedKind != QuestManager.QuestKind.None && Game.GameData.Quest.InProgress[(int)Game.GameData.Quest.TrackedKind] == lvi.Text)
                {
                    Game.GameData.Quest.InProgress[(int)Game.GameData.Quest.TrackedKind] = "";
                }
                Game.GameData.Quest.Passed.Remove(lvi.Text);
                QuestStateComboBox.SelectedIndex = 0;
            }
        }

        private void QuestChangeState2Button_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in QuestListView.SelectedItems)
            {
                if (Game.GameData.Quest.TrackedKind != QuestManager.QuestKind.None)
                {
                    Game.GameData.Quest.InProgress[(int)Game.GameData.Quest.TrackedKind] = lvi.Text;
                }
                Game.GameData.Quest.Passed.Remove(lvi.Text);
                QuestStateComboBox.SelectedIndex = 1;
            }
        }

        private void QuestChangeState3Button_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in QuestListView.SelectedItems)
            {
                if (Game.GameData.Quest.TrackedKind != QuestManager.QuestKind.None && Game.GameData.Quest.InProgress[(int)Game.GameData.Quest.TrackedKind] == lvi.Text)
                {
                    Game.GameData.Quest.InProgress[(int)Game.GameData.Quest.TrackedKind] = "";
                }
                if (!Game.GameData.Quest.Passed.Contains(lvi.Text))
                {
                    Game.GameData.Quest.Passed.Add(lvi.Text);
                }
                QuestStateComboBox.SelectedIndex = 2;
            }
        }

        private void SetCurrentElectiveButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ElectiveListView.SelectedItems)
            {
                Game.GameData.Elective.Id = lvi.Text;
                if (!Data.Get<Elective>(lvi.Text).IsRepeat)
                {
                    if (!Game.GameData.Elective.Triggered.Contains(lvi.Text))
                    {
                        Game.GameData.Elective.Triggered.Add(lvi.Text);
                    }
                }
            }

            readElective();
        }

        private void NurturanceOrderListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in NurturanceOrderListView.SelectedItems)
            {
                NurturanceOrderStateTextBox.Text = getNurturanceOrderContain(Game.GameData.NurturanceOrder.Root, lvi.Text) ? "开启" : "关闭";
            }
        }

        private void NurturanceOrderOpenButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in NurturanceOrderListView.SelectedItems)
            {
                Game.GameData.NurturanceOrder.OpenOrder(lvi.Text);
                NurturanceOrderStateTextBox.Text = getNurturanceOrderContain(Game.GameData.NurturanceOrder.Root, lvi.Text) ? "开启" : "关闭";
            }

        }
        private void NurturanceOrderCloseButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in NurturanceOrderListView.SelectedItems)
            {
                Game.GameData.NurturanceOrder.CloseOrder(lvi.Text);
                NurturanceOrderStateTextBox.Text = getNurturanceOrderContain(Game.GameData.NurturanceOrder.Root, lvi.Text) ? "开启" : "关闭";
            }
        }

        public bool getNurturanceOrderContain(Tree<Nurturance> root, string nurturanceId)
        {
            bool isContrain = false;
            for (int i = 0; i < root.Children.Count; i++)
            {
                if (root.Children[i].Value.Id == nurturanceId)
                {
                    isContrain = true;
                    break;
                }
                isContrain = getNurturanceOrderContain(root.Children[i], nurturanceId);
                if (isContrain)
                {
                    break;
                }
            }
            return isContrain;
        }

        private void BookListView_GotFocus(object sender, EventArgs e)
        {
            AddBookButton.Enabled = true;
            RemoveBookButton.Enabled = false;
        }

        private void HavingBookListView_GotFocus(object sender, EventArgs e)
        {
            AddBookButton.Enabled = false;
            RemoveBookButton.Enabled = true;
        }

        private void AddBookButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in BookListView.SelectedItems)
            {
                Game.GameData.ReadBookManager.GetBook(lvi.Text);
            }
            readBook();
        }

        private void RemoveBookButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in HavingBookListView.SelectedItems)
            {
                Game.GameData.ReadBookManager.Remove(lvi.Text);
            }
            readBook();
        }

        private void LearnAlchemyButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in AlchemyListView.SelectedItems)
            {
                Game.GameData.Alchemy.Learn(lvi.Text);
            }
            readAlchemy();
        }

        private void AbolishAlchemyButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in AlchemyListView.SelectedItems)
            {
                Game.GameData.Alchemy.Learned.Remove(lvi.Text);
            }
            readAlchemy();
        }

        private void OpenForgeFightButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ForgeFightListView.SelectedItems)
            {
                Game.GameData.Forge.Open(lvi.Text);
            }
            readForge();
        }

        private void CloseForgeFightButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ForgeFightListView.SelectedItems)
            {
                Game.GameData.Forge.Opened.Remove(lvi.Text);
            }
            readForge();
        }

        private void OpenForgeBladeAndSwordButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeBladeAndSwordListView.SelectedItems)
            {
                Game.GameData.Forge.Open(lvi.Text);
            }
            readForge();
        }

        private void CloseForgeBladeAndSwordButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeBladeAndSwordListView.SelectedItems)
            {
                Game.GameData.Forge.Opened.Remove(lvi.Text);
            }
            readForge();
        }

        private void OpenForgeLongAndShortButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeLongAndShortListView.SelectedItems)
            {
                Game.GameData.Forge.Open(lvi.Text);
            }
            readForge();
        }

        private void CloseForgeLongAndShortButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ForgeLongAndShortListView.SelectedItems)
            {
                Game.GameData.Forge.Opened.Remove(lvi.Text);
            }
            readForge();

        }

        private void OpenForgeQimenButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeQimenListView.SelectedItems)
            {
                Game.GameData.Forge.Open(lvi.Text);
            }
            readForge();
        }

        private void CloseForgeQimenButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeQimenListView.SelectedItems)
            {
                Game.GameData.Forge.Opened.Remove(lvi.Text);
            }
            readForge();
        }

        private void OpenForgeArmorButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeArmorListView.SelectedItems)
            {
                Game.GameData.Forge.Open(lvi.Text);
            }
            readForge();
        }

        private void CloseForgeArmorButton_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in ForgeArmorListView.SelectedItems)
            {
                Game.GameData.Forge.Opened.Remove(lvi.Text);
            }
            readForge();
        }

        private void AddShopButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ShopListView.SelectedItems)
            {
                ShopSoldOutInfo shopSoldOutInfo = Game.GameData.Shop.SoldOuts.Find((ShopSoldOutInfo x) => x.SoldOutId == lvi.Text);
                Game.GameData.Shop.SoldOuts.Remove(shopSoldOutInfo);
            }
            readShop();
        }

        private void RemoveShopButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ShopListView.SelectedItems)
            {
                ShopSoldOutInfo item = new ShopSoldOutInfo
                {
                    SoldOutId = lvi.Text,
                    SoldOutRound = Game.GameData.Round.CurrentRound
                };
                Game.GameData.Shop.SoldOuts.Add(item);
            }
            readShop();
        }

        private void SearchFlagButton_Click(object sender, EventArgs e)
        {
            SearchResultLabel.Text = "";
            string searchFlag = SearchFlagTextBox.Text;
            int index = 0;
            if(FlagListView.SelectedItems.Count != 0)
            {
                index = FlagListView.SelectedItems[0].Index + 1;
            }
            if(index == FlagListView.Items.Count)
            {
                index = 0;
            }
            ListViewItem lvi = FlagListView.FindItemWithText(searchFlag, false,index);
            if(lvi != null)
            {
                FlagListView.Items[lvi.Index].Selected = true;
                FlagListView.EnsureVisible(lvi.Index);
            }
            else
            {
                lvi = FlagListView.FindItemWithText(searchFlag, false, 0); if (lvi != null)
                {
                    FlagListView.Items[lvi.Index].Selected = true;
                    FlagListView.EnsureVisible(lvi.Index);
                }
                else
                {
                    SearchResultLabel.Text = "未找到该旗标";
                }
            }
        }
    }
}