﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanKaraoke.DTO
{
    public class BillInfo
    {
        public BillInfo(int id, int billID, int foodID, int count)
        {
            this.ID = id;
            this.BillID = billID;
            this.FoodID = foodID;
            this.count = count;
        }

        public BillInfo(DataRow row)
        {
            this.ID = (int)row["id"];
            this.BillID = (int)row["idbill"];
            this.FoodID = (int)row["idfood"];
            this.count = (int)row["count"];   
        }

        private int count;
        
        private int foodID;

        private int billID;

        private int iD;

        public int BillID
        {
            get
            {
                return billID;
            }

            set
            {
                billID = value;
            }
        }

        public int ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }

        public int FoodID
        {
            get
            {
                return foodID;
            }

            set
            {
                foodID = value;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
            }
        }

        
    }
}
