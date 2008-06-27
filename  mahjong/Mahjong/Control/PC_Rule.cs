using System;
using System.Collections.Generic;
using System.Text;
using Mahjong.Brands;
using System.Windows.Forms;
using Mahjong.Forms;
using Mahjong.Players;

namespace Mahjong.Control
{
    public partial class ProgramControl
    {
        /// <summary>
        /// 摸牌
        /// </summary>
        void touchBrand()
        {
            table.updateNowPlayer();
            // 摸牌給現在的玩家
            Brand nextbrand = all.nextBrand();
            // 補花並加上一張牌
            if (all.Player_setFlower(nextbrand))
            {
                touchBrand();
            }
            else
            {
                // 是否胡牌或槓牌(手牌加摸到的牌)
                Check win = new Check(nextbrand, all.NowPlayer);
                // 除去顯示牌看是否有暗槓(移除牌組的牌加摸到的牌)
                Check kong = new Check(nextbrand, NowPlayer_removeTeam);
                // 除去顯示或打出的牌看是否有暗槓
                Check darkkong = new Check(NowPlayer_removeTeam);
                // 只有牌組和摸進來的牌做比較
                Check teamKong = new Check(nextbrand, NowPlayer_OnlyTeam);
                if (win.Win())
                {
                    // 更新資訊盒
                    setInforamtion();
                    if (showMessageBox)
                        MessageBox.Show(Mahjong.Properties.Settings.Default.TouchWin, all.Name[all.state].ToString());
                    win_game(nextbrand);
                }
                // 手牌暗槓(摸到有暗槓和手牌中就有暗槓)
                else if (darkkong.DarkKong() || kong.Kong())
                {
                    // 如果是玩家
                    if (all.State == location.South)
                    {
                        Brand br = null;
                        if (darkkong.DarkKong())
                        {
                            br = darkkong.SuccessPlayer.getBrand(0);
                        }
                        else if (kong.Kong())
                        {
                            br = kong.SuccessPlayer.getBrand(0);
                        }

                        toUser(br, false, false, false, kong.Kong() || darkkong.DarkKong(), false);
                        // 如果玩家按下過水 就跳過
                        if (Player_Pass_Brand)
                            Player_Pass_Brand = false;
                        else
                        {
                            table.updateNowPlayer();
                            touchBrand();
                        }
                    }
                    // 如果不是玩家
                    else
                    {
                        if (showMessageBox)
                            MessageBox.Show(Mahjong.Properties.Settings.Default.DarkKong, all.Name[all.state].ToString());
                        if (kong.Kong())
                            all.DarkKong(nextbrand, kong.SuccessPlayer);
                        else if (darkkong.DarkKong())
                            all.DarkKong(nextbrand, darkkong.SuccessPlayer);
                        table.updateNowPlayer();
                        touchBrand();
                    }
                }
                // 明碰之後再槓 
                else if (teamKong.Kong())
                {
                    if (all.State == location.South)
                    {
                        toUser(nextbrand, false, false, teamKong.Kong(), false, false);
                        if (Player_Pass_Brand)
                            Player_Pass_Brand = false;
                        else
                        {
                            table.updateNowPlayer();
                            touchBrand();
                        }
                    }
                    else
                    {
                        if (showMessageBox)
                            MessageBox.Show(Mahjong.Properties.Settings.Default.Kong, all.Name[all.state].ToString());
                        all.kong(nextbrand, darkkong.SuccessPlayer);
                        table.updateNowPlayer();
                        touchBrand();
                    }
                }
                else
                {
                    // 把牌加入玩家
                    all.NowPlayer.add(nextbrand);
                    table.updateNowPlayer();
                }
            }

        }

