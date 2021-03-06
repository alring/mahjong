using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace Mahjong.Brands
{
    /// <summary>
    /// 牌的介面
    /// </summary>
    public interface Brand
    {
        /// <summary>
        /// 牌的類別
        /// </summary>
        string getClass();
        /// <summary>
        /// 牌的值的大小
        /// </summary>
        int getNumber();
        /// <summary>
        /// 牌是否可視
        /// </summary> 
        bool IsCanSee
        {
            get;
            set;
        }
        /// <summary>
        /// 牌的圖片位置
        /// </summary>  
        Image image
        {
            get;
            set;
        }
        /// <summary>
        /// 牌的組別
        /// </summary>
        int Team
        {
            get;
            set;
        }
        /// <summary>
        /// 牌的分數
        /// </summary>
        int Source
        {
            get;
            set;
        }
        /// <summary>
        /// 誰打出去的
        /// </summary>
        Mahjong.Control.location WhoPush
        {
            get;
            set;
        }       
    }
}
