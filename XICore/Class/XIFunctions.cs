using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using XISystem;

namespace XICore
{
    public class XIFunctions
    {
        XIIXI oXI = new XIIXI();
        XIInstanceBase oXIInstance = new XIInstanceBase();
        public string Format(string dValue, string sFormatType)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(dValue) && dValue != "-")
                {
                    //var dtValue = oXI.ConvertToDateTime(dValue,sFormatType);
                    //DateTime dtValue= DateTime.ParseExact(dValue, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    // DateTime dtValue = Convert.ToDateTime(dValue);
                    XIIAttribute oAttr = new XIIAttribute();
                    XIIBO oBO = new XIIBO();
                    var dtValue = oBO.ConvertToDtTime(dValue);
                    sReturnValue = dtValue.ToString(sFormatType);
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string LowerCase(string sValue)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sValue))
                {
                    sReturnValue = sValue.ToLower();
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string UpperCase(string sValue)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sValue))
                {
                    sReturnValue = sValue.ToUpper();
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string GetMonth(string dValue)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(dValue) && dValue != "-")
                {
                    XIIAttribute oAttr = new XIIAttribute();
                    XIIBO oBO = new XIIBO();
                    var dtValue = oBO.ConvertToDtTime(dValue);
                    sReturnValue = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dtValue.Month);
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string GetYear(string dValue)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(dValue) && dValue != "-")
                {
                    XIIAttribute oAttr = new XIIAttribute();
                    XIIBO oBO = new XIIBO();
                    var dtValue = oBO.ConvertToDtTime(dValue);
                    sReturnValue = Convert.ToString(dtValue.Year);
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        //public decimal FindDifference(decimal nr1, decimal nr2)
        //{
        //    return Math.Abs(nr1 - nr2);
        //}
        public string Difference(string number1, string number2)
        {
            decimal dValue = 0; string sReturnValue = string.Empty;
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(number1) && !string.IsNullOrEmpty(number2))
                {
                    dValue = Math.Abs(Convert.ToDecimal(number1) - Convert.ToDecimal(number2));
                }
                //sReturnValue = Convert.ToString(dValue);
                sReturnValue = String.Format("{0:0.00}", dValue);
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string Add(string number1, string number2)
        {
            CResult oCResult = new CResult();
            decimal dValue = 0; string sReturnValue = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(number1) && !string.IsNullOrEmpty(number2))
                {
                    dValue = Convert.ToDecimal(number1) + Convert.ToDecimal(number2);
                }
                //sReturnValue = Convert.ToString(dValue);
                sReturnValue = String.Format("{0:0.00}", dValue);
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string MonthName(string sValue)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sValue) && sValue != "0")
                {
                    sReturnValue = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(sValue));
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string Percentageof(string sAmount, string sPercentage, string sFormat = "")
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sPercentage))
                {
                    decimal iPercentageAmount = Convert.ToDecimal(sPercentage) / 100;
                    decimal sFinalValue = Convert.ToDecimal(sAmount) * iPercentageAmount;
                    decimal sPercentageValue = Convert.ToDecimal(sAmount) + sFinalValue;
                    //sReturnValue = String.Format("{0:0.00}", sPercentageValue);
                    if (!string.IsNullOrEmpty(sFormat))
                    {
                        CultureInfo rgi = new CultureInfo(sFormat);
                        sReturnValue = string.Format(rgi, "{0:c}", sPercentageValue).ToString();
                    }
                    else
                    {
                        sReturnValue = String.Format("{0:0.00}", sPercentageValue);
                    }
                    //sReturnValue = Convert.ToString(sPercentageValue);
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string ExcludePercentageof(string sAmount, string sPercentage, string sFormat = "")
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sPercentage))
                {
                    decimal iPercentageAmount = Convert.ToDecimal(sPercentage) / 100;
                    decimal sFinalValue = Convert.ToDecimal(sAmount) * iPercentageAmount;
                    decimal sPercentageValue = Convert.ToDecimal(sAmount) - sFinalValue;
                    //sReturnValue = Convert.ToString(sPercentageValue);
                    //sReturnValue = String.Format("{0:0.00}", sPercentageValue);
                    if (!string.IsNullOrEmpty(sFormat))
                    {
                        CultureInfo rgi = new CultureInfo(sFormat);
                        sReturnValue = string.Format(rgi, "{0:c}", sPercentageValue).ToString();
                    }
                    else
                    {
                        sReturnValue = String.Format("{0:0.00}", sPercentageValue);
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string ExcludeIPT(string sAmount, string sPercentage, string sFormat = "")
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sPercentage))
                {
                    decimal Amount = Convert.ToDecimal(sAmount) * 100 / (100 + Convert.ToDecimal(sPercentage));

                    //sReturnValue = Convert.ToString(sPercentageValue);
                    //sReturnValue = String.Format("{0:0.00}", sPercentageValue);
                    if (!string.IsNullOrEmpty(sFormat))
                    {
                        CultureInfo rgi = new CultureInfo(sFormat);
                        sReturnValue = string.Format(rgi, "{0:c}", Amount).ToString();
                    }
                    else
                    {
                        sReturnValue = String.Format("{0:0.00}", Amount);
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string GetIPT(string sAmount, string sPercentage, string sFormat = "")
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sPercentage))
                {
                    decimal Amount = Convert.ToDecimal(sAmount) * 100 / (100 + Convert.ToDecimal(sPercentage));
                    decimal sValue = Convert.ToDecimal(sAmount) - Amount;
                    //sReturnValue = Convert.ToString(sPercentageValue);
                    //sReturnValue = String.Format("{0:0.00}", sPercentageValue);
                    if (!string.IsNullOrEmpty(sFormat))
                    {
                        CultureInfo rgi = new CultureInfo(sFormat);
                        sReturnValue = string.Format(rgi, "{0:c}", sValue).ToString();
                    }
                    else
                    {
                        sReturnValue = String.Format("{0:0.00}", sValue);
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string Percentage(string sAmount, string sPercentage, string sFormat = "")
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sPercentage))
                {
                    decimal iPercentageAmount = Convert.ToDecimal(sPercentage) / 100;
                    decimal sFinalValue = Convert.ToDecimal(sAmount) * iPercentageAmount;
                    //sReturnValue = Convert.ToString(sFinalValue);
                    //sReturnValue = String.Format("{0:0.00}", sFinalValue);
                    if (!string.IsNullOrEmpty(sFormat))
                    {
                        CultureInfo rgi = new CultureInfo(sFormat);
                        sReturnValue = string.Format(rgi, "{0:c}", sFinalValue).ToString();
                    }
                    else
                    {
                        sReturnValue = String.Format("{0:0.00}", sFinalValue);
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string FormatAmount(string sAmount)
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount))
                {
                    decimal dAmount = Convert.ToDecimal(sAmount);
                    sReturnValue = String.Format("{0:0.00}", dAmount);
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        //Get 2 digits after decimal
        public string FormatCurrency(string sAmount, string sFormat)
        {
            string sFormattedValue = string.Empty;
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sFormat))
                {
                    CultureInfo rgi = new CultureInfo(sFormat);
                    sFormattedValue = string.Format(rgi, "{0:c}", Convert.ToDecimal(sAmount)).ToString();
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sFormattedValue;
        }
        public string GetCurrentDate(string sFormat)
        {
            CResult oCResult = new CResult();
            string sFormattedValue = string.Empty;
            try
            {
                var dCurrentDate = DateTime.Now.ToString();
                XIIBO oBO = new XIIBO();
                var dtValue = oBO.ConvertToDtTime(dCurrentDate);
                if (!string.IsNullOrEmpty(sFormat))
                {
                    sFormattedValue = dtValue.ToString(sFormat);
                }
                else
                {
                    sFormattedValue = dCurrentDate;
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sFormattedValue;
        }
        public string GetCurrentMonth()
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                var iMonth = DateTime.Now.Month;
                sReturnValue = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(iMonth);
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string GetCurrentYear()
        {
            string sReturnValue = "";
            CResult oCResult = new CResult();
            try
            {
                var iYear = DateTime.Now.Year;
                sReturnValue = Convert.ToString(iYear);
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public int GetMonthsDifference(string dValue)
        {
            int sMonthsDiff = 0;
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(dValue))
                {
                    var iPresentYear = DateTime.Now.Month;
                    sMonthsDiff = ((DateTime.Now.Year - Convert.ToDateTime(dValue).Year) * 12) + DateTime.Now.Month - Convert.ToDateTime(dValue).Month;
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sMonthsDiff;
        }
        //GetYearDifference
        public string GetYearDifference(string dValue)
        {
            string sDiffYear = "";
            int iYear = Convert.ToInt32(dValue);
            CResult oCResult = new CResult();
            try
            {
                if (iYear > 0)
                {
                    var iPresentYear = DateTime.Now.Year;
                    sDiffYear = (Math.Abs(iPresentYear - iYear)).ToString();
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sDiffYear;
        }
        //Add days
        public string AddDays(string sDate, string sDays, string sFormatType = "")
        {
            CResult oCResult = new CResult();
            DateTime dValue = new DateTime(); string sReturnValue = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(sDays))
                {
                    XIIBO oBO = new XIIBO();
                    var dtValue = oBO.ConvertToDtTime(sDate);
                    int iDays = 0;
                    if (int.TryParse(sDays, out iDays))
                    {
                        dValue = dtValue.AddDays(iDays);
                    }
                }
                if (!string.IsNullOrEmpty(sFormatType))
                {
                    sReturnValue = dValue.ToString(sFormatType);
                }
                else
                {
                    sReturnValue = dValue.ToString();
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        public string GetMapValue(string sBOName, string sAttributeName, string sSelectValue, string FKiProductID, string sAPICode)
        {
            string sReturnValue = "";
            string sQuery = @"select sTo from APIMapValEnum_T inner join APIMapVal_T on APIMapValEnum_T.FKiMapValID=APIMapVal_T.ID 
inner join APIMap_T on APIMapVal_T.FKiMapID = APIMap_T.ID and APIMapVal_T.sBO='" + sBOName + "' and APIMapVal_T.Attribute='"
+ sAttributeName + "' and APIMapValEnum_T.sFrom='" + sSelectValue + "' and FKiProductID = " + FKiProductID + " and sCode='" + sAPICode + "'";
            CResult oCResult = new CResult();
            try
            {
                XID1Click oXI1Click = new XID1Click();
                oXI1Click.Query = sQuery;
                oXI1Click.Name = "APIMapValEnum_T";
                var Result = oXI1Click.Execute_Query();
                if (Result.Rows.Count > 0)
                {
                    sReturnValue = Result.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oXIInstance.SaveErrortoDB(oCResult);
                oCResult.LogToFile();
            }
            return sReturnValue;
        }
        //public string CommissionCalculation(string sAmount, string IPT, string sPercentage, string sFormat = "")
        //{
        //    string sReturnValue = "";
        //    CResult oCResult = new CResult();
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrEmpty(sPercentage))
        //        {
        //            decimal iPercentageAmount = Convert.ToDecimal(sPercentage) / 100;
        //            decimal sFinalValue = (Convert.ToDecimal(sAmount) - Convert.ToDecimal(IPT)) * iPercentageAmount;
        //            //sReturnValue = Convert.ToString(sFinalValue);
        //            //sReturnValue = String.Format("{0:0.00}", sFinalValue);
        //            if (!string.IsNullOrEmpty(sFormat))
        //            {
        //                CultureInfo rgi = new CultureInfo(sFormat);
        //                sReturnValue = string.Format(rgi, "{0:c}", Math.Round(sFinalValue, 2)).ToString();
        //            }
        //            else
        //            {
        //                sReturnValue = String.Format("{0:0.00}", Math.Round(sFinalValue,2));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
        //        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
        //        oXIInstance.SaveErrortoDB(oCResult);
        //        oCResult.LogToFile();
        //    }
        //    return sReturnValue;
        //}
    }
}