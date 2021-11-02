using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using XICore;
using XIDNA.Hubs;
using XIDNA.Repository;

namespace XIDNA
{
    public class Constants
    {
        public const string Admin = "Admin";
        public const int SetupAdminMenuID = 1;
    }

    public class SignalR: iSiganlR
    {
        public void HitSignalR(int InstanceID, int ProductversionID, string sRoleName, string sDatabase,string sGUID,string sSessionID,int iQuoteType)
        {
            //string sDatabase = "XICoreQA";
            CommonRepository Common = new CommonRepository();
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                var ConnectionID = oCache.Get_ParamVal(sSessionID, sGUID, "", "SignalRConnectionID");
               ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, InstanceID);
                var oStepD=oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                var IsStepLock = false;
                if (oStepD.iLockStage != 0 && oStepD.iLockStage <= oQSInstance.iStage)
                {
                    IsStepLock = true;
                }
                Common.SaveErrorLog("Log: SignalREvent Method started Execution" + InstanceID +" " +ProductversionID, sDatabase);
                // NOTE: EXECUTE THE BELOW QUERIES HERE
                // Select ID,iQuoteStatus,rCompulsoryExcess, rVoluntaryExcess, rTotalExcess, rMonthlyPrice, rMonthlyTotal, zDefaultDeposit, rFinalQuote AS Yearly from Aggregations_T Where FKiQSInstanceID = 51787 and FKiProductVersionID = 2 and XIDeleted = 0
                //SELECT rBestQuote FROM Lead_T where FKiQSInstanceID=51787
                XIIBO oXiBo = new XIIBO();
                XIIBO oQuoteI = new XIIBO();
                QueryEngine oQE = new QueryEngine();
                //string sWhereCondition = $"FKiQSInstanceID={InstanceID},FKiProductVersionID={ProductversionID},XIDeleted=0";
                string sWhereCondition = "FKiQSInstanceID="+InstanceID+",FKiProductVersionID="+ProductversionID+ ",XIDeleted=0, iType=" + iQuoteType;
                var oQResult = oQE.Execute_QueryEngine("Aggregations", "sGUID,ID,iQuoteStatus,rCompulsoryExcess, rVoluntaryExcess, rTotalExcess, rMonthlyPrice, rMonthlyTotal, zDefaultDeposit, rFinalQuote, bIsFlood, bIsApplyFlood", sWhereCondition);
                if (oQResult.bOK && oQResult.oResult != null)
                {
                    oXiBo = ((Dictionary<string, XIIBO>)oQResult.oResult).Values.OrderByDescending(f => f.AttributeI("ID").iValue).FirstOrDefault();
                }
                if(oXiBo == null)
                {
                    string sWhereQuote = "FKiQSInstanceID = " + InstanceID;
                    var oQuoteResult = oQE.Execute_QueryEngine("Aggregations", "ID,iQuoteStatus", sWhereQuote);
                    if (oQuoteResult.bOK && oQuoteResult.oResult != null)
                    {
                        oQuoteI = ((Dictionary<string, XIIBO>)oQuoteResult.oResult).Values.FirstOrDefault();
                    }
                }
                // Get the product id from ProductVersionID
                XIIBO oXiProduct = new XIIBO();
                XIIXI oIXI = new XIIXI();
                oXiProduct = oIXI.BOI("ProductVersion_T", ProductversionID.ToString(), "FKiProductID,bIsIndicativePrice");
                int iProductID = 0;bool bIsIndicativePrice = false;
                if (oXiProduct != null && oXiProduct.Attributes.ContainsKey("FKiProductID"))
                {
                    iProductID = oXiProduct.AttributeI("FKiProductID").iValue;
                    bIsIndicativePrice=oXiProduct.AttributeI("bIsIndicativePrice").bValue;
                }
                // QUERY FOR BEST QUOTE. 
                XIIBO OLeadBO = new XIIBO();
                string sWhereLead = "FKiQSInstanceID = "+InstanceID;
                var oLeadQResult = oQE.Execute_QueryEngine("Lead_T", "rBestQuote", sWhereLead);
                if (oLeadQResult.bOK && oLeadQResult.oResult != null)
                {
                    OLeadBO = ((Dictionary<string, XIIBO>)oLeadQResult.oResult).Values.FirstOrDefault();
                }
                // BUILD ONE STANDARD ANNONYMOUS OBJECT HERE
                if (oXiBo != null && OLeadBO != null)
                {
                    var oAnnonymous = new
                    {
                        IsLockStep = IsStepLock,
                        ProductversionID = ProductversionID,
                        QSInstanceID = InstanceID,
                        ProductID = iProductID,
                        iQuoteStatus = oXiBo.AttributeI("iQuoteStatus").sValue,
                        rCompulsoryExcess = oXiBo.AttributeI("rCompulsoryExcess").rValue,
                        rVoluntaryExcess = oXiBo.AttributeI("rVoluntaryExcess").rValue,
                        rTotalExcess = oXiBo.AttributeI("rTotalExcess").rValue,
                        rMonthlyPrice = oXiBo.AttributeI("rMonthlyPrice").rValue,
                        rMonthlyTotal = oXiBo.AttributeI("rMonthlyTotal").rValue,
                        zDefaultDeposit = oXiBo.AttributeI("zDefaultDeposit").rValue,
                        rFinalQuote = oXiBo.AttributeI("rFinalQuote").rValue,
                        rBestQuote = OLeadBO.AttributeI("rBestQuote").rValue,
                        QuoteID = oXiBo.AttributeI("sGUID").sValue,
                        RoleName = sRoleName,
                        bIsIndicativePrice = bIsIndicativePrice,
                        sQSType = oQSInstance.sQSType,
                        bIsFlood = oXiBo.AttributeI("bIsFlood").sValue,
                        bIsApplyFlood = oXiBo.AttributeI("bIsApplyFlood").sValue
                    };
                    Common.SaveErrorLog("Log: SignalREvent Method Executed addNewMessageToPage "+InstanceID+" "+ProductversionID, sDatabase);
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
                    //hubContext.Clients.All.addNewMessageToPage(oAnnonymous);
                    //string ConnectionID = "";
                    hubContext.Clients.Client(ConnectionID).addNewMessageToPage(oAnnonymous);
                }
                else if (OLeadBO != null)
                {
                    var oAnnonymous = new
                    {
                        rBestQuote = OLeadBO.AttributeI("rBestQuote").rValue,
                        QSInstanceID = InstanceID,
                        iQuoteStatus = oQuoteI.AttributeI("iQuoteStatus").sValue,
                    };
                    Common.SaveErrorLog("Log: SignalREvent Method Executed rBestQuote addNewMessageToPage "+InstanceID+" "+ProductversionID, sDatabase);
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
                    //hubContext.Clients.All.addNewMessageToPage(oAnnonymous);
                    hubContext.Clients.Client(ConnectionID).addNewMessageToPage(oAnnonymous);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                Common.SaveErrorLog("ErrorLog: SignalREvent" + ex.ToString(), sDatabase);
                throw ex;
            }
        }

        public void ShowSignalRMsg(string sMessage)
        {
            XIInfraCache oCache = new XIInfraCache();
            var connid = SessionManager.sSignalRCID;
            if (string.IsNullOrEmpty(connid))
            {
                connid = Guid.NewGuid().ToString();
            }
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
            //hubContext.Clients.Client(connid).addNewMessageToPage(sMessage);
            hubContext.Clients.All.addNewMessageToPage(sMessage);
        }

    }
}