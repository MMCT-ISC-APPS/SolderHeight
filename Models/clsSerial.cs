using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolidHeight.Models
{
    public class clsSerial
    {
        public int SeqNo { get; set; }
        public string strSerialNo { get; set; }
        public string strSerialValues { get; set; }

    }

    public class clsSPCHeader
    {
        public string MaxID { get; set; }
        public DateTime dateTime { get; set; }
        public string Type { get; set; }
        public string EN { get; set; }
        public string Shift { get; set; }
        public string Line { get; set; }
        public string Lot { get; set; }
        public string Stencil { get; set; }
        public string Model { get; set; }
        public string LotType { get; set; }
        public int CavSeq { get; set; }
        public int MaxCavSeq { get; set; }
        public string Machine_Type { get; set; }
        public string Machine_Name { get; set; }
       
    }

    public class clsReConfirm_info
    {
         
        public string Cause { get; set; }
        public string Action { get; set; }
        public string Remark { get; set; }

    }


    
}
