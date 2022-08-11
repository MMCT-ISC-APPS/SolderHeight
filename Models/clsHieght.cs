using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolidHeight.Models
{
    public class clsHieght
    {
        public int SeqNo { get; set; }
        public string strHeight { get; set; }
        public Double Height { get; set; }
        public List<clsCal> Results { get; set; }
    }

    public class clsCal
    {
      //  public int SeqNo { get; set; }
        public String MyProperty { get; set; }
        public String Values { get; set; }

        public Double Result { get; set; }
    }
}