        /// <summary>
        /// 打牌
        /// </summary>
        /// <param name="brand">準備要打的牌</param>
        bool pushToTable(Brand brand)
        {
            // 把牌從現在的玩家手上移除
            all.NowPlayer.remove(brand);
            // 放到桌面上
            all.PushToTable(brand);
            // 排序現在的玩家
            all.sortNowPlayer();
            // 更新現在玩家和桌面
            updatePlayer_Table();
            // 看是否有人要 胡 槓 碰 吃
            return check_chow_pong_kong_win(brand);
        }
        /// <summary>
        /// 是否有人要 胡 槓 碰 吃
        /// </summary>
        /// <param name="brand">打出到桌面的牌</param>
        /// <returns>是否被拿走了</returns>
        bool check_chow_pong_kong_win(Brand brand)
        {
            // 有沒有人要胡
            for (int i = 0; i < 3; i++)
            {
                all.next();
                Check w = new Check(brand, all.NowPlayer);
                Ai.setPlayer(brand, all.NowPlayer);
                if (w.Win())
                {
                    // 如果是玩家
                    if (all.State == location.South)
                    {
                        toUser(brand, false, false, false, false, true);
                        // 如果玩家按下過水 就跳過
                        if (Player_Pass_Brand)
                            Player_Pass_Brand = false;
                        // 若不是按下過水就傳回 失敗
                        else
                            return false;
                    }
                    else if (Ai.Win)
                    {
                        // 更新資訊盒
                        setInforamtion();
                        if (showMessageBox)
                            MessageBox.Show(Mahjong.Properties.Settings.Default.Win, all.Name[all.state].ToString());
                        win_game(brand);
                        return false;
                    }
                }
            }
            all.next();

            // 測試是否被 槓 碰
            for (int i = 0; i < 3; i++)
            {
                all.next();
                Check c = new Check(brand, NowPlayer_removeTeam);
                Check w = new Check(brand, all.NowPlayer);
                Ai.setPlayer(brand, all.NowPlayer);
                // 如果是玩家
                if (all.State == location.South)
                {
                    if (c.Pong() || c.Kong())
                    {
                        toUser(brand, (c.Chow() && i == 0), c.Pong(), c.Kong(), false, w.Win());
                        if (Player_Pass_Brand)
                            Player_Pass_Brand = false;
                        else
                            return false;
                    }
                }
                else
                {
                    // 槓
                    if (c.Kong() && Ai.Kong)
                    {
                        setInforamtion();
                        if (showMessageBox)
                            MessageBox.Show(Mahjong.Properties.Settings.Default.Kong, all.Name[all.state].ToString());
                        all.kong(brand, c.SuccessPlayer);
                        Chow_Pong_Brand = false;
                        updatePlayer_Table();
                        return false;
                    }
                    // 碰
                    else if (c.Pong() && Ai.Pong)
                    {
                        setInforamtion();
                        if (showMessageBox)
                            MessageBox.Show(Mahjong.Properties.Settings.Default.Pong, all.Name[all.state].ToString());
                        all.chow_pong(brand, c.SuccessPlayer);
                        updatePlayer_Table();
                        Chow_Pong_Brand = true;

                        return false;
                    }
                }
            }
            all.next();

            // 有沒有人要吃
            for (int i = 0; i < 3; i++)
            {
                all.next();
                Check c = new Check(brand, NowPlayer_removeTeam);
                Check w = new Check(brand, all.NowPlayer);
                Ai.setPlayer(brand, all.NowPlayer);
                if (c.Chow() && i == 0)
                {
                    // 如果是玩家
                    if (all.State == location.South)
                    {
                        toUser(brand, (c.Chow() && i == 0), c.Pong(), c.Kong(), false, w.Win());
                        if (Player_Pass_Brand)
                            Player_Pass_Brand = false;
                        else
                            return false;
                    }
                    else if (Ai.Chow)
                    {
                        setInforamtion();
                        if (showMessageBox)
                            MessageBox.Show(Mahjong.Properties.Settings.Default.Chow, all.Name[all.state].ToString());
                        all.chow_pong(brand, c.SuccessPlayer);
                        updatePlayer_Table();
                        Chow_Pong_Brand = true;
                        return false;
                    }
                }

            }
            all.next();

            return true;
        }

        /// <summary>
        /// 從AI得到一張牌
        /// </summary>
        /// <returns></returns>
        private Brand getfromAI()
        {
            Ai.setPlayer(NowPlayer_removeTeam);
            return Ai.getReadyBrand();
        }

        /// <summary>
        /// 把牌丟給玩家，看是否要吃 碰 槓 過水 胡
        /// </summary>
        private void toUser(Brand brand, bool chow, bool pong, bool kong, bool darkkong, bool win)
        {
            CPK cpk = new CPK(this, brand);
            Check c = new Check(brand, NowPlayer_removeTeam);
            Check w = new Check(brand, all.NowPlayer);
            cpk.Enabled_Button(chow, pong, kong, darkkong, win);
            if (chow || pong || kong || win || darkkong)
                cpk.ShowDialog();
        }

        /// <summary>
        /// 結束遊戲
        /// </summary>
        void win_game(Brand brand)
        {
            //清除桌面上的牌
            table.cleanImage();
            //停止迴圈
            roundTimer.Stop();
            //把最後那張牌加入玩家手牌
            all.NowPlayer.add(brand);
            //設定牌為可視並更新
            table.ShowAll = true;
            table.addImage();
            //呼叫台數計算
            Tally t = new Tally();
            t.setLocation(all.getLocation(), all.Win_Times);
            t.setPlayer(all);
            t.ShowDialog();
            //清除桌面上的牌
            table.cleanImage();
            //換到下一個莊
            all.nextWiner(false);
            //設定全部的牌為不顯示
            table.ShowAll = false;
            //開新的遊戲
            newgame2();
            this.factory = new BrandFactory();
        }

        /// <summary>
        /// 打一圈要做的事
        /// </summary>
        void round()
        {
            // 計時器停止
            roundTimer.Stop();

            // 如果是吃或碰不摸牌
            if (Chow_Pong_Brand)
                Chow_Pong_Brand = false;
            else
                touchBrand();
            // 目前狀態不等於玩家時
            if (all.State != location.South)
            {
                // 把牌打到桌面上看是否有人要 胡 槓 碰 吃
                // 若成立就表示沒有人要，不成立就表示被人拿走
                if (pushToTable(getfromAI()))
                {
                    // 換下一個人
                    all.next();
                    setInforamtion();
                }
                // 計時器重新啟動
                roundTimer.Start();
            }
            else
                setInforamtion();
        }
    }
}
