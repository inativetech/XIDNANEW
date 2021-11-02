using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using xiEnumSystem;
using XISystem;

namespace XICore
{

    //souce: https://www.vivienfabing.com/dotnetcore/2019/04/14/how-to-send-a-dynamic-email-from-sendgrid-with-dotnet-core.html
    //about SendGrid: https://github.com/sendgrid/sendgrid-csharp
    /// <summary>
    /// This class for SendGrid emails
    /// </summary>
    public class XIInfraSendGridComponent : XIInstanceBase
    {
        /// <summary>
        /// receiver email id
        /// </summary>
        public string sTo { get; set; }

        /// <summary>
        /// receiver name
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// cc email id
        /// </summary>
        public string sCC { get; set; }

        /// <summary>
        /// cc name
        /// </summary>
        public string sCCName { get; set; }

        /// <summary>
        /// populate data in SendGrid Dynamic Email Template. create dynamic object when using Dynamic Template
        /// </summary>
        public object oDynamicData { get; set; }

        /// <summary>
        /// sBody contains plain text or html string. This property use when email contains custom body (user definde html or content)
        /// </summary>
        public string sBody { get; set; }

        /// <summary>
        /// Subject of Email
        /// </summary>
        public string sSubject { get; set; }

        /// <summary>
        /// SengGrid TemplateID
        /// </summary>
        //public string sTemplateID { get; set; }



        private readonly XIInfraCache oCache = new XIInfraCache();
        /// <summary>
        /// This method send the mail and returns cResult Object
        /// </summary>
        /// <returns>CResult Object</returns>
        /// 
        public CResult Load(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            long iTraceLevel = 10;
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
                //oCResult.oTraceStack.Trace("Stage",sError)
            }
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";
            try
            {
                //accountid
                //templateName  --> welcome mail
                //new method for template with templateName

                //get params
                int iSGADID = 0;
                int.TryParse(oParams.Where(m => m.sName.ToLower() == XIConstant.Param_SendGridAccountID.ToLower()).Select(m => m.sValue).FirstOrDefault(), out iSGADID);
                string sTemplateName = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_SendGridTemplateName.ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (iSGADID > 0 && !string.IsNullOrEmpty(sTemplateName))
                {
                    var oSendGridInfo = (XIDSendGridAccountDetails)oCache.GetObjectFromCache(XIConstant.CacheSendGridAccount, null, iSGADID.ToString());
                    var oSendGridTemplate = (XIDSendGridTemplate)oCache.GetObjectFromCache(XIConstant.CacheSendGridTemplate, null, sTemplateName);

                    if (!string.IsNullOrEmpty(sTo))
                    {

                        string sSendGridAPIKey = oSendGridInfo?.sAPIKey,
                        sSingleSender = oSendGridInfo?.sSingleSender,
                        sSingleSenderName = oSendGridInfo?.sSingleSenderName;

                        if (!string.IsNullOrEmpty(oSendGridInfo?.sAPIKey) && !string.IsNullOrEmpty(sSingleSender))
                        {
                            var Client = new SendGridClient(sSendGridAPIKey);
                            var sendGridMessage = new SendGridMessage();
                            sendGridMessage.SetFrom(sSingleSender, sSingleSenderName);
                            sendGridMessage.AddTo(sTo, sName);
                            if (!string.IsNullOrEmpty(oSendGridTemplate?.sTemplateID) && oDynamicData != null)
                            {
                                sendGridMessage.SetTemplateId(oSendGridTemplate.sTemplateID);
                                sendGridMessage.SetTemplateData(oDynamicData);

                            }
                            else
                            {
                                //write email body code if without using templateid (dynamic email template)
                            }
                            if (!string.IsNullOrEmpty(sCC))
                                sendGridMessage.AddBcc(sCC, sCCName);

                            var response = Client.SendEmailAsync(sendGridMessage).Result;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                Dictionary<string, string> Headers = response.DeserializeResponseHeaders(response.Headers);
                                if (Headers.Keys.Any(m => m.ToLower() == "x-message-id"))
                                {
                                    string sMessageID = Headers["X-Message-Id"];
                                    //save object in communicationInstance_t table along with sMessageID
                                    oCR = SaveEmailActivity(sMessageID, sSingleSender);
                                    if(oCR.bOK && oCR.oResult != null)
                                    {
                                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                        oCResult.oResult = response;
                                    }
                                    else
                                    {
                                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                        oCResult.sMessage = "Mail activity not saved correctly";
                                        SaveErrortoDB(oCResult);
                                    }
                                }
                            }                            
                        }
                        else
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                            oCResult.sMessage = "Mandatory params not passed correctly";
                            SaveErrortoDB(oCResult);
                        }
                    }
                    else
                    {
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                        oCResult.sMessage = "sTo param not passed correctly";
                        SaveErrortoDB(oCResult);
                    }
                }
                else
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oCResult.sMessage = "Mandatory params not passed correctly";
                    SaveErrortoDB(oCResult);
                }
                    
            }
            catch (Exception ex)
            {
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        private CResult SaveEmailActivity(string sMessageID, string sFrom)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            long iTraceLevel = 10;
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
                //oCResult.oTraceStack.Trace("Stage",sError)
            }
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "communicationinstance");
                XIIBO oBOI = new XIIBO();
                oBOI.BOD = BOD;
                oBOI.SetAttribute("sFrom", sFrom);
                oBOI.SetAttribute("sTo", sTo);
                //oBOI.SetAttribute("iType", ((int)EnumCommnicationType.Email).ToString());
                oBOI.SetAttribute("sReference", sMessageID);
                oBOI.SetAttribute("FKiToContactID", "0");//will be change later
                oBOI.SetAttribute("sDirection", "outbound");
                oBOI.SetAttribute("iRetryCount", "0");
                oBOI.SetAttribute("sAPI", "");
                oBOI.SetAttribute("iStatus", ((int)EnumEmailStatus.sent).ToString());
                //oBOI.SetAttribute("iDeliveryStatus", ((int)EnumEmailStatus.processed).ToString());
                oBOI.SetAttribute("iInteractionStatus", ((int)EnumEmailStatus.processed).ToString());
                int iUserID = 0;
                int.TryParse(HttpContext.Current.Session["UserID"].ToString(), out iUserID);

                oBOI.SetAttribute("CreatedByID", iUserID.ToString());
                string sUser = "";
                if (iUserID > 0)
                {
                    sUser = HttpContext.Current.Session["sUserName"].ToString();
                }
                oBOI.SetAttribute("CreatedByName", sUser);
                oBOI.SetAttribute("CreatedDate", DateTime.Now.ToString());

                oCR = oBOI.Save(oBOI);
                int ID = 0;
                if (oCR.bOK && oCR.oResult != null)
                {
                    int.TryParse(oBOI.Attributes["ID"].sValue, out ID);
                    if (ID > 0)
                    {
                        oCache = new XIInfraCache();
                        BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "commsTransaction");
                        oBOI = new XIIBO();
                        oBOI.BOD = BOD;
                        oBOI.SetAttribute("FKiCommInstanceID", ID.ToString());
                        oBOI.SetAttribute("iType", ((int)EnumEmailStatus.processed).ToString());
                        oCR = oBOI.Save(oBOI);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oCResult.oResult = oCR.oResult;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;                            
                        }
                        else
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                            oCResult.sMessage = "Error while saving into commsTransaction object";
                            SaveErrortoDB(oCResult);
                        }
                    }
                    else
                    {
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                        oCResult.sMessage = "Error while saving into communication instance object";
                        SaveErrortoDB(oCResult);
                    }
                }
                else
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oCResult.sMessage = "Error while saving into communication instance object";
                    SaveErrortoDB(oCResult);
                }
            }
            catch (Exception ex)
            {
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            return oCResult;

        }

    }
}