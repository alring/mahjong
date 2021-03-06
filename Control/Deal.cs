using System;
using System.Collections.Generic;
using System.Text;
using Mahjong.Players;
using Mahjong.Brands;

namespace Mahjong.Control
{
    /// <summary>
    /// 依桌面上的牌來發牌
    /// </summary>
    class Deal
    {
        /// <summary>
        /// 玩家陣列
        /// </summary>
        BrandPlayer[] player;
        /// <summary>
        /// 計算每一個要分配多少
        /// </summary>
        private int countbrands;
        /// <summary>
        /// 計算一共有多少玩家
        /// </summary>
        private int countplayer;
        /// <summary>
        /// 桌面,牌的來源
        /// </summary>
        BrandPlayer table;

        /// <summary>
        /// 建構基本玩家數量和分配數
        /// </summary>
        /// <param name="countbrands">每一個玩家分配數</param>
        /// <param name="countplayer">一共有多少玩家</param>
        /// <param name="table">桌面玩家</param>
        public Deal(int countbrands, int countplayer,BrandPlayer table)
        {
            this.countbrands = countbrands;
            this.countplayer = countplayer;
            this.player = new BrandPlayer[countplayer];
            this.table = table;
            for (int i = 0; i < countplayer; i++)
                this.player[i] = new BrandPlayer();
        }
        /// <summary>
        /// 建構基本玩家數量和分配數
        /// </summary>
        /// <param name="countbrands">每一個玩家分配數</param>
        /// <param name="table">桌面玩家</param>
        public Deal(int countbrands, BrandPlayer table)
        {
            this.countbrands = countbrands;
            this.countplayer = 4;
            this.player = new BrandPlayer[countplayer];
            this.table = table;
            for (int i = 0; i < countplayer; i++)
                this.player[i] = new BrandPlayer();
        }
        /// <summary>
        /// 分配牌
        /// </summary>
        public void DealBrands()
        {
            BrandPlayer temp = new BrandPlayer();
            // 讀出數量的牌，並移除
            for (int i = 0; i < countplayer * countbrands; i++)
            {
                Brand brand = table.getBrand(i);
                temp.add(brand);
                table.remove(brand);
            }
            // 把牌加入玩家
            for (int i = 0; i < temp.getCount(); i++)
                player[i % countplayer].add(temp.getBrand(i));
        }
        /// <summary>
        /// 從桌面上移除
        /// </summary>
        /// <param name="iterator">桌面反覆器</param>
        /// <param name="re">桌面玩家</param>
        /// <returns>移除後的桌面玩家</returns>
        BrandPlayer removefromtable(Iterator iterator, BrandPlayer re)
        {
            while (iterator.hasNext())
            {
                Brand brand = (Brand)iterator.next();
                //把牌從玩家中移除
                re.remove(brand);
                //Console.WriteLine(">>{0}", re.remove(brand));
            }
            return re;
        }
        /// <summary>
        /// 傳回玩家
        /// </summary>
        public BrandPlayer[] Player
        {
            get { return player; }
        }
        /// <summary>
        /// 傳回桌面
        /// </summary>
        public BrandPlayer Table
        {
            get { return table; }
        }
    }
}
