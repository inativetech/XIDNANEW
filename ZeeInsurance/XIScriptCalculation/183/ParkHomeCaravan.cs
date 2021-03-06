using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using XICore;
using XISystem;

namespace ZeeInsurance
{
    public class ParkHomeCaravan
    {
        public static CResult PHTowerGateCalculation(List<CNV> lParam)
        {
            XIInstanceBase oIB = new XIInstanceBase();
            //XIIQS oQsInstance = new XIIQS();
            PHTowergate Pcal = new PHTowergate();
            CResult oResult = new CResult();
            oResult.sMessage = "PHTowerGate script running";
            oIB.SaveErrortoDB(oResult);
            CNV oNV = new CNV();
            oNV.sName = "sCode";
            try
            {
                string sGUID = lParam.Where(m => m.sName == "sUID").FirstOrDefault().sValue;
                int iInstanceID = Convert.ToInt32(lParam.Where(m => m.sName == "iInsatnceID").FirstOrDefault().sValue);
                int iUserID = Convert.ToInt32(lParam.Where(m => m.sName == "iUserID").FirstOrDefault().sValue);
                int iCustomerID = Convert.ToInt32(lParam.Where(m => m.sName == "iCustomerID").FirstOrDefault().sValue);
                string sDataBase = lParam.Where(m => m.sName == "sDataBase").FirstOrDefault().sValue;
                string sProductName = lParam.Where(m => m.sName == "ProductName").FirstOrDefault().sValue;
                string sVersion = lParam.Where(m => m.sName == "Version").FirstOrDefault().sValue;
                string sSessionID = lParam.Where(m => m.sName == "sSessionID").FirstOrDefault().sValue;
                string sProductCode = lParam.Where(m => m.sName == "ProductCode").FirstOrDefault().sValue;
                int iQuoteID = Convert.ToInt32(lParam.Where(m => m.sName == "iQuoteID").FirstOrDefault().sValue);
                //XIInfraCache XIInfraCache = new XIInfraCache();
                //oQsInstance = XIInfraCache.Get_QuestionSetCache("QuestionSetCache", sUID, iInsatnceID);
                //oResult.sMessage = "In Script: Questionset object Loaded from cache";
                //oIB.SaveErrortoDB(oResult);
                oResult = Pcal.GetCaravanFinalPremium(sGUID, iInstanceID, iUserID, sProductName, sVersion, sSessionID, iCustomerID, iQuoteID);
                oNV.sValue = "00";
            }
            catch (Exception ex)
            {
                oResult.sMessage = "ERROR: [" + oResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oResult.oResult = "Error";
                oNV.sValue = "100";
                oIB.SaveErrortoDB(oResult);
            }
            oResult.oCollectionResult.Add(oNV);
            return oResult;
        }
        public class PHTowergate
        {
            List<CResult> oGeneralDeclines = new List<CResult>();
            public CResult GetBaseRate(string sUnitType)
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    oresult.sMessage = "UnitType-" + sUnitType;
                    oIB.SaveErrortoDB(oresult);
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+0";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "Base Rate";
                    switch (sUnitType)
                    {
                        case "10":
                            oNV1.sValue = "236.75";
                            break;
                        case "20":
                            oNV1.sValue = "287.90";
                            break;
                    }
                    oresult.sMessage = "Base Rate-" + oNV1.sValue + "_" + oNV.sValue;
                    oIB.SaveErrortoDB(oresult);
                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oresult.LogToFile();
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetPersonalPossessionsLoad(string sType)
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    oresult.sMessage = "PersonalPossession-" + sType;
                    oIB.SaveErrortoDB(oresult);
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+0";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "Personal Possessions";
                    CNV oNV3 = new CNV();
                    oNV3.sName = "Type";
                    oNV3.sValue = "Value";
                    if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "yes")
                    {
                        oNV1.sValue = "+50";
                    }
                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                    oresult.oCollectionResult.Add(oNV3);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oresult.LogToFile();
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetRoofLoad(string sType)
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    oresult.sMessage = "Roof-" + sType;
                    oIB.SaveErrortoDB(oresult);
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+0";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "Roof";
                    CNV oNV3 = new CNV();
                    oNV3.sName = "Type";
                    oNV3.sValue = "Percent";
                    if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "no")
                    {
                        oNV1.sValue = "+30";
                    }
                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                    oresult.oCollectionResult.Add(oNV3);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oresult.LogToFile();
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetExcessLoad(int NoOfClaims, List<XIIBO> ClaimI, string CoverStartDate)
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+0";
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "Policy Excess";
                    CNV oNV3 = new CNV();
                    oNV3.sName = "Type";
                    oNV3.sValue = "Value";
                    double rTotalClaim = 0.00;
                    List<String> sClaimType = new List<string>();
                    List<DateTime> dtClaims = new List<DateTime>();
                    int iNoofClaims = 0;
                    foreach (var Claim in ClaimI)
                    {
                        var dt3Years = DateTime.Now.AddYears(-3).Date;
                        var dtClaimDate = Convert.ToDateTime(Claim.AttributeI("dDate").sValue);
                        double rCost = 0.00;
                        if (dtClaimDate >= dt3Years)
                        {
                            dtClaims.Add(dtClaimDate);
                            iNoofClaims++;
                            sClaimType.Add(Claim.Attributes["sName"].sResolvedValue);
                            string sClaimCost = Claim.Attributes["rTotalClaimCost"].sResolvedValue;
                            if (double.TryParse(sClaimCost, out rCost))
                            {
                            }
                            rTotalClaim += rCost;
                        }
                    }
                    if (iNoofClaims == 2)
                    {
                        if (sClaimType[0] == sClaimType[1])
                        {
                            if (dtClaims.All(m => m > DateTime.Now.AddYears(-1).Date))
                            {
                                if (rTotalClaim >= 1500 && rTotalClaim <= 2499)
                                {
                                    oNV1.sValue = "+150";
                                }
                                else if (rTotalClaim >= 2500 && rTotalClaim <= 4999)
                                {
                                    oNV1.sValue = "+250";
                                }
                            }
                            else if (dtClaims.Any(m => m > DateTime.Now.AddYears(-1).Date))
                            {
                                if (rTotalClaim >= 1500 && rTotalClaim <= 2499)
                                {
                                    oNV1.sValue = "+100";
                                }
                                else if (rTotalClaim >= 2500 && rTotalClaim <= 4999)
                                {
                                    oNV1.sValue = "+200";
                                }
                            }
                            else
                            {
                                if (rTotalClaim >= 1500 && rTotalClaim <= 2499)
                                {
                                    oNV1.sValue = "+100";
                                }
                                else if (rTotalClaim >= 2500 && rTotalClaim <= 4999)
                                {
                                    oNV1.sValue = "+150";
                                }
                            }
                        }
                        else
                        {
                            if (dtClaims.All(m => m > DateTime.Now.AddYears(-1).Date))
                            {
                                if (rTotalClaim >= 1500 && rTotalClaim <= 2499)
                                {
                                    oNV1.sValue = "+100";
                                }
                                else if (rTotalClaim >= 2500 && rTotalClaim <= 4999)
                                {
                                    oNV1.sValue = "+250";
                                }
                            }
                            else if (dtClaims.Any(m => m > DateTime.Now.AddYears(-1).Date))
                            {
                                if (rTotalClaim >= 1500 && rTotalClaim <= 2499)
                                {
                                    oNV1.sValue = "+100";
                                }
                                else if (rTotalClaim >= 2500 && rTotalClaim <= 4999)
                                {
                                    oNV1.sValue = "+200";
                                }
                            }
                            else
                            {
                                if (rTotalClaim >= 1500 && rTotalClaim <= 2499)
                                {
                                    oNV1.sValue = "+100";
                                }
                                else if (rTotalClaim >= 2500 && rTotalClaim <= 4999)
                                {
                                    oNV1.sValue = "+150";
                                }
                            }
                        }
                    }

                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                    oresult.oCollectionResult.Add(oNV3);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oresult.LogToFile();
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetIPTLoadFactor()
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+12";
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "IPT";
                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetArea(string PostCode, int iProductID)
            {
                CResult oCResult = new CResult();
                XIInstanceBase oIB = new XIInstanceBase();
                XIAPI oXIAPI = new XIAPI();
                try
                {
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+0";
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "Postcode";
                    List<XIWhereParams> oWHParams = new List<XIWhereParams>();
                    oWHParams.Add(new XIWhereParams { sField = "sPostCode", sOperator = "=", sValue = PostCode });
                    oWHParams.Add(new XIWhereParams { sField = "FKiProductID", sOperator = "=", sValue = iProductID.ToString() });
                    var oArea = oXIAPI.GetValue("AreaLookUp_T", "Area", oWHParams);
                    //var oArea = oXIAPI.GetValue("AreaLookUp_T", PostCode, "Area", "sPostCode");
                    if (!string.IsNullOrEmpty(oArea) && oArea.ToLower() == "a")
                    {
                        oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    }
                    oCResult.oCollectionResult.Add(oNV);
                    oCResult.oCollectionResult.Add(oNV1);
                    oCResult.oCollectionResult.Add(oNV2);
                }
                catch (Exception ex)
                {
                    oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oCResult.LogToFile();
                    oIB.SaveErrortoDB(oCResult);
                }
                return oCResult;
            }
            public CResult BuildCResultObject(string Value, string Name, string Status)
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = Status;
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = Value;
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = Name;
                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oresult.LogToFile();
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetOccupationsLoadFactor(string sOccupation, string sSecOccupation)
            {
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oresult = new CResult();
                try
                {
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    CNV oNV = new CNV();
                    oNV.sName = "sMessage";
                    oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString();
                    CNV oNV1 = new CNV();
                    oNV1.sName = "oResult";
                    oNV1.sValue = "+0";
                    CNV oNV2 = new CNV();
                    oNV2.sName = "LoadFactorName";
                    oNV2.sValue = "Occupation/Secondary Occupation";
                    CNV oNV3 = new CNV();
                    oNV3.sName = "Type";
                    oNV3.sValue = "Percent";
                    if (!string.IsNullOrEmpty(sOccupation))
                    {
                        sOccupation = sOccupation.ToLower();
                    }
                    if (!string.IsNullOrEmpty(sSecOccupation))
                    {
                        sSecOccupation = sSecOccupation.ToLower();
                    }
                    string[] DeclineArray = new string[] {"art buyer","acrobat","amusement arcade worker","antique dealer","antique renovator","art dealer","asphalter","road worker","bodyguard","bookmaker","bricklayer","broadcaster","broadcaster - tv/radio","actor or actress","circus proprietor","circus worker","coin dealer","croupier","dancer","dealer","dealer - general","dealer - scrap/waste","diamond dealer","disc jockey","entertainer","exotic dancer","fairground worker","floor manager","footballer","footballer - semi professional","furniture dealer","furniture restorer","gambler","gaming club manager","gaming club proprietor","gaming club staff - licensed premises","gaming club staff - unlicensed premises","golf caddy","golf club professional","golf coach","golfer","hawker","horse breeder","horse dealer","horse dealer (non sport)","horse dealer (sport)","horse trader","horse trainer","interviewer","jeweller","jockey","journalist","journalistic agent","journalist - freelance","kissagram person","landscape gardener","landworker","licensee","magician","manager - ring sports","manager - sports","market trader","medal dealer","model","money broker","money dealer","moneylender","motor racing driver","motor racing organiser","motor trader","music producer","musician","musician - amateur","musician - classical","musician - dance band","musician - pop group","night club staff","non professional footballer","non professional sports coach","opera singer","orchestral violinist","orchestral violinist","playwright","private detective","private investigator","professional apprentice footballer","professional boxer","professional cricketer","professional cyclist","professional footballer","professional racing driver","professional racing motorcyclist","professional sports coach","professional sportsperson","professional wrestler","promoter","promoter - entertainments","promoter - sports","promoter - racing","promoter - ring sports","publican","publicity manager","publisher","publishing manager","racehorse groom","racing motorcyclist","racing organiser","radio director","radio director","rally driver","scrap dealer","semi-professional sportsperson","second hand dealer","show jumper","showman or woman","snooker player","song writer","sports administrator - other sports","sports administrator - ring sports","sports agent","sports centre attendant","sports coach","sports commentator","sports scout","sportsman or woman","store detective","street entertainer","street trader","student","student - foreign","student - living at home","student - living away","student nurse","student nurse - living at home","student nurse - living away","student teacher","student teacher - living at home","student teacher - living away","tv announcer","tv broadcasting technician","tv editor","tarot reader/palmistry expert","television director","television presenter","television producer","trainer - animal","trainer - greyhound","trainer - race horse","travelling showman","turf accountant","undergraduate student - living at home","undergraduate student - living away from home","unemployed","ventriloquist","waste dealer", "radio producer" };
                    //string[] DeclineArray = new string[] { "acrobat", "actor", "actor/actress", "actress", "amusement arcade worker", "antique dealer", "antique renovator", "art buyer", "art dealer", "asphalter/roadworker", "bodyguard", "bookmaker", "bricklayer", "broadcaster", "broadcaster - tv/radio", "circus proprietor", "circus worker", "coin dealer", "croupier", "dancer", "dealer", "dealer - general", "dealer – scrap/waste", "diamond dealer", "disc jockey", "entertainer", "exotic dancer", "fairground worker", "floor manager", "footballer", "footballer - semi professional", "furniture dealer", "furniture restorer", "gambler", "gaming club manager", "gaming club proprietor", "gaming club staff - licensed premises", "gaming club staff - unlicensed premises", "golf caddy", "golf club professional", "golf coach", "golfer", "hawker", "horse breeder", "horse dealer", "horse dealer (non sport)", "horse dealer (sport)", "horse trader", "horse trainer", "interviewer", "jeweller", "jockey", "journalist", "journalist – freelance", "journalistic agent", "kissagram person", "landscape gardener", "landworker", "licensee", "magician", "anager - ring sports", "manager – sports", "market trader", "medal dealer", "model", "money broker", "money dealer", "moneylender", "motor dealer", "motor racing driver", "motor racing organiser", "motor trader", "music producer", "musician", "musician – amateur", "musician - classical", "musician - dance band", "musician - pop group", "night club staff", "non professional footballer", "non professional sports coach", "opera singer", "orchestra leader", "orchestral violinist", "playwright", "private detective", "private investigator", "professional apprentice footballer", "professional boxer", "professional cricketer", "professional cyclist", "professional footballer", "professional racing driver", "professional racing motorcyclist", "professional sports coach", "professional sportsperson", "professional wrestler", "promoter", "promoter – entertainments", "promoter - racing", "promoter - ring sports", "promoter – sports", "publican", "publicity manager", "publisher", "publishing manager", "racehorse groom", "racing motorcyclist", "racing organiser", "radio drector", "radio presenter", "radio producer", "rally driver", "scrap dealer", "second hand dealer", "semi-professional sportsperson", "show jumper", "showman", "snooker player", "song writer", "sports administrator - other sports", "sports administrator - ring sports", "sports agent", "sports centre attendant", "sports coach", "sports commentator", "sports scout", "sportsman", "sportswoman", "store detective", "street entertainer", "street trader", "student ", "student – foreign", "student - living away", "student - living at home", "student nurse", "student nurse - living at home", "student nurse - living away", "student teacher ", "student teacher - living at home", "student teacher - living away", "tv announcer", "tv broadcasting technician", "tv editor", "tarot reader/palmistry expert", "television director", "television presenter", "television producer", "trainer - animal", "trainer – greyhound", "trainer - race horse", "travelling showman", "turf accountant", "undergraduate student - living at home", "undergraduate student - living away from home", "mployed", "ventriloquist", "waste dealer" };
                    //string[] DeclineArray = new string[] { "acrobat", "actor", "actor/actress", "actress", "amusement arcade worker", "antique dealer", "antique renovator", "art buyer", "art dealer", "asphalter/roadworker", "bodyguard", "bookmaker", "bricklayer", "broadcaster", "broadcaster - tv/radio", "circus proprietor", "circus worker", "coin dealer", "croupier", "dancer", "dealer", "dealer - general", "dealer - scrap/waste", "diamond dealer", "disc jockey", "entertainer", "exotic dancer", "fairground worker", "floor manager", "footballer", "footballer - semi professional", "furniture dealer", "furniture restorer", "gambler", "gaming club manager", "gaming club proprietor", "gaming club staff - licensed premises", "gaming club staff - unlicensed premises", "golf caddy", "golf club professional", "golf coach", "golfer", "hawker", "horse breeder", "horse dealer", "horse dealer (non sport)", "horse dealer (sport)", "horse trader", "horse trainer", "interviewer", "jeweller", "jockey", "journalist", "journalist - freelance", "journalistic agent", "kissagram person", "landscape gardener", "landworker", "licensee", "magician", "manager - ring sports", "manager - sports", "market trader", "medal dealer", "model", "money broker", "money dealer", "moneylender", "motor dealer", "motor racing driver", "motor racing organiser", "motor trader", "music producer", "musician", "musician - amateur", "musician - classical", "musician - dance band", "musician - pop group", "night club staff", "non professional footballer", "non professional sports coach", "opera singer", "orchestra leader", "orchestral violinist", "playwright", "private detective", "private investigator", "professional apprentice footballer", "professional boxer", "professional cricketer", "professional cyclist", "professional footballer", "professional racing driver", "professional racing motorcyclist", "professional sports coach", "professional sportsperson", "professional wrestler", "promoter", "promoter - entertainments", "promoter - racing", "promoter - ring sports", "promoter - sports", "publican", "publicity manager", "publisher", "publishing manager", "racehorse groom", "racing motorcyclist", "racing organiser", "radio director", "radio presenter", "radio producer", "rally driver", "scrap dealer", "second hand dealer", "semi-professional sportsperson", "show jumper", "showman", "snooker player", "song writer", "sports administrator - other sports", "sports administrator - ring sports", "sports agent", "sports centre attendant", "sports coach", "sports commentator", "sports scout", "sportsman", "sportswoman", "store detective", "street entertainer", "street trader", "student", "student - foreign", "student - living away", "student - living at home", "student nurse", "student nurse - living at home", "student nurse - living away", "student teacher", "student teacher - living at home", "student teacher - living away", "tv announcer", "tv broadcasting technician", "tv editor", "tarot reader/palmistry expert", "television director", "television presenter", "television producer", "trainer - animal", "trainer - greyhound", "trainer - race horse", "travelling showman", "turf accountant", "undergraduate student - living at home", "undergraduate student - living away from home", "unemployed", "ventriloquist", "waste dealer" };
                    int pos = Array.IndexOf(DeclineArray, sOccupation);
                    int iSesPos = Array.IndexOf(DeclineArray, sSecOccupation);
                    if (pos > -1 || iSesPos > -1)
                    {
                        oNV.sValue = xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString();
                    }
                    oresult.oCollectionResult.Add(oNV);
                    oresult.oCollectionResult.Add(oNV1);
                    oresult.oCollectionResult.Add(oNV2);
                    oresult.oCollectionResult.Add(oNV3);
                }
                catch (Exception ex)
                {
                    oresult.sMessage = "ERROR: [" + oresult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oresult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oresult.LogToFile();
                    oIB.SaveErrortoDB(oresult);
                }
                return oresult;
            }
            public CResult GetCaravanFinalPremium(string sGUID, int iInstanceID, int iUserID, string sProductName, string sVersion, string sSessionID, int iCustomerID, int iQuoteID)
            {
                List<CResult> oGeneralDeclines = new List<CResult>();
                List<string> Info = new List<string>();
                Info.Add("QsInstanceID_" + iInstanceID);
                XIInstanceBase oIB = new XIInstanceBase();
                CResult oCResult = new CResult();
                XIInfraCache oCache = new XIInfraCache();
                XIIXI oIXI = new XIIXI();
                List<CResult> oXiResults = new List<CResult>();
                try
                {
                    //double rFinalPremium = 0; int iProposerAge = 0; string sContentsSI = string.Empty; string sEquipmentSI = string.Empty; string sAwningSI = string.Empty;
                    var oProductVersionI = oIXI.BOI("ProductVersion_T", sVersion);
                    int iProductID = 0;
                    string ProductID = oProductVersionI.Attributes["FKiProductID"].sValue;
                    if (int.TryParse(ProductID, out iProductID)) { }
                    var oProductI = oIXI.BOI("Product", ProductID);
                    double rCompulsaryExcess = 0; double rVoluntaryExcess = 0;
                    //double rBaseLoad = 0;
                    var oQSI = oCache.Get_QsStructureObj(sSessionID, sGUID, "QSInstance_" + iInstanceID + "Caravan NotationStructure");
                    var ostructureInstance = oQSI.oStructureInstance.FirstOrDefault().Value.FirstOrDefault();
                    string sCoverStartDate = ostructureInstance.XIIValue("dCoverStart").sValue;
                    string sUnitType = ostructureInstance.XIIValue("singleordoubleunitPH").sValue;
                    string sYearofManufacture = ostructureInstance.XIIValue("YearOfManufacturePH").sValue;
                    string spersonalpossessions = ostructureInstance.XIIValue("personalpossessionsPH").sResolvedValue;
                    string sroof = ostructureInstance.XIIValue("tiledroofPH").sResolvedValue;
                    string sPreviousMotorOrCaravanInsurancePolicyDetails = ostructureInstance.XIIValue("PreviousMotorOrCaravanInsurancePolicyDetailsPH").sResolvedValue;
                    string sbankruptcy = ostructureInstance.XIIValue("bankruptcyPH").sResolvedValue;
                    string sfeltroof = ostructureInstance.XIIValue("feltroofPH").sResolvedValue;
                    string ssMotorOffence = ostructureInstance.XIIValue("sMotorOffencePH").sResolvedValue;
                    string sPostCode = ostructureInstance.XIIValue("sPostCode").sValue;
                    string sOccupation = ostructureInstance.XIIValue("OccupationPH").sResolvedValue;
                    string sSecOccupation = ostructureInstance.XIIValue("SecondaryOccupationPH").sResolvedValue;
                    var oAreaResult = GetArea(sPostCode, iProductID);
                    if (oAreaResult.xiStatus == 0 && oAreaResult.oCollectionResult.Count > 0)
                    {
                        oXiResults.Add(oAreaResult);
                    }
                    List<CNV> oParams = new List<CNV>();
                    oParams.Add(new CNV { sName = "sUnit", sValue = sUnitType });
                    CResult oBaseLoadFactor = new CResult();
                    var oResult = GetBaseRate(sUnitType);
                    if (oResult.xiStatus == 0 && oResult.oCollectionResult.Count > 0)
                    {
                        oBaseLoadFactor = oResult;
                    }
                    oResult = GetPersonalPossessionsLoad(spersonalpossessions);
                    if (oResult.xiStatus == 0 && oResult.oCollectionResult.Count > 0)
                    {
                        oXiResults.Add(oResult);
                    }

                    //Occupations
                    if (!string.IsNullOrEmpty(sOccupation) || !string.IsNullOrEmpty(sSecOccupation))
                    {
                        Info.Add("Occupation " + sOccupation);
                        Info.Add("Secondary Occupation " + sSecOccupation);
                        var oOccupation = GetOccupationsLoadFactor(sOccupation, sSecOccupation);
                        if (oOccupation.bOK && oOccupation.oCollectionResult.Count > 0)
                        {
                            oXiResults.Add(oOccupation);
                        }
                    }

                   
                    //Roof
                    var oRoofLoadFactor = GetRoofLoad(sroof);
                    if (oRoofLoadFactor.xiStatus == 0 && oRoofLoadFactor.oCollectionResult.Count > 0)
                    {
                        oXiResults.Add(oRoofLoadFactor);
                    }
                    var IPTLoadFactor = new CResult();
                    var oIPTLoadFactor = GetIPTLoadFactor();
                    if (oIPTLoadFactor.xiStatus == 0 && oIPTLoadFactor.oCollectionResult != null)
                    {
                        IPTLoadFactor = oIPTLoadFactor;
                    }

                    //General Declines
                    var oClaimLIst = oQSI.oSubStructureI("Claim_T");
                    var oClaimI = (List<XIIBO>)oClaimLIst.oBOIList;
                    if (oClaimI != null && oClaimI.Count > 2)
                    {
                        oGeneralDeclines.Add(BuildCResultObject("0.0", "NoOfClaims", xiEnumSystem.xiEnumPolicyLookupResponses.Refer.ToString()));
                    }
                    List<double> oClaimCostList = new List<double>();
                    if (oClaimI != null)
                    {
                        foreach (var Claim in oClaimI)
                        {
                            double rCost = 0.00;
                            var sCost = Claim.Attributes["rTotalClaimCost"].sValue;
                            if (double.TryParse(sCost, out rCost))
                            {
                            }
                            oClaimCostList.Add(rCost);
                        }
                        if (oClaimCostList.Any(m=>m >= 1500))
                        {
                            oGeneralDeclines.Add(BuildCResultObject("0.0", "Claim Cost", xiEnumSystem.xiEnumPolicyLookupResponses.Refer.ToString()));
                        }
                        //var oExcess = GetExcessLoad(oClaimI.Count, oClaimI, sCoverStartDate);
                        // if (oExcess.xiStatus == 0 && oExcess.oCollectionResult.Count > 0)
                        //{
                        //     oXiResults.Add(oExcess);
                        // }
                    }
                    if (!string.IsNullOrEmpty(sPreviousMotorOrCaravanInsurancePolicyDetails) && sPreviousMotorOrCaravanInsurancePolicyDetails.ToLower() == "yes")
                    {
                        oGeneralDeclines.Add(BuildCResultObject("0.0", "Refused or canceled insurance", xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString()));
                    }
                    if (!string.IsNullOrEmpty(sbankruptcy) && sbankruptcy.ToLower() == "yes")
                    {
                        oGeneralDeclines.Add(BuildCResultObject("0.0", "bankrupt", xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString()));
                    }
                    if (!string.IsNullOrEmpty(sfeltroof) && sfeltroof.ToLower() == "no")
                    {
                        oGeneralDeclines.Add(BuildCResultObject("0.0", "If felt roof has not been replaced in last 10 years", xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString()));
                    }
                    if (!string.IsNullOrEmpty(ssMotorOffence) && ssMotorOffence.ToLower() == "ssMotorOffence")
                    {
                        oGeneralDeclines.Add(BuildCResultObject("0.0", "Non-motoring offence", xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString()));
                    }
                    double total = 0.00;
                    double BaseLoad = 0.00;


                    XIIBO oBOI = new XIIBO();
                    XIAPI oXIAPI = new XIAPI();
                    oBOI.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Aggregations");
                    Info.Add("RiskFactorsCount:" + oGeneralDeclines.Count);
                    foreach (var item in oGeneralDeclines)
                    {
                        string sMessage = item.oCollectionResult.Where(m => m.sName == "LoadFactorName").Select(m => m.sValue).FirstOrDefault() + "_" + item.oCollectionResult.Where(m => m.sName == "sMessage").Select(m => m.sValue).FirstOrDefault() + "_" + item.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault();
                        Info.Add(sMessage);
                    }
                    if (oXiResults.All(m => m.oCollectionResult.Where(x => x.sName == "sMessage").All(x => x.sValue == xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString())) && oGeneralDeclines.Count <= 0)
                    {
                        if (oBaseLoadFactor.oCollectionResult != null)
                        {
                            BaseLoad = Convert.ToDouble(oBaseLoadFactor.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault());
                        }
                        total = BaseLoad;
                        if (oXiResults != null && oXiResults.Count() > 0)
                        {
                            int i = 0;
                            foreach (var item in oXiResults)
                            {
                                if (item.oCollectionResult != null)
                                {
                                    if (item.oCollectionResult.Where(m => m.sName == "Type").Select(m => m.sValue).FirstOrDefault() == "Percent")
                                    {
                                        BaseLoad += ((Convert.ToDouble(item.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault()) * 0.01) * BaseLoad);
                                    }
                                    else
                                    {
                                        BaseLoad += Convert.ToDouble(item.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault());
                                    }
                                }
                                i++;
                            }
                        }
                        double NetPremium = Math.Round(BaseLoad, 2);
                        //BaseLoad = Math.Round(BaseLoad, 2);
                        total = BaseLoad;
                        var result = BuildCResultObject(NetPremium.ToString(), "Net Premium", xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString());
                        oXiResults.Add(result);
                        double IPT = 0;
                        double rInterestRate = 0;
                        if (IPTLoadFactor.oCollectionResult != null)
                        {
                            rInterestRate = Convert.ToDouble(IPTLoadFactor.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault());
                            IPT = ((Convert.ToDouble(IPTLoadFactor.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault()) * 0.01) * total);
                            //IPT = Math.Round(IPT, 2);
                            result = BuildCResultObject(String.Format("{0:0.00}", IPT), "Net IPT", xiEnumSystem.xiEnumPolicyLookupResponses.Normal.ToString());
                            oXiResults.Add(result);
                            total += IPT;
                        }
                        total = Math.Round(total, 2);
                        double rFinalQuote = 0;
                        double rPFAmount = 0;
                        double rMonthlyTotal = 0;
                        double rPaymentCharge = 0; double rInsurerCharge = 0; double rAdmin = 0;
                        if (double.TryParse(oProductI.Attributes["rPaymentCharge"].sValue, out rPaymentCharge)) { }
                        if (double.TryParse(oProductI.Attributes["rInsurerCharge"].sValue, out rInsurerCharge)) { }
                        rInsurerCharge += ((rInterestRate * 0.01) * rInsurerCharge);
                        if (double.TryParse(oProductI.Attributes["zDefaultAdmin"].sValue, out rAdmin)) { }
                        rFinalQuote = total + rPaymentCharge + rInsurerCharge + rAdmin;
                        oBOI.Attributes["rInterestAmount"] = new XIIAttribute { sName = "rInterestAmount", sValue = String.Format("{0:0.00}", IPT), bDirty = true };
                        oBOI.Attributes["rInterestRate"] = new XIIAttribute { sName = "rInterestRate", sValue = String.Format("{0:0.00}", rInterestRate), bDirty = true };
                        oBOI.Attributes["rPaymentCharge"] = new XIIAttribute { sName = "rPaymentCharge", sValue = String.Format("{0:0.00}", oProductI.Attributes["rPaymentCharge"].sValue), bDirty = true };
                        oBOI.Attributes["rInsurerCharge"] = new XIIAttribute { sName = "rInsurerCharge", sValue = String.Format("{0:0.00}", oProductI.Attributes["rInsurerCharge"].sValue), bDirty = true };
                        var MinimumDeposit = oXIAPI.GetMinimumDepostAmount(rPaymentCharge, rInsurerCharge, rFinalQuote, rAdmin, sGUID, iInstanceID, iProductID, 0, 0);
                        double rMinDeposit = 0;
                        if (double.TryParse(MinimumDeposit, out rMinDeposit)) { }
                        oBOI.Attributes["rPrice"] = new XIIAttribute { sName = "rPrice", sValue = String.Format("{0:0.00}", total), bDirty = true };
                        oBOI.Attributes["rQuotePremium"] = new XIIAttribute { sName = "rQuotePremium", sValue = String.Format("{0:0.00}", total), bDirty = true };
                        oBOI.Attributes["rGrossPremium"] = new XIIAttribute { sName = "rGrossPremium", sValue = String.Format("{0:0.00}", total), bDirty = true };
                        oBOI.Attributes["rFinalQuote"] = new XIIAttribute { sName = "rFinalQuote", sValue = String.Format("{0:0.00}", rFinalQuote), bDirty = true };
                        oBOI.Attributes["zDefaultDeposit"] = new XIIAttribute { sName = "zDefaultDeposit", sValue = String.Format("{0:0.00}", MinimumDeposit), bDirty = true };
                        oBOI.Attributes["zDefaultAdmin"] = new XIIAttribute { sName = "zDefaultAdmin", sValue = String.Format("{0:0.00}", oProductI.Attributes["zDefaultAdmin"].sValue), bDirty = true };
                        var PFSchemeID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iPFSchemeID}");
                        int iPFSchemeID = 0;
                        if (int.TryParse(PFSchemeID, out iPFSchemeID))
                        { }
                        var MonthlyAmount = oXIAPI.GetMonthlyPremiumAmount(rFinalQuote, rMinDeposit, iProductID, 0, 0, iPFSchemeID);
                        Info.Add("Monthly Amount:" + MonthlyAmount);
                        rMonthlyTotal = (MonthlyAmount * 10) + rMinDeposit;
                        oBOI.Attributes["rMonthlyPrice"] = new XIIAttribute { sName = "rMonthlyPrice", sValue = String.Format("{0:0.00}", MonthlyAmount), bDirty = true };
                        oBOI.Attributes["rMonthlyTotal"] = new XIIAttribute { sName = "rMonthlyTotal", sValue = String.Format("{0:0.00}", rMonthlyTotal), bDirty = true };
                        rPFAmount = rMonthlyTotal - rMinDeposit;
                        oBOI.Attributes["rPremiumFinanceAmount"] = new XIIAttribute { sName = "rPremiumFinanceAmount", sValue = String.Format("{0:0.00}", rPFAmount), bDirty = true };
                        oBOI.Attributes["bIsCoverAbroad"] = new XIIAttribute { sName = "bIsCoverAbroad", sValue = oProductI.Attributes["bIsCoverAbroad"].sValue, bDirty = true };
                        oBOI.Attributes["iQuoteStatus"] = new XIIAttribute { sName = "iQuoteStatus", sValue = "0", bDirty = true };
                    }
                    else if (oGeneralDeclines != null && oGeneralDeclines.Count() > 0 && oGeneralDeclines.Any(m => m.oCollectionResult.Where(x => x.sName == "sMessage").Any(x => x.sValue == xiEnumSystem.xiEnumPolicyLookupResponses.Decline.ToString())))
                    {
                        Info.Add("Some Load Factors are Declined");
                        oBOI.Attributes["iQuoteStatus"] = new XIIAttribute { sName = "iQuoteStatus", sValue = "20", bDirty = true };
                        oBOI.Attributes["rPrice"] = new XIIAttribute { sName = "rPrice", sValue = "0.00", bDirty = true };
                        oBOI.Attributes["rFinalQuote"] = new XIIAttribute { sName = "rFinalQuote", sValue = "0.00", bDirty = true };
                        oBOI.Attributes["zDefaultDeposit"] = new XIIAttribute { sName = "zDefaultDeposit", sValue = "0.00", bDirty = true };
                        oBOI.Attributes["zDefaultAdmin"] = new XIIAttribute { sName = "zDefaultAdmin", sValue = "0.00", bDirty = true };
                        oBOI.Attributes["rMonthlyPrice"] = new XIIAttribute { sName = "rMonthlyPrice", sValue = "0.00", bDirty = true };
                    }
                    var sQuoteGUID = Guid.NewGuid().ToString("N").Substring(0, 10);
                    var oSource = oIXI.BOI("XISource_T", ostructureInstance.Attributes["fkisourceid"].sValue);
                    string sPrefix = string.Empty;
                    if (oSource.Attributes != null && oSource.Attributes.ContainsKey("sprefixcode"))
                    {
                        sPrefix = oSource.Attributes["sprefixcode"].sValue;
                    }
                    var iBatchID = iCustomerID.ToString() + iInstanceID.ToString();
                    oBOI.Attributes["sRegNo"] = new XIIAttribute { sName = "sRegNo", sValue = ostructureInstance.XIIValue("sRegNo").sValue, bDirty = true };
                    oBOI.Attributes["dCoverStart"] = new XIIAttribute { sName = "dCoverStart", sValue = sCoverStartDate, bDirty = true };
                    oBOI.Attributes["sCaravanMake"] = new XIIAttribute { sName = "sCaravanMake", sValue = ostructureInstance.XIIValue("sCaravanMakeUpdated").sDerivedValue, bDirty = true };
                    oBOI.Attributes["sCaravanModel"] = new XIIAttribute { sName = "sCaravanModel", sValue = ostructureInstance.XIIValue("sModelofCaravanUpdated").sValue, bDirty = true };
                    oBOI.Attributes["FKiQSInstanceID"] = new XIIAttribute { sName = "FKiQSInstanceID", sValue = iInstanceID.ToString(), bDirty = true };
                    oBOI.Attributes["sInsurer"] = new XIIAttribute { sName = "sInsurer", sValue = oProductI.Attributes["sName"].sValue, bDirty = true };
                    oBOI.Attributes["FKiCustomerID"] = new XIIAttribute { sName = "FKiCustomerID", sValue = iCustomerID.ToString(), bDirty = true };
                    oBOI.Attributes["FKiUserID"] = new XIIAttribute { sName = "FKiUserID", sValue = iUserID.ToString(), bDirty = true };
                    oBOI.Attributes["dtqsupdateddate"] = new XIIAttribute { sName = "dtqsupdateddate", sValue = DateTime.Now.ToString(), bDirty = true };
                    oBOI.Attributes["CreatedTime"] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                    oBOI.Attributes["FKiProductVersionID"] = new XIIAttribute { sName = "FKiProductVersionID", sValue = sVersion, bDirty = true };
                    oBOI.Attributes["sGUID"] = new XIIAttribute { sName = "sGUID", sValue = sQuoteGUID, bDirty = true };
                    Random generator = new Random();
                    string sRef = generator.Next(1, 10000000).ToString(new String('0', 7));
                    oBOI.Attributes["sRefID"] = new XIIAttribute { sName = "sRefID", sValue = sPrefix + sRef, bDirty = true };
                    //oBII.Attributes["sRefID"] = new XIIAttribute { sName = "sRefID", sValue = sPrefix + Guid.NewGuid().ToString("N").Substring(0, 6), bDirty = true };
                    //oBOI.Attributes["sRefID"] = new XIIAttribute { sName = "sRefID", sValue = Guid.NewGuid().ToString("N").Substring(0, 6), bDirty = true };
                    oBOI.Attributes["BatchID"] = new XIIAttribute { sName = "BatchID", sValue = iBatchID, bDirty = true };
                    oBOI.Attributes["ID"] = new XIIAttribute { sName = "ID", sValue = iQuoteID.ToString(), bDirty = true };
                    oBOI.Attributes["rCompulsoryExcess"] = new XIIAttribute { sName = "rCompulsoryExcess", sValue = String.Format("{0:0.00}", rCompulsaryExcess), bDirty = true };
                    oBOI.Attributes["rVoluntaryExcess"] = new XIIAttribute { sName = "rVoluntaryExcess", sValue = String.Format("{0:0.00}", rVoluntaryExcess), bDirty = true };
                    double rTotalExcess = rCompulsaryExcess + rVoluntaryExcess;
                    oBOI.Attributes["rTotalExcess"] = new XIIAttribute { sName = "rTotalExcess", sValue = String.Format("{0:0.00}", rTotalExcess), bDirty = true };
                    //Info.Add("QuoteRefID_" + oBOI.Attributes["sRefID"].sValue);
                    var oRes = oBOI.Save(oBOI);
                    if (oRes.bOK && oRes.oResult != null)
                    {
                        oBOI = (XIIBO)oRes.oResult;
                    }
                    Info.Add(oProductI.Attributes["sName"].sValue + "Quote inserted Sucessfully with the amount of " + total);
                    XIDBO oRiskFactorsBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "RiskFactor_T");
                    XIIBO oBO = new XIIBO();
                    oBO.BOD = oRiskFactorsBOD;
                    List<XIIBO> oBOIList = new List<XIIBO>();
                    oXiResults.Add(IPTLoadFactor);
                    if (oGeneralDeclines.Count > 0)
                    {
                        Info.Add("oGeneralDeclines: Declined" + oGeneralDeclines.Count);
                        foreach (var declineCase in oGeneralDeclines)
                        {
                            oXiResults.Add(declineCase);
                        }
                    }
                    oBO.Attributes["FKiQuoteID"] = new XIIAttribute { sName = "FKiQuoteID", sValue = oBOI.Attributes["ID"].sValue, bDirty = true };
                    oBO.Attributes["sFactorName"] = new XIIAttribute { sName = "sFactorName", sValue = "Base Rate", bDirty = true };
                    oBO.Attributes["sValue"] = new XIIAttribute { sName = "sValue", sValue = oBaseLoadFactor.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault(), bDirty = true };
                    oBO.Attributes["sMessage"] = new XIIAttribute { sName = "sMessage", sValue = oBaseLoadFactor.oCollectionResult.Where(m => m.sName == "sMessage").Select(m => m.sValue).FirstOrDefault(), bDirty = true };
                    oBO.Attributes["CreatedTime"] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                    oBO.Attributes["FKsQuoteID"] = new XIIAttribute { sName = "FKsQuoteID", sValue = sQuoteGUID, bDirty = true };
                    oBO.Attributes["ID"] = new XIIAttribute { sName = "ID", bDirty = true };
                    oBOIList.Add(oBO);
                    foreach (var item in oXiResults)
                    {
                        oBO = new XIIBO();
                        oBO.BOD = oRiskFactorsBOD;
                        oBO.Attributes["FKiQuoteID"] = new XIIAttribute { sName = "FKiQuoteID", sValue = oBOI.Attributes["ID"].sValue, bDirty = true };
                        oBO.Attributes["sFactorName"] = new XIIAttribute { sName = "sFactorName", sValue = item.oCollectionResult.Where(m => m.sName == "LoadFactorName").Select(m => m.sValue).FirstOrDefault(), bDirty = true };
                        oBO.Attributes["sValue"] = new XIIAttribute { sName = "sValue", sValue = item.oCollectionResult.Where(m => m.sName == "oResult").Select(m => m.sValue).FirstOrDefault(), bDirty = true };
                        oBO.Attributes["sMessage"] = new XIIAttribute { sName = "sMessage", sValue = item.oCollectionResult.Where(m => m.sName == "sMessage").Select(m => m.sValue).FirstOrDefault(), bDirty = true };
                        oBO.Attributes["CreatedTime"] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                        oBO.Attributes["FKsQuoteID"] = new XIIAttribute { sName = "FKsQuoteID", sValue = sQuoteGUID, bDirty = true };
                        oBO.Attributes["ID"] = new XIIAttribute { sName = "ID", bDirty = true };
                        oBOIList.Add(oBO);
                    }
                    XIIBO xibulk = new XIIBO();
                    DataTable dtbulk = xibulk.MakeBulkSqlTable(oBOIList);
                    xibulk.SaveBulk(dtbulk, oBOIList[0].BOD.iDataSource, oBOIList[0].BOD.TableName);
                    oCResult.oCollectionResult.Add(new CNV { sName = "QuoteID", sValue = oBOI.Attributes["ID"].sValue });
                    oCResult.sMessage = "Success";
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = "Success";
                    string sInfo = "INFO: " + string.Join(",\r\n ", Info);
                    oCResult.sMessage = sInfo;
                    oIB.SaveErrortoDB(oCResult);
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = total;
                }
                catch (Exception ex)
                {
                    string sInfo = "INFO: " + string.Join(",\r\n ", Info);
                    oCResult.sMessage = sInfo;
                    oIB.SaveErrortoDB(oCResult);
                    oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oCResult.oResult = "Error";
                    oIB.SaveErrortoDB(oCResult);
                }
                return oCResult;
            }
        }

    }
}