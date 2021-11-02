using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XIDNA.Repository;
using XIDNA.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using XIDNA.ViewModels;
using Microsoft.AspNet.SignalR;
using XIDNA.Hubs;
using System.Data.SqlClient;
using System.Data.Entity;
using System.IO;
using XIDNA.Common;
using ZeeInsurance;
using System.Configuration;
using XICore;
using XISystem;
using System.Diagnostics;

namespace XIDNA.Controllers
{
    // [System.Web.Mvc.Authorize]
    public class MailController : Controller
    {
        LeadImport LeadImport = new LeadImport();
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMailRepository MailRepository;

        public MailController() : this(new MailRepository()) { }

        public MailController(MailRepository MailRepository)
        {
            this.MailRepository = MailRepository;
        }
        CommonRepository Common = new CommonRepository();
        XIInfraCache oCache = new XIInfraCache();
        //
        // GET: /Mail/
        public ActionResult Index()
        {
            string sDatabase = SessionManager.ConfigDatabase;
            try
            {
                //int OrgID = 0;
                //var Mail = MailRepository.GetEmailCredentials(OrgID);
                IOServerDetails Mail = new IOServerDetails();
                XID1Click PV1Click = new XID1Click();
                XIDStructure oStructure = new XIDStructure();
                List<CNV> nParams = new List<CNV>();

                var o1ClickIO = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "IOServerDetails", null);
                PV1Click = (XID1Click)o1ClickIO.Clone(o1ClickIO);
                var oQueryIO = oStructure.ReplaceExpressionWithCacheValue(o1ClickIO.Query, nParams);
                PV1Click.Query = oQueryIO;
                PV1Click.Name = "XIIOServerDetails_T";
                var oOneClickIO = PV1Click.OneClick_Execute();

                var o1ClickAPP = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "AppSettings", null);
                PV1Click = (XID1Click)o1ClickAPP.Clone(o1ClickAPP);
                var oQueryAPP = oStructure.ReplaceExpressionWithCacheValue(o1ClickAPP.Query, nParams);
                PV1Click.Query = oQueryAPP;
                PV1Click.Name = "XIAPISettings_T";
                var oOneClickAPP = PV1Click.OneClick_Execute();
                List<string> ListApp = new List<string>();
                ListApp = oOneClickAPP.ToList().Select(m => m.Value.Attributes.Where(s => s.Key == "scode").Select(s => s.Value.sValue).FirstOrDefault()).ToList();
                Dictionary<int, string> ListIO = new Dictionary<int, string>();
                for (int j = 0; j < oOneClickIO.Values.Count(); j++)
                {
                    var Value = oOneClickIO.Values.ElementAt(j).Attributes.Values.Where(s => s.sName == "ID").Select(s => s.iValue).FirstOrDefault();
                    var text = oOneClickIO.Values.ElementAt(j).Attributes.Values.Where(s => s.sName.ToLower() == "fromaddress").Select(s => s.sPreviousValue).FirstOrDefault();
                    ListIO.Add(Value, text);
                }
                VMDropDown Mails = new VMDropDown();
                Mail.MailIDs = new List<VMDropDown>();
                var APPIO = ListApp.Where(b => ListIO.Any(a => b.Contains(a.Value)));
                if (APPIO != null)
                {
                    foreach (var item in APPIO)
                    {
                        int Value = ListIO.Where(s => s.Value == item).Select(s => s.Key).FirstOrDefault();
                        var text = item;
                        Mails.Value = Value;
                        Mails.text = text;
                        Mail.MailIDs.Add(Mails);
                    }
                }
                return View(Mail);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        //Display partial view for email to display on button click 
        public ActionResult ImportEmails()
        {
            return View();
        }

        //Getting folder list
        [HttpPost]
        public ActionResult SelectFoldersWithIMAP(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int OrgID = 0;
                var sFolderList = LeadImport.Select_FoldersWithIMAP(ID, OrgID);
                return Json(sFolderList.oResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //Getting Subject
        public ActionResult GetSubjectWithIMAP(int ID, string sFolder)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int OrgID = 0;
                string Flag = string.Empty;
                var sSubjects = LeadImport.Get_EmailSubjects(ID, sFolder, OrgID, Flag);
                return Json(sSubjects.oResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //On selecting subject get email details which is saved to DB
        public ActionResult GetEmailDetailsByUID(int ID, int iUID, string sFolder, string sSubject)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int OrgID = 0;
                var sEmailDetails = LeadImport.Get_EmailFullDetails(ID, iUID, sFolder);
                return Json(sEmailDetails.oResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult SaveSelectedEmailByUID(int ID, int iUID, string sFolder)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {

                int UserID = Convert.ToInt32(User.Identity.GetUserId());
                var Response = LeadImport.Save_MailContent(ID, iUID, sFolder);
                if (Response.bOK && Response.oResult != null)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Failure", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var LeadDetails = new List<List<string>>();
                List<string> NoData = new List<string>();
                NoData.Add("Error while saving mail");
                LeadDetails.Add(NoData);
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(LeadDetails, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult MailExtractStrings()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int orgid = 0;
                return PartialView("_MailExtractStrings", orgid);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }
        public ActionResult AddEditMailExtractStrings(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                if (ID == 0)
                {
                    int orgid = 0;
                    VMMailExtractStrings model = new VMMailExtractStrings();
                    model.SubscriptionList = MailRepository.AddEditMailExtractStrings(orgid, sDatabase);
                    return View("AddEditMailExtractStrings", model);
                }
                else
                {
                    int orgid = 0;

                    var row = MailRepository.GetMailExtractStringsRow(ID, orgid, sDatabase);
                    VMMailExtractStrings model = new VMMailExtractStrings();
                    model.SubscriptionList = MailRepository.AddEditMailExtractStrings(orgid, sDatabase);

                    model.ID = ID;
                    model.SubscriptionID = row.SubscriptionID;
                    model.sStartString = row.sStartString;
                    model.sEndString = row.sEndString;
                    model.SourceID = row.SourceID;
                    model.StatusTypeID = row.StatusTypeID;
                    model.OrganizationID = row.OrganizationID;
                    return View("AddEditMailExtractStrings", model);
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }
        [HttpPost]
        public ActionResult SaveMailExtractStrings(VMMailExtractStrings model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int orgid = 0;
                model.OrganizationID = orgid;
                var result = MailRepository.SaveMailExtractStrings(model, orgid, sDatabase);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult MailExtractStringsGrid(jQueryDataTableParamModel param, int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = MailRepository.MailExtractStringsGrid(param, OrgID, sDatabase);
                return Json(new
                {
                    sEcho = result.sEcho,
                    iTotalRecords = result.iTotalRecords,
                    iTotalDisplayRecords = result.iTotalDisplayRecords,
                    aaData = result.aaData
                },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult MailExtractStringsPopUp(int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return PartialView("_MailExtractStringsPopUp", OrgID);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }


        public ActionResult AppNotifications()
        {
            return View();
        }

        public ActionResult AddAppNotification()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                VMAppNotifications model = new VMAppNotifications();
                model.Roles = MailRepository.GetOrgRoles(0, sDatabase);
                model.GetUsers = MailRepository.GetUsers(0, sDatabase);
                return PartialView("_AddNotificationForm", model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult AppNotificationsGrid(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int OrgID = 0;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = MailRepository.AppNotificationsGrid(param, OrgID, sDatabase);
                return Json(new
                {
                    sEcho = result.sEcho,
                    iTotalRecords = result.iTotalRecords,
                    iTotalDisplayRecords = result.iTotalDisplayRecords,
                    aaData = result.aaData
                },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        [HttpPost]
        public ActionResult SaveAppNotification(VMAppNotifications model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                model.OrganizationID = 0;
                int UserID = Convert.ToInt32(User.Identity.GetUserId());
                var Res = MailRepository.SaveAppNotification(model, UserID, sDatabase);
                return Json(Res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult EditAppNotification(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int OrgID = 0;
                var row = MailRepository.GetOrgRoles(OrgID, sDatabase);
                var obj = MailRepository.EditAppNotifications(ID, sDatabase);
                obj.GetUsers = MailRepository.GetUsers(obj.OrganizationID, sDatabase);
                obj.Roles = row;
                return View("_AddNotificationForm", obj);
            }

            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        //public ActionResult UploadFiles(int id, HttpPostedFileBase ImageName)
        //{
        //    try
        //    {
        //        if (ImageName != null)
        //        {
        //            string ext = Path.GetExtension(ImageName.FileName);
        //            string physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
        //            string str = physicalPath.Substring(0, physicalPath.Length) + "\\Content\\images";
        //            var Image = "Notification_" + id + ext;
        //            ImageName.SaveAs(str + "\\" + Image);
        //            var res = MailRepository.SaveNotificationImage(id, Image, Util.GetDatabaseName());
        //        }
        //        return Content(id.ToString(), "text/plain");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        return Content("0", "text/plain");
        //    }
        //}

        public ActionResult GetUsers(int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var Name = MailRepository.GetUsers(OrgID, sDatabase);
                return Json(Name, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SendNotificationForAndroid(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int OrgID = 0;
                var row = MailRepository.GetOrgRoles(OrgID, sDatabase);
                var Res = MailRepository.SendNotification(ID, sDatabase);
                return Json(Res, JsonRequestBehavior.AllowGet);
                //obj.GetUsers = MailRepository.GetUsers(obj.OrganizationID);
                //obj.Roles = row;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SendMail(List<CNV> Params, string sBOName)
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
                int ID = 0;
                XIIXI oXII = new XIIXI();
                XIIBO oBOI = new XIIBO();
                XIInfraSendGridComponent oSendGrid = new XIInfraSendGridComponent();
      
                foreach (var item in Params)
                {
                    int.TryParse(item.sValue, out ID);
                    if(ID > 0)
                    {
                        List<CNV> oWhrParams = new List<CNV>();
                        oWhrParams.Add(new CNV { sName = "ID", sValue = ID.ToString() });
                         oBOI = oXII.BOI(sBOName, ID.ToString());
                        if(oBOI.Attributes?.Count() > 0)
                        {
                            oSendGrid.sTo = oBOI.Attributes["sEmail"].sValue;
                            oSendGrid.sName = oBOI.Attributes["sName"].sValue;
                            oSendGrid.oDynamicData = new
                            {
                                subject = "Send Grid Using Dynamic Template",
                                header = "dynamic header",
                                name = oSendGrid.sName,
                                address = "west godavari",
                                emailID = oSendGrid.sTo,
                                url = "www.google.com",
                                buttonName = "Go to Google"
                            };

                            var listParams = new List<CNV>();
                            listParams.Add(new CNV { sName = "SendGridAccountID", sValue = "1" });
                            listParams.Add(new CNV { sName = "SendGridTemplateName", sValue = "test" });
                            var result = oSendGrid.Load(listParams);
                            if (result.bOK)
                            {
                                oCResult = result;
                            }
                        }
                    }
                }

                //these code testing purpose
            
                //oSendGrid.sTo = "raviteja.m@inativetech.com";
                //oSendGrid.sName = "sarvesh";
                //oSendGrid.sCC = "sarveswararao.s@inativetech.com";
                //oSendGrid.sCCName = "sarveswararao";

                //oSendGrid.sTemplateID = "d-467b33cf4fbc43238f748f90b068b308";
                //oSendGrid.iSGADID = 1;
                
            }
            catch (Exception ex)
            {
                XIInstanceBase oInstanceBase = new XIInstanceBase();
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oInstanceBase.SaveErrortoDB(oCResult);

            }

            return Json(oCResult.oResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMail_Test()
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
                int ID = 0;
                XIIXI oXII = new XIIXI();
                XIIBO oBOI = new XIIBO();
                XIInfraSendGridComponent oSendGrid = new XIInfraSendGridComponent();
                List<CNV> Params = new List<CNV>();
                Params.Add(new CNV { sName = "ID", sValue = "1" });
                string sBOName = "CampLead";
                foreach (var item in Params)
                {
                    int.TryParse(item.sValue, out ID);
                    if (ID > 0)
                    {
                        List<CNV> oWhrParams = new List<CNV>();
                        oWhrParams.Add(new CNV { sName = "ID", sValue = ID.ToString() });
                        oBOI = oXII.BOI(sBOName, ID.ToString());
                        if (oBOI.Attributes?.Count() > 0)
                        {
                            oSendGrid.sTo = oBOI.Attributes["sEmail"].sValue;
                            oSendGrid.sName = oBOI.Attributes["sName"].sValue;
                            oSendGrid.oDynamicData = new
                            {
                                subject = "Send Grid Using Dynamic Template",
                                header = "dynamic header",
                                name = oSendGrid.sName,
                                address = "west godavari",
                                emailID = oSendGrid.sTo,
                                url = "www.google.com",
                                buttonName = "Go to Google"
                            };

                            var listParams = new List<CNV>();
                            listParams.Add(new CNV { sName = "SendGridAccountID", sValue = "1" });
                            listParams.Add(new CNV { sName = "SendGridTemplateName", sValue = "test" });
                            var result = oSendGrid.Load(listParams);
                            if (result.bOK)
                            {
                                oCResult = result;
                            }
                        }
                    }
                }

                //these code testing purpose

                //oSendGrid.sTo = "raviteja.m@inativetech.com";
                //oSendGrid.sName = "sarvesh";
                //oSendGrid.sCC = "sarveswararao.s@inativetech.com";
                //oSendGrid.sCCName = "sarveswararao";

                //oSendGrid.sTemplateID = "d-467b33cf4fbc43238f748f90b068b308";
                //oSendGrid.iSGADID = 1;

            }
            catch (Exception ex)
            {
                XIInstanceBase oInstanceBase = new XIInstanceBase();
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oInstanceBase.SaveErrortoDB(oCResult);

            }

            return Json(oCResult.oResult, JsonRequestBehavior.AllowGet);
        }

    }
}