using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolidHeight.Models;

namespace SolidHeight.Controls
{
    class clsCalculate
    {
        List<clsCal> lsCalR = new List<clsCal>();
        public Double CalSD(ref List<clsHieght> HeightList, Double myHeight, int SeqNo, string strPadperCal, string strCPKUpper, string strCPKLower)
        {
            try
            {
                Double SDBar = 0, Xbar = 0, sumZ = 0, sumZ2 = 0, Z, RAll, CPU, CPL, CPK, CPKUpper, CPKLower;
                Double dbMax, dbMin;
                Double Total = 0.000000;

                foreach (clsHieght i in HeightList)
                {
                    List<clsCal> lsResult = new List<clsCal>();
                    if (i.ToString() != "")
                    {
                        sumZ2 += Convert.ToDouble(Math.Pow(i.Height, 2));
                        sumZ += i.Height;
                    }

                    if (i.Results != null)
                    {
                        continue;
                    }


                    CPKUpper = Convert.ToDouble(strCPKUpper.ToString());
                    CPKLower = Convert.ToDouble(strCPKLower.ToString());

                    Total = Convert.ToDouble(HeightList.Count.ToString("0.000000"));
                    Z = ((Total * (Total - 1)) == 0 ? 1 : (Total * (Total - 1)));

                    dbMin = HeightList.OrderBy(o => o.Height).Select(s => s.Height).First();
                    dbMax = HeightList.OrderByDescending(o => o.Height).Select(s => s.Height).First();

                    Xbar = sumZ / Total;

                    Double SDR = ((Total * sumZ2) - (Math.Pow(sumZ, 2))) / Z;

                    SDBar = Z == 0 ? 0 : Math.Sqrt(((Total * sumZ2) - (Math.Pow(sumZ, 2))) / Z);
                    RAll = dbMax - dbMin;

                    CPU = SDBar != 0 ? (CPKUpper - Xbar) / (3 * SDBar) : 0;
                    CPL = SDBar != 0 ? (Xbar - CPKLower) / (3 * SDBar) : 0;


                    CPK = Math.Min(CPU, CPL);

                    if (Double.IsNaN(SDBar))
                    {
                        SDBar = 0;
                        CPK = 0;
                    }

                    lsCalR.AddRange(CalR(ref HeightList, myHeight, SeqNo, strPadperCal));
                    lsResult.AddRange(new List<clsCal>{
                     new clsCal
                     {
                         MyProperty = "X",
                         Values =  Math.Round(Xbar,6).ToString("0.000000"),
                         Result = Math.Round(Xbar,6)
                    },
                     new clsCal
                     {
                        MyProperty = "R",
                        Values = Math.Round(RAll,6).ToString("0.000000"), //((decimal)(RAll)).ToString(),
                        Result = Math.Round(RAll,6)
                    },
                });
                    lsResult.AddRange(lsCalR);

                    lsResult.AddRange(new List<clsCal>
                    {
                        new clsCal
                    {
                        MyProperty = "SD",
                        Values = Math.Round(SDBar,6).ToString("0.000000"), //((decimal)(SDBar)).ToString(),
                        Result =  Math.Round(SDBar,6)
                    },
                     new clsCal
                    {
                        MyProperty = "CPK",
                        Values = Math.Round(CPK,6).ToString("0.000000"), //((decimal)(CPK)).ToString(),
                        Result = Math.Round(CPK,6)
                    },
                    });
                    HeightList.Where(w => w.SeqNo == SeqNo).Select(c => { c.Results = lsResult; return c; }).ToList();
                }
                return SDBar;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<clsCal> CalR(ref List<clsHieght> HeightList, Double myHeight, int SeqNo, string strPadperCal)
        {
            Double dbMax, dbMin, R;
            int RNo = Convert.ToInt32(strPadperCal);
            List<clsCal> rrR = new List<clsCal>();
            List<clsCal> lsCalR = new List<clsCal>();

            dbMin = HeightList.OrderBy(o => o.Height).Select(s => s.Height).First();
            dbMax = HeightList.OrderByDescending(o => o.Height).Select(s => s.Height).First();

            if ((SeqNo % RNo) == 0)
            {

                R = dbMax - dbMin;
                rrR.AddRange(new List<clsCal>
                {
                    new clsCal
                    {
                        MyProperty = "R"+(SeqNo/RNo).ToString(),
                        Values = Math.Round(R ,6).ToString("0.000000"), //((decimal)(R)).ToString(),
                        Result = Math.Round(R ,6)
                    }
                });
            }

            return rrR;

        }


        private void CalCPK()
        {

        }
    }
}
