using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Mahjong.Forms;
using Mahjong.Brands;
using Mahjong.Players;
using Mahjong.AIs;
using System.Diagnostics;

namespace Mahjong.Control
{
    public partial class ProgramControl
    {
        /// <summary>
        /// 桌面介面
        /// </summary>
        Table table;
        /// <summary>
        /// 網路連線介面
        /// </summary>
        ChatServerForm chat;
        /// <summary>
        /// 換到下一家的計時器
        /// </summary>
        Timer roundTimer;
        /// <summary>
        /// 全部玩家和桌面
        /// </summary>
        AllPlayers all;
        /// <summary>
        /// AI介面
        /// </summary>
        MahjongAI Ai;
        /// <summary>
        /// 資訊盒
        /// </summary>
        Information information;
        /// <summary>
        /// 設定盒
        /// </summary>
        Config con;
        /// <summary>
        /// 牌工廠
        /// </summary>
        BrandFactory factory;
        /// <summary>
        /// 吃碰牌之後是否要補牌
        /// </summary>
        bool Chow_Pong_Brand;
        /// <summary>
        /// 玩家按下過水
        /// </summary>
        bool Player_Pass_Brand;
        /// <summary>
        /// 是否要顯示提示訊息
        /// </summary>
        bool showMessageBox;
        public ProgramControl()
        {
            roundTimer = new Timer();
            table = new Table(this);
            information = new Information();
            con = new Config(table);
            factory = new BrandFactory();
            roundTimer.Tick += new EventHandler(rotateTimer_Tick);
            showMessageBox = table.SetCheck;
            roundTimer.Interval = Mahjong.Properties.Settings.Default.RunRoundTime_Normal;            
            table.ShowDialog();
        }

        void rotateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                round();
            }
            catch (FlowOverException)
            {
                // 流局
                MessageBox.Show(Mahjong.Properties.Settings.Default.FlowEnd);
                table.cleanImage();
                factory = new BrandFactory();
                all.nextWiner(true);                
                // 新局
                newgame2();
            }
            catch (ErrorBrandPlayerCountException)
            {
                MessageBox.Show(Mahjong.Properties.Settings.Default.ErrorBrandPlayer);
            }
        }
        /// <summary>
        /// 使用者按下一張牌
        /// </summary>
        /// <param name="brand">按下的牌</param>
        internal void makeBrand(Brand brand)
        {
            // 把牌打到桌面上看是否有人要 胡 槓 碰 吃
            if (pushToTable(brand))
            {
                // 換下一個人
                all.next();
                setInforamtion();
            }
            // 計時器重新啟動
            roundTimer.Start();
        }

        /// <summary>
        /// 玩家按下吃事件呼叫
        /// </summary>
        internal void chow(Brand brand)
        {
            Check c = new Check(brand, NowPlayer_removeTeam);
            if (c.Chow())
                if (c.ChowLength == 1)
                {
                    PlayerSort p = new PlayerSort(c.SuccessPlayer);
                    all.chow_pong(brand, p.getPlayer);
                }
                else
                {
                    ChowBrandCheck cbc = new ChowBrandCheck(c.ChowPlayer);
                    cbc.ShowDialog();
                    PlayerSort p = new PlayerSort(cbc.SelectBrandPlayer);
                    all.chow_pong(brand, p.getPlayer);
                }
            Chow_Pong_Brand = true;
            updatePlayer_Table();
        }
        /// <summary>
        /// 玩家按下碰事件呼叫
        /// </summary>
        internal void pong(Brand brand)
        {
            Check c = new Check(brand, NowPlayer_removeTeam);
            if (c.Pong())
                all.chow_pong(brand, c.SuccessPlayer);
            Chow_Pong_Brand = true;
            updatePlayer_Table();
        }
        /// <summary>
        /// 玩家按下槓事件呼叫
        /// </summary>
        internal void kong(Brand brand)
        {
            Check c = new Check(brand, NowPlayer_removeTeam);
            Check d = new Check(brand, all.NowPlayer);
            if (c.Kong())
                all.kong(brand, c.SuccessPlayer);
            else if (d.Kong())
                all.kong(brand, d.SuccessPlayer);
            updatePlayer_Table();

        }
        /// <summary>
        /// 玩家按下暗槓事件呼叫
        /// </summary>
        internal void dark_kong(Brand brand)
        {
            Check c = new Check(brand, NowPlayer_removeTeam);
            Check d = new Check(NowPlayer_removeTeam);
            if (c.Kong())
                all.DarkKong(brand, c.SuccessPlayer);
            else if (d.DarkKong())
                all.DarkKong(brand, d.SuccessPlayer);
        }
        /// <summary>
        /// 玩家按下胡事件呼叫
        /// </summary>
        internal void win(Brand brand)
        {
            win_game(brand);
        }
        /// <summary>
        /// 玩家按下過水事件呼叫
        /// </summary>
        /// <param name="brand">牌</param>
        internal void pass(Brand brand)
        {
            Player_Pass_Brand = true;
        }

        /// <summary>
        /// 程式結束
        /// </summary>
        internal void exit()
        {
            Application.Exit();
        }
        /// <summary>
        /// About box
        /// </summary>
        internal void about()
        {
            new AboutBox();
        }
        /// <summary>
        /// config Box
        /// </summary>
        internal void config()
        {
            con.Dispose();
            con = new Config(table);
            con.Show();
        }
        /// <summary>
        /// 設定顯示資訊
        /// </summary>
        internal void setInforamtion()
        {
            information.setAllPlayers(all);
            information.DebugMode = table.ShowAll;
            information.Show();
        }
        /// <summary>
        /// 移除掉已經打出去的牌組，以牌組編號來區分
        /// </summary>
        BrandPlayer NowPlayer_removeTeam
        {
            get
            {
                BrandPlayer bp = new BrandPlayer();
                for (int i = 0; i < all.NowPlayer.getCount(); i++)
                    if (all.NowPlayer.getBrand(i).Team < 1)
                        bp.add(all.NowPlayer.getBrand(i));
                return bp;
            }
        }
        /// <summary>
        /// 只有牌組
        /// </summary>
        BrandPlayer NowPlayer_OnlyTeam
        {
            get
            {
                BrandPlayer bp = new BrandPlayer();
                for (int i = 0; i < all.NowPlayer.getCount(); i++)
                    if (all.NowPlayer.getBrand(i).Team > 1)
                        bp.add(all.NowPlayer.getBrand(i));
                return bp;
            }
        }

        /// <summary>
        /// 更新現在的玩家和桌面
        /// </summary>
        void updatePlayer_Table()
        {
            table.updateNowPlayer();
            table.updateTable();
        }
        /// <summary>
        /// 連線設定
        /// </summary>
        internal void onlineGame()
        {
            chat = new ChatServerForm();
            chat.Show();
        }
        /// <summary>
        /// 提示資訊是否開啟
        /// </summary>
        internal bool ShowMessageBox
        {
            set
            {
                showMessageBox = value;
            }
        }
        /// <summary>
        /// 設定延遲時間
        /// </summary>
        internal int SetDealyTime
        {
            set 
            {
                roundTimer.Interval = value;
            }
        }

    }
}
