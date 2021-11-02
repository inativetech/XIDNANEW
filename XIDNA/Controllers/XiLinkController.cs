﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XIDNA.Models;
using XIDNA.Repository;
using XIDNA.ViewModels;
using Microsoft.AspNet.Identity;
using XIDNA.Common;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Xml;
using System.Text;
using System.Xml.Linq;
using XIDNA.Mailer;
using XICore;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using XISystem;
using System.IO;
//using NPOI.XWPF.UserModel;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using ZeeInsurance;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Web.UI;
using ZeeBNPPFServices;
//using ZeePremiumFinance;
//using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Web.UI.WebControls;
//using System.Web.UI;
using ZeePremiumFinance;
//using XIDatabase;
//using ZeeBNPPFServices;
using XIDatabase;
using System.Web.Security;
using System.Xml.Schema;
using System.Net;
using static ZeeBNPPFServices.XIPFCommon;
using System.Web.Hosting;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
//using itext.Layout;
//using iText.Kernel.Pdf;
//using iText.Html2pdf;
//using iTextSharp.text.pdf.parser;
//using iText.Kernel.Geom;
using System.Globalization;
using System.Diagnostics;
using XIDNA.Hubs;
//using Microsoft.SqlServer.Management.Common;
//using Microsoft.SqlServer.Management.Smo;
//using Microsoft.SqlServer.Management.Sdk.Sfc;
//using System.Reflection;
//using System.Collections.Specialized;

namespace XIDNA.Controllers
{
    [Authorize]
    [SessionTimeout]
    public class XiLinkController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IXiLinkRepository XiLinkRepository;
        private readonly IContentRepository ContentRepository;
        readonly string _connString = ServiceUtil.GetClientConnectionString();

        public XiLinkController() : this(new XiLinkRepository(), new ContentRepository()) { }

        public XiLinkController(IXiLinkRepository XiLinkRepository, IContentRepository ContentRepository)
        {
            this.XiLinkRepository = XiLinkRepository;
            this.ContentRepository = ContentRepository;
        }
        XIInfraUsers oUser = new XIInfraUsers();
        XIDropDown XIDrop = new XIDropDown();
        CommonRepository Common = new CommonRepository();
        BusinessObjectsRepository BusinessObjectsRepository = new BusinessObjectsRepository();
        XIComponentsRepository XIComponentsRepository = new XIComponentsRepository();
        XIInfraCache oCache = new XIInfraCache();
        XIInfraEmail oEmail = new XIInfraEmail();
        XIConfigs oConfig = new XIConfigs();
        CXiAPI oXiAPI = new CXiAPI();
        ContentController ContentController = new ContentController();
        AccountController Accountcontroller = new AccountController();
        XIIXI oIXI = new XIIXI();
        XID1Click oD1Click = new XID1Click();
        ModelDbContext dbContext = new ModelDbContext();
        Dictionary<string, object> Params = new Dictionary<string, object>();
        private int OCID { get; set; }
        private string sSearchType { get; set; }
        private string sSearchText { get; set; }
        private int LoadingPattern = -1;
        private int PopupID { get; set; }
        public string BO { get; set; }
        public string Group { get; set; }
        public string StartAction { get; set; }
        public string ContentType { get; set; }
        public string BespokeURL { get; set; }
        public List<string> MenuIDs { get; set; }
        public string MenuName { get; set; }
        public int DialogID { get; set; }
        public int iInstanceID { get; set; }
        public string sVisualisation { get; set; }
        public int iQSDID { get; set; }
        public string sQSType { get; set; }
        public string sStepName { get; set; }
        public string FormName { get; set; }
        public string ActiveForeginKey { get; set; }
        public int iBODID { get; set; }
        public string sMode { get; set; }
        public int iCustomerID { get; set; }
        public int iLayoutID { get; set; }
        int iStructureLoopCount = 0;
        int iLoopCount = 0;
        public object oDynamicObject { get; set; }
        public string sHtmlPage { get; set; }
        private int ReportID { get; set; }
        //
        // GET: /XiLink/
        public ActionResult Index(string sType)
        {
            ViewBag.sType = sType;
            return View();
        }

        #region AddEditXiLink
        public ActionResult XiLinksList(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XiLinkRepository.XiLinksList(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult ViewXiLink(int XiLinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiLinks model = new VMXiLinks();
                VMXiLinks Result = XiLinkRepository.GetXiLinkByID(XiLinkID, sDatabase);
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                if (fkiApplicationID == 0)
                {
                    Result.ddlApplications = Common.GetApplicationsDDL();
                }
                Result.FKiApplicationID = fkiApplicationID;
                return PartialView("_ViewXiLink", Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult AddEditXiLink(int XiLinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                VMXiLinks model = new VMXiLinks();
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                model.ddlXIComponents = Common.GetXIComponentsDDL(iUserID, sOrgName, sDatabase);
                ModelDbContext code = new ModelDbContext();
                var res = code.XIComponents.ToList();
                if (fkiApplicationID == 0)
                {
                    model.ddlApplications = Common.GetApplicationsDDL();
                }
                if (XiLinkID == 0)
                {
                    model.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiLink", model);
                }
                else
                {
                    VMXiLinks Result = XiLinkRepository.GetXiLinkByID(XiLinkID, sDatabase);
                    Result.ddlXIComponents = Common.GetXIComponentsDDL(iUserID, sOrgName, sDatabase);
                    if (fkiApplicationID == 0)
                    {
                        Result.ddlApplications = Common.GetApplicationsDDL();
                    }
                    Result.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiLink", Result);
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
        public ActionResult SaveEditXiLink(int XiLinkID, string Name, string URL, int OneClickID, int FKiApplicationID, string[] NVPairs, string[] LNVPairs, int FKiComponentID, int Status, string sActive, string sType)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiLinks Link = new VMXiLinks();
                Link.XiLinkID = XiLinkID;
                Link.Name = Name;
                Link.URL = URL;
                Link.OneClickID = OneClickID;
                Link.FKiComponentID = FKiComponentID;
                Link.FKiApplicationID = FKiApplicationID;
                Link.NVPairs = NVPairs;
                Link.LNVPairs = LNVPairs;
                Link.sActive = sActive;
                Link.sType = sType;
                Link.StatusTypeID = Status;
                Link.CreatedBy = iUserID;
                Link.UpdatedBy = iUserID;
                Link.CreatedBySYSID = Request.UserHostAddress;
                var Result = XiLinkRepository.SaveXiLink(Link, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Save_Xilink(int XiLinkID, string Name, string URL, int OneClickID, int FKiApplicationID, string[] NVPairs, string[] LNVPairs, int FKiComponentID, int Status, string sActive, string sType)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var iOrgID = oUser.FKiOrganisationID;
                SignalR oSignalR = new SignalR();
                XIConfigs oConfig = new XIConfigs(oSignalR);
                oConfig.iUserID = iUserID;
                oConfig.sSessionID = HttpContext.Session.SessionID;
                XILink oXL = new XILink();
                oXL.XiLinkID = XiLinkID;
                oXL.Name = Name;
                oXL.URL = URL;
                oXL.OneClickID = OneClickID;
                oXL.FKiComponentID = FKiComponentID;
                oXL.FKiApplicationID = FKiApplicationID;
                oXL.OrganisationID = iOrgID;
                oXL.sActive = sActive;
                oXL.sType = sType;
                oXL.StatusTypeID = Status;
                var oXLDef = oConfig.Save_XILink(oXL);
                var iXiLinkID = 0;
                if (oXLDef.bOK && oXLDef.oResult != null)
                {
                    var oXLD = (XIIBO)oXLDef.oResult;
                    var oXiLinkID = oXLD.Attributes.Values.Where(m => m.sName.ToLower() == "xilinkid").Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(oXiLinkID, out iXiLinkID);
                    var RemoveXiLink = XiLinkRepository.RemoveXilinkID(iXiLinkID);
                }
                if (NVPairs != null && NVPairs.Count() > 0)
                {
                    for (int i = 0; i < NVPairs.Count(); i++)
                    {
                        XIIBO oBOINV = new XIIBO();
                        XIDBO oBODNV = new XIDBO();
                        oBODNV = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XILinkNVs");
                        oBOINV.BOD = oBODNV;
                        var AttrValues = NVPairs[i].ToString().Split('^').ToList();
                        XiLinkNV oXLNV = new XiLinkNV();
                        oXLNV.Name = AttrValues[0];
                        oXLNV.Value = AttrValues[1];
                        oXLNV.XiLinkID = iXiLinkID;
                        oXLNV.XiLinkListID = 0;
                        var oXLNVDef = oConfig.Save_XILinkNV(oXLNV);
                        if (oXLNVDef.bOK && oXLNVDef.oResult != null)
                        {
                        }
                    }
                }
                //if (LNVPairs != null && LNVPairs.Count() > 0)
                //{
                //    XIIBO oBOIList = new XIIBO();
                //    XIDBO oBODList = new XIDBO();
                //    oBODList = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XILinkList");
                //    oBOIList.BOD = oBODList;
                //    for (int i = 0; i < LNVPairs.Count(); i++)
                //    {
                //        var AttrValues = LNVPairs[i].ToString().Split('^').ToList();
                //        oBOIList.Attributes["Name".ToLower()] = new XIIAttribute { sName = "Name", sValue = AttrValues[0].ToString() };
                //        oBOIList.Attributes["Name".ToLower()].bDirty = true;
                //        oBOIList.Attributes["Value".ToLower()] = new XIIAttribute { sName = "Value", sValue = AttrValues[1].ToString() };
                //        oBOIList.Attributes["Value".ToLower()].bDirty = true;
                //        oBOIList.Attributes["XiLinkID".ToLower()] = new XIIAttribute { sName = "XiLinkID", sValue = oXiLinkID.ToString() };
                //        oBOIList.Attributes["XiLinkID".ToLower()].bDirty = true;
                //        var XiLinkList = oBOIList.Save(oBOIList);
                //    }
                //}
                //return null;
                //var Result = XiLinkRepository.SaveXiLink(Link, iUserID, sOrgName, sDatabase);
                var Result = Common.ResponseMessage();
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult IsExistsXiLinkName(string Name, int XiLinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                return XiLinkRepository.IsExistsXiLinkName(Name, XiLinkID, iUserID, sOrgName, sDatabase) ? Json(true, JsonRequestBehavior.AllowGet)
                     : Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //CopyXiLinkByXiLinkID
        public ActionResult CopyXiLink(int XiLinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID;
                var CopyXiLink = XiLinkRepository.CopyXiLinkByID(XiLinkID, OrgID, iUserID, sDatabase);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        #endregion AddEditXiLink

        #region AddEditXiParameters

        public ActionResult XiParameters()
        {
            return View();
        }

        public ActionResult XiParametersList(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XiLinkRepository.XiParametersList(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult ViewXiParameter(int XiParameterID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiParameters model = new VMXiParameters();
                VMXiParameters Result = XiLinkRepository.GetXiParameterByID(XiParameterID, sDatabase);
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                if (fkiApplicationID == 0)
                {
                    Result.ddlApplications = Common.GetApplicationsDDL();
                }
                Result.FKiApplicationID = fkiApplicationID;
                return PartialView("_ViewXiParameter", Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult AddEditXiParameter(int XiParameterID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var oParam = XiLinkRepository.GetXIParameterDetails(XiParameterID, sDatabase);
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiParameters model = new VMXiParameters();
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                if (fkiApplicationID == 0)
                {
                    model.ddlApplications = Common.GetApplicationsDDL();
                }
                if (XiParameterID == 0)
                {
                    model.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiParameter", model);
                }
                else
                {
                    VMXiParameters Result = XiLinkRepository.GetXiParameterByID(XiParameterID, sDatabase);
                    if (fkiApplicationID == 0)
                    {
                        Result.ddlApplications = Common.GetApplicationsDDL();
                    }
                    Result.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiParameter", Result);
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
        public ActionResult SaveEditXiParameter(int XiParameterID, string Name, int FKiApplicationID, string[] NVPairs, string[] LNVPairs, int Status)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiParameters Parameter = new VMXiParameters();
                Parameter.XiParameterID = XiParameterID;
                Parameter.FKiApplicationID = FKiApplicationID;
                Parameter.Name = Name;
                //Parameter.URL = URL;
                //Parameter.OneClickID = OneClickID;
                Parameter.NVPairs = NVPairs;
                Parameter.LNVPairs = LNVPairs;
                Parameter.StatusTypeID = Status;
                Parameter.CreatedBy = iUserID;
                Parameter.UpdatedBy = iUserID;
                var Result = XiLinkRepository.SaveXiParameter(Parameter, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        //Save_XiParameter
        [HttpPost]
        public ActionResult Save_XiParameter(int XiParameterID, string Name, int FKiApplicationID, string[] NVPairs, string[] LNVPairs, int Status)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var iOrgID = oUser.FKiOrganisationID;
                var FKiAppID = oUser.FKiApplicationID;
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Parameters");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("XiParameterID", XiParameterID.ToString());
                oBOI.SetAttribute("Name", Name);
                oBOI.SetAttribute("StatusTypeID", Status.ToString());
                oBOI.SetAttribute("FKiApplicationID", FKiApplicationID.ToString());
                oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                if (XiParameterID == 0)
                {
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                }
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                if (XiParameterID > 0)
                {
                    dbContext.XiParameterLists.RemoveRange(dbContext.XiParameterLists.Where(m => m.XiParameterID == XiParameterID));
                    dbContext.XiParameterNVs.RemoveRange(dbContext.XiParameterNVs.Where(m => m.XiParameterID == XiParameterID));
                    dbContext.SaveChanges();
                }
                var XiParameters = oBOI.Save(oBOI);
                var XiParametersID = string.Empty;
                if (XiParameters.bOK && XiParameters.oResult != null)
                {
                    var oXL = (XIIBO)XiParameters.oResult;
                    XiParametersID = oXL.Attributes.Values.Where(m => m.sName.ToLower() == "xiparameterid").Select(m => m.sValue).FirstOrDefault();
                    //var esfd = XiLinkRepository.RemoveXilinkID(ExistXilinkID);
                }
                if (NVPairs != null && NVPairs.Count() > 0)
                {
                    for (int i = 0; i < NVPairs.Count(); i++)
                    {
                        XIIBO oBOINV = new XIIBO();
                        XIDBO oBODNV = new XIDBO();
                        oBODNV = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XiParameterNVs");
                        oBOINV.BOD = oBODNV;
                        var AttrValues = NVPairs[i].ToString().Split('^').ToList();
                        oBOINV.SetAttribute("Name", AttrValues[0].ToString());
                        oBOINV.SetAttribute("Value", AttrValues[1].ToString());
                        if (!string.IsNullOrEmpty(AttrValues[2]) && AttrValues[2] != "")
                        {
                            oBOINV.SetAttribute("Type", AttrValues[2].ToString());
                        }
                        oBOINV.SetAttribute("XiParameterID", XiParametersID.ToString());
                        oBOINV.SetAttribute("XiParameterListID", "0"); oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                        oBOINV.SetAttribute("CreatedTime", DateTime.Now.ToString());
                        oBOINV.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        oBOINV.SetAttribute("UpdatedBy", iUserID.ToString());
                        oBOINV.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                        oBOINV.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        var XiParameterNVs = oBOINV.Save(oBOINV);
                    }
                }

                //if (LNVPairs != null && LNVPairs.Count() > 0)
                //{
                //    XIIBO oBOIList = new XIIBO();
                //    XIDBO oBODList = new XIDBO();
                //    oBODList = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XiParameterLists");
                //    oBOIList.BOD = oBODList;

                //    var AllLists =LNVPairs.ToList();
                //    var ListPairs = LNVPairs.Where(m => !m.Contains("^")).Select(m => m).ToList();
                //    for (int k = 0; k < ListPairs.Count(); k++)
                //    {
                //        oBOIList.Attributes["XiParameterID".ToLower()] = new XIIAttribute { sName = "XiParameterID", sValue = XiParametersID.ToString(), bDirty = true };
                //        oBOIList.Attributes["ListName".ToLower()] = new XIIAttribute { sName = "ListName", sValue = ListPairs[k].ToString(), bDirty = true };
                //        oBOIList.Attributes["StatusTypeID".ToLower()] = new XIIAttribute { sName = "StatusTypeID", sValue = Status.ToString(), bDirty = true };
                //        oBOI.Attributes["CreatedBy".ToLower()] = new XIIAttribute { sName = "CreatedBy", sValue = iUserID.ToString(), bDirty = true };
                //        oBOI.Attributes["CreatedTime".ToLower()] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                //        oBOI.Attributes["CreatedBySYSID".ToLower()] = new XIIAttribute { sName = "CreatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                //        oBOI.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = iUserID.ToString(), bDirty = true };
                //        oBOI.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                //        oBOI.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                //        var XiParameterList = oBOIList.Save(oBOIList);
                //        var XiParameterListID = string.Empty;
                //        if (XiParameterList.bOK && XiParameterList.oResult != null)
                //        {
                //            var oXL = (XIIBO)XiParameterList.oResult;
                //            XiParameterListID = oXL.Attributes.Values.Where(m => m.sName.ToLower() == "xiparameterlistid").Select(m => m.sValue).FirstOrDefault();
                //            //var esfd = XiLinkRepository.RemoveXilinkID(ExistXilinkID);
                //        }
                //        var LNVpairs = AllLists.Where(m => m.StartsWith(ListPairs[k] + "-")).Select(m => m).ToList();
                //        for (int i = 0; i < LNVpairs.Count(); i++)
                //        {
                //            XIIBO oBOINV = new XIIBO();
                //            XIDBO oBODNV = new XIDBO();
                //            oBODNV = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XiParameterNVs");
                //            oBOINV.BOD = oBODNV;
                //            var Pair = LNVpairs[i].ToString().Replace(ListPairs[k] + "-", "").Split('^').ToList();
                //            oBOINV.Attributes["Name".ToLower()] = new XIIAttribute { sName = "Name", sValue = Pair[0].ToString(), bDirty = true };
                //            oBOINV.Attributes["Value".ToLower()] = new XIIAttribute { sName = "Value", sValue = Pair[1].ToString(), bDirty = true };
                //            oBOINV.Attributes["Type".ToLower()] = new XIIAttribute { sName = "Type", sValue = Pair[2].ToString(), bDirty = true };
                //            oBOINV.Attributes["XiParameterID".ToLower()] = new XIIAttribute { sName = "XiParameterID", sValue = XiParametersID.ToString(), bDirty = true };
                //            oBOINV.Attributes["XiParameterListID".ToLower()] = new XIIAttribute { sName = "XiParameterListID", sValue = XiParameterListID.ToString(), bDirty = true };
                //            oBOI.Attributes["CreatedBy".ToLower()] = new XIIAttribute { sName = "CreatedBy", sValue = iUserID.ToString(), bDirty = true };
                //            oBOI.Attributes["CreatedTime".ToLower()] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                //            oBOI.Attributes["CreatedBySYSID".ToLower()] = new XIIAttribute { sName = "CreatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                //            oBOI.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = iUserID.ToString(), bDirty = true };
                //            oBOI.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                //            oBOI.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                //            var XiParameterNVs = oBOINV.Save(oBOINV);
                //        }
                //    }
                //}
                // var Result = XiLinkRepository.SaveXiParameter(Parameter, iUserID, sOrgName, sDatabase);
                var Result = Common.ResponseMessage();
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion AddEditXiParameters
        #region AddEditXiVisualisations

        public ActionResult XiVisualisations()
        {
            return View();
        }

        public ActionResult XiVisualisationsList(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XiLinkRepository.XiVisualisationsList(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult ViewXiVisualisation(int XiVisualID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiVisualisations model = new VMXiVisualisations();
                VMXiVisualisations Result = XiLinkRepository.GetXiVisualisationByID(XiVisualID, iUserID, sOrgName, sDatabase);
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                if (fkiApplicationID == 0)
                {
                    Result.ddlApplications = Common.GetApplicationsDDL();
                }
                Result.FKiApplicationID = fkiApplicationID;
                return PartialView("_ViewXiVisualisation", Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult AddEditXiVisualisation(int XiVisualID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiVisualisations model = new VMXiVisualisations();
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                if (fkiApplicationID == 0)
                {
                    model.ddlApplications = Common.GetApplicationsDDL();
                }
                if (XiVisualID == 0)
                {
                    model.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiVisualisation", model);
                }
                else
                {
                    VMXiVisualisations Result = XiLinkRepository.GetXiVisualisationByID(XiVisualID, iUserID, sOrgName, sDatabase);
                    if (fkiApplicationID == 0)
                    {
                        Result.ddlApplications = Common.GetApplicationsDDL();
                    }
                    //Result.FKiApplicationID = fkiApplicationID;
                    Result.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiVisualisation", Result);
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
        public ActionResult SaveEditXiVisualisation(int XiVisualID, string Type, int FKiApplicationID, string Name, string[] NVPairs, string[] LNVPairs, int Status)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXiVisualisations Visualisation = new VMXiVisualisations();
                Visualisation.XiVisualID = XiVisualID;
                Visualisation.FKiApplicationID = FKiApplicationID;
                Visualisation.Name = Name;
                Visualisation.Type = Type;
                Visualisation.NVPairs = NVPairs;
                Visualisation.LNVPairs = LNVPairs;
                Visualisation.StatusTypeID = Status;
                Visualisation.CreatedBy = iUserID;
                Visualisation.UpdatedBy = iUserID;
                var Result = XiLinkRepository.SaveEditXiVisualisation(Visualisation, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //Save_XiVisualisation
        [HttpPost]
        public ActionResult Save_XiVisualisation(int XiVisualID, string Type, int FKiApplicationID, string Name, string[] NVPairs, string[] LNVPairs, int Status)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var iOrgID = oUser.FKiOrganisationID;
                var FKiAppID = oUser.FKiApplicationID;
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XiVisualisations");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("XiVisualID", XiVisualID.ToString());
                oBOI.SetAttribute("FKiApplicationID", FKiApplicationID.ToString());
                oBOI.SetAttribute("Name", Name);
                oBOI.SetAttribute("Type", Type);
                oBOI.SetAttribute("StatusTypeID", Status.ToString());
                oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                var oVisual = oBOI.Save(oBOI);
                XIVisualisation oVisDef = new XIVisualisation();
                var iVisualID = string.Empty;
                if (oVisual.bOK && oVisual.oResult != null)
                {
                    var oXV = (XIIBO)oVisual.oResult;
                    iVisualID = oXV.Attributes.Values.Where(m => m.sName.ToLower() == "xivisualid").Select(m => m.sValue).FirstOrDefault();
                    var ExistVisualID = Convert.ToInt32(iVisualID);
                    var RemoveXiVisual = XiLinkRepository.RemoveXiVisualID(ExistVisualID);
                }
                if (NVPairs != null && NVPairs.Count() > 0)
                {
                    for (int i = 0; i < NVPairs.Count(); i++)
                    {
                        XIIBO oBOINV = new XIIBO();
                        XIDBO oBODNV = new XIDBO();
                        oBODNV = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XiVisualisationNVs");
                        oBOINV.BOD = oBODNV;
                        var Pairs = NVPairs[i].ToString().Split(',').ToList();
                        oBOINV.SetAttribute("sName", Pairs[0].ToString());
                        oBOINV.SetAttribute("sValue", Pairs[1].ToString());
                        oBOINV.SetAttribute("XiVisualID", iVisualID.ToString());
                        oBOINV.SetAttribute("XiVisualListID", "0");
                        oBOINV.SetAttribute("StatusTypeID", Status.ToString());
                        oBOINV.SetAttribute("CreatedBy", iUserID.ToString());
                        oBOINV.SetAttribute("CreatedTime", DateTime.Now.ToString());
                        oBOINV.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        oBOINV.SetAttribute("UpdatedBy", iUserID.ToString());
                        oBOINV.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                        oBOINV.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        var XiLinkNVs = oBOINV.Save(oBOINV);
                    }
                }
                //if (LNVPairs != null && LNVPairs.Count() > 0)
                //{
                //    XIIBO oBOIList = new XIIBO();
                //    XIDBO oBODList = new XIDBO();
                //    oBODList = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XILinkList");
                //    oBOIList.BOD = oBODList;
                //    for (int i = 0; i < LNVPairs.Count(); i++)
                //    {
                //        var AttrValues = LNVPairs[i].ToString().Split('^').ToList();
                //        oBOIList.Attributes["Name".ToLower()] = new XIIAttribute { sName = "Name", sValue = AttrValues[0].ToString() };
                //        oBOIList.Attributes["Name".ToLower()].bDirty = true;
                //        oBOIList.Attributes["Value".ToLower()] = new XIIAttribute { sName = "Value", sValue = AttrValues[1].ToString() };
                //        oBOIList.Attributes["Value".ToLower()].bDirty = true;
                //        oBOIList.Attributes["XiLinkID".ToLower()] = new XIIAttribute { sName = "XiLinkID", sValue = oXiLinkID.ToString() };
                //        oBOIList.Attributes["XiLinkID".ToLower()].bDirty = true;
                //        var XiLinkList = oBOIList.Save(oBOIList);
                //    }
                //}
                //return null;
                //var Result = XiLinkRepository.SaveXiLink(Link, iUserID, sOrgName, sDatabase);
                var Result = Common.ResponseMessage();
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult IsExistsXiVisualisationsName(string Name, int XiVisualID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                return XiLinkRepository.IsExistsXiVisualisationsName(Name, XiVisualID, iUserID, sOrgName, sDatabase) ? Json(true, JsonRequestBehavior.AllowGet)
                     : Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }


        #endregion AddEditXiVisualisations

        #region XiLinkData

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetXiLinkContent(int XiLinkID, string sGUID, int BODID = 0, int ID = 0, List<CNV> oNVParams = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            //if (string.IsNullOrEmpty(sGUID))
            //{
            //    if (SessionManager.sGUID != null) 
            //    {
            //        sGUID = SessionManager.sGUID;
            //    }
            //}
            try
            {
                string sOrgName = SessionManager.OrganisationName;
                var sSessionID = HttpContext.Session.SessionID;
                if (oNVParams != null)
                {
                    var values = oNVParams.Where(x => x.sName.Contains("FKiACTransactionID")).Select(t => t.sValue).ToList();
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "Actions", string.Join(",", values), null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "Action1ClickBO", BODID.ToString(), null, null);
                }

                if (sGUID != null)
                {
                    if (sGUID != null && oNVParams != null && oNVParams.Count() > 0)
                    {
                        if (oNVParams.Any(x => x.sName.ToLower() == "{XIP|refAccountCategory}".ToLower() && x.sValue == "1"))
                        {
                            foreach (var item in oNVParams)
                            {
                                if (item.sName.ToLower() == "{XIP|refAccountCategory}".ToLower() && item.sValue == "1")
                                {
                                    QueryEngine oQE = new QueryEngine();
                                    var oQResult = oQE.Execute_QueryEngine("refAccountCategory_T", "id", "");
                                    if (oQResult.bOK && oQResult.oResult != null)
                                    {
                                        item.sValue = string.Join(",", ((Dictionary<string, XIIBO>)oQResult.oResult).Values.ToList().Skip(1).Select(x => x.AttributeI("id").sValue));
                                    }
                                }
                            }
                        }
                        oCache.SetXIParams(oNVParams, sGUID, sSessionID);
                    }
                    //var sParentGUID = oCache.Get_ParamVal(sSessionID, sGUID, null, "|XIParent");
                    //if (!string.IsNullOrEmpty(sParentGUID))
                    //{
                    //    sGUID = sParentGUID;
                    //}
                    if (BODID != 0 && ID != 0)
                    {
                        XIDBO oBOD = new XIDBO();
                        oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BODID.ToString());
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|" + oBOD.Name + ".id}", ID.ToString(), null, null);
                    }
                    BO = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}");
                    ActiveForeginKey = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveFK}");
                    iInstanceID = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + BO + ".id}"));
                    iCustomerID = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iCustomerID}"));
                    //iUserid = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iUserID}"));
                }
                else
                {
                    iInstanceID = 0;
                }
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                if (oUser == null)
                {
                    oUser = new XIInfraUsers();
                }
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = 0;
                string sUserRoleName = string.Empty;
                if (oUser != null)
                {
                    OrgID = oUser.FKiOrganisationID;
                    if (oUser.Role != null)
                    {
                        sUserRoleName = oUser.Role.sRoleName;
                    }
                }
                var XiLink = GetXiLinkDetails(XiLinkID, sGUID);
                CResult oResult = oCache.MergeXILinkParameters(XiLink, sGUID, null, sSessionID);
                var ReportID = OCID;
                string ResultIn = "";
                string Query = "";
                int ClassValue = 0;
                int DateValue = 0;
                XIRun(XiLink, sGUID, oNVParams);
                if (XiLink.FKiComponentID > 0)
                {
                    ViewBag.sGUID = sGUID;
                    ViewBag.XiLinkID = XiLinkID;
                    cXIComponents oXIComponent = XIComponentsRepository.XIInitialise(XiLink.FKiComponentID, iUserID, sOrgName, sDatabase);
                    var Params = oXIComponent.XIComponentParams.Where(m => m.iXiLinkID == XiLink.XiLinkID).ToList();
                    oXIComponent.XIComponentParams = Params;
                    oXIComponent.XIComponentParams.Where(m => m.sValue == "{XIP|BODID}").ToList().ForEach(m => m.sValue = BODID.ToString());
                    return PartialView("_LoadComponent", oXIComponent); // RedirectToAction("LoadComponentByID", "XIComponents", new { iXIComponentID = XiLink.FKiComponentID, sGUID = "" });
                }
                else if (StartAction == null)
                {
                    if (!string.IsNullOrEmpty(XiLink.URL))
                    {
                        var URL = XiLink.URL;
                        if (URL.IndexOf("|") > 0)
                        {
                            var URLParams = URL.Split('|').ToList();
                            if (URLParams[0] == "XIN")
                            {
                                XIIXI oXII = new XIIXI();
                                sStepName = URLParams[1];
                                int iCount = 0;
                                var sStatus = "";
                                //CResult oCResult = oCache.MergeXILinkParameters(XiLink, sGUID, null, sSessionID);
                                var oXILinkC = XiLink.GetCopy();
                                foreach (var items in oXILinkC.XiLinkNVs)
                                {
                                    if (items.Value != null && items.Value.IndexOf("{XIP") >= 0)
                                    {
                                        items.Value = oCache.Get_ParamVal(sSessionID, sGUID, null, items.Value);
                                    }
                                }
                                var sIsEdit = oXILinkC.XiLinkNVs.Where(x => x.Name.ToLower() == "IsEdit".ToLower()).Select(x => x.Value).FirstOrDefault();
                                var sIsDelete = oXILinkC.XiLinkNVs.Where(x => x.Name.ToLower() == "IsDelete".ToLower()).Select(x => x.Value).FirstOrDefault();
                                if (!string.IsNullOrEmpty(sIsEdit) && sIsEdit.ToLower() == "False".ToLower() && !string.IsNullOrEmpty(sIsDelete) && sIsDelete.ToLower() == "True".ToLower())
                                {
                                    XIIBO oDeleteBOI = new XIIBO();
                                    if (BO.ToLower() == "Driver_T".ToLower())
                                    {
                                        oDeleteBOI = oXII.BOI(BO, iInstanceID.ToString());
                                        if (oDeleteBOI != null)
                                        {
                                            oDeleteBOI.Delete(oDeleteBOI);
                                        }
                                    }
                                }
                                var QSDID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}");
                                XIIQS oQSI = oXII.GetQSInstanceByID(Convert.ToInt32(QSDID));
                                var oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSI.FKiQSDefinitionID.ToString());
                                XIDQS oQSDC = (XIDQS)oQSD.Clone(oQSD); //.GetCopy();
                                if (sStepName == "Additional Driver")
                                {
                                    var Count = "";
                                    if (ID > 0)
                                    {
                                        Count = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|sDriverSNo_" + ID + "}");
                                        if (int.TryParse(Count, out iCount))
                                        { }
                                        if (iCount == 0)
                                        {
                                            Count = "";
                                        }
                                    }
                                    else
                                    {
                                        Count = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iCount}");
                                        if (int.TryParse(Count, out iCount))
                                        { }
                                        var oCacheI = oCache.Get_XICache();
                                        var DeletedList = oCacheI.NInstance("XISession").NInstance("SS_" + sSessionID).NInstance("UID_" + sGUID).NMyInstance.Where(m => m.Key.Contains("{XIP|sDeletedDriverSNo_")).Select(m => m.Value.sValue).ToList();
                                        var ExistingList = oCacheI.NInstance("XISession").NInstance("SS_" + sSessionID).NInstance("UID_" + sGUID).NMyInstance.Where(m => m.Key.Contains("{XIP|sDriverSNo_")).Select(m => m.Value.sValue).ToList();
                                        if (iCount > 0)
                                        {
                                            if (DeletedList.Count > 0)
                                            {
                                                iCount = iCount + DeletedList.Count;
                                                Count = iCount.ToString();
                                            }
                                        }
                                        if (iCount > 3)
                                        {

                                            foreach (var item in DeletedList)
                                            {
                                                if (!ExistingList.Contains(item))
                                                {
                                                    Count = item;
                                                    sStatus = "New";
                                                }
                                            }
                                        }
                                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|sDriverSno}", Count, null, null);
                                    }
                                    sStepName = sStepName + "_" + Count;
                                    if (!oQSDC.Steps.ContainsKey(sStepName))
                                    {
                                        sStepName = "Additional Driver";
                                    }
                                }
                                var StepDef = oQSDC.Steps[sStepName];
                                var oParams = new List<CNV>();
                                oParams.Add(new CNV { sName = "{XIP|iStepDID}", sValue = StepDef.ID.ToString() });
                                var SessionItems = SessionManager.SessionItems();
                                oParams.AddRange(SessionItems);
                                XIDComponent oComponentD = new XIDComponent();
                                oComponentD.sName = "Step Component";
                                //oComponentD.iQSIID = QSDID;
                                oComponentD.sGUID = sGUID;
                                oComponentD.nParams = oParams;
                                var sQSIID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}");
                                int iQSIID = 0;
                                XIIQS oQSInstance = new XIIQS();
                                //CResult oCResult = oCache.MergeXILinkParameters(XiLink, sGUID, null, sSessionID);
                                if (int.TryParse(sQSIID, out iQSIID))
                                {
                                    //oQSInstance = oXII.GetQuestionSetInstanceByID(0, Convert.ToInt32(iQSIID), null, 0, 0, null);
                                    oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
                                    oQSInstance.QSDefinition = oQSDC;
                                    oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, StepDef.ID, sGUID);
                                    var oCurrentStep = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == StepDef.ID).FirstOrDefault().Value;
                                    oQSInstance.sCurrentStepName = oCurrentStep.sName;
                                    if (oNVParams != null && oNVParams.Count() > 0)
                                    {
                                        var sShowSections = oNVParams.Where(x => x.sName == "ShowSections").Select(x => x.sValue).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(sShowSections))
                                        {
                                            var oShowSections = sShowSections.Split('_');
                                            foreach (var sSection in oShowSections)
                                            {
                                                foreach (var section in oQSInstance.QSDefinition.Steps[oQSInstance.sCurrentStepName].Sections)
                                                {
                                                    if (sSection == oQSInstance.QSDefinition.Steps[oQSInstance.sCurrentStepName].Sections[section.Key].sCode)
                                                    {
                                                        oQSInstance.QSDefinition.Steps[oQSInstance.sCurrentStepName].Sections[section.Key].sIsHidden = "off";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (oCurrentStep.QSLinks != null && oCurrentStep.QSLinks.Count() > 0)
                                    {
                                        var iActiveStepID = oQSInstance.GetActiveStepID(oCurrentStep.ID, sGUID);
                                        var oNextStep = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).Select(m => m.Value).FirstOrDefault();
                                        oQSInstance.iCurrentStepID = iActiveStepID;
                                        var oStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                                        if (oQSI.iStage < oStep.iStage)
                                        {
                                            oQSInstance.iStage = oStep.iStage;
                                        }
                                        oQSInstance.sCurrentStepName = oStep.sName;
                                        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                                        oQSInstance = Connection.Update<XIIQS>(oQSInstance, "XIQSInstance_T", "ID");
                                        foreach (var oQSLink in oCurrentStep.QSLinks.Values)
                                        {
                                            foreach (var oLink in oQSLink.XiLink.OrderBy(m => m.Value.rOrder))
                                            {
                                                if (oLink.Value.sType == "Pre")
                                                {
                                                    GetXiLinkContent(oLink.Value.FKiXILInkID, sGUID);
                                                }
                                            }
                                        }
                                    }
                                    if (sStatus == "New")
                                    {
                                        var CurrentStep = oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == StepDef.ID).FirstOrDefault();
                                        foreach (var sec in CurrentStep.Sections.Values)
                                        {
                                            sec.XIValues.Values.ToList().ForEach(m => m.sValue = "");
                                        }
                                    }

                                    oQSInstance.Steps.Values.ToList().ForEach(m => m.bIsCurrentStep = false);
                                    oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == StepDef.ID).FirstOrDefault().bIsCurrentStep = true;
                                    oQSInstance.iCurrentStepID = StepDef.ID;
                                    oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == StepDef.ID).Select(m => m.sName).FirstOrDefault();
                                    bool IsHistory = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == StepDef.ID).Select(m => m.bIsHistory).FirstOrDefault();
                                    if (oQSInstance.History == null)
                                    {
                                        oQSInstance.History = new List<int>();
                                    }
                                    if (oQSInstance.History.IndexOf(oQSInstance.iCurrentStepID) == -1 && IsHistory)
                                    {
                                        oQSInstance.History.Add(oQSInstance.iCurrentStepID);
                                    }
                                    var oStepMessage = oCache.Get_ObjectSetCache("StepMessage", sGUID, sSessionID);//(sSessionID, sGUID, null, "StepMessage");
                                    oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == StepDef.ID).FirstOrDefault().XiMessages = new Dictionary<string, string>();
                                    if (oStepMessage != null)
                                    {
                                        Dictionary<string, string> dictmsgs = new Dictionary<string, string>();
                                        dictmsgs = (Dictionary<string, string>)oStepMessage;
                                        oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == StepDef.ID).FirstOrDefault().XiMessages = dictmsgs;
                                        HttpRuntime.Cache.Remove("StepMessage" + "_" + sGUID + "_" + sSessionID);
                                    }
                                    oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
                                }
                                var iCurrentStepID = oQSInstance.iCurrentStepID;
                                var oCurrentStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iCurrentStepID).FirstOrDefault();
                                if (oCurrentStepD != null)
                                {
                                    Dictionary<string, XIDQSStep> Steps = new Dictionary<string, XIDQSStep>();
                                    foreach (var Step in oQSDC.Steps.Values.ToList())
                                    {
                                        Steps[Step.sName] = new XIDQSStep() { ID = Step.ID, sName = Step.sName, iOrder = Step.iOrder, sDisplayName = Step.sDisplayName, sIsHidden = Step.sIsHidden, iStage = Step.iStage, iLockStage = Step.iLockStage, iCutStage = Step.iLockStage };
                                    }
                                    oQSInstance.QSDefinition.Steps = Steps;
                                    oQSInstance.QSDefinition.Steps[oCurrentStepD.sName] = oCurrentStepD;
                                }
                                //var Response = (XIDQSStep)oComponentD.LoadComponent("QSStep", StepDef.ID).oResult;
                                //var oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, "Step Component", "");
                                //ViewBag.sGUID = sGUID;
                                //return PartialView(oXIComponent.sHTMLPage, Response);

                                //return RedirectToAction("LoadComponentByID", "XIComponents", new { iXIComponentID = 0, sGUID = sGUID, nParams = oParams, sName = "Step Component", sType = "QSStep", ID = StepDef.ID, iInstanceID = 0, sContext = string.Empty, iQSIID = QSDID });
                                //var oQSInstance = GetQSInstance(iQSDID, sGUID, null, 0, 0);
                                ViewBag.sGUID = sGUID;
                                return PartialView("_QuestionSet", oQSInstance);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return XIMethod(XiLink, sGUID, oNVParams);
                            //return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (StartAction.ToLower() == "Search".ToLower())
                {
                    if (OCID > 0)
                    {
                        ViewBag.XiLinkID = XiLinkID;
                        var Result = OneClickResult(sGUID);
                        Result.PreviewType = null;
                        Result.sGUID = sGUID;
                        List<VMNameValuePairs> Params = new List<VMNameValuePairs>();
                        Params.Add(new VMNameValuePairs { sName = "Context", sValue = "1Click" });
                        if (!string.IsNullOrEmpty(ActiveForeginKey))
                        {
                            Params.Add(new VMNameValuePairs { sName = ActiveForeginKey, sValue = iInstanceID.ToString(), sType = "autoset", sContext = "1Click" });
                        }
                        if (!string.IsNullOrEmpty(BO))
                        {
                            Params.Add(new VMNameValuePairs { sName = "ActiveBO", sValue = BO.ToString(), sContext = "1Click" });
                        }
                        if (!string.IsNullOrEmpty(ActiveForeginKey))
                        {
                            Params.Add(new VMNameValuePairs { sName = "ActiveFK", sValue = ActiveForeginKey.ToString(), sContext = "1Click" });
                        }
                        if (iCustomerID == 0)
                        {
                            Params.Add(new VMNameValuePairs { sName = "{XIP|iCustomerID}", sValue = 34.ToString(), sContext = null });
                        }
                        Params.Add(new VMNameValuePairs { sName = "{XIP|iUserID}", sValue = iUserID.ToString(), sContext = null });


                        Result.nParams = Params;
                        List<cNameValuePairs> oParams = new List<cNameValuePairs>();
                        oParams = Params.Select(m => new cNameValuePairs { sName = m.sName, sValue = m.sValue, sContext = m.sContext, sType = m.sType }).ToList();
                        //SetXIParams(oParams, sGUID);
                        ViewBag.RoleName = sUserRoleName;
                        return View("OneClickResults", Result);
                        //  return View("OneClickResultsList", Result);
                    }
                }
                else if (StartAction.ToLower() == "SearchList".ToLower())
                {
                    if (OCID > 0)
                    {
                        ViewBag.XiLinkID = XiLinkID;
                        var Result = OneClickResult(sGUID);
                        Result.PreviewType = "Popup";
                        Result.sGUID = sGUID;
                        return View("_SearchList", Result);
                        //  return View("OneClickResultsList", Result);
                    }
                }
                else if (StartAction.ToLower() == "PopupLeftContent".ToLower())
                {
                    if (BO != null && Group != null)
                    {
                        var oBODisplay = oXiAPI.GetFormData(BO, Group, iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                        if (!string.IsNullOrEmpty(sVisualisation))
                        {
                            var Visualisations = oXiAPI.GetVisualistions(BO, sVisualisation, iInstanceID, iUserID, sOrgName, sDatabase);
                            oBODisplay.Visualisations = Visualisations;
                        }
                        return PartialView("_LeadPopupLeftContent", oBODisplay);
                    }
                }
                else if (StartAction.ToLower() == "InlineView".ToLower())
                {
                    if (BO != null && Group != null)
                    {
                        var oBODisplay = oXiAPI.GetFormData(BO, Group, iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                        if (!string.IsNullOrEmpty(sVisualisation))
                        {
                            var Visualisations = oXiAPI.GetVisualistions(BO, sVisualisation, iInstanceID, iUserID, sOrgName, sDatabase);
                            oBODisplay.Visualisations = Visualisations;
                        }
                        return PartialView("_InlineView", oBODisplay);
                    }
                }
                else if (StartAction.ToLower() == "InlineEdit".ToLower() || StartAction.ToLower() == "CreateForm".ToLower())
                {
                    if (BO != null && Group != null)
                    {
                        var oBODisplay = oXiAPI.GetFormData(BO, Group, iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                        if (StartAction.ToLower() == "CreateForm".ToLower())
                        {
                            var IDPair = oBODisplay.BOInstance.NVPairs.Where(m => m.sName.ToLower() == "id").FirstOrDefault();
                            if (IDPair != null && IDPair.sValue == null)
                            {
                                oBODisplay.BOInstance.NVPairs.Remove(IDPair);
                            }
                        }

                        if (!string.IsNullOrEmpty(sVisualisation))
                        {
                            var Visualisations = oXiAPI.GetVisualistions(BO, sVisualisation, iInstanceID, iUserID, sOrgName, sDatabase);
                            oBODisplay.Visualisations = Visualisations;
                        }
                        return PartialView("_CreateForm", oBODisplay);
                    }
                }
                else if (StartAction.ToLower() == "List".ToLower())
                {
                    if (OCID > 0)
                    {
                        var Result = XiLinkRepository.GetHeadings(OCID, null, sDatabase, OrgID, Convert.ToInt32(User.Identity.GetUserId()), sOrgName);
                        Result.ReportID = OCID;
                        Result.LeadID = iInstanceID;
                        Result.sGUID = sGUID;
                        //Result.PreviewType = "Inline";
                        return PartialView("_List", Result);
                    }
                }
                else if (StartAction.ToLower() == "OneClickList".ToLower())
                {
                    if (OCID > 0)
                    {
                        var Result = XiLinkRepository.GetHeadings(OCID, null, sDatabase, OrgID, Convert.ToInt32(User.Identity.GetUserId()), sOrgName);
                        Result.ReportID = OCID;
                        Result.PreviewType = "Inline";
                        return PartialView("_List", Result);
                    }
                }
                else if (StartAction.ToLower() == "BarChart".ToLower())
                {
                    if (OCID > 0)
                    {
                        ReportID = OCID;
                        VMChart Charts = new VMChart();
                        Charts.ReportID = ReportID;
                        Charts.OrgID = OrgID;
                        Charts.UserID = iUserID;
                        Charts.Database = sDatabase;
                        Charts.DateFilter = DateValue;
                        Charts.ClassFilter = ClassValue;
                        Charts.Query = Query;
                        LineGraph graph = XiLinkRepository.GetBarChart(Charts, sDatabase, iUserID, sOrgName);
                        graph.ReportID = ReportID;
                        graph.Type = "Run";
                        graph.SectionName = "";
                        graph.ClassDDL = new List<VMDropDown>();
                        graph.DateDDL = new List<VMDropDown>();
                        return PartialView("_KPIBarChart", graph);
                    }
                }

                else if (StartAction.ToLower() == "PieChart".ToLower())
                {
                    if (OCID > 0)
                    {
                        ReportID = OCID;
                        VMChart Chart = new VMChart();
                        Chart.ReportID = ReportID;
                        Chart.Query = Query;
                        Chart.ClassFilter = ClassValue;
                        Chart.DateFilter = DateValue;
                        Chart.OrgID = OrgID;
                        Chart.UserID = iUserID;
                        Chart.Database = sDatabase;
                        GraphData Graph = new GraphData();
                        Graph = XiLinkRepository.GetPieChart(Chart, iUserID, sOrgName, sDatabase);
                        Graph.Type = "Run";
                        Graph.SectionName = "";
                        Graph.ClassDDL = new List<VMDropDown>();
                        Graph.DateDDL = new List<VMDropDown>();
                        return PartialView("_KPIPieChart", Graph);
                    }
                }

                else if (StartAction.ToLower() == "LineGraph".ToLower())
                {
                    if (OCID > 0)
                    {
                        ReportID = OCID;
                        VMChart Chart = new VMChart();
                        Chart.ReportID = ReportID;
                        Chart.OrgID = OrgID;
                        Chart.UserID = iUserID;
                        Chart.Database = sDatabase;
                        Chart.DateFilter = DateValue;
                        Chart.ClassFilter = ClassValue;
                        Chart.Query = Query;
                        LineGraph graph = XiLinkRepository.GetLineChart(Chart, iUserID, sOrgName, sDatabase);
                        graph.ReportID = ReportID;
                        graph.Type = "Run";
                        graph.SectionName = "";
                        graph.ClassDDL = new List<VMDropDown>();
                        graph.DateDDL = new List<VMDropDown>();
                        return PartialView("_LineGraph", graph);
                    }
                }

                else if (StartAction.ToLower() == "Circle".ToLower())
                {
                    ReportID = OCID;
                    VMChart Chart = new VMChart();
                    Chart.ReportID = ReportID;
                    Chart.OrgID = OrgID;
                    Chart.UserID = iUserID;
                    Chart.Database = sDatabase;
                    Chart.DateFilter = DateValue;
                    Chart.ClassFilter = ClassValue;
                    Chart.Query = Query;
                    List<VMKPIResult> KPIs = XiLinkRepository.GetKPICircleResult(Chart, iUserID, sDatabase, sOrgName);
                    KPIs.FirstOrDefault().UserID = iUserID;
                    return PartialView("_KPICircles", KPIs);
                }

                else if (StartAction.ToLower() == "Popup".ToLower() || StartAction.ToLower() == "InlinePopup".ToLower())
                {
                    var PopDetails = XiLinkRepository.GetPopupLayoutDetails(PopupID, 0, 0, sGUID, iUserID, sOrgName, sDatabase);
                    return PartialView("_PopupContent", PopDetails);
                }
                else if (StartAction.ToLower() == "Inline".ToLower())
                {
                    var PopDetails = XiLinkRepository.GetPopupLayoutDetails(PopupID, 0, 0, sGUID, iUserID, sOrgName, sDatabase);
                    return PartialView("_InlineContent", PopDetails);
                }
                else if (StartAction.ToLower() == "Dialog".ToLower())
                {
                    var DailogDetails = XiLinkRepository.GetDialogLayoutDetails(DialogID, 0, 0, sGUID, iUserID, sOrgName, sDatabase);
                    return PartialView("_DialogContent", DailogDetails);
                }
                //else if (StartAction.ToLower() == "Menu".ToLower())
                //{
                //    var url = XiLink.URL.Split('/').ToList();
                //    return RedirectToAction(url[1], url[0]);
                //}
                else if (StartAction.ToLower() == "Bespoke".ToLower())
                {
                    return PartialView("_Bespoke", BespokeURL);
                }
                else if (StartAction.ToLower() == "Menu".ToLower())
                {
                    List<RightMenuTrees> Menus = new List<RightMenuTrees>();
                    if (MenuName != null && MenuName.Length > 0)
                    {
                        int UserID = iUserID;
                        // Menus = XiLinkRepository.GetMenus(MenuIDs, OrgID);
                        Menus = XiLinkRepository.GetMenus(MenuName, UserID, OrgID, sDatabase);
                    }
                    return PartialView("_Menus", Menus);
                }
                else if (StartAction.ToLower() == "Tabs".ToLower())
                {
                    List<XiLinkNVs> Tabs = new List<XiLinkNVs>();
                    var TabsDetails = XiLinkRepository.GetTabsDetails(XiLinkID, sDatabase);
                    ViewBag.LeadID = iInstanceID;
                    return PartialView("_TabsContent", TabsDetails);
                }
                else if (StartAction.ToLower() == "DynamicForm".ToLower())
                {
                    var url = XiLink.URL.Split('/').ToList();
                    return RedirectToAction(url[1], url[0], new { XiLinkID = XiLinkID });
                }
                else if (StartAction.ToLower() == "Editable Grid".ToLower())
                {
                    if (OCID > 0)
                    {
                        ViewBag.XiLinkID = XiLinkID;
                        var Result = OneClickResult(sGUID);
                        var oBODisplay = oXiAPI.GetFormData(Result.BO, ServiceConstants.SaveGroup.ToString(), iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                        ViewBag.oBODisplay = oBODisplay;
                        Result.PreviewType = null;
                        // return View("OneClickResults", Result);
                        return View("OneClickResultsList", Result);
                    }
                }
                else if (StartAction.ToLower() == "QuestionSet".ToLower())
                {
                    var iUserOrg = SessionManager.iUserOrg;
                    var sQSIID = oCache.Get_ParamVal(sSessionID, "ClientQSParams", "", "{XIP|iQSInstanceID}");
                    int iQSIID = 0;
                    var stepName = oCache.Get_ParamVal(sSessionID, "ClientQSParams", "", "sCurrentStepName");
                    var BuyType = oCache.Get_ParamVal(sSessionID, "ClientQSParams", "", "BuyType");
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "BuyType", BuyType, null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "sCurrentStepName", stepName, null, null);
                    var sInstanceID = oCache.Get_ParamVal(sSessionID, "ClientQSParams", "", "{XIP|iInstanceID}");
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iInstanceID}", sInstanceID, null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|-iUserOrg}", iUserOrg.ToString(), null, null);
                    var sQSDID = oCache.Get_ParamVal(sSessionID, "ClientQSParams", "", "iQSDID");
                    int iCQSDID = 0;
                    if (int.TryParse(sQSDID, out iCQSDID))
                    { }
                    if (int.TryParse(sQSIID, out iQSIID))
                    {
                        if (iQSIID > 0)
                        {

                        }
                        oCache.Set_ParamVal(sSessionID, "ClientQSParams", null, "{XIP|iQSInstanceID}", "", null, null);
                        oCache.Set_ParamVal(sSessionID, "ClientQSParams", null, "sCurrentStepName", "", null, null);
                        oCache.Set_ParamVal(sSessionID, "ClientQSParams", null, "iQSDID", "", null, null);
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}", Convert.ToString(iQSIID), "autoset", null);
                        return RedirectToAction("ClientLoadQs", new { iQSIID = iQSIID, sGUID = sGUID, iQSDID = iCQSDID });
                    }
                    stepName = oCache.Get_ParamVal(sSessionID, sGUID, "", "sCurrentStepName");
                    if (!string.IsNullOrEmpty(stepName))
                    {
                        sStepName = stepName;
                    }
                    var oQSInstance = GetQSInstance(iQSDID, sGUID, sMode, iBODID, iInstanceID);
                    ViewBag.sGUID = sGUID;
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "-iQSIID", oQSInstance.ID.ToString(), null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}", Convert.ToString(oQSInstance.ID), "autoset", null);
                    if (!string.IsNullOrEmpty(sQSType))
                    {
                        oQSInstance.sQSType = sQSType;
                    }
                    oCache.Set_ParamVal(sSessionID, sGUID, "", "sQSType", sQSType, null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, "", "sCurrentStepName", "", null, null);
                    oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
                    oCache.Set_ParamVal(sSessionID, sGUID, "", "sCurrentStepName", "", null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|ClassID}", oQSInstance.QSDefinition.FKiClassID.ToString(), null, null);
                    SessionManager.QSName = oQSInstance.QSDefinition.sDescription;
                    var oQSD = oQSInstance.QSDefinition;
                    var iCurrentStepID = oQSInstance.iCurrentStepID;
                    var oCurrentStepD = oQSD.Steps.Values.Where(m => m.ID == iCurrentStepID).FirstOrDefault();
                    if (oCurrentStepD != null)
                    {
                        Dictionary<string, XIDQSStep> Steps = new Dictionary<string, XIDQSStep>();
                        foreach (var Step in oQSD.Steps.Values.ToList())
                        {
                            Steps[Step.sName] = new XIDQSStep() { ID = Step.ID, sName = Step.sName, iOrder = Step.iOrder, sDisplayName = Step.sDisplayName, sIsHidden = Step.sIsHidden, iStage = Step.iStage, iLockStage = Step.iLockStage, iCutStage = Step.iLockStage };
                        }
                        oQSInstance.QSDefinition.Steps = Steps;
                        oQSInstance.QSDefinition.Steps[oCurrentStepD.sName] = oCurrentStepD;
                    }
                    return PartialView("QuestionSet", oQSInstance);
                }
                else if (StartAction.ToLower() == "DataSource".ToLower())
                {
                    var url = XiLink.URL.Split('/').ToList();
                    return RedirectToAction(url[1], url[0]);
                }
                else if (StartAction.ToLower() == "QSStep".ToLower())
                {
                    var oQSInstance = GetQSInstance(iQSDID, sGUID, sMode, iBODID, iInstanceID);
                    ViewBag.sGUID = sGUID;
                    return PartialView("QuestionSet", oQSInstance);
                }
                else if (StartAction.ToLower() == "QuestionSetInternal".ToLower())
                {
                    var oQSInstance = GetInternalQuestionSet(0);
                    ViewBag.sGUID = sGUID;
                    //oQSInstance.iCurrentStepID = oQSInstance.nStepInstances.Where(m => m.bIsCurrentStep == true).Select(m => m.FKiQSStepDefinitionID).FirstOrDefault();
                    return PartialView("QuestionSetInternal", oQSInstance);
                }
                else if (StartAction.ToLower() == "MyPolicies".ToLower())
                {
                    var Result = OneClickResult(sGUID);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iUserID}", iUserID.ToString(), null, null);
                    ViewBag.sGUID = sGUID;
                    return View("_MyPolicies", Result);
                }
                //else if (StartAction.ToLower() == "DocumentViewer".ToLower())
                //{
                //    var Result = OneClickResult(sGUID);
                //    //oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iUserID}", iUserID.ToString(), null, null);
                //    //ViewBag.sGUID = sGUID;
                //    var str = "http://localhost:53996//";
                //    var sPath = "Attachments//33030e0b-9979-411b-9ef8-b6f48270d6c0//devorg//PDF//RunTime_Attachment.pdf";
                //    ViewBag.sFinalPath = str + sPath;
                //    ViewBag.sPath = sPath;
                //    return View("_PDFViewer");
                //}
                else if (StartAction.ToLower() == "MyQuotes".ToLower())
                {

                }
                else if (StartAction.ToLower() == "FAInbox".ToLower())
                {
                    return PartialView("_FAInbox");
                }
                else if (StartAction.ToLower() == "FAEvents".ToLower())
                {
                    return PartialView("_FAEvents");
                }
                else if (StartAction.ToLower() == "Layout".ToLower())
                {
                    //load notifications and noification count
                    XIDLayout oLayout = new XIDLayout();
                    var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, null, iLayoutID.ToString()); //oXID.Get_LayoutDefinition(null, iLayoutID.ToString());
                    //var oLayDef = oLayout.Load();
                    if (oLayDef != null)
                    {
                        oLayout = (XIDLayout)oLayDef;
                    }
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iUserID}", iUserID.ToString(), null, null);
                    ViewBag.sGUID = sGUID;
                    //sGUID = oLayout.sGUID;

                    return View("_UserLayoutContent", oLayout);
                }
                return null;
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Critical Error: while loading XILink: " + XiLinkID.ToString(), sDatabase);
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        private ActionResult XIMethod(XILink xiLink, string sGUID, List<CNV> oParams = null)
        {
            oParams = oParams == null ? new List<CNV>() : oParams;
            var oXILinkC = (XILink)(xiLink.Clone(xiLink));
            var sSessionID = HttpContext.Session.SessionID;
            foreach (var items in oXILinkC.XiLinkNVs)
            {
                if (items.Value != null && items.Value.IndexOf("{XIP") >= 0)
                {
                    items.Value = oCache.Get_ParamVal(sSessionID, sGUID, null, items.Value);
                }
            }
            var sMethodName = oXILinkC.XiLinkNVs.Where(m => m.Name.ToLower() == "sMethodName".ToLower()).Select(m => m.Value).FirstOrDefault();
            string sHTMLPage = string.Empty;
            sHTMLPage = oXILinkC.XiLinkNVs.Where(m => m.Name.ToLower() == "sHTMLPage".ToLower()).Select(m => m.Value).FirstOrDefault();
            var sURL = oXILinkC.URL.Split('/').ToList().First();
            var sClass = oXILinkC.URL.Split('/').ToList().Last();
            //Creating Instance
            Assembly exceutable;
            Type Ltype;
            object objclass;
            exceutable = Assembly.Load(sURL);
            Ltype = exceutable.GetType(sURL + "." + sClass);
            objclass = Activator.CreateInstance(Ltype);
            if (!string.IsNullOrEmpty(sMethodName))
            {
                MethodInfo method = Ltype.GetMethod(sMethodName);
                var sParentGUID = oCache.Get_ParamVal(sSessionID, sGUID, null, "|XIParent");
                if (!string.IsNullOrEmpty(sParentGUID))
                {
                    sGUID = sParentGUID;
                }
                //oParams = oCache.ResolveParameters(oParams, sSessionID, sGUID);

                
                oParams.AddRange(oXILinkC.XiLinkNVs.Select(m => new CNV { sName = m.Name, sValue = m.Value }).ToList());
                oParams.Add(new CNV { sName = "sGUID", sValue = sGUID });
                oParams.Add(new CNV { sName = "sDataBase", sValue = SessionManager.CoreDatabase });
                oParams.Add(new CNV { sName = "iUserID", sValue = SessionManager.UserID.ToString() });
                oParams.Add(new CNV { sName = "srolename", sValue = SessionManager.sRoleName });
                oParams.Add(new CNV { sName = "sOrgDatabase", sValue = SessionManager.OrgDatabase });
                oParams.Add(new CNV { sName = "sOrgName", sValue = SessionManager.OrganisationName });
                oParams.Add(new CNV { sName = "iOrganizationID", sValue = SessionManager.OrganizationID.ToString() });
                oParams.Add(new CNV { sName = "iApplicationID", sValue = SessionManager.ApplicationID.ToString() });
                oParams.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
                var sTID = HttpContext.Session["sCurrentTransactionID"];
                if (sTID != null && !string.IsNullOrEmpty(sTID.ToString()))
                {
                    oParams.Add(new CNV { sName = "sTID", sValue = sTID.ToString() });
                }
                object Response = new object();
                if (sMethodName == "ExeThread" || sMethodName == "ExeThreadForPartial" || sMethodName == "BindOverRideValues")
                {
                    SignalR oSignalR = new SignalR();
                    object[] parametersArray = new object[] { oParams, oSignalR };
                    Response = (object)method.Invoke(objclass, parametersArray);
                }
                else
                {
                    object[] parametersArray = new object[] { oParams };
                    Response = (object)method.Invoke(objclass, parametersArray);
                }

                if (((CResult)Response).bOK && ((CResult)Response).oResult != null)
                {
                    var oResult = ((CResult)Response).oResult;
                    if (!string.IsNullOrEmpty(sHTMLPage))
                    {
                        ViewBag.sGUID = sGUID;
                        //oDynamicObject = oResult;
                        //sHtmlPage = sHTMLPage;
                        return PartialView(sHTMLPage, oResult);
                    }
                    return Json(oResult, JsonRequestBehavior.AllowGet);
                }
                var UserID = oCache.Get_ParamVal(sSessionID, sGUID, null, "-iUserID");
                int iUserID = 0;
                if (int.TryParse(UserID, out iUserID))
                {
                    XIInfraUsers oUser = new XIInfraUsers();
                    oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                    var UserDetails = oUser.Get_UserDetails(SessionManager.CoreDatabase);
                    if (UserDetails.xiStatus == 0 && UserDetails.oResult != null)
                    {
                        oUser = (XIInfraUsers)UserDetails.oResult;

                        Singleton.Instance.sCoreDatabase = oUser.sCoreDatabaseName;
                        Singleton.Instance.sUserID = oUser.UserID.ToString();
                        Singleton.Instance.sOrgID = oUser.FKiOrganisationID.ToString();
                        Singleton.Instance.sAppName = oUser.sAppName;
                        SessionManager.sUserName = oUser.sUserName;
                        SessionManager.sEmail = oUser.sEmail;
                        SessionManager.sRoleName = oUser.Role.sRoleName;
                        SessionManager.OrgDatabase = oUser.sDatabaseName;
                        SessionManager.UserID = oUser.UserID;
                        SessionManager.OrganisationName = "Org";
                        SessionManager.UserUniqueID = null;
                        SessionManager.OrganizationID = oUser.FKiOrganisationID;
                        SessionManager.sName = oUser.sFirstName + " " + oUser.sLastName;
                        SessionManager.iRoleID = oUser.RoleID.RoleID;
                        SessionManager.sCustomerRefNo = oUser.sCustomerRefNo;

                        FormsAuthentication.SetAuthCookie(oUser.sUserName, false);
                        var authTicket = new FormsAuthenticationTicket(1, oUser.sUserName, DateTime.Now, DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), false, "Admin", FormsAuthentication.FormsCookiePath);
                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                        {
                            HttpOnly = true,
                            Secure = FormsAuthentication.RequireSSL,
                            Path = FormsAuthentication.FormsCookiePath,
                            Domain = FormsAuthentication.CookieDomain,
                            Expires = DateTime.Now.AddDays(1)
                        };
                        HttpContext.Response.Cookies.Add(authCookie);

                        // HERE SETTING ROLENAME AND USERNAME INTO CACHE.
                        XIInfraCache oUserCache = new XIInfraCache();
                        var oCacheduser = new VM_UserLoginCache { sUserName = oUser.sUserName, sRole = oUser.Role.sRoleName };
                        oUserCache.InsertIntoCache(oCacheduser, "SSS_" + sSessionID);
                    }
                }
            }
            return null;
        }

        public void SetXIParams(List<cNameValuePairs> oParams, string sGUID)
        {
            cXICache oCache = new cXICache();
            string sSessionID = HttpContext.Session.SessionID;
            foreach (var items in oParams)
            {
                if (!string.IsNullOrEmpty(items.sValue))
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, items.sName, items.sValue, null, items.nSubParams);
                }
            }
        }

        public XILink GetXiLinkDetails(int XiLinkID = 0, string sGUID = "")
        {

            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sOrgName = SessionManager.OrganisationName;
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var XiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, XiLinkID.ToString());
                return XiLink;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult GetXILinkDefinition(int XiLinkID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var XiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, XiLinkID.ToString());
                return Json(XiLink, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult GetPopupDefinition(int PopupID, List<CNV> oParams = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var oPopupD = (XIDPopup)oCache.GetObjectFromCache(XIConstant.CachePopup, null, PopupID.ToString());
                XIDLayout oLayout = new XIDLayout();
                oLayout.ID = oPopupD.LayoutID;
                oLayout.oLayoutParams = oParams;
                return View("~/Views/Home/InternalUsers.cshtml", oLayout);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult OpenInPopup(string sBOName, int iBOIID, int iLayoutID, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                List<CNV> oParams = new List<CNV>();
                string sSessionID = HttpContext.Session.SessionID;
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                oParams.AddRange(oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue }));
                XIDLayout oLayout = new XIDLayout();
                oLayout.ID = iLayoutID;
                oLayout.oLayoutParams = oParams;
                return View("~/Views/Home/InternalUsers.cshtml", oLayout);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [AllowAnonymous]
        public ActionResult GetXiLinkData(int XiLinkID, string sGUID, bool IsMerge)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sOrgName = SessionManager.OrganisationName;
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var XiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, XiLinkID.ToString());
                return Json(XiLink, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult XIRun(XILink oXilink, string sGUID, List<CNV> oNVParams = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                if (oXilink != null)
                {
                    OCID = oXilink.OneClickID;
                    foreach (var NVPair in oXilink.XiLinkNVs.Where(m => m.XiLinkListID == 0))
                    {
                        if (!string.IsNullOrEmpty(NVPair.Name) && NVPair.Name.ToLower() == "StartAction".ToLower() && !string.IsNullOrEmpty(NVPair.Value))
                        {
                            //get the value and assign to property of 1click instance

                            if (NVPair.Value.ToLower() == "Search".ToLower())
                            {
                                sSearchType = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "SearchType".ToLower()).Select(m => m.Value).FirstOrDefault();
                                //get the value and assign to  firststep property of 1click instance
                            }
                            else if (NVPair.Value.ToLower() == "List".ToLower())
                            {
                                //get the value and assign to  firststep property of 1click instance }
                            }
                            else if (NVPair.Value.ToLower() == "InlineView".ToLower() || NVPair.Value.ToLower() == "InlineEdit".ToLower() || NVPair.Value.ToLower() == "CreateForm".ToLower() || NVPair.Value.ToLower() == "PopupLeftContent".ToLower())
                            {
                                ContentType = NVPair.Value;
                                BO = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BO".ToLower()).Select(m => m.Value).FirstOrDefault();
                                Group = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "Group".ToLower() || m.Name.ToLower() == "Save Group".ToLower() || m.Name.ToLower() == "Show Group".ToLower()).Select(m => m.Value).FirstOrDefault();
                                sVisualisation = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "visualisation".ToLower()).Select(m => m.Value).FirstOrDefault();
                            }
                            else if (NVPair.Value.ToLower() == "Popup".ToLower() || NVPair.Value.ToLower() == "InlinePopup".ToLower() || NVPair.Value.ToLower() == "Inline".ToLower())
                            {
                                PopupID = Convert.ToInt32(oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "PopupID".ToLower()).Select(m => m.Value).FirstOrDefault());
                            }
                            else if (NVPair.Value.ToLower() == "Bespoke".ToLower())
                            {
                                BespokeURL = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "Bespoke url".ToLower()).Select(m => m.Value).FirstOrDefault();
                            }
                            else if (NVPair.Value.ToLower() == "Menu".ToLower())
                            {
                                MenuName = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "MenuName".ToLower()).Select(m => m.Value).FirstOrDefault();
                            }
                            else if (NVPair.Value.ToLower() == "Dialog".ToLower())
                            {
                                DialogID = Convert.ToInt32(oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "DialogID".ToLower()).Select(m => m.Value).FirstOrDefault());
                            }
                            else if (NVPair.Value.ToLower() == "QuestionSet".ToLower())
                            {
                                var sSessionID = HttpContext.Session.SessionID;
                                string sQSValue = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "QuestionSetID".ToLower()).Select(m => m.Value).FirstOrDefault();
                                if (sQSValue.Contains("{XIP|"))
                                {
                                    if (oNVParams != null && oNVParams.Count() > 0)
                                    {
                                        string sQSDID = oNVParams.Where(m => m.sName == sQSValue).Select(m => m.sValue).FirstOrDefault();
                                        int iValue;
                                        if (int.TryParse(sQSDID, out iValue))
                                        {
                                        }
                                        iQSDID = iValue;
                                    }
                                }
                                else
                                {
                                    iQSDID = Convert.ToInt32(sQSValue);
                                }
                                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQSDID}", iQSDID.ToString(), null, null);
                                oCache.Set_ParamVal(sSessionID, sGUID, null, "-iQSDID", iQSDID.ToString(), null, null);
                                sQSType = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "sQSType".ToLower()).Select(m => m.Value).FirstOrDefault();
                                sMode = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "Mode".ToLower()).Select(m => m.Value).FirstOrDefault();
                                if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BODID".ToLower()).FirstOrDefault() != null)
                                {
                                    if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BODID".ToLower()).Select(m => m.Value).FirstOrDefault().ToLower() == "{XIP|BODID}".ToLower())
                                    {
                                        var ParamBOID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|BODID}");
                                        if (!string.IsNullOrEmpty(ParamBOID))
                                        {
                                            iBODID = Convert.ToInt32(ParamBOID);
                                        }
                                    }
                                }
                                if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BOIID".ToLower()).FirstOrDefault() != null)
                                {
                                    if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BOIID".ToLower()).Select(m => m.Value).FirstOrDefault().ToLower() == "{xip|boiid}".ToLower())
                                    {
                                        var ParamBOIID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + BO + ".id}");
                                        if (!string.IsNullOrEmpty(ParamBOIID))
                                        {
                                            iInstanceID = Convert.ToInt32(ParamBOIID);
                                        }
                                    }
                                }
                            }
                            else if (NVPair.Value.ToLower() == "QSStep".ToLower())
                            {
                                var sSessionID = HttpContext.Session.SessionID;
                                iQSDID = Convert.ToInt32(oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "QSDID".ToLower()).Select(m => m.Value).FirstOrDefault());
                                sStepName = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "QSStepName".ToLower()).Select(m => m.Value).FirstOrDefault();
                                sMode = oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "Mode".ToLower()).Select(m => m.Value).FirstOrDefault();
                                if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BODID".ToLower()).FirstOrDefault() != null)
                                {
                                    if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BODID".ToLower()).Select(m => m.Value).FirstOrDefault().ToLower() == "{XIP|BODID}".ToLower())
                                    {
                                        var ParamBOID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|BODID}");
                                        if (!string.IsNullOrEmpty(ParamBOID))
                                        {
                                            iBODID = Convert.ToInt32(ParamBOID);
                                        }
                                    }
                                }
                                if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BOIID".ToLower()).FirstOrDefault() != null)
                                {
                                    if (oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "BOIID".ToLower()).Select(m => m.Value).FirstOrDefault().ToLower() == "{XIP|boiid}".ToLower())
                                    {
                                        var ParamBOIID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + BO + ".id}");
                                        if (!string.IsNullOrEmpty(ParamBOIID))
                                        {
                                            iInstanceID = Convert.ToInt32(ParamBOIID);
                                        }
                                    }
                                }
                            }
                            else if (NVPair.Value.ToLower() == "Layout".ToLower())
                            {
                                iLayoutID = Convert.ToInt32(oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "LayoutID".ToLower()).Select(m => m.Value).FirstOrDefault());
                            }
                            StartAction = NVPair.Value;
                        }
                        else if (NVPair.Name.ToLower() == "ListClick".ToLower())
                        {
                            if (NVPair.Value.ToLower() == "Popup".ToLower())
                            {
                                PopupID = Convert.ToInt32(oXilink.XiLinkNVs.Where(m => m.Name.ToLower() == "PopupID".ToLower()).Select(m => m.Value).FirstOrDefault());
                            }
                        }
                        else if (NVPair.Name.ToLower() == "LoadingType".ToLower())
                        {
                            LoadingPattern = Convert.ToInt32(NVPair.Value);
                        }
                    }
                    //check mandatory params are gathered and if not return error with specified params
                    //var Result = ReportResult(ID, 1, "Inline", null, SearchType, null, 0);

                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetParentGUID(string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sSessionID = HttpContext.Session.SessionID;
                var sParentGUID = oCache.Get_ParamVal(sSessionID, sGUID, null, "|XIParent");
                return Json(sParentGUID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateInstanceID(XIIBO oBOInstance, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var iNewInstanceID = string.Empty;
                var id = oBOInstance.Attributes["ID"].sValue;
                iNewInstanceID = id;
                //var IDPair = oBOInstance.NVPairs.Where(m => m.sName.ToLower() == "id".ToLower()).FirstOrDefault();
                //if (IDPair != null)
                //{
                //    iNewInstanceID = IDPair.sValue;
                //}
                if (!string.IsNullOrEmpty(iNewInstanceID))
                {
                    string sSessionID = HttpContext.Session.SessionID;
                    //var ActiveBO = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}");
                    var ActiveBO = oBOInstance.BOD.TableName;
                    if (ActiveBO == "Driver_T")
                    {
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}", "Driver_T", "autoset", null);
                    }
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|" + ActiveBO + ".id}", Convert.ToString(iNewInstanceID), "autoset", null);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public void ClearCache()
        {
            var AppName = SessionManager.AppName;
            oCache.ClearCache(AppName);
        }

        #endregion XiLinkData

        #region 1ClickResult
        public VMResultList OneClickResult(string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string ResultIn = "Inline";
                string SearchText = sSearchText;
                int PageIndex = 1;
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var LoadingType = XiLinkRepository.GetLoadingType(OCID, sDatabase);
                if (LoadingPattern != -1)
                {
                    LoadingType[0] = LoadingPattern;
                }
                if (LoadingType[1] == 1 && sSearchType != "Structured")
                {
                    var Result = XiLinkRepository.GetStructuredOneClicks(OrgID, OCID, sDatabase);
                    if (ResultIn == "Run")
                    {
                        Result.FirstOrDefault().ResultIn = ResultIn;
                    }
                    return null;
                    //return PartialView("_StructuredOneClicksTree", Result);
                }
                else
                {
                    if (LoadingType[0] == 1)
                    {
                        var Report = XiLinkRepository.GetHeadings(OCID, sSearchType, sDatabase, OrgID, iUserID, sOrgName);
                        Report.ResultListDisplayType = LoadingPattern;
                        Report.ReportID = OCID;
                        Report.QueryID = OCID;
                        Report.PreviewType = ResultIn;
                        Report.SearchText = SearchText;
                        if (sSearchType == "Quick")
                        {
                            Report.SearchType = sSearchType;
                        }
                        else if (sSearchType == "Structured")
                        {
                            Report.PreviewType = "Structured";
                        }
                        Report.ResultListDisplayType = LoadingType[0];
                        //else
                        //{
                        //    Report.SearchType = SearchType;
                        //}
                        //Report.BO = BO;
                        //return View(Report);
                        return Report;
                    }
                    else
                    {
                        int? LeadID = 0;
                        VMRunUserQuery UserQuery = new VMRunUserQuery();
                        UserQuery.OrgID = OrgID;
                        UserQuery.UserID = iUserID;
                        UserQuery.database = sDatabase;
                        UserQuery.PageIndex = PageIndex;
                        UserQuery.ReportID = OCID;
                        UserQuery.LeadID = 0;
                        UserQuery.ClassFilter = 0;
                        UserQuery.DateFilter = 0;
                        UserQuery.SearchText = SearchText;
                        //UserQuery.BO = BO;
                        UserQuery.SearchType = sSearchType;
                        UserQuery.ResultListDisplayType = LoadingPattern;
                        VMResultList model = XiLinkRepository.RunUserQuery(UserQuery, iUserID, sOrgName, sDatabase);
                        model.ReportID = OCID;
                        var AutoLoadResultLlist = model.Rows;
                        if (sSearchType == "Structured")
                        {
                            model.PreviewType = "Structured";
                        }
                        else
                        {
                            model.PreviewType = ResultIn;
                        }
                        model.QueryID = OCID;
                        model.SearchText = SearchText;
                        //model.BO = BO;
                        model.SearchType = sSearchType;
                        if (PageIndex >= 2)
                        {
                            //return model;
                        }
                        //return View(model);
                        return model;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }


        [AllowAnonymous]
        public ActionResult GetOneClickResult(jQueryDataTableParamModel param, int i1ClickID, string sGUID, string sSearchText)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                if (Singleton.Instance.oParentGUID.ContainsKey(sGUID))
                {
                    sGUID = Singleton.Instance.oParentGUID[sGUID];
                }
                string sSessionID = HttpContext.Session.SessionID;
                XIInfraCache oCache = new XIInfraCache();
                XID1Click o1Click = new XID1Click();
                ModelDbContext dbconx = new ModelDbContext();
                o1Click = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
                if (!string.IsNullOrEmpty(o1Click.sActionOneClickType))
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|-iActionOneClickType}", o1Click.sActionOneClickType.ToString(), null, null);
                XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, o1Click.BOID.ToString());
                var oCopy = (XID1Click)o1Click.Clone(o1Click);
                //oCopy.bIsResolveFK = true;
                if (!string.IsNullOrEmpty(param.NVPairs))
                {
                    var NVPair = param.NVPairs.Replace("[", "").Replace("]", "").Replace("\"", "").Split(',').ToList();
                    foreach (var NV in NVPair)
                    {
                        var MergeValues = NV.Split('-');
                        if (MergeValues.Count() == 2)
                        {
                            oCopy.Query = oCopy.Query.Replace(MergeValues[0], "'" + MergeValues[1] + "'");
                        }
                    }
                }
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                List<CNV> nParams = new List<CNV>();
                nParams = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                oCopy.ReplaceFKExpressions(nParams);
                oCopy.iSkip = param.iDisplayStart;
                if (oCopy.iPaginationCount != 0)
                {
                    oCopy.iTake = oCopy.iPaginationCount;
                }
                else
                {
                    oCopy.iTake = param.iDisplayLength;
                }
                oCopy.NVPairs = param.NVPairs;
                oCopy.SearchType = param.Type;
                oCopy.Fields = param.Fields;
                oCopy.Optrs = param.Optrs;
                oCopy.Values = param.Values;
                oCopy.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                oCopy.sSortDir = Request["sSortDir_0"].ToString();
                oCopy.SearchText = sSearchText;
                if (string.IsNullOrEmpty(sSearchText) && !string.IsNullOrEmpty(param.SearchText))
                {
                    oCopy.SearchText = param.SearchText;
                }
                if (!oCopy.bIsMultiBO)
                {
                    oCopy.bIsResolveFK = true;
                }
                oCopy.Resolve1Click();
                if (oCopy.Query.Contains("{") || oCopy.Query.Contains("|"))
                {
                    Common.SaveErrorLog("Error: Unresolved Query in GetOneClickResult Method: " + oCopy.Query, sDatabase);
                }
                oCopy.sParentWhere = param.sParentWhere;
                var bOrgSwitch = SessionManager.bOrgSwitch;
                if (bOrgSwitch)
                {
                    var iUserOrg = SessionManager.iUserOrg;
                    var OrgID = string.Empty;
                    if (iUserOrg > 0)
                    {
                        XIIXI oXI = new XIIXI();
                        var oBOI = oXI.BOI("XIUserOrgMapping", iUserOrg.ToString());
                        if (oBOI != null && oBOI.Attributes.Count() > 0)
                        {
                            var RoleID = oBOI.AttributeI("FKiRoleID").sValue;
                            OrgID = oBOI.AttributeI("FKiOrgID").sValue;
                            if (!string.IsNullOrEmpty(OrgID))
                            {
                                var Org = oXI.BOI("Organisations", OrgID);
                                if (Org != null && Org.Attributes.Count() > 0)
                                {
                                    var OrgDB = Org.AttributeI("DatabaseName").sValue;
                                    oCopy.sSwitchDB = OrgDB;
                                }

                            }
                        }
                    }
                }
                var oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, oBOD.iDataSource.ToString());
                if (!string.IsNullOrEmpty(oDataSource.sQueryType) && oDataSource.sQueryType.ToLower() == "mongodb")
                {
                    oCopy.OneClick_ExecuteV2();
                }
                else
                {
                    oCopy.OneClick_Execute();
                }
                if (oCopy.Headings != null)
                {
                    oCopy.Headings.Add("Color");
                }
                List<string[]> oDataTableResult = new List<string[]>();
                var MyLinks = oCopy.MyLinks;
                string sButtons = string.Empty;
                var oSetting = MyLinks.Where(m => m.iType == 0).FirstOrDefault();
                string sCheckBox = string.Empty;
                if (oCopy != null)
                {
                    string sEditBtn = string.Empty;
                    string sCopyBtn = string.Empty;
                    string sDeleteBtn = string.Empty;
                    string sViewbtn = string.Empty;
                    string sPreviewbtn = string.Empty;
                    string sAddBottombtn = string.Empty, sAddTopbtn = string.Empty;
                    string sOrderIncrementbtn = string.Empty, sOrderDecrementbtn = string.Empty, sCompilebtn = string.Empty;
                    if (oCopy.IsEdit)
                    {
                        sEditBtn = "<input type='button' class='btn btn-theme' value='Edit' onclick ='fncEditBO(this," + oCopy.ID + "," + oCopy.EditGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsCopy)
                    {
                        sCopyBtn = "<input type='button' class='btn btn-theme' value='Copy' onclick ='fncCopyBO(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsDelete)
                    {
                        sDeleteBtn = "<input type='button' class='btn btn-theme' value='Delete' onclick ='fncDeleteBO(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsView)
                    {
                        sViewbtn = "<input type='button' class='btn btn-theme' value='View' onclick ='fncViewBO(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsCheckbox) // * TO DO THIS IS HARD CODED FOR RECONCILLIATION NEED TO CHANGE IT LATER
                    {
                        sCheckBox = "<input type='checkbox' class='chkReconcilliation' Onchange ='fncCheckboxOnchange(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsAddTop)
                    {
                        sAddTopbtn = "<input type='button' class='btn btn-theme lbluebtn' value='+' onclick ='fncAddInstanceTop(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsAddBottom)
                    {
                        sAddBottombtn = "<input type='button' class='btn btn-theme lbluebtn' value='-' onclick ='fncAddInstanceBottom(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                    }
                    if (oCopy.bIsOrderIncrement)
                    {
                        sOrderIncrementbtn = "<input type='button' class='btn btn-theme lbluebtn' value='↑' onclick ='fncOrderIncrement(this," + oCopy.ID + "," + oCopy.EditGroupID + ",\"" + oCopy.sBOName + "\"," + oCopy.iCreateXILinkID + ",\"" + oCopy.SelectFields + "\")' />";
                    }
                    if (oCopy.bIsOrderDecrement)
                    {
                        sOrderDecrementbtn = "<input type='button' class='btn btn-theme lbluebtn' value='↓' onclick ='fncOrderDecrement(this," + oCopy.ID + "," + oCopy.EditGroupID + ",\"" + oCopy.sBOName + "\"," + oCopy.iCreateXILinkID + ",\"" + oCopy.SelectFields + "\")' />";
                    }
                    if (oCopy.sCompile != "0")
                    {
                        sCompilebtn = "<input type='button' class='btn btn-theme lbluebtn' value='Compile' onclick ='XIRun(this," + oCopy.sCompile + "," + oCopy.ID + ",\"" + sGUID + "\",\"" + oCopy.sBOName + "\",false)' />";
                    }
                    if (oCopy.bIsPreview)
                    {
                        sPreviewbtn = "<input type='button' class='btn btn-theme' value='Preview' onclick ='PreviewTemplate(this, " + oCopy.ID + ", " + oCopy.iCreateXILinkID + ")' />";
                        if (oCopy.bIsCheckbox)
                        {
                            sCheckBox = "<input type='checkbox' class='chkTemplate' Onchange ='fncSetTemplateToCache(this," + oCopy.ID + "," + oCopy.iCreateXILinkID + ")' />";
                        }
                    }
                    sButtons = sEditBtn + sCopyBtn + sDeleteBtn + sViewbtn + sPreviewbtn + sAddTopbtn + sAddBottombtn + sOrderIncrementbtn + sOrderDecrementbtn + sCompilebtn;
                }
                if (oCopy.Actions != null && oCopy.Actions.Count() > 0)
                {
                    string sCopyBtn = string.Empty;
                    string sDeleteBtn = string.Empty;
                    string sOtherBtn = string.Empty;
                    foreach (var items in oCopy.Actions)
                    {
                        if (items.FKiActionID > 0)
                        {
                            var oActionD = (XIDBOAction)oCache.GetObjectFromCache(XIConstant.CacheBOAction, null, items.FKiActionID.ToString());
                            if (oActionD.iType == 10 && oActionD.iSystemAction == 10)
                            {
                                sCopyBtn = "<input type='button' class='btn btn-theme' value='" + oActionD.sName + "' onclick ='fncCopyBO(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                            }
                            else if (oActionD.iType == 10 && oActionD.iSystemAction == 20)
                            {
                                sDeleteBtn = "<input type='button' class='btn btn-theme' value='" + oActionD.sName + "' onclick ='fncDeleteBO(this," + oCopy.ID + "," + oCopy.CreateGroupID + "," + oCopy.BOID + "," + oCopy.iCreateXILinkID + ")' />";
                            }
                            else if (oActionD.iType == 60)
                            {
                                XIDBODefault oDefault = new XIDBODefault();
                                oDefault = (XIDBODefault)oCache.GetObjectFromCache(XIConstant.CacheBODefault, null, oCopy.BOID.ToString());
                                sOtherBtn = sOtherBtn + "<input type='button' class='btn btn-theme' onclick='fnc1ClickAction(this, " + oCopy.BOID + "," + oDefault.iPopupID + ", \"" + oCopy.RowXiLinkID + "\", \"" + sGUID + "\")' data-type='defaultpopup' value='" + oActionD.sName + "' />";
                            }
                            else
                            {
                                sOtherBtn = sOtherBtn + "<input type='button' class='btn btn-theme' value='" + oActionD.sName + "' />";
                            }
                        }
                        sButtons = sCopyBtn + sDeleteBtn + sOtherBtn;
                    }
                }

                string[] sHiddenArray = oCopy.oOneClickParameters.Where(x => x.iType == 10).Select(x => x.sName).ToArray();
                int ListCount = 0; int LoopCount = 0;
                if (!string.IsNullOrEmpty(oCopy.sTotalColumns))
                {
                    ListCount = oCopy.oDataSet.Count();
                }
                var sPrimarykey = oBOD.sPrimaryKey;
                var Enc = oCopy.oDataSet.Values.Select(y => y.Attributes).ToList();
                XIInfraEncryption oEncrypt = new XIInfraEncryption();
                foreach (var attr in Enc)
                {
                    var ioRoleID = attr.Where(i => i.Key.ToLower() == "iroleid").Select(u => u.Value.sValue).FirstOrDefault();
                    var bIsEncrypt = oBOD.Attributes.Values.Where(m => m.bIsEncrypt == true).ToList().ToList().Select(i => i.Name).ToList();
                    var bIsUserEncrypt = oBOD.Attributes.Values.Where(m => m.bIsUserEncrypt == true).ToList().Select(i => i.Name).ToList();
                    if (bIsEncrypt != null && bIsEncrypt.Count > 0 /*&& !string.IsNullOrEmpty(Userid)*/)
                    {
                        foreach (var Encitem in bIsEncrypt)
                        {
                            var ID = attr.Where(i => i.Key.ToLower().Contains(sPrimarykey.ToLower())).Select(u => u.Value.sValue).FirstOrDefault();
                            var sEncrptionData = attr.Where(i => i.Key.ToLower().Contains(Encitem.ToLower())).Select(u => u.Value).FirstOrDefault();
                            if (sEncrptionData != null)
                            {
                                var sDecryptData = oEncrypt.DecryptData(sEncrptionData.sValue, true, ID);
                                sEncrptionData.sValue = sDecryptData;
                            }
                        }
                    }
                    if (bIsUserEncrypt != null && bIsUserEncrypt.Count > 0 /*&& !string.IsNullOrEmpty(Userid)*/)
                    {
                        if (ioRoleID == SessionManager.iRoleID.ToString())
                        {
                            var sRoleDetails = oIXI.BOI("XIAppRoles_AR_T", SessionManager.iRoleID.ToString());
                            var XIGUIDValue = sRoleDetails.Attributes.Values.Where(n => n.sName.ToLower() == "xiguid").Select(i => i.sValue).FirstOrDefault();
                            foreach (var Encitem in bIsUserEncrypt)
                            {
                                var ID = attr.Where(i => i.Key.ToLower().Contains(sPrimarykey.ToLower())).Select(u => u.Value.sValue).FirstOrDefault();
                                var sEncryptValue = attr.Where(i => i.Key.ToLower().Contains(Encitem.ToLower())).Select(u => u.Value).FirstOrDefault();
                                if (sEncryptValue != null)
                                {
                                    var DecVaule = oEncrypt.DecryptData(sEncryptValue.sValue, true, ID, XIGUIDValue);
                                    sEncryptValue.sValue = DecVaule;
                                }
                            }
                        }
                    }
                }
                var SrchText = param.SearchText;
                List<string> QuickAttrs = new List<string>();
                if (oBOD.Groups.ContainsKey("Quick Search"))
                {
                    QuickAttrs = oBOD.GroupD("Quick Search").BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                if (QuickAttrs != null && QuickAttrs.Count() == 0)
                {
                    QuickAttrs.Add("sName");
                }
                oCopy.oDataSet.ToList().ForEach(m =>
                {
                    if (param.Type == "Quick")
                    {
                        foreach (var items in QuickAttrs)
                        {
                            if (m.Value.Attributes.ContainsKey(items))
                            {
                                var valdd = m.Value.AttributeI(items.ToLower()).sValue;
                                string r = @"" + SrchText + "$";
                                var Matches = Regex.Matches(valdd, r);
                                var bIS = Regex.IsMatch(valdd, SrchText, RegexOptions.IgnoreCase);
                                MatchCollection matches = Regex.Matches(valdd, SrchText, RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    foreach (Match mat in matches)
                                    {
                                        m.Value.AttributeI(items.ToLower()).sValue = m.Value.AttributeI(items.ToLower()).sValue.Replace(mat.Value, "<span class=\"cell-color\">" + mat.Value + "</span>");
                                    }
                                }

                                //m.Value.AttributeI(items.ToLower()).sValue = m.Value.AttributeI(items.ToLower()).sValue.Replace(SrchText, "<span class=\"cell-color\">" + SrchText + "</span>");
                            }
                        }
                    }
                    foreach (var Link in MyLinks.Where(n => n.iType == 10))
                    {
                        XIIAttribute oAttrI = new XIIAttribute();
                        oAttrI.sValue = "<input type='button' class='btn btn-theme' value='Post' onclick='XIRun('" + Link.FKiXILinkID + "')' />";
                        m.Value.Attributes["Post"] = oAttrI;
                    }
                    if (!string.IsNullOrEmpty(sButtons))
                    {
                        XIIAttribute oAttrI = new XIIAttribute();
                        oAttrI.sValue = sButtons;
                        m.Value.Attributes["Actions"] = oAttrI;
                    }
                    //m.Value.Attributes.ToList().ForEach(s => s.Value.BOD = oBOD);
                    var dtrow = m.Value.Attributes.Values;
                    string sIndentString = string.Empty;
                    if (dtrow.Any(x => !string.IsNullOrEmpty(x.sName) && x.sName.ToLower() == "sindent"))
                    {
                        string sIndent = dtrow.Where(x => !string.IsNullOrEmpty(x.sName) && x.sName.ToLower() == "sindent").Select(t => t.sValue).FirstOrDefault();
                        if (!string.IsNullOrEmpty(sIndent))
                        {
                            for (int i = 0; i < sIndent.Length; i++)
                            {
                                sIndentString += "<span><i class='far fa-window-restore'></i></span>";
                            }
                            dtrow.Where(x => x.sName.ToLower() == "sindent").FirstOrDefault().sValue = sIndentString;
                        }
                    }
                    LoopCount++;
                    List<string> strarray = dtrow.Select(n => n.sValue).ToList();
                    if (oCopy.oScripts != null && oCopy.oScripts.Count() > 0)
                    {
                        m.Value.GetScriptResult(oCopy.oScripts);
                        strarray.Add(m.Value.ScriptResults.Select(x => x.sScriptResult).FirstOrDefault());
                    }
                    if (!string.IsNullOrEmpty(sCheckBox))
                    {
                        string[] newarray = new string[strarray.Count() + 1];
                        if (!string.IsNullOrEmpty(oCopy.sTotalColumns) && ListCount == LoopCount)
                        {
                            newarray[0] = "";
                        }
                        else
                        {
                            newarray[0] = sCheckBox;
                        }
                        for (int i = 0; i < strarray.Count(); i++)
                        {
                            newarray[i + 1] = strarray[i];
                        }
                        strarray = newarray.ToList();
                        oDataTableResult.Add(strarray.ToArray());
                    }
                    else
                    {
                        oDataTableResult.Add(strarray.ToArray());
                    }
                });
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = oCopy.iTotalRecords,
                    iTotalDisplayRecords = oCopy.iTotalRecords,
                    aaData = oDataTableResult
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

        //[HttpPost]
        [AllowAnonymous]
        public ActionResult GetReportResult(jQueryDataTableParamModel param, int ReportID, string SearchText, string SearchType, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sSessionID = HttpContext.Session.SessionID;
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                List<cNameValuePairs> nParams = new List<cNameValuePairs>();
                nParams = oGUIDParams.NMyInstance.Select(m => new cNameValuePairs { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                string sCurrentGuestUser = string.Empty;
                if (SessionManager.UserUniqueID != null)
                {
                    sCurrentGuestUser = SessionManager.UserUniqueID;
                }
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                VMQuickSearch Search = new VMQuickSearch();
                Search.ReportID = ReportID;
                Search.UserID = iUserID;
                Search.OrgID = OrgID;
                Search.database = sDatabase;
                Search.SearchType = SearchType;
                Search.SearchText = SearchText;
                if (oUser.Role != null)
                {
                    Search.Role = oUser.Role.sRoleName;
                }
                Search.BO = BO;
                Search.sGUID = sGUID;
                var result = XiLinkRepository.GetReportResult(param, Search, iUserID, sOrgName, sDatabase, sCurrentGuestUser, nParams);
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
        public ActionResult RunUserQuery(VMRunUserQuery QValues)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                QValues.OrgID = OrgID;
                QValues.UserID = iUserID;
                QValues.database = sDatabase;
                QValues.LeadID = 0;
                QValues.ClassFilter = 0;
                QValues.DateFilter = 0;
                VMResultList model = XiLinkRepository.RunUserQuery(QValues, iUserID, sOrgName, sDatabase);
                ViewBag.queryid = QValues.ReportID;
                var AutoLoadResultLlist = model.Rows;
                if (QValues.PageIndex >= 2 || QValues.SearchType == "FilterSearch" || QValues.SearchType == "NaturalSearch" || QValues.SearchType == "Quick")
                {
                    return Json(AutoLoadResultLlist, JsonRequestBehavior.AllowGet);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult ReportResult(int QueryID, int PageIndex, string ResultIn, string SearchText, string SearchType, int iPID)
        {
            List<CNV> oParams = new List<CNV>();
            CNV Prm = new CNV();
            Prm.sName = XIConstant.Param_1ClickID;
            Prm.sValue = QueryID.ToString();
            oParams.Add(Prm);
            CNV Prm2 = new CNV();
            Prm2.sName = XIConstant.Param_SearchText;
            Prm2.sValue = SearchText;
            oParams.Add(Prm2);
            CNV Prm3 = new CNV();
            Prm3.sName = XIConstant.Param_SearchType;
            Prm3.sValue = SearchType;
            oParams.Add(Prm3);
            if (iPID > 0)
            {
                CNV Prm4 = new CNV();
                Prm4.sName = XIConstant.Param_ParentFKColumn;
                Prm4.sValue = "iBuildingID";
                oParams.Add(Prm4);
                CNV Prm5 = new CNV();
                Prm5.sName = XIConstant.Param_ParentInsID;
                Prm5.sValue = iPID.ToString();
                oParams.Add(Prm5);
            }
            CNV Prm6 = new CNV();
            Prm6.sName = "Visualisation";
            Prm6.sValue = "OneClickVisibility";
            oParams.Add(Prm6);
            XIInfraOneClickComponent oClick = new XIInfraOneClickComponent();
            var oCR = oClick.XILoad(oParams);
            XIIComponent oCompnent = new XIIComponent();
            if (oCR.bOK && oCR.oResult != null)
            {
                oCompnent.oContent[XIConstant.OneClickComponent] = oCR.oResult;
            }
            oCompnent.oVisualisation = oCompnent.oVisualisation ?? new List<XIVisualisation>();
            if (oCompnent.oVisualisation.Count() == 0)
            {
                oCompnent.oVisualisation.Add(new XIVisualisation());
            }
            if (oCompnent.oVisualisation.FirstOrDefault().XiVisualisationNVs == null)
            {
                oCompnent.oVisualisation.FirstOrDefault().XiVisualisationNVs = new List<XIVisualisationNV>();
            }
            oCompnent.oVisualisation.FirstOrDefault().XiVisualisationNVs.Add(new XIVisualisationNV() { sName = "bIsQuickSearch", sValue = "yes" });
            oCompnent.oVisualisation.FirstOrDefault().XiVisualisationNVs.Add(new XIVisualisationNV() { sName = "HiddenData", sValue = "yes" });
            return PartialView("~\\Views\\XIComponents\\_OneClickComponent.cshtml", oCompnent);
            //OCID = QueryID;
            //sSearchType = SearchType;
            //sSearchText = SearchText;
            //var Result = OneClickResult(null);
            //return View("OneClickResults", Result);
        }

        public ActionResult GetPieChart(int ReportID, string ResultIn, string Query)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                VMChart Chart = new VMChart();
                Chart.ReportID = ReportID;
                Chart.Query = Query;
                Chart.OrgID = OrgID;
                Chart.UserID = iUserID;
                Chart.Database = sDatabase;
                GraphData Graph = new GraphData();
                Graph = XiLinkRepository.GetPieChart(Chart, iUserID, sOrgName, sDatabase);
                Graph.Type = "Run";
                Graph.SectionName = "";
                Graph.ClassDDL = new List<VMDropDown>();
                Graph.DateDDL = new List<VMDropDown>();
                return PartialView("_KPIPieChart", Graph);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        public ActionResult GetBarChart(int ReportID, string ResultIn, string Query)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                //ReportID = OCID;
                VMChart Charts = new VMChart();
                Charts.ReportID = ReportID;
                Charts.OrgID = OrgID;
                Charts.UserID = iUserID;
                Charts.Database = sDatabase;
                Charts.Query = Query;
                LineGraph graph = XiLinkRepository.GetBarChart(Charts, sDatabase, iUserID, sOrgName);
                graph.ReportID = ReportID;
                graph.Type = "Run";
                graph.SectionName = "";
                graph.ClassDDL = new List<VMDropDown>();
                graph.DateDDL = new List<VMDropDown>();
                return PartialView("_KPIBarChart", graph);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        public ActionResult GetLineGraph(int ReportID, string ResultIn, string Query)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                //ReportID = OCID;
                VMChart Chart = new VMChart();
                Chart.ReportID = ReportID;
                Chart.OrgID = OrgID;
                Chart.UserID = iUserID;
                Chart.Database = sDatabase;
                Chart.Query = Query;
                LineGraph graph = XiLinkRepository.GetLineChart(Chart, iUserID, sOrgName, sDatabase);
                graph.ReportID = ReportID;
                graph.Type = "Run";
                graph.SectionName = "";
                graph.ClassDDL = new List<VMDropDown>();
                graph.DateDDL = new List<VMDropDown>();
                return PartialView("_LineGraph", graph);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        public ActionResult GetFormData(string sBOName, string sGroupName)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                if (sBOName != null && sGroupName != null)
                {
                    var oBODisplay = oXiAPI.GetFormData(sBOName, sGroupName, iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                    return PartialView("_CreateForm", oBODisplay);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return null;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetAutoCompleteData(int i1ClickID, string sAutoText, string sField)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                //int iUserID = 0;
                //if (SessionManager.UserID > 0)
                //{
                //    iUserID = SessionManager.UserID;
                //}
                //string sOrgName = SessionManager.OrganisationName;
                //XID1Click o1ClickD = new XID1Click();
                //XID1Click o1ClickC = new XID1Click();
                //o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
                //o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                //var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickC.BOID.ToString());
                //o1ClickC.BOD = oBOD;
                //o1ClickC.bIsResolveFK = true;
                //o1ClickC.Get_1ClickHeadings();
                //o1ClickC.OneClick_Execute();
                //return PartialView("~/views/XIComponents/_OneClickComponent.cshtml", o1ClickC);

                OCID = i1ClickID;
                sSearchText = sAutoText;
                var Result = OneClickResult(null);
                ViewBag.Field = sField;
                Result.SearchType = "Quick";
                return PartialView("_AutoComplete", Result);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetLabelData(int iBOID = 0, string Label = null, int i1ClickID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                var Result = XiLinkRepository.GetLabelData(iBOID, Label, i1ClickID, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        #endregion 1ClickResult

        #region Popup

        [AllowAnonymous]
        public ActionResult GetPopupORDialog(int XiLinkID, string sGUID, string sNewGuid, string BO = "", string sID = "", int PRDID = 0, int BODID = 0, string ActRec = "", string EnumReconciliation = "", string Type = "", List<CNV> oNVParams = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            string sParGUID = string.Empty;
            try
            {
                int ID = 0;
                if (int.TryParse(sID, out ID))
                { }
                XILink oXiLink = null;
                string sOrgName = SessionManager.OrganisationName;
                string sSessionID = HttpContext.Session.SessionID;
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                if (XiLinkID > 0)
                {
                    oXiLink = GetXiLinkDetails(XiLinkID, sGUID);
                    XIRun(oXiLink, sGUID);
                }
                else
                {
                    DialogID = PRDID;
                }
                if (string.IsNullOrEmpty(BO) && BODID != 0)
                {
                    var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BODID.ToString());
                    BO = oBOD.Name;
                }
                XICacheInstance oGUIDParams = new XICacheInstance();
                if (oXiLink.sType.ToLower() == "QuestionSet".ToLower() || oXiLink.sType.ToLower() == "QS".ToLower())
                {
                    oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                    sGUID = "";
                    Common.SaveErrorLog("XiLinkID: " + XiLinkID + " is Loaded from previous xiload method", sDatabase);
                }
                if (PopupID > 0)
                {
                    //var PopDetails = XiLinkRepository.GetPopupLayoutDetails(PopupID, ID, BOID, sGUID, iUserID, sOrgName, sDatabase);
                    //PopDetails.sNewGUID = sGUID;
                    //return PartialView("Popup", PopDetails);
                }
                else if (DialogID > 0)
                {
                    XIDDialog oDialog = null;
                    oDialog = (XIDDialog)oCache.GetObjectFromCache(XIConstant.CacheDialog, null, DialogID.ToString()); //oXID.Get_DialogDefinition(DialogID.ToString());
                    XIDLayout oLayout = new XIDLayout();
                    oLayout = (XIDLayout)oCache.GetObjectFromCache(XIConstant.CacheLayout, null, oDialog.LayoutID.ToString()); //oXID.Get_LayoutDefinition(null, oDialog.LayoutID.ToString());
                    //var sSessionID = HttpContext.Session.SessionID;
                    if (!string.IsNullOrEmpty(sGUID))
                    {
                        oCache.sSessionID = sSessionID;
                        var ParentGUID = oCache.GetParentGUIDRecurrsive(sGUID);
                        sParGUID = ParentGUID;
                    }
                    else
                    {
                        sParGUID = sNewGuid;
                    }
                    if (oGUIDParams != null && oGUIDParams.NMyInstance.Count() > 0)
                    {
                        List<CNV> oParams = new List<CNV>();
                        oParams.AddRange(oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue }));
                        oCache.SetXIParams(oParams, sParGUID, sSessionID);
                    }
                    oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|ActRec}", ActRec, null, null);
                    oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|EnumReconciliations_T.id}", EnumReconciliation, null, null);
                    oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|EnumReconType}", Type, null, null);
                    //sParGUID = sNewGuid;
                    oCache.Set_ParamVal(sSessionID, sParGUID, null, "XICache", "XICache", null, null);
                    if (!string.IsNullOrEmpty(BO))
                    {
                        oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|ActiveBO}", BO.ToString(), null, null);
                        oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|sBOName}", BO.ToString(), null, null);
                    }
                    if (ID > 0)
                    {
                        oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|" + BO + ".id}", ID.ToString(), "autoset", null);
                        oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|iInstanceID}", ID.ToString(), null, null);
                    }

                    oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|BODID}", BODID.ToString(), null, null);
                    if (BO.ToLower() == "riskfactors")
                    {
                        oCache.Set_ParamVal(sSessionID, sParGUID, null, "{XIP|FKsQuoteGUID}", sID, null, null);
                    }
                    // oCache.Get_ParamVal(sSessionID, sParGUID, null, "|XIParent");

                    CResult oCResult = oCache.MergeXILinkParameters(oXiLink, sParGUID, null, sSessionID);
                    oLayout.sNewGUID = sParGUID;
                    oLayout.LayoutMappings = oLayout.LayoutMappings.Where(m => m.PopupID == oDialog.ID).ToList();
                    if (oLayout.XiParameterID > 0)
                    {
                        oCache.AddParamsToGUID(oLayout.XiParameterID, sParGUID, null);
                    }
                    if (!string.IsNullOrEmpty(sGUID) && !string.IsNullOrEmpty(sParGUID))
                    {
                        XIInfraCache oXICache = new XIInfraCache();
                        oXICache.Init_RuntimeParamSet(sSessionID, sNewGuid, sGUID, null);
                    }
                    ViewBag.oPramas = oNVParams;
                    return PartialView("Dialog", oLayout);
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //[HttpPost]
        [AllowAnonymous]
        public ActionResult GetLayoutDetails(int iLayoutID = 0, string sParentGUID = "", string sSection = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                var oLayout = (XIDLayout)oCache.GetObjectFromCache(XIConstant.CacheLayout, null, iLayoutID.ToString());
                //var Result = XiLinkRepository.GetLayoutDetails(iLayoutID, sParentGUID, sSection, sDatabase);
                return PartialView("Dialog", oLayout);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetPopupContent(VMPopupLayout model)
        {
            return PartialView("_PopupContent", model);
        }

        [AllowAnonymous]
        public ActionResult ShowPopupContent(int ID)
        {
            return PartialView("MultiPopupContent");
        }

        [AllowAnonymous]
        public ActionResult GetDialogContent(XIDLayout model, List<CNV> oNVParams = null)
        {
            ViewBag.oParams = oNVParams;
            var code = model.LayoutCode.Replace(@"\", string.Empty);
            return PartialView("_DialogContent", model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult LoadLayout(int iLayoutID = 0, List<CNV> oParams = null, string sGUID = "", string sType = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIDLayout oLayout = new XIDLayout();
                oLayout.ID = iLayoutID;
                if (!string.IsNullOrEmpty(sGUID))
                {
                    oLayout.sGUID = sGUID;
                }
                oParams = oParams ?? new List<CNV>();
                if (sType == "Refresh")
                {
                    string sSessionID = HttpContext.Session.SessionID;
                    XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                    oParams.AddRange(oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue }));
                }
                oLayout.oLayoutParams = oParams;
                var oLayRes = oLayout.Load();
                if (oLayRes.bOK && oLayRes.oResult != null)
                {
                    oLayout = (XIDLayout)oLayRes.oResult;
                    XIInstanceBase oInsBase = new XIInstanceBase();
                    oInsBase.oContent[XIConstant.ContentLayout] = oLayout;
                    var oMergedData = (XIInstanceBase)MergeHTMLRecurrsive(oInsBase);
                    oLayout = (XIDLayout)oMergedData.oContent[XIConstant.ContentLayout];
                    return Json(oLayout, JsonRequestBehavior.AllowGet);
                }
                return Json(oLayout, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [AllowAnonymous]
        public ActionResult GetLayoutData(XIDLayout model)
        {
            //Check for parentguid
            if (model.sGUID != null)
            {
                string sSessionID = HttpContext.Session.SessionID;
                //SessionManager.sGUID = model.sGUID;
                var sParentGUID = oCache.Get_ParamVal(sSessionID, model.sGUID, null, "{XIP|ParentGUID}");
                if (!string.IsNullOrEmpty(sParentGUID))
                {
                    //model.sGUID = sParentGUID;
                }
                else
                {
                    sParentGUID = oCache.Set_ParamVal(sSessionID, model.sGUID, null, "{XIP|ParentGUID}", model.sGUID.ToString(), null, null);
                }
                // model.sGUID = sParentGUID;
            }
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return PartialView("_LayoutContent", model);
                //XIInstanceBase oData = new XIInstanceBase();
                //oData.oContent[XIConstant.ContentLayout] = model;
                //dynamic oMergedData = MergeHTMLRecurrsive(oData);
                //var LayoutData = oMergedData.oContent[XIConstant.ContentLayout];
                //return PartialView("_LayoutContent", LayoutData);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [AllowAnonymous]
        public ActionResult GetLayoutContent(XIDLayout model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIInstanceBase oData = new XIInstanceBase();
                oData.oContent[XIConstant.ContentLayout] = model;
                dynamic oMergedData = MergeHTMLRecurrsive(oData);
                var LayoutData = oMergedData.oContent[XIConstant.ContentLayout];
                //return Json(oMergedData, JsonRequestBehavior.AllowGet);
                return PartialView("_LayoutData", LayoutData);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            //XIDLayout oLayout = new XIDLayout();
            ////var oLayDef = oXID.Get_LayoutDefinition(null, iLayoutID.ToString());
            //var oLayDef = model.Load();
            //if (oLayDef.bOK && oLayDef.oResult != null)
            //{
            //    oLayout = (XIDLayout)((XIInstanceBase)oLayDef.oResult).oContent[XIConstant.ContentLayout];
            //}
            ////Check for parentguid
            //if (model.sGUID != null)
            //{
            //    string sSessionID = HttpContext.Session.SessionID;
            //    //SessionManager.sGUID = model.sGUID;
            //    var sParentGUID = oCache.Get_ParamVal(sSessionID, model.sGUID, null, "{XIP|ParentGUID}");
            //    if (!string.IsNullOrEmpty(sParentGUID))
            //    {
            //        //model.sGUID = sParentGUID;
            //    }
            //    else
            //    {
            //        sParentGUID = oCache.Set_ParamVal(sSessionID, model.sGUID, null, "{XIP|ParentGUID}", model.sGUID.ToString(), null, null);
            //    }
            //    // model.sGUID = sParentGUID;
            //}

        }

        [AllowAnonymous]
        public ActionResult GetStepLayoutContent(cLayouts model)
        {
            return PartialView("_StepLayoutContent", model);
        }

        public ActionResult GetMenus()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Menus = XiLinkRepository.GetMenus(string.Empty, iUserID, OrgID, sDatabase);
                return Json(Menus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetDialog(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sOrgName = SessionManager.OrganisationName;
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Res = XiLinkRepository.GetDialog(ID, sDatabase);
                return Json(Res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetPopupDetails(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Res = XiLinkRepository.GetPopupDetailsByID(ID, sDatabase);
                return Json(Res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult GetInlineDetails(int ID)
        //{
        //    try
        //    {
        //        int OrgID = OrgID;
        //        string database = sDatabase;
        //        var Res = XiLinkRepository.GetInlineDetails(ID, database);
        //        return Json(Res, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return Json(0, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public ActionResult GetChildForMenu(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                List<RightMenuTrees> Models = XiLinkRepository.GetChildForMenu(ID, OrgID, sDatabase);
                return Json(Models, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeadPopupOLD(int LeadID, int XiLinkID, string PopType, int ClientID = 0, int StageID = 0, int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var XiLink = GetXiLinkDetails(XiLinkID, null);
                //XIRun(XiLink);
                VMViewPopup popup = new VMViewPopup();
                popup.LeadID = LeadID;
                popup.PopupID = PopupID;
                popup.UserName = oUser.Role.sRoleName;
                popup.StageID = StageID;
                popup.PopType = PopType;
                popup.ClientID = ClientID;
                popup.RowID = ID;
                popup.DailogID = ID;
                var Res = XiLinkRepository.GetPopupDetails(PopupID, sDatabase);
                if (Res.IsLeftMenu)
                {
                    popup.IsLeftMenu = true;
                }
                else
                {
                    popup.IsLeftMenu = false;
                }
                popup.LayoutType = Res.LayoutID;
                return View("LeadPopup", popup);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeadPopupLeftContent(VMViewPopup Popup)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                VMLeadPopupLeft model = XiLinkRepository.GetLeadPopupLeftContent(Popup, sDatabase, OrgID, iUserID, sOrgName);
                return PartialView("_LeadPopupLeftContent", model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult LeadContent(VMViewPopup Popup)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                VMViewPopup popup = new VMViewPopup();
                popup.LeadID = Popup.LeadID;
                popup.PopupID = Popup.ReportID;
                popup.Tabs = XiLinkRepository.GetAllTabs(Popup.ReportID, Popup.PopupID, sDatabase);
                popup.ClientID = Popup.ClientID;
                popup.PopType = Popup.PopType;
                popup.StageID = Popup.StageID;
                popup.RowID = Popup.RowID;
                popup.LayoutType = Popup.LayoutType;
                return PartialView("_LeadPopupContent", popup);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult CallAction(VMLeadActions model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var result = XiLinkRepository.CallAction(model, OrgID, iUserID, sOrgName, sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveLeadTransaction(Stages model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var result = XiLinkRepository.SaveLeadTransaction(model, OrgID, sDatabase, iUserID, sOrgName);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNextStages(int LeadID, int StageID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var result = XiLinkRepository.GetNextStages(LeadID, StageID, OrgID, iUserID, sOrgName, sDatabase);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult SendClientRequest(int LeadID, string Email, int OrgID, string OrgName, int ClassID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                //LeadRepository.SendRegisterMail(Email, "Register", OrgID, sDatabase, ClassID);
                WalletRequests Request = new WalletRequests();
                Request.LeadID = LeadID;
                Request.EmailID = Email;
                Request.OrganizationID = OrgID;
                Request.IsActivated = false;
                Request.FKiLeadClassID = 0;
                var Res = XiLinkRepository.SaveWalletRequest(Request, sDatabase);
                return Json(Res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetTabContent(VMViewPopup Popup)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                if (Popup.Name == "Reminders")
                {
                    Reminders model = new Reminders();
                    model.ReportID = Popup.ReportID;
                    model.LeadID = Popup.LeadID;
                    return PartialView("_LeadReminder", model);
                }
                else if (Popup.Name == "Sent Documents")
                {
                    return PartialView("_TabUploadDocument", Popup.LeadID);
                }
                else if (Popup.Name == "Received Documents")
                {
                    return PartialView("_TabClientDocument", Popup.LeadID);
                }
                else
                {
                    List<VMQueryPreview> content = XiLinkRepository.GetTabContent(Popup, iUserID, OrgID, sDatabase, sOrgName);
                    if (content.Count() > 0 && content != null)
                    {
                        content.ToList().ForEach(m => m.PopType = Popup.PopType);
                        content.ToList().ForEach(m => m.StageID = Popup.StageID);
                        VMViewPopup popup = new VMViewPopup();
                        popup.TabID = Popup.TabID;
                        popup.ClassID = Popup.ClassID;
                        content.FirstOrDefault().popup = popup;
                        content.FirstOrDefault().LeadID = Popup.LeadID;
                        return PartialView("_LeadPopupTabContent", content);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //public ActionResult CreateData(List<VMFormData> FormValues, string BOName, int iInstanceID = 0)
        //{
        //    string sDatabase = SessionManager.CoreDatabase;
        //    try
        //    {
        //        VMSaveInlineEdit SaveData = new VMSaveInlineEdit();
        //        SaveData.FormValues = FormValues;
        //        SaveData.iInstanceID = iInstanceID;
        //        int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //        oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //        var Response = BusinessObjectsRepository.CreateFormData(SaveData, sDatabase, OrgID, iUserID, BOName, sOrgName);
        //        return Json(Response, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return Json("Failure", JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult CliamData()
        {
            return View();
        }
        public ActionResult ClaimConvictionData()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult CliamTermData(List<VMFormData> FormValues)
        //{
        //    string sDatabase = SessionManager.CoreDatabase;
        //    try
        //    { 
        //        int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //        oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //        var Response = XiLinkRepository.ClaimTerms(FormValues, sDatabase, iUserID, sOrgName);
        //        return Json(Response, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return Json("Failure", JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public ActionResult CliamConvictionData(List<VMFormData> FormValues)
        //{
        //    string sDatabase = SessionManager.CoreDatabase;
        //    try
        //    {
        //        int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //        oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //        var Response = XiLinkRepository.CliamConvictionData(FormValues, sDatabase, iUserID, sOrgName);
        //        return Json(Response, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return Json("Failure", JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveBO(List<XIIAttribute> Attributes, string sGUID, string sContext, string sBOName, string sSaveType, string sHierarchy = null, List<CNV> oParams = null)
        {
            List<XIIBO> oBOIList = new List<XIIBO>();
            //string str = Singleton.Instance.oParentGUID[sGUID];
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIBO Response = new XIIBO();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var iDataSourceID = 0;
                var iOrgID = SessionManager.OrganizationID;
                XIDXI oXID = new XIDXI();
                oXID.sOrgDatabase = SessionManager.CoreDatabase;
                if (iOrgID > 0)
                {
                    var oCR = oXID.Get_OrgDefinition("", iOrgID.ToString());
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        var oOrgD = (XIDOrganisation)oCR.oResult;
                        if (oOrgD.bNannoApp)
                        {
                            var sDatabaseName = oOrgD.DatabaseName;
                            oCR = oXID.Get_DataSourceDefinition(sDatabaseName);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                var DataSource = (XIDataSource)oCR.oResult;
                                iDataSourceID = DataSource.ID;
                            }
                        }
                    }
                }
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers();
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName);
                if (iDataSourceID > 0)
                {
                    oBOD = (XIDBO)oBOD.Clone(oBOD);
                    oBOD.iDataSource = iDataSourceID;
                }
                oBOI.BOD = oBOD;
                oBOI.sHierarchy = sHierarchy;
                List<XIIBO> oResponse = new List<XIIBO>();
                Dictionary<string, XIIAttribute> oAttrs = new Dictionary<string, XIIAttribute>(StringComparer.CurrentCultureIgnoreCase);
                var sReference = SessionManager.sReference;
                if (!string.IsNullOrEmpty(sReference) && sReference.ToLower() == "xiguid")
                {
                    foreach (var attrI in Attributes)
                    {
                        var Attr = oBOD.Attributes.Values.Where(m => m.XIGUID.ToString() == attrI.sName).FirstOrDefault();
                        if (Attr != null && !string.IsNullOrEmpty(Attr.Name))
                        {
                            attrI.sName = Attr.Name;
                            //oAttrs = Enumerable.Range(0, 0).ToDictionary(i => Attr.Name, attrI , StringComparer.CurrentCultureIgnoreCase);
                            oAttrs.Add(Attr.Name, attrI);
                        }
                        else
                        {
                            oAttrs.Add(attrI.sName, attrI);
                        }
                    }
                }
                else
                {
                    oAttrs = Attributes.ToDictionary(x => x.sName.ToLower(), x => x, StringComparer.CurrentCultureIgnoreCase);

                }
                XIIXI oXI = new XIIXI();
                if (Attributes.Any(x => x.sName.ToLower() != "shierarchy") && oBOD.Attributes.Any(x => x.Key.ToLower() == "shierarchy"))
                {
                    if (Attributes.Any(x => x.sName.ToLower() == "id"))
                    {
                        var oRefBOI = oXI.BOI(sBOName, Attributes.Where(x => x.sName.ToLower() == "id").Select(x => x.sValue).FirstOrDefault());
                        if (oRefBOI != null && oRefBOI.Attributes.Count() > 0)
                        {

                        }
                        if (oRefBOI != null && oRefBOI.Attributes.Count() > 0 && string.IsNullOrEmpty(Attributes.Where(x => x.sName.ToLower() == "shierarchy").Select(t => t.sValue).FirstOrDefault()) && oRefBOI.AttributeI("iParentID").sValue == "0")
                        {
                            oAttrs.Where(x => x.Key.ToLower() == "shierarchy").ToList().ForEach(m => m.Value.sValue = Attributes.Where(x => x.sName.ToLower() == "id").Select(t => t.sValue).FirstOrDefault());
                        }
                        else if (oRefBOI != null && oRefBOI.Attributes.Count() > 0 && string.IsNullOrEmpty(Attributes.Where(x => x.sName.ToLower() == "shierarchy").Select(t => t.sValue).FirstOrDefault()) && oRefBOI.AttributeI("iParentID").sValue != "0")
                        {
                            oRefBOI = oXI.BOI(sBOName, Attributes.Where(x => x.sName.ToLower() == "iparentid").Select(x => x.sValue).FirstOrDefault());
                            oAttrs.Where(x => x.Key.ToLower() == "shierarchy").ToList().ForEach(m => m.Value.sValue = oRefBOI.AttributeI("sHierarchy").sValue + "_" + Attributes.Where(x => x.sName.ToLower() == "id").Select(t => t.sValue).FirstOrDefault());
                        }
                        else
                        {
                            oAttrs.Where(x => x.Key.ToLower() == "shierarchy").ToList().ForEach(m => m.Value.sValue = Attributes.Any(x => x.sName.ToLower() == "shierarchy" && x.sValue.EndsWith("_")) ?
                              Attributes.Where(x => x.sName.ToLower() == "shierarchy").Select(t => t.sValue).FirstOrDefault() + Attributes.Where(x => x.sName.ToLower() == "id").Select(t => t.sValue).FirstOrDefault() :
                              Attributes.Where(x => x.sName.ToLower() == "shierarchy").Select(t => t.sValue).FirstOrDefault());
                        }
                    }
                    else
                    {
                        var oRefBOI = oXI.BOI(sBOName, Attributes.Where(x => x.sName.ToLower() == "iparentid").Select(x => x.sValue).FirstOrDefault());
                        oAttrs.Add("sHierarchy", new XIIAttribute() { sName = "sHierarchy", sValue = oRefBOI.AttributeI("sHierarchy").sValue + "_", bDirty = true });
                    }
                }
                oBOI.Attributes = oAttrs;
                if (!string.IsNullOrEmpty(sReference) && sReference.ToLower() == "xiguid" && oBOI.Attributes.ContainsKey("XIGUID") && !string.IsNullOrEmpty(oBOI.Attributes["XIGUID"].sValue))
                {
                    XIIXI oXII = new XIIXI();
                    List<CNV> oNV = new List<CNV>();
                    oNV.Add(new CNV { sName = "xiguid", sValue = oBOI.Attributes["XIGUID"].sValue });
                    var oBOII = oXII.BOI(sBOName, "", "Create", oNV);
                    var PkAttrI = oBOII.AttributeI(oBOD.sPrimaryKey);
                    if (!oBOI.Attributes.ContainsKey(oBOD.sPrimaryKey))
                    {
                        oBOI.Attributes.Add(oBOD.sPrimaryKey, PkAttrI);
                    }
                }
                else if (!string.IsNullOrEmpty(sReference) && sReference.ToLower() == "xiguid" && oBOI.Attributes.ContainsKey("XIGUID") && !oBOI.Attributes.ContainsKey(oBOD.sPrimaryKey))
                {
                    var PkAttrD = oBOD.AttributeD(oBOD.sPrimaryKey);
                    if (!oBOI.Attributes.ContainsKey(oBOD.sPrimaryKey))
                    {
                        oBOI.Attributes.Add(oBOD.sPrimaryKey, new XIIAttribute { sName = PkAttrD.Name, sValue = "", bDirty = true });
                    }
                    oBOI.Attributes.Remove("XIGUID");
                }
                string sRole = SessionManager.sRoleName;
                if (!string.IsNullOrEmpty(sRole) && (sRole.ToLower() == EnumRoles.OrgAdmin.ToString().ToLower() || sRole.ToLower() == EnumRoles.OrgIDE.ToString().ToLower()))
                {
                    oBOI.SetAttribute("fkiappid", SessionManager.ApplicationID.ToString());
                    oBOI.SetAttribute("fkiapplicationid", SessionManager.ApplicationID.ToString());
                }              
                var bUpdate = false;
                var iID = oBOI.AttributeI(oBOD.sPrimaryKey).sValue;
                if (!string.IsNullOrEmpty(iID))
                {
                    bUpdate = true;
                }
                string sActiveBO = string.Empty;
                string sActiveFK = string.Empty;
                string sSessionID = HttpContext.Session.SessionID;
                var ISS = oCache.Get_ParamVal(sSessionID, sGUID, sContext, "|XIParent");
                XICacheInstance parentparams = new XICacheInstance();
                if (!string.IsNullOrEmpty(ISS))
                {
                    parentparams = oCache.GetAllParamsUnderGUID(sSessionID, ISS, sContext);
                }
                else
                {
                    parentparams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, sContext);
                }
                sActiveBO = parentparams.NMyInstance.Where(m => m.Key == "{XIP|ActiveBO}").Select(m => m.Value.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sActiveBO) && oBOI.BOD.BOID != 1431)
                {
                    sActiveFK = oBOI.BOD.Attributes.Values.Where(m => m.sFKBOName == sActiveBO).Select(m => m.Name).FirstOrDefault();
                }
                if (!string.IsNullOrEmpty(sActiveFK))
                {
                    var FKValue = parentparams.NMyInstance.Where(m => m.Key.ToLower() == ("{XIP|" + sActiveBO + ".id}").ToLower()).Select(m => m.Value.sValue).FirstOrDefault();
                    if (string.IsNullOrEmpty(FKValue))
                    {
                        FKValue = parentparams.NMyInstance.Where(m => m.Key.ToLower() == ("{XIP|" + sActiveFK + "}").ToLower()).Select(m => m.Value.sValue).FirstOrDefault();
                    }
                    var ColExists = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault();
                    if (ColExists == null)
                    {
                        oBOI.Attributes[sActiveFK.ToLower()] = new XIIAttribute { sName = sActiveFK, sValue = FKValue };
                        oBOI.Attributes[sActiveFK.ToLower()].bDirty = true;
                    }
                    else
                    {
                        oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault().sValue = FKValue;
                        oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault().bDirty = true;
                    }
                }
                foreach (var itesm in parentparams.NMyInstance)
                {
                    if (itesm.Value.sType == "autoset")
                    {
                        var ColExists = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == itesm.Key.ToLower()).FirstOrDefault();
                        if (ColExists == null)
                        {
                            oBOI.Attributes[itesm.Key.ToLower()] = new XIIAttribute { sName = itesm.Key.ToLower(), sValue = itesm.Value.sValue };
                            oBOI.Attributes[itesm.Key.ToLower()].bDirty = true;
                        }
                        else
                        {
                            oBOI.Attributes.Values.Where(m => m.sName.ToLower() == itesm.Key.ToLower()).FirstOrDefault().sValue = itesm.Value.sValue;
                            oBOI.Attributes.Values.Where(m => m.sName.ToLower() == itesm.Key.ToLower()).FirstOrDefault().bDirty = true;
                        }
                    }
                }
                //var Response = oBOI.Save(oBOI);

                if (oBOI != null && oBOI.Attributes != null && oBOI.Attributes.Count() > 0)
                {
                    if (oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault() != null && oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault() != "0")
                    {
                        if (oUser != null)
                        {
                            var sUserName = oUser.sFirstName + " " + oUser.sLastName;
                            if (oBOI.Attributes.ContainsKey(XIConstant.Key_XIUpdtdBy.ToLower()))
                            {
                                oBOI.Attributes[XIConstant.Key_XIUpdtdBy.ToLower()].sValue = sUserName;
                            }
                        }
                    }
                }
                CResult oResult = new CResult();
                if (sSaveType == "grid" && oBOI.BOD.Name.ToLower() == "documents_t")
                {
                    if (oBOI.Attributes.ContainsKey("sfullpath"))
                    {
                        var docIds = oBOI.Attributes["sfullpath"].sValue;
                        //if (docIds.Contains(','))
                        //{
                        var docs = docIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (var docid in docs)
                        {
                            if (oBOI.Attributes.ContainsKey("id"))
                            {
                                oBOI.Attributes["id"].sValue = docid;
                                oBOI.Attributes["id"].bDirty = true;
                                oBOI.Attributes["sfullpath"].bDirty = false;
                                if (oBOI.Attributes.ContainsKey("isystemgeneratedoruploaded"))
                                {
                                    oBOI.Attributes["isystemgeneratedoruploaded"].sValue = "10";
                                    oBOI.Attributes["isystemgeneratedoruploaded"].bDirty = false;
                                }
                                else
                                {
                                    oBOI.Attributes.Add("isystemgeneratedoruploaded", new XIIAttribute { sName = "isystemgeneratedoruploaded", sValue = "10" });
                                    oBOI.Attributes["isystemgeneratedoruploaded"].bDirty = false;
                                }
                            }
                            else
                            {
                                oBOI.Attributes.Add("id", new XIIAttribute { sName = "id", sValue = docid });
                                oBOI.Attributes["id"].bDirty = true;
                                oBOI.Attributes["sfullpath"].bDirty = false;
                                if (oBOI.Attributes.ContainsKey("isystemgeneratedoruploaded"))
                                {
                                    oBOI.Attributes["isystemgeneratedoruploaded"].sValue = "10";
                                    oBOI.Attributes["isystemgeneratedoruploaded"].bDirty = false;
                                }
                                else
                                {
                                    oBOI.Attributes.Add("isystemgeneratedoruploaded", new XIIAttribute { sName = "isystemgeneratedoruploaded", sValue = "10" });
                                    oBOI.Attributes["isystemgeneratedoruploaded"].bDirty = false;
                                }
                            }
                            if (oBOI.Attributes.ContainsKey("fkiboid"))
                            {
                                oBOI.Attributes["fkiboid"].sValue = "17";
                                oBOI.Attributes["fkiboid"].bDirty = true;
                            }
                            else
                            {
                                oBOI.Attributes.Add("fkiboid", new XIIAttribute { sName = "fkiboid", sValue = "17" });
                                oBOI.Attributes["fkiboid"].bDirty = true;
                            }
                            oResult = oBOI.Save(oBOI);
                        }
                        //}
                    }
                }
                else
                {
                    if (sBOName == "XI1Click")
                    {
                        var oBOIs = SaveSimple1Click(oBOI.Attributes.Values.ToList(), sGUID);
                        oResult = oBOIs;
                    }
                    else
                    {
                        oBOI.iUserID = iUserID;
                        oBOI.iRoleID = SessionManager.iRoleID;
                        oResult = oBOI.Save(oBOI);
                    }
                }
                if (oResult.bOK && oResult.oResult != null)
                {
                    Response = (XIIBO)oResult.oResult;
                    Response.oScriptErrors = new Dictionary<string, string>();
                    if (Response.BOD.Scripts.Values.Where(m => m.IsSuccess == false).OrderBy(x => x.iOrder).Count() >= 0)
                    {
                        foreach (var script in Response.BOD.Scripts.Values.OrderBy(x => x.iOrder))
                        {
                            if (!script.IsSuccess)
                            {
                                string sFieldName = script.sName;
                                //string sFieldName = script.sFieldName;
                                foreach (var scriptresult in script.ScriptResults)
                                {
                                    //if (scriptresult.iType == 30)
                                    //{
                                    string sUserError = scriptresult.sUserError;
                                    string sKey = sFieldName;
                                    if (!string.IsNullOrEmpty(sKey) && !Response.oScriptErrors.ContainsKey(sKey))
                                    {
                                        if (!string.IsNullOrEmpty(sReference) && sReference.ToLower() == "xiguid" && Response.BOD.AttributeD(sKey).XIGUID != null)
                                        {
                                            sKey = Response.BOD.AttributeD(sKey).XIGUID.ToString();
                                        }
                                        Response.oScriptErrors[sKey] = sUserError;
                                    }
                                    //}
                                }
                            }
                        }
                    }
                    oBOIList.Add(Response);
                    if (oParams != null && oParams.Count() > 0)
                    {
                        var sUpdateAction = oParams.Where(m => m.sName.ToLower() == "sUpdateAction".ToLower()).Select(m => m.sValue).FirstOrDefault();
                        if (!string.IsNullOrEmpty(sUpdateAction))
                        {
                            var oActionD = (XIDBOAction)oCache.GetObjectFromCache(XIConstant.CacheBOAction, sUpdateAction);
                            var oCR = oActionD.Execute_Action(oBOI);
                        }
                    }
                }
                else if (!oResult.bOK && oResult.oResult != null && oResult.oResult.ToString() == "No Access")
                {
                    oBOI.sErrorMessage = "No Access";
                    oBOIList.Add(oBOI);
                }
                else
                {
                    oBOI.sErrorMessage = "Failure";
                    if (!oResult.bOK && oResult.oResult != null)
                    {
                        Response = (XIIBO)oResult.oResult;
                        Response.oScriptErrors = new Dictionary<string, string>();
                        if (Response.BOD.Scripts.Values.Where(m => m.IsSuccess == false).OrderBy(x => x.iOrder).Count() >= 0)
                        {
                            foreach (var script in Response.BOD.Scripts.Values.OrderBy(x => x.iOrder))
                            {
                                if (!script.IsSuccess)
                                {
                                    string sFieldName = script.sName;
                                    //string sFieldName = script.sFieldName;
                                    foreach (var scriptresult in script.ScriptResults)
                                    {
                                        //if (scriptresult.iType == 30)
                                        //{
                                        string sUserError = scriptresult.sUserError;
                                        string sKey = sFieldName;
                                        if (!string.IsNullOrEmpty(sKey) && !Response.oScriptErrors.ContainsKey(sKey))
                                        {
                                            if (!string.IsNullOrEmpty(sReference) && sReference.ToLower() == "xiguid" && Response.BOD.AttributeD(sKey).XIGUID != null)
                                            {
                                                sKey = Response.BOD.AttributeD(sKey).XIGUID.ToString();
                                            }
                                            Response.oScriptErrors[sKey] = sUserError;
                                        }
                                        //}
                                    }
                                }
                            }
                        }
                        oBOIList.Add(Response);
                    }
                    else
                    {
                        oBOIList.Add(oBOI);
                    }
                }
                if (oBOI.BOD.Name.ToLower() == "driver_t")
                {
                    string sPrimaryKey = oBOI.BOD.sPrimaryKey;
                    var iDriverID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}", "Driver_T", null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|Driver_T.id}", iDriverID, null, null);
                }

                else if (oBOI.BOD.Name.ToLower() == "ACReconcilliation_T".ToLower()) // NOTE: Setting Primarykey ID in Cache this is Reconcilliations Form
                {
                    string iPKID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ACReconcilliation_T.id}", iPKID, null, new List<CNV>());
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|refAccountCategory}", Attributes.Where(m => m.sName.ToLower() == "refAccountCategory".ToLower()).Select(m => m.sValue).FirstOrDefault(), null, new List<CNV>());
                }
                else if (sBOName == "BO Data Insertion")
                {
                    DummyDataInsertiononBO(Attributes, sGUID, sBOName);
                }
                //string svalue = oCache.Get_ParamVal(sSessionID, sGUID, null, "ACReconcilliationID");

                foreach (var oInstance in oBOIList)
                {
                    XIIBO oBO = new XIIBO();
                    oBO.BOD = new XIDBO();
                    //oBO.BOD.Scripts = oInstance.BOD.Scripts;
                    oBO.sBOName = oInstance.BOD.Name;
                    oBO.BOD.sPrimaryKey = oInstance.BOD.sPrimaryKey;
                    var sPKValue = oInstance.Attributes.Where(x => x.Key.ToLower() == oInstance.BOD.sPrimaryKey.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                    oBO.iInstanceID = Convert.ToInt32(sPKValue);
                    //oBO.Attributes.Where(x => x.Key.ToLower() == oInstance.BOD.sPrimaryKey.ToLower()).ToList().ForEach(m => m.Value.sValue = sPKValue);
                    if (!string.IsNullOrEmpty(sReference) && sReference.ToLower() == "xiguid")
                    {
                        XIIXI oXII = new XIIXI();
                        var oBOII = oXII.BOI(oInstance.BOD.Name, oBO.iInstanceID.ToString(), "Create");
                        var sGUIDValue = oBOII.Attributes.Where(x => x.Key.ToLower() == "xiguid").Select(x => x.Value.sValue).FirstOrDefault();
                        oBO.BOD.XIGUID = oInstance.BOD.XIGUID;
                        //oBO.XIGUID = sGUIDValue;
                        oBO.XIGUID = sGUIDValue;
                        oBO.BOD.sPrimaryKey = oInstance.BOD.Attributes["xiguid"].XIGUID.ToString();
                    }
                    else
                    {
                        oBO.BOD.BOID = oInstance.BOD.BOID;
                    }
                    oBO.sErrorMessage = oInstance.sErrorMessage;
                    oBO.oScriptErrors = oInstance.oScriptErrors;
                    oResponse.Add(oBO);
                }
                var bISFileProcess = oBOI.BOD.Attributes.Values.Where(m => m.bIsFileProcess).Select(m => m.bIsFileProcess).FirstOrDefault();
                if (bISFileProcess)
                {
                    FileProcess(oBOI);
                }
                if (!string.IsNullOrEmpty(sSaveType) && sSaveType.ToLower() == "nanno")
                {
                    var iUserOrg = SessionManager.iUserOrg;
                    var oNanno = Save_NannoFeed(iUserOrg, oResponse.FirstOrDefault().iInstanceID, oResponse.FirstOrDefault().BOD.BOID, sGUID, bUpdate);
                }
                //var Responsea = oXiAPI.SaveFormData(Attributes, sGUID, sContext, sDatabase, iUserID, sOrgName);
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        public CResult Save_NannoFeed(int iOrg, int iBOIID, int iBODID, string sGUID, bool bUpdate)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";//expalin about this method logic
            try
            {
                XIIXI oXI = new XIIXI();
                var sSessionID = HttpContext.Session.SessionID;
                XIInfraCache oCache = new XIInfraCache();
                var Params = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                var Group = Params.NMyInstance["ListClickparamname"].nSubParams;
                var iGroupID = Group.Where(m => m.sName == "{XIP|iNannoGroupID}").Select(m => m.sValue).FirstOrDefault();
                var oSrcOrgI = oXI.BOI("Organisations", iOrg.ToString());
                var oXNMBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XNannoMessage");
                var sOrgs = string.Empty;
                List<string> OrgIDs = new List<string>();
                NotifyHub oHub = new NotifyHub();
                XID1Click o1Click = new XID1Click();
                o1Click.BOID = 1346;
                o1Click.Query = "select * from xorggroup_t where fkinannogroupid=" + iGroupID;
                var Result = o1Click.OneClick_Run();
                if (bUpdate)
                {
                    List<CNV> Whrprms = new List<CNV>();
                    Whrprms.Add(new CNV { sName = "iBODID", sValue = iBODID.ToString() });
                    Whrprms.Add(new CNV { sName = "iBOIID", sValue = iBOIID.ToString() });
                    var oMesBOI = new XIIBO();
                    oMesBOI = oXI.BOI("Nanno Message", null, null, Whrprms);
                    var sFeedMessge = oMesBOI.AttributeI("sMessage").sValue;
                    var Time = oMesBOI.AttributeI("XIUpdatedWhen").sResolvedValue;
                    var sMesID = oMesBOI.AttributeI("ID").sValue;
                    var iOrgObjectTypeID = oMesBOI.AttributeI("FKiOrgObjectTypeID").sValue;
                    var OrgObjectType = oXI.BOI("OrgObjectType", iOrgObjectTypeID);
                    var iWidgetID = OrgObjectType.AttributeI("FKiWidgetID").sValue;
                    var oWidget = (XIDWidget)oCache.GetObjectFromCache(XIConstant.CacheWidget, null, iWidgetID);
                    foreach (var BOI in Result.Values.ToList())
                    {
                        var iOrgID = BOI.AttributeI("fkiorgid").sValue;
                        if (iOrgID != iOrg.ToString())
                        {
                            OrgIDs.Add(iOrgID);
                        }
                    }
                    sOrgs = string.Join(",", OrgIDs);
                    oHub.UpdateFeed(sFeedMessge, Time, sMesID, oWidget.FKiLayoutID, iBODID + "-" + iBOIID, sOrgs, iOrg, "SignalR", "", bUpdate);
                }
                else
                {
                    var iOrgObjectTypeID = Params.NMyInstance["iOrgObjectTypeID"].sValue;
                    var sFeedMessge = Params.NMyInstance["sFeedMessage"].sValue;
                    var OrgObjectType = oXI.BOI("OrgObjectType", iOrgObjectTypeID);
                    var iWidgetID = OrgObjectType.AttributeI("FKiWidgetID").sValue;
                    var oWidget = (XIDWidget)oCache.GetObjectFromCache(XIConstant.CacheWidget, null, iWidgetID);
                    var sMesID = string.Empty;

                    var oMesBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Nanno Message");
                    var oMesBOI = new XIIBO();
                    oMesBOI.BOD = oMesBOD;
                    oMesBOI.SetAttribute("iboiid", iBOIID.ToString());
                    oMesBOI.SetAttribute("ibodid", iBODID.ToString());
                    oMesBOI.SetAttribute("FKiOrgObjectTypeID", iOrgObjectTypeID);
                    oMesBOI.SetAttribute("sMessage", sFeedMessge);
                    oCR = oMesBOI.SaveV2(oMesBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oMesBOI = (XIIBO)oCR.oResult;
                        var iMessageID = oMesBOI.AttributeI(oMesBOD.sPrimaryKey).sValue;
                        oMesBOI = oXI.BOI("Nanno Message", iMessageID);
                        var Time = oMesBOI.AttributeI(XIConstant.Key_XICrtdWhn).sResolvedValue;
                        foreach (var BOI in Result.Values.ToList())
                        {
                            var iOrgID = BOI.AttributeI("fkiorgid").sValue;
                            if (iOrgID != iOrg.ToString())
                            {
                                var oTrgtOrgI = oXI.BOI("Organisations", iOrgID.ToString());
                                OrgIDs.Add(iOrgID);
                                var oXMesBOI = new XIIBO();
                                oXMesBOI.BOD = oXNMBOD;
                                oXMesBOI.SetAttribute("fkiorgidto", iOrgID.ToString());
                                oXMesBOI.SetAttribute("fkiorgidtoxiguid", oTrgtOrgI.AttributeI("xiguid").sValue);
                                oXMesBOI.SetAttribute("fkiorgidfrom", iOrg.ToString());
                                oXMesBOI.SetAttribute("fkiorgidfromxiguid", oSrcOrgI.AttributeI("xiguid").sValue);
                                oXMesBOI.SetAttribute("fkinannogroupid", iGroupID.ToString());
                                oXMesBOI.SetAttribute("fkinannomessageid", iMessageID.ToString());
                                oCR = oXMesBOI.SaveV2(oXMesBOI);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oXMesBOI = (XIIBO)oCR.oResult;
                                    sMesID = oXMesBOI.AttributeI("ID").sValue;
                                    sOrgs = string.Join(",", OrgIDs);
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    oCResult.oResult = "Success";
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        if (oTrace.iStatus == (int)xiEnumSystem.xiFuncResult.xiSuccess)
                        {
                            oHub.UpdateFeed(sFeedMessge, Time, sMesID, oWidget.FKiLayoutID, iBODID + "-" + iBOIID, sOrgs, iOrg, "SignalR", "", bUpdate);
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        //Dummy data Insertion 
        public void DummyDataInsertiononBO(List<XIIAttribute> Attributes, string sGUID, string sBOName)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            XIGraphData oXIGD = new XIGraphData();
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = oCResult.Get_MethodName();

            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;

            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
            //if iTraceLevel>0 then 
            //oCResult.oTraceStack.Trace("Stage", "Begin query build",milliseconds)
            //oCResult.oTraceStack.Trace("Stage",sError)
            //end if

            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
                //oCResult.oTraceStack.Trace("Stage",sError)
            }
            try
            {
                var boid = Attributes[0].sValue;
                var stringvalue = Attributes[1].sValue;
                var NewInsertionfields = Attributes[2].sValue;
                string BracketInsideValues = stringvalue.Split('(', ')')[1];
                string firstOutBrackets = stringvalue.Split('(', ')')[0];
                var sListoffirstOutBrackets = String.Join("-", firstOutBrackets);
                var sDateRange = firstOutBrackets.Split('-').ToList();
                var date = sDateRange[1].Split('_').ToList();
                var fromDate = date[0];
                var ToDate = date[1];
                var listofBracketInsideValues = BracketInsideValues.Split(',').ToList();
                var iStatusSpliting = listofBracketInsideValues[0].Split('-').ToList();
                //var iStatusSpliting1 = listofBracketInsideValues[1].Split('-').ToList();
                var ListofNewInsertionfields = NewInsertionfields.Split(',').ToList();
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                XIInfraCache oCache = new XIInfraCache();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, boid);
                var tablename = oBOD.TableName;
                oBOI.BOD = oBOD;
                var attr = oBOD.Attributes.ToList();
                int iDataSourceID = oBOD.iDataSource;
                //var oBODataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, oBOD.iDataSource.ToString());
                //string sConnectionString = oBODataSource.sConnectionString;
                XIDXI oXID = new XIDXI();
                string sConnectionString = oXID.GetBODataSource(iDataSourceID, oBOD.FKiApplicationID);
                //XIDataSource oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, iDataSourceID.ToString());
                List<BOFields> BOFields = new List<Models.BOFields>();
                using (var connection = new SqlConnection(sConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(@"SELECT table_name, column_name as 'Column Name', data_type as 'Data Type', character_maximum_length as 'Max Length' FROM information_schema.columns WHERE table_name LIKE '" + tablename + "'", connection);
                    SqlDataReader reader = command.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    BOFields = dt.AsEnumerable().Select(ColFields =>
                    new BOFields
                    {
                        TableName = ColFields.Field<string>("table_name"),
                        Name = ColFields.Field<string>("Column Name"),
                        LabelName = ColFields.Field<string>("Column Name"),
                        DataType = ColFields.Field<string>("Data Type"),
                        FieldMaxLength = ColFields.Field<string>("Data Type") == "int" ? "0" : (ColFields.Field<string>("Data Type") == "smalldatetime" ? "0" : (ColFields.Field<string>("Data Type") == "decimal" ? "0" : (ColFields.Field<string>("Data Type") == "tinyint" ? "0" : (ColFields.Field<string>("Data Type") == "nvarchar" ? "max" : (ColFields.Field<string>("Data Type") == "bigint" ? "0" : (ColFields.Field<string>("Data Type") == "datetime" ? "0" : (ColFields.Field<string>("Data Type") == "bit" ? "0" : (ColFields.Field<string>("Data Type") == "float" ? "0" : ColFields.Field<int?>("Max Length").ToString())))))))),
                    }).ToList();
                    connection.Close();
                }
                if (BOFields.Count > 0)
                {

                    var sFindDate = BOFields.Where(k => k.Name.ToLower() == sDateRange[0].ToLower()).FirstOrDefault();
                    // var sFindDate = sDateRange.Where(x => x == item.Name).FirstOrDefault();
                    if (sFindDate != null)
                    {
                        foreach (var ista in listofBracketInsideValues)
                        {
                            var sStatusValue = ista.Trim().Split('-').ToList();
                            var splitingistatus = BOFields.Find(n => n.Name.ToLower() == sStatusValue[0].ToLower());
                            if (splitingistatus != null)
                            {
                                int iFindsvalue = 0;
                                string sQuery = "select sValues from XIBOOptionList_T_N where BOID=" + Convert.ToInt32(boid) + " and sOptionName='" + sStatusValue[1] + "'";
                                XID1Click oXI1Click = new XID1Click();
                                oXI1Click.Query = sQuery;
                                oXI1Click.Name = "XIBOOptionList";
                                //oXI1Click.DataSource = 10;
                                var Result = oXI1Click.GetList();
                                List<XIIBO> oDocumentsList = new List<XIIBO>();
                                if (Result.bOK && Result.oResult != null)
                                {
                                    oDocumentsList = ((Dictionary<string, XIIBO>)Result.oResult).Values.ToList();
                                    iFindsvalue = Convert.ToInt32(oDocumentsList.Select(s => s.Attributes.Select(t => t.Value.sValue).FirstOrDefault()).FirstOrDefault());

                                    var sColName = "";
                                    var sColValue = "";
                                    foreach (var field in ListofNewInsertionfields)
                                    {
                                        var sIdentifyName = field.Split('(', ')')[0].Trim();
                                        var sField = BOFields.Find(n => n.Name.ToLower() == sIdentifyName.ToLower());
                                        if (sField != null)
                                        {
                                            sColName += field.Split('(', ')')[0] + ',';
                                            sColValue += (field.Split('(', ')')[1]) + ',';
                                        }
                                    }
                                    sColName = sColName.TrimEnd(',');
                                    sColValue = sColValue.TrimEnd(',');
                                    int DaysCount = (Convert.ToDateTime(ToDate) - Convert.ToDateTime(fromDate)).Days;
                                    var values = "";
                                    Random rnd = new Random();
                                    Random randomTest = new Random();
                                    var count = Convert.ToInt32(sStatusValue[2]);
                                    for (int i = 0; i < count; i++)
                                    {
                                        var listofcolvalue = sColValue.Split(',').ToList();
                                        var RandomValues = "";
                                        foreach (var itr in listofcolvalue)
                                        {
                                            var abc = itr.Split('/').ToList();
                                            RandomValues += "'" + abc[rnd.Next(abc.Count)] + "',";
                                        }
                                        RandomValues = RandomValues.TrimEnd(',');
                                        var daterange = Convert.ToDateTime(fromDate).AddDays(randomTest.Next(DaysCount));
                                        var RandomDates = daterange.ToString(XIConstant.SqlDateFormat);
                                        values += "(" + RandomValues + ", " + iFindsvalue + " , '" + RandomDates + "'),";
                                    }
                                    values = values.TrimEnd(',');
                                    string insertionQuery = "insert into " + tablename + "(" + sColName + ", " + sStatusValue[0] + ", " + sFindDate.Name + ") values " + values + "";
                                    //string insertionQuery = "insert into " + tablename + "(sName, " + sStatusValue[0] + ", " + sFindDate + ") values('Zee', " + findsvalues + ", '" + fromDate + "')";
                                    using (SqlConnection Con = new SqlConnection(sConnectionString))
                                    {
                                        SqlCommand cmd = new SqlCommand();
                                        cmd.Connection = Con;
                                        cmd.CommandText = insertionQuery;
                                        Con.Open();
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string sDatabase = SessionManager.CoreDatabase;
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
            }
        }
        private void FileProcess(XIIBO oBOI)
        {
            WalletFileHandler oFile = new WalletFileHandler();
            oFile.ProcessFile(oBOI);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveGrid(List<XIIAttribute> Attributes, string sGUID, string sContext, string sBOName, List<XIIBO> oBOIInstance)
        {
            int iUserID = 0; string sDatabase = SessionManager.CoreDatabase;
            if (SessionManager.UserID > 0)
            {
                iUserID = SessionManager.UserID;
            }
            try
            {
                List<XIIAttribute> oAttributes = new List<XIIAttribute>();
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName);
                oBOI.BOD = oBOD;
                var attr = Attributes.FirstOrDefault().sName.Split('_')[0];
                var oRowData = Attributes.Where(x => x.sName.StartsWith(attr + "_")).ToList();
                XIDefinitionBase oDefBase = new XIDefinitionBase();
                oAttributes = (List<XIIAttribute>)(oDefBase.Clone(oRowData));
                foreach (var item in oAttributes)
                {
                    item.sName = item.sName.Replace(attr + "_", "");
                }
                //Check For Empty row
                bool bIsSave = true;
                var Creatable = oAttributes.Where(s => !string.IsNullOrEmpty(s.sValue)).ToList();
                if (Creatable != null && Creatable.Count() > 0)
                {

                }
                else
                {
                    bIsSave = false;
                }

                oBOI.Attributes = oAttributes.ToDictionary(x => x.sName.ToLower(), x => x);
                string sActiveBO = string.Empty;
                string sActiveFK = string.Empty;
                string sSessionID = HttpContext.Session.SessionID;
                var ISS = oCache.Get_ParamVal(sSessionID, sGUID, sContext, "|XIParent");
                XICacheInstance parentparams = new XICacheInstance();
                if (!string.IsNullOrEmpty(ISS))
                {
                    parentparams = oCache.GetAllParamsUnderGUID(sSessionID, ISS, sContext);
                }
                else
                {
                    parentparams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, sContext);
                }
                sActiveBO = parentparams.NMyInstance.Where(m => m.Key == "{XIP|ActiveBO}").Select(m => m.Value.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sActiveBO))
                {
                    sActiveFK = oBOI.BOD.Attributes.Values.Where(m => m.sFKBOName == sActiveBO).Select(m => m.Name).FirstOrDefault();
                }
                if (!string.IsNullOrEmpty(sActiveFK))
                {
                    var FKValue = parentparams.NMyInstance.Where(m => m.Key == "{XIP|" + sActiveBO + ".id}").Select(m => m.Value.sValue).FirstOrDefault();
                    var ColExists = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault();
                    if (ColExists == null)
                    {
                        oBOI.Attributes[sActiveFK.ToLower()] = new XIIAttribute { sName = sActiveFK, sValue = FKValue, sPreviousValue = FKValue };
                        oBOI.Attributes[sActiveFK.ToLower()].bDirty = true;
                    }
                    else
                    {
                        oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault().sValue = FKValue;
                        oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault().bDirty = true;
                    }
                }
                var IDEParams = oCache.Get_Paramobject(sSessionID, sGUID, null, "IDEParams");
                if (IDEParams != null && IDEParams.nSubParams != null && IDEParams.nSubParams.Count() > 0)
                {
                    var sParentBO = IDEParams.nSubParams.Where(m => m.sName.ToLower() == XIConstant.Param_ParentBO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(sParentBO))
                    {
                        int iParentBOIID = 0;
                        var sParentBOIID = IDEParams.nSubParams.Where(m => m.sName.ToLower() == XIConstant.Param_ParentBOIID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                        int.TryParse(sParentBOIID, out iParentBOIID);
                        if (iParentBOIID > 0)
                        {
                            sActiveFK = oBOI.BOD.Attributes.Values.Where(m => m.sFKBOName == sParentBO).Select(m => m.Name).FirstOrDefault();
                            if (!string.IsNullOrEmpty(sActiveFK))
                            {
                                var ColExists = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault();
                                if (ColExists == null)
                                {
                                    oBOI.Attributes[sActiveFK.ToLower()] = new XIIAttribute { sName = sActiveFK, sValue = sParentBOIID, sPreviousValue = sParentBOIID };
                                    oBOI.Attributes[sActiveFK.ToLower()].bDirty = true;
                                }
                                else
                                {
                                    oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault().sValue = sParentBOIID;
                                    oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sActiveFK.ToLower()).FirstOrDefault().bDirty = true;
                                }
                            }
                        }
                    }
                }
                foreach (var itesm in parentparams.NMyInstance)
                {
                    if (itesm.Value.sType == "autoset")
                    {
                        var ColExists = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == itesm.Key.ToLower()).FirstOrDefault();
                        if (ColExists == null)
                        {
                            oBOI.Attributes[itesm.Key.ToLower()] = new XIIAttribute { sName = itesm.Key.ToLower(), sValue = itesm.Value.sValue, sPreviousValue = itesm.Value.sValue };
                            oBOI.Attributes[itesm.Key.ToLower()].bDirty = true;
                        }
                        else
                        {
                            oBOI.Attributes.Values.Where(m => m.sName.ToLower() == itesm.Key.ToLower()).FirstOrDefault().sValue = itesm.Value.sValue;
                            oBOI.Attributes.Values.Where(m => m.sName.ToLower() == itesm.Key.ToLower()).FirstOrDefault().bDirty = true;
                        }
                    }
                }
                var transtype = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|sTranstype}");
                if (!string.IsNullOrEmpty(transtype) && transtype.ToLower() == "renewal" && sBOName == "Aggregations" && oBOI.Attributes.ContainsKey("fkiqsinstanceid"))
                {
                    var ParentQSID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iRenewalQSInstanceID}");
                    if (!string.IsNullOrEmpty(ParentQSID))
                    {
                        oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "FKiQSInstanceID".ToLower()).FirstOrDefault().sValue = ParentQSID;
                    }
                }
                var oRes = oBOI;
                if (bIsSave)
                {
                    if (oBOIInstance == null)
                    {
                        oBOIInstance = new List<XIIBO>();
                    }

                    bool bIsModified = false;
                    foreach (var item in oBOI.Attributes)
                    {
                        if (item.Value.sValue != item.Value.sPreviousValue)
                        {
                            bIsModified = true;
                        }
                    }
                    if (bIsModified == true)
                    {
                        if (oBOI.Attributes.ContainsKey("StatusScript".ToLower()))
                        {
                            if (!string.IsNullOrEmpty(oBOI.Attributes["StatusScript".ToLower()].sValue))
                            {
                                string sScript = oBOI.Attributes["StatusScript".ToLower()].sValue;
                                if (!string.IsNullOrEmpty(sScript))
                                {
                                    //string sGUID = Guid.NewGuid().ToString();
                                    string sReturnValue = string.Empty;
                                    CResult oCR = new CResult();
                                    XIDScript oXIScript = new XIDScript();
                                    oXIScript.sScript = sScript.ToString();
                                    oCR = oXIScript.Execute_Script(sGUID, sSessionID);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        sReturnValue = (string)oCR.oResult;
                                    }
                                }
                            }
                        }
                        if (sBOName.ToLower() == "requirement_t")
                        {
                            oBOI.AttributeI("idueindays").sValue = oBOI.AttributeI("idueindays").sValue.ToLower() == "overdue" ? oBOI.AttributeI("idueindays").sValue = "28" : oBOI.AttributeI("idueindays").sValue;
                        }
                        var oResult = oBOI.Save(oBOI);
                        if (oResult.bOK && oResult.oResult != null)
                        {
                            oRes.oScriptErrors = new Dictionary<string, string>();
                            oRes = (XIIBO)oResult.oResult;
                            if (oRes.BOD.Scripts.Values.Where(m => m.IsSuccess == false).Count() >= 0)
                            {
                                foreach (var script in oRes.BOD.Scripts.Values)
                                {
                                    if (!script.IsSuccess)
                                    {
                                        string sFieldName = script.sFieldName;
                                        foreach (var scriptresult in script.ScriptResults)
                                        {
                                            //if (scriptresult.iType == 30)
                                            //{
                                            string sUserError = scriptresult.sUserError;
                                            string sKey = attr + "_" + sFieldName;
                                            oRes.oScriptErrors[sKey] = sUserError;
                                            //}
                                        }
                                    }
                                }
                            }
                            oBOIInstance.Add(oRes);
                        }
                        else
                        {
                            oBOI.sErrorMessage = "Failure";
                            oBOIInstance.Add(oBOI);
                        }
                    }
                }
                //var Response = oBOI.Save(oBOI);
                Attributes.RemoveAll(t => t.sName.StartsWith(attr + "_"));
                if (Attributes != null && Attributes.Count() > 0)
                {
                    SaveGrid(Attributes, sGUID, sContext, sBOName, oBOIInstance);
                }
                //var Response = oXiAPI.SaveFormData(oBOInstance, sGUID, sContext, sDatabase, iUserID, sOrgName);
                List<XIIBO> oResponse = new List<XIIBO>();
                foreach (var oInstance in oBOIInstance)
                {
                    XIIBO oBO = new XIIBO();
                    oBO.BOD = new XIDBO();
                    // oBO.BOD.Scripts = oInstance.BOD.Scripts;
                    oBO.sBOName = oInstance.BOD.Name;
                    oBO.BOD.sPrimaryKey = oInstance.BOD.sPrimaryKey;
                    var sPKValue = oInstance.Attributes.Where(x => x.Key.ToLower() == oInstance.BOD.sPrimaryKey.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                    oBO.iInstanceID = Convert.ToInt32(sPKValue);
                    //oBO.Attributes.Where(x => x.Key.ToLower() == oInstance.BOD.sPrimaryKey.ToLower()).ToList().ForEach(m => m.Value.sValue = sPKValue);
                    oBO.BOD.BOID = oInstance.BOD.BOID;
                    oBO.sErrorMessage = oInstance.sErrorMessage;
                    oBO.oScriptErrors = oInstance.oScriptErrors;
                    oResponse.Add(oBO);
                }
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Popup

        #region UserDialogs

        [HttpPost]
        public ActionResult SaveUserDialog(int QueryID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Result = XiLinkRepository.SaveUserDialog(QueryID, iUserID, OrgID, sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetUserDialogs(int UserID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Dialogs = XiLinkRepository.GetUserDialogs(iUserID, OrgID, sDatabase);
                //Session["IsLogin"] = false;
                return Json(Dialogs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #endregion UserDialogs

        #region EditableGrid

        public VMResultList OneClickResultForEditableGrid()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string ResultIn = "Inline";
                string SearchText = null;
                int PageIndex = 1;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var LoadingType = XiLinkRepository.GetLoadingType(OCID, sDatabase);
                if (LoadingPattern != -1)
                {
                    LoadingType[0] = LoadingPattern;
                }
                if (LoadingType[1] == 1 && sSearchType != "Structured")
                {
                    var Result = XiLinkRepository.GetStructuredOneClicks(OrgID, OCID, sDatabase);
                    if (ResultIn == "Run")
                    {
                        Result.FirstOrDefault().ResultIn = ResultIn;
                    }
                    return null;
                    //return PartialView("_StructuredOneClicksTree", Result);
                }
                else
                {
                    if (LoadingType[0] == 1)
                    {
                        var Report = XiLinkRepository.GetHeadings(OCID, sSearchType, sDatabase, OrgID, iUserID, sOrgName);
                        Report.ResultListDisplayType = LoadingPattern;
                        Report.ReportID = OCID;
                        Report.QueryID = OCID;
                        Report.PreviewType = ResultIn;
                        Report.SearchText = SearchText;
                        if (sSearchType == "Quick")
                        {
                            Report.SearchType = sSearchType;
                        }
                        else if (sSearchType == "Structured")
                        {
                            Report.PreviewType = "Structured";
                        }
                        Report.ResultListDisplayType = LoadingType[0];
                        //else
                        //{
                        //    Report.SearchType = SearchType;
                        //}
                        //Report.BO = BO;
                        //return View(Report);
                        return Report;
                    }
                    else
                    {
                        int? LeadID = 0;
                        VMRunUserQuery UserQuery = new VMRunUserQuery();
                        UserQuery.OrgID = OrgID;
                        UserQuery.UserID = iUserID;
                        UserQuery.database = sDatabase;
                        UserQuery.PageIndex = PageIndex;
                        UserQuery.ReportID = OCID;
                        UserQuery.LeadID = 0;
                        UserQuery.ClassFilter = 0;
                        UserQuery.DateFilter = 0;
                        UserQuery.SearchText = SearchText;
                        //UserQuery.BO = BO;
                        UserQuery.SearchType = sSearchType;
                        UserQuery.ResultListDisplayType = LoadingPattern;
                        VMResultList model = XiLinkRepository.RunUserQuery(UserQuery, iUserID, sOrgName, sDatabase);
                        model.ReportID = OCID;
                        var AutoLoadResultLlist = model.Rows;
                        if (sSearchType == "Structured")
                        {
                            model.PreviewType = "Structured";
                        }
                        else
                        {
                            model.PreviewType = ResultIn;
                        }
                        model.QueryID = OCID;
                        model.SearchText = SearchText;
                        //model.BO = BO;
                        model.SearchType = sSearchType;
                        if (PageIndex >= 2)
                        {
                            //return model;
                        }
                        //return View(model);
                        return model;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //public ActionResult GetReportResult(jQueryDataTableParamModel param, int ReportID)
        //{
        //    try
        //    {
        //        int OrgID = OrgID;
        //        int UserID = iUserID
        //        string database = sDatabase;
        //        param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
        //        param.sSortDir = Request["sSortDir_0"].ToString();
        //        VMQuickSearch Search = new VMQuickSearch();
        //        Search.ReportID = ReportID;
        //        Search.UserID = UserID;
        //        Search.OrgID = OrgID;
        //        Search.database = database;
        //        Search.SearchType = SearchType;
        //        Search.Role = Util.GetRoleName();
        //        Search.BO = BO;
        //        var result = XiLinkRepository.GetReportResult(param, Search);
        //        return Json(new
        //        {
        //            sEcho = result.sEcho,
        //            iTotalRecords = result.iTotalRecords,
        //            iTotalDisplayRecords = result.iTotalDisplayRecords,
        //            aaData = result.aaData
        //        },
        //        JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return null;
        //    }
        //}

        #endregion EditableGrid

        #region UploadImage
        //[HttpPost]  

        //public ActionResult SaveFiles(int ID, int BOFieldID, string sInstanceID, List<HttpPostedFileBase> UploadImage)
        //{
        //    string sDatabase = SessionManager.CoreDatabase;
        //    int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //    oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
        //    oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
        //    int OrgID = oUser.FKiOrganisationID;
        //    try
        //    {
        //        VMCustomResponse Docs = new VMCustomResponse();
        //        if (UploadImage != null)
        //        {
        //            Docs = XiLinkRepository.SaveFiles(ID, BOFieldID, OrgID, UploadImage, iUserID, sOrgName, sDatabase, sInstanceID);
        //        }

        //        return Content(Docs.sID.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return Json(0, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult SaveDocx(int ID, int BOFieldID, string sInstanceID, List<HttpPostedFileBase> UploadFile)
        {
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
            oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
            int OrgID = oUser.FKiOrganisationID;
            try
            {
                VMCustomResponse SaveDocs = new VMCustomResponse();
                if (UploadFile != null)
                {
                    SaveDocs = XiLinkRepository.SaveFiles(ID, BOFieldID, OrgID, UploadFile, iUserID, sOrgName, sDatabase, sInstanceID);

                }
                // return Json(true, "text/html");
                return Content(SaveDocs.sID.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                //return Json(false, "text/html");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion UploadImage

        #region Delete attribute image
        public ActionResult DeleteAttrImage(int ImgID, int BOFieldID, int LeadID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iStatus = XiLinkRepository.DeleteAttrImage(sDatabase, ImgID, BOFieldID, LeadID);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(false, "text/html");
            }
        }


        #endregion Delete attribute image

        #region LayoutPanel

        [HttpPost]
        public ActionResult GetCacheParameterValue(string sParamName, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                Dictionary<string, string> oParams = new Dictionary<string, string>();
                var sSessionID = HttpContext.Session.SessionID;
                //sUID = "3";
                var Params = sParamName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in Params)
                {
                    oParams.Add(item, oCache.Get_ParamVal(sSessionID, sGUID, null, item));
                }
                //var sParamVal = oCache.Get_ParamVal(sSessionID, sGUID, null, sParamName);
                if (oParams != null)
                {
                    return Json(oParams, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Value", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion LayoutPanel

        #region ListActions

        public ActionResult ListHover(int ID = 0, int BOID = 0, string ColumnName = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XIIBO oBOI = new XIIBO();
                var Result = oBOI.Get_FKHover(ID, BOID, ColumnName);
                //var Result = oXiAPI.GetListHover(ID, BOID, BOName, ColumnName, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        public ActionResult AttrTrackHover(int iAttrID, int iBODID, int iBOIID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "AttributeTrackHover");
                var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOAttributeTrack");
                o1ClickC.BOD = BOD;
                List<CNV> nParams = new List<CNV>();
                nParams.Add(new CNV { sName = "{XIP|BODID}", sValue = iBODID.ToString() });
                nParams.Add(new CNV { sName = "{XIP|AttrID}", sValue = iAttrID.ToString() });
                nParams.Add(new CNV { sName = "{XIP|iInstanceID}", sValue = iBOIID.ToString() });
                o1ClickC.Get_1ClickHeadings();
                o1ClickC.ReplaceFKExpressions(nParams);
                o1ClickC.OneClick_Execute();
                o1ClickC.BOD = null;
                return Json(o1ClickC.oDataSet.Values.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        #endregion ListActions

        #region QuestionSet

        [AllowAnonymous]
        public XIIQS GetQSInstance(int iQSDID, string sGUID, string sMode, int iBODID, int iBOIID)
        {
            XIIXI oIXI = new XIIXI();
            XIInfraCache xifCache = new XIInfraCache();
            string sDatabase = SessionManager.CoreDatabase;
            string sOrgName = SessionManager.OrganisationName;
            var sSessionID = HttpContext.Session.SessionID;
            int iUserID = 0;
            if (SessionManager.UserID > 0)
            {
                iUserID = SessionManager.UserID;
            }
            string sCurrentGuestUser = string.Empty;
            if (SessionManager.UserUniqueID != null)
            {
                sCurrentGuestUser = SessionManager.UserUniqueID;
            }
            //string FKsQSSourceID = SessionManager.QSSourceID;
            //int FKiQSSourceID = 0;
            //int.TryParse(FKsQSSourceID, out FKiQSSourceID);
            string sExternalRefID = SessionManager.sExternalRefID;
            int iOrgID = SessionManager.OrganizationID;
            //Get Question Set Object
            XIIQS oQSI = new XIIQS();
            XIDQS oQSD = (XIDQS)xifCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDID.ToString(), sSessionID, sGUID, 0, 0);
            XIDQS oQSDC = (XIDQS)oQSD.Clone(oQSD); //.GetCopy();
            //var oQSD = oDXI.Get_QSDefinition(null, iQSDID);
            CResult oCR = oIXI.CreateQSI(null, iQSDID, null, null, iBODID, iInstanceID, sCurrentGuestUser, oQSDC.FKiSourceID, sExternalRefID, oQSDC.FKiOriginID, sGUID);
            var oQSInstance = (XIIQS)oCR.oResult;
            oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}", Convert.ToString(oQSInstance.ID), "autoset", null);
            oQSInstance.QSDefinition = oQSDC;
            //var oQSInstance = XiLinkRepository.GetQuestionSetInstance(iQSDID, sGUID, sMode, iBODID, iInstanceID, iUserID, sOrgName, sDatabase, sCurrentGuestUser);
            if (!string.IsNullOrEmpty(sMode) && iBODID > 0 && iInstanceID > 0)
            {
                oQSInstance.iBOIID = iInstanceID;
                oQSInstance.FKiBODID = iBODID;
            }
            var iActiveStepID = 0;
            if (oQSInstance.Steps == null || oQSInstance.Steps.Count() == 0)
            {
                //Load First Step Instance
                if (!string.IsNullOrEmpty(sStepName))
                {
                    iActiveStepID = oQSInstance.QSDefinition.Steps[sStepName].ID;
                }
                else
                {
                    iActiveStepID = oQSInstance.GetActiveStepID(0, sGUID);
                }
            }
            else
            {
                if (oQSInstance.iCurrentStepID == -1)
                {
                    oQSInstance.iCurrentStepID = oQSInstance.QSDefinition.Steps.Last().Value.ID;
                }
                //Checking Incomplete Question Set
                var StepDefs = oQSInstance.QSDefinition.Steps.ToList();
                var LastStepID = oQSInstance.Steps.Last().Value.FKiQSStepDefinitionID;
                //var RemainingSteps = oQSInstance.QSDefinition.QSSteps.Where(m => m.iOrder > StepDefs.Where(n => n.ID == oQSInstance.iCurrentStepID).FirstOrDefault().iOrder).Select(m => m.ID).ToList().Except(oQSInstance.nStepInstances.Select(m => m.FKiQSStepDefinitionID).ToList()).ToList();
                if (!string.IsNullOrEmpty(sStepName))
                {
                    int iStepID = oQSInstance.QSDefinition.Steps[sStepName].ID;
                    if (iStepID > 0)
                    {
                        iActiveStepID = iStepID;
                    }
                }
                else if (oQSInstance.iCurrentStepID == 0)
                {
                    iActiveStepID = oQSInstance.GetActiveStepID(0, sGUID);
                }
                else if (oQSInstance.iCurrentStepID > 0)
                {
                    var RemainingSteps = oQSInstance.QSDefinition.Steps.Where(m => m.Value.iOrder > StepDefs.Where(n => n.Value.ID == oQSInstance.iCurrentStepID).FirstOrDefault().Value.iOrder).OrderBy(m => m.Value.iOrder).Select(m => m.Value.ID).ToList();
                    if (RemainingSteps != null && RemainingSteps.Count() > 0)
                    {
                        iActiveStepID = oQSInstance.GetActiveStepID(RemainingSteps.FirstOrDefault(), sGUID);
                    }
                    else
                    {
                        iActiveStepID = oQSInstance.GetActiveStepID(oQSInstance.iCurrentStepID, sGUID);
                    }
                }
                else
                {
                    if (oQSInstance.Steps.Count() == 1)
                    {
                        oQSInstance.Steps.FirstOrDefault().Value.bIsCurrentStep = true;
                    }
                    else
                    {
                        oQSInstance.Steps.FirstOrDefault().Value.bIsCurrentStep = true;
                    }
                }
            }
            if (oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).FirstOrDefault().Value.QSLinks != null && oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).FirstOrDefault().Value.QSLinks.Count() > 0)
            {
                foreach (var oQSLink in oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).FirstOrDefault().Value.QSLinks.Values)
                {
                    foreach (var oLink in oQSLink.XiLink.OrderBy(m => m.Value.rOrder))
                    {
                        if (oLink.Value.sType == "Pre")
                        {
                            StartAction = null;
                            GetXiLinkContent(oLink.Value.FKiXILInkID, sGUID);
                        }
                    }
                }
            }
            oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, iActiveStepID, sGUID);
            //if (oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).FirstOrDefault().Value.QSLinks != null && oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).FirstOrDefault().Value.QSLinks.Count() > 0)
            //{
            //    foreach (var oQSLink in oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).FirstOrDefault().Value.QSLinks.Values)
            //    {
            //        foreach (var oLink in oQSLink.XiLink.OrderBy(m => m.Value.rOrder))
            //        {
            //            if (oLink.Value.sType == "Post")
            //            {
            //                GetXiLinkContent(oLink.Value.FKiXILInkID, sGUID);
            //            }
            //        }
            //    }
            //}
            oQSInstance.sGUID = sGUID;
            if (oQSInstance.QSDefinition != null && oQSInstance.QSDefinition.Steps.Count() > 0)
            {
                oQSInstance.iCurrentStepID = oQSInstance.Steps.Where(m => m.Value.bIsCurrentStep == true).FirstOrDefault().Value.FKiQSStepDefinitionID;
                oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).Select(m => m.sName).FirstOrDefault();
                //Updating CurrentStepID and Question Set Instance in Cache
                //oCache.Set_ParamVal(sSessionID, sGUID, "CurrentStepID_" + iXISemanticID, oQSInstance.nStepInstances.Where(m => m.bIsCurrentStep == true).FirstOrDefault().FKiQSStepDefinitionID.ToString());
                //oCache.UpdateCacheObject("QuestionSet", sGUID, oQSInstance, sDatabase, oQSInstance.FKiQSDefinitionID);
            }
            if (!string.IsNullOrEmpty(sStepName))
            {
                int iStepID = oQSInstance.QSDefinition.Steps[sStepName].ID;
                if (iStepID > 0)
                {
                    oQSInstance.Steps.ToList().ForEach(m => m.Value.bIsCurrentStep = false);
                    oQSInstance.Steps.Where(m => m.Value.FKiQSStepDefinitionID == iStepID).FirstOrDefault().Value.bIsCurrentStep = true;
                }
            }
            if (oQSInstance.History == null)
            {
                oQSInstance.History = new List<int>();
            }
            bool IsHistory = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).Select(m => m.bIsHistory).FirstOrDefault();
            if (oQSInstance.History.IndexOf(oQSInstance.iCurrentStepID) == -1 && IsHistory)
            {
                oQSInstance.History.Add(oQSInstance.iCurrentStepID);
            }
            oQSInstance.iStage = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).Select(m => m.iStage).FirstOrDefault();
            return oQSInstance;
        }

        [HttpPost]
        public ActionResult GetQuestionSetInstance(int iQSID, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sCurrentGuestUser = string.Empty;
                if (SessionManager.UserUniqueID != null)
                {
                    sCurrentGuestUser = SessionManager.UserUniqueID;
                }
                string sOrgName = SessionManager.OrganisationName;
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var oQSInstance = XiLinkRepository.GetQuestionSetInstance(iQSID, sGUID, sMode, iBODID, iInstanceID, iUserID, sOrgName, sDatabase, sCurrentGuestUser);
                return Json(oQSInstance, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }


        public XIIQS GetInternalQuestionSet(int iQSIID)
        {
            int iUserID = 0;
            if (SessionManager.UserID > 0)
            {
                iUserID = SessionManager.UserID;
            }
            //var iQSIID = iInstanceID;

            XIIXI oXI = new XIIXI();
            var oQSIns = oXI.GetQSInstanceByID(iQSIID);
            var oQSInstance = oXI.GetQuestionSetInstanceByID(oQSIns.FKiQSDefinitionID, iQSIID, null, 0, 0, oQSIns.FKiUserCookieID);
            if (oQSInstance.Steps.Values.Count() > 0)
            {
                oQSInstance.Steps.Values.FirstOrDefault().bIsCurrentStep = true;
            }
            //cQSInstance oQSIns = new cQSInstance();
            //oQSIns = XiLinkRepository.GetQSInstanceByID(iQSIID);
            // var oQSInstance = XiLinkRepository.GetQuestionSetInstance(oQSIns.FKiQSDefinitionID, null, null, 0, 0, iUserID, sOrgName, sDatabase, oQSIns.FKiUserCookieID);
            //oQSInstance = LoadStepInstance(oQSInstance, null, 0);//Commented
            return oQSInstance;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetNextStep(XIIQSStep oStepI, string sGUID, string sType, int iQSIID = 0)
        {
            List<string> Info = new List<string>();
            try
            {
                XIIXI oIXI = new XIIXI();
                XIInfraCache oCache = new XIInfraCache();
                XIIQS oQSInstance = new XIIQS();
                var oQSIns = oIXI.GetQSInstanceByID(iQSIID);
                if (string.IsNullOrEmpty(SessionManager.sRoleName) || SessionManager.sRoleName.ToLower() == xiEnumSystem.EnumRoles.WebUsers.ToString().ToLower())
                {
                    if (oQSIns.bAdminTakeOver)
                    {
                        return PartialView("~\\Views\\Shared\\Error.cshtml");
                    }
                }
                oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);

                if (oQSInstance.QSDefinition == null)
                {
                    Info.Add("sGUID " + sGUID);
                    Info.Add("iQSIID " + iQSIID);
                    oQSInstance = oIXI.GetQuestionSetInstanceByID(0, iQSIID, null, 0, 0, null);
                }
                int iOrgID = SessionManager.OrganizationID;
                XIDQS oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSInstance.FKiQSDefinitionID.ToString(), null, null, 0, iOrgID);
                XIDQS oQSDC = (XIDQS)oQSD.Clone(oQSD);
                oQSInstance.QSDefinition = oQSDC;
                var oStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oStepI.FKiQSStepDefinitionID).FirstOrDefault();
                if (oQSInstance.iCurrentStepID == 0)
                {
                    oQSInstance.iCurrentStepID = oStepD.ID;
                }
                if (oQSInstance.Steps.ContainsKey(oStepD.sName))
                {
                    oQSInstance.Steps[oStepD.sName] = oStepI;
                    foreach (var Section in oQSInstance.Steps[oStepD.sName].Sections)
                    {
                        foreach (var Field in Section.Value.XIValues)
                        {
                            oQSInstance.XIValues[Field.Key].sValue = Field.Value.sValue;
                        }
                    }
                }
                //oQSInstance.Steps.Values.Where(m=>m.ID == oStepI.ID).FirstOrDefault()
                SessionManager.sGUID = sGUID;
                string sCurrentGuestUser = string.Empty;
                if (SessionManager.UserUniqueID != null)
                {
                    sCurrentGuestUser = SessionManager.UserUniqueID;
                }
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var sSessionID = HttpContext.Session.SessionID;
                string sDatabase = SessionManager.CoreDatabase;

                string sOrgName = SessionManager.OrganisationName;
                //var CurrentStepID = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, "CurrentStepID_" + oQSInstance.FKiQSDefinitionID));
                var CurrentStepID = oQSInstance.iCurrentStepID;
                ViewBag.sGUID = sGUID;
                var LastStepID = oQSInstance.QSDefinition.Steps.Select(m => m.Value.ID).LastOrDefault();
                bool IsSaveStep = true;
                if (oStepD.iLockStage != 0 && oStepD.iLockStage <= oQSInstance.iStage && oQSInstance.QSDefinition.bIsStage)
                {
                    IsSaveStep = false;
                }
                //Checking Database Save type for Question Set
                if (oQSInstance.QSDefinition.SaveType.ToLower() == "Save at end".ToLower() && CurrentStepID == LastStepID && IsSaveStep)
                {
                    if (oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == CurrentStepID).Select(m => m.Value.bInMemoryOnly).FirstOrDefault() == false)
                    {
                        SaveQSInstances(oQSInstance, sGUID);
                    }
                }
                else if (oQSInstance.QSDefinition.SaveType.ToLower() == "Save as Populated".ToLower() && IsSaveStep)
                {
                    if (oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == CurrentStepID).Select(m => m.Value.bInMemoryOnly).FirstOrDefault() == false)
                    {
                        SaveQSInstances(oQSInstance, sGUID);
                    }
                }
                XIDScript oXIScript = new XIDScript();
                var oCurrentStep = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == CurrentStepID).Select(m => m.Value).FirstOrDefault();
                var oRefStageDef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "RefTraceStage");
                oRefStageDef.sNameAttribute = "sName";
                string sStageName = string.Empty;
                if (oCurrentStep.QSLinks != null && oCurrentStep.QSLinks.Count() > 0 && IsSaveStep)
                {
                    foreach (var oQSLink in oCurrentStep.QSLinks.Values.OrderBy(x => x.ID))
                    {
                        foreach (var oLink in oQSLink.XiLink.OrderBy(m => m.Value.rOrder))
                        {
                            if (oLink.Value.sType == "Post")
                            {
                                if (!string.IsNullOrEmpty(oLink.Value.sRunType))
                                {
                                    if (oLink.Value.sRunType.ToLower() == "xilink" || oLink.Value.sRunType == "0")
                                        GetXiLinkContent(oLink.Value.FKiXILInkID, sGUID);
                                    else if (oLink.Value.sRunType.ToLower() == "xialgorithm")
                                    {
                                        XIDAlgorithm oAlogD = new XIDAlgorithm();
                                        oAlogD = (XIDAlgorithm)oCache.GetObjectFromCache(XIConstant.CacheXIAlgorithm, null, oLink.Value.FKIXIScriptID.ToString());
                                        oAlogD.Execute_XIAlgorithm(sSessionID, sGUID);
                                    }

                                }
                            }
                        }
                    }
                }
                var CurrentOrder = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == CurrentStepID).Select(m => m.Value.iOrder).FirstOrDefault();
                //oQSInstance.nStepInstances = oQSInstance.nStepInstances;
                int NextStepID = 0;
                //Checking Navigations of a step to decide next step
                if (oCurrentStep.Navigations != null && oCurrentStep.Navigations.Count() > 0)
                {
                    foreach (var Navs in oCurrentStep.Navigations)
                    {
                        if (NextStepID == 0)
                        {
                            var Nav = Navs.Value;
                            if (Nav.sField != null && Nav.sOperator != null && Nav.sValue != null)
                            {
                                var oStepIns = oQSInstance.Steps.Where(m => m.Value.FKiQSStepDefinitionID == CurrentStepID).FirstOrDefault();
                                var StepDef = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == oStepIns.Value.FKiQSStepDefinitionID).FirstOrDefault();
                                var FieldDef = StepDef.Value.FieldDefs.Where(m => m.Value.FieldOrigin.sName == Nav.sField).FirstOrDefault();
                                var FieldValue = oStepIns.Value.XIValues.Where(m => m.Value.FKiFieldDefinitionID == FieldDef.Value.ID).FirstOrDefault();
                                //var Result = EvaluateExpression(FieldValue.Value, Nav.sOperator, Nav.sValue, FieldDef.FieldOrigin);
                                //if (Result)
                                //{
                                //    NextStepID = Nav.iNextStepID;
                                //}//Commented
                            }
                        }
                    }
                }
                //Getting Next Step
                var NextSteps = new List<XIDQSStep>();
                var NextStep = new XIDQSStep();
                string sNextStep = oCache.Get_ParamVal(sSessionID, sGUID, null, "NextStep");
                string sIsQsLoad = oCache.Get_ParamVal(sSessionID, sGUID, null, "IsQsLoad");
                Info.Add("NextStep from Cache: " + sNextStep);
                //Common.SaveErrorLog("sNextStep:" + sNextStep, "XIDNA");
                if (!string.IsNullOrEmpty(sNextStep))
                {
                    NextStep = oQSInstance.QSDefinition.StepD(sNextStep);
                    if (NextStep != null)
                    {
                        NextStepID = NextStep.ID;
                    }
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "NextStep", "", null, null);
                }

                if (NextStepID == 0)
                {
                    NextSteps = oQSInstance.QSDefinition.Steps.Values.OrderBy(m => m.iOrder).Where(m => m.iOrder > CurrentOrder).ToList();
                    NextStep = NextSteps.FirstOrDefault();
                }
                else
                {
                    NextStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == NextStepID).FirstOrDefault();
                    NextSteps = oQSInstance.QSDefinition.Steps.Values.OrderBy(m => m.iOrder).Where(m => m.iOrder > NextStep.iOrder).ToList();
                }
                //Getting Next Step Instance
                int iActiveStepID = 0;
                if (NextStep != null)
                {
                    iActiveStepID = oQSInstance.GetActiveStepID(NextStep.ID, sGUID);
                    var oNextStep = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iActiveStepID).Select(m => m.Value).FirstOrDefault();
                    oQSInstance.iCurrentStepID = iActiveStepID;
                    var oStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                    if (oQSIns.iStage < oStep.iStage)
                    {
                        oQSInstance.iStage = oStep.iStage;
                    }
                    oQSInstance.sCurrentStepName = oStep.sName;
                    XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                    oQSInstance = Connection.Update<XIIQS>(oQSInstance, "XIQSInstance_T", "ID");
                    if (oNextStep.QSLinks != null && oNextStep.QSLinks.Count() > 0 && string.IsNullOrEmpty(sIsQsLoad) && IsSaveStep)
                    {
                        foreach (var oQSLink in oNextStep.QSLinks.Values)
                        {
                            foreach (var oLink in oQSLink.XiLink.OrderBy(m => m.Value.rOrder))
                            {
                                if (oLink.Value.sType == "Pre")
                                {
                                    if (!string.IsNullOrEmpty(oLink.Value.sRunType))
                                    {
                                        if (oLink.Value.sRunType.ToLower() == "xilink" || oLink.Value.sRunType == "0")
                                            GetXiLinkContent(oLink.Value.FKiXILInkID, sGUID);
                                        else if (oLink.Value.sRunType.ToLower() == "xialgorithm")
                                        {
                                            //oLink.Value.FKIXIScriptID = 3031;
                                            //sSessionID = Guid.NewGuid().ToString();
                                            //sGUID = Guid.NewGuid().ToString();
                                            //List<CNV> oNVsList = new List<CNV>();
                                            //oNVsList.Add(new CNV { sName = "-iSignalRID", sValue = "1" });
                                            //oNVsList.Add(new CNV { sName = "-iBODID", sValue = "" });
                                            //oNVsList.Add(new CNV { sName = "-iBOIID", sValue = "" });
                                            //oCache.SetXIParams(oNVsList, sGUID, sSessionID);
                                            XIDAlgorithm oAlogD = new XIDAlgorithm();
                                            oAlogD = (XIDAlgorithm)oCache.GetObjectFromCache(XIConstant.CacheXIAlgorithm, null, oLink.Value.FKIXIScriptID.ToString());
                                            oAlogD.Execute_XIAlgorithm(sSessionID, sGUID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(sIsQsLoad))
                    {
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "IsQsLoad", "", null, null);
                    }
                    if (oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault() == null || oNextStep.bIsReload)
                    {
                        oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, iActiveStepID, sGUID);
                    }
                    else
                    {
                        oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault().bIsCurrentStep = true;
                    }
                    oQSInstance.Steps.Values.ToList().ForEach(m => m.bIsCurrentStep = false);
                    oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault().bIsCurrentStep = true;
                    oQSInstance.iCurrentStepID = iActiveStepID;
                    oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iActiveStepID).Select(m => m.sName).FirstOrDefault();// NextStep.sName;
                    Info.Add("Step Execution: Started " + oQSInstance.sCurrentStepName + " Step");
                    //Common.SaveErrorLog("Step Execution: Started " + oQSInstance.sCurrentStepName + " Step", "XIDNA");
                    //oQSInstance.oDynamicObject = oDynamicObject;
                    //oQSInstance.sHtmlPage = sHtmlPage;
                    var oStepMessage = oCache.Get_ObjectSetCache("StepMessage", sGUID, sSessionID);//(sSessionID, sGUID, null, "StepMessage");
                    int oDecision = Convert.ToInt32(oCache.Get_ObjectSetCache("Decision", sGUID, sSessionID));
                    oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault().XiMessages = new Dictionary<string, string>();
                    if (oStepMessage != null)
                    {
                        if (NextStep != null)
                        {
                            Dictionary<string, string> dictmsgs = new Dictionary<string, string>();
                            dictmsgs = (Dictionary<string, string>)oStepMessage;
                            oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault().XiMessages = dictmsgs;
                            if (oDecision == 3 || oDecision == 5 || oDecision == 6)
                            {
                                oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iActiveStepID).FirstOrDefault().bIsBack = true;
                            }
                            else if (oDecision == 1 || oDecision == 2 || oDecision == 4)
                            {
                                oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iActiveStepID).FirstOrDefault().bIsBack = false;
                            }
                            HttpRuntime.Cache.Remove("StepMessage" + "_" + sGUID + "_" + sSessionID);
                        }
                    }
                    var oStepPrevalition = oCache.Get_ObjectSetCache("StepPreValidationMessage", sGUID, sSessionID);
                    oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault().XiPreMessages = new Dictionary<string, List<string>>();
                    if (oStepPrevalition != null)
                    {
                        if (NextStep != null)
                        {
                            Dictionary<string, List<string>> dictmsgs = new Dictionary<string, List<string>>();
                            dictmsgs = (Dictionary<string, List<string>>)oStepPrevalition;
                            oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iActiveStepID).FirstOrDefault().XiPreMessages = dictmsgs;
                            HttpRuntime.Cache.Remove("StepPreValidationMessage" + "_" + sGUID + "_" + sSessionID);
                        }
                    }
                    //oQSInstanceNew.nStepInstances = oQSInstance.nStepInstances;
                    //oCache.Set_ParamVal(sSessionID, sGUID, "CurrentStepID_" + oQSInstance.FKiQSDefinitionID, NextStep.ID.ToString());
                    //oCache.UpdateCacheObject("QuestionSet", sGUID, oQSInstance, sDatabase, oQSInstance.FKiQSDefinitionID);
                }
                else
                {
                }
                bool IsHistory = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iActiveStepID).Select(m => m.bIsHistory).FirstOrDefault();
                if (oQSInstance.History == null)
                {
                    oQSInstance.History = new List<int>();
                }
                if (oQSInstance.History.IndexOf(oQSInstance.iCurrentStepID) == -1 && IsHistory)
                {
                    oQSInstance.History.Add(oQSInstance.iCurrentStepID);
                }
                oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}", Convert.ToString(oQSInstance.ID), "autoset", null);
                string IsOverRideQuote = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|IsOverRideQuote}");
                ViewBag.IsOverRideQuote = IsOverRideQuote;
                int LastStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.sName.ToLower() == "Your Quotes".ToLower()).Select(m => m.ID).FirstOrDefault();
                var iCurrentStepID = oQSInstance.iCurrentStepID;
                var oCurrentStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iCurrentStepID).FirstOrDefault();
                if (oCurrentStepD != null)
                {
                    Dictionary<string, XIDQSStep> Steps = new Dictionary<string, XIDQSStep>();
                    foreach (var Step in oQSDC.Steps.Values.ToList())
                    {
                        Steps[Step.sName] = new XIDQSStep() { ID = Step.ID, sName = Step.sName, iOrder = Step.iOrder, sDisplayName = Step.sDisplayName, sIsHidden = Step.sIsHidden, iStage = Step.iStage, iLockStage = Step.iLockStage, iCutStage = Step.iCutStage };
                    }
                    oQSInstance.QSDefinition.Steps = Steps;
                    oQSInstance.QSDefinition.Steps[oCurrentStepD.sName] = oCurrentStepD;
                }
                if (sType.ToLower() == "public")
                {
                    Info.Add("Step Execution: Returned " + oQSInstance.sCurrentStepName + " Step");
                    //Common.SaveErrorLog("Step Execution: Returned " + oQSInstance.sCurrentStepName + " Step", "XIDNA");
                    return PartialView("_QuestionSet", oQSInstance);
                }
                else
                {
                    return PartialView("_QuestionSetInternal", oQSInstance);
                }
            }
            catch (Exception ex)
            {
                string sInfo = "INFO: " + string.Join(",\r\n ", Info);
                logger.Error(ex);
                Common.SaveErrorLog(sInfo, "XIDNA");
                Common.SaveErrorLog("Exception: " + ex.ToString(), "XIDNA");
                return PartialView("~/views/Shared/Error.cshtml");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public XIIQS SaveQSStep(XIIQSStep oStepI, string sGUID, int iQSIID = 0)
        {
            List<string> Info = new List<string>();
            XIIQS oQSInstance = new XIIQS();
            try
            {
                XIIXI oIXI = new XIIXI();
                XIInfraCache oCache = new XIInfraCache();
                oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
                if (oQSInstance.QSDefinition == null)
                {
                    Info.Add("sGUID " + sGUID);
                    Info.Add("iQSIID " + iQSIID);
                    oQSInstance = oIXI.GetQuestionSetInstanceByID(0, iQSIID, null, 0, 0, null);
                }
                var oStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oStepI.FKiQSStepDefinitionID).FirstOrDefault();
                if (oQSInstance.iCurrentStepID == 0)
                {
                    oQSInstance.iCurrentStepID = oStepD.ID;
                }
                if (oQSInstance.Steps.ContainsKey(oStepD.sName))
                {
                    oQSInstance.Steps[oStepD.sName] = oStepI;
                    foreach (var Section in oQSInstance.Steps[oStepD.sName].Sections)
                    {
                        foreach (var Field in Section.Value.XIValues)
                        {
                            oQSInstance.XIValues[Field.Key].sValue = Field.Value.sValue;
                        }
                    }
                }
                SaveQSInstances(oQSInstance, sGUID);
            }
            catch (Exception ex)
            {
                string sInfo = "INFO: " + string.Join(",\r\n ", Info);
                logger.Error(ex);
                Common.SaveErrorLog(sInfo, "XIDNA");
                Common.SaveErrorLog("Exception: " + ex.ToString(), "XIDNA");
            }
            return oQSInstance;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool EvaluateExpression(cFieldInstance fieldValue, string sOperator, string sExpectedValue, cFieldOrigin oFieldOrigin)
        {
            MethodInfo function = CreateDynamicFunction();
            var Datatype = oFieldOrigin.DataTypes.sBaseDataType;
            object[] parametersArray = new object[] { Datatype, fieldValue.sValue, sOperator, sExpectedValue };
            var Dynresult = function.Invoke(null, parametersArray);
            return (bool)Dynresult;
        }

        public static MethodInfo CreateDynamicFunction()
        {
            string code = @"
                using System;
            
                namespace UserFunctionsNew
                {                
                    public class EvaluateExpression
                    {                
                        public static bool GetResult(string DataType, string InputValue, string Operator, string ExpectedValue)
                        {
                            if(DataType == ""int""){
                                var ConvertedValue = Convert.ToInt32(InputValue);
                                if(Operator == "">""){
                                    if(ConvertedValue > Convert.ToInt32(ExpectedValue))
                                    {
                                        return true;
                                    }
                                }
                                else if(Operator == ""<""){
                                    if(ConvertedValue < Convert.ToInt32(ExpectedValue))
                                    {
                                        return true;
                                    }
                                }else if(Operator == "">=""){
                                    if(ConvertedValue >= Convert.ToInt32(ExpectedValue))
                                    {
                                        return true;
                                    }
                                }else if(Operator == ""<=""){
                                    if(ConvertedValue <= Convert.ToInt32(ExpectedValue))
                                    {
                                        return true;
                                    }
                                }else if(Operator == ""==""){
                                    if(ConvertedValue == Convert.ToInt32(ExpectedValue))
                                    {
                                        return true;
                                    }
                                }
                                else{ return false;}
                            }
                            else {
                                if(Operator == ""=="" || Operator == ""=""){
                                    if(InputValue == ExpectedValue)
                                    {
                                        return true;
                                    }
                                }
                                else{ return false;}
                            }
                            return false;
                        }
                    }
                }
            ";

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), code);

            Type binaryFunction = results.CompiledAssembly.GetType("UserFunctionsNew.EvaluateExpression");
            return binaryFunction.GetMethod("GetResult");
        }



        [HttpPost]
        public ActionResult GetCurrentStepID(int iQSID, string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            var CurrentStepID = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, null, "CurrentStepID_" + iQSID));
            return Json(CurrentStepID, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetPreviousStep(int iQSIID, string sGUID, string sType)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIIQS oQSInstance = new XIIQS();
            oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
            string sCurrentGuestUser = string.Empty;
            if (SessionManager.UserUniqueID != null)
            {
                sCurrentGuestUser = SessionManager.UserUniqueID;
            }
            var sSessionID = HttpContext.Session.SessionID;
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = 0;
            if (SessionManager.UserID > 0)
            {
                iUserID = SessionManager.UserID;
            }
            string sOrgName = SessionManager.OrganisationName;
            ViewBag.sGUID = sGUID;
            //var oQSInstance = XiLinkRepository.GetQuestionSetInstance(iQSID, sGUID, iUserID, sOrgName, sDatabase, sCurrentGuestUser);
            //var CurrentStepID = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, "CurrentStepID_" + oQSInstance.FKiQSDefinitionID));

            var CurrentStepID = oQSInstance.iCurrentStepID;
            //Getting Previous StepInstance Based on Index of nStepInstances list
            int index = oQSInstance.Steps.Values.ToList().FindIndex(a => a.FKiQSStepDefinitionID == CurrentStepID);
            if (index == 0)
            {
                return PartialView("_QuestionSet", oQSInstance);
            }
            //var CurrentOrder = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == CurrentStepID).Select(m => m.iOrder).FirstOrDefault();

            //var PreviousOrder = oQSInstance.QSDefinition.Steps.Values.Where(m => m.sIsHidden == "off").OrderByDescending(m => m.iOrder).Where(m => m.iOrder < CurrentOrder).Select(m => m.ID).FirstOrDefault();
            //var NxtStp = oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == PreviousOrder).FirstOrDefault();

            //var IsVisible = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == NxtStp.FKiQSStepDefinitionID).FirstOrDefault();
            //if (IsVisible.sIsHidden == "on")
            //{
            //    PreviousOrder = oQSInstance.QSDefinition.Steps.Values.Where(m => m.sIsHidden == "off").OrderByDescending(m => m.iOrder).Where(m => m.iOrder < PreviousOrder).Select(m => m.ID).FirstOrDefault();
            //    NxtStp = oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == PreviousOrder).FirstOrDefault();
            //}
            var CurrentStep = oQSInstance.Steps.Values.ToList().Where(a => a.FKiQSStepDefinitionID == CurrentStepID).FirstOrDefault();
            int iPrevStepID = 0;
            var iCurrentStepIndex = oQSInstance.History.IndexOf(oQSInstance.iCurrentStepID);
            if (CurrentStep.iOverrideStep != null && CurrentStep.iOverrideStep != 0)
            {
                iPrevStepID = Convert.ToInt32(CurrentStep.iOverrideStep);
            }
            else
            {
                if (iCurrentStepIndex >= 0)
                {
                    iPrevStepID = oQSInstance.History[iCurrentStepIndex - 1];
                    //oQSInstance.History.Remove(oQSInstance.iCurrentStepID);
                }
                else
                {
                    iPrevStepID = oQSInstance.History.LastOrDefault();
                }
            }
            //var Step = QSSteps.FirstOrDefault();
            oQSInstance.Steps.Values.ToList().ForEach(m => m.bIsCurrentStep = false);
            oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iPrevStepID).FirstOrDefault().bIsCurrentStep = true;
            oQSInstance.iCurrentStepID = iPrevStepID;
            oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iPrevStepID).Select(m => m.Value.sName).FirstOrDefault();
            XIDQS oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSInstance.FKiQSDefinitionID.ToString());
            XIDQS oQSDC = (XIDQS)oQSD.Clone(oQSD);
            var oCurrentStepD = oQSDC.Steps.Values.Where(m => m.ID == iPrevStepID).FirstOrDefault();
            oQSInstance.QSDefinition = oQSDC;
            bool IsLoad = true;
            if (oCurrentStepD.iLockStage != 0 && oCurrentStepD.iLockStage <= oQSInstance.iStage && oQSInstance.QSDefinition.bIsStage)
            {
                IsLoad = false;
            }
            if (oCurrentStepD.bIsReload && IsLoad)
            {
                oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, iPrevStepID, sGUID);
                oCurrentStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iPrevStepID).FirstOrDefault();
            }
            //oQSInstance.History.Add(iPrevStepID);
            //oCache.Set_ParamVal(sSessionID, sGUID, "CurrentStepID_" + oQSInstance.FKiQSDefinitionID, NxtStp.FKiQSStepDefinitionID.ToString());
            XIIXI oXII = new XIIXI();
            List<CNV> oWhrParams = new List<CNV>();
            oWhrParams.Add(new CNV { sName = "FKiQSInstanceID", sValue = iQSIID.ToString() });
            XIIBO oBOI = new XIIBO();
            oBOI = oXII.BOI("Lead_T", null, "create", oWhrParams);
            if (oBOI != null && oBOI.Attributes.ContainsKey("sQSStage"))
            {
                oBOI.Attributes["sQSStage"].sValue = oQSInstance.sCurrentStepName;
                oBOI.Attributes["sQSStage"].bDirty = true;
                oBOI.Save(oBOI);
            }

            var oQSI = oXII.BOI("QS Instance", iQSIID.ToString(), "updatestage");
            if (oQSI != null && oQSI.Attributes.ContainsKey("iStage"))
            {
                int iStage = 0;
                var sStage = oQSI.Attributes["iStage"].sValue;
                if (int.TryParse(sStage, out iStage))
                {
                    var oStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                    if (iStage > oStep.iStage && oStep.iCutStage <= iStage && iStage < oStep.iLockStage)
                    {
                        oQSI.Attributes["iStage"].sValue = oStep.iCutStage.ToString();
                        oQSI.Attributes["iStage"].bDirty = true;
                        oQSI.Save(oQSI);
                    }
                }
            }
            if (oCurrentStepD != null)
            {
                Dictionary<string, XIDQSStep> Steps = new Dictionary<string, XIDQSStep>();
                foreach (var Step in oQSD.Steps.Values.ToList())
                {
                    Steps[Step.sName] = new XIDQSStep() { ID = Step.ID, sName = Step.sName, iOrder = Step.iOrder, sDisplayName = Step.sDisplayName, sIsHidden = Step.sIsHidden, iStage = Step.iStage, iLockStage = Step.iLockStage, iCutStage = Step.iLockStage };
                }
                oQSInstance.QSDefinition.Steps = Steps;
                oQSInstance.QSDefinition.Steps[oCurrentStepD.sName] = oCurrentStepD;
            }
            if (sType.ToLower() == "public")
            {
                return PartialView("_QuestionSet", oQSInstance);
            }
            else
            {
                return PartialView("_QuestionSetInternal", oQSInstance);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public XIIQS SaveQSInstances(XIIQS oQSInstance, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIQS oQSI = new XIIQS(); var sSessionID = HttpContext.Session.SessionID;
                string sCurrentGuestUser = string.Empty;
                int iUserID = 0;
                if (SessionManager.UserUniqueID != null)
                {
                    sCurrentGuestUser = SessionManager.UserUniqueID;
                }
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                if (SessionManager.OrganizationID > 0)
                {
                    oQSInstance.FKiOrgID = SessionManager.OrganizationID;
                }
                string sOrgName = SessionManager.OrganisationName;
                var CurrentStepID = oQSInstance.iCurrentStepID;
                var oStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == CurrentStepID).FirstOrDefault();
                XIIQSStep oStepI = oQSInstance.Steps[oStepD.sName];
                //var CurrentStepID = Convert.ToInt32(oCache.Get_ParamVal(sSessionID, sGUID, "CurrentStepID_" + oQSInstance.FKiQSDefinitionID));
                //var CurrentStepID = oQSInstance.iCurrentStepID;
                if (oQSInstance.Steps.ContainsKey(oStepD.sName))
                {
                    //oQSInstance.Steps[oStepD.sName] = oStepI;
                    foreach (var Section in oQSInstance.Steps[oStepD.sName].Sections)
                    {
                        var oFieldDef = oStepD.Sections[Section.Key].FieldDefs;
                        foreach (var Field in Section.Value.XIValues)
                        {
                            var oFiledOrigin = oFieldDef[Field.Key].FieldOrigin;
                            if (oFiledOrigin.bIsSetToCache)
                            {
                                string sCacheParam = oFiledOrigin.sName;
                                if (!string.IsNullOrEmpty(oFiledOrigin.sCacheName))
                                {
                                    sCacheParam = oFiledOrigin.sCacheName;
                                }
                                oCache.Set_ParamVal(sSessionID, sGUID, "", sCacheParam, Field.Value.sValue, "", null);
                            }
                            if (!string.IsNullOrEmpty(oFiledOrigin.sDependentFieldIDs) && oFiledOrigin.sIsHidden == "on")
                            {
                                var oDependentFields = oFiledOrigin.sDependentFieldIDs.Split(',');
                                if (oFiledOrigin.DataType.sBaseDataType == "datetime")
                                {
                                    XIIBO oBOI = new XIIBO();
                                    string sDay = string.Empty; string sMonth = string.Empty; string sYear = string.Empty; string sResult = string.Empty;
                                    string sFormat = oFiledOrigin.sFormat;
                                    if (string.IsNullOrEmpty(sFormat))
                                    {
                                        sFormat = XIConstant.Date_Format; //"dd-MMM-yyyy";
                                    }
                                    foreach (var sOriginID in oDependentFields)
                                    {
                                        int iOriginId = 0;
                                        if (int.TryParse(sOriginID, out iOriginId))
                                        {
                                            //int iOriginId = Convert.ToInt32(sOriginID);
                                            var oXIValues = Section.Value.XIValues.Values.ToList();
                                            var oDependentFieldDef = oFieldDef.Values.ToList();
                                            var oDependentFiledOrigin = oDependentFieldDef.Where(x => x.FKiXIFieldOriginID == iOriginId).Select(x => x.FieldOrigin).FirstOrDefault();
                                            if (oDependentFiledOrigin.sCode.ToLower() == "month")
                                            {
                                                sMonth = oXIValues.Where(x => x.FKiFieldOriginID == iOriginId).Select(x => x.sValue).FirstOrDefault();
                                                //if (!string.IsNullOrEmpty(sMonth) && !sMonth.StartsWith("0"))
                                                //{
                                                //int iMonth = Convert.ToInt32(sMonth);
                                                int iMonth = 0;
                                                if (int.TryParse(sMonth, out iMonth))
                                                {
                                                    if (iMonth <= 9 && !sMonth.StartsWith("0"))
                                                    {
                                                        sMonth = "0" + sMonth;
                                                    }
                                                }
                                                //}
                                            }
                                            if (oDependentFiledOrigin.sCode.ToLower() == "year")
                                            {
                                                sYear = oXIValues.Where(x => x.FKiFieldOriginID == iOriginId).Select(x => x.sValue).FirstOrDefault();
                                            }
                                            if (oDependentFiledOrigin.sCode.ToLower() == "date")
                                            {
                                                sDay = oXIValues.Where(x => x.FKiFieldOriginID == iOriginId).Select(x => x.sValue).FirstOrDefault();
                                                //if (!string.IsNullOrEmpty(sDay) && !sDay.StartsWith("0"))
                                                //{
                                                //int iDay = Convert.ToInt32(sDay);
                                                int iDay = 0;
                                                if (int.TryParse(sDay, out iDay))
                                                {
                                                    if (iDay <= 9 && !sDay.StartsWith("0"))
                                                    {
                                                        sDay = "0" + sDay;
                                                    }
                                                }
                                                //}
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(sDay) && !string.IsNullOrEmpty(sMonth) && !string.IsNullOrEmpty(sYear))
                                        {
                                            sResult = sDay + "-" + sMonth + "-" + sYear;
                                        }
                                        else if (string.IsNullOrEmpty(sDay) && !string.IsNullOrEmpty(sMonth) && !string.IsNullOrEmpty(sYear))
                                        {
                                            sResult = "01" + "-" + sMonth + "-" + sYear;
                                        }
                                        if (!string.IsNullOrEmpty(sResult))
                                        {
                                            Field.Value.sValue = String.Format("{0:" + sFormat + "}", oBOI.ConvertToDtTime(sResult));
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var sOriginID in oDependentFields)
                                    {
                                        int iOriginId = 0;
                                        if (int.TryParse(sOriginID, out iOriginId))
                                        {
                                            //int iOriginId = Convert.ToInt32(sOriginID);
                                            var oXIValues = Section.Value.XIValues.Values.ToList();
                                            var oDependentFieldDef = oFieldDef.Values.ToList();
                                            var oDependentFiledOrigin = oDependentFieldDef.Where(x => x.FKiXIFieldOriginID == iOriginId).Select(x => x.FieldOrigin).FirstOrDefault();
                                            if (oDependentFiledOrigin != null && !oDependentFiledOrigin.bIsHidden)
                                            {
                                                var oFieldI = oXIValues.Where(m => m.FKiFieldOriginID == iOriginId).FirstOrDefault();
                                                if (oFieldI != null && !string.IsNullOrEmpty(oFieldI.sValue))
                                                {
                                                    Field.Value.sValue = oFieldI.sValue;
                                                    Field.Value.sDerivedValue = oFieldI.sDerivedValue;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            oQSInstance.XIValues[Field.Key].sValue = Field.Value.sValue;
                        }
                    }
                }
                var Response = oQSI.Save(oQSInstance, sCurrentGuestUser);
                oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
                //var Response = XiLinkRepository.SaveQSInstance(oQSInstance, CurrentStepID, iUserID, sOrgName, sDatabase, sCurrentGuestUser);
                return oQSInstance;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetAboutInsurer(string sInsurer)
        {
            return PartialView("_AboutInsurer", sInsurer);
        }

        [HttpPost]
        public ActionResult GetStepContent(int iStepID, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                var Step = XiLinkRepository.GetStepDefinition(iStepID, sGUID, iUserID, sOrgName, sDatabase);
                return Json(Step, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //[HttpPost]
        [AllowAnonymous]
        public ActionResult GetStepData(XIDQSStep oStepDef, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIQS oQSInstance = new XIIQS();
                XIDQS QSDefinition = new XIDQS();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                var Step = oStepDef; //XiLinkRepository.GetStepDefinition(iStepID, sGUID, iUserID, sOrgName, sDatabase);
                QSDefinition.Steps = new Dictionary<string, XIDQSStep>();
                if (!string.IsNullOrEmpty(Step.sName))
                {
                    QSDefinition.Steps[Step.sName] = Step;
                }
                else
                {
                    QSDefinition.Steps[Step.ID.ToString()] = Step;
                }

                XIIQSStep oStepInstance = new XIIQSStep();
                List<XIIValue> nFieldValues = new List<XIIValue>();
                if (((EnumSemanticsDisplayAs)Step.iDisplayAs).ToString() == EnumSemanticsDisplayAs.Fields.ToString())
                {
                    //if (Step.FieldDefs != null)
                    //{
                    //    nFieldValues = Step.FieldDefs.Select(m => new XIIValue { FKiFieldDefinitionID = m.Value.ID }).ToList();
                    //    oStepInstance.XIValues = nFieldValues;
                    //}
                }
                //Loading Sections for Step
                else if (((EnumSemanticsDisplayAs)Step.iDisplayAs).ToString() == EnumSemanticsDisplayAs.Sections.ToString())
                {
                    Dictionary<string, XIIQSSection> nSecIns = new Dictionary<string, XIIQSSection>();
                    if (Step.Sections != null && Step.Sections.Count() > 0)
                    {
                        foreach (var sec in Step.Sections)
                        {
                            XIIQSSection oSecIns = new XIIQSSection();
                            oSecIns.FKiStepSectionDefinitionID = sec.Value.ID;
                            //if (((EnumSemanticsDisplayAs)sec.Value.iDisplayAs).ToString() == EnumSemanticsDisplayAs.Fields.ToString())
                            //{
                            //    List<XIIValue> nSecFieldValues = new List<XIIValue>();
                            //    if (sec.Value.FieldDefs != null && sec.Value.FieldDefs.Count() > 0)
                            //    {
                            //        nSecFieldValues = sec.Value.FieldDefs.Select(m => new XIIValue { FKiFieldDefinitionID = m.Value.ID }).ToList();
                            //    }
                            //    oSecIns.XIValues = nSecFieldValues;
                            //}
                            nSecIns[sec.Value.ID.ToString()] = oSecIns;
                        }
                        //oStepInstance.nSectionInstances = Step.Sections;
                    }
                    oStepInstance.Sections = nSecIns;
                }
                oStepInstance.bIsCurrentStep = true;
                oStepInstance.FKiQSStepDefinitionID = oStepDef.ID;
                oQSInstance.QSDefinition = QSDefinition;
                oQSInstance.Steps = new Dictionary<string, XIIQSStep>();
                if (!string.IsNullOrEmpty(Step.sName))
                {
                    oQSInstance.Steps[Step.sName] = oStepInstance;
                }
                else
                {
                    oQSInstance.Steps[Step.ID.ToString()] = oStepInstance;
                }
                ViewBag.sGUID = sGUID;
                return PartialView("~/Views/XIComponents/_StepComponentContent.cshtml", oQSInstance);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AmendDetails(int iQSIID, string sGUID, string sType)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIIQS oQSInstance = new XIIQS();
            oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
            string sCurrentGuestUser = string.Empty;
            if (SessionManager.UserUniqueID != null)
            {
                sCurrentGuestUser = SessionManager.UserUniqueID;
            }
            var sSessionID = HttpContext.Session.SessionID;
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = 0;
            if (SessionManager.UserID > 0)
            {
                iUserID = SessionManager.UserID;
            }
            string sOrgName = SessionManager.OrganisationName;
            ViewBag.sGUID = sGUID;

            if (oQSInstance.Steps.Values.Count() > 0)
            {
                var StepID = oQSInstance.QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).Select(m => m.ID).FirstOrDefault();
                var oCurrentStep = oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == StepID).FirstOrDefault();
                if (oCurrentStep == null)
                {
                    var ActiveStepID = oQSInstance.GetActiveStepID(StepID, sGUID);
                    oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, ActiveStepID);
                    oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == ActiveStepID).FirstOrDefault().bIsCurrentStep = true;
                }
                else
                {
                    var ActiveStepID = oQSInstance.GetActiveStepID(StepID, sGUID);
                    oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == ActiveStepID).FirstOrDefault().bIsCurrentStep = true;
                }
            }
            oQSInstance.iCurrentStepID = oQSInstance.Steps.Values.Where(m => m.bIsCurrentStep == true).Select(m => m.FKiQSStepDefinitionID).FirstOrDefault();
            oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).Select(m => m.sName).FirstOrDefault();
            XIDQS oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSInstance.FKiQSDefinitionID.ToString());
            XIDQS oQSDC = (XIDQS)oQSD.Clone(oQSD);
            var oCurrentStepD = oQSDC.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
            oQSInstance.QSDefinition = oQSDC;
            //oQSInstance.sQSType = "Internal";
            oQSInstance.History = new List<int>();
            oQSInstance.History.Add(oQSInstance.iCurrentStepID);
            if (oCurrentStepD != null)
            {
                Dictionary<string, XIDQSStep> Steps = new Dictionary<string, XIDQSStep>();
                foreach (var Step in oQSD.Steps.Values.ToList())
                {
                    Steps[Step.sName] = new XIDQSStep() { ID = Step.ID, sName = Step.sName, iOrder = Step.iOrder, sDisplayName = Step.sDisplayName, sIsHidden = Step.sIsHidden, iStage = Step.iStage, iLockStage = Step.iLockStage, iCutStage = Step.iLockStage };
                }
                oQSInstance.QSDefinition.Steps = Steps;
                oQSInstance.QSDefinition.Steps[oCurrentStepD.sName] = oCurrentStepD;
            }
            oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
            oCache.Set_ParamVal(sSessionID, sGUID, "", "IsAmend", "yes", "", null);
            if (sType.ToLower() == "public")
            {
                return PartialView("_QuestionSet", oQSInstance);
            }
            else
            {
                return PartialView("_QuestionSetInternal", oQSInstance);
            }
        }
        public ActionResult ClientLoadQs(int iQSIID, string sGUID, int iQSDID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIQS oQSInstance = new XIIQS();
                XIIXI oXI = new XIIXI();
                var sSessionID = HttpContext.Session.SessionID;
                oQSInstance = oXI.GetQSXIValuesByQSIID(iQSIID);//.GetXIValuesByQSIID(iQSSIID);
                oQSInstance.ID = iQSIID;
                if (iQSDID == 0)
                {
                    iQSDID = oQSInstance.FKiQSDefinitionID;
                }
                var oQSDefinition = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDID.ToString());//oDXI.GetQuestionSetDefinitionByID(null, iQSID.ToString());
                if (oQSDefinition != null)
                {
                    oQSInstance.QSDefinition = oQSDefinition;
                }
                oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|ClassID}", oQSInstance.QSDefinition.FKiClassID.ToString(), null, null);
                var StepID = oQSInstance.QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).Select(m => m.ID).FirstOrDefault();
                var stepName = oCache.Get_ParamVal(sSessionID, sGUID, "", "sCurrentStepName");
                if (!string.IsNullOrEmpty(stepName))
                {
                    if (!string.IsNullOrEmpty(stepName))
                    {
                        StepID = oQSInstance.QSDefinition.Steps[stepName].ID;
                    }
                    else
                    {
                        StepID = oQSInstance.GetActiveStepID(0, sGUID);
                    }
                }
                oCache.Set_ParamVal(sSessionID, sGUID, "", "sCurrentStepName", "", null, null);
                var oCurrentStep = oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == StepID).FirstOrDefault();
                if (oCurrentStep == null)
                {
                    var ActiveStepID = oQSInstance.GetActiveStepID(StepID, sGUID);
                    oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, ActiveStepID);
                    oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == ActiveStepID).FirstOrDefault().bIsCurrentStep = true;
                }
                else
                {
                    var ActiveStepID = oQSInstance.GetActiveStepID(StepID, sGUID);
                    oQSInstance.Steps.Values.ToList().Where(m => m.FKiQSStepDefinitionID == ActiveStepID).FirstOrDefault().bIsCurrentStep = true;
                }
                oQSInstance.FKiQSDefinitionID = iQSDID;
                oQSInstance.iCurrentStepID = oQSInstance.Steps.Values.Where(m => m.bIsCurrentStep == true).Select(m => m.FKiQSStepDefinitionID).FirstOrDefault();
                oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).Select(m => m.sName).FirstOrDefault();
                oQSInstance.sQSType = "Public";
                oCache.Set_ParamVal(sSessionID, sGUID, "", "sQSType", oQSInstance.sQSType, null, null);
                if (oQSInstance.History == null)
                {
                    oQSInstance.History = new List<int>();
                }
                oQSInstance.sGUID = sGUID;
                oQSInstance.History.Add(oQSInstance.iCurrentStepID);
                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQSInstanceID}", Convert.ToString(oQSInstance.ID), "autoset", null);
                oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
                ViewBag.sGUID = sGUID;
                return PartialView("QuestionSet", oQSInstance);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #endregion QuestionSet

        #region DemoXIScripts
        [AllowAnonymous]
        public ActionResult XIScripting(int XILinkID, string sGUID, int iInstanceID, int iBOID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                //iBOID = 2;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                int iCustomerID = SessionManager.CustomerID;
                var ScriptResults = XiLinkRepository.XIScripting(XILinkID, sGUID, iInstanceID, iBOID, iUserID, sOrgName, sDatabase, iCustomerID);
                return Json(ScriptResults, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }

        }
        #endregion DemoXIScripts
        #region EditableGrid
        public ActionResult GetBOInstance(int iBOID, string sGroupName, string sBOName, int iInstanceID = 0)
        {

            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                if (sBOName != null && sGroupName != null)
                {
                    var oBODisplay = oXiAPI.GetFormData(sBOName, sGroupName, iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                    return PartialView("_AddEditableView", oBODisplay);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return null;
        }
        public ActionResult DeleteBOInstance(int iBOID, string sGroupName, string sBOName, int iInstanceID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;

                var oBODisplay = BusinessObjectsRepository.DeleteFormData(iBOID, sBOName, sGroupName, iInstanceID, sVisualisation, iUserID, sOrgName, sDatabase, null);
                return Json(oBODisplay, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        #endregion


        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteBO(int iInstanceID, string sGUID, string sBOName)
        {
            XIIXI oXIIXI = new XIIXI();
            var oBOI = oXIIXI.BOI(sBOName, iInstanceID.ToString());
            try
            {
                var Response = oBOI.Delete(oBOI);
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        //, bool lazyload
        public XIIBO GetStructureBoInstanceList(string strName)
        {
            XIIBO oBOI = new XIIBO();
            XIDStructure oXIDStructure = new XIDStructure();
            Dictionary<string, List<XIIBO>> oMyClass = null;
            XIIBO obj = null;
            int iBOID = 17;
            string sDatabase = SessionManager.CoreDatabase; var sSessionID = HttpContext.Session.SessionID;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            ModelDbContext dbcontext = new ModelDbContext();
            string sUID = SessionManager.sGUID;
            oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
            oXIDStructure.sCoreDataBase = sDatabase;
            oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
            oXIDStructure.sOrgDataBase = oUser.sDatabaseName;
            oXIDStructure.iOrgId = SessionManager.OrganizationID;
            //oMyClass=oXIDStructure.GetStructureBoInstance(strName, obj, 0, oMyClass, iBOID, "ACPolicy_T", 0,null,null);
            //oMyClass = GetStructureBoInstance(strName, obj, 0, oMyClass, iBOID, "ACPolicy_T",0);
            oBOI.SubChildI = oMyClass;
            //oBOInstanceLIst = GetStructureBoInstance(strName);
            return oBOI;
        }

        [AllowAnonymous]
        public Dictionary<string, List<XIIBO>> GetStructureBoInstance(string sStructureCode, XIIBO oBOIParent, int iOneClickID, Dictionary<string, List<XIIBO>> oMainBOInstances, int iBOID, string sBOName, int iParentStructureID)
        {
            List<XIIBO> oTempList = new List<XIIBO>();
            List<XIIBO> oBOInstanceLIst = new List<XIIBO>();
            Dictionary<string, List<XIIBO>> oMyClass = new Dictionary<string, List<XIIBO>>();
            Dictionary<string, XIIBO> oDictionaryBOI = null;
            Dictionary<string, List<XIIBO>> oMyDictClass = new Dictionary<string, List<XIIBO>>();
            if (oMainBOInstances != null)
            {
                oMyDictClass = oMainBOInstances;
            }
            XIIBO oBOInstance = new XIIBO();
            string sDatabase = SessionManager.CoreDatabase; var sSessionID = HttpContext.Session.SessionID;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            ModelDbContext dbcontext = new ModelDbContext();
            string sUID = SessionManager.sGUID;
            oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
            oUser.sCoreDatabaseName = sDatabase;
            oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
            var sOrgDB = oUser.sDatabaseName;
            //laod structure def
            XIDStructure oXIDStructure = new XIDStructure();
            List<XIDStructure> oLXIDStructure = new List<XIDStructure>();
            List<XIDStructure> oLXIDSubStructure = new List<XIDStructure>();
            oXIDStructure.sCode = sStructureCode;
            oXIDStructure.BOID = iBOID;
            oXIDStructure.FKiParentID = "#";
            var XIDStruct = oXIDStructure.Get_XIStructureDefinition();
            if (XIDStruct.xiStatus == 0 && XIDStruct.oResult != null)
            {
                oLXIDStructure = (List<XIDStructure>)XIDStruct.oResult;
                oXIDStructure = oLXIDStructure.FirstOrDefault();
                if (oXIDStructure == null)
                {
                    if (iParentStructureID != 0)
                    {
                        XIDStructure oXIDSubStructure = new XIDStructure();
                        oXIDSubStructure.FKiParentID = iParentStructureID.ToString();
                        oXIDSubStructure.sCode = sStructureCode;
                        oXIDSubStructure.BOID = iBOID;
                        var XIDSubStruct = oXIDSubStructure.Get_XIStructureDefinition();
                        if (XIDSubStruct.xiStatus == 0 && XIDSubStruct.oResult != null)
                        {
                            oLXIDSubStructure = (List<XIDStructure>)XIDSubStruct.oResult;
                        }
                    }
                    oXIDStructure = oLXIDSubStructure.FirstOrDefault();
                }
                if (oXIDStructure != null)
                {
                    long iMainStructureid = oXIDStructure.ID;
                    string sMainStructureid = Convert.ToString(iMainStructureid);
                    int imainBOID = oXIDStructure.BOID;
                    string MainBoName = oXIDStructure.sBO;
                    int iMainOneClickID = 436;
                    if (iOneClickID != 0)
                    {
                        iMainOneClickID = iOneClickID;
                    }
                    XIDXI oXID = new XIDXI();
                    XIDBO oBOD = (XIDBO)oXID.Get_BODefinition("", iBOID.ToString()).oResult;
                    var param = oCache.GetAllParamsUnderGUID(sSessionID, sSessionID, "");
                    List<CNV> nParams = new List<CNV>();
                    nParams = param.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                    //oIXI.oNVParams = nParams;
                    XID1Click oD1Click = new XID1Click();
                    oXID.sCoreDatabase = sDatabase;
                    oXID.sOrgDatabase = sOrgDB;
                    oXID.iOrgID = oUser.FKiOrganisationID;
                    //list data loading
                    oD1Click.ID = iMainOneClickID;
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["Name"] = sBOName;
                    var oOneclickDef = oXID.Get_1ClickDefinition(null, iMainOneClickID.ToString());
                    if (oOneclickDef.xiStatus == 0 && oOneclickDef.oResult != null)
                    {
                        XID1Click oXID1Click = (XID1Click)oOneclickDef.oResult;
                        XIDStructure oStructure = new XIDStructure();
                        oD1Click.Query = oStructure.ReplaceExpressionWithCacheValue(oXID1Click.Query, nParams);
                    }
                    var DataSource = oXID.GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID);
                    oD1Click.sConnectionString = DataSource;
                    oDictionaryBOI = oD1Click.OneClick_Execute();
                    if (oBOIParent != null)
                    {
                        oBOIParent.SubChildI.Add(sBOName, oDictionaryBOI.Values.ToList());//add substructures dictionarylist to main structure
                    }
                    //check for sub stru
                    oTempList = oDictionaryBOI.Values.ToList();
                    if (oMainBOInstances == null)
                    {
                        oMyDictClass.Add(sBOName, oTempList);
                    }
                    oXIDStructure = new XIDStructure();
                    oXIDStructure.FKiParentID = sMainStructureid;
                    XIDStruct = oXIDStructure.Get_XIStructureDefinition();
                    if (XIDStruct.xiStatus == 0 && XIDStruct.oResult != null)
                    {
                        oLXIDStructure = (List<XIDStructure>)XIDStruct.oResult;
                        oXIDStructure = oLXIDStructure.FirstOrDefault();
                        var oSubChildBOList = oLXIDStructure.Where(x => x.sType == "Sub Entity").ToList();//substructures list
                        if (oSubChildBOList != null)
                        {
                            foreach (var item in oTempList)// for every BO load sub structure
                            {
                                foreach (var subchild in oSubChildBOList) //for every sub structure
                                {
                                    string sParamValue = item.Attributes.Where(x => x.Key.ToLower() == "id".ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                                    string sParamName = MainBoName;
                                    oCache.Set_ParamVal(sSessionID, sSessionID, null, "{XIP|" + sParamName + ".id}", sParamValue, "autoset", null);
                                    int iSubChildOneClickID = 0;
                                    List<XIIBO> oBOSubChildInstanceLIst = new List<XIIBO>(); // load sub structure & verify
                                    string sSubChildBOName = subchild.sBO;
                                    List<XIDStructure> oLXIDSubStructure1 = new List<XIDStructure>();
                                    XIDStructure oXIDSubStructure = new XIDStructure();
                                    oXIDSubStructure.FKiParentID = "#";
                                    oXIDSubStructure.sBO = sSubChildBOName;
                                    oXIDSubStructure.sCode = sStructureCode;
                                    XIDStruct = oXIDSubStructure.Get_XIStructureDefinition();
                                    if (XIDStruct.xiStatus == 0 && XIDStruct.oResult != null)
                                    {
                                        oLXIDSubStructure1 = (List<XIDStructure>)XIDStruct.oResult;
                                        oXIDSubStructure = oLXIDSubStructure1.FirstOrDefault();
                                    }
                                    XIDStructureDetail oXIDStructureDetails = new XIDStructureDetail();
                                    List<XIDStructureDetail> oLXIDStructureDetails = new List<XIDStructureDetail>();
                                    if (oXIDSubStructure == null)//if substructure don't have structure 
                                    {
                                        iParentStructureID = Convert.ToInt32(subchild.FKiParentID);
                                        imainBOID = subchild.BOID;
                                        oXIDStructureDetails.iParentStructureID = iParentStructureID;
                                        oXIDStructureDetails.FKiStructureID = subchild.ID;
                                    }
                                    else
                                    {
                                        if (oXIDSubStructure.ID != 0)
                                        {
                                            iParentStructureID = Convert.ToInt32(oXIDSubStructure.ID);
                                            oXIDStructureDetails.iParentStructureID = iParentStructureID;
                                            oXIDStructureDetails.FKiStructureID = oXIDSubStructure.ID;
                                            imainBOID = oXIDSubStructure.BOID;
                                        }
                                    }
                                    var oXIStructureRes = oXIDStructureDetails.Get_XIStructureDetailsDefinition();
                                    if (oXIStructureRes.xiStatus == 00 && oXIStructureRes.oResult != null)
                                    {
                                        oLXIDStructureDetails = (List<XIDStructureDetail>)oXIStructureRes.oResult;
                                        oXIDStructureDetails = oLXIDStructureDetails.FirstOrDefault();
                                    }
                                    iSubChildOneClickID = oXIDStructureDetails.i1ClickID;
                                    GetStructureBoInstance(sStructureCode, item, iSubChildOneClickID, oMyDictClass, imainBOID, subchild.sBO, iParentStructureID);
                                }
                            }
                        }
                    }
                    List<XIIBO> oSTRList = oBOInstanceLIst;
                    oMyClass = new Dictionary<string, List<XIIBO>>();
                    oMyClass.Add("", oDictionaryBOI.Values.ToList());
                }
            }
            return oMainBOInstances == null ? oMyDictClass : oMainBOInstances;
        }

        [AllowAnonymous]
        public XIIBO GetNotationValue()
        {
            string sDatabase = SessionManager.CoreDatabase; var sSessionID = HttpContext.Session.SessionID;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName; int iOrgID = SessionManager.OrganizationID;
            ModelDbContext dbcontext = new ModelDbContext();
            string sUID = SessionManager.sGUID;
            oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
            oUser.sCoreDatabaseName = sDatabase;
            oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
            var sOrgDB = oUser.sDatabaseName;
            XIIBO oBII = new XIIBO(); long rTotalCost = 0;
            var boi = oIXI.BOI("ACPolicy_T", "127");
            var oLIst = oIXI.BOI("ACPolicy_T", "127").Structure("New Policy").XILoad();
            return oBII;
        }
        [AllowAnonymous]
        public ActionResult SetParams(string sID, string sGUID, string sName, string sBO, string sType = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sSessionID = HttpContext.Session.SessionID;

                //if (!string.IsNullOrEmpty(sID))
                //{
                if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "session")
                {
                    if(!string.IsNullOrEmpty(sBO) && sBO.ToLower() == "campaign")
                    {
                        SessionManager.iCampaignID = Convert.ToInt32(sID);
                    }
                    else
                    {
                        SessionManager.sSignalRCID = sID;
                    }
                    
                }
                else
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, sName, sID, null, null);
                }

                if (sBO == "Aggregations")
                {
                    List<CNV> oNV = new List<CNV>();
                    oNV.Add(new CNV { sName = "sGUID", sValue = sID });
                    var oQuoteI = oIXI.BOI("Aggregations", "", "", oNV);
                    if (oQuoteI.Attributes != null)
                    {
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|IsOverRideQuote}", oQuoteI.Attributes["bIsOverRide"].sValue, null, null);
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|Aggregations.id}", oQuoteI.Attributes["id"].sValue, null, null);
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|FKiQuoteID}", oQuoteI.Attributes["id"].sValue, null, null);
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iQuoteID}", oQuoteI.Attributes["id"].sValue, null, null);
                        var iProductVersionID = oQuoteI.Attributes["FKiProductVersionID"].sValue;
                        var oProductVersionI = oIXI.BOI("ProductVersion_T", iProductVersionID);
                        string iProductID = oProductVersionI.Attributes["FKiProductID"].sValue;
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|FKiProductID}", iProductID, null, null);
                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|Product.id}", iProductID, null, null);
                        //Common.SaveErrorLog("Buy Now: Product id: " + iProductID + " and SGUID: "+sGUID , sDatabase);
                        List<CNV> oNV1 = new List<CNV>();
                        oNV1.Add(new CNV { sName = "FKiProductID", sValue = iProductID });
                        var oTaskI = oIXI.BOI("XProdDiaryBatch_T", "", "", oNV1);
                        if (oTaskI != null && oTaskI.Attributes != null && oTaskI.Attributes.ContainsKey("FKiDiaryBatchID"))
                        {
                            oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|TaskBatchTempalte_T.ID}", oTaskI.Attributes["FKiDiaryBatchID"].sValue, null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            //}
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        //public ActionResult StructureCopy()
        //{
        //    XIDStructure oStructure = new XIDStructure();
        //    XIIXI oXI = new XIIXI();
        //    XIBOBuilding oBOBuilding = new XIBOBuilding();
        //    int iACPolicyID = 127;
        //    string FKiProductID = "1";
        //    string sPolicyStartDate = "2018-10-10 16:16:50.220";
        //    //var oInstance = oIXI.BOI("ACPolicy_T", Convert.ToString(22677)).Structure("TS").XILoad();
        //    //var oResponse = oBOBuilding.BuildRequirementsBO(iACPolicyID, FKiProductID, sPolicyStartDate);
        //    //if (!oResponse.bOK && oResponse.xiStatus == xiEnumSystem.xiFuncResult.xiError)
        //    //{
        //    //    //oResult.oTraceStack.Add(new CNV { sName = "Create Requirements", sValue = "Error: While Creating Requirements" });
        //    //}
        //    XIInfraMenuComponent oMenuComponent = new XIInfraMenuComponent();
        //    List<CNV> oParams = new List<CNV>();
        //    CNV ONVPairs = new CNV();
        //    ONVPairs.sName = "MenuName";
        //    ONVPairs.sValue = "HomeMenu";
        //    oParams.Add(ONVPairs);

        //    ONVPairs = new CNV();
        //    ONVPairs.sName = "iUserID";
        //    ONVPairs.sValue = SessionManager.UserID.ToString();
        //    oParams.Add(ONVPairs);

        //    ONVPairs = new CNV();
        //    ONVPairs.sName = "sDatabase";
        //    ONVPairs.sValue = SessionManager.CoreDatabase;
        //    oParams.Add(ONVPairs);

        //    ONVPairs = new CNV();
        //    ONVPairs.sName = "iOrganizationID";
        //    ONVPairs.sValue = SessionManager.OrganizationID.ToString();
        //    oParams.Add(ONVPairs);
        //    var olist = oMenuComponent.XILoad(oParams);
        //    List<XIMenu> oRightMenus = new List<XIMenu>();
        //    if (olist.bOK && olist.oResult!=null)
        //    {
        //        oRightMenus = (List<XIMenu>)olist.oResult;
        //    }
        //    //var oList = oStructure.StructureCopy("ACPolicy_T", "127", "New Policy");
        //    //return Json(null, JsonRequestBehavior.AllowGet);
        //    return View("~/Views/XIComponents/_MenuComponent.cshtml", oRightMenus);
        //}


        public ActionResult BOICopy(string sInstanceID, string sBOName)
        {
            string sDatabase = SessionManager.CoreDatabase;
            int iInstanceID = 0;
            try
            {
                XIIBO oBOI = new XIIBO();
                var oCResult = oBOI.BOICopy(sInstanceID, sBOName);
                if (oCResult.bOK && oCResult.oResult != null)
                {
                    iInstanceID = (int)oCResult.oResult;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

            return Json(iInstanceID, JsonRequestBehavior.AllowGet);
        }

        #region XILinkLoad

        public ActionResult XILinkLoad(int iXILinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {

                XIInfraCache oCache = new XIInfraCache();
                XIInstanceBase oInstBase = new XIInstanceBase();
                XIILink oXILinkI = new XIILink();
                XIILink oXILinkRes = new XIILink();
                if (iXILinkID > 0)
                {
                    oXILinkI.iXILinkID = iXILinkID;
                    var Result = oXILinkI.Load();
                    if (Result.bOK && Result.oResult != null)
                    {
                        oInstBase = (XIInstanceBase)Result.oResult;
                    }
                }
                return PartialView("_XILinkContent", oInstBase);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        [Compress]
        public JsonResult XILinkLoadJson(int iXILinkID = 0, string sGUID = "", List<CNV> oParams = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                if (oParams != null)
                {
                    //oParams = XIGUIDConversion.GetXIGUIDList(oParams);
                }
                string sSessionID = HttpContext.Session.SessionID;
                if (string.IsNullOrEmpty(sGUID))
                {
                    Singleton.Instance.sGUID = null;
                    Singleton.Instance.sActiveGUID.Remove(sSessionID);
                }
                XIILink oXILinkI = new XIILink();
                XIILink oXILinkRes = new XIILink();
                oParams = oParams ?? new List<CNV>();
                oParams.Add(new CNV { sName = XIConstant.Param_UserID, sValue = SessionManager.UserID.ToString() });
                if (iXILinkID > 0)
                {
                    oXILinkI.sGUID = sGUID;
                    oXILinkI.iXILinkID = iXILinkID;
                    oXILinkI.oParams = oParams;
                    //oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|" + oBOD.Name + ".id}", ID.ToString(), null, null);
                    var Result = oXILinkI.Load();
                    if (Result.bOK && Result.oResult != null)
                    {
                        oXILinkRes = (XIILink)Result.oResult;
                        dynamic oMergedData = MergeHTMLRecurrsive(oXILinkRes);
                        return Json(oMergedData, JsonRequestBehavior.AllowGet);
                    }
                }
                //string json = JsonConvert.SerializeObject(oInstBase, Newtonsoft.Json.Formatting.Indented);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        private object MergeHTMLRecurrsive(dynamic oInstBase, string sGUID = "")
        {
            string sRenderType = string.Empty;
            try
            {
                if (oInstBase.oContent.ContainsKey(XIConstant.ContentXIComponent))
                {
                    var control = oInstBase.oContent[XIConstant.ContentXIComponent];
                    var sType = control.GetType();
                    XIIComponent compI = new XIIComponent();
                    var oIns = new XIInstanceBase();
                    if (sType.Name == "XIInstanceBase")
                    {
                        oIns = (XIInstanceBase)oInstBase.oContent[XIConstant.ContentXIComponent];
                        compI = (XIIComponent)oIns.oContent[XIConstant.ContentXIComponent];
                    }
                    else if (sType.Name == "XIIComponent")
                    {
                        compI = (XIIComponent)oInstBase.oContent[XIConstant.ContentXIComponent];
                    }
                    //var comp = (XIIComponent)XiiComp.oContent[XIConstant.ContentXIComponent];
                    var compD = (XIDComponent)compI.oDefintion;
                    sRenderType = compD.sName;
                    if (compD.sName.ToLower() == XIConstant.OneClickComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_OneClickComponent.cshtml", compI);
                        compI.oContent[XIConstant.OneClickComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.FormComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_FormComponent.cshtml", compI);
                        compI.oContent[XIConstant.FormComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XITreeStructure.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XITreeStructureView.cshtml", compI);
                        compI.oContent[XIConstant.XITreeStructure] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.TabComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_TabComponent.cshtml", compI);
                        compI.oContent[XIConstant.TabComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MenuComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuComponent.cshtml", compI);
                        compI.oContent[XIConstant.MenuComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.HTMLComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_HtmlRepeaterComponent.cshtml", compI);
                        compI.oContent[XIConstant.HTMLComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.GridComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_GridComponent.cshtml", compI);
                        compI.oContent[XIConstant.GridComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.InboxComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_InboxComponent.cshtml", compI);
                        compI.oContent[XIConstant.InboxComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.GroupComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_GroupComponent.cshtml", compI);
                        compI.oContent[XIConstant.GroupComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.ScriptComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_ScriptComponent.cshtml", compI);
                        compI.oContent[XIConstant.ScriptComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XilinkComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XILinkComponent.cshtml", compI);
                        compI.oContent[XIConstant.XilinkComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.LayoutComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutComponent.cshtml", compI);
                        compI.oContent[XIConstant.LayoutComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.LayoutDetailsComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutDetailsComponent.cshtml", compI);
                        compI.oContent[XIConstant.LayoutDetailsComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.LayoutMappingComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutMappingComponent.cshtml", compI);
                        compI.oContent[XIConstant.LayoutMappingComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIApplicationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_ApplicationComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIApplicationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MenuNodeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuNodeComponent.cshtml", compI);
                        compI.oContent[XIConstant.MenuNodeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIUrlMappingComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIUrlMappingComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIUrlMappingComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DialogComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DialogComponent.cshtml", compI);
                        compI.oContent[XIConstant.DialogComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.FieldOriginComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_FieldOriginComponent.cshtml", compI);
                        compI.oContent[XIConstant.FieldOriginComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIParameterComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIParameterComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIParameterComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DataTypeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DataTypeComponent.cshtml", compI);
                        compI.oContent[XIConstant.DataTypeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIComponentComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIComponentComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIComponentComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOAttributeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOAttributeComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOAttributeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOScriptComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOScriptComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOScriptComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOStructureComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOStructureComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOStructureComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIInfraXIBOUIComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOUIDetailsComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIInfraXIBOUIComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIDataSourceComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIDataSourceComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIDataSourceComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QueryManagementComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QueryManagementComponent.cshtml", compI);
                        compI.oContent[XIConstant.QueryManagementComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSConfigComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSConfigComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSConfigComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSStepConfigComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSStepConfigComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSStepConfigComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSSectionConfigComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSSectionConfigComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSSectionConfigComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.VisualisationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_VisualisationComponent.cshtml", compI);
                        compI.oContent[XIConstant.VisualisationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.ReportComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIReportComponent.cshtml", compI);
                        compI.oContent[XIConstant.ReportComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.PieChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_PieChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.PieChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.CombinationChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_CombinationChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.CombinationChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.GaugeChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_GaugeChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.GaugeChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DashBoardChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DashBoardChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.DashBoardChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QuoteReportDataComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QuoteReportDataComponent.cshtml", compI);
                        compI.oContent[XIConstant.QuoteReportDataComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MappingComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MappingComponent.cshtml", compI);
                        compI.oContent[XIConstant.MappingComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MultiRowComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MultiRowComponent.cshtml", compI);
                        compI.oContent[XIConstant.MultiRowComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DailerComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DailerComponent.cshtml", compI);
                        compI.oContent[XIConstant.DailerComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSLinkComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkComponentComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSLinkComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSLinkDefinationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkDefinationComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSLinkDefinationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.FeedComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_FeedComponent.cshtml", compI);
                        compI.oContent[XIConstant.FeedComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4PriceChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PriceChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4PriceChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4PieChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PieChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4PieChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4GaugeChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4GaugeChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4GaugeChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4SemiPieChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4SemiPieChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4SemiPieChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4BarChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4BarChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4BarChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4LineChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4LineChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4LineChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4HeatChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4HeatChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4HeatChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.ReportDataComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_ReportDataComponent.cshtml", compI);
                        compI.oContent[XIConstant.ReportDataComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DocumentTreeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DocumentTreeComponent.cshtml", compI);
                        compI.oContent[XIConstant.DocumentTreeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.UserCreationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_UserCreationComponent.cshtml", compI);
                        compI.oContent[XIConstant.UserCreationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.KPICircleComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_KPICircleComponent.cshtml", compI);
                        compI.oContent[XIConstant.KPICircleComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DynamicTreeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIDynamicTreeStructureView.cshtml", compI);
                        compI.oContent[XIConstant.DynamicTreeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSComponent.ToLower())
                    {
                        var oQSI = (XIIQS)compI.oContent[XIConstant.QSComponent];
                        if (oQSI.Steps != null && oQSI.Steps.Count() > 0)
                        {
                            var oStepI = oQSI.Steps.FirstOrDefault().Value;
                            var oStepD = (XIDQSStep)oStepI.oDefintion;
                            if (oStepD.iLayoutID > 0)
                            {
                                //var oLayoutIns = oStepI.oContent[XIConstant.ContentLayout];
                                var oLayoutD = (XIDLayout)oStepI.oContent[XIConstant.ContentLayout];
                                for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
                                {
                                    MergeHTMLRecurrsive(oLayoutD.LayoutMappings[i]);
                                }
                            }
                            else if (oStepD.Sections.Count() > 0)
                            {
                                //TO DO : Load XIFields as other components loading
                                var DATAMerge = MergeHTML("~/views/XiLink/QuestionSet.cshtml", oQSI);
                                oQSI.oContent[XIConstant.ContentFields] = DATAMerge;
                            }
                        }
                    }
                }
                else
                {
                    if (oInstBase.oContent.ContainsKey(XIConstant.ContentDialog))
                    {
                        var con = (XIDDialog)oInstBase.oContent[XIConstant.ContentDialog];
                        //var xifs = (XIDDialog)con.oContent.Values.FirstOrDefault();
                        MergeHTMLRecurrsive(con);
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentPopup))
                    {
                        var con = (XIDPopup)oInstBase.oContent[XIConstant.ContentPopup];
                        //var xifs = (XIDDialog)con.oContent.Values.FirstOrDefault();
                        MergeHTMLRecurrsive(con);
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentLayout))
                    {
                        var oLayoutD = (XIDLayout)oInstBase.oContent[XIConstant.ContentLayout];
                        for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
                        {
                            MergeHTMLRecurrsive(oLayoutD.LayoutMappings[i]);
                        }
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentXILink))
                    {
                        var XiLinkD = oInstBase.oContent[XIConstant.ContentXILink];
                        MergeHTMLRecurrsive(XiLinkD);
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentStep))
                    {
                        var oStepI = (XIIQSStep)oInstBase.oContent[XIConstant.ContentStep];
                        if (oStepI.Sections != null && oStepI.Sections.Count() > 0)
                        {
                            var oSecI = oStepI.Sections.FirstOrDefault().Value;
                            if (oSecI != null)
                            {
                                var oSecD = (XIDQSSection)oSecI.oDefintion;
                                //var osecIns = new XIInstanceBase();
                                if (oSecI.oContent.ContainsKey(XIConstant.ContentXIComponent))
                                {
                                    //osecIns = (XIInstanceBase)oSecI.oContent[XIConstant.ContentXIComponent];
                                    var compI = (XIIComponent)oSecI.oContent[XIConstant.ContentXIComponent];
                                    var CompD = (XIDComponent)compI.oDefintion;
                                    sRenderType = CompD.sName;
                                    if (CompD.sName.ToLower() == XIConstant.OneClickComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_OneClickComponent.cshtml", compI);
                                        compI.oContent[XIConstant.OneClickComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.GroupComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_GroupComponent.cshtml", compI);
                                        compI.oContent[XIConstant.GroupComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.ScriptComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_ScriptComponent.cshtml", compI);
                                        compI.oContent[XIConstant.ScriptComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XilinkComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XILinkComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XilinkComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.LayoutComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutComponent.cshtml", compI);
                                        compI.oContent[XIConstant.LayoutComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.LayoutDetailsComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutDetailsComponent.cshtml", compI);
                                        compI.oContent[XIConstant.LayoutDetailsComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.LayoutMappingComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutMappingComponent.cshtml", compI);
                                        compI.oContent[XIConstant.LayoutMappingComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIApplicationComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_ApplicationComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIApplicationComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MenuNodeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuNodeComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MenuNodeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.FormComponent.ToLower())
                                    {
                                        ViewBag.sGUID = sGUID;
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_FormComponent.cshtml", compI);
                                        compI.oContent[XIConstant.FormComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XITreeStructure.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XITreeStructureView.cshtml", compI);
                                        compI.oContent[XIConstant.XITreeStructure] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.TabComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_TabComponent.cshtml", compI);
                                        compI.oContent[XIConstant.TabComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MenuComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MenuComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.GridComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_GridComponent.cshtml", compI);
                                        compI.oContent[XIConstant.GridComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIUrlMappingComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIUrlMappingComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIUrlMappingComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DialogComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DialogComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DialogComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.FieldOriginComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_FieldOriginComponent.cshtml", compI);
                                        compI.oContent[XIConstant.FieldOriginComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIParameterComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIParameterComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIParameterComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DataTypeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DataTypeComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DataTypeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIComponentComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIComponentComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIComponentComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOAttributeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOAttributeComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOAttributeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOScriptComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOScriptComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOScriptComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOStructureComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOStructureComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOStructureComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIInfraXIBOUIComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOUIDetailsComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIInfraXIBOUIComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIDataSourceComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIDataSourceComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIDataSourceComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QueryManagementComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QueryManagementComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QueryManagementComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSConfigComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSConfigComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSConfigComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSStepConfigComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSStepConfigComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSStepConfigComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSSectionConfigComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSSectionConfigComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSSectionConfigComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.VisualisationComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_VisualisationComponent.cshtml", compI);
                                        compI.oContent[XIConstant.VisualisationComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.HTMLComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_HtmlRepeaterComponent.cshtml", compI);
                                        compI.oContent[XIConstant.HTMLComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.InboxComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_InboxComponent.cshtml", compI);
                                        compI.oContent[XIConstant.InboxComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.PieChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_PieChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.PieChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.CombinationChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_CombinationChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.CombinationChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.GaugeChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_GaugeChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.GaugeChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DailerComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DailerComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DailerComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DashBoardChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DashBoardChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DashBoardChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QuoteReportDataComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QuoteReportDataComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QuoteReportDataComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MappingComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MappingComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MappingComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MultiRowComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MultiRowComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MultiRowComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.ReportComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIReportComponent.cshtml", compI);
                                        compI.oContent[XIConstant.ReportComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSLinkComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkComponentComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSLinkComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSLinkDefinationComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkDefinitionComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSLinkDefinationComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4PieChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PieChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4PieChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4GaugeChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4GaugeChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4GaugeChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4SemiPieChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4SemiPieChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4SemiPieChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4BarChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4BarChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4BarChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4LineChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4LineChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4LineChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4HeatChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4HeatChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4HeatChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4PriceChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PriceChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4PriceChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.FeedComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_FeedComponent.cshtml", compI);
                                        compI.oContent[XIConstant.FeedComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DynamicTreeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIDynamicTreeStructureView.cshtml", compI);
                                        compI.oContent[XIConstant.DynamicTreeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.KPICircleComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_KPICircleComponent.cshtml", compI);
                                        compI.oContent[XIConstant.KPICircleComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }

                                    //oSecI.oContent[XIConstant.ContentXIComponent] = osecIns;
                                    oStepI.Sections[oSecD.ID + "_Sec"] = oSecI;
                                }
                                else if (oSecI.XIValues != null && oSecI.XIValues.Count() > 0)
                                {
                                    var DATAMerge = MergeHTML("~/views/XIComponents/_QSField.cshtml", oSecI);
                                    oSecI.oContent[XIConstant.InboxComponent] = DATAMerge;
                                    oSecI.oContent[XIConstant.ContentXIComponent] = oSecI;
                                }
                            }
                            else
                            {
                                oStepI.Sections[0 + "_Sec"] = oSecI;
                            }
                        }
                        else if (oStepI.oContent.ContainsKey(XIConstant.ContentLayout))
                        {
                            var oLayoutD = (XIDLayout)oStepI.oContent[XIConstant.ContentLayout];
                            for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
                            {
                                MergeHTMLRecurrsive(oLayoutD.LayoutMappings[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Error at HTML Merge: Component Name: " + sRenderType + " and exception Message: " + ex.ToString(), "");
            }
            return oInstBase;
        }

        //private XIInstanceBase MergeHTMLRecurrsive(XIInstanceBase oInstBase)
        //{
        //    if (oInstBase.oContent.ContainsKey(XIConstant.ContentXIComponent))
        //    {
        //        var control = oInstBase.oContent[XIConstant.ContentXIComponent];
        //        var sType = control.GetType();
        //        XIIComponent compI = new XIIComponent();
        //        var oIns = new XIInstanceBase();
        //        if (sType.Name == "XIInstanceBase")
        //        {
        //            oIns = (XIInstanceBase)oInstBase.oContent[XIConstant.ContentXIComponent];
        //            compI = (XIIComponent)oIns.oContent[XIConstant.ContentXIComponent];
        //        }
        //        else if (sType.Name == "XIIComponent")
        //        {
        //            compI = (XIIComponent)oInstBase.oContent[XIConstant.ContentXIComponent];
        //        }
        //        //var comp = (XIIComponent)XiiComp.oContent[XIConstant.ContentXIComponent];
        //        var compD = (XIDComponent)compI.oDefintion;
        //        if (compD.sName.ToLower() == XIConstant.OneClickComponent.ToLower())
        //        {
        //            var o1ClickD = (XID1Click)compI.oContent[XIConstant.OneClickComponent];
        //            var DATAMerge = MergeHTML("~/views/XIComponents/_OneClickComponent.cshtml", compI);
        //            compI.oContent[XIConstant.OneClickComponent] = DATAMerge;
        //            if (sType.Name == "XIInstanceBase")
        //            {
        //                oIns.oContent[XIConstant.ContentXIComponent] = compI;
        //                oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
        //            }
        //            else
        //            {
        //                oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
        //            }
        //        }
        //        else if (compD.sName.ToLower() == XIConstant.QSComponent.ToLower())
        //        {
        //            var oQSI = (XIIQS)compI.oContent[XIConstant.QSComponent];
        //            if (oQSI.Steps != null && oQSI.Steps.Count() > 0)
        //            {
        //                var oStepI = oQSI.Steps.FirstOrDefault().Value;
        //                var oStepD = (XIDQSStep)oStepI.oDefintion;
        //                if (oStepD.iLayoutID > 0)
        //                {
        //                    var oLayoutIns = (XIInstanceBase)oStepI.oContent[XIConstant.ContentLayout];
        //                    var oLayoutD = (XIDLayout)oLayoutIns.oContent[XIConstant.ContentLayout];
        //                    for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
        //                    {
        //                        MergeHTMLRecurrsive((XIInstanceBase)oLayoutD.LayoutMappings[i].oContent.Values.FirstOrDefault());
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (oInstBase.oContent.ContainsKey(XIConstant.ContentDialog))
        //        {
        //            var con = (XIInstanceBase)oInstBase.oContent[XIConstant.ContentDialog];
        //            var xifs = (XIDDialog)con.oContent.Values.FirstOrDefault();
        //            MergeHTMLRecurrsive((XIInstanceBase)xifs.oContent.Values.FirstOrDefault());
        //        }
        //        else if (oInstBase.oContent.ContainsKey(XIConstant.ContentLayout))
        //        {
        //            var oLayoutD = (XIDLayout)oInstBase.oContent[XIConstant.ContentLayout];
        //            for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
        //            {
        //                MergeHTMLRecurrsive((XIInstanceBase)oLayoutD.LayoutMappings[i].oContent.Values.FirstOrDefault());
        //            }
        //        }
        //        else if (oInstBase.oContent.ContainsKey(XIConstant.ContentXILink))
        //        {
        //            var XiLinkD = oInstBase.oContent[XIConstant.ContentXILink];
        //            MergeHTMLRecurrsive((XIInstanceBase)oInstBase.oContent.Values.FirstOrDefault());
        //        }
        //        else if (oInstBase.oContent.ContainsKey(XIConstant.ContentStep))
        //        {
        //            var oStepI = (XIIQSStep)oInstBase.oContent[XIConstant.ContentStep];
        //            if (oStepI.Sections != null && oStepI.Sections.Count() > 0)
        //            {
        //                var oSecI = oStepI.Sections.FirstOrDefault().Value;
        //                var oSecD = (XIDQSSection)oSecI.oDefintion;
        //                var osecIns = new XIInstanceBase();
        //                if (oSecI.oContent.ContainsKey(XIConstant.ContentXIComponent))
        //                {
        //                    osecIns = (XIInstanceBase)oSecI.oContent[XIConstant.ContentXIComponent];
        //                    var compI = (XIIComponent)osecIns.oContent[XIConstant.ContentXIComponent];
        //                    var CompD = (XIDComponent)compI.oDefintion;
        //                    if (CompD.sName.ToLower() == XIConstant.OneClickComponent.ToLower())
        //                    {
        //                        var DATAMerge = MergeHTML("~/views/XIComponents/_OneClickComponent.cshtml", compI);
        //                        compI.oContent[XIConstant.OneClickComponent] = DATAMerge;
        //                        osecIns.oContent[XIConstant.ContentXIComponent] = compI;
        //                    }
        //                    else if (CompD.sName.ToLower() == XIConstant.FormComponent.ToLower())
        //                    {
        //                        var DATAMerge = MergeHTML("~/views/XIComponents/_FormComponent.cshtml", compI);
        //                        compI.oContent[XIConstant.FormComponent] = DATAMerge;
        //                        osecIns.oContent[XIConstant.ContentXIComponent] = compI;
        //                    }
        //                    else if (CompD.sName.ToLower() == XIConstant.XITreeStructure.ToLower())
        //                    {
        //                        var DATAMerge = MergeHTML("~/views/XIComponents/_XITreeStructureView.cshtml", compI);
        //                        compI.oContent[XIConstant.XITreeStructure] = DATAMerge;
        //                        osecIns.oContent[XIConstant.ContentXIComponent] = compI;
        //                    }
        //                    oSecI.oContent[XIConstant.ContentXIComponent] = osecIns;
        //                    oStepI.Sections[oSecD.ID + "_Sec"] = oSecI;
        //                }
        //            }
        //        }

        //    }
        //    return oInstBase;
        //}

        [HttpPost]
        public ActionResult XIContent(string oData)
        {
            string sDatabase = SessionManager.CoreDatabase;
            string sPartialView = string.Empty;
            object oResult = null;
            try
            {
                XIInstanceBase account = JsonConvert.DeserializeObject<XIInstanceBase>(oData);
                var Data = account.oContent.Values.FirstOrDefault();
                if (account.oContent.ContainsKey(XIConstant.ContentXIComponent))
                {
                    var oComponent = Deserialise(oData.ToString());
                    sPartialView = "_XILinkContent";
                    oResult = oComponent.Values.FirstOrDefault();
                }
                if (account.oContent.ContainsKey(XIConstant.ContentLayout))
                {
                    var oLayoutD = JsonConvert.DeserializeObject<XIDLayout>(Data.ToString());
                    foreach (var items in oLayoutD.LayoutMappings)
                    {
                        if (!string.IsNullOrEmpty(items.HTMLCode))
                        {
                            items.oContent["html"] = items.HTMLCode;
                        }
                        else
                        {
                            var Response = Deserialise(items.oContent.Values.FirstOrDefault().ToString());
                            items.oContent = Response;
                        }

                    }
                    sPartialView = "_LayoutData";
                    oResult = oLayoutD;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return PartialView(sPartialView, oResult);
        }


        public Dictionary<string, object> Deserialise(string sData)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                Dictionary<string, object> oInstanceBase = new Dictionary<string, object>();
                XIInstanceBase oData = JsonConvert.DeserializeObject<XIInstanceBase>(sData);
                if (oData.oContent.ContainsKey(XIConstant.ContentLayout))
                {
                    string oSerlize = oData.oContent[XIConstant.ContentLayout].ToString();
                    var oLayoutD = JsonConvert.DeserializeObject<XIDLayout>(oSerlize.ToString());
                    foreach (var items in oLayoutD.LayoutMappings)
                    {
                        var Response = Deserialise(items.oContent.Values.FirstOrDefault().ToString());
                        items.oContent = Response;
                    }
                    oData.oContent[XIConstant.ContentLayout] = oLayoutD;
                    oInstanceBase[XIConstant.ContentLayout] = oData;
                }
                else if (oData.oContent.ContainsKey(XIConstant.ContentXILink))
                {

                    var oStrXiLink = oData.oContent.Values.FirstOrDefault();
                    var oXiLinkD = JsonConvert.DeserializeObject<XIILink>(oStrXiLink.ToString());
                    if (oXiLinkD.oContent != null)
                    {
                        var oResult3 = Deserialise(oXiLinkD.oContent.Values.FirstOrDefault().ToString());
                        oXiLinkD.oContent = oResult3;
                        oData.oContent[XIConstant.ContentXILink] = oXiLinkD;
                        oInstanceBase[XIConstant.ContentXILink] = oData;
                    }
                }
                else if (oData.oContent.ContainsKey(XIConstant.ContentXIComponent))
                {
                    var oStrXiLink = oData.oContent.Values.FirstOrDefault();
                    var oXICompD = JsonConvert.DeserializeObject<XIIComponent>(oStrXiLink.ToString());
                    if (oXICompD.oContent.ContainsKey(XIConstant.QSComponent))
                    {
                        var oStrQSet = oXICompD.oContent[XIConstant.QSComponent];
                        var oQSI = JsonConvert.DeserializeObject<XIIQS>(oStrQSet.ToString());
                        if (oQSI.Steps != null)
                        {
                            foreach (var oStepI in oQSI.Steps)
                            {
                                if (oStepI.Value != null && oStepI.Value.oContent != null)
                                {
                                    if (oStepI.Value.oContent.ContainsKey(XIConstant.ContentLayout))
                                    {
                                        var oResult3 = Deserialise(oStepI.Value.oContent.Values.FirstOrDefault().ToString());
                                        oStepI.Value.oContent = oResult3;
                                    }
                                }
                            }
                        }


                        oXICompD.oContent[XIConstant.QSComponent] = oQSI;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.OneClickComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.OneClickComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.OneClickComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.GroupComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.GroupComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.GroupComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.TabComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.TabComponent];
                        var result2 = JsonConvert.DeserializeObject<List<XIDStructure>>(result.ToString());
                        oXICompD.oContent[XIConstant.TabComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.FormComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.FormComponent];
                        var result2 = JsonConvert.DeserializeObject<XIBODisplay>(result.ToString());
                        oXICompD.oContent[XIConstant.FormComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.MenuComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.MenuComponent];
                        var result2 = JsonConvert.DeserializeObject<List<XIMenu>>(result.ToString());
                        oXICompD.oContent[XIConstant.MenuComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.XITreeStructure))
                    {
                        var result = oXICompD.oContent[XIConstant.XITreeStructure];
                        var result2 = JsonConvert.DeserializeObject<List<XIDStructure>>(result.ToString());
                        oXICompD.oContent[XIConstant.XITreeStructure] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.GridComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.GridComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.GridComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.ReportComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.ReportComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.ReportComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.ReportComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.ReportComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.ReportComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.PieChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.PieChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.PieChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.CombinationChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.CombinationChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.CombinationChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.GaugeChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.GaugeChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.GaugeChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.DashBoardChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.DashBoardChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.DashBoardChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.QuoteReportDataComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.QuoteReportDataComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.QuoteReportDataComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.DailerComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.DailerComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.DailerComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.MappingComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.MappingComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.MappingComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.MultiRowComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.MultiRowComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.MultiRowComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4PieChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4PieChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4PieChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4BarChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4BarChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4BarChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4LineChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4LineChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4LineChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4HeatChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4HeatChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4HeatChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4PriceChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4PriceChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4PriceChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4GaugeChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4GaugeChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4GaugeChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                    else if (oXICompD.oContent.ContainsKey(XIConstant.AM4SemiPieChartComponent))
                    {
                        var result = oXICompD.oContent[XIConstant.AM4SemiPieChartComponent];
                        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                        oXICompD.oContent[XIConstant.AM4SemiPieChartComponent] = result2;
                        oData.oContent[XIConstant.ContentXIComponent] = oXICompD;
                        oInstanceBase[XIConstant.ContentXIComponent] = oData;
                    }
                }
                else if (oData.oContent.ContainsKey(XIConstant.ContentStep))
                {
                    var oStrStep = oData.oContent.Values.FirstOrDefault().ToString();
                    var oStepI = JsonConvert.DeserializeObject<XIIQSStep>(oStrStep);
                    if (oStepI.Sections != null && oStepI.Sections.Count() > 0)
                    {
                        foreach (var oSecI in oStepI.Sections)
                        {
                            if (oSecI.Value.oContent != null)
                            {
                                if (oSecI.Value.oContent.ContainsKey(XIConstant.ContentXIComponent))
                                {
                                    var oResult3 = Deserialise(oSecI.Value.oContent.Values.FirstOrDefault().ToString());
                                    oSecI.Value.oContent = oResult3;
                                }
                            }
                        }
                    }
                    else if (oStepI.oContent != null)
                    {
                        if (oStepI.oContent.ContainsKey(XIConstant.ContentLayout))
                        {
                            var oResult3 = Deserialise(oStepI.oContent.Values.FirstOrDefault().ToString());
                            oStepI.oContent = oResult3;
                        }
                    }
                    oData.oContent[XIConstant.ContentStep] = oStepI;
                    oInstanceBase[XIConstant.ContentStep] = oData;
                }
                else if (oData.oContent.ContainsKey(XIConstant.ContentHTML))
                {

                }
                return oInstanceBase;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult MergeHTML(string HTMLPage, object oContent)
        {
            var viewStr = RenderRazorViewToString(HTMLPage, oContent);
            return Content(viewStr);
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public ActionResult XIContentLoad(string oData)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var Result = Deserialise(oData);
                //XIInstanceBase oInsBase = JsonConvert.DeserializeObject<XIInstanceBase>(oData);
                //if (oInsBase.oContent.ContainsKey(XIConstant.ContentXILink))
                //{
                //    var xiLink = oInsBase.oContent[XIConstant.ContentXILink].ToString();
                //    var oXILinkD = JsonConvert.DeserializeObject<XILink>(xiLink.ToString());
                //    var Data = oXILinkD.oContent.Values.FirstOrDefault();
                //    XIInstanceBase oInsBase2 = JsonConvert.DeserializeObject<XIInstanceBase>(Data.ToString());
                //    var xiComp = oInsBase2.oContent[XIConstant.ContentXIComponent].ToString();
                //    var oXICompD = JsonConvert.DeserializeObject<XIIComponent>(xiComp.ToString());
                //    var result = oXICompD.oContent["OneClickComponent"];
                //    var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                //    oXICompD.oContent["OneClickComponent"] = result2;
                //    oInsBase2.oContent["xicomponent"] = oXICompD;
                //    oXILinkD.oContent["xicomponent"] = oInsBase2;
                //    oInsBase.oContent["xilink"] = oXILinkD;
                //}
                //else if (oInsBase.oContent.ContainsKey(XIConstant.ContentXIComponent))
                //{
                //    var xiComp = oInsBase.oContent[XIConstant.ContentXIComponent].ToString();
                //    var oXICompD = JsonConvert.DeserializeObject<XIIComponent>(xiComp.ToString());
                //}
                //else
                //{
                //    var Data = oInsBase.oContent.Values.FirstOrDefault();
                //    var oLayoutD = JsonConvert.DeserializeObject<XIDLayout>(Data.ToString());
                //    foreach (var items in oLayoutD.LayoutMappings)
                //    {
                //        var fdfsfsf = items.oContent["xilink"].ToString();
                //        var oInstance = JsonConvert.DeserializeObject<XIInstanceBase>(fdfsfsf.ToString());
                //        var Data2 = oInstance.oContent.Values.FirstOrDefault();
                //        var oXiLink = JsonConvert.DeserializeObject<XILink>(Data2.ToString());
                //        var xicomp = oXiLink.oContent["xicomponent"].ToString();
                //        var oXICompD = JsonConvert.DeserializeObject<XIInstanceBase>(xicomp.ToString());
                //        var data3 = oXICompD.oContent.Values.FirstOrDefault();
                //        var xicompD = JsonConvert.DeserializeObject<XIIComponent>(data3.ToString());
                //        var result = xicompD.oContent["OneClickComponent"];
                //        var result2 = JsonConvert.DeserializeObject<XID1Click>(result.ToString());
                //        xicompD.oContent["OneClickComponent"] = result2;
                //        oXICompD.oContent["xicomponent"] = xicompD;
                //        oXiLink.oContent["xicomponent"] = oXICompD;
                //        oInstance.oContent["xilink"] = oXiLink;
                //        items.oContent["xilink"] = oInstance;
                //    }
                //}                
                var oData2 = Result.Values.FirstOrDefault();
                return PartialView("_XILinkContent", oData2);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult PreviewXiLink(int XiLinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIDefinitionBase oXID = new XIDefinitionBase();
                XIILink oXII = new XIILink();
                oXII.iXILinkID = XiLinkID;
                var oXIPreview = oXII.Preview();
                if (oXIPreview.bOK && oXIPreview.oResult != null)
                {

                    oXID = (XIDefinitionBase)oXIPreview.oResult;
                }
                return PartialView("_XIPreview", oXID);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        [HttpPost]
        public ActionResult XIPreviewXiLink(int XiLinkID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIDefinitionBase oXID = new XIDefinitionBase();
                XIILink oXII = new XIILink();
                oXII.iXILinkID = XiLinkID;
                var oXIPreview = oXII.Preview();
                if (oXIPreview.bOK && oXIPreview.oResult != null)
                {
                    var oXilinkDef = (XILink)oXIPreview.oResult;
                    dynamic oMergedData = MergePreviewRecurrsive(oXilinkDef);
                    return Json(oMergedData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return null;
        }
        private object MergePreviewRecurrsive(dynamic oInstBase)
        {
            string sRenderType = string.Empty;
            try
            {
                if (oInstBase.oContent.ContainsKey(XIConstant.ContentXIComponent))
                {
                    var control = oInstBase.oContent[XIConstant.ContentXIComponent];
                    var sType = control.GetType();
                    XIIComponent compI = new XIIComponent();
                    var oIns = new XIInstanceBase();
                    if (sType.Name == "XIInstanceBase")
                    {
                        oIns = (XIInstanceBase)oInstBase.oContent[XIConstant.ContentXIComponent];
                        compI = (XIIComponent)oIns.oContent[XIConstant.ContentXIComponent];
                    }
                    else if (sType.Name == "XIIComponent")
                    {
                        compI = (XIIComponent)oInstBase.oContent[XIConstant.ContentXIComponent];
                    }
                    //var comp = (XIIComponent)XiiComp.oContent[XIConstant.ContentXIComponent];
                    var compD = (XIDComponent)compI.oDefintion;
                    sRenderType = compD.sName;
                    if (compD.sName.ToLower() == XIConstant.OneClickComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_OneClickComponent.cshtml", compI);
                        compI.oContent[XIConstant.OneClickComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.FormComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_FormComponent.cshtml", compI);
                        compI.oContent[XIConstant.FormComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.TabComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_TabComponent.cshtml", compI);
                        compI.oContent[XIConstant.TabComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MenuComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuComponent.cshtml", compI);
                        compI.oContent[XIConstant.MenuComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.HTMLComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_HtmlRepeaterComponent.cshtml", compI);
                        compI.oContent[XIConstant.HTMLComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.GridComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_GridComponent.cshtml", compI);
                        compI.oContent[XIConstant.GridComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.InboxComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_InboxComponent.cshtml", compI);
                        compI.oContent[XIConstant.InboxComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.GroupComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_GroupComponent.cshtml", compI);
                        compI.oContent[XIConstant.GroupComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.ScriptComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_ScriptComponent.cshtml", compI);
                        compI.oContent[XIConstant.ScriptComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XilinkComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XILinkComponent.cshtml", compI);
                        compI.oContent[XIConstant.XilinkComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.LayoutComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutComponent.cshtml", compI);
                        compI.oContent[XIConstant.LayoutComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.LayoutDetailsComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutDetailsComponent.cshtml", compI);
                        compI.oContent[XIConstant.LayoutDetailsComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.LayoutMappingComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutMappingComponent.cshtml", compI);
                        compI.oContent[XIConstant.LayoutMappingComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.PieChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_PieChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.PieChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.CombinationChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_CombinationChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.CombinationChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.GaugeChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_GaugeChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.GaugeChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DashBoardChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DashBoardChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.DashBoardChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DailerComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DailerComponent.cshtml", compI);
                        compI.oContent[XIConstant.DailerComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QuoteReportDataComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QuoteReportDataComponent.cshtml", compI);
                        compI.oContent[XIConstant.QuoteReportDataComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MappingComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MappingComponent.cshtml", compI);
                        compI.oContent[XIConstant.MappingComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MultiRowComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MultiRowComponent.cshtml", compI);
                        compI.oContent[XIConstant.MultiRowComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIApplicationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_ApplicationComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIApplicationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.MenuNodeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuNodeComponent.cshtml", compI);
                        compI.oContent[XIConstant.MenuNodeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIUrlMappingComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIUrlMappingComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIUrlMappingComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DialogComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DialogComponent.cshtml", compI);
                        compI.oContent[XIConstant.DialogComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.FieldOriginComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_FieldOriginComponent.cshtml", compI);
                        compI.oContent[XIConstant.FieldOriginComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIParameterComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIParameterComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIParameterComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.DataTypeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_DataTypeComponent.cshtml", compI);
                        compI.oContent[XIConstant.DataTypeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIComponentComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIComponentComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIComponentComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOAttributeComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOAttributeComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOAttributeComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOScriptComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOScriptComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOScriptComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIBOStructureComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOStructureComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIBOStructureComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIInfraXIBOUIComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOUIDetailsComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIInfraXIBOUIComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.XIDataSourceComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIDataSourceComponent.cshtml", compI);
                        compI.oContent[XIConstant.XIDataSourceComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QueryManagementComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QueryManagementComponent.cshtml", compI);
                        compI.oContent[XIConstant.QueryManagementComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSConfigComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSConfigComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSConfigComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSStepConfigComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSStepConfigComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSStepConfigComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSSectionConfigComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSSectionConfigComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSSectionConfigComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.VisualisationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_VisualisationComponent.cshtml", compI);
                        compI.oContent[XIConstant.VisualisationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.ReportComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIReportComponent.cshtml", compI);
                        compI.oContent[XIConstant.ReportComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSLinkComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkComponentComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSLinkComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSLinkDefinationComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkDefinationComponent.cshtml", compI);
                        compI.oContent[XIConstant.QSLinkDefinationComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.QSComponent.ToLower())
                    {
                        var oQSI = (XIIQS)compI.oContent[XIConstant.QSComponent];
                        if (oQSI.Steps != null && oQSI.Steps.Count() > 0)
                        {
                            var oStepI = oQSI.Steps.FirstOrDefault().Value;
                            var oStepD = (XIDQSStep)oStepI.oDefintion;
                            if (oStepD.iLayoutID > 0)
                            {
                                //var oLayoutIns = oStepI.oContent[XIConstant.ContentLayout];
                                var oLayoutD = (XIDLayout)oStepI.oContent[XIConstant.ContentLayout];
                                for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
                                {
                                    MergeHTMLRecurrsive(oLayoutD.LayoutMappings[i]);
                                }
                            }
                            else if (oStepD.Sections.Count() > 0)
                            {
                                //TO DO : Load XIFields as other components loading
                                var DATAMerge = MergeHTML("~/views/XiLink/QuestionSet.cshtml", oQSI);
                                oQSI.oContent[XIConstant.ContentFields] = DATAMerge;
                            }
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4PieChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PieChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4PieChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4GaugeChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4GaugeChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4GaugeChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4SemiPieChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4SemiPieChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4SemiPieChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4BarChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4BarChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4BarChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4LineChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4LineChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4LineChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4HeatChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4HeatChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4HeatChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                    else if (compD.sName.ToLower() == XIConstant.AM4PriceChartComponent.ToLower())
                    {
                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PriceChartComponent.cshtml", compI);
                        compI.oContent[XIConstant.AM4PriceChartComponent] = DATAMerge;
                        if (sType.Name == "XIInstanceBase")
                        {
                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                            oInstBase.oContent[XIConstant.ContentXIComponent] = oIns;
                        }
                        else
                        {
                            oInstBase.oContent[XIConstant.ContentXIComponent] = compI;
                        }
                    }
                }
                else
                {
                    if (oInstBase.oContent.ContainsKey(XIConstant.ContentDialog))
                    {
                        var con = (XIDDialog)oInstBase.oContent[XIConstant.ContentDialog];
                        //var xifs = (XIDDialog)con.oContent.Values.FirstOrDefault();
                        MergeHTMLRecurrsive(con);
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentPopup))
                    {
                        var con = (XIDPopup)oInstBase.oContent[XIConstant.ContentPopup];
                        //var xifs = (XIDDialog)con.oContent.Values.FirstOrDefault();
                        MergeHTMLRecurrsive(con);
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentLayout))
                    {
                        var oLayoutD = (XIDLayout)oInstBase.oContent[XIConstant.ContentLayout];
                        for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
                        {
                            MergeHTMLRecurrsive(oLayoutD.LayoutMappings[i]);
                        }
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentXILink))
                    {
                        var XiLinkD = oInstBase.oContent[XIConstant.ContentXILink];
                        MergeHTMLRecurrsive(XiLinkD);
                    }
                    else if (oInstBase.oContent.ContainsKey(XIConstant.ContentStep))
                    {
                        var oStepI = (XIIQSStep)oInstBase.oContent[XIConstant.ContentStep];
                        if (oStepI.Sections != null && oStepI.Sections.Count() > 0)
                        {
                            var oSecI = oStepI.Sections.FirstOrDefault().Value;
                            if (oSecI != null)
                            {
                                var oSecD = (XIDQSSection)oSecI.oDefintion;
                                //var osecIns = new XIInstanceBase();
                                if (oSecI.oContent.ContainsKey(XIConstant.ContentXIComponent))
                                {
                                    //osecIns = (XIInstanceBase)oSecI.oContent[XIConstant.ContentXIComponent];
                                    var compI = (XIIComponent)oSecI.oContent[XIConstant.ContentXIComponent];
                                    var CompD = (XIDComponent)compI.oDefintion;
                                    sRenderType = CompD.sName;
                                    if (CompD.sName.ToLower() == XIConstant.OneClickComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_OneClickComponent.cshtml", compI);
                                        compI.oContent[XIConstant.OneClickComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.GroupComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_GroupComponent.cshtml", compI);
                                        compI.oContent[XIConstant.GroupComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.ScriptComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_ScriptComponent.cshtml", compI);
                                        compI.oContent[XIConstant.ScriptComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XilinkComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XILinkComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XilinkComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.LayoutComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutComponent.cshtml", compI);
                                        compI.oContent[XIConstant.LayoutComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.LayoutDetailsComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutDetailsComponent.cshtml", compI);
                                        compI.oContent[XIConstant.LayoutDetailsComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.LayoutMappingComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_LayoutMappingComponent.cshtml", compI);
                                        compI.oContent[XIConstant.LayoutMappingComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIApplicationComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_ApplicationComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIApplicationComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MenuNodeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuNodeComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MenuNodeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.FormComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_FormComponent.cshtml", compI);
                                        compI.oContent[XIConstant.FormComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XITreeStructure.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XITreeStructureView.cshtml", compI);
                                        compI.oContent[XIConstant.XITreeStructure] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.TabComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_TabComponent.cshtml", compI);
                                        compI.oContent[XIConstant.TabComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MenuComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MenuComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MenuComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.GridComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_GridComponent.cshtml", compI);
                                        compI.oContent[XIConstant.GridComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIUrlMappingComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIUrlMappingComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIUrlMappingComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DialogComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DialogComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DialogComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.FieldOriginComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_FieldOriginComponent.cshtml", compI);
                                        compI.oContent[XIConstant.FieldOriginComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIParameterComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIParameterComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIParameterComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DataTypeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DataTypeComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DataTypeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIComponentComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIComponentComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIComponentComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOAttributeComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOAttributeComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOAttributeComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOScriptComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOScriptComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOScriptComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIBOStructureComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOStructureComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIBOStructureComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIInfraXIBOUIComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIBOUIDetailsComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIInfraXIBOUIComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.XIDataSourceComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIDataSourceComponent.cshtml", compI);
                                        compI.oContent[XIConstant.XIDataSourceComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QueryManagementComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QueryManagementComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QueryManagementComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSConfigComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSConfigComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSConfigComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSStepConfigComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSStepConfigComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSStepConfigComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSSectionConfigComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSSectionConfigComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSSectionConfigComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.VisualisationComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_VisualisationComponent.cshtml", compI);
                                        compI.oContent[XIConstant.VisualisationComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.HTMLComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_HtmlRepeaterComponent.cshtml", compI);
                                        compI.oContent[XIConstant.HTMLComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.InboxComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_InboxComponent.cshtml", compI);
                                        compI.oContent[XIConstant.InboxComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.PieChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_PieChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.PieChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.CombinationChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_CombinationChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.CombinationChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.GaugeChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_GaugeChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.GaugeChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DashBoardChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DashBoardChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DashBoardChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.DailerComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_DailerComponent.cshtml", compI);
                                        compI.oContent[XIConstant.DailerComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QuoteReportDataComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QuoteReportDataComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QuoteReportDataComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MappingComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MappingComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MappingComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.MultiRowComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_MultiRowComponent.cshtml", compI);
                                        compI.oContent[XIConstant.MultiRowComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.ReportComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_XIReportComponent.cshtml", compI);
                                        compI.oContent[XIConstant.ReportComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSLinkComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkComponentComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSLinkComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.QSLinkDefinationComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_QSLinkDefinationComponent.cshtml", compI);
                                        compI.oContent[XIConstant.QSLinkDefinationComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4PriceChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PriceChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4PriceChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4PieChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4PieChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4PieChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4GaugeChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4GaugeChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4GaugeChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4SemiPieChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4SemiPieChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4SemiPieChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4BarChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4BarChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4BarChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4LineChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4LineChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4LineChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    else if (CompD.sName.ToLower() == XIConstant.AM4HeatChartComponent.ToLower())
                                    {
                                        var DATAMerge = MergeHTML("~/views/XIComponents/_AM4HeatChartComponent.cshtml", compI);
                                        compI.oContent[XIConstant.AM4HeatChartComponent] = DATAMerge;
                                        oSecI.oContent[XIConstant.ContentXIComponent] = compI;
                                    }
                                    //oSecI.oContent[XIConstant.ContentXIComponent] = osecIns;
                                    oStepI.Sections[oSecD.ID + "_Sec"] = oSecI;
                                }
                                else if (oSecI.XIValues != null && oSecI.XIValues.Count() > 0)
                                {
                                    var DATAMerge = MergeHTML("~/views/XIComponents/_QSField.cshtml", oSecI);
                                    oSecI.oContent[XIConstant.InboxComponent] = DATAMerge;
                                    oSecI.oContent[XIConstant.ContentXIComponent] = oSecI;
                                }
                            }
                            else
                            {
                                oStepI.Sections[0 + "_Sec"] = oSecI;
                            }
                        }
                        else if (oStepI.oContent.ContainsKey(XIConstant.ContentLayout))
                        {
                            var oLayoutD = (XIDLayout)oStepI.oContent[XIConstant.ContentLayout];
                            for (int i = 0; i < oLayoutD.LayoutMappings.Count(); i++)
                            {
                                MergeHTMLRecurrsive(oLayoutD.LayoutMappings[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Error at HTML Merge: Component Name: " + sRenderType + " and exception Message: " + ex.ToString(), "");
            }
            return oInstBase;
        }
        //[HttpPost]
        //public ActionResult LoadTabContent(string sGUID, string TabResultType, string i1ClickID, string sBO, string iInstanceID)
        //{
        //    string sDatabase = SessionManager.CoreDatabase;
        //    try
        //    {
        //        var sSessionID = HttpContext.Session.SessionID;
        //        var sBOName = oCache.Get_ParamVal(sSessionID, sGUID, null, "sBOName");
        //        var iInsID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{-iInstanceID}");
        //        var iBODID = oCache.Get_ParamVal(sSessionID, sGUID, null, "iBODID");
        //        var xiboins = Singleton.Instance.oFullData[sBOName + "_" + iInsID];
        //        if (TabResultType.ToLower() == "single")
        //        {
        //            if (xiboins != null)
        //            {
        //                var Prodata = xiboins.oSubStructureI(sBOName);
        //                if (Prodata != null && Prodata.oBOIList != null && Prodata.oBOIList.Count() > 0)
        //                {
        //                    var oboi = Prodata.oBOIList.FirstOrDefault();
        //                    XIIComponent oCompI = new XIIComponent();
        //                    XIBODisplay oBOIns = new XIBODisplay();
        //                    oboi.iBODID = Convert.ToInt32(iBODID);
        //                    oboi.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
        //                    oBOIns.BOInstance = oboi;
        //                    oCompI.oContent[XIConstant.FormComponent] = oBOIns;
        //                    return PartialView("~/views/xicomponents/_formcomponent.cshtml", oCompI);
        //                }
        //            }

        //        }
        //        else if (TabResultType.ToLower() == "multiple")
        //        {
        //            XIDComponent oXICompD = new XIDComponent();
        //            oXICompD = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, XIConstant.OneClickComponent, null);
        //            XIDComponent oXICompC = new XIDComponent();
        //            oXICompC = (XIDComponent)oXICompD.Clone(oXICompD);
        //            oXICompC.Params = new List<XIDComponentParam>();
        //            oXICompC.Params.Add(new XIDComponentParam { sName = "1ClickID", sValue = i1ClickID });
        //            XIIComponent oCompI = new XIIComponent();
        //            oCompI.sGUID = sGUID;
        //            oCompI.oDefintion = oXICompC;
        //            var oCompRes = oCompI.Load();
        //            if (oCompRes.bOK && oCompRes.oResult != null)
        //            {
        //                var oXiBase = ((XIIComponent)((XIInstanceBase)oCompRes.oResult).oContent[XIConstant.ContentXIComponent]).oContent[XIConstant.OneClickComponent];
        //                oCompI.oContent[XIConstant.OneClickComponent] = oXiBase;
        //                return PartialView(oXICompC.sHTMLPage, oCompI);
        //            }
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return null;
        //    }
        //}

        public ActionResult LoadStep(string sStep, int iQSID, int i1ClickID, string sDefaultStep, int iInstanceID = 0, string sGUID = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            XIIQSStep oStepICont = new XIIQSStep();
            try
            {
                XIDQSStep oQSStepD = new XIDQSStep();
                long iStepID = 0;
                string sStepName = sStep;
                long.TryParse(sStepName, out iStepID);
                XIInfraCache oCache = new XIInfraCache();
                XIIQSStep oQSStepI = new XIIQSStep();
                //XIDQSStep oQSStepD = new XIDQSStep();
                XIDQS oQSD = new XIDQS();
                oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSID.ToString());

                if (oQSD != null && oQSD.Steps.Count() > 0)
                {
                    if (iStepID > 0)
                    {
                        oQSStepD = oQSD.Steps.ToList().Where(m => m.Value.ID == iStepID).FirstOrDefault().Value;
                    }
                    else
                    {
                        oQSStepD = oQSD.Steps.Values.Where(m => m.sName != null && m.sName != "" && m.sName.ToLower() == sStepName.ToLower()).FirstOrDefault();
                        //oQSStepD = oQSD.Steps[sStepName];
                    }
                    //if (oQSD.Steps.ContainsKey(sStepName))
                    //var oQSStepData = oQSD.Steps.ToList().Where(m => m.Value.ID == iStepID).FirstOrDefault().Value;
                    if (oQSStepD != null)
                    {
                        //XIDQSStep oQSStepD = oQSD.Steps[sStepName];
                        // XIDQSStep oQSStepD = oQSStepData;
                        if (oQSStepD != null)
                        {
                            var result = oQSStepD.Clone(oQSStepD);
                            XIDQSStep oQSStepC = (XIDQSStep)oQSStepD.Clone(oQSStepD);
                            oQSStepI.oDefintion = oQSStepC;
                            oQSStepI.sGUID = sGUID;
                            var oResult = oQSStepI.Load();
                            if (oResult.bOK && oResult.oResult != null)
                            {
                                oStepICont = (XIIQSStep)oResult.oResult;
                                XIInstanceBase oIns = new XIInstanceBase();
                                oIns.oContent[XIConstant.ContentStep] = oStepICont;
                                dynamic oMergedData = MergeHTMLRecurrsive(oIns);
                                var StepData = oMergedData.oContent[XIConstant.ContentStep];
                                return Json(StepData, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                //string json = JsonConvert.SerializeObject(oInstance, Newtonsoft.Json.Formatting.Indented);
                //var oDeserializeData = Deserialise(json);
                //oInstance = (XIInstanceBase)oDeserializeData.Values.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return Json(oStepICont, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TriggerStep(string sStep, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIQSStep oStepICont = new XIIQSStep();
                int iStepID = 0;
                if (sStep.Contains('_'))
                {
                    var oDetails = sStep.Split('_');
                    if (oDetails != null && oDetails.Count() == 2)
                    {
                        int.TryParse(oDetails[1], out iStepID);
                        if (iStepID > 0)
                        {
                            XIIQSStep oStepI = new XIIQSStep();
                            oStepI.sGUID = sGUID;
                            oStepI.FKiQSStepDefinitionID = iStepID;
                            var oResult = oStepI.Load();
                            if (oResult.bOK && oResult.oResult != null)
                            {
                                oStepICont = (XIIQSStep)oResult.oResult;
                                XIInstanceBase oIns = new XIInstanceBase();
                                oIns.oContent[XIConstant.ContentStep] = oStepICont;
                                dynamic oMergedData = MergeHTMLRecurrsive(oIns, sGUID);
                                var StepData = oMergedData.oContent[XIConstant.ContentStep];
                                return Json(StepData, JsonRequestBehavior.AllowGet);
                            }
                            return Json(oStepICont, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion XILinkLoad

        public void PostTransactions()
        {
            try
            {

                XIPFServiceDev wrapper = new XIPFServiceDev();
                var oParams = new List<CNV>();
                wrapper.PFNewBusiness(oParams);
                //JournalTransactions jr = new JournalTransactions();
                //jr.PostTransaction("NEWP", 3000, 1059, false);
                //var data = jr.PostTransaction("CREC", 1000, 540, false);
                //var _data = jr.PostTransaction("ACSI", 40, 0, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult PFBNP()
        {
            try
            {
                CResult oCResult = new CResult();
                //var cacheobj = HttpRuntime.Cache.GetEnumerator();
                //int cachecount = 0;
                //List<string> lsterrors = new List<string>();
                //List<string> lstkeys = new List<string>();
                //long bytes = 0;
                //while (cacheobj.MoveNext())
                //{
                //    var pair = (DictionaryEntry)cacheobj.Current;
                //    cachecount++;
                //    lstkeys.Add(pair.Key.ToString() + "_" + pair.Value.GetType().ToString());
                //    try
                //    {
                //        using (Stream s = new MemoryStream())
                //        {
                //            BinaryFormatter formatter = new BinaryFormatter();
                //            formatter.Serialize(s, pair.Value);
                //            bytes = bytes + s.Length;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        lsterrors.Add(ex.Message.ToString());
                //    }
                //}

                var sServerKey = System.Configuration.ConfigurationManager.AppSettings["ServerEnvironment"];
                var oParams = new List<CNV>();
                //wrapper.PFNewBusiness(oParams);
                Dictionary<string, XIPFfields> dictlst = new Dictionary<string, XIPFfields>();
                string strxml = "<?xml version=\"1.0\" ?><BNPRequest><LiveRateRequestFlag>true</LiveRateRequestFlag></BNPRequest>";
                dictlst.Add("LiveRateRequestFlag", new XIPFfields { bMandatory = true, sType = "System.Boolean", sFiledName = "LiveRateRequestFlag", sErrorMessage = "$KEY$ Please fill {skey} | $DATA$ Invalid Data for {skey} |$REGEXP$ Please give valid {skey}" });
                if (sServerKey.ToLower() == "dev")
                {
                    XIBNPFileWrapper wrapper = new XIBNPFileWrapper();
                    oCResult.oResult = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.LiveRateQuote, strxml, dictlst);
                }
                else if (sServerKey.ToLower() == "live")
                {
                    XIBNPFileWrapperLive wrapper = new XIBNPFileWrapperLive();
                    oCResult.oResult = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.LiveRateQuote, strxml, dictlst);
                }

                // string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\NewBusinessReq_test2.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.NewBusiness, strxml);

                // XIPFServices wrapper = new XIPFServices(); // REQUEST FROM IPMSYSTEM
                //string strxml = "<?xml version=\"1.0\" ?><BNPRequest><ProvideStatusPF>true</ProvideStatusPF></BNPRequest>";
                // string strxml = "<?xml version=\"1.0\" ?><BNPRequest><LiveRateRequestFlagPF>true</LiveRateRequestFlagPF></BNPRequest>";

                // var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.LiveRateQuote, strxml);

                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\Cancellation.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.Cancellation, strxml);

                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\EditCustomerAddress.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.EditCustomerAddress, strxml);


                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\EditCustomerBank.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.EditCustomerBank, strxml);

                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\EditCustomerEmail.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.EditCustomerEmail, strxml);

                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\EditCustomerPaymentDay.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.EditCustomerPaymentDay, strxml);

                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\EditCustomerPhone.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.EditCustomerPhone, strxml);

                //string strxml = System.IO.File.ReadAllText(@"E:\Documents\XISystems\BNPRequestXMLs\MidTermAdjustment.txt");
                //var resultobj = wrapper.LoadXIBNPAPI(EnumXIBNPMethods.MidTermAdjustment, strxml);
                return View("PFProducts", oCResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private T GetInstance<T>()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
        public bool VerifyConnection()
        {
            dynamic xibnpapi = GetInstance<XIBNPAPI>();
            var Result = xibnpapi.VerifyBNPConnectionStatus();
            return Result;
        }
        public ActionResult AgeDebt(int Short = 0, int Medium = 0, int Long = 0, string Date = "", string Select = "", string sCode = "")
        {
            CResult oCResult = new CResult();
            Dictionary<string, XIIValue> XIValues = new Dictionary<string, XIIValue>();
            XIValues.Add("iShort", new XIIValue { sValue = Short.ToString() });
            XIValues.Add("iMedium", new XIIValue { sValue = Medium.ToString() });
            XIValues.Add("iLong", new XIIValue { sValue = Long.ToString() });
            XIValues.Add("dAsOfDate", new XIIValue { sValue = Date.ToString() });
            XIValues.Add("oSelect", new XIIValue { sValue = Select.ToString() });
            TrailBalance oTrailBalance = new TrailBalance();
            if (sCode == "AGEDDEBT")
            {
                oCResult = oTrailBalance.AGEDebt(XIValues);
            }
            else
            {
                oCResult = oTrailBalance.AGECreditors(XIValues);
            }
            return PartialView("_DataView", oCResult.oResult);
        }
        //public void AgeCredit()
        //{
        //    TrailBalance oTrailBalance = new TrailBalance();
        //    oTrailBalance.AGECreditors();
        //}
        public ActionResult MakeaChange(string sInstanceID, string sType)
        {
            string sMessage = string.Empty;
            try
            {
                int iUserID = SessionManager.UserID;
                XIInfraNotifications oNotifications = new XIInfraNotifications();
                oNotifications.iUserID = iUserID;
                oNotifications.sInstanceID = sInstanceID;
                oNotifications.iStatus = 10;
                var oResponse = oNotifications.Create(Convert.ToString(iUserID), sType, oNotifications.iDocumentID.ToString());
                if (oResponse.bOK && oResponse.oResult != null)
                {
                    sMessage = "Your request has sent to admin,admin will contact against to your request";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
                return null;
            }
            return Json(sMessage, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveFiles(int ID, int BOFieldID, string sInstanceID, string sFileAliasName, string sParentID, int iBuildingID, string sFolderName, List<HttpPostedFileBase> UploadImage, string sNewGUID = null, int iDocumentID = 0)
        {
            CResult oCResult = new CResult();
            CResult oResult = new CResult(); // always
            XIDefinitionBase oXIDB = new XIDefinitionBase();
            try
            {
                long iTraceLevel = 10;
                oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
                oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
                if (iTraceLevel > 0)
                {
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
                }
                //if iTraceLevel>0 then 
                //oCResult.oTraceStack.Trace("Stage", "Begin query build",milliseconds)
                //oCResult.oTraceStack.Trace("Stage",sError)
                //end if
                if (oResult.xiStatus == xiEnumSystem.xiFuncResult.xiError)
                {
                    oCResult.xiStatus = oResult.xiStatus;
                    //oCResult.oTraceStack.Trace("Stage",sError)
                }
                var sNotUploaded = new List<string>();
                int iStatus = 0;
                int iSavedToDoc = 0;
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                string sDatabase = SessionManager.OrgDatabase;
                int iOrgID = SessionManager.OrganizationID;
                ModelDbContext dbContext = new ModelDbContext();
                //ModelDbContext sdbContext = new ModelDbContext(sDatabase);
                //var sOrgDB = Common.GetUserDetails(iUserID, sOrgName, sDatabase).sUserDatabase;
                //BOFields BO = new BOFields();
                string DocumentID = "";
                var CIFDocID = string.Empty;
                string sBOName = string.Empty;
                string sValues = string.Empty;
                string sLabelValue = "";
                var iVersionBatchID = "0";
                var cType = "";
                var iOldDocID = "0";
                sFolderName = sFolderName.Replace(@"//", @"\");
                sFolderName = sFolderName.Replace(@"$", @"\");
                XIIXI oXI = new XIIXI();
                if (ID > 0 || ID == 0)
                {
                    BOFields BODetails = dbContext.BOFields.Find(BOFieldID);
                    int FileTypeID = BODetails.FKiFileTypeID;
                    int BOID = BODetails.BOID;
                    string sFieldName = BODetails.Name;
                    sBOName = dbContext.BOs.Where(m => m.BOID == BOID).Select(m => m.Name).FirstOrDefault();
                    string sTableName = dbContext.BOs.Where(m => m.Name == sBOName).Select(m => m.TableName).FirstOrDefault();
                    var oInstanceBOII = oXI.BOI(sBOName, sInstanceID);
                    if (oInstanceBOII != null)
                    {
                        iUserID = oInstanceBOII.AttributeI("FKiUserID").iValue;
                    }
                    XIFileTypes FileDetails = dbContext.XIFileTypes.Find(FileTypeID);
                    string FileType = FileDetails.Type;
                    //Check uploaded file format and size before saving
                    var bIsSave = CheckFilesAllowedToSave(FileDetails.FileType, UploadImage);
                    if (!bIsSave)
                    {
                        return Content("-1");
                    }
                    int iDocID = 0;
                    string sFileFormat = string.Empty;
                    string sNewFileName = string.Empty;
                    var sDocIDList = new List<string>();
                    if (FileType == "10")
                    {
                        int iDeletDocIDifNull = 0;
                        foreach (var items in UploadImage)
                        {
                            try
                            {
                                //First save the empty file name and get the docID
                                string sDFileName = "";
                                string sID = string.Empty;
                                string NewGUID = Guid.NewGuid().ToString();
                                XIIBO oBOI = new XIIBO();
                                XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Documents_T", null); //oXID.Get_BODefinition("Documents_T").oResult;
                                oBOI.BOD = oBOD;
                                oBOI.Attributes["ID"] = new XIIAttribute { sName = "ID", sValue = null, bDirty = true };
                                oBOI.Attributes["FileName"] = new XIIAttribute { sName = "FileName", sValue = sDFileName, bDirty = true };
                                oBOI.Attributes["FKiDocType"] = new XIIAttribute { sName = "FKiDocType", sValue = "0", bDirty = true };
                                oBOI.Attributes["dCreatedTime"] = new XIIAttribute { sName = "dCreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                                oBOI.Attributes["dUpdatedTime"] = new XIIAttribute { sName = "dUpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                                oBOI.Attributes["FKiUserID"] = new XIIAttribute { sName = "FKiUserID", sValue = iUserID.ToString(), bDirty = true };
                                oBOI.Attributes["iInstanceID"] = new XIIAttribute { sName = "iInstanceID", sValue = sInstanceID, bDirty = true };
                                oBOI.Attributes["FKiBOID"] = new XIIAttribute { sName = "FKiBOID", sValue = BOID.ToString(), bDirty = true };
                                oBOI.Attributes["XIGUID"] = new XIIAttribute { sName = "XIGUID", sValue = NewGUID, bDirty = true };
                                oBOI.Attributes["bIsVisibleToUser"] = new XIIAttribute { sName = "bIsVisibleToUser", sValue = "1", bDirty = true };
                                var Response = oBOI.Save(oBOI);//to save 
                                XIIBO oBOInstance = new XIIBO();
                                if (Response.bOK && Response.oResult != null)
                                {
                                    oBOInstance = (XIIBO)Response.oResult;
                                }
                                if (oBOInstance != null)
                                {
                                    sID = oBOInstance.Attributes.Where(x => x.Key.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                                }
                                string sNewImageName = "";
                                if (!string.IsNullOrEmpty(sID))
                                {
                                    iDocID = Convert.ToInt32(sID);
                                }
                                iDeletDocIDifNull = iDocID;
                                sLabelValue = iDocID.ToString();
                                DocumentID = DocumentID + iDocID + ',';
                                //get the details of filename form uploaded file
                                var sImageName = items.FileName;
                                string[] sFormat = sImageName.Split('.');
                                sFileFormat = sFormat[1];
                                //create a new filename
                                if (iDocID > 0)
                                {
                                    sNewImageName = NewGUID;
                                }
                                string sNewPathForSubDir = "";
                                int iDocTypeID = 0;
                                string sNewPath = "";
                                List<CNV> oNVList = new List<CNV>();
                                CNV oNV = new CNV();
                                oNV.sName = "";
                                oNV.sValue = sFileFormat;
                                oNVList.Add(oNV);
                                List<XIDocTypes> DocTypes = dbContext.XIDocTypes.Where(m => m.Type == sFileFormat).ToList();
                                int iFileTypeID = Convert.ToInt32(FileDetails.FileType);
                                string sFileTypeCheck = dbContext.XIDocTypes.Where(m => m.ID == iFileTypeID).Select(m => m.Type).FirstOrDefault();
                                //Check if the file format matches the doctype details
                                if (sFileTypeCheck.ToLower() != sFileFormat.ToLower())
                                {
                                    //do nothing as file format dosnot match
                                    sNotUploaded.Add(items.FileName);
                                }
                                else
                                {
                                    List<string> sNewPathDetails = CheckAndCreateDirectory(DocTypes, false);
                                    for (var i = 0; i < sNewPathDetails.Count(); i++)
                                    {
                                        iDocTypeID = Convert.ToInt32(sNewPathDetails[0]);
                                        sNewPathForSubDir = sNewPathDetails[1];
                                        sNewPath = sNewPathDetails[2];
                                    }
                                    Common.SaveErrorLog("File Saving Path: " + sNewPath + "\\" + sNewImageName + "." + sFileFormat, "XIDNA");
                                    //Virtual Directory Saving
                                    //string saveFile = Path.Combine("~\\Content\\Uploads\\", sNewImageName + "." + sImageFormat);
                                    //Common.SaveErrorLog("File Saving Path: " + Server.MapPath(saveFile), "XIDNA");
                                    //items.SaveAs(Server.MapPath(saveFile));
                                    items.SaveAs(sNewPath + "\\" + sNewImageName + "." + sFileFormat);
                                    //Aspect ratio
                                    //Get max and min height of image from xi filesetttings
                                    var iMaxWidth = Convert.ToInt32(FileDetails.MaxWidth);
                                    var iMaxHeight = Convert.ToInt32(FileDetails.MaxHeight);
                                    using (var image = Image.FromFile(sNewPath + "\\" + sNewImageName + "." + sFileFormat))
                                    using (var newImage = ScaleImage(image, iMaxWidth, iMaxHeight))
                                    {
                                        string sImgNme = sNewImageName.Replace("_Org", "");
                                        newImage.Save(sNewPath + "\\" + sImgNme + "." + sFileFormat);
                                    }
                                    try
                                    {
                                        //check DocID and update the details
                                        if (iDocID > 0)
                                        {
                                            oBOI = new XIIBO();
                                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Documents_T", null);
                                            oBOI.BOD = oBOD;
                                            oBOI.Attributes["ID"] = new XIIAttribute { sName = "ID", sValue = iDocID.ToString(), bDirty = true };
                                            oBOI.Attributes["FileName"] = new XIIAttribute { sName = "FileName", sValue = sNewImageName.Replace("_Org", "") + "." + sFileFormat, bDirty = true };
                                            oBOI.Attributes["FKiDocType"] = new XIIAttribute { sName = "FKiDocType", sValue = iDocTypeID.ToString(), bDirty = true };
                                            oBOI.Attributes["SubDirectoryPath"] = new XIIAttribute { sName = "SubDirectoryPath", sValue = sNewPathForSubDir, bDirty = true };
                                            oBOI.Attributes["iSystemgeneratedorUploaded"] = new XIIAttribute { sName = "iSystemgeneratedorUploaded", sValue = "20", bDirty = true };
                                            if (string.IsNullOrEmpty(sFileAliasName))
                                            {
                                                sFileAliasName = sImageName;
                                            }
                                            oBOI.Attributes["sAliasName"] = new XIIAttribute { sName = "sAliasName", sValue = sFileAliasName + "." + sFileFormat, bDirty = true };
                                            oBOI.Attributes["sName"] = new XIIAttribute { sName = "sName", sValue = sImageName, bDirty = true };
                                            var Result = oBOI.Save(oBOI);
                                            iSavedToDoc = 1;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While File saving" });
                                        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                        oCResult.LogToFile();
                                        Common.SaveErrorLog(oCResult.sMessage, "XIDNA");
                                    }
                                    //if (FileType == "10")
                                    //{
                                    if (iSavedToDoc == 1)
                                    {
                                        if (FileDetails.Thumbnails == "10")
                                        {
                                            int iThHeight = Convert.ToInt32(FileDetails.ThumbHeight);
                                            int iThWidth = Convert.ToInt32(FileDetails.ThumbWidth);
                                            Image image = Image.FromFile(sNewPath + "\\" + "images_" + iOrgID + "_" + iUserID + "_" + iDocID + "_Org." + sFileFormat);
                                            using (var thumbImage = ThumbImage(image, iThWidth, iThHeight))
                                            {
                                                string sImgNme = sNewImageName.Replace("_Org", "");
                                                thumbImage.Save(sNewPath + "\\" + sImgNme + "_thumb." + sFileFormat);
                                            }

                                        }

                                        if (FileDetails.Preview == "10")
                                        {
                                            int iPrevHeight = Convert.ToInt32(FileDetails.PreviewHeight);
                                            int iPrevWidth = Convert.ToInt32(FileDetails.PreviewWidth);
                                            Image image = Image.FromFile(sNewPath + "\\" + "images_" + iOrgID + "_" + iUserID + "_" + iDocID + "." + sFileFormat);
                                            // Image prev = image.GetThumbnailImage(iPrevHeight, iPrevWidth, () => false, IntPtr.Zero);
                                            //thumb.Save(Path.ChangeExtension(sPath + "\\" + "images_" + OrgID + "_" + ID + "." + sImageFormat, "thumb"));
                                            // prev.Save(sPath + "\\" + sNewImageName + "_prev." + sImageFormat);
                                            using (var newImage = ScaleImage(image, iPrevWidth, iPrevHeight))
                                            {
                                                string sImgNme = sNewImageName.Replace("_Org", "");
                                                newImage.Save(sNewPath + "\\" + sImgNme + "_prev." + sFileFormat);
                                            }
                                        }

                                        if (FileDetails.Drilldown == "10")
                                        {
                                            int iDrillHeight = Convert.ToInt32(FileDetails.DrillHeight);
                                            int iDrillWidth = Convert.ToInt32(FileDetails.DrillWidth);
                                            Image image = Image.FromFile(sNewPath + "\\" + "images_" + iOrgID + "_" + iUserID + "_" + iDocID + "." + sFileFormat);
                                            using (var newImage = ScaleImage(image, iDrillWidth, iDrillHeight))
                                            {
                                                string sImgNme = sNewImageName.Replace("_Org", "");
                                                newImage.Save(sNewPath + "\\" + sImgNme + "_drill." + sFileFormat);
                                            }
                                        }
                                    }
                                }
                                //Add Doc ID where the image name is saved to a list.
                                sDocIDList.Add(iDocID.ToString());
                            }//try

                            catch (Exception ex)
                            {
                                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While File saving" });
                                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                oCResult.LogToFile();
                                oXIDB.SaveErrortoDB(oCResult);
                                if (iDocID > 0)
                                {
                                    //XIDocs Doc = sdbContext.XIDocs.Find(iDocID);
                                    //sdbContext.XIDocs.Remove(Doc);
                                    //sdbContext.SaveChanges();
                                    XIIBO oDeleteBOI = new XIIBO();
                                    oDeleteBOI = oXI.BOI("Documents_T", iDocID.ToString());
                                    if (oDeleteBOI != null)
                                    {
                                        oDeleteBOI.Delete(oDeleteBOI);
                                    }
                                    DocumentID = "";
                                }
                            }
                        }//end upload image for
                         //save the Doc ID where the file name is stored to perticular table
                         //check the not uploaded files
                        if (sNotUploaded.Count == 0)
                        {

                            if (Convert.ToInt32(sInstanceID) != 0)
                            {
                                if (oInstanceBOII.BOD != null)
                                {
                                    int iDataSourceID = oInstanceBOII.BOD.iDataSource;
                                    XIDXI oXID = new XIDXI();
                                    string sConnection = oXID.GetBODataSource(iDataSourceID, oInstanceBOII.BOD.FKiApplicationID);
                                    if (!string.IsNullOrEmpty(sConnection))
                                    {
                                        //check if the table with field has a image ID.
                                        using (SqlConnection Con = new SqlConnection(sConnection))
                                        {
                                            Con.Open();
                                            SqlCommand cmd = new SqlCommand();
                                            cmd.Connection = Con;
                                            //if (sTableName != "Reports")
                                            //{
                                            //    //Con.ChangeDatabase(sOrgDB);
                                            //}
                                            cmd.CommandText = "SELECT " + sFieldName + " FROM " + sTableName + " WHERE ID=" + sInstanceID;
                                            SqlDataReader reader = cmd.ExecuteReader();
                                            string sDocID = "";
                                            while (reader.Read())
                                            {
                                                sDocID = reader.IsDBNull(0) ? null : reader.GetValue(0).ToString();
                                            }
                                            Con.Close();
                                            string sNewDocID = "";
                                            if (!string.IsNullOrEmpty(sDocID))
                                            {
                                                string sDocIDs = string.Join(",", sDocIDList.Select(x => x.ToString()).ToArray());
                                                sNewDocID = sDocID + "," + sDocIDs;
                                            }
                                            else
                                            {
                                                sNewDocID = string.Join(",", sDocIDList.Select(x => x.ToString()).ToArray());
                                            }
                                            Con.Open();
                                            SqlCommand cmd1 = new SqlCommand();
                                            cmd1.Connection = Con;
                                            //if (sTableName != "Reports")
                                            //{
                                            //    Con.ChangeDatabase(sOrgDB);
                                            //}
                                            cmd1.CommandText = "UPDATE " + sTableName + " SET " + sFieldName + "='" + sNewDocID + "' WHERE ID=" + sInstanceID;
                                            cmd1.ExecuteNonQuery();
                                            Con.Close();
                                            DocumentID = sNewDocID;
                                        }
                                    }
                                }
                                iStatus = 1;
                            }
                        }
                        else
                        {
                            iStatus = 0;
                            if (iDeletDocIDifNull != 0)
                            {
                                //XIDocs Doc = sdbContext.XIDocs.Find(iDeletDocIDifNull);
                                //sdbContext.XIDocs.Remove(Doc);
                                //sdbContext.SaveChanges();
                                XIIBO oDeleteBOI = new XIIBO();
                                oDeleteBOI = oXI.BOI("Documents_T", iDeletDocIDifNull.ToString());
                                if (oDeleteBOI != null)
                                {
                                    oDeleteBOI.Delete(oDeleteBOI);
                                }
                                DocumentID = "";
                            }

                        }

                    }
                    else if (FileType == "20")
                    {
                        string sID = string.Empty;
                        int iDeleteDocIfNull = 0;
                        foreach (var items in UploadImage)
                        {
                            try
                            {
                                //First save the empty file name and get the docID
                                string sDFileName = "";
                                //XIDocs Docs = new XIDocs();
                                //Docs.FileName = sDFileName;
                                //Docs.FKiDocType = 0;
                                //Docs.FKiUserID = iUserID;
                                //Docs.dCreatedTime = DateTime.Now;
                                //Docs.dUpdatedTime = DateTime.Now;
                                //if (!string.IsNullOrEmpty(sInstanceID))
                                //{
                                //    Docs.FKiACPolicyID = Convert.ToInt32(sInstanceID);
                                //}
                                ////dbContext = new ModelDbContext(sDatabase);
                                //sdbContext.XIDocs.Add(Docs);
                                //sdbContext.SaveChanges();

                                string sSessionID = HttpContext.Session.SessionID;
                                var ISS = oCache.Get_ParamVal(sSessionID, sNewGUID, null, "|XIParent");
                                XICacheInstance parentparams = new XICacheInstance();
                                if (!string.IsNullOrEmpty(ISS))
                                {
                                    parentparams = oCache.GetAllParamsUnderGUID(sSessionID, ISS, null);
                                }
                                else
                                {
                                    parentparams = oCache.GetAllParamsUnderGUID(sSessionID, sNewGUID, null);
                                }
                                string NewGUID = Guid.NewGuid().ToString();
                                XIIBO oBOI = new XIIBO();
                                XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Documents_T", null);
                                oBOI.BOD = oBOD;
                                oBOI.Attributes["ID"] = new XIIAttribute { sName = "ID", sValue = null, bDirty = true };
                                oBOI.Attributes["FileName"] = new XIIAttribute { sName = "FileName", sValue = sDFileName, bDirty = true };
                                oBOI.Attributes["FKiDocType"] = new XIIAttribute { sName = "FKiDocType", sValue = "0", bDirty = true };
                                oBOI.Attributes["dCreatedTime"] = new XIIAttribute { sName = "dCreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                                oBOI.Attributes["dUpdatedTime"] = new XIIAttribute { sName = "dUpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                                oBOI.Attributes["FKiUserID"] = new XIIAttribute { sName = "FKiUserID", sValue = parentparams.NMyInstance.Where(x => x.Key.ToLower() == "{XIP|FKiUserID}".ToLower()).Select(t => t.Value.sValue).FirstOrDefault(), bDirty = true };
                                oBOI.Attributes["iInstanceID"] = new XIIAttribute { sName = "iInstanceID", sValue = sInstanceID, bDirty = true };
                                oBOI.Attributes["FKiBOID"] = new XIIAttribute { sName = "FKiBOID", sValue = BOID.ToString(), bDirty = true };
                                oBOI.Attributes["bIsVisibleToUser"] = new XIIAttribute { sName = "bIsVisibleToUser", sValue = "1", bDirty = true };
                                oBOI.Attributes["XIGUID"] = new XIIAttribute { sName = "XIGUID", sValue = NewGUID, bDirty = true };
                                var Response = oBOI.Save(oBOI);//to save 
                                XIIBO oBOInstance = new XIIBO();
                                if (Response.bOK && Response.oResult != null)
                                {
                                    oBOInstance = (XIIBO)Response.oResult;
                                }
                                if (oBOInstance != null)
                                {
                                    sID = oBOInstance.Attributes.Where(x => x.Key.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                                }
                                if (!string.IsNullOrEmpty(sID))
                                {
                                    iDocID = Convert.ToInt32(sID);
                                }
                                DocumentID = DocumentID + iDocID + ',';
                                iDeleteDocIfNull = iDocID;
                                //get the details of filename form uploaded file
                                var sFileName = items.FileName;
                                var LastIndexOfDot = sFileName.LastIndexOf('.');
                                var FormatwithDot = sFileName.Substring(LastIndexOfDot, sFileName.Length - LastIndexOfDot);
                                var sFormat = FormatwithDot.Replace(".", "");
                                var sFLname = sFileName.Substring(0, LastIndexOfDot);
                                //string[] sFormat = sFileName.Split('.');
                                sFileFormat = sFormat;
                                //create a new filename
                                sNewFileName = "";
                                bool bIsCreateIF = false;
                                if (iDocID > 0)
                                {
                                    sNewFileName = NewGUID;
                                }
                                if (sBOName.ToLower() == "XIDocumentTree".ToLower())
                                {
                                    sNewFileName = sFLname;
                                    bIsCreateIF = true;
                                }
                                string sNewPathForSubDir = "";
                                int iDocTypeID = 0;
                                string sNewPath = "";
                                List<XIDocTypes> DocTypes = dbContext.XIDocTypes.Where(m => m.Type.ToLower() == sFileFormat.ToLower()).ToList();
                                //int iFileTypeID = Convert.ToInt32(FileDetails.FileType);
                                List<int> oFileTypeList = new List<int>();
                                if (!string.IsNullOrEmpty(FileDetails.FileType))
                                {
                                    List<string> sFileTypeList = FileDetails.FileType.Split(',').ToList();
                                    oFileTypeList = sFileTypeList.ConvertAll(int.Parse);
                                }
                                //string sFileTypeCheck = dbContext.XIDocTypes.Where(m => m.ID == iFileTypeID).Select(m => m.Type).FirstOrDefault();
                                //string sFileTypeCheck = dbContext.XIDocTypes.Where(m => oFileTypeList.Contains(m.ID)).Select(m => m.Type).FirstOrDefault();
                                List<string> sFileTypeCheck = dbContext.XIDocTypes.Where(m => oFileTypeList.Contains(m.ID)).Select(m => m.Type.ToLower()).ToList();
                                //Check if the file format matches the doctype details
                                //if (sFileTypeCheck.ToLower() != sImageFormat.ToLower())
                                if (!sFileTypeCheck.Contains(sFileFormat.ToLower()))
                                {
                                    //do nothing as file format dosnot match
                                    sNotUploaded.Add(items.FileName);
                                }
                                else
                                {
                                    //call method to check and create directory
                                    var CIFPath = string.Empty;
                                    List<string> sNewPathDetails = CheckAndCreateDirectory(DocTypes, bIsCreateIF);
                                    if (sNewPathDetails == null)
                                    {
                                        sNotUploaded.Add(items.FileName);
                                        DocumentID = "-1";
                                    }

                                    if (sBOName.ToLower() == "XIDocumentTree".ToLower())
                                    {
                                        sFileAliasName = sFLname;
                                        int iVersion = 0;
                                        var sVersion = "0";
                                        string sName = string.Empty;
                                        var sDocID = "";
                                        CIFPath = @"\CreateIF\PDF\Client1\Project1\" + sFolderName + @"\";
                                        List<CNV> WhrParams = new List<CNV>();
                                        WhrParams.Add(new CNV { sName = "sParentID", sValue = sParentID });
                                        WhrParams.Add(new CNV { sName = "iBuildingID", sValue = iBuildingID.ToString() });
                                        WhrParams.Add(new CNV { sName = "id", sValue = iDocumentID.ToString() });
                                        XIIXI oXII = new XIIXI();
                                        var oBOII = oXI.BOI("XIDocumentTree", null, null, WhrParams);
                                        var bIsNew = true;
                                        if (oBOII != null && oBOII.Attributes.Values.Count() > 0)
                                        {
                                            bIsNew = false;
                                            if (oBOII.Attributes.ContainsKey("id"))
                                            {
                                                iOldDocID = oBOII.Attributes["id"].sValue;
                                            }
                                            if (oBOII.Attributes.ContainsKey("sversion"))
                                            {
                                                sVersion = oBOII.Attributes["sversion"].sValue;
                                            }
                                            if (oBOII.Attributes.ContainsKey("sName"))
                                            {
                                                sName = oBOII.Attributes["sName"].sValue;
                                            }
                                            if (oBOII.Attributes.ContainsKey("sPath"))
                                            {
                                                sDocID = oBOII.Attributes["sPath"].sValue;
                                            }
                                            if (oBOII.Attributes.ContainsKey("iVersionBatchID"))
                                            {
                                                iVersionBatchID = oBOII.Attributes["iVersionBatchID"].sValue;
                                            }
                                            if (sVersion != "0")
                                            {
                                                int.TryParse(sVersion, out iVersion);
                                                iVersion = iVersion + 1;
                                            }
                                            else if (sVersion == "0")
                                            {
                                                iVersion = 1;
                                            }
                                            sNewFileName = sName;
                                            sFileAliasName = sName;
                                            sFileName = sName;
                                            sName = "v" + iVersion + "_" + sName;
                                            oBOII.SetAttribute("sName", sName);
                                            oBOII.SetAttribute(XIConstant.Key_XIDeleted, "1");
                                            oBOII.SetAttribute("sDocType", "Version");
                                            oBOII.Save(oBOII);
                                            var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                                            var sVirtualPath = @"~\" + sVirtualDir + @"\";
                                            var sVirtualPathOld = Server.MapPath(sVirtualPath) + @"\" + CIFPath + sNewFileName + "." + sFileFormat;
                                            var sVirtualPathNew = Server.MapPath(sVirtualPath) + @"\" + CIFPath + sName + '.' + sFileFormat;
                                            System.IO.File.Copy(sVirtualPathOld, sVirtualPathNew);


                                            WhrParams = new List<CNV>();
                                            WhrParams.Add(new CNV { sName = "ID", sValue = sDocID });
                                            var oBOIID = oXI.BOI("Documents_T", null, null, WhrParams);
                                            oBOIID.SetAttribute("FileName", CIFPath + sName);
                                            oBOIID.SetAttribute("sFullPath", CIFPath + sName + "." + sFileFormat);
                                            oBOIID.SetAttribute("sAliasName", sName + "." + sFileFormat);
                                            oBOIID.SetAttribute("sName", sName + "." + sFileFormat);
                                            var oCRes = oBOIID.Save(oBOIID);
                                        }
                                        var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIDocumentTree");
                                        XIIBO oBOIDoc = new XIIBO();
                                        oBOIDoc.SetAttribute("sName", sFileAliasName);
                                        oBOIDoc.SetAttribute("sPath", iDocID.ToString());
                                        oBOIDoc.SetAttribute("sParentID", sParentID);
                                        oBOIDoc.SetAttribute("iBuildingID", iBuildingID.ToString());
                                        oBOIDoc.SetAttribute("sType", "20");
                                        oBOIDoc.SetAttribute("sPageNo", "1");
                                        oBOIDoc.SetAttribute("sVersion", iVersion.ToString());
                                        oBOIDoc.SetAttribute("iVersionBatchID", iVersionBatchID);
                                        oBOIDoc.SetAttribute("iApprovalStatus", "30");
                                        oBOIDoc.SetAttribute("sFolderName", sFolderName);
                                        oBOIDoc.BOD = BOD;
                                        var oCR = oBOIDoc.Save(oBOIDoc);

                                        if (bIsNew)
                                        {
                                            oBOIDoc = (XIIBO)oCR.oResult;
                                            CIFDocID = oBOIDoc.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                                            iVersionBatchID = CIFDocID;
                                            oBOIDoc.SetAttribute("iVersionBatchID", CIFDocID);
                                            oCR = oBOIDoc.Save(oBOIDoc);
                                            cType = "new";
                                            iOldDocID = CIFDocID;
                                        }
                                        else
                                        {
                                            CIFDocID = oBOIDoc.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                                            cType = "old";
                                        }
                                    }

                                    string sFilePath = string.Empty;
                                    if (sNewPathDetails != null && sNewPathDetails.Count() > 0)
                                    {
                                        for (var i = 0; i < sNewPathDetails.Count(); i++)
                                        {
                                            iDocTypeID = Convert.ToInt32(sNewPathDetails[0]);
                                            sNewPathForSubDir = sNewPathDetails[1];
                                            sNewPath = sNewPathDetails[2];
                                        }
                                        if (sBOName.ToLower() == "XIDocumentTree".ToLower())
                                        {
                                            sNewPath = sNewPath + "\\" + sFolderName;
                                        }
                                        sFilePath = sNewPath + "\\" + sNewFileName + "." + sFileFormat;
                                        //Common.SaveErrorLog("File Saving Path: " + sNewPath + "\\" + sNewFileName + "." + sImageFormat, "XIDNA");
                                        items.SaveAs(sNewPath + "\\" + sNewFileName + "." + sFileFormat);
                                    }
                                    //Virtual Directory Saving
                                    //Common.SaveErrorLog("Pdf save", "XIDNA");
                                    //string saveFile = Path.Combine("~\\Content\\Uploads\\", sNewFileName + "." + sImageFormat);
                                    //Common.SaveErrorLog("File Saving Path: " + Server.MapPath(saveFile), "XIDNA");
                                    //items.SaveAs(Server.MapPath(saveFile));
                                    try
                                    {
                                        //check DocID and update the details
                                        if (iDocID > 0)
                                        {
                                            if (sBOName.ToLower() == "XIDocumentTree".ToLower())
                                            {
                                                if (iDocumentID == 0)
                                                {
                                                    sNewFileName = CIFPath + sFLname;
                                                }
                                                else
                                                {
                                                    sNewFileName = CIFPath + sNewFileName;
                                                }
                                                sNewPathForSubDir = null;
                                            }
                                            oBOI = new XIIBO();
                                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Documents_T", null);
                                            oBOI.BOD = oBOD;
                                            oBOI.Attributes["ID"] = new XIIAttribute { sName = "ID", sValue = iDocID.ToString(), bDirty = true };
                                            oBOI.Attributes["FileName"] = new XIIAttribute { sName = "FileName", sValue = sNewFileName + "." + sFileFormat, bDirty = true };
                                            oBOI.Attributes["FKiDocType"] = new XIIAttribute { sName = "FKiDocType", sValue = iDocTypeID.ToString(), bDirty = true };
                                            oBOI.Attributes["SubDirectoryPath"] = new XIIAttribute { sName = "SubDirectoryPath", sValue = sNewPathForSubDir, bDirty = true };
                                            oBOI.Attributes["iSystemgeneratedorUploaded"] = new XIIAttribute { sName = "iSystemgeneratedorUploaded", sValue = "20", bDirty = true };
                                            oBOI.Attributes["sFullPath"] = new XIIAttribute { sName = "sFullPath", sValue = sNewPathForSubDir + @"\" + sNewFileName + "." + sFileFormat, bDirty = true };
                                            if (string.IsNullOrEmpty(sFileAliasName))
                                            {
                                                sFileAliasName = sFLname;
                                            }
                                            oBOI.Attributes["sAliasName"] = new XIIAttribute { sName = "sAliasName", sValue = sFileAliasName + "." + sFileFormat, bDirty = true };
                                            oBOI.Attributes["sName"] = new XIIAttribute { sName = "sName", sValue = sFileName, bDirty = true };
                                            var Result = oBOI.Save(oBOI);
                                            iSavedToDoc = 1;
                                            if (sBOName.ToLower() == "XIDocumentTree".ToLower())
                                            {
                                                GetTextFromPDF(sFilePath, CIFDocID);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While File saving" });
                                        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                        oCResult.LogToFile();
                                        oXIDB.SaveErrortoDB(oCResult);
                                        //do nothin
                                    }
                                    if (iSavedToDoc == 1)
                                    {
                                        //for now no preview
                                    }
                                }
                                //Add Doc ID where the image name is saved to a list.
                                sDocIDList.Add(iDocID.ToString());
                            }//try
                            catch (Exception ex)
                            {
                                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While File saving" });
                                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                oCResult.LogToFile();
                                oXIDB.SaveErrortoDB(oCResult);
                                //Common.SaveErrorLog(ex.ToString(), "XIDNA");
                                if (iDocID > 0)
                                {
                                    XIIBO oDeleteBOI = new XIIBO();
                                    oDeleteBOI = oXI.BOI("Documents_T", iDocID.ToString());
                                    if (oDeleteBOI != null)
                                    {
                                        oDeleteBOI.Delete(oDeleteBOI);
                                    }
                                    DocumentID = "";
                                    //XIDocs Doc = sdbContext.XIDocs.Find(iDocID);
                                    //sdbContext.XIDocs.Remove(Doc);
                                    //sdbContext.SaveChanges();
                                }
                            }
                        }//end upload image for

                        //save the Doc ID where the file name is stored to perticular table
                        //check the not uploaded files
                        if (sNotUploaded.Count == 0)
                        {

                            if (Convert.ToInt32(sInstanceID) != 0)
                            {
                                int iDataSourceID = oInstanceBOII.BOD.iDataSource;
                                XIDXI oXID = new XIDXI();
                                string sConnection = oXID.GetBODataSource(iDataSourceID, oInstanceBOII.BOD.FKiApplicationID);
                                if (!string.IsNullOrEmpty(sConnection))
                                {
                                    //check if the table with field has a image ID.
                                    using (SqlConnection Con = new SqlConnection(sConnection))
                                    {
                                        Con.Open();
                                        SqlCommand cmd = new SqlCommand();
                                        cmd.Connection = Con;
                                        //if (sTableName != "Reports")
                                        //{
                                        //    Con.ChangeDatabase(sOrgDB);
                                        //}
                                        cmd.CommandText = "SELECT " + sFieldName + " FROM " + sTableName + " WHERE ID=" + sInstanceID;
                                        SqlDataReader reader = cmd.ExecuteReader();
                                        string sDocID = "";
                                        while (reader.Read())
                                        {
                                            sDocID = reader.IsDBNull(0) ? null : reader.GetValue(0).ToString();
                                        }
                                        Con.Close();

                                        string sNewDocID = "";
                                        if (!string.IsNullOrEmpty(sDocID))
                                        {
                                            string sDocIDs = string.Join(",", sDocIDList.Select(x => x.ToString()).ToArray());
                                            sNewDocID = sDocID + "," + sDocIDs;
                                        }
                                        else
                                        {
                                            sNewDocID = string.Join(",", sDocIDList.Select(x => x.ToString()).ToArray());
                                        }

                                        Con.Open();
                                        SqlCommand cmd1 = new SqlCommand();
                                        cmd1.Connection = Con;
                                        //if (sTableName != "Reports")
                                        //{
                                        //    Con.ChangeDatabase(sOrgDB);
                                        //}
                                        cmd1.CommandText = "UPDATE " + sTableName + " SET " + sFieldName + "='" + sNewDocID + "' WHERE ID=" + sInstanceID;
                                        cmd1.ExecuteNonQuery();
                                        Con.Close();
                                        DocumentID = sNewDocID;
                                    }
                                }
                                iStatus = 1;
                            }
                        }
                        else
                        {
                            iStatus = 0;
                            if (iDeleteDocIfNull != 0)
                            {
                                XIIBO oDeleteBOI = new XIIBO();
                                oDeleteBOI = oXI.BOI("Documents_T", iDeleteDocIfNull.ToString());
                                if (oDeleteBOI != null)
                                {
                                    oDeleteBOI.Delete(oDeleteBOI);
                                }
                                if (DocumentID != "-1")
                                {
                                    DocumentID = "";
                                }
                                //XIDocs Doc = sdbContext.XIDocs.Find(iDeleteDocIfNull);
                                //sdbContext.XIDocs.Remove(Doc);
                                //sdbContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        //save the image as Blob
                    }
                }
                //if (!string.IsNullOrEmpty(DocumentID))
                //{
                //    if (DocumentID != "-1")
                //    {
                //        DocumentID = DocumentID.Substring(0, DocumentID.Length - 1);
                //    }
                //}
                if (!string.IsNullOrEmpty(CIFDocID) && sBOName.ToLower() == "xidocumenttree")
                {
                    DocumentID = DocumentID + "_cif," + CIFDocID + "," + sFileAliasName + "," + iVersionBatchID + "," + cType + "," + iOldDocID;
                }
                return Content(DocumentID.ToString());
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While File saving" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                oXIDB.SaveErrortoDB(oCResult);
                return Content("");
            }
            //return Json(sQuery.ToString(), JsonRequestBehavior.AllowGet);
        }

        private bool CheckFilesAllowedToSave(string fileType, List<HttpPostedFileBase> uploadImage)
        {
            ModelDbContext dbContext = new ModelDbContext();
            bool bSave = true;
            var FileTypes = fileType.Split(',').ToList();
            foreach (var file in uploadImage)
            {
                string[] sFormat = file.FileName.Split('.');
                string sImageFormat = sFormat[1];
                var sFileTypeCheck = dbContext.XIDocTypes.Where(m => m.Type.ToLower() == sImageFormat).FirstOrDefault();
                if (sFileTypeCheck != null)
                {
                    if (FileTypes.Contains(sFileTypeCheck.ID.ToString()))
                    {
                        if (!string.IsNullOrEmpty(sFileTypeCheck.sSizeInMB))
                        {
                            decimal AllowedSize = 0;
                            decimal.TryParse(sFileTypeCheck.sSizeInMB, out AllowedSize);
                            var FileSize = Utility.GetFileSizeinMB(file.ContentLength);
                            FileSize = FileSize.Replace("MB", "");
                            decimal iSize = 0;
                            decimal.TryParse(FileSize, out iSize);
                            if (iSize != 0)
                            {
                                if (iSize < AllowedSize)
                                {

                                }
                                else
                                {
                                    bSave = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        bSave = false;
                    }
                }
            }
            return bSave;
        }
        //Calculate the aspect ratio and recreate the image
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        //thumbnail
        public static Image ThumbImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            Image thumb = image.GetThumbnailImage(newWidth, newHeight, () => false, IntPtr.Zero);
            return thumb;
        }

        //Create dir

        public List<string> CheckAndCreateDirectory(List<XIDocTypes> DocTypes, bool bIsCreateIF)
        {
            try
            {
                string physicalPath = "";
                string sPath = "";
                string sFilePath = "";
                int iDocTypeID = 0;
                string sSubDirectory = "";
                string sNewPath = "";
                string sNewPathForSubDir = "";
                var lPathDetails = new List<string>();
                foreach (var DTypes in DocTypes)
                {
                    sFilePath = DTypes.Path;
                    iDocTypeID = DTypes.ID;
                    lPathDetails.Add(iDocTypeID.ToString());
                    sSubDirectory = DTypes.SubDirectory.ToLower();
                    if (bIsCreateIF)
                    {
                        sSubDirectory = "CreateIF/PDF/Client1/Project1";
                    }
                    if (sSubDirectory == "year/month/day" || sSubDirectory == "CreateIF/PDF/Client1/Project1")
                    {
                        //check if sub directory has "/"
                        if ((sSubDirectory.Contains("/")))
                        {
                            sSubDirectory = sSubDirectory.Replace(@"\", "/");
                            string sSubDirCsV = sSubDirectory.Replace("/", ",").TrimStart();
                            List<string> sSubDirList = sSubDirCsV.Split(',').ToList();
                            List<string> sNewSubDirPath = new List<string>(); ;
                            foreach (var DirNames in sSubDirList)
                            {
                                string sVal = "";
                                DateTime DateTme = DateTime.Now;
                                if (DirNames.ToLower() == "year")
                                {
                                    sVal = DateTme.Year.ToString();
                                }
                                else if (DirNames.ToLower() == "month")
                                {
                                    sVal = DateTme.Month.ToString();
                                }
                                else if (DirNames.ToLower() == "day")
                                {
                                    sVal = DateTme.Day.ToString();
                                }
                                else
                                {
                                    sVal = DirNames;
                                }
                                sNewSubDirPath.Add(sVal);
                            }//for
                             //physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                             //sPath = physicalPath.Substring(0, physicalPath.Length) + "\\" + sFilePath;

                            var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                            var sVirtualPath = "~\\" + sVirtualDir + "\\";
                            sPath = Server.MapPath(sVirtualPath) + sFilePath;
                            //Common.SaveErrorLog("Creating SubDirectory Path " + sPath, "");
                            //if (sPath.Contains("~/"))
                            //{
                            //    sPath = sPath.Replace("~/", "");
                            //}

                            if (sPath.Contains('/'))
                            {
                                sPath = sPath.Replace("/", "\\");
                            }
                            ////Save the new created sub dir path to "XI Doc Settings"http://localhost:53996/Home/LandingPages
                            string sNewSubDirPathCSV = String.Join(",", sNewSubDirPath.Select(x => x.ToString()).ToArray());
                            sNewPathForSubDir = sNewSubDirPathCSV.Replace(",", "/");
                            //Add subdirpath to list
                            lPathDetails.Add(sNewPathForSubDir);
                            foreach (var sNwSubdir in sNewSubDirPath)
                            {
                                //sNewPath = "";
                                if (sNewPath == "" || sNewPath == null)
                                {
                                    sNewPath = sPath + "\\" + sNwSubdir;
                                }
                                else
                                {
                                    sNewPath = sNewPath + "\\" + sNwSubdir;
                                }
                                if (Directory.Exists(sNewPath))
                                {

                                }
                                else
                                {
                                    System.IO.Directory.CreateDirectory(sNewPath);
                                }
                            }
                        }//sub dir
                        lPathDetails.Add(sNewPath);

                    }
                    else if (bIsCreateIF)
                    {

                    }
                    //sub=10;
                    else
                    {
                        //physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                        //sPath = physicalPath.Substring(0, physicalPath.Length) + sFilePath;
                        var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                        var sVirtualPath = "~\\" + sVirtualDir + "\\";
                        sPath = Server.MapPath(sVirtualPath) + sFilePath;
                        Common.SaveErrorLog("Creating SubDirectory Path " + sPath, "");
                        //if (sPath.Contains("~/"))
                        //{
                        //    sPath = sPath.Replace("~/", "");
                        //}

                        if (sPath.Contains('/'))
                        {
                            sPath = sPath.Replace("/", "\\");
                        }
                        sNewPath = sPath;
                        if (Directory.Exists(sNewPath))
                        {

                        }
                        else
                        {
                            var createDir = System.IO.Directory.CreateDirectory(sNewPath);
                        }
                        lPathDetails.Add(sNewPath);

                    }

                }
                return lPathDetails;
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Error while creating folder to save uploaded files", "");
                Common.SaveErrorLog(ex.ToString(), "");
                return null;
            }

        }
        #region Bordereau
        //public ActionResult CheckLexisNexis()
        //{
        //    Policy oPolicy = new Policy();
        //    var oResponse = oPolicy.TestLexisNexisResult();
        //    return Json(null, JsonRequestBehavior.AllowGet);
        //}
        public string CheckCSVGeneration()
        {
            Policy oPolicy = new Policy();
            List<CNV> oParams = new List<CNV>();
            CNV oCNV = new CNV();
            oCNV.sName = "sBOName";
            oCNV.sValue = "ACPolicy_T";
            oParams.Add(oCNV);
            oCNV = new CNV();
            oCNV.sName = "iInstanceID";
            oCNV.sValue = "23150";//23053//23113
            oParams.Add(oCNV);
            oCNV = new CNV();
            oCNV.sName = "sTemplate";
            oCNV.sValue = "Policy CSV";
            oParams.Add(oCNV);
            oCNV = new CNV();
            oCNV.sName = "sStructure";
            oCNV.sValue = "MTA Copy";
            oParams.Add(oCNV);
            var oResponse = oPolicy.GenerateCSVFile(oParams);
            var sResult = (string)oResponse.oResult;
            return sResult;
        }
        public ActionResult CheckDocumentGeneration()
        {
            Policy oPolicy = new Policy();
            List<CNV> oParams = new List<CNV>();
            oParams.Add(new CNV { sName = "sGUID", sValue = Guid.NewGuid().ToString() });
            oParams.Add(new CNV { sName = "sDataBase", sValue = SessionManager.CoreDatabase });
            oParams.Add(new CNV { sName = "iUserID", sValue = SessionManager.UserID.ToString() });
            oParams.Add(new CNV { sName = "srolename", sValue = SessionManager.sRoleName });
            oParams.Add(new CNV { sName = "sOrgDatabase", sValue = SessionManager.OrgDatabase });
            oParams.Add(new CNV { sName = "sOrgName", sValue = SessionManager.OrganisationName });
            oParams.Add(new CNV { sName = "iOrganizationID", sValue = SessionManager.OrganizationID.ToString() });
            oParams.Add(new CNV { sName = "Object", sValue = "ACPolicy_T" });
            //oParams.Add(new CNV { sName = "iACPolicyID", sValue = "26147" });//23349(no convictions)//23350(no claims and conv)
            //oParams.Add(new CNV { sName = "iProductID", sValue = "1" });
            //oParams.Add(new CNV { sName = "iACPolicyID", sValue = "25077" });//3drivers
            //oParams.Add(new CNV { sName = "iProductID", sValue = "69" });
            //oParams.Add(new CNV { sName = "iACPolicyID", sValue = "25157" });//2 drivers
            //oParams.Add(new CNV { sName = "iProductID", sValue = "69" });
            oParams.Add(new CNV { sName = "iACPolicyID", sValue = "1" });//1 drivers
            oParams.Add(new CNV { sName = "iProductID", sValue = "69" });
            oPolicy.GeneratePolicyDocuments(oParams);

            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportToExcel()
        {
            var gv = new System.Web.UI.WebControls.GridView();
            gv.DataSource = this.CheckCSVGeneration();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DemoExcel.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckPolicyIDUpdate()
        {
            Policy oPolicy = new Policy();
            List<CNV> oParams = new List<CNV>();
            oParams.Add(new CNV { sName = "iACPolicyID", sValue = "23053" });
            oParams.Add(new CNV { sName = "sQSInstanceID", sValue = "10420" });
            oPolicy.UpdatePolicyIDtoBOI(oParams);
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateBordereau()
        {
            try
            {
                //int iOneClickID = 2429;
                string sOneClickName = "Generate Bordereau";
                XID1Click o1ClickD = new XID1Click();
                if (!string.IsNullOrEmpty(sOneClickName))
                {
                    //Get 1-Click Defintion             
                    o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, sOneClickName, null);
                    Dictionary<string, XIIBO> oRes = o1ClickD.OneClick_Execute();
                    List<XIIBO> oBOIList = new List<XIIBO>();
                    oBOIList = oRes.Values.ToList();
                    List<string> oEDIData = new List<string>();
                    if (oBOIList != null && oBOIList.Count() > 0)
                    {
                        string sMergedTextHeader = oBOIList.FirstOrDefault().AttributeI("sMergedTextHeader").sValue;
                        DataTable dt = new DataTable();
                        List<string> sInstanceIDList = new List<string>();
                        if (!string.IsNullOrEmpty(sMergedTextHeader))
                        {
                            oEDIData.Add(sMergedTextHeader);
                        }
                        foreach (var oBOI in oBOIList)
                        {
                            string sMergedContent = oBOI.AttributeI("MergedText").sValue;
                            string sInstanceID = oBOI.AttributeI("iInstanceID").sValue;
                            sInstanceIDList.Add(sInstanceID);
                            oEDIData.Add(sMergedContent);
                        }
                        string sFinalString = string.Join("\r\n", oEDIData);
                        string sInstanceIDs = string.Join(",", sInstanceIDList);
                        string sFileName = "KGM Bordereau";
                        var oResponse = ExportsToCSV(sFinalString, sFileName);
                        if (oResponse.bOK)
                        {
                            XIDBO oBOD = new XIDBO();
                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Bordereau_T");
                            XIIBO oBOI = new XIIBO();
                            oBOI.BOD = oBOD;
                            oBOI.SetAttribute("sBordereauFileName", sFileName);
                            oBOI.SetAttribute("iStatus", "10");
                            var Response = oBOI.Save(oBOI);
                            if (Response.bOK && Response.oResult != null)
                            {
                                oBOI = (XIIBO)Response.oResult;
                                var sBordereauID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).FirstOrDefault().sValue;
                                oBOD = new XIDBO();
                                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "BordereauDetails_T");
                                oBOI = new XIIBO();
                                oBOI.BOD = oBOD;
                                oBOI.SetAttribute("FKiBordereauID", sBordereauID);
                                oBOI.SetAttribute("sInstanceIDs", sInstanceIDs);
                                var oBOIResponse = oBOI.Save(oBOI);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog("GenerateBordereau Failed: " + ex.ToString(), "");
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public CResult ExportsToCSV(string sResult, string sFileName)
        {
            CResult oCResult = new CResult();
            try
            {
                sFileName = sFileName + ".csv";
                string sCurrentDate = DateTime.Now.Date.ToString(XIConstant.Date_Format); //"dd-MMM-yyyy"

                var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                var sVirtualPath = "~\\" + sVirtualDir + "\\";
                string filePath = Server.MapPath(sVirtualPath) + "\\BordereauFiles\\" + sCurrentDate + "";

                //string physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                //string filePath =  physicalPath.Substring(0, physicalPath.Length) + "//content//BordereauFiles//" + sCurrentDate + "";
                // Check if file already exists. If yes, delete it.     
                //if (File.Exists(filePath))
                //{
                //    File.Delete(filePath);
                //}
                if (Directory.Exists(filePath))
                {

                }
                else
                {
                    var createDir = System.IO.Directory.CreateDirectory(filePath);
                }
                System.IO.File.WriteAllText(filePath + "//" + sFileName, sResult.ToString());
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                //SaveErrortoDB(oCResult);
                // oCResult.LogToFile();
            }
            return oCResult;
        }
        public ActionResult GenerateBordereauWithAddons()
        {
            try
            {
                //int iOneClickID = 2507;
                string sOneClickName = "Generate Broadreau WithAddons";
                XID1Click o1ClickD = new XID1Click();
                if (!string.IsNullOrEmpty(sOneClickName))
                {
                    //Get 1-Click Defintion             
                    o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, sOneClickName, null);
                    Dictionary<string, XIIBO> oRes = o1ClickD.OneClick_Execute();
                    List<XIIBO> oBOIList = new List<XIIBO>();
                    oBOIList = oRes.Values.ToList();
                    if (oBOIList != null && oBOIList.Count() > 0)
                    {
                        if (oBOIList.Any(x => x.Attributes.ContainsKey("FKiAddonID")))
                        {
                            var oProductGroupList = oBOIList.Where(x => string.IsNullOrEmpty(x.AttributeI("FKiAddonID").sValue) || x.AttributeI("FKiAddonID").sValue == "0").ToList();
                            var oAddonGroupList = oBOIList.Where(x => !string.IsNullOrEmpty(x.AttributeI("FKiAddonID").sValue) && x.AttributeI("FKiAddonID").sValue != "0").ToList();
                            if (oProductGroupList != null && oProductGroupList.Count() > 0)
                            {
                                if (oProductGroupList.Any(x => x.Attributes.ContainsKey("FKiProductID")))
                                {
                                    var oProductIDs = oProductGroupList.Select(x => x.AttributeI("FKiProductID").sValue).Distinct();
                                    foreach (var product in oProductIDs)
                                    {
                                        string sFileName = string.Empty;
                                        var oProductI = oIXI.BOI("Product", product);
                                        if (oProductI != null && oProductI.Attributes.ContainsKey("sName"))
                                        {
                                            sFileName = oProductI.Attributes["sName"].sValue;
                                        }
                                        var oProductGroupBOIList = oProductGroupList.Where(x => x.AttributeI("FKiProductID").sValue == product).ToList();
                                        if (oProductGroupBOIList != null && oProductGroupBOIList.Count() > 0)
                                        {
                                            SaveBroadreau(oProductGroupBOIList, sFileName);
                                        }
                                    }
                                }
                            }
                            if (oAddonGroupList != null && oAddonGroupList.Count() > 0)
                            {
                                if (oAddonGroupList.Any(x => x.Attributes.ContainsKey("FKiAddonID")))
                                {
                                    var oAddonIDs = oAddonGroupList.Select(x => x.AttributeI("FKiAddonID").sValue + "_" + x.AttributeI("sWhenGenerate").sValue + "_" + x.AttributeI("sDocumentName").sValue).Distinct();
                                    foreach (var addon in oAddonIDs)
                                    {
                                        var sAddonId = addon.Split('_')[0];
                                        var sTemplateID = addon.Split('_')[1];
                                        var sFileName = addon.Split('_')[2];
                                        //XIContentEditors oTemplateContent = new XIContentEditors();
                                        //string sFileName = string.Empty;
                                        //var oDocContent = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, "", sTemplateID); //oXIDX.Get_ContentDefinition(iDocumentID, sDocumentType);
                                        //if (oDocContent != null && oDocContent.Count() > 0)
                                        //{
                                        //    oTemplateContent = oDocContent.FirstOrDefault();
                                        //    sFileName = oTemplateContent.Name;
                                        //}
                                        List<CNV> oWhereParams = new List<CNV>();
                                        CNV oCNV = new CNV();
                                        //string sFkProductID = string.Empty;
                                        //if (oAddonGroupList.Any(x => x.Attributes.ContainsKey("FKiProductID")))
                                        //{
                                        //    sFkProductID = oAddonGroupList.Select(x => x.AttributeI("FKiProductID").sValue).FirstOrDefault();
                                        //}
                                        //string sFileName = string.Empty;
                                        //oCNV.sName = "FKiProductID";
                                        //oCNV.sValue = sFkProductID;
                                        //oWhereParams.Add(oCNV);
                                        //oCNV = new CNV();
                                        //oCNV.sName = "refAddon";
                                        //oCNV.sValue = addon;
                                        //oWhereParams.Add(oCNV);
                                        //// string sProductAddonID= oAddonGroupList
                                        //var oAddonI = oIXI.BOI("ProductAddon_T", "", "", oWhereParams);
                                        //if (oAddonI != null && oAddonI.Attributes.ContainsKey("sName"))
                                        //{
                                        //    sFileName = oAddonI.Attributes["sName"].sValue;
                                        //}
                                        var oAddonGroupBOIList = oAddonGroupList.Where(x => x.AttributeI("FKiAddonID").sValue == sAddonId && x.AttributeI("sWhenGenerate").sValue == sTemplateID).ToList();
                                        if (oAddonGroupBOIList != null && oAddonGroupBOIList.Count() > 0)
                                        {
                                            SaveBroadreau(oAddonGroupBOIList, sFileName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog("Generate Bordereau GenerateBordereauWithAddons method falied: " + ex.ToString(), "");
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        private void SaveBroadreau(List<XIIBO> oList, string sFileName)
        {
            try
            {
                List<string> oEDIData = new List<string>();
                string sMergedTextHeader = string.Empty;
                if (oList.FirstOrDefault().Attributes.ContainsKey("sMergedTextHeader"))
                {
                    sMergedTextHeader = oList.FirstOrDefault().AttributeI("sMergedTextHeader").sValue;
                }
                List<string> sInstanceIDList = new List<string>();
                if (!string.IsNullOrEmpty(sMergedTextHeader))
                {
                    oEDIData.Add(sMergedTextHeader);
                }
                foreach (var oBOI in oList)
                {
                    string sMergedContent = string.Empty; string sInstanceID = string.Empty;
                    if (oBOI.Attributes.ContainsKey("MergedText"))
                    {
                        sMergedContent = oBOI.AttributeI("MergedText").sValue;
                    }
                    if (oBOI.Attributes.ContainsKey("iInstanceID"))
                    {
                        sInstanceID = oBOI.AttributeI("iInstanceID").sValue;
                    }
                    sInstanceIDList.Add(sInstanceID);
                    if (!string.IsNullOrEmpty(sMergedContent))
                    {
                        oEDIData.Add(sMergedContent);
                    }
                }
                string sFinalString = string.Empty;
                if (oEDIData != null && oEDIData.Count() > 0)
                {
                    sFinalString = string.Join("\r\n", oEDIData);
                }
                string sInstanceIDs = string.Join(",", sInstanceIDList);
                //string sFileName = "KGM Bordereau";
                if (!string.IsNullOrEmpty(sFinalString))
                {
                    var oResponse = ExportsToCSV(sFinalString, sFileName);
                    if (oResponse.bOK)
                    {
                        XIDBO oBOD = new XIDBO();
                        oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Bordereau_T");
                        XIIBO oBOI = new XIIBO();
                        oBOI.BOD = oBOD;
                        oBOI.SetAttribute("sBordereauFileName", sFileName);
                        oBOI.SetAttribute("iStatus", "10");
                        var Response = oBOI.Save(oBOI);
                        if (Response.bOK && Response.oResult != null)
                        {
                            oBOI = (XIIBO)Response.oResult;
                            var sBordereauID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).FirstOrDefault().sValue;
                            oBOD = new XIDBO();
                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "BordereauDetails_T");
                            oBOI = new XIIBO();
                            oBOI.BOD = oBOD;
                            oBOI.SetAttribute("FKiBordereauID", sBordereauID);
                            oBOI.SetAttribute("sInstanceIDs", sInstanceIDs);
                            var oBOIResponse = oBOI.Save(oBOI);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog("SaveBroadreau failed: " + ex.ToString(), "");
            }
        }
        #endregion
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Reconcilliation(int ReconciliationID)
        {
            try
            {
                Reconcilliation oRec = new Reconcilliation();
                var oCResult = oRec.ReverseReconciliation(ReconciliationID);
                if (oCResult.iTraceLevel == 1)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult CancelTransaction(string XILinkID, string sGUID)
        {
            try
            {
                XILinkLoadJson(Convert.ToInt32(XILinkID), sGUID);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult TaskScheduler()
        //{
        //    try
        //    {
        //        string Str = "Test";
        //        #region Load_Template_Oneclick,Structures,Layout
        //        List<Dictionary<string, string>> lstAnnonymus = new List<Dictionary<string, string>>();
        //        int One_clickid = 2670;
        //        int One_TemplateID = 116;
        //        int one_LayoutID = 0;
        //        string sOneclickBoname = string.Empty;
        //        string sexpression = "{XIP|@@BO.@@ID}";
        //        XIInfraHtmlComponent oMerge = new XIInfraHtmlComponent();
        //        XIDXI oneoDXI = new XIDXI();
        //        var soneclickDef = oneoDXI.Get_1ClickDefinition("", One_clickid.ToString());
        //        if (soneclickDef.bOK && soneclickDef.oResult != null)
        //        {
        //            oMerge.oneClickDef = (XID1Click)soneclickDef.oResult;
        //        }
        //        XIDBO xiiBOD = new XIDBO();
        //        var ooBODef = oneoDXI.Get_BODefinition("", oMerge.oneClickDef.BOID.ToString());
        //        if (ooBODef.bOK && ooBODef.oResult != null)
        //        {
        //            xiiBOD = (XIDBO)ooBODef.oResult;
        //        }
        //        oMerge.oneClickDef.BOD = xiiBOD;
        //        sOneclickBoname = xiiBOD.Name;
        //        List<CNV> Listresolveparams = new List<CNV>();
        //        var oParams = new List<CNV>();
        //        oParams.Add(new CNV { sName = "sSessionID", sValue = Guid.NewGuid().ToString() });
        //        oParams.Add(new CNV { sName = "sGUID", sValue = Guid.NewGuid().ToString() });
        //        oParams.Add(new CNV { sName = "i1ClickID", sValue = One_clickid.ToString() });
        //        oParams.Add(new CNV { sName = "sTemplate", sValue = One_TemplateID.ToString() });
        //        Listresolveparams.Add(new CNV { sName = "{XIP|ACPolicy_T.id}", sValue = "23496" });
        //        Listresolveparams.Add(new CNV { sName = "{XIP|ACPolicy_T.id}", sValue = "23495" });
        //        oMerge.resolveExpressions = Listresolveparams;

        //        XIDXI oDXI = new XIDXI();
        //        if (One_TemplateID > 0)
        //        {
        //            List<XIContentEditors> oTemplateD = new List<XIContentEditors>();
        //            var oXiTemplateDef = oDXI.Get_ContentDefinition(Convert.ToInt32(One_TemplateID));
        //            if (oXiTemplateDef.bOK && oXiTemplateDef.oResult != null)
        //            {
        //                oTemplateD = (List<XIContentEditors>)oXiTemplateDef.oResult;
        //            }
        //            oMerge.XiDTemplate = oTemplateD;
        //        }
        //        if (one_LayoutID > 0)
        //        {
        //            oDXI = new XIDXI();
        //            XIDLayout xiLayout = new XIDLayout();
        //            var oXiLayoutDef = oDXI.Get_LayoutDefinition("", One_clickid.ToString());
        //            if (oXiLayoutDef.bOK && oXiLayoutDef.oResult != null)
        //            {
        //                xiLayout = (XIDLayout)oXiLayoutDef.oResult;
        //            }
        //            oMerge.XiDLayout = xiLayout;
        //        }
        //        var mergedContent = oMerge.XILoadRefactored(oParams);
        //        var result = (CResult)mergedContent;
        //        var content = ((XID1Click)result.oResult).RepeaterResult.FirstOrDefault();
        //        //foreach (var merge in lstAnnonymus)
        //        //{
        //        //    // Merge Content Here
        //        //    Listresolveparams = new List<CNV>();
        //        //    if (merge.ContainsKey("PKID"))
        //        //    {
        //        //        Listresolveparams.Add(new CNV { sName = sexpression.Replace("@@BO", sOneclickBoname).Replace("@@ID", "id").Trim(), sValue = merge["PKID"] });
        //        //    }
        //        //    oMerge.resolveExpressions = Listresolveparams;
        //        //    var mergedContent = oMerge.XILoadRefactored(oParams);
        //        //    // Send Me mail Here and Log also
        //        //}
        //        #endregion
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        [AllowAnonymous]
        public ActionResult GetDependencyDropDownSearch(string ParentID, string sValue, string sParentBo)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            List<XIDFieldOptionList> obj = new List<XIDFieldOptionList>();
            try
            {
                //oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}", ParentID.ToString(), null, null);
                //XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                Dictionary<string, object> FieldOrgParams = new Dictionary<string, object>();
                FieldOrgParams["ID"] = ParentID;
                var oAttr = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", FieldOrgParams).FirstOrDefault();
                FieldOrgParams = new Dictionary<string, object>();
                FieldOrgParams["ID"] = oAttr.iDependentFieldID;
                var oDependentAttr = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", FieldOrgParams).FirstOrDefault();
                //FieldOrgParams = new Dictionary<string, object>();
                //FieldOrgParams["ID"] = oDependentAttr.BOID;
                //var oDependentBO = Connection.Select<XIDAttribute>("XIBO_T_N", FieldOrgParams).FirstOrDefault();
                XIDXI oXIDXI = new XIDXI();
                string sUID = "bo-" + oDependentAttr.sFKBOName; string sName = string.Empty;
                //XIIBO oBO = new XIIBO();
                //oBO.BOI("")
                List<CNV> oParams = new List<CNV>();
                //oParams = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                //oParams.Add(new CNV { sName = "{XIP|iParentID}", sValue = ParentID.ToString() });
                var Result = oXIDXI.Get_DependencyAutoCompleteSearchList(ParentID.ToString(), sUID, oDependentAttr.sFKBOName, sValue, sParentBo, 10).oResult;
                obj = (List<XIDFieldOptionList>)Result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetDependencyDropDown(int ParentID, int FieldOriginID, string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            List<XIDFieldOptionList> obj = new List<XIDFieldOptionList>();
            try
            {
                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}", ParentID.ToString(), null, null);
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                Dictionary<string, object> FieldOrgParams = new Dictionary<string, object>();
                FieldOrgParams["ID"] = FieldOriginID;
                var QSFieldOrigin = Connection.Select<XIDFieldOrigin>("XIFieldOrigin_T", FieldOrgParams).FirstOrDefault();
                XIDXI oXIDXI = new XIDXI();
                string sUID = "1click-" + QSFieldOrigin.FK1ClickID; string sName = string.Empty;
                List<CNV> oParams = new List<CNV>();
                oParams = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                oParams.Add(new CNV { sName = "{XIP|iParentID}", sValue = ParentID.ToString() });
                var Result = oXIDXI.Get_AutoCompleteList(sUID, "", oParams, 10).oResult;
                obj = (List<XIDFieldOptionList>)Result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
       public ActionResult GetDependencyGroupDropDown(int BOID, string sGUID, string BOName)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            List<XIDGroup> obj = new List<XIDGroup>();
            try
            {
                if (BOID > 0 && !string.IsNullOrEmpty(BOName))
                {
                    var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BOName);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}", BOID.ToString(), null, null);
                    XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                    Dictionary<string, object> BOGroupsParams = new Dictionary<string, object>();
                    BOGroupsParams["BOID"] = BOID;
                    var QSFieldOrigin = Connection.Select<XIDGroup>(BOD.TableName, BOGroupsParams).ToList();
                    obj = (List<XIDGroup>)QSFieldOrigin;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetBoAttributeDependencyDropDown(string ChildAttr, string sValue, string sParentBo)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            List<XIDFieldOptionList> obj = new List<XIDFieldOptionList>();
            try
            {
                //oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}", ParentID.ToString(), null, null);
                //XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                Dictionary<string, object> FieldOrgParams = new Dictionary<string, object>();
                FieldOrgParams["ID"] = ChildAttr;
                var oDependentAttr = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", FieldOrgParams).FirstOrDefault();
                //FieldOrgParams = new Dictionary<string, object>();
                //FieldOrgParams["ID"] = oDependentAttr.BOID;
                //var oDependentBO = Connection.Select<XIDAttribute>("XIBO_T_N", FieldOrgParams).FirstOrDefault();
                XIDXI oXIDXI = new XIDXI();
                string sUID = "bo-" + oDependentAttr.sFKBOName; string sName = string.Empty;
                //XIIBO oBO = new XIIBO();
                //oBO.BOI("")
                List<CNV> oParams = new List<CNV>();
                //oParams = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                //oParams.Add(new CNV { sName = "{XIP|iParentID}", sValue = ParentID.ToString() });
                var Result = oXIDXI.Get_DependencyAutoCompleteSearchList(ChildAttr, sUID, oDependentAttr.sFKBOName, sValue, sParentBo, 10).oResult;
                obj = (List<XIDFieldOptionList>)Result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        //For Structure Details in simple1click
        [AllowAnonymous]
        public ActionResult GetStructureDependency(string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            List<XIDStructure> obj = new List<XIDStructure>();
            try
            {
                XIDBO oBODs = new XIDBO();
                string BOID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}");
                Dictionary<string, object> BOGroupsParams = new Dictionary<string, object>();
                BOGroupsParams["BOID"] = BOID;
                BOGroupsParams["FKiParentID"] = "#";
                var QSFieldOrigin = Connection.Select<XIDStructure>("XIBOStructure_T", BOGroupsParams).ToList();
                obj = (List<XIDStructure>)QSFieldOrigin;


            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public void PFNewBusiness()
        {
            try
            {
                Policy obj = new Policy();
                List<CNV> oParams = new List<CNV>();
                obj.InsertTransactions(oParams, 0, 0);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        #region LetterGeneration
        public ActionResult PreviewTemplate(string sGUID, string sXiLinkID, List<CNV> oParams)
        {
            var sSessionID = HttpContext.Session.SessionID; XIContentEditors oContent = new XIContentEditors();
            try
            {
                string sInstanceID = string.Empty;
                string sPreviewContent = string.Empty; string sPreviewStructure = string.Empty;
                XIBOInstance oStructobj = new XIBOInstance();
                List<XIContentEditors> oContentDef = new List<XIContentEditors>();
                XIContentEditors oContentC = new XIContentEditors();
                XIIXI oXI = new XIIXI(); string iTemplateID = string.Empty; string sSubject = string.Empty;
                if (!string.IsNullOrEmpty(sXiLinkID))
                {
                    var oXiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, sXiLinkID);
                    sPreviewStructure = oXiLink.XiLinkNVs.Where(m => m.Name.ToLower() == "sStructureName".ToLower()).Select(m => m.Value).FirstOrDefault();
                    //sBOName = oXiLink.XiLinkNVs.Where(m => m.Name.ToLower() == "sBOName".ToLower()).Select(m => m.Value).FirstOrDefault();
                }
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                string sBO = oGUIDParams.NMyInstance.Where(m => m.Key == "sBOName").Select(m => m.Value.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sBO))
                {
                    sInstanceID = oGUIDParams.NMyInstance.Where(m => m.Key == "iInstanceID").Select(m => m.Value.sValue).FirstOrDefault();
                }
                string ocacheLetter = string.Empty;
                if (oParams != null && oParams.Count() > 0)
                {
                    iTemplateID = oParams.Where(m => m.sName == "{XIP|FKiTemplateID}").Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(Convert.ToString(iTemplateID)))
                    {
                        oContentDef = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, null, Convert.ToString(iTemplateID));
                        ocacheLetter = oGUIDParams.NMyInstance.Where(m => m.Key.Contains("sLetterAttachement_" + iTemplateID)).Select(m => m.Key).FirstOrDefault();
                        if (oContentDef != null && oContentDef.Count() > 0)
                        {
                            oContent = oContentDef.FirstOrDefault();
                            oContentC = oContent.GetCopy();
                            if (!string.IsNullOrEmpty(oContentC.sSubject))
                            {
                                sSubject = oContentC.sSubject;
                            }
                            else
                            {
                                sSubject = oContentC.Name;
                            }
                            oContentC.Content = oContentC.SubStringNotation(oContentC.Content, "<body", "</body>");
                        }
                    }
                }
                string sEmail = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|sEmail}").Select(m => m.Value.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(ocacheLetter))
                {
                    sPreviewContent = oGUIDParams.NMyInstance.Where(m => m.Key.Contains("sLetterAttachement_" + iTemplateID)).Select(m => m.Value.sValue).FirstOrDefault();
                }
                else if (!string.IsNullOrEmpty(sBO) && !string.IsNullOrEmpty(sInstanceID) && !string.IsNullOrEmpty(sPreviewStructure))
                {
                    oStructobj = oIXI.BOI(sBO, sInstanceID).Structure(sPreviewStructure).XILoad(null, true);
                    oContentC.sSessionID = sSessionID;
                    oContentC.sGUID = sGUID;
                    var oCResult = oContentC.MergeContentTemplate(oContentC, oStructobj);
                    if (oCResult.bOK && oCResult.oResult != null)
                    {
                        sPreviewContent = (string)oCResult.oResult;
                    }
                }
                return Json(new { sPreviewContent = sPreviewContent, iTemplateID = iTemplateID, sXiLinkID = sXiLinkID, sEmail = sEmail, sSubject = sSubject }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SendLetterTemplate(string sLetterAttachmentContent, string sTemplateID, string sXiLinkID, string sGUID)
        {
            XIDefinitionBase oXID = new XIDefinitionBase();
            var sSessionID = HttpContext.Session.SessionID;
            int iOrgID = SessionManager.OrganizationID; bool bISSend = false; string sDocIDs = string.Empty; List<string> Docs = new List<string>();
            try
            {
                var AttachementPath = new List<string>();
                string sCallnotes = string.Empty;
                List<string> AttachmentNotes = new List<string>();
                List<XIContentEditors> oContentDef = new List<XIContentEditors>();
                XIContentEditors oContent = new XIContentEditors();
                XIContentEditors oContentC = new XIContentEditors();
                XIInfraEmail oEmail = new XIInfraEmail();
                var oXiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, sXiLinkID);
                string sStructureName = oXiLink.XiLinkNVs.Where(m => m.Name.ToLower() == "sStructureName".ToLower()).Select(m => m.Value).FirstOrDefault();
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                string sEmail = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|sEmail}").Select(m => m.Value.sValue).FirstOrDefault();
                string sSubject = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|sSubject}").Select(m => m.Value.sValue).FirstOrDefault();
                string sInstanceID = oGUIDParams.NMyInstance.Where(m => m.Key == "iInstanceID").Select(m => m.Value.sValue).FirstOrDefault();
                string sProductID = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|FKiProductID}").Select(m => m.Value.sValue).FirstOrDefault();
                string sProduct = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|ProductName}").Select(m => m.Value.sValue).FirstOrDefault();
                string sPolicyType = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|sTranstype}").Select(m => m.Value.sValue).FirstOrDefault();
                //mail body
                string sLetterBodyContent = oGUIDParams.NMyInstance.Where(m => m.Key == "sLetterBodyContent").Select(m => m.Value.sValue).FirstOrDefault();
                string sParentTemplateID = oGUIDParams.NMyInstance.Where(m => m.Key == "sParentTemplateID").Select(m => m.Value.sValue).FirstOrDefault();
                int iParentTemplateID = 0; string sSentStatus = "20"; int iDocID = 0; string sProductType = string.Empty;
                if (int.TryParse(sParentTemplateID, out iParentTemplateID)) { }
                int iLeadID = 0;
                XIIXI oXI = new XIIXI(); //string sSubject = string.Empty;
                int iPolicyID = 0;
                if (int.TryParse(sInstanceID, out iPolicyID))
                {
                }
                if (!string.IsNullOrEmpty(sPolicyType) && sPolicyType.ToLower() == "renewal")
                {
                    iPolicyID = 0;
                    string sQSInstanceID = oGUIDParams.NMyInstance.Where(m => m.Key == "{XIP|iRenewalQSInstanceID}").Select(m => m.Value.sValue).FirstOrDefault();
                    int iQSInstanceID = 0;
                    var PolicyID = oGUIDParams.NMyInstance.Where(m => m.Key == "-policyid").Select(m => m.Value.sValue).FirstOrDefault();
                    if (int.TryParse(PolicyID, out iPolicyID)) { }
                    if (int.TryParse(sQSInstanceID, out iQSInstanceID))
                    {
                        List<CNV> oNVs = new List<CNV>();
                        oNVs.Add(new CNV { sName = "FKiQSInstanceID", sValue = iQSInstanceID.ToString() });
                        var oLeadI = oXI.BOI("Lead_T", "", "", oNVs);
                        if (oLeadI != null && oLeadI.Attributes.ContainsKey("id"))
                        {
                            if (int.TryParse(oLeadI.Attributes["id"].sValue, out iLeadID))
                            { }
                        }
                    }
                }
                List<Attachment> oAttachments = new List<Attachment>();
                var Attachements = oGUIDParams.NMyInstance.Where(m => m.Key.Contains("sLetterAttachement_")).Select(m => m.Key).ToList();
                if (Attachements.Count > 0)
                {
                    foreach (var attachement in Attachements)
                    {
                        var sAttachmentContent = oGUIDParams.NMyInstance.Where(m => m.Key.Contains(attachement)).Select(m => m.Value.sValue).FirstOrDefault();
                        if (!string.IsNullOrEmpty(attachement) && !string.IsNullOrEmpty(sAttachmentContent))
                        {
                            var oList = attachement.Split('_');
                            oContentDef = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, null, oList[1]);
                            if (oContentDef != null && oContentDef.Count() > 0)
                            {
                                oContent = oContentDef.FirstOrDefault();
                                oContentC = oContent.GetCopy();
                                if (oContentC.Category == 30)
                                {
                                    CResult oResult = new CResult();
                                    var sReplaceContent = oContentC.SubStringNotation(oContentC.Content, "<body", "</body>");
                                    string sMergeTempContent = oContentC.Content.Replace(sReplaceContent, sAttachmentContent);
                                    //string sMergeTempContent = sPreviewContent;
                                    if (oContentC.iTypeofPDF == 20)//using ironPDF
                                    {
                                        oResult = oEmail.IronPdf(sMergeTempContent, oContentC.sCSSFileName, oContentC.bIsPaswordProtected, "", ""); //pdf generation
                                    }
                                    else if (oContentC.iTypeofPDF == 40)//using iTextsharp
                                    {
                                        //oResult = PDFGenerateiText7(sMergeTempContent, oContentC.bIsPaswordProtected, "", ""); //pdf generation//oEmail.PDFGenerate(sMergeTempContent, oContentC.bIsPaswordProtected, "", ""); //pdf generation
                                    }
                                    else
                                    {
                                        oResult = oEmail.PDFGenerate(sMergeTempContent, oContentC.bIsPaswordProtected, "", ""); //pdf generation
                                    }

                                    if (oResult.bOK && oResult.oResult != null)
                                    {
                                        oEmail.sDocumentName = oContentC.Name;
                                        var oAttachment = oEmail.GeneratePDFFile((MemoryStream)oResult.oResult);
                                        if (oAttachment.bOK && oAttachment.oResult != null)
                                        {
                                            Attachment data = (Attachment)oAttachment.oResult;
                                            MemoryStream file = (MemoryStream)oResult.oResult;
                                            XIInfraDocs oXIDocs = new XIInfraDocs();
                                            string[] sFileTypeArray = data.Name.Split('.');
                                            string sFileType = sFileTypeArray[1];
                                            var oXIDocDetails = (XIInfraDocTypes)oCache.GetObjectFromCache(XIConstant.CacheDocType, sFileType);
                                            var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                                            string physicalPath = HostingEnvironment.MapPath("~\\" + sVirtualDir + "\\");
                                            var sFolderPath = oXIDocDetails.Path;
                                            oXIDocs.sOrgName = SessionManager.OrganisationName;
                                            oXIDocs.iOrgID = iOrgID;
                                            var oResponse = oXIDocs.SaveDocuments(file, data.Name);
                                            if (oResponse.bOK && oResponse.oResult != null)
                                            {
                                                string sDocID = (string)oResponse.oResult;
                                                if (int.TryParse(sDocID, out iDocID))
                                                { }
                                                Docs.Add(iDocID.ToString());
                                                AttachmentNotes.Add(oContentC.Name);
                                                var oDocumentBOI = oIXI.BOI("Documents_T", sDocID);
                                                if (oDocumentBOI != null && oDocumentBOI.Attributes.ContainsKey("sFullPath"))
                                                {
                                                    if (!string.IsNullOrEmpty(oDocumentBOI.Attributes["sFullPath"].sValue))
                                                    {
                                                        AttachementPath.Add(physicalPath.Substring(0, physicalPath.Length) + sFolderPath.Replace("~", "") + "\\" + oDocumentBOI.Attributes["sFullPath"].sValue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Docs.Count > 0)
                {
                    sDocIDs = string.Join(",", Docs);
                }
                if (AttachmentNotes.Count > 0)
                {
                    sCallnotes = string.Join(",", AttachmentNotes);
                }
                var oProductI = oXI.BOI("Product", sProductID);
                if (oProductI != null && oProductI.Attributes.ContainsKey("sname"))
                {
                    sProduct = oProductI.Attributes["sname"].sValue;
                }
                if (oProductI != null && oProductI.Attributes.ContainsKey("fkiclassid"))
                {
                    sProductType = oProductI.AttributeI("fkiclassid").ResolveFK("display");
                }
                if (iParentTemplateID != 0 && !string.IsNullOrEmpty(sLetterBodyContent))
                {
                    XIContentEditors oParentContent = new XIContentEditors();
                    List<XIContentEditors> oPrentContentDef = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, null, iParentTemplateID.ToString());
                    if (oPrentContentDef != null && oPrentContentDef.Count() > 0)
                    {
                        oParentContent = oPrentContentDef.FirstOrDefault();
                        var oParentContentC = oParentContent.GetCopy();
                        if (oParentContentC.Category == 10)
                        {
                            CResult oResult = new CResult();
                            var sReplaceContent = oParentContentC.SubStringNotation(oParentContentC.Content, "<body", "</body>");
                            string sMergeTempContent = oParentContentC.Content.Replace(sReplaceContent, sLetterBodyContent);
                            if (oParentContentC.bIsHavingAttachments)
                            {
                                var oAttachmnetContentDef = oPrentContentDef.Where(x => x.ID != oParentContentC.ID).ToList();
                                XIInfraDocs oXIDocs = new XIInfraDocs();
                                foreach (var oAttachement in oAttachmnetContentDef)
                                {
                                    oXIDocs.sOrgName = SessionManager.OrganisationName;
                                    oXIDocs.iOrgID = iOrgID;
                                    string sCopyDocumentName = oAttachement.Name;
                                    string sCopyDocumentPath = string.Empty;
                                    string physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                                    string sPath = physicalPath.Substring(0, physicalPath.Length) + "\\Content\\ProductNonMergedDocuments\\" + sProduct + "_" + sProductType + "";
                                    string[] filePaths = Directory.GetFiles(@"" + sPath + "");
                                    string sFileNameC = string.Empty;
                                    foreach (var item in filePaths)
                                    {
                                        int ipos = item.LastIndexOf("\\") + 1;
                                        string sFileName = item.Substring(ipos, item.Length - ipos);
                                        if (sCopyDocumentName == sFileName.Split('.')[0])
                                        {
                                            sFileNameC = sFileName;
                                            sCopyDocumentPath = item;
                                        }
                                    }
                                    var oAttachementC = new Attachment(sCopyDocumentPath);
                                    using (MemoryStream ms = new MemoryStream())
                                    using (FileStream file = new FileStream(sCopyDocumentPath, FileMode.Open, FileAccess.Read))
                                    {
                                        byte[] bytes = new byte[file.Length];
                                        file.Read(bytes, 0, (int)file.Length);
                                        ms.Write(bytes, 0, (int)file.Length);
                                        var oResponse = oXIDocs.SaveDocuments(ms, sFileNameC);
                                        if (oResponse.bOK && oResponse.oResult != null)
                                        {
                                            int iDocumentID = 0;
                                            string sDocID = (string)oResponse.oResult;
                                            if (int.TryParse(sDocID, out iDocumentID))
                                            {
                                            }
                                            Docs.Add(iDocumentID.ToString());
                                        }
                                    }
                                    if (System.IO.File.Exists(sCopyDocumentPath))
                                    {
                                        AttachementPath.Add(sCopyDocumentPath);
                                    }
                                }
                            }
                            oEmail.EmailID = sEmail;
                            if (!string.IsNullOrEmpty(sSubject))
                            {
                                oEmail.sSubject = sSubject;
                            }
                            else if (!string.IsNullOrEmpty(oParentContentC.sSubject))
                            {
                                oEmail.sSubject = oParentContentC.sSubject;
                            }
                            else
                            {
                                oEmail.sSubject = oParentContentC.Name;
                            }
                            var oMailResult = oEmail.Sendmail(iOrgID, sMergeTempContent, oAttachments, 0, "Letter", iLeadID, AttachementPath, iPolicyID, false, true, sCallnotes, sDocIDs);//send mail with attachment
                            if (oMailResult.bOK && oMailResult.oResult != null)
                            {
                                sSentStatus = "10";
                                oResult.oTraceStack.Add(new CNV { sName = "Mail send successfully", sValue = oParentContentC.Name + " Mail send successfully to email:" + sEmail + "" });
                                oXID.SaveErrortoDB(oResult);
                                bISSend = true;
                            }
                            XIIBO oLettersBOI = new XIIBO();
                            oLettersBOI.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, "Letters_T", null);
                            //oBOICopy.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, oBOInstance.BOD.sAuditBOName);
                            oLettersBOI.SetAttribute("sMergedBodyContent", sLetterBodyContent);
                            oLettersBOI.SetAttribute("sMergedAttachmentContent", sLetterAttachmentContent);
                            oLettersBOI.SetAttribute("FKiPolicyID", sInstanceID);
                            oLettersBOI.SetAttribute("FKiProductID", sProductID);
                            oLettersBOI.SetAttribute("iSentStatus", sSentStatus);
                            oLettersBOI.SetAttribute("FKiDocID", iDocID.ToString());
                            oLettersBOI.SetAttribute("id", "");
                            var oBOIResponse = oLettersBOI.Save(oLettersBOI);

                            //string sBO = oGUIDParams.NMyInstance.Where(m => m.Key == "sBOName").Select(m => m.Value.sValue).FirstOrDefault();
                            //if (!string.IsNullOrEmpty(sBO))
                            //{
                            //string sInstanceID = oGUIDParams.NMyInstance.Where(m => m.Key == "iInstanceID").Select(m => m.Value.sValue).FirstOrDefault();

                            //if (!string.IsNullOrEmpty(sBO) && !string.IsNullOrEmpty(sInstanceID) && !string.IsNullOrEmpty(sStructureName))
                            //{
                            //oStructobj = oIXI.BOI(sBO, sInstanceID).Structure(sStructureName).XILoad(null, true);
                            //var oCResult = oParentContent.MergeContentTemplate(oParentContent, oStructobj);
                            //if (oCResult.bOK && oCResult.oResult != null)
                            //{
                            //    oEmail.EmailID = sEmail;
                            //    oEmail.sSubject = oParentContent.Name;
                            //    var oMailResult = oEmail.Sendmail(iOrgID, (string)oCResult.oResult, oAttachments);//send mail with attachment
                            //    if (oMailResult.bOK && oMailResult.oResult != null)
                            //    {
                            //        oCResult.oTraceStack.Add(new CNV { sName = "Mail send successfully", sValue = oParentContent.Name + " Mail send successfully to email:" + sEmail + "" });
                            //    }
                            //}
                            //}
                            //}
                        }
                    }
                }
                else
                {
                    oEmail.EmailID = sEmail;
                    if (!string.IsNullOrEmpty(sSubject))
                    {
                        oEmail.sSubject = sSubject;
                    }
                    var oMailResult = oEmail.Sendmail(iOrgID, "", oAttachments, 0, "Letter", iLeadID, AttachementPath, iPolicyID, false, true, sCallnotes, sDocIDs);//send mail with attachment
                    if (oMailResult.bOK && oMailResult.oResult != null)
                    {
                        sSentStatus = "10";
                        //oResult.oTraceStack.Add(new CNV { sName = "Mail send successfully", sValue = oParentContentC.Name + " Mail send successfully to email:" + sEmail + "" });
                        //oXID.SaveErrortoDB(oResult);
                        bISSend = true;
                    }
                }
                return Json(bISSend, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        //public CResult PDFGenerateiText7(string message, bool bIsPaswordProtected, string sSurNamePasswordRange, string sDOBPasswordRange)
        //{
        //    CResult oCResult = new CResult();
        //    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
        //    try
        //    {
        //        MemoryStream output = new MemoryStream();
        //        StringReader sr = new StringReader(message);
        //        //Document pdfDoc = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
        //        List<string> cssFiles = new List<string>();
        //        cssFiles.Add(@"~/Content/css/pdfSendStyle.css");
        //        //PdfPTable table = new PdfPTable(columnWidths);
        //        //table.WidthPercentage = 100f;
        //        //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
        //        using (var workStream = new MemoryStream())
        //        using (var pdfWriter = new PdfWriter(workStream))
        //        {
        //            PdfDocument pdf = new PdfDocument(pdfWriter);
        //            pdf.SetTagged();
        //            PageSize pageSize = PageSize.A4;
        //            pdf.SetDefaultPageSize(pageSize);
        //            Document document = new Document(pdf, PageSize.A4);
        //            using (document = HtmlConverter.ConvertToDocument(message, pdfWriter))
        //            {
        //                //Passes the document to a delegated function to perform some content, margin or page size manipulation
        //                //pdfModifier(document);
        //            }

        //            //Returns the written-to MemoryStream containing the PDF.   
        //            oCResult.oResult = workStream;
        //        }

        //        //using (MemoryStream memoryStream = new MemoryStream())
        //        //{
        //        //    //string watermark = "Test";

        //        //    //PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
        //        //    //pdfDoc.Open();
        //        //    //XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
        //        //    ////htmlparser.Parse(sr);
        //        //    //pdfDoc.Close();
        //        //    //byte[] bytes = memoryStream.ToArray();
        //        //    //memoryStream.Close();
        //        //    string sSurNamePW = string.Empty; string sDOBPW = string.Empty;

        //        //    //if (bIsPaswordProtected)
        //        //    //{
        //        //    //    if (!string.IsNullOrEmpty(sSurNamePasswordRange))
        //        //    //    {
        //        //    //        if (!string.IsNullOrEmpty(sSurName))
        //        //    //        {
        //        //    //            int iStartPos = Convert.ToInt32(sSurNamePasswordRange.Split('-')[0]);
        //        //    //            int iEndPos = Convert.ToInt32(sSurNamePasswordRange.Split('-')[1]);
        //        //    //            if (sSurName.Length >= iEndPos)
        //        //    //            {
        //        //    //                sSurNamePW = sSurName.Substring(iStartPos, iEndPos);
        //        //    //            }

        //        //    //        }
        //        //    //    }
        //        //    //    if (!string.IsNullOrEmpty(sDOBPasswordRange))
        //        //    //    {
        //        //    //        if (!string.IsNullOrEmpty(sDOB))
        //        //    //        {
        //        //    //            if (sDOBPasswordRange == "yyyy")
        //        //    //            {
        //        //    //                XIIBO oBOI = new XIIBO();
        //        //    //                DateTime dDate = oBOI.ConvertToDtTime(sDOB);
        //        //    //                sDOBPW = dDate.Year.ToString();
        //        //    //            }
        //        //    //        }
        //        //    //    }
        //        //    //    if (!string.IsNullOrEmpty(sSurNamePW) && !string.IsNullOrEmpty(sDOBPW))
        //        //    //    {
        //        //    //        using (MemoryStream input = new MemoryStream(bytes))
        //        //    //        {
        //        //    //            //using (MemoryStream soutput = new MemoryStream())
        //        //    //            //{
        //        //    //            string password = sSurNamePW + sDOBPW;
        //        //    //            PdfReader reader = new PdfReader(input);
        //        //    //            PdfEncryptor.Encrypt(reader, output, true, password, password, PdfWriter.ALLOW_SCREENREADERS);
        //        //    //            bytes = output.ToArray();
        //        //    //            //}
        //        //    //        }
        //        //    //    }
        //        //    //    else
        //        //    //    {
        //        //    //        output = memoryStream;
        //        //    //    }
        //        //    //}
        //        //    //else
        //        //    //{
        //        //    //    output = memoryStream;
        //        //    //}
        //        //    //Response.Clear();
        //        //    //output = memoryStream;
        //        //    oCResult.oResult = output;
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
        //        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
        //        //SaveErrortoDB(oCResult);
        //        oCResult.LogToFile();
        //    }
        //    return oCResult;
        //}
        #endregion
        [AllowAnonymous]
        public ActionResult GetMinimumDepositValue(double rPaymentCharge, double rInsurerCharge, string sGUID, int QSIID)
        {
            string obj = string.Empty;
            try
            {
                XIIBO oBO = new XIIBO();
                XIIXI oXIIXI = new XIIXI();
                double rFinalQuote = 0;
                double rAdmin = 0;
                var sSessionID = HttpContext.Session.SessionID;
                string sQuoteID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|Aggregations.id}");
                var oQuoteI = oXIIXI.BOI("Aggregations", sQuoteID, "Create");
                if (double.TryParse(oQuoteI.Attributes["rFinalQuote"].sValue, out rFinalQuote))
                {
                }
                int iProductID = 0;
                string ProductID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|Product.id}");
                if (int.TryParse(ProductID, out iProductID)) { }
                var oProductI = oIXI.BOI("Product", ProductID);
                double rDiffPaymentCharge = 0;
                double rDiffInsuerCharge = 0;
                double rDefaultPaymentCharge = 0;
                double rDefaultInsurerCharge = 0;
                if (double.TryParse(oQuoteI.Attributes["rPaymentCharge"].sValue, out rDefaultPaymentCharge)) { }
                rDiffPaymentCharge = rPaymentCharge - rDefaultPaymentCharge;
                if (double.TryParse(oQuoteI.Attributes["rInsurerCharge"].sValue, out rDefaultInsurerCharge)) { }
                rDiffInsuerCharge = rInsurerCharge - rDefaultInsurerCharge;
                rFinalQuote += rDiffInsuerCharge + rDiffPaymentCharge;
                //
                double rAddonTotal = 0;
                double rAddonAdminTotal = 0;
                double rTotalAdmin = 0;
                QueryEngine oQE = new QueryEngine();
                List<XIWhereParams> oWParams = new List<XIWhereParams>();
                XIWhereParams oWP = new XIWhereParams();
                oWP.sField = "FKiQsInstanceID";
                oWP.sOperator = "=";
                oWP.sValue = QSIID.ToString();
                oWParams.Add(oWP);
                XIWhereParams oWP1 = new XIWhereParams();
                oWP1.sField = "iStatus";
                oWP1.sOperator = "=";
                oWP1.sValue = "10";
                oWParams.Add(oWP1);
                oWParams.Add(new XIWhereParams { sField = XIConstant.Key_XIDeleted, sOperator = "=", sValue = "0" });
                oQE.AddBO("ACPurchase_T", "Create", oWParams);
                CResult oCresult = oQE.BuildQuery();
                if (oCresult.bOK && oCresult.oResult != null)
                {

                    var sSql = (string)oCresult.oResult;
                    ExecutionEngine oEE = new ExecutionEngine();
                    oEE.XIDataSource = oQE.XIDataSource;
                    oEE.sSQL = sSql;
                    var oQResult = oEE.Execute();
                    if (oQResult.bOK && oQResult.oResult != null)
                    {
                        var oBOIList = ((Dictionary<string, XIIBO>)oQResult.oResult).Values.ToList();
                        var oBOD = oQE.QParams.FirstOrDefault().BOD;
                        oBOIList.ForEach(x => x.BOD = oBOD);
                        foreach (var instance in oBOIList)
                        {
                            if (instance.Attributes.ContainsKey("rPremiumOverride"))
                            {
                                var AddonPrice = instance.Attributes["rPremiumOverride"].sValue;
                                var AddonAdminPrice = instance.Attributes["rAdmin"].sValue;
                                double rAddon = 0;
                                if (double.TryParse(AddonPrice, out rAddon))
                                {
                                    rAddonTotal += rAddon;
                                }
                                double rAddonAdmin = 0;
                                if (double.TryParse(AddonAdminPrice, out rAddonAdmin))
                                {
                                    rAddonAdminTotal += rAddonAdmin;
                                }
                            }
                        }
                    }
                }
                if (double.TryParse(oQuoteI.Attributes["zDefaultAdmin"].sValue, out rAdmin))
                {
                }
                rTotalAdmin = rAdmin + rAddonAdminTotal;
                XIAPI oXIAPI = new XIAPI();
                obj = oXIAPI.GetMinimumDepostAmount(rPaymentCharge, rInsurerCharge, rFinalQuote, rAdmin, sGUID, QSIID, iProductID, rAddonAdminTotal, rAddonTotal);
                oQuoteI.Attributes["zDefaultDeposit"].sValue = String.Format("{0:0.00}", obj);
                oQuoteI.Attributes["rPaymentCharge"].sValue = String.Format("{0:0.00}", rPaymentCharge);
                oQuoteI.Attributes["rInsurerCharge"].sValue = String.Format("{0:0.00}", rInsurerCharge);
                double rMinDeposit = 0;
                if (double.TryParse(obj, out rMinDeposit)) { }
                var PFSchemeID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iPFSchemeID}");
                int iPFSchemeID = 0;
                if (int.TryParse(PFSchemeID, out iPFSchemeID))
                { }
                var MonthlyAmount = oXIAPI.GetMonthlyPremiumAmount(rFinalQuote, rMinDeposit, iProductID, rAddonAdminTotal, rAddonTotal, iPFSchemeID);
                double rMonthlyTotal = (MonthlyAmount * 10) + rMinDeposit;
                oQuoteI.Attributes["rMonthlyPrice"].sValue = String.Format("{0:0.00}", MonthlyAmount);
                oQuoteI.Attributes["rMonthlyTotal"].sValue = String.Format("{0:0.00}", rMonthlyTotal);
                oQuoteI.Attributes["rFinalQuote"].sValue = String.Format("{0:0.00}", rFinalQuote);
                oQuoteI.Attributes["rAddonPrice"].sValue = String.Format("{0:0.00}", rAddonTotal);
                oQuoteI.Attributes["rAddonAdmin"].sValue = String.Format("{0:0.00}", rAddonAdminTotal);
                oQuoteI.Attributes["rTotalAdmin"].sValue = String.Format("{0:0.00}", rTotalAdmin);
                oQuoteI.Attributes["rTotal"].sValue = String.Format("{0:0.00}", obj);
                oQuoteI.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
                oBO.Save(oQuoteI);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult UpdateQuotePrice(string sPaymentType, string sGUID)
        {
            string obj = string.Empty;
            try
            {
                XIIBO oBO = new XIIBO();
                XIIXI oXIIXI = new XIIXI();
                var sSessionID = HttpContext.Session.SessionID;
                string sQuoteID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|Aggregations.id}");
                var PFSchemeID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iPFSchemeID}");
                int iPFSchemeID = 0;
                if (int.TryParse(PFSchemeID, out iPFSchemeID))
                { }
                var oQuoteI = oXIIXI.BOI("Aggregations", sQuoteID, "");
                if (sPaymentType == "Annual")
                {
                    oQuoteI.Attributes["rFinalPremium"].sValue = oQuoteI.Attributes["rTotal"].sValue;
                    oQuoteI.Attributes["rFinalPremium"].bDirty = true;
                    oQuoteI.Attributes["rPremiumFinanceAmount"].sValue = "0.00";
                    oQuoteI.Attributes["rPremiumFinanceAmount"].bDirty = true;
                    oQuoteI.Attributes["iPaymentType"] = new XIIAttribute { sName = "iPaymentType", sValue = "10", bDirty = true };
                    //oQuoteI.Attributes["rInterestAmount"].sValue = "0.00";
                    //oQuoteI.Attributes["rInterestAmount"].bDirty = true;
                    //oQuoteI.Attributes["rInterestRate"].sValue = "0.00";
                    //oQuoteI.Attributes["rInterestRate"].bDirty = true;
                    oQuoteI.Attributes["rPayableAmount"].sValue = oQuoteI.Attributes["rTotal"].sValue;
                    oQuoteI.Attributes["rPayableAmount"].bDirty = true;
                }
                else
                {
                    float rMonthlyPrice = 0;
                    if (float.TryParse(oQuoteI.Attributes["rMonthlyPrice"].sValue, out rMonthlyPrice))
                    { }
                    float rMonthlyTotal = 0;
                    if (float.TryParse(oQuoteI.Attributes["rMonthlyTotal"].sValue, out rMonthlyTotal))
                    { }
                    float rQuotePrice = 0;
                    if (float.TryParse(oQuoteI.Attributes["rTotal"].sValue, out rQuotePrice))
                    { }
                    float rDepositAmount = 0;
                    if (float.TryParse(oQuoteI.Attributes["zDefaultDeposit"].sValue, out rDepositAmount))
                    { }

                    oQuoteI.Attributes["rPayableAmount"].sValue = oQuoteI.Attributes["zDefaultDeposit"].sValue;
                    oQuoteI.Attributes["rPayableAmount"].bDirty = true;
                    oQuoteI.Attributes["rPremiumFinanceAmount"].sValue = String.Format("{0:0.00}", (rQuotePrice - rDepositAmount));
                    oQuoteI.Attributes["rPremiumFinanceAmount"].bDirty = true;
                    oQuoteI.Attributes["rFinalPremium"].sValue = oQuoteI.Attributes["rTotal"].sValue;
                    oQuoteI.Attributes["rFinalPremium"].bDirty = true;
                    oQuoteI.Attributes["iPaymentType"] = new XIIAttribute { sName = "iPaymentType", sValue = "20", bDirty = true };
                    //oQuoteI.Attributes["rInterestAmount"].sValue = String.Format("{0:0.00}", (rMonthlyTotal - rQuotePrice));
                    //oQuoteI.Attributes["rInterestAmount"].bDirty = true;
                    double zGrossRatePC = 0;
                    if (iPFSchemeID > 0)
                    {
                        var oPFSchemeI = oXIIXI.BOI("PFScheme_T", iPFSchemeID.ToString());
                        if (oPFSchemeI != null && oPFSchemeI.Attributes != null && oPFSchemeI.Attributes.ContainsKey("zGrossRatePC"))
                        {
                            if (double.TryParse(oPFSchemeI.Attributes["zGrossRatePC"].sValue, out zGrossRatePC)) { }
                        }
                    }
                    else
                    {
                        XID1Click oD1Click = new XID1Click();
                        string sQuery = "select sDefaultValue from PFBNPFields_T WHERE sDefaultValue is not null AND sServiceName='NEWBUSINESS' AND sXIFieldName='CreditProductCodePF'";
                        oD1Click.Query = sQuery;
                        oD1Click.Name = "PFBNPFields_T";
                        var oresult = oD1Click.OneClick_Run(false);
                        foreach (var result in oresult)
                        {
                            if (result.Value.Attributes != null && result.Value.Attributes.ContainsKey("sDefaultValue"))
                            {
                                string ProductCode = result.Value.Attributes["sDefaultValue"].sValue;
                                if (!string.IsNullOrEmpty(ProductCode))
                                {
                                    string Query = "select zGrossRatePC, ID from PFScheme_T where sSchemeRefCode=" + "'" + ProductCode + "'";
                                    oD1Click.Query = Query;
                                    oD1Click.Name = "PFScheme_T";
                                    var oresult1 = oD1Click.OneClick_Run(false);
                                    string GrossRate = "";
                                    string sPFSchemeID = string.Empty;
                                    foreach (var item in oresult1.Values)
                                    {
                                        GrossRate = item.Attributes["zGrossRatePC"].sValue;
                                        sPFSchemeID = item.Attributes["ID"].sValue;
                                    }
                                    if (double.TryParse(GrossRate, out zGrossRatePC))
                                    {
                                        if (int.TryParse(sPFSchemeID, out iPFSchemeID))
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //oQuoteI.Attributes["rInterestRate"].sValue = String.Format("{0:0.00}", zGrossRatePC);
                    //oQuoteI.Attributes["rInterestRate"].bDirty = true;
                    oQuoteI.Attributes["FKiPFSchemeID"].sValue = iPFSchemeID.ToString();
                    oQuoteI.Attributes["FKiPFSchemeID"].bDirty = true;
                }
                oQuoteI.Attributes["id"].bDirty = true;
                oBO.Save(oQuoteI);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SchedularNotification()
        {
            try
            {
                string sOrgID = Convert.ToString(SessionManager.OrganizationID);
                string sOneClickName = "Requirement Chaser List";
                XID1Click o1ClickD = new XID1Click();
                if (!string.IsNullOrEmpty(sOneClickName))
                {
                    //Get 1-Click Defintion             
                    o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, sOneClickName, null);
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, o1ClickD.BOID.ToString());
                    o1ClickD.BOD = oBOD;
                    o1ClickD.sBOName = oBOD.Name;
                    Dictionary<string, XIIBO> oRes = o1ClickD.OneClick_Execute();
                    List<XIIBO> oBOIList = new List<XIIBO>();
                    oBOIList = oRes.Values.ToList();
                    if (oBOIList != null && oBOIList.Count() > 0)
                    {
                        foreach (var item in oBOIList)
                        {
                            double iDays = 0;
                            if (item.Attributes.ContainsKey("dDue"))
                            {
                                //var sDueDate = item.AttributeI("dDue").sValue;
                                var dDueDate = item.ConvertToDtTime(item.AttributeI("dDue").sValue);
                                var dCurrentDate = DateTime.Now;
                                if (dDueDate >= dCurrentDate)
                                {
                                    iDays = dDueDate.Date.Subtract(dCurrentDate.Date).TotalDays;
                                    if (iDays < 5)
                                    {
                                        XIContentEditors oContent = new XIContentEditors();
                                        List<XIIBO> nBOI = new List<XIIBO>();
                                        Policy oPolicy = new Policy();
                                        List<CNV> oParams = new List<CNV>();
                                        CNV oCNV = new CNV();
                                        XIBOInstance oBOIns = new XIBOInstance();
                                        oBOIns.oStructureInstance = new Dictionary<string, List<XIIBO>>();
                                        nBOI.Add(item);
                                        oBOIns.oStructureInstance[oBOD.Name] = nBOI;
                                        var oContentDef = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, "Requirement Chaser", "0");
                                        if (oContentDef != null && oContentDef.Count() > 0)
                                        {
                                            oContent = oContentDef.FirstOrDefault();
                                        }
                                        var Result = oContent.MergeContentTemplate(oContent, oBOIns);
                                        if (Result.bOK && Result.oResult != null)
                                        {
                                            XIInfraNotifications oNotifications = new XIInfraNotifications();
                                            string sUserID = string.Empty; string sInstanceID = string.Empty; string sDocumentID = string.Empty;
                                            if (item.Attributes.ContainsKey("FKiUserID"))
                                            {
                                                sUserID = item.AttributeI("FKiUserID").sValue;
                                            }
                                            if (item.Attributes.ContainsKey("FKiPolicyID"))
                                            {
                                                sInstanceID = item.AttributeI("FKiPolicyID").sValue;
                                            }
                                            if (item.Attributes.ContainsKey("FKizXDoc"))
                                            {
                                                sDocumentID = item.AttributeI("FKizXDoc").sValue;
                                            }
                                            oNotifications.iStatus = 10;
                                            oNotifications.Create(sUserID, oContent.Name, sDocumentID, oContent.sSubject, (string)Result.oResult, sInstanceID, sOrgID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult SchedularQuotes()
        {
            try
            {
                string sOrgID = Convert.ToString(SessionManager.OrganizationID);
                string sOneClickName = "Quotes FollowUp List";
                XID1Click o1ClickD = new XID1Click();
                if (!string.IsNullOrEmpty(sOneClickName))
                {
                    //Get 1-Click Defintion             
                    o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, sOneClickName, null);
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, o1ClickD.BOID.ToString());
                    o1ClickD.BOD = oBOD;
                    o1ClickD.sBOName = oBOD.Name;
                    Dictionary<string, XIIBO> oRes = o1ClickD.OneClick_Execute();
                    List<XIIBO> oBOIList = new List<XIIBO>();
                    oBOIList = oRes.Values.ToList();
                    if (oBOIList != null && oBOIList.Count() > 0)
                    {
                        List<string> oUsersList = new List<string>();
                        if (oBOIList.Any(x => x.Attributes.ContainsKey("FKiUserID")))
                        {
                            oUsersList = oBOIList.Select(x => x.AttributeI("FKiUserID").sValue).ToList();
                            oUsersList = oUsersList.Distinct().ToList();
                            XIContentEditors oContent = new XIContentEditors();
                            var oContentDef = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, "Follow Up", "0");
                            if (oContentDef != null && oContentDef.Count() > 0)
                            {
                                oContent = oContentDef.FirstOrDefault();
                                if (oContent != null)
                                {
                                    foreach (var sUser in oUsersList)
                                    {
                                        XIInfraNotifications oNotifications = new XIInfraNotifications();
                                        var sInstanceID = oBOIList.Where(x => x.AttributeI("FKiUserID").sValue == sUser).Select(x => x.AttributeI("FKiQSInstanceID").sValue).FirstOrDefault();
                                        oNotifications.iStatus = 10;
                                        oNotifications.Create(sUser, oContent.Name, "", oContent.sSubject, oContent.Content, sInstanceID, sOrgID);
                                    }
                                }
                            }
                        }
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult NavigateQSStep(int iStepID, string sGUID, string sType, int iQSIID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                XIIQS oQSInstance = new XIIQS();
                oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
                var CurrentStepStage = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == iStepID).FirstOrDefault().iStage;
                //var MaxStep=oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iMaxStage).FirstOrDefault();
                //if(oQSInstance.iStage >0 && oQSInstance.iStage > CurrentStepStage)
                //{
                //    oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iStepID).FirstOrDefault().bIsLock = true;
                //}
                oQSInstance.Steps.Values.ToList().ForEach(m => m.bIsCurrentStep = false);
                oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == iStepID).FirstOrDefault().bIsCurrentStep = true;
                oQSInstance.iCurrentStepID = iStepID;
                ViewBag.sGUID = sGUID;
                oQSInstance.sCurrentStepName = oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == iStepID).Select(m => m.Value.sName).FirstOrDefault();
                XIIXI oXII = new XIIXI();
                List<CNV> oWhrParams = new List<CNV>();
                oWhrParams.Add(new CNV { sName = "FKiQSInstanceID", sValue = iQSIID.ToString() });
                XIIBO oBOI = new XIIBO();

                oBOI = oXII.BOI("Lead_T", null, "create", oWhrParams);
                if (oBOI != null)
                {
                    if (oBOI.Attributes.ContainsKey("sQSStage"))
                    {
                        oBOI.Attributes["sQSStage"].sValue = oQSInstance.sCurrentStepName;
                        oBOI.Attributes["sQSStage"].bDirty = true;
                        oBOI.Save(oBOI);
                    }
                }
                XIDQS oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSInstance.FKiQSDefinitionID.ToString());
                XIDQS oQSDC = (XIDQS)oQSD.Clone(oQSD);
                var oQSI = oXII.BOI("QS Instance", iQSIID.ToString(), "updatestage");
                if (oQSI != null && oQSI.Attributes.ContainsKey("iStage"))
                {
                    int iStage = 0;
                    var sStage = oQSI.Attributes["iStage"].sValue;
                    if (int.TryParse(sStage, out iStage))
                    {
                        var oStepD = oQSDC.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                        if (iStage > oStepD.iStage && oStepD.iCutStage <= iStage && iStage < oStepD.iLockStage)
                        {
                            oQSI.Attributes["iStage"].sValue = oStepD.iCutStage.ToString();
                            oQSI.Attributes["iStage"].bDirty = true;
                            oQSI.Save(oQSI);
                        }
                    }
                }

                var oCurrentStepD = oQSDC.Steps.Values.Where(m => m.ID == iStepID).FirstOrDefault();
                bool IsLoad = true;
                if (oCurrentStepD.iLockStage != 0 && oCurrentStepD.iLockStage <= oQSInstance.iStage && oQSInstance.QSDefinition.bIsStage)
                {
                    IsLoad = false;
                }
                if (oCurrentStepD.bIsReload && IsLoad)
                {
                    oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, oCurrentStepD.ID, sGUID);
                    oCurrentStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oCurrentStepD.ID).FirstOrDefault();
                }
                if (oCurrentStepD != null)
                {
                    var oCD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oCurrentStepD.ID).FirstOrDefault();
                    if (oCurrentStepD.FieldDefs.Count > 0)
                    {
                        foreach (var oSecD in oCurrentStepD.Sections)
                        {
                            foreach (var FieldD in oSecD.Value.FieldDefs)
                            {
                                if (!string.IsNullOrEmpty(FieldD.Value.FieldOrigin.sMinDate) && (FieldD.Value.FieldOrigin.sMinDate.Contains("xi.s")))
                                {
                                    var sSessionID = HttpContext.Session.SessionID;
                                    XIDScript oXIScript = new XIDScript();
                                    oXIScript.sScript = FieldD.Value.FieldOrigin.sMinDate.ToString();
                                    var oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                                    if (oCResult.bOK && oCResult.oResult != null)
                                    {
                                        var sVal = (string)oCResult.oResult;
                                        sVal = Utility.GetDateResolvedValue(sVal, "yyyy-MM-dd");
                                        FieldD.Value.FieldOrigin.sMinDate = sVal;
                                    }
                                }
                                if (!string.IsNullOrEmpty(FieldD.Value.FieldOrigin.sMaxDate) && (FieldD.Value.FieldOrigin.sMaxDate.Contains("xi.s")))
                                {
                                    var sSessionID = HttpContext.Session.SessionID;
                                    XIDScript oXIScript = new XIDScript();
                                    oXIScript.sScript = FieldD.Value.FieldOrigin.sMaxDate.ToString();
                                    var oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                                    if (oCResult.bOK && oCResult.oResult != null)
                                    {
                                        var sVal = (string)oCResult.oResult;
                                        sVal = Utility.GetDateResolvedValue(sVal, "yyyy-MM-dd");
                                        FieldD.Value.FieldOrigin.sMaxDate = sVal;
                                    }
                                }
                            }
                        }
                    }
                    Dictionary<string, XIDQSStep> Steps = new Dictionary<string, XIDQSStep>();
                    foreach (var Step in oQSD.Steps.Values.ToList())
                    {
                        Steps[Step.sName] = new XIDQSStep() { ID = Step.ID, sName = Step.sName, iOrder = Step.iOrder, sDisplayName = Step.sDisplayName, sIsHidden = Step.sIsHidden, iStage = Step.iStage, iLockStage = Step.iLockStage, iCutStage = Step.iLockStage };
                    }
                    oQSInstance.QSDefinition.Steps = Steps;
                    oQSInstance.QSDefinition.Steps[oCurrentStepD.sName] = oCurrentStepD;
                }
                if (sType.ToLower() == "public")
                {
                    return PartialView("_QuestionSet", oQSInstance);
                }
                else
                {
                    return PartialView("_QuestionSetInternal", oQSInstance);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return PartialView("~/views/Shared/Error.cshtml");
            }
        }
        [AllowAnonymous]
        public ActionResult GetSectionInstance(int ParentID, string sGUID, string sSectionID, string sCurrentStepName, string iQSDefinitionID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIInfraCache xifCache = new XIInfraCache();
            XIIQSSection oSecI = new XIIQSSection();
            try
            {
                XIIQS oXIIQS = new XIIQS();
                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}", ParentID.ToString(), null, null);
                XIDQS oQSD = (XIDQS)xifCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDefinitionID);
                var oSecD = oQSD.Steps[sCurrentStepName].Sections[sSectionID];
                oSecI = oXIIQS.LoadSectionInstance(oSecD, sGUID);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(oSecI, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public XIIQS ReCalculate(int XiLinkID, string sGUID, int QsDefinitionid, int iQSIID, List<CNV> oNV)
        {
            string sDatabase = SessionManager.CoreDatabase;
            XIIQS oQSInstance = new XIIQS();
            try
            {
                XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
                XIDQS oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSInstance.FKiQSDefinitionID.ToString(), null, null, 0, 0);
                foreach (var nv in oNV)
                {
                    if (oQSInstance.XIValues.ContainsKey(nv.sName) && !string.IsNullOrEmpty(nv.sValue))
                    {
                        oQSInstance.XIValues[nv.sName].sValue = nv.sValue;                        
                        Dictionary<string, object> fieldParams = new Dictionary<string, object>();
                        fieldParams["FKiQSInstanceID"] = oQSInstance.ID;
                        //fieldParams["FKiQSSectionDefinitionID"] = sec.Value.FKiStepSectionDefinitionID;
                        fieldParams["FKiFieldOriginID"] = oQSInstance.XIValues[nv.sName].FKiFieldOriginID;
                        var oFIns = Connection.Select<XIIValue>("XIFieldInstance_T", fieldParams).FirstOrDefault();
                        var oStepD = oQSD.Steps.Values.Where(m => m.ID == oFIns.FKiQSStepDefinitionID).FirstOrDefault();
                        var oStep = oQSInstance.Steps.Values.Where(m => m.FKiQSStepDefinitionID == oFIns.FKiQSStepDefinitionID).ToList();
                        foreach (var oStepI in oStep)
                        {
                            var oSecD = oStepD.Sections.Where(m => m.Value.ID == oFIns.FKiQSSectionDefinitionID).Select(m => m.Value).FirstOrDefault();
                            var oSec = oStepI.Sections.Where(m => m.Value.FKiStepSectionDefinitionID == oFIns.FKiQSSectionDefinitionID).ToList();
                            foreach (var oSecI in oSec)
                            {
                                
                                if (oSecI.Value.XIValues.ContainsKey(nv.sName))
                                {
                                    var FOrigin = oSecD.FieldDefs.Where(m => m.Value.ID == oSecI.Value.XIValues[nv.sName].FKiFieldDefinitionID).Select(m => m.Value).FirstOrDefault();
                                    oFIns.sValue = nv.sValue;
                                    oFIns.dValue = DateTime.Now;
                                    if (FOrigin.FieldOrigin.bIsOptionList)
                                    {
                                        oFIns.sDerivedValue = FOrigin.FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == nv.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                    }
                                    //oQSInstance.XIValues[nv.sName].FKiQSInstanceID = oQSInstance.ID;
                                    oFIns = Connection.Update<XIIValue>(oFIns, "XIFieldInstance_T", "ID");
                                    oSecI.Value.XIValues[nv.sName].sValue = nv.sValue;
                                }
                            }
                        }
                    }
                }
                oCache.Set_QuestionSetCache("QuestionSetCache", sGUID, oQSInstance.ID, oQSInstance);
                GetXiLinkContent(XiLinkID, sGUID);
                //GetXiLinkContent(5012, sGUID);

                return oQSInstance;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public void LogSignalR(int iQSIID, string sLog)
        {
            Common.SaveErrorLog("Log: SignalrEventConnection: " + sLog + " " + iQSIID, "XIDNA");
        }

        [HttpPost]
        [AllowAnonymous]
        public void LogJqueryError(string sContext, string sMessage)
        {
            Common.SaveErrorLog("JQuery: Context - " + sContext + ", Message - " + sMessage, "XIDNA");
        }
        public ActionResult SaveAuditBO(int iBODID, string sAuditBOName, string sBOName, string sInstanceID, string sAuditContent = "", string sGUID = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIBO oBOI = new XIIBO();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int iBOIID = 0;
                if (!string.IsNullOrEmpty(sAuditBOName))
                {
                    XIIBO oAuditBOI = new XIIBO();
                    oAuditBOI.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, sAuditBOName, null);
                    var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, null);
                    if (!string.IsNullOrEmpty(BOD.sAuditBOfield) && BOD.Attributes.ContainsKey(BOD.sAuditBOfield) && !string.IsNullOrEmpty(sInstanceID))
                    {
                        if (!string.IsNullOrEmpty(BOD.Attributes[BOD.sAuditBOfield].sFKBOName))
                        {
                            var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BOD.Attributes[BOD.sAuditBOfield].sFKBOName, null);
                            XIIXI oXII = new XIIXI();
                            oBOI = oXII.BOI(sBOName, sInstanceID);
                            if (oBOI != null && oBOI.Attributes.Count() > 0 && oBOI.Attributes.ContainsKey(BOD.sAuditBOfield) && !string.IsNullOrEmpty(oBOI.Attributes[BOD.sAuditBOfield].sValue))
                            {
                                if (int.TryParse(oBOI.Attributes[BOD.sAuditBOfield].sValue, out iBOIID)) { }
                            }
                            oAuditBOI.SetAttribute("FksParentBOName", oBOD.Name);
                            oAuditBOI.SetAttribute("FKiParentInstanceID", iBOIID.ToString());
                            oAuditBOI.SetAttribute("FkiParentBOID", oBOD.BOID.ToString());
                        }
                    }
                    //oBOICopy.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, oBOInstance.BOD.sAuditBOName);
                    oAuditBOI.SetAttribute("FKiBOID", iBODID.ToString());
                    oAuditBOI.SetAttribute("sBOName", sBOName);
                    var sSessionID = HttpContext.Session.SessionID;
                    if (string.IsNullOrEmpty(sInstanceID))
                    {
                        sInstanceID = oCache.Get_ParamVal(sSessionID, sGUID, "", "{XIP|ACPolicy_T.id}");
                    }
                    if (string.IsNullOrEmpty(sAuditContent))
                    {
                        oAuditBOI.SetAttribute("sData", BOD.LabelName + " opened by user " + oUser.sFirstName + " " + oUser.sLastName + "");
                    }
                    else
                    {
                        oAuditBOI.SetAttribute("sData", sAuditContent + " by " + oUser.sFirstName + " " + oUser.sLastName + "");
                    }
                    oAuditBOI.SetAttribute("sOldData", "");
                    //oAuditBOI.SetAttribute(XICreatedBy, oUser.sFirstName + " " + oUser.sLastName);
                    //oAuditBOI.SetAttribute(XIConstant.Key_XICrtdWhn, DateTime.Now.ToString());
                    oAuditBOI.SetAttribute("sType", "User View");
                    oAuditBOI.SetAttribute("sActivity", "User " + BOD.LabelName + " View");
                    oAuditBOI.SetAttribute("FKiInstanceID", sInstanceID);
                    var oAuditBOResponse = oBOI.Update_TODB(oAuditBOI, "id");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetbLock(int iBODID, string sBOName, string sInstanceID)
        {
            string sDatabase = SessionManager.CoreDatabase; string sMessage = string.Empty;
            try
            {
                XIIBO oBOI = new XIIBO(); XIIXI oIXI = new XIIXI();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }

                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                List<CNV> oWhereParams = new List<CNV>();
                oWhereParams.Add(new CNV { sName = "FKiUserID", sValue = oUser.UserID.ToString() });
                oWhereParams.Add(new CNV { sName = "FKiInstanceID", sValue = sInstanceID });
                oWhereParams.Add(new CNV { sName = "FKiBOID", sValue = iBODID.ToString() });
                oWhereParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                //oWhereParams.Add(new CNV { sName = "bIsLock", sValue = "0" });
                oBOI = oIXI.BOI("Lock_T", null, null, oWhereParams);
                if (oBOI != null && oBOI.Attributes != null)
                {
                    if (oBOI.Attributes.ContainsKey(XIConstant.Key_XIDeleted))
                    {
                        oBOI.Attributes[XIConstant.Key_XIDeleted].sValue = "1";
                        oBOI.Attributes[XIConstant.Key_XIDeleted].bDirty = true;
                    }
                    var oBOIResponse = oBOI.Update_TODB(oBOI, "id");
                }
                bool bIsLock = CheckbISLock(iBODID, sInstanceID, iUserID.ToString());
                //else
                //{
                XIIBO oLockBOI = new XIIBO();
                oLockBOI.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, "Lock_T", null);
                //oBOICopy.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, oBOInstance.BOD.sAuditBOName);
                oLockBOI.SetAttribute("FKiBOID", iBODID.ToString());
                oLockBOI.SetAttribute("FKsBOName", sBOName);
                oLockBOI.SetAttribute(XIConstant.Key_XICrtdBy, oUser.sFirstName + " " + oUser.sLastName);
                oLockBOI.SetAttribute(XIConstant.Key_XICrtdWhn, DateTime.Now.ToString());
                oLockBOI.SetAttribute("FKiInstanceID", sInstanceID);
                oLockBOI.SetAttribute(XIConstant.Key_XIDeleted, "0");
                oLockBOI.SetAttribute("FKiUserID", oUser.UserID.ToString());
                string sLock = string.Empty;
                if (bIsLock == true)
                {
                    sLock = "1";
                }
                else
                {
                    sLock = "0";
                }
                oLockBOI.SetAttribute("bIsLock", sLock);
                var oLockBOResponse = oLockBOI.Update_TODB(oLockBOI, "id");
                //}
                if (bIsLock == true)
                {
                    sMessage = "You are unable to update this search,you can view the details";
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return Json(sMessage, JsonRequestBehavior.AllowGet);
        }

        public bool CheckbISLock(int iBODID, string sInstanceID, string sUserID)
        {
            string sDatabase = SessionManager.CoreDatabase; bool bIsLock = false;
            try
            {
                XIIBO oBOI = new XIIBO(); XIIXI oIXI = new XIIXI();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                List<CNV> oWhereParams = new List<CNV>();
                oWhereParams.Add(new CNV { sName = "FKiUserID", sValue = oUser.UserID.ToString() });
                oWhereParams.Add(new CNV { sName = "FKiInstanceID", sValue = sInstanceID });
                oWhereParams.Add(new CNV { sName = "FKiBOID", sValue = iBODID.ToString() });
                oWhereParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                oBOI = oIXI.BOI("Lock_T", null, null, oWhereParams);
                if (oBOI != null && oBOI.Attributes != null)
                {
                    bIsLock = false;
                }
                else
                {
                    oWhereParams = new List<CNV>();
                    oWhereParams.Add(new CNV { sName = "FKiInstanceID", sValue = sInstanceID });
                    oWhereParams.Add(new CNV { sName = "FKiBOID", sValue = iBODID.ToString() });
                    oWhereParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                    oBOI = oIXI.BOI("Lock_T", null, null, oWhereParams);
                    if (oBOI != null && oBOI.Attributes != null)
                    {
                        bIsLock = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
            }
            return bIsLock;
        }

        public ActionResult UpdatebLock(string sBOName, string sInstanceID, string sIsLock)
        {
            string sDatabase = SessionManager.CoreDatabase; string sMessage = string.Empty;
            try
            {
                XIIBO oBOI = new XIIBO(); XIIXI oIXI = new XIIXI();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                if (!string.IsNullOrEmpty(sIsLock) && sIsLock.ToLower() == "false")
                {
                    sIsLock = "0";
                }
                else if (!string.IsNullOrEmpty(sIsLock) && sIsLock.ToLower() == "true")
                {
                    sIsLock = "1";
                }
                string sOrgName = SessionManager.OrganisationName;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                if (!string.IsNullOrEmpty(sBOName) && !string.IsNullOrEmpty(sInstanceID))
                {
                    List<CNV> oWhereParams = new List<CNV>();
                    oWhereParams.Add(new CNV { sName = "FKiUserID", sValue = oUser.UserID.ToString() });
                    oWhereParams.Add(new CNV { sName = "FKiInstanceID", sValue = sInstanceID });
                    oWhereParams.Add(new CNV { sName = "FKsBOName", sValue = sBOName });
                    oWhereParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                    oBOI = oIXI.BOI("Lock_T", null, null, oWhereParams);
                    if (oBOI != null && oBOI.Attributes != null)
                    {
                        if (oBOI.Attributes.ContainsKey(XIConstant.Key_XIDeleted))
                        {
                            oBOI.Attributes[XIConstant.Key_XIDeleted].sValue = "1";
                            oBOI.Attributes[XIConstant.Key_XIDeleted].bDirty = true;
                        }
                        if (oBOI.Attributes.ContainsKey("bIsLock"))
                        {
                            oBOI.Attributes["bIsLock"].sValue = sIsLock;
                            oBOI.Attributes["bIsLock"].bDirty = true;
                        }
                        var oBOIResponse = oBOI.Update_TODB(oBOI, "id");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult ValidateSSL()
        {
            try
            {
                Policy oPolicy = new Policy();
                //oPolicy.HTTPRequest(34767, 0, EnumRoles.WebUsers.ToString(), "");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult RemoveSectionInstance(string sSectionID, int iXIComponentID, string sGUID)
        {
            List<CNV> oParams = new List<CNV>(); XIIXI oIXI = new XIIXI();
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            try
            {
                XIDComponent oComponent = new XIDComponent();
                string sDatabase = SessionManager.CoreDatabase;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                oParams.AddRange(oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue }));
                if (iXIComponentID != 0 && iXIComponentID == 2)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["FKiComponentID"] = iXIComponentID;
                    ListParams["iStepSectionID"] = sSectionID;
                    oComponent.Params = Connection.Select<XIDComponentParam>("XIComponentParams_T", ListParams).ToList();
                    string sBOName = oComponent.Params.Where(x => x.sName == "BO").Select(x => x.sValue).FirstOrDefault();
                    string sGroupName = oComponent.Params.Where(x => x.sName == "Group").Select(x => x.sValue).FirstOrDefault();
                    string sInstanceID = oComponent.Params.Where(x => x.sName == "iInstanceID").Select(x => x.sValue).FirstOrDefault();
                    sInstanceID = oParams.Where(x => x.sName == sInstanceID).Select(x => x.sValue).FirstOrDefault();
                    var oBOI = oIXI.BOI(sBOName, sInstanceID, sGroupName);
                    if (oBOI != null && oBOI.Attributes != null && oBOI.Attributes.Count() > 0)
                    {
                        oBOI.Attributes.Where(m => m.Value.sName.ToLower() != oBOI.BOD.sPrimaryKey.ToLower()).ToList().ForEach(m => m.Value.sValue = "");
                        oBOI.Attributes.Where(m => m.Value.sName.ToLower() != oBOI.BOD.sPrimaryKey.ToLower()).ToList().ForEach(m => m.Value.bDirty = true);
                        oBOI.Save(oBOI, false);
                    }
                    //var oBODisplay = BusinessObjectsRepository.DeleteFormData(0, sBOName, sGroupName, Convert.ToInt32(sInstanceID), sVisualisation, iUserID, sOrgName, sDatabase, null);
                }
                if (iXIComponentID != 0 && iXIComponentID == 3)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["FKiComponentID"] = iXIComponentID;
                    ListParams["iStepSectionID"] = sSectionID;
                    oComponent.Params = Connection.Select<XIDComponentParam>("XIComponentParams_T", ListParams).ToList();
                    string iOneClickID = oComponent.Params.Where(x => x.sName == "1ClickID").Select(x => x.sValue).FirstOrDefault();
                    var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, iOneClickID.ToString());
                    var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                    //Get BO Definition
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, o1ClickD.BOID.ToString());
                    o1ClickC.BOD = oBOD;
                    o1ClickC.sBOName = oBOD.Name;
                    o1ClickC.ReplaceFKExpressions(oParams);
                    Dictionary<string, XIIBO> oRes = o1ClickC.OneClick_Execute();
                    foreach (var oInstance in o1ClickC.oDataSet.Values)
                    {
                        oInstance.BOD = oBOD;
                        var Response = oInstance.Delete(oInstance);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExecuteXIScript(string sScript, string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            string sValue = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(sScript))
                {
                    CResult oCR = new CResult();
                    XIDScript oXIScript = new XIDScript();
                    oXIScript.sScript = sScript.ToString();
                    oCR = oXIScript.Execute_Script(sGUID, sSessionID);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        sValue = (string)oCR.oResult;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(sValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TriggerXILink(string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            try
            {
                Dictionary<string, string> oResult = new Dictionary<string, string>();
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                var TriggerXILinks = oGUIDParams.NMyInstance.Values.Where(m => m.sType == "XILinkTrigger").ToList();
                foreach (var xiLink in TriggerXILinks)
                {
                    var XiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, xiLink.sValue.ToString());
                    var oRes1 = XIMethod(XiLink, sGUID);
                    var oRes = oRes1 as JsonResult;
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var result = serializer.Deserialize<Dictionary<string, string>>(serializer.Serialize(oRes.Data));
                    oResult = oResult.Concat(result).GroupBy(d => d.Key)
             .ToDictionary(d => d.Key, d => d.First().Value);
                }
                return Json(oResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddTriggerXILink(int iXILinkID, string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                oCache.Set_ParamVal(sSessionID, sGUID, null, "XILinkTrigger_" + iXILinkID, iXILinkID.ToString(), "XILinkTrigger", null);
                //var XiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, null, iXILinkID.ToString());
                //var oRes = XIMethod(XiLink, sGUID);                
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public ActionResult GetQSDefinitionByInstanceID(string sQSIID, string sType)
        {
            var sSessionID = HttpContext.Session.SessionID;
            string sValue = string.Empty;
            int iQSDefinitionID = 0; string sURL = string.Empty;
            try
            {
                int iQSIID = 0;
                if (int.TryParse(sQSIID, out iQSIID)) { }
                XIIQS oQSInstance = new XIIQS();
                oQSInstance.FKiQSDefinitionID = iQSDID;
                XIDXI oDXI = new XIDXI();
                XIDQS oQSD = new XIDQS();
                XIIXI oXI = new XIIXI();
                oQSInstance = oXI.GetQSXIValuesByQSIID(iQSIID);//.GetXIValuesByQSIID(iQSSIID);
                oQSInstance.ID = iQSIID;
                List<CNV> oWhereParams = new List<CNV>();
                oWhereParams.Add(new CNV { sName = "FKiSourceID", sValue = oQSInstance.FKiSourceID.ToString() });
                oWhereParams.Add(new CNV { sName = "FKiClassID", sValue = oQSInstance.FKiClassID.ToString() });
                oWhereParams.Add(new CNV { sName = "sType", sValue = sType });

                var oQSMaPI = oXI.BOI("XIQSClassMapping_T", null, null, oWhereParams);
                if (oQSMaPI != null && oQSMaPI.Attributes.ContainsKey("FKiQSDefinitionID"))
                {
                    var sQSDID = oQSMaPI.AttributeI("FKiQSDefinitionID").sValue;
                    if (int.TryParse(sQSDID, out iQSDefinitionID)) { }
                }
                if (oQSMaPI != null && oQSMaPI.Attributes.ContainsKey("sURL"))
                {
                    sURL = oQSMaPI.AttributeI("sURL").sValue;
                }
                var oQSDefinition = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDefinitionID.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(new { iQSDefinitionID = iQSDefinitionID, sURL = sURL }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetparentQSIID(string sQSIID, string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            string sValue = string.Empty;
            int iQSIID = 0; string sParentGUID = string.Empty;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                string sQSID = oCache.Get_ParamVal(sSessionID, sGUID, "", "{XIP|iRenewalQSInstanceID}");
                if (int.TryParse(sQSID, out iQSIID)) { }
                sParentGUID = oCache.Get_ParamVal(sSessionID, sGUID, "", "{XIP|sMainQSGUID}");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(new { iQSIID = iQSIID, sParentGUID = sParentGUID }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetTemplateIDByClass(string sClassID, string sCode, string sGUID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            //string sGUID = Guid.NewGuid().ToString();
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            try
            {
                int iTemplateID = 0;
                if (!string.IsNullOrEmpty(sClassID) && !string.IsNullOrEmpty(sCode))
                {
                    Dictionary<string, object> oWhereParams = new Dictionary<string, object>();
                    oWhereParams["FKiClassID"] = sClassID;
                    oWhereParams["sCode"] = sCode;
                    var oContent = Connection.Select<XIContentEditors>("XITemplate_T", oWhereParams).FirstOrDefault();
                    if (oContent != null)
                    {
                        iTemplateID = oContent.ID;
                    }
                }
                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iTemplateID}", iTemplateID.ToString(), null, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(sGUID, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult RemoveStepXIIValues(string sStepName, string sGUID, int iQSIID)
        {
            try
            {
                XIIXI oIXI = new XIIXI();
                XIInfraCache oCache = new XIInfraCache();
                XIIQS oQSInstance = new XIIQS();
                oQSInstance = (XIIQS)oCache.Get_QuestionSetCache("QuestionSetCache", sGUID, iQSIID);
                if (oQSInstance.QSDefinition == null)
                {
                    oQSInstance = oIXI.GetQuestionSetInstanceByID(0, iQSIID, null, 0, 0, null);
                }
                if (!string.IsNullOrEmpty(sStepName))
                {
                    var oStepD = oQSInstance.QSDefinition.Steps.Values.Where(m => m.sName.ToLower() == sStepName.ToLower()).FirstOrDefault();
                    oQSInstance = oQSInstance.LoadStepInstance(oQSInstance, oStepD.ID, sGUID);
                    if (oQSInstance.Steps.ContainsKey(oStepD.sName))
                    {
                        foreach (var Section in oQSInstance.Steps[oStepD.sName].Sections)
                        {
                            foreach (var Field in Section.Value.XIValues)
                            {
                                var oSecD = oStepD.Sections.Where(m => m.Value.ID == Section.Value.FKiStepSectionDefinitionID).FirstOrDefault();
                                oSecD.Value.sIsHidden = "on";
                                oQSInstance.XIValues[Field.Key].sValue = "";
                                oQSInstance.XIValues[Field.Key].sDerivedValue = "";
                                Field.Value.sValue = "";
                                Field.Value.sDerivedValue = "";
                            }
                        }
                        //oQSInstance.iCurrentStepID = oStepD.ID;
                        SaveQSInstances(oQSInstance, sGUID);
                    }
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Exception: " + ex.ToString(), "XIDNA");
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        //Grid Search Code
        [AllowAnonymous]
        public ActionResult GetGridClickResult(int i1ClickID, string sGUID, string Fields, string Optrs, string Values, string SearchType, string VisualName, string sSearType, string SearchText, string sLockGroup, int iPageCount = 0)
        {
            var sSessionID = HttpContext.Session.SessionID;
            int iUserID = SessionManager.UserID;
            XID1Click oXD = new XID1Click();
            oXD.Fields = Fields; oXD.Optrs = Optrs; oXD.Values = Values; oXD.SearchType = sSearType; oXD.SearchText = SearchText;
            XIInfraOneClickComponent oOCC = new XIInfraOneClickComponent();
            XIInfraGridComponent oOGD = new XIInfraGridComponent();
            XIIComponent oCompI = new XIIComponent();
            XIDComponent oCompD = new XIDComponent();
            var oParams = oCompD.Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue }).ToList();
            oParams = (List<CNV>)oCache.ResolveParameters(oParams, sSessionID, sGUID);
            oParams.Add(new CNV { sName = "sGUID", sValue = sGUID });
            oParams.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
            oParams.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
            oParams.Add(new CNV { sName = "sSearchType", sValue = sSearType });
            if (!string.IsNullOrEmpty(SearchText))
            {
                oParams.Add(new CNV { sName = "sSearchText", sValue = SearchText.ToString() });
            }
            oParams.Add(new CNV { sName = "1ClickID", sValue = i1ClickID.ToString() });
            oParams.Add(new CNV { sName = "Fields", sValue = Fields });
            oParams.Add(new CNV { sName = "Optrs", sValue = Optrs });
            oParams.Add(new CNV { sName = "Values", sValue = Values });
            oParams.Add(new CNV { sName = "LockGroup", sValue = sLockGroup });
            oParams.Add(new CNV { sName = "iPageCount", sValue = iPageCount.ToString() });
            List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
            if (!string.IsNullOrEmpty(VisualName))
            {
                var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, VisualName, null);
                if (oXIvisual != null)
                {
                    oXIVisualisations.Add(oXIvisual);
                }
            }
            if (SearchType == "Grid")
            {
                var oCR = oOGD.XILoad(oParams);
                if (oCR.bOK && oCR.oResult != null)
                {
                    var oXI1Click = (XID1Click)oCR.oResult;
                    //oXI1Click.iPaginationCount = iPageCount;
                    oCompI.oContent[XIConstant.GridComponent] = oXI1Click;
                    oCompI.oVisualisation = oXIVisualisations;
                }
                return PartialView("~\\Views\\XIComponents\\_GridComponent.cshtml", oCompI);
            }
            else
            {
                var oCR = oOCC.XILoad(oParams);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oCompI.oContent[XIConstant.OneClickComponent] = oCR.oResult;
                }
                return PartialView("~\\Views\\XIComponents\\_OneClickComponent.cshtml", oCompI);
            }
        }
        [HttpPost]
        public ActionResult GetNextHTMLRows(string sParams, int iPageCount, string sGUID, string sVisualisation)
        {
            try
            {
                XIIComponent oCompI = new XIIComponent();
                XIInfraHtmlComponent oHTML = new XIInfraHtmlComponent();
                List<CNV> oParams = new List<CNV>();
                var Params = sParams.Split(':').ToList();
                foreach (var Param in Params)
                {
                    var Par = Param.Split('_').ToList();
                    if (Par.Count() == 2)
                    {
                        CNV oNV = new CNV();
                        oNV.sName = Par[0];
                        oNV.sValue = Par[1];
                        oParams.Add(oNV);
                    }
                }
                var oCompD = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, "14");
                var oCompC = (XIDComponent)oCompI.Clone(oCompD);
                var oCompParams = oParams.Select(m => new XIDComponentParam { sName = m.sName, sValue = m.sValue, FKiComponentID = oCompC.ID }).ToList();
                oCompC.Params = oCompParams;
                var sSessionID = HttpContext.Session.SessionID;
                int iUserID = SessionManager.UserID;
                oParams.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
                oParams.Add(new CNV { sName = "sGUID", sValue = sGUID });
                oParams.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                oParams.Add(new CNV { sName = "iPageCount", sValue = iPageCount.ToString() });
                var oCR = oHTML.XILoad(oParams);
                if (oCR.bOK && oCR.oResult != null)
                {
                    XID1Click o1ClickD = (XID1Click)oCR.oResult;
                    oCompI.oContent[XIConstant.HTMLComponent] = o1ClickD;

                    List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                    if (!string.IsNullOrEmpty(sVisualisation))
                    {
                        var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, null);
                        if (oXIvisual != null)
                        {
                            oXIVisualisations.Add(oXIvisual);
                        }
                    }
                    oCompI.oDefintion = oCompC;
                    oCompI.oVisualisation = oXIVisualisations;
                    return PartialView("~\\Views\\XIComponents\\_HTMLRepeaterComponent.cshtml", oCompI);
                }

            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Exception: GetNextHTMLRows()" + ex.ToString(), "XIDNA");
            }
            return null;
        }
        [HttpPost]
        public ActionResult UpdateQuoteRank(int iQSIID)
        {
            try
            {
                Policy oPol = new Policy();
                List<CNV> nParams = new List<CNV>();
                CNV oNV = new CNV();
                oNV.sName = "iQSInstanceID";
                oNV.sValue = iQSIID.ToString();
                nParams.Add(oNV);
                oPol.UpdateQuoteRank(nParams);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Exception: " + ex.ToString(), "XIDNA");
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridData(int i1ClickID = 0, string sGUID = "")
        {
            try
            {
                var sSessionID = HttpContext.Session.SessionID;
                XIInfraGridComponent oGrid = new XIInfraGridComponent();
                List<CNV> oParams = new List<CNV>();
                oParams.Add(new CNV { sName = "1ClickID", sValue = i1ClickID.ToString() });
                oParams.Add(new CNV { sName = "sGUID", sValue = sGUID });
                oParams.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
                var oResult = oGrid.XILoad(oParams);
                XIDComponent oCompD = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, "13");
                XIIComponent oCompI = new XIIComponent();
                oCompI.oContent[XIConstant.GridComponent] = oResult.oResult;
                oCompI.oDefintion = oCompD;
                return PartialView("~\\Views\\XIComponents\\_GridComponent.cshtml", oCompI);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Exception: " + ex.ToString(), "XIDNA");
                return null;
            }
        }
        public void XMLGeneration()
        {
            string sQuery = "select Name, sOptionCode, sOptionName from XIBOOptionList_T_N where Name in ('ititle','iAccidentType') and sOptionCode is not null order by sOptionCode";
            XID1Click oXI1Click = new XID1Click();
            oXI1Click.Query = sQuery;
            //oXI1Click.DataSource = 10;
            var Result = oXI1Click.GetList();
            List<XIIBO> oDocumentsList = new List<XIIBO>();
            if (Result.bOK || Result.oResult != null)
            {
                oDocumentsList = ((Dictionary<string, XIIBO>)Result.oResult).Values.ToList();
            }
            BuildXML(oDocumentsList);
        }
        public static void BuildXML(List<XIIBO> oData)
        {
            XmlSchema schema = new XmlSchema();

            var Name = "";
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();

            foreach (var item in oData) //.Where(x => x.AttributeI("sOptionCode").sValue != "").ToList()
            {
                if (Name != item.AttributeI("Name").sValue)
                {
                    // <xs:simpleType name="titleType">
                    XmlSchemaSimpleType SimpleType = new XmlSchemaSimpleType();
                    SimpleType.Name = item.AttributeI("Name").sValue;
                    schema.Items.Add(SimpleType);

                    // <xs:annotation>
                    XmlSchemaAnnotation Annotation = new XmlSchemaAnnotation();
                    SimpleType.Annotation = Annotation;

                    // <xs:documentation>States in the Pacific Northwest of US</xs:documentation>
                    XmlSchemaDocumentation Documentation = new XmlSchemaDocumentation();
                    Annotation.Items.Add(Documentation);
                    Documentation.Markup = TextToNodeArray(item.AttributeI("sOptionName").sValue);

                    // <xs:restriction base="xs:string">
                    restriction = new XmlSchemaSimpleTypeRestriction();
                    SimpleType.Content = restriction;
                    restriction.BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
                }
                // <xs:enumeration value='ID'>
                XmlSchemaEnumerationFacet enumerationID = new XmlSchemaEnumerationFacet();
                restriction.Facets.Add(enumerationID);
                enumerationID.Value = item.AttributeI("sOptionCode").sValue;

                // <xs:annotation>
                XmlSchemaAnnotation annID = new XmlSchemaAnnotation();
                enumerationID.Annotation = annID;

                // <xs:documentation>Idaho</xs:documentation>
                XmlSchemaDocumentation docID = new XmlSchemaDocumentation();
                annID.Items.Add(docID);
                docID.Markup = TextToNodeArray(item.AttributeI("sOptionName").sValue);
                Name = item.AttributeI("Name").sValue;
            }

            //schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
            schemaSet.Add(schema);
            schemaSet.Compile();

            XmlWriter writer;
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;
            foreach (XmlSchema schema1 in schemaSet.Schemas())
            {
                writer = XmlWriter.Create(@"D:\Mounika\XSD\PFRequest.xml");
                schema1.Write(writer);
                writer.Close();
            }
        }
        //public static void ValidationCallbackOne(object sender, ValidationEventArgs args)
        //{
        //    Console.WriteLine(args.Message);
        //}

        public static XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[1] { doc.CreateTextNode(text) };
        }
        [AllowAnonymous]
        public ActionResult GetStructureResult(string sGUID)
        {
            List<object> obj = new List<object>();
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                var sSessionID = HttpContext.Session.SessionID;
                var sPolicyID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iACPolicyID}");

                Dictionary<string, object> oAddonD = new Dictionary<string, object>();
                List<XIIBO> oAttrI = new List<XIIBO>();
                XIIXI oXII = new XIIXI();
                var oBO = oXII.BOI("ACPolicy_T", sPolicyID).Structure("DataLayer").XILoad(null, true);
                if (oBO != null)
                {
                    var oPolicyI = oBO.oSubStructureI("ACPolicy_T");
                    var oPolicyIResult = (XIIBO)oPolicyI.oBOIList.FirstOrDefault();
                    if (oPolicyIResult != null)
                    {
                        XIIBO oAddonI = new XIIBO();
                        string PolicyID = string.Empty;
                        string TotalPremium = string.Empty;
                        string sClass = string.Empty;
                        string Tax = string.Empty;
                        if (oPolicyIResult.Attributes.ContainsKey("id"))
                        {
                            PolicyID = oPolicyIResult.Attributes["id"].sValue;
                        }
                        if (oPolicyIResult.Attributes.ContainsKey("rTotalPremium"))
                        {
                            TotalPremium = oPolicyIResult.Attributes["rTotalPremium"].sValue;
                        }
                        if (oPolicyIResult.Attributes.ContainsKey("FKsClass"))
                        {
                            sClass = oPolicyIResult.Attributes["FKsClass"].sValue;
                        }
                        if (oPolicyIResult.Attributes.ContainsKey("rTax"))
                        {
                            Tax = oPolicyIResult.Attributes["rTax"].sValue;
                        }
                        oAddonI.SetAttribute("id", PolicyID);
                        oAddonI.SetAttribute("rTotalPremium", TotalPremium);
                        oAddonI.SetAttribute("sClass", sClass);
                        oAddonI.SetAttribute("rTax", Tax);
                        oAddonD.Add("Policy", oAddonI);
                    }
                    XIIBO oProductI = new XIIBO();
                    string ProductID = string.Empty;
                    string sProductName = string.Empty;
                    string QuotePremium = string.Empty;
                    if (oPolicyIResult.Attributes.ContainsKey("FKiProductID"))
                    {
                        ProductID = oPolicyIResult.Attributes["FKiProductID"].sValue;

                        int iProductID = 0;
                        if (int.TryParse(ProductID, out iProductID))
                        {
                            var oProductInstance = oXII.BOI("Product", iProductID.ToString());
                            if (oProductInstance != null && oProductInstance.Attributes.ContainsKey("sName"))
                            {
                                sProductName = oProductInstance.Attributes["sName"].sValue;
                            }
                        }
                        //sProductName = oPolicyIResult.Attributes["FKiProductID"].ResolveFK("display");
                    }
                    if (oPolicyIResult.Attributes.ContainsKey("rQuotePremium"))
                    {
                        QuotePremium = oPolicyIResult.Attributes["rQuotePremium"].sValue;
                    }
                    oProductI.SetAttribute("id", ProductID);
                    oProductI.SetAttribute("sName", sProductName);
                    oProductI.SetAttribute("rCost", QuotePremium);
                    oAttrI.Add(oProductI);
                    var oListI = oBO.oSubStructureI("ACPurchase_T");
                    var oPurchaseIResult = (List<XIIBO>)oListI.oBOIList;
                    if (oPurchaseIResult != null && oPurchaseIResult.Count > 0)
                    {
                        foreach (var oPurchaseI in oPurchaseIResult)
                        {
                            string sAddon = string.Empty;
                            string AddonCost = string.Empty;
                            string AddonID = string.Empty;
                            if (oPurchaseI.Attributes.ContainsKey("sName"))
                            {
                                sAddon = oPurchaseI.Attributes["sName"].sValue;
                            }
                            if (oPurchaseI.Attributes.ContainsKey("rCost"))
                            {
                                AddonCost = oPurchaseI.Attributes["rCost"].sValue;
                            }
                            if (oPurchaseI.Attributes.ContainsKey("id"))
                            {
                                AddonID = oPurchaseI.Attributes["id"].sValue;
                            }
                            XIIBO oAddonI = new XIIBO();
                            oAddonI.SetAttribute("sName", sAddon);
                            oAddonI.SetAttribute("rCost", AddonCost);
                            oAddonI.SetAttribute("id", AddonID);
                            oAttrI.Add(oAddonI);
                        }
                    }
                    oAddonD.Add("Products", oAttrI);
                    obj.Add(oAddonD);
                }
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog("Exception: " + ex.ToString(), "XIDNA");
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveMultiRow(int OneClickID, string BOName, string sGUID, string[] NVPairs, string flag)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sSessionID = HttpContext.Session.SessionID;
                var oBOIDDef = new XIIAttribute();
                XIDBO oBODs = new XIDBO();
                XIDBO oBOD1s = new XIDBO();
                var skey = oCache.CacheKeyBuilder(XIConstant.Cache1Click, "", "") + "_" + OneClickID;
                XIApplicationsController cache = new XIApplicationsController();
                cache.RemoveCacheByKey(skey, EnumCacheTypes.Application);
                XID1Click oBO1Click = new XID1Click();
                oBO1Click = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, OneClickID.ToString());
                var Query1 = string.Empty;
                var MainQuery = string.Empty;
                XIIBO oBOIs = new XIIBO();
                XIIBO oBOI = new XIIBO();
                var wherefields = "";
                var wheref = string.Empty;
                oBOD1s = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, oBO1Click.sBOName);
                var fun = "";
                var sfunction = "";
                if (NVPairs != null && NVPairs.Count() > 0)
                {
                    if (flag == "1")
                    {
                        foreach (var item in NVPairs)
                        {
                            VMXIComponentsNVs NVs = new VMXIComponentsNVs();
                            var Pairs = item.Split('^').ToList();
                            NVs.Name = Pairs[0];
                            NVs.sType = Pairs[1];
                            NVs.Value = Pairs[2];
                            if (NVs.Name != "")
                            {
                                var oAttributeDef = oBOD1s.Attributes.Where(s => s.Value.ID == Convert.ToInt32(NVs.Name)).Select(s => s).FirstOrDefault();

                                fun = "{if|{eq|" + "'" + oAttributeDef.Key + "','" + oAttributeDef.Value.OptionList.Where(s => s.sValues == NVs.sType).Select(s => s.sOptionName).FirstOrDefault() + "'},'" + NVs.Value + "'";
                                sfunction += fun + ',';
                                wheref = NVs.Name + '^' + NVs.sType + '^' + NVs.Value + ',';
                                wherefields += wheref;
                            }
                        }
                        if (wherefields != "")
                        {
                            wherefields = wherefields.Substring(0, wherefields.Length - 1);
                            sfunction = "xi.s|" + sfunction.Substring(0, sfunction.Length - 1) + ",''";
                            for (int i = 0; i < NVPairs.Count(); i++)
                            {
                                sfunction += '}';
                            }
                            oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI1ClickScripts");
                            oBOIs.BOD = oBODs;
                            if (oBO1Click.FKi1ClickScriptID != null && oBO1Click.FKi1ClickScriptID != "")
                            {
                                oBOIs.Attributes["Id".ToLower()] = new XIIAttribute { sName = "Id", sValue = oBO1Click.FKi1ClickScriptID.ToString(), bDirty = true };
                            }
                            oBOIs.Attributes["sName".ToLower()] = new XIIAttribute { sName = "sName", sValue = "RowColour", bDirty = true };
                            oBOIs.Attributes["sFunction".ToLower()] = new XIIAttribute { sName = "sFunction", sValue = sfunction, bDirty = true };
                            oBOIs.Attributes["FKiOneClickID".ToLower()] = new XIIAttribute { sName = "FKiOneClickID", sValue = OneClickID.ToString(), bDirty = true };
                            oBOIs.Attributes["CreatedBy".ToLower()] = new XIIAttribute { sName = "CreatedBy", sValue = "1", bDirty = true };
                            oBOIs.Attributes["CreatedBySYSID".ToLower()] = new XIIAttribute { sName = "CreatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                            oBOIs.Attributes["CreatedTime".ToLower()] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                            oBOIs.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = "1", bDirty = true };
                            oBOIs.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                            oBOIs.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                            var XiBO = oBOIs.Save(oBOIs);
                            if (XiBO.bOK && XiBO.oResult != null)
                            {
                                oBOIs = (XIIBO)XiBO.oResult;
                                var xi1ClickScriptID = oBOIs.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|1ClickScriptID}", xi1ClickScriptID, "", null);
                                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BOName);
                                oBOI.BOD = oBODs;
                                oBOI.Attributes["FKi1ClickScriptID".ToLower()] = new XIIAttribute { sName = "FKi1ClickScriptID", sValue = xi1ClickScriptID.ToString(), bDirty = true };
                                oBOI.Attributes["sRowColour".ToLower()] = new XIIAttribute { sName = "sRowColour", sValue = wherefields, bDirty = true };
                                oBOI.Attributes["id".ToLower()] = new XIIAttribute { sName = "id", sValue = OneClickID.ToString(), bDirty = true };
                                var XiBOs = oBOI.Save(oBOI);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < NVPairs.Count(); i++)
                        {
                            VMXIComponentsNVs NVs = new VMXIComponentsNVs();
                            var Pairs = NVPairs[i].ToString().Split('^').ToList();
                            NVs.Name = Pairs[0];
                            NVs.sType = Pairs[1];
                            NVs.Value = Pairs[2];
                            var oAttributeDef = "";
                            if (NVs.Name != "")
                            {
                                oAttributeDef = oBOD1s.Attributes.Where(s => s.Value.ID == Convert.ToInt32(NVs.Name)).Select(s => s.Key).FirstOrDefault();
                            }
                            XIDAttribute oXID = new XIDAttribute();
                            string sFormat = "";
                            if (string.IsNullOrEmpty(sFormat))
                            {
                                sFormat = "yyyy-MM-dd";
                            }
                            string[] sDateRange = new string[] { "t", "t+", "t-", "m", "m+", "m-", "tm", "tm+", "tm-", "y", "y+", "y-", "ty", "ty+", "ty-", "ms", "ws", "ys", "ms+", "ws+", "ys+", "ms-", "ws-", "ys-" };
                            var Dates = NVs.Value.Split('#').ToList();
                            if (Dates[0].Contains("+") || Dates[0].Contains("-"))
                            {
                                Dates[0] = Dates[0].Substring(0, Dates[0].Length - 1);
                            }
                            if (sDateRange.Contains(Dates[0].ToLower()))
                            {
                                if (NVs.sType.Contains("between"))
                                {

                                    var Date1 = Utility.GetDateResolvedValue(Dates[0], sFormat); //"dd-MMM-yyyy"
                                    var Date2 = Utility.GetDateResolvedValue(Dates[1], sFormat);
                                    Query1 = oAttributeDef + " between '" + Date1 + "' and '" + Date2 + "'";
                                }
                                else
                                {
                                    var Bdate = NVs.sType.Split('#');
                                    var Date = Utility.GetDateResolvedValue(NVs.Value, sFormat);
                                    Query1 = oAttributeDef + " " + Bdate[1] + "'" + Date + "'";
                                }
                                wheref = NVs.Name + '^' + NVs.sType + '^' + NVs.Value + ',';
                                MainQuery += Query1 + " and ";
                                wherefields += wheref;
                            }
                            else if (!string.IsNullOrEmpty(NVs.Value))
                            {
                                if (NVs.Value.ToLower() == "null")
                                {
                                    if (NVs.sType == "=")
                                    {
                                        Query1 = oAttributeDef + " IS NULL";
                                    }
                                    else
                                    {
                                        Query1 = oAttributeDef + " IS NOT NULL";
                                    }
                                    wheref = NVs.Name + '^' + NVs.sType + '^' + "NULL" + ',';
                                    MainQuery += Query1 + " and ";
                                    wherefields += wheref;
                                }
                                else if (NVs.Value != "")
                                {
                                    if (NVs.sType == "starts with")
                                    {
                                        Query1 = oAttributeDef + " Like '" + NVs.Value + "%'";
                                    }
                                    else if (NVs.sType == "not starts with")
                                    {
                                        Query1 = oAttributeDef + " not Like '" + NVs.Value + "%'";
                                    }
                                    else if (NVs.sType == "ends with")
                                    {
                                        Query1 = oAttributeDef + " Like '%" + NVs.Value + "'";
                                    }
                                    else if (NVs.sType == "not ends with")
                                    {
                                        Query1 = oAttributeDef + " not Like '%" + NVs.Value + "'";
                                    }
                                    else if (NVs.sType == "contains")
                                    {
                                        Query1 = oAttributeDef + " Like '%" + NVs.Value + "%'";
                                    }
                                    else if (NVs.sType.Contains("between"))
                                    {
                                        var Bdate = NVs.Value.Split('#');
                                        Query1 = oAttributeDef + " between '" + Bdate[0] + "' and '" + Bdate[1] + "'";
                                    }
                                    else if (NVs.sType.Contains("#"))
                                    {
                                        var Bdate = NVs.Value.Split('#');
                                        Query1 = oAttributeDef + NVs.sType.Split('#')[1] + "'" + NVs.Value + "'";
                                    }
                                    else
                                    {
                                        Query1 = oAttributeDef + NVs.sType + "'" + NVs.Value + "'";
                                    }
                                    wheref = NVs.Name + '^' + NVs.sType + '^' + NVs.Value + ',';
                                    MainQuery += Query1 + " and ";
                                    wherefields += wheref;
                                }
                            }
                            // }
                        }
                        if (MainQuery != "")
                        {
                            var selectQuery = "";
                            MainQuery = MainQuery.Substring(0, MainQuery.Length - 5);
                            wherefields = wherefields.Substring(0, wherefields.Length - 1);

                            oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BOName);
                            oBOIs.BOD = oBODs;
                            if (oBO1Click.Query.ToLower().Contains("where".ToLower()) || oBO1Click.Query.Contains("order".ToLower()))
                            {
                                var w = oBO1Click.Query.IndexOf("where".ToLower());
                                oBO1Click.Query = oBO1Click.Query.Substring(0, w);
                                var o = oBO1Click.Query.ToLower().IndexOf("order".ToLower());
                                if (o != -1)
                                {
                                    selectQuery = oBO1Click.Query.Insert(o, " where " + MainQuery);
                                }
                                else
                                {
                                    selectQuery = oBO1Click.Query + " where " + MainQuery;
                                }
                                //oBO1Click.Query = oBO1Click.Query;
                            }
                            else
                            {
                                selectQuery = oBO1Click.Query + " where " + MainQuery;
                            }
                            oBOIs.Attributes["Query"] = new XIIAttribute { sName = "Query", sValue = selectQuery, bDirty = true };
                            oBOIs.Attributes["VisibleQuery".ToLower()] = new XIIAttribute { sName = "VisibleQuery", sValue = selectQuery, bDirty = true };
                        }
                        oBOIs.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = OneClickID.ToString(), bDirty = true };
                        oBOIs.Attributes["WhereFields".ToLower()] = new XIIAttribute { sName = "WhereFields", sValue = MainQuery, bDirty = true };
                        oBOIs.Attributes["FKiWhereValue".ToLower()] = new XIIAttribute { sName = "FKiWhereValue", sValue = wherefields, bDirty = true };
                        var XiBO = oBOIs.Save(oBOIs);
                    }
                }
                var skey1 = oCache.CacheKeyBuilder(XIConstant.Cache1Click, "", "") + "_" + OneClickID;
                cache.RemoveCacheByKey(skey1, EnumCacheTypes.Application);
                var sRes = Common.ResponseMessage();
                return Json(sRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse
                {
                    Status = false,
                    ResponseMessage = ServiceConstants.ErrorMessage
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //SimpleOneClickSaving
        public CResult SaveSimple1Click(List<XIIAttribute> Attributes, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            CResult XiBO = new CResult();
            try
            {
                XIApplicationsController cache = new XIApplicationsController();
                string sSessionID = HttpContext.Session.SessionID;
                XIIBO oBOI = new XIIBO();
                XIDBO oBODs = new XIDBO();
                XID1Click oBO1Click = new XID1Click();
                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI1Click");
                XIDBO oBODef1 = new XIDBO();
                var oBOIDDef = new XIIAttribute();
                Dictionary<string, XIIAttribute> oAttrs = new Dictionary<string, XIIAttribute>();
                oAttrs = Attributes.ToDictionary(x => x.sName.ToLower(), x => x);
                oBOI.Attributes = oAttrs;

                oBOIDDef = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "boid").FirstOrDefault();
                string sOrderFields = string.Empty;
                var sListQuery = "";
                string ClickID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|1ClickID}");
                if (ClickID != null)
                {
                    oBO1Click = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, ClickID.ToString());
                }
                if (oBOIDDef == null)
                {
                    XIDAttribute oXID = new XIDAttribute();
                    ClickID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|1ClickID}");
                    var oAttrID = Attributes.Where(m => m.sName.ToLower() == "fkiattributelist").Select(s => s.sValue).FirstOrDefault();
                    var OrderF = Attributes.Where(m => m.sName.ToLower() == "fkiorderlist").Select(s => s.sValue).FirstOrDefault();
                    var Operator = Attributes.Where(m => m.sName.ToLower() == "fkioperator").Select(s => s.sValue).FirstOrDefault();
                    var whereValue = Attributes.Where(m => m.sName.ToLower() == "fkiwherevalue").Select(s => s.sValue).FirstOrDefault();
                    if (ClickID != null)
                    {
                        var skey = oCache.CacheKeyBuilder(XIConstant.Cache1Click, "", "") + "_" + ClickID;
                        cache.RemoveCacheByKey(skey, EnumCacheTypes.Application);
                    }

                    var AttrF = "";
                    if (oAttrID != null)
                    {
                        XIDBO oBOD1s = new XIDBO();
                        oBOD1s = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, oBO1Click.sBOName);
                        AttrF = oBOD1s.Attributes.Where(s => s.Value.ID == Convert.ToInt32(oAttrID)).Select(s => s.Key).FirstOrDefault();
                    }
                    sOrderFields = AttrF + ' ' + OrderF;
                    oBOI.BOD = oBODs;
                    oBOI.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = ClickID, bDirty = true };
                    if (oAttrID != null && OrderF != null)
                    {
                        if (oBO1Click.Query.ToLower().Contains("order".ToLower()))
                        {
                            var o = oBO1Click.Query.ToLower().IndexOf("order".ToLower());
                            if (o != -1)
                            {
                                oBO1Click.Query = oBO1Click.Query.Substring(0, o);
                                sListQuery = oBO1Click.Query + " ORDER BY " + sOrderFields;
                            }
                        }
                        else
                        {
                            sListQuery = oBO1Click.Query + " ORDER BY " + sOrderFields;
                        }
                        oBOI.Attributes["Query".ToLower()] = new XIIAttribute { sName = "Query", sValue = sListQuery, bDirty = true };
                        oBOI.Attributes["OrderFields".ToLower()] = new XIIAttribute { sName = "OrderFields", sValue = sOrderFields, bDirty = true };
                        oBOI.Attributes["FKiOrderList".ToLower()] = new XIIAttribute { sName = "FKiOrderList", sValue = OrderF, bDirty = true };
                        oBOI.Attributes["FKiAttributeList".ToLower()] = new XIIAttribute { sName = "FKiAttributeList", sValue = oAttrID, bDirty = true };
                    }
                    XiBO = oBOI.Save(oBOI);
                }
                else
                {
                    XIDGroup oXID = new XIDGroup();
                    var oAttDef = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "name").Select(s => s.sValue).FirstOrDefault();
                    var oAttExport = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "bisexport").Select(s => s.bValue).FirstOrDefault();
                    var oAttRowClick = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "isrowclick").Select(s => s.bValue).FirstOrDefault();
                    var oAttType = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "typeid").Select(s => s.sValue).FirstOrDefault();
                    var iPaginationCount = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ipaginationcount").Select(s => s.sValue).FirstOrDefault();
                    var sTotal = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "stotalcolumns").Select(s => s.sValue).FirstOrDefault();
                    var OnRowClickType = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "OnRowClickType".ToLower()).Select(s => s.sValue).FirstOrDefault();
                    var ID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").FirstOrDefault();
                    var oSearchGroupDef = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "fkisearchgroup").FirstOrDefault();
                    var oListGroupDef = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "fkilistgroup").FirstOrDefault();
                    var searchFilter = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "fkifiltersearch").FirstOrDefault();
                    var FKiStructureID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "FKiStructureID".ToLower()).FirstOrDefault();
                    if (oBOIDDef.sName == "BOID")
                    {
                        var iBOID = oBOIDDef.sValue;
                        var ListGroupID = oListGroupDef.sValue;
                        var SearchGroupID = "";
                        var oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBOID.ToString());
                        var sTableName = oBODef.TableName;
                        string sSelectListFields = string.Empty;
                        string sSelectSearchFields = string.Empty;
                        var iDataSourceID = oBODef.iDataSource;
                        // var squery=oBODef.
                        if (sTotal != null)
                        {
                            var Tot = sTotal.Split(',');
                            sTotal = "";
                            if (Tot != null)
                            {
                                var sTot = "";
                                foreach (var item in Tot)
                                {
                                    if (item.Contains("]."))
                                    {
                                        sTotal = item;
                                    }
                                    else
                                    {
                                        sTot = "[" + sTableName + "]." + item;
                                    }
                                    sTotal += sTot + ',';
                                }
                                sTotal = sTotal.Substring(0, sTotal.Length - 1);

                            }
                        }
                        if (oSearchGroupDef.sValue.Contains(',') || oBODef.Attributes.ContainsKey(oSearchGroupDef.sValue))
                        {
                            sSelectSearchFields = oSearchGroupDef.sValue.Replace(",", ", ");
                        }
                        else
                        {
                            oXID.ID = Convert.ToInt32(oSearchGroupDef.sValue);
                            SearchGroupID = oSearchGroupDef.sValue;
                            var oGroupDef = oBODef.Groups.Values.Where(m => m.ID == oXID.ID).FirstOrDefault();
                            if (oGroupDef != null)
                            {
                                //var oAttrD = (XIDGroup)oGroupDef.oResult;
                                var GroupF = oGroupDef.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                foreach (var item in GroupF)
                                {
                                    sSelectSearchFields = sSelectSearchFields + item + ", ";
                                }
                                sSelectSearchFields = sSelectSearchFields.Substring(0, sSelectSearchFields.Length - 2);
                            }
                        }
                        if (oListGroupDef.sValue.Contains(',') || oBODef.Attributes.ContainsKey(oListGroupDef.sValue))
                        {
                            foreach (var item in oListGroupDef.sValue.Split(','))
                            {
                                sSelectListFields = sSelectListFields + item + ", ";
                            }
                            sSelectListFields = sSelectListFields.Substring(0, sSelectListFields.Length - 2);
                            if (oBO1Click.Query != null && oBO1Click.Query.ToLower().Contains("where"))
                            {
                                var w = oBO1Click.Query.IndexOf("where".ToLower());
                                oBO1Click.Query = oBO1Click.Query.Substring(w, oBO1Click.Query.Length - w);
                                if (w != -1)
                                {
                                    sListQuery = "SELECT " + sSelectListFields + " FROM " + sTableName + " " + oBO1Click.Query;
                                }
                            }
                            else
                            {
                                sListQuery = "SELECT " + sSelectListFields + " FROM " + sTableName;
                            }
                        }
                        else
                        {
                            oXID.ID = Convert.ToInt32(oListGroupDef.sValue);
                            ListGroupID = oListGroupDef.sValue;
                            var oGroupDef = oXID.Get_XIBOGroupDetails();
                            if (oGroupDef.bOK && oGroupDef.oResult != null)
                            {
                                var oAttrD = (XIDGroup)oGroupDef.oResult;
                                var GroupF = oAttrD.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                foreach (var item in GroupF)
                                {
                                    sSelectListFields = sSelectListFields + "[" + sTableName + "]" + "." + item + ", ";
                                }
                                sSelectListFields = sSelectListFields.Substring(0, sSelectListFields.Length - 2);
                            }
                            if (oBO1Click.Query != null && oBO1Click.Query.ToLower().Contains("where"))
                            {
                                var w = oBO1Click.Query.IndexOf("where".ToLower());
                                oBO1Click.Query = oBO1Click.Query.Substring(w, oBO1Click.Query.Length - w);
                                if (w != -1)
                                {
                                    sListQuery = "SELECT " + sSelectListFields + " FROM " + sTableName + " " + oBO1Click.Query;
                                }
                            }
                            else
                            {
                                sListQuery = "SELECT " + sSelectListFields + " FROM " + sTableName;
                            }
                        }
                        oBOI.BOD = oBODs;

                        if (ID != null && ID.sValue != null)
                        {
                            oBOI.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = ID.sValue.ToString(), bDirty = true };
                        }
                        else
                        {
                            oBOI.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = "0", bDirty = true };
                        }
                        oBOI.Attributes["OrganizationID".ToLower()] = new XIIAttribute { sName = "OrganizationID", sValue = SessionManager.OrganizationID.ToString(), bDirty = true };
                        oBOI.Attributes["CategoryID".ToLower()] = new XIIAttribute { sName = "CategoryID", sValue = "10", bDirty = true };
                        oBOI.Attributes["BOID".ToLower()] = new XIIAttribute { sName = "BOID", sValue = oBOIDDef.sValue.ToString(), bDirty = true };
                        oBOI.Attributes["ParentID".ToLower()] = new XIIAttribute { sName = "ParentID", sValue = "0", bDirty = true };
                        oBOI.Attributes["Name".ToLower()] = new XIIAttribute { sName = "Name", sValue = oAttDef, bDirty = true };
                        oBOI.Attributes["Title".ToLower()] = new XIIAttribute { sName = "Title", sValue = oAttDef, bDirty = true };
                        oBOI.Attributes["Code".ToLower()] = new XIIAttribute { sName = "Code", sValue = "Default 1-Click", bDirty = true };
                        oBOI.Attributes["TypeID".ToLower()] = new XIIAttribute { sName = "TypeID", sValue = oAttType, bDirty = true };
                        oBOI.Attributes["Query".ToLower()] = new XIIAttribute { sName = "Query", sValue = sListQuery, bDirty = true };
                        oBOI.Attributes["VisibleQuery".ToLower()] = new XIIAttribute { sName = "VisibleQuery", sValue = sListQuery, bDirty = true };
                        oBOI.Attributes["DisplayAs".ToLower()] = new XIIAttribute { sName = "DisplayAs", sValue = "50", bDirty = true };
                        oBOI.Attributes["ResultListDisplayType".ToLower()] = new XIIAttribute { sName = "ResultListDisplayType", sValue = "1", bDirty = true };
                        oBOI.Attributes["Class".ToLower()] = new XIIAttribute { sName = "Class", sValue = "43", bDirty = true };
                        oBOI.Attributes["IsDynamic".ToLower()] = new XIIAttribute { sName = "IsDynamic", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsStoredProcedure".ToLower()] = new XIIAttribute { sName = "IsStoredProcedure", sValue = "0", bDirty = true };
                        oBOI.Attributes["SelectFields".ToLower()] = new XIIAttribute { sName = "SelectFields", sValue = sSelectListFields, bDirty = true };
                        oBOI.Attributes["FromBos".ToLower()] = new XIIAttribute { sName = "FromBos", sValue = sTableName, bDirty = true };
                        oBOI.Attributes["bIsExport".ToLower()] = new XIIAttribute { sName = "bIsExport", sValue = oAttExport.ToString(), bDirty = true };

                        if (SearchGroupID != "0")
                        {
                            oBOI.Attributes["SearchFields".ToLower()] = new XIIAttribute { sName = "SearchFields", sValue = sSelectSearchFields, bDirty = true };
                        }
                        else
                        {
                            oBOI.Attributes["SearchFields".ToLower()] = new XIIAttribute { sName = "SearchFields", sValue = null, bDirty = true };
                        }
                        oBOI.Attributes["InnerReportID".ToLower()] = new XIIAttribute { sName = "InnerReportID", sValue = "0", bDirty = true };
                        oBOI.Attributes["ActionFields".ToLower()] = new XIIAttribute { sName = "ActionFields", sValue = null, bDirty = true };
                        oBOI.Attributes["ActionFieldValue".ToLower()] = new XIIAttribute { sName = "ActionFieldValue", sValue = null, bDirty = true };
                        oBOI.Attributes["ViewFields".ToLower()] = new XIIAttribute { sName = "ViewFields", sValue = null, bDirty = true };
                        oBOI.Attributes["EditableFields".ToLower()] = new XIIAttribute { sName = "EditableFields", sValue = null, bDirty = true };
                        oBOI.Attributes["NonEditableFields".ToLower()] = new XIIAttribute { sName = "NonEditableFields", sValue = null, bDirty = true };
                        oBOI.Attributes["Description".ToLower()] = new XIIAttribute { sName = "Description", sValue = oAttDef, bDirty = true };
                        if (FKiStructureID.sValue != null)
                        {
                            oBOI.Attributes["FKiStructureID".ToLower()] = new XIIAttribute { sName = "FKiStructureID", sValue = FKiStructureID.sValue, bDirty = true };
                            XIDBODefault oDefault = new XIDBODefault();
                            oDefault = (XIDBODefault)oCache.GetObjectFromCache(XIConstant.CacheBODefault, null, oBOIDDef.sValue.ToString());
                            if(oDefault.iStructureID.ToString() == FKiStructureID.sValue)
                            {
                                oBOI.SetAttribute("RowXiLinkID", oDefault.iPopupID.ToString());
                            }
                        }
                        else if (oAttRowClick)
                        {
                            XIDBODefault oDefault = new XIDBODefault();
                            oDefault = (XIDBODefault)oCache.GetObjectFromCache(XIConstant.CacheBODefault, null, iBODID.ToString());
                            oBOI.SetAttribute("RowXiLinkID", oDefault.iPopupID.ToString());
                        }
                        if (searchFilter != null && searchFilter.sValue == "20")
                        {
                            oBOI.Attributes["IsFilterSearch".ToLower()] = new XIIAttribute { sName = "IsFilterSearch", sValue = "1", bDirty = true };
                        }
                        else
                        {
                            oBOI.Attributes["IsFilterSearch".ToLower()] = new XIIAttribute { sName = "IsFilterSearch", sValue = "0", bDirty = true };
                        }
                        if (searchFilter != null && searchFilter.sValue == "10")
                        {
                            oBOI.Attributes["IsNaturalSearch".ToLower()] = new XIIAttribute { sName = "IsNaturalSearch", sValue = "1", bDirty = true };
                        }
                        else
                        {
                            oBOI.Attributes["IsNaturalSearch".ToLower()] = new XIIAttribute { sName = "IsNaturalSearch", sValue = "0", bDirty = true };
                        }
                        if (searchFilter != null && searchFilter.sValue == "30")
                        {
                            oBOI.Attributes["IsFilterSearch".ToLower()] = new XIIAttribute { sName = "IsFilterSearch", sValue = "0", bDirty = true };
                            oBOI.Attributes["IsNaturalSearch".ToLower()] = new XIIAttribute { sName = "IsNaturalSearch", sValue = "0", bDirty = true };
                        }
                        if (searchFilter != null)
                        {
                            oBOI.Attributes["FKiFilterSearch".ToLower()] = new XIIAttribute { sName = "FKiFilterSearch", sValue = searchFilter.sValue, bDirty = true };
                        }

                        oBOI.Attributes["IsParent".ToLower()] = new XIIAttribute { sName = "IsParent", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsExport".ToLower()] = new XIIAttribute { sName = "IsExport", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsRowClick".ToLower()] = new XIIAttribute { sName = "IsRowClick", sValue = oAttRowClick.ToString(), bDirty = true };
                        oBOI.Attributes["OnRowClickType".ToLower()] = new XIIAttribute { sName = "OnRowClickType", sValue = OnRowClickType, bDirty = true };
                        oBOI.Attributes["OnRowClickValue".ToLower()] = new XIIAttribute { sName = "OnRowClickValue", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsColumnClick".ToLower()] = new XIIAttribute { sName = "IsColumnClick", sValue = "0", bDirty = true };
                        oBOI.Attributes["ColumnXiLinkID".ToLower()] = new XIIAttribute { sName = "ColumnXiLinkID", sValue = "0", bDirty = true };
                        oBOI.Attributes["OnClickColumn".ToLower()] = new XIIAttribute { sName = "OnClickColumn", sValue = null, bDirty = true };
                        oBOI.Attributes["OnColumnClickType".ToLower()] = new XIIAttribute { sName = "OnColumnClickType", sValue = null, bDirty = true };
                        oBOI.Attributes["OnClickParameter".ToLower()] = new XIIAttribute { sName = "OnClickParameter", sValue = null, bDirty = true };
                        oBOI.Attributes["OnColumnClickValue".ToLower()] = new XIIAttribute { sName = "OnColumnClickValue", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsCellClick".ToLower()] = new XIIAttribute { sName = "IsCellClick", sValue = "0", bDirty = true };
                        oBOI.Attributes["CellXiLinkID".ToLower()] = new XIIAttribute { sName = "CellXiLinkID", sValue = "0", bDirty = true };
                        oBOI.Attributes["OnClickCell".ToLower()] = new XIIAttribute { sName = "OnClickCell", sValue = null, bDirty = true };
                        oBOI.Attributes["OnCellClickType".ToLower()] = new XIIAttribute { sName = "OnCellClickType", sValue = null, bDirty = true };
                        oBOI.Attributes["OnCellClickValue".ToLower()] = new XIIAttribute { sName = "OnCellClickValue", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsRowTotal".ToLower()] = new XIIAttribute { sName = "IsRowTotal", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsColumnTotal".ToLower()] = new XIIAttribute { sName = "IsColumnTotal", sValue = "0", bDirty = true };
                        oBOI.Attributes["ResultIn".ToLower()] = new XIIAttribute { sName = "ResultIn", sValue = null, bDirty = true };
                        oBOI.Attributes["PopupType".ToLower()] = new XIIAttribute { sName = "PopupType", sValue = null, bDirty = true };
                        oBOI.Attributes["PopupLeft".ToLower()] = new XIIAttribute { sName = "PopupLeft", sValue = "0", bDirty = true };
                        oBOI.Attributes["PopupTop".ToLower()] = new XIIAttribute { sName = "PopupTop", sValue = "0", bDirty = true };
                        oBOI.Attributes["PopupWidth".ToLower()] = new XIIAttribute { sName = "PopupWidth", sValue = "0", bDirty = true };
                        oBOI.Attributes["PopupHeight".ToLower()] = new XIIAttribute { sName = "PopupHeight", sValue = "0", bDirty = true };
                        oBOI.Attributes["DialogMy1".ToLower()] = new XIIAttribute { sName = "DialogMy1", sValue = null, bDirty = true };
                        oBOI.Attributes["DialogMy2".ToLower()] = new XIIAttribute { sName = "DialogMy2", sValue = null, bDirty = true };
                        oBOI.Attributes["DialogAt1".ToLower()] = new XIIAttribute { sName = "DialogAt1", sValue = null, bDirty = true };
                        oBOI.Attributes["DialogAt2".ToLower()] = new XIIAttribute { sName = "DialogAt2", sValue = null, bDirty = true };
                        oBOI.Attributes["IsCreate".ToLower()] = new XIIAttribute { sName = "IsCreate", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsEdit".ToLower()] = new XIIAttribute { sName = "IsEdit", sValue = "0", bDirty = true };
                        oBOI.Attributes["IsDelete".ToLower()] = new XIIAttribute { sName = "IsDelete", sValue = "0", bDirty = true };
                        oBOI.Attributes["CreateRoleID".ToLower()] = new XIIAttribute { sName = "CreateRoleID", sValue = "0", bDirty = true };
                        oBOI.Attributes["EditRoleID".ToLower()] = new XIIAttribute { sName = "EditRoleID", sValue = "0", bDirty = true };
                        oBOI.Attributes["DeleteRoleID".ToLower()] = new XIIAttribute { sName = "DeleteRoleID", sValue = "0", bDirty = true };
                        oBOI.Attributes["CreateGroupID".ToLower()] = new XIIAttribute { sName = "CreateGroupID", sValue = "0", bDirty = true };
                        oBOI.Attributes["EditGroupID".ToLower()] = new XIIAttribute { sName = "EditGroupID", sValue = "0", bDirty = true };
                        oBOI.Attributes["iLayoutID".ToLower()] = new XIIAttribute { sName = "iLayoutID", sValue = "0", bDirty = true };
                        oBOI.Attributes["StatusTypeID".ToLower()] = new XIIAttribute { sName = "StatusTypeID", sValue = "10", bDirty = true };
                        oBOI.Attributes["CreatedBy".ToLower()] = new XIIAttribute { sName = "CreatedBy", sValue = SessionManager.UserID.ToString(), bDirty = true };
                        oBOI.Attributes["CreatedBySYSID".ToLower()] = new XIIAttribute { sName = "CreatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                        oBOI.Attributes["CreatedTime".ToLower()] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                        oBOI.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = "1", bDirty = true };
                        oBOI.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                        oBOI.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                        oBOI.Attributes["sSystemType".ToLower()] = new XIIAttribute { sName = "sSystemType", sValue = "Organisation", bDirty = true };
                        oBOI.Attributes["FKiApplicationID".ToLower()] = new XIIAttribute { sName = "FKiApplicationID", sValue = SessionManager.ApplicationID.ToString(), bDirty = true };
                        oBOI.Attributes["FKiComponentID".ToLower()] = new XIIAttribute { sName = "FKiComponentID", sValue = "0", bDirty = true };
                        oBOI.Attributes["RepeaterType".ToLower()] = new XIIAttribute { sName = "RepeaterType", sValue = "0", bDirty = true };
                        oBOI.Attributes["RepeaterComponentID".ToLower()] = new XIIAttribute { sName = "RepeaterComponentID", sValue = "0", bDirty = true };
                        oBOI.Attributes["iCreateXILinkID".ToLower()] = new XIIAttribute { sName = "iCreateXILinkID", sValue = "0", bDirty = true };
                        oBOI.Attributes["sAddLabel".ToLower()] = new XIIAttribute { sName = "sAddLabel", sValue = null, bDirty = true };
                        oBOI.Attributes["sCreateType".ToLower()] = new XIIAttribute { sName = "sCreateType", sValue = null, bDirty = true };
                        oBOI.Attributes["IsRefresh".ToLower()] = new XIIAttribute { sName = "IsRefresh", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsMultiBO".ToLower()] = new XIIAttribute { sName = "bIsMultiBO", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsCheckbox".ToLower()] = new XIIAttribute { sName = "bIsCheckbox", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsView".ToLower()] = new XIIAttribute { sName = "bIsView", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsCopy".ToLower()] = new XIIAttribute { sName = "bIsCopy", sValue = "0", bDirty = true };
                        oBOI.Attributes["sRowXiLinkType".ToLower()] = new XIIAttribute { sName = "sRowXiLinkType", sValue = null, bDirty = true };
                        oBOI.Attributes["bIsLockToUser".ToLower()] = new XIIAttribute { sName = "bIsLockToUser", sValue = "0", bDirty = true };
                        oBOI.Attributes["iPaginationCount".ToLower()] = new XIIAttribute { sName = "iPaginationCount", sValue = iPaginationCount, bDirty = true };
                        oBOI.Attributes["bIsStaticView".ToLower()] = new XIIAttribute { sName = "bIsStaticView", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsPreview".ToLower()] = new XIIAttribute { sName = "bIsPreview", sValue = "0", bDirty = true };
                        oBOI.Attributes["sFileExtension".ToLower()] = new XIIAttribute { sName = "sFileExtension", sValue = ".xls", bDirty = true };
                        oBOI.Attributes["sTotalColumns".ToLower()] = new XIIAttribute { sName = "sTotalColumns", sValue = sTotal, bDirty = true };
                        oBOI.Attributes["FKiVisualisationID".ToLower()] = new XIIAttribute { sName = "FKiVisualisationID", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsRecordLock".ToLower()] = new XIIAttribute { sName = "bIsRecordLock", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsMultiSearch".ToLower()] = new XIIAttribute { sName = "bIsMultiSearch", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsXICreatedBy".ToLower()] = new XIIAttribute { sName = "bIsXICreatedBy", sValue = "0", bDirty = true };
                        oBOI.Attributes["FKiCrtd1ClickID".ToLower()] = new XIIAttribute { sName = "FKiCrtd1ClickID", sValue = "0", bDirty = true };
                        oBOI.Attributes["bIsXIUpdatedBy".ToLower()] = new XIIAttribute { sName = "bIsXIUpdatedBy", sValue = "0", bDirty = true };
                        oBOI.Attributes["FKiUpdtd1ClickID".ToLower()] = new XIIAttribute { sName = "FKiUpdtd1ClickID", sValue = "0", bDirty = true };
                        oBOI.Attributes["sLog".ToLower()] = new XIIAttribute { sName = "sLog", sValue = "No", bDirty = true };
                        if (ListGroupID != "0")
                        {
                            oBOI.Attributes["FKiListGroup".ToLower()] = new XIIAttribute { sName = "FKiListGroup", sValue = ListGroupID, bDirty = true };
                        }
                        else
                        {
                            oBOI.Attributes["FKiListGroup".ToLower()] = new XIIAttribute { sName = "FKiListGroup", sValue = null, bDirty = true };
                        }
                        oBOI.Attributes["FKiSearchGroup".ToLower()] = new XIIAttribute { sName = "FKiSearchGroup", sValue = SearchGroupID, bDirty = true };
                        XiBO = oBOI.Save(oBOI);
                        var i1ClickID = oBOI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();

                        if (XiBO.bOK && XiBO.oResult != null)
                        {
                            oBOI = (XIIBO)XiBO.oResult;
                            var Query = sListQuery;
                            oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|1ClickID}", i1ClickID, "", null);
                            oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|iBODID}", iBOID, "", null);
                            oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|Query}", Query, "", null);
                        }
                        var skey1 = oCache.CacheKeyBuilder(XIConstant.Cache1Click, "", "") + "_" + i1ClickID;
                        XIApplicationsController cache1 = new XIApplicationsController();
                        cache.RemoveCacheByKey(skey1, EnumCacheTypes.Application);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
            }
            return XiBO;
        }


        public ActionResult AttributeChange(int BOID, string AttributeID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, BOID.ToString());
                if (AttributeID != "")
                {
                    var AttrF = oBOD.Attributes.Where(s => s.Value.ID == Convert.ToInt32(AttributeID)).Select(s => s.Value).FirstOrDefault();
                    return Json(AttrF, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [AllowAnonymous]
        public ActionResult GetBoDependencyGroupDropDown(int BOID, string sGUID, string BOName)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            List<XIDAttribute> obj = new List<XIDAttribute>();
            try
            {
                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iParentID}", BOID.ToString(), null, null);
                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                Dictionary<string, object> BOLabelName = new Dictionary<string, object>();
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BOName, null);
                BOLabelName["BOID"] = BOID;
                if (BOD != null && !string.IsNullOrEmpty(BOD.TableName))
                {
                    var QSFieldOrigin = Connection.Select<XIDAttribute>(BOD.TableName, BOLabelName).ToList();
                    obj = (List<XIDAttribute>)QSFieldOrigin;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "XIDNA");
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DispalyTemplate(int iStructureID, int iInstanceID, string sTemplateID, string sGUID)
        {
            try
            {
                if (sTemplateID == "")
                {
                    sTemplateID = null;
                }
                List<CNV> oParams = new List<CNV>();
                List<CNV> oParamoneClicks = new List<CNV>();
                var sSessionID = HttpContext.Session.SessionID;
                int iUserID = SessionManager.UserID;
                var oStructured = (XIDStructure)oCache.GetObjectFromCache(XIConstant.CacheStructureCode, null, iStructureID.ToString());
                XIInfraOneClickComponent o1click = new XIInfraOneClickComponent();
                XIInfraHtmlComponent oHTML = new XIInfraHtmlComponent();
                if (iInstanceID > 0)
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ID}", iInstanceID.ToString(), null, null);
                }
                oParamoneClicks.Add(new CNV { sName = "sGUID", sValue = sGUID });
                oParamoneClicks.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
                oParamoneClicks.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                oParamoneClicks.Add(new CNV { sName = "1ClickID", sValue = oStructured.oStructureDetails.i1ClickID.ToString() });
                oParamoneClicks.Add(new CNV { sName = "StructureName", sValue = oStructured.sCode });
                oParamoneClicks.Add(new CNV { sName = "sTemplate", sValue = sTemplateID });
                oParamoneClicks.Add(new CNV { sName = "InsertParameter", sValue = "true" });
                var i1clickResult = o1click.XILoad(oParamoneClicks);
                var resulit = i1clickResult.oResult;
                XID1Click D1Click = (XID1Click)resulit;
                var HtmlResult = D1Click.RepeaterResult[0];
                return Json(HtmlResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult HTMLDataTemplate(int iStructureID, int iInstanceID, string sTemplateID, string sGUID)
        {
            try
            {
                if (sTemplateID == "")
                {
                    sTemplateID = null;
                }
                List<CNV> oParamoneClicks = new List<CNV>();
                var sSessionID = HttpContext.Session.SessionID;
                int iUserID = SessionManager.UserID;
                var oStructured = (XIDStructure)oCache.GetObjectFromCache(XIConstant.CacheStructureCode, null, iStructureID.ToString());
                XIInfraOneClickComponent o1click = new XIInfraOneClickComponent();
                XIInfraHtmlComponent oHTML = new XIInfraHtmlComponent();
                if (iInstanceID > 0)
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ID}", iInstanceID.ToString(), null, null);
                }
                oParamoneClicks.Add(new CNV { sName = "sGUID", sValue = sGUID });
                oParamoneClicks.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
                oParamoneClicks.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                oParamoneClicks.Add(new CNV { sName = "1ClickID", sValue = oStructured.oStructureDetails.i1ClickID.ToString() });
                oParamoneClicks.Add(new CNV { sName = "StructureName", sValue = oStructured.sCode });
                oParamoneClicks.Add(new CNV { sName = "sTemplate", sValue = sTemplateID });
                oParamoneClicks.Add(new CNV { sName = "InsertParameter", sValue = "true" });
                var i1clickResult = o1click.XILoad(oParamoneClicks);
                if (i1clickResult.bOK == true && i1clickResult.oResult != null && sTemplateID == null)
                {
                    var boid = oStructured.BOID;
                    XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, boid.ToString());
                    var sGroup = oBOD.Groups;
                    var GroupofFields = sGroup.Select(t => t.Value).ToList();
                    var Groupname = GroupofFields.Where(n => n.GroupName.Contains(XIConstant.MainGroup)).FirstOrDefault();
                    if (Groupname != null)
                    {
                        var bofieldnames = Groupname.BOFieldNames.ToLower();
                        List<string> TagIds = bofieldnames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var result = i1clickResult.oResult;
                        XID1Click D1Click = (XID1Click)result;
                        var StructureDetails = D1Click.StructureInstanceData;
                        StructureDetails.oStructureInstance.Values.ToList().FirstOrDefault().Select(n => n.Attributes).ToList()
                           .FirstOrDefault().Where(r => TagIds.Any(n => n == r.Value.sName.ToLower())).ToList().ForEach(c => c.Value.bIsHidden = true);
                        var oparent = (Dictionary<string, List<XIIBO>>)StructureDetails.oParent;
                        ChildResult(oparent);
                        var DATAMerge = MergeHTML("~/views/XIComponents/_HTMLDataComponent.cshtml", StructureDetails);
                        var oDataMerged = DATAMerge;
                        var oResultContent = oDataMerged.GetType().GetProperty("Content").GetValue(oDataMerged, null);
                        return Json(oResultContent, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var resulit = i1clickResult.oResult;
                    XID1Click D1Click1 = (XID1Click)resulit;
                    var HtmlResult = D1Click1.RepeaterResult[0];
                    return Json(HtmlResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public XIBOInstance ChildResult(Dictionary<string, List<XIIBO>> oparent)
        {
            XIBOInstance structuredata = new XIBOInstance();
            foreach (var item in oparent)
            {
                XIDBO oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, item.Key, null);
                var sBOGroup = oBODef.Groups;
                var GroupFields = sBOGroup.Select(t => t.Value).ToList();
                var GroupofName = GroupFields.Where(n => n.GroupName.Contains(XIConstant.MainGroup)).FirstOrDefault();
                if (GroupofName != null)
                {
                    var FieldNames = GroupofName.BOFieldNames.ToLower();
                    List<string> FieldsTagIds = FieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var list = item.Value.Select(n => n.Attributes).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var listitem in list)
                        {
                            listitem.Where(r => FieldsTagIds.Any(n => n == r.Value.sName.ToLower())).ToList().ForEach(c => c.Value.bIsHidden = true);
                        }
                    }
                }
                item.Value.FirstOrDefault().BOD = oBODef;
                var o2SubChild = item.Value.FirstOrDefault().SubChildI;
                if (o2SubChild != null && o2SubChild.Count > 0)
                {
                    ChildResult(o2SubChild);
                }
            }
            structuredata.oParent = oparent;
            return structuredata;
        }
        public ActionResult DownloadPDF(string iInstanceID, string sGUID, string iStructureID, string sTemplateID)
        {
            try
            {
                if (sTemplateID == "")
                {
                    sTemplateID = null;
                }
                FileResult fileresult = null;
                XIInfraCache oCache = new XIInfraCache();
                object oResultContent = null;
                var sSessionID = HttpContext.Session.SessionID;
                if (!string.IsNullOrEmpty(iInstanceID))
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ID}", iInstanceID, null, null);
                }
                var oStructured = (XIDStructure)oCache.GetObjectFromCache(XIConstant.CacheStructureCode, null, iStructureID);
                XIInfraOneClickComponent oneclick = new XIInfraOneClickComponent();
                oneclick.OneClickID = oStructured.oStructureDetails.i1ClickID;
                XIDComponent oCompD = new XIDComponent();
                var oParams = oCompD.Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue }).ToList();
                oParams = (List<CNV>)oCache.ResolveParameters(oParams, null, sGUID);
                oParams.Add(new CNV { sName = "sGUID", sValue = sGUID });
                oParams.Add(new CNV { sName = "sSessionID", sValue = sSessionID });
                oParams.Add(new CNV { sName = "1ClickID", sValue = oStructured.oStructureDetails.i1ClickID.ToString() });
                oParams.Add(new CNV { sName = "StructureName", sValue = oStructured.sCode });
                oParams.Add(new CNV { sName = "sTemplate", sValue = sTemplateID });
                oParams.Add(new CNV { sName = "InsertParameter", sValue = "true" });
                var i1clickResult = oneclick.XILoad(oParams);
                if (i1clickResult.bOK == true && i1clickResult.oResult != null && sTemplateID == null)
                {
                    var boid = oStructured.BOID;
                    XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, boid.ToString());
                    var sGroup = oBOD.Groups;
                    var GroupofFields = sGroup.Select(t => t.Value).ToList();
                    var Groupname = GroupofFields.Where(n => n.GroupName.Contains(XIConstant.MainGroup)).FirstOrDefault();
                    if (Groupname != null)
                    {
                        var bofieldnames = Groupname.BOFieldNames.ToLower();
                        List<string> TagIds = bofieldnames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var result = i1clickResult.oResult;
                        XID1Click D1Click = (XID1Click)result;
                        var StructureDetails = D1Click.StructureInstanceData;
                        StructureDetails.oStructureInstance.Values.ToList().FirstOrDefault().Select(n => n.Attributes).ToList()
                           .FirstOrDefault().Where(r => TagIds.Any(n => n == r.Value.sName.ToLower())).ToList().ForEach(c => c.Value.bIsHidden = true);
                        var oparent = (Dictionary<string, List<XIIBO>>)StructureDetails.oParent;
                        ChildResult(oparent);
                        var DATAMerge = MergeHTML("~/views/XIComponents/_HTMLDataComponent.cshtml", StructureDetails);
                        var MergedResult = DATAMerge;
                        oResultContent = MergedResult.GetType().GetProperty("Content").GetValue(MergedResult, null);
                    }
                }
                else
                {
                    var resulit = i1clickResult.oResult;
                    XID1Click D1Click1 = (XID1Click)resulit;
                    oResultContent = D1Click1.RepeaterResult[0];
                }
                XIInfraEmail oEmail = new XIInfraEmail();
                var oConvertData = oEmail.ModificationPDFGenerate(oResultContent.ToString(), false, "", "");
                fileresult = new FileContentResult((byte[])(((MemoryStream)oConvertData.oResult).ToArray()), "application/pdf");
                fileresult.FileDownloadName = oStructured.sStructureName + "(" + iInstanceID + ").pdf";
                return fileresult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [HttpPost]
        public ActionResult SaveGaugeChartValues(VMList model)
        {
            bool bIsSuccess = false;
            try
            {
                //CResult XiBO = new CResult();
                //XIInfraCache oCache = new XIInfraCache();
                //XID1Click o1ClickD = new XID1Click();
                //o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, model.OneClickID.ToString());
                //string sOnColumnLast = o1ClickD.SelectFields.Split(',')[o1ClickD.SelectFields.Split(',').Count() - 1].Replace(" ", "").ToLower();
                //string sOnColumnFirst = o1ClickD.SelectFields.Split(',')[0].Replace(" ", "").ToLower();
                //XIDBO oBODs = new XIDBO();
                //XIIBO oBOI = new XIIBO();
                //XID1Click oBO1Click = new XID1Click();
                ////oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, o1ClickD.FromBos);
                //oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickD.BOID.ToString());
                //oBOI.Attributes[sOnColumnLast.ToLower()] = new XIIAttribute { sName = sOnColumnLast, sValue = model.Value, bDirty = true };
                //oBOI.Attributes[sOnColumnFirst.ToLower()] = new XIIAttribute { sName = sOnColumnFirst, sValue = model.XIGUID, bDirty = true };
                //oBOI.BOD = oBODs;
                //XiBO = oBOI.Save(oBOI);
                //var skey = oCache.CacheKeyBuilder(XIConstant.Cache1Click, "", "") + "_" + model.Value;
                //XIApplicationsController cache = new XIApplicationsController();
                //cache.RemoveCacheByKey(skey, EnumCacheTypes.Application);
                XIIBO oBOI = new XIIBO();
                XIDBO oBODs = new XIDBO();
                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "ProductVersion_T");
                oBOI.BOD = oBODs;
                oBOI.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = model.XIGUID, bDirty = true };
                oBOI.Attributes["rAdditionalLoad".ToLower()] = new XIIAttribute { sName = "rAdditionalLoad", sValue = model.Value, bDirty = true };
                var XiBO = oBOI.Save(oBOI);
                if (XiBO.bOK && XiBO.oResult != null)
                {
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(bIsSuccess, JsonRequestBehavior.AllowGet);
        }
        //Copy For Simple1click
        public ActionResult CopyFor1Click(int iInstanceID, int BOID)
        {
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIApplicationsController cache = new XIApplicationsController();
                string sSessionID = HttpContext.Session.SessionID;
                XIDBO oBODs = new XIDBO();
                XIIXI oXII = new XIIXI();
                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BOID.ToString());
                XIIBO oBOICopy = oXII.BOI(oBODs.Name, iInstanceID.ToString());
                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BOID.ToString());
                oBOICopy.BOD = oBODs;
                oBOICopy.Exclude("id");
                oBOICopy.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
                if (oBODs.Name == "XI1Click")
                {
                    var name = oBOICopy.Attributes.Where(m => m.Value.sName.ToLower() == "Name".ToLower()).Select(m => m.Value.sPreviousValue).FirstOrDefault() + "Copy";//().ForEach(m => m.Value.bDirty = true);
                    oBOICopy.Attributes["Name"].sValue = name;
                    oBOICopy.Attributes["Title"].sValue = name;
                    oBOICopy.Attributes["Description"].sValue = name;
                    var XiBO = oBOICopy.Save(oBOICopy);
                }
                else
                {
                    var XiBO = oBOICopy.Save(oBOICopy);

                }
                var i1ClickID = oBOICopy.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        //Delete For Simple1click
        public ActionResult DeleteFor1Click(int iInstanceID, int BOID)
        {
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIApplicationsController cache = new XIApplicationsController();
                string sSessionID = HttpContext.Session.SessionID;
                XIDBO oBODs = new XIDBO();
                XIIXI oXII = new XIIXI();
                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BOID.ToString());
                XIIBO oBOIDelete = oXII.BOI(oBODs.Name, iInstanceID.ToString());
                CResult oCResult = oBOIDelete.Delete(oBOIDelete);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        // Rowxilinkid for simple1click result grid
        public string GetRowXilinkID(string OneClickID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                Dictionary<string, object> BOGroupsParams = new Dictionary<string, object>();
                var RowXilinkID = Connection.Select<XID1Click>("XI1Click_T", BOGroupsParams).Where(s => s.ID == Convert.ToInt32(OneClickID)).Select(s => s.RowXiLinkID).FirstOrDefault();
                return RowXilinkID.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        private string GetTextFromPDF(string sPath, string DocID)
        {
            StringBuilder text = new StringBuilder();
            using (PdfReader reader = new PdfReader(sPath))
            {
                List<CNV> WhrParams = new List<CNV>();
                WhrParams.Add(new CNV { sName = "id", sValue = DocID });
                XIIXI oXII = new XIIXI();
                var oBOII = oXII.BOI("XIDocumentTree", null, null, WhrParams);
                if (oBOII != null && oBOII.Attributes.Count() > 0)
                {
                    //for (int i = 1; i <= reader.NumberOfPages; i++)
                    //{
                    //    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                    //}
                    var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIDocumentTree");
                    XIIBO oBOIDoc = new XIIBO();
                    oBOII.SetAttribute("sPageNo", "1");
                    oBOII.SetAttribute("stags", text.ToString());
                    oBOII.BOD = BOD;
                    var oCR = oBOII.Save(oBOII);
                }

                //if (i == 1)
                //{
                //    oBOII.SetAttribute("sPageNo", i.ToString());
                //    oBOII.SetAttribute("stags", PdfTextExtractor.GetTextFromPage(reader, i));
                //    oBOII.BOD = BOD;
                //    var oCR = oBOII.Save(oBOII);
                //}
                //else
                //{
                //    oBOIDoc.SetAttribute("sName", oBOII.AttributeI("sname").sValue);
                //    oBOIDoc.SetAttribute("sPath", oBOII.AttributeI("sPath").sValue);
                //    oBOIDoc.SetAttribute("sParentID", oBOII.AttributeI("sParentID").sValue);
                //    oBOIDoc.SetAttribute("iBuildingID", oBOII.AttributeI("iBuildingID").sValue);
                //    oBOIDoc.SetAttribute("sType", "20");
                //    oBOIDoc.SetAttribute("sPageNo", i.ToString());
                //    oBOIDoc.SetAttribute("sVersion", oBOII.AttributeI("sVersion").sValue);
                //    oBOIDoc.SetAttribute("iVersionBatchID", oBOII.AttributeI("iVersionBatchID").sValue);
                //    oBOIDoc.SetAttribute("iApprovalStatus", "20");
                //    oBOIDoc.SetAttribute("stags", PdfTextExtractor.GetTextFromPage(reader, i));
                //    oBOIDoc.BOD = BOD;
                //    var oCR = oBOIDoc.Save(oBOIDoc);
                //}
            }

            return text.ToString();
        }
        //Save Mappping Details for only Menu Mapping  Tables
        public string SaveMenuRoles(string BOFieldNames, string SGUID, string sBO)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            CResult oCR = new CResult(); // always
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                int iUserID = SessionManager.UserID;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var MenuID = oCache.Get_ParamVal(sSessionID, SGUID, null, "{XIP|iInstanceID}");
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["MenuID"] = MenuID;
                Params[XIConstant.Key_XIDeleted] = "0";
                var oXIDRoles = Connection.Select<XIRoleMenus>(oBOD.TableName, Params).ToList();
                oBOI.BOD = oBOD;
                if (!string.IsNullOrEmpty(BOFieldNames))
                {
                    for (var i = 0; i < BOFieldNames.Split(',').Count(); i++)
                    {
                        var idhereornot = oXIDRoles.Where(s => s.RoleID == Convert.ToInt32(BOFieldNames.Split(',')[i]) && s.FKiInboxID == MenuID).Count();
                        if (idhereornot == 0)
                        {
                            oBOI.SetAttribute("ID", "0");
                            oBOI.SetAttribute("MenuID", MenuID);
                            oBOI.SetAttribute("FKiApplicationID", oUser.FKiApplicationID.ToString());
                            oBOI.SetAttribute("OrgID", oUser.FKiOrganisationID.ToString());
                            oBOI.SetAttribute("RoleID", BOFieldNames.Split(',')[i]);
                            oBOI.SetAttribute("StatusTypeID", "10");
                            oBOI.SetAttribute(XIConstant.Key_XIDeleted, "0");
                            oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                            oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                            oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                            oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                            oBOI.BOD = oBOD;
                            oCR = oBOI.Save(oBOI);
                        }
                    }
                }
                foreach (var item in oXIDRoles)
                {
                    if (!BOFieldNames.Split(',').ToList().Contains(item.RoleID.ToString()))
                    {
                        List<CNV> WhrParams = new List<CNV>();

                        WhrParams.Add(new CNV { sName = "MenuID", sValue = MenuID });
                        WhrParams.Add(new CNV { sName = "RoleID", sValue = item.RoleID.ToString() });
                        WhrParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                        XIIXI oXI = new XIIXI();
                        var RIBOI = oXI.BOI(sBO, null, null, WhrParams);
                        RIBOI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                        oCR = RIBOI.Save(RIBOI);
                    }
                }
                if (oCR.bOK && oCR.oResult != null)
                {
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public string ClickPermission(string BOFieldNames, string SGUID, string sBO, string sAttr)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            CResult oCR = new CResult(); // always
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                int iUserID = SessionManager.UserID;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var InstanceID = oCache.Get_ParamVal(sSessionID, SGUID, null, "{XIP|iInstanceID}");
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[sAttr] = InstanceID;
                Params[XIConstant.Key_XIDeleted] = "0";
                XID1Click o1Click = new XID1Click();
                o1Click.BOID = oBOD.BOID;
                o1Click.Query = "select * from " + oBOD.TableName + " where " + sAttr + "=" + InstanceID + " and " + XIConstant.Key_XIDeleted + "=0";
                var oRes = o1Click.OneClick_Run();
                var oXIDRoles = Connection.Select<XIRoleMenus>(oBOD.TableName, Params).ToList();
                oBOI.BOD = oBOD;
                var RoleIDs = new List<string>();
                var BOIs = oRes.Values.ToList();
                int j = 0;
                foreach (var BOI in BOIs)
                {
                    var ID = BOI.AttributeI("fkiroleid").sValue;
                    var iDelete = BOI.AttributeI(XIConstant.Key_XIDeleted).sValue;
                    if (iDelete == "0")
                    {
                        RoleIDs.Add(ID);
                    }
                    j++;
                }
                if (!string.IsNullOrEmpty(BOFieldNames))
                {
                    for (var i = 0; i < BOFieldNames.Split(',').Count(); i++)
                    {
                        var Found = RoleIDs.Where(m => m == BOFieldNames.Split(',')[i]).FirstOrDefault();
                        //var idhereornot = oXIDRoles.Where(s => s.RoleID == Convert.ToInt32(BOFieldNames.Split(',')[i]) && s.FKiInboxID == MenuID).Count();
                        if (Found == null)
                        {
                            oBOI.SetAttribute("ID", "0");
                            oBOI.SetAttribute(sAttr, InstanceID);
                            oBOI.SetAttribute("FKiAppID", oUser.FKiApplicationID.ToString());
                            oBOI.SetAttribute("FKiOrgID", oUser.FKiOrganisationID.ToString());
                            oBOI.SetAttribute("FKiRoleID", BOFieldNames.Split(',')[i]);
                            oBOI.SetAttribute("StatusTypeID", "10");
                            oBOI.SetAttribute(XIConstant.Key_XIDeleted, "0");
                            oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                            oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                            oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                            oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                            oBOI.BOD = oBOD;
                            oCR = oBOI.Save(oBOI);
                        }
                    }
                }
                foreach (var item in RoleIDs)
                {
                    if (!BOFieldNames.Split(',').ToList().Contains(item.ToString()))
                    {
                        List<CNV> WhrParams = new List<CNV>();

                        WhrParams.Add(new CNV { sName = sAttr, sValue = InstanceID });
                        WhrParams.Add(new CNV { sName = "FKiRoleID", sValue = item.ToString() });
                        WhrParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                        XIIXI oXI = new XIIXI();
                        var RIBOI = oXI.BOI(sBO, null, null, WhrParams);
                        RIBOI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                        oCR = RIBOI.Save(RIBOI);
                    }
                }
                if (oCR.bOK && oCR.oResult != null)
                {
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public string SaveInboxRoles(string BOFieldNames, string SGUID, string sBO)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            CResult oCR = new CResult(); // always
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                int iUserID = SessionManager.UserID;
                oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var MenuID = oCache.Get_ParamVal(sSessionID, SGUID, null, "{XIP|iInstanceID}");
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["FKiInboxID"] = MenuID;
                Params[XIConstant.Key_XIDeleted] = "0";
                var oXIDRoles = Connection.Select<XIRoleMenus>(oBOD.TableName, Params).ToList();
                oBOI.BOD = oBOD;
                if (!string.IsNullOrEmpty(BOFieldNames))
                {
                    for (var i = 0; i < BOFieldNames.Split(',').Count(); i++)
                    {
                        var idhereornot = oXIDRoles.Where(s => s.RoleID == Convert.ToInt32(BOFieldNames.Split(',')[i]) && s.FKiInboxID == MenuID).Count();
                        if (idhereornot == 0)
                        {
                            oBOI.SetAttribute("ID", "0");
                            oBOI.SetAttribute("FKiInboxID", MenuID);
                            oBOI.SetAttribute("FKiApplicationID", oUser.FKiApplicationID.ToString());
                            oBOI.SetAttribute("OrgID", oUser.FKiOrganisationID.ToString());
                            oBOI.SetAttribute("RoleID", BOFieldNames.Split(',')[i]);
                            oBOI.SetAttribute("StatusTypeID", "10");
                            oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                            oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                            oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                            oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                            oBOI.BOD = oBOD;
                            oCR = oBOI.Save(oBOI);
                        }
                    }
                }
                foreach (var item in oXIDRoles)
                {
                    if (!BOFieldNames.Split(',').ToList().Contains(item.RoleID.ToString()))
                    {
                        List<CNV> WhrParams = new List<CNV>();

                        WhrParams.Add(new CNV { sName = "FKiInboxID", sValue = MenuID });
                        WhrParams.Add(new CNV { sName = "RoleID", sValue = item.RoleID.ToString() });
                        WhrParams.Add(new CNV { sName = XIConstant.Key_XIDeleted, sValue = "0" });
                        XIIXI oXI = new XIIXI();
                        var RIBOI = oXI.BOI(sBO, null, null, WhrParams);
                        RIBOI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                        oCR = RIBOI.Save(RIBOI);
                    }
                }
                if (oCR.bOK && oCR.oResult != null)
                {
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult SaveMenuTreeDetails(string RootNode, string ParentNode, string NodeID, string NodeTitle, string Type, string SBOName, int iOrgID)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            var iAppID = SessionManager.ApplicationID;
            oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
            oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
            try
            {
                XIMenu oMenu = new XIMenu();
                oMenu.FKiApplicationID = iAppID;
                XIConfigs Configs = new XIConfigs();
                CResult Result = new CResult();
                XIDXI oXID = new XIDXI();
                if (SBOName == "XI Menu")
                {
                    if (Type == "create")
                    {
                        oMenu.ParentID = ParentNode;
                        if (ParentNode == "#")
                        {
                            oMenu.Name = RootNode;
                        }
                        else
                        {
                            oMenu.Name = NodeTitle;
                        }
                        oMenu.RootName = RootNode;
                        oMenu.OrgID = oUser.FKiOrganisationID;
                        oMenu.FKiApplicationID = oUser.FKiApplicationID;
                        oMenu.RoleID = oUser.RoleID.RoleID;
                        oMenu.StatusTypeID = 10;
                        //oMenu.XIDeleted = 0;
                        Result = Configs.Save_Menu(oMenu);

                    }
                    else if (Type == "rename")
                    {
                        oMenu.ID = Convert.ToInt32(NodeID);
                        var oCR = oXID.Get_MenuNodeDefinition(NodeID);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oMenu = ((List<XIMenu>)oCR.oResult).FirstOrDefault();
                            oMenu.Name = NodeTitle;
                            Result = Configs.Save_Menu(oMenu);
                        }
                        //oMenu.iOrgID = iOrgID;
                        //oMenu.XIDeleted = 0;

                    }
                    else if (Type == "delete")
                    {
                        oMenu.ID = Convert.ToInt32(NodeID);
                        var oCR = oXID.Get_MenuNodeDefinition(NodeID);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oMenu = ((List<XIMenu>)oCR.oResult).FirstOrDefault();
                            oMenu.XIDeleted = 1;
                            oMenu.StatusTypeID = 20;
                            Result = Configs.Save_Menu(oMenu);
                        }
                    }
                    else
                    {
                        oMenu.RootName = RootNode;
                        oMenu.ParentID = "#";
                        oMenu.Name = RootNode;
                        Configs.iOrgID = oUser.FKiOrganisationID;
                        Configs.iAppID = oUser.FKiApplicationID;
                        Result = Configs.Save_Menu(oMenu);
                    }
                }
                else if (SBOName == "XIInbox")
                {
                    XIDInbox oInbox = new XIDInbox();
                    if (Type == "create")
                    {

                        oInbox.OrganizationID = iOrgID;
                        oInbox.Name = NodeTitle;
                        //oInbox.ParentID = NodeID;
                        //oInbox.OrgID = oUser.FKiOrganisationID;
                        oInbox.FKiApplicationID = oUser.FKiApplicationID;
                        if (!NodeID.ToLower().StartsWith("org"))
                        {
                            oInbox.ParentID = ParentNode;
                        }
                        oInbox.XIDeleted = 0;
                        oInbox.StatusTypeID = 10;
                        Result = Configs.Save_InboxMenu(oInbox);
                    }
                    else if (Type == "rename")
                    {
                        var oCR = oXID.Get_InboxDefinition("", NodeID);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oInbox = ((List<XIDInbox>)oCR.oResult).FirstOrDefault();
                            // oInbox.ID = Convert.ToInt32(NodeID);
                            oInbox.Name = NodeTitle;
                            Result = Configs.Save_InboxMenu(oInbox);
                        }

                    }
                    else if (Type == "delete")
                    {
                        oInbox.ID = Convert.ToInt32(NodeID);
                        var oCR = oXID.Get_InboxDefinition("", NodeID);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oInbox = ((List<XIDInbox>)oCR.oResult).FirstOrDefault();
                            oInbox.XIDeleted = 1;
                            oInbox.StatusTypeID = 20;
                            Result = Configs.Save_InboxMenu(oInbox);
                        }
                    }
                }
                var oMenuDef = (XIIBO)Result.oResult;
                var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                return Json(sMenuID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetMenuTreeDetails(string RootName)
        {
            var sSessionID = HttpContext.Session.SessionID;
            XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            oUser = oUser ?? new XIInfraUsers(); oUser.UserID = iUserID;
            oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; try
            {
                List<RightMenuTrees> lMenuDetails = new List<RightMenuTrees>();
                lMenuDetails = dbContext.RightMenuTrees.Where(m => m.OrgID == oUser.FKiOrganisationID).Where(m => m.RootName == RootName).OrderBy(m => m.Priority).ToList();
                return Json(lMenuDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }
        public ActionResult DocumentsTree()
        {
            return View();
        }
        public ActionResult GetDirectory(string SearchText = null)
        {
            //List<string> Results = ProcessDirectory(null);
            //System.Web.UI.WebControls.TreeView treeView = new System.Web.UI.WebControls.TreeView();
            //treeView.PathSeparator = '\"';
            //ListDirectory(treeView, @"\\192.168.7.12\uploads\Files", SearchText);
            //PopulateTreeView(treeView, Results, '\\');
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string PhysicalPath = System.Configuration.ConfigurationManager.AppSettings["SharedPath"];
                //var sPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                //sPath = sPath + "\\UploadedFiles";
                List<string> fileArray = Directory.GetFiles(PhysicalPath + @"\Files", "*", SearchOption.AllDirectories).ToList();
                List<string> FilesList = new List<string>();
                if (!string.IsNullOrEmpty(SearchText))
                {
                    List<XIIBO> oReruirements = new List<XIIBO>();
                    QueryEngine oQE = new QueryEngine();
                    string sWhereCondition = "FKiPolicyID=" + SearchText + ",FKizXDoc!=null," + XIConstant.Key_XIDeleted + "=0";
                    var oQResult = oQE.Execute_QueryEngine("Requirement_T", "*", sWhereCondition);
                    if (oQResult.bOK && oQResult.oResult != null)
                    {
                        oReruirements = ((Dictionary<string, XIIBO>)oQResult.oResult).Values.ToList();
                    }
                    if (oReruirements != null)
                    {
                        foreach (var item in oReruirements.Where(x => x.AttributeI("FKizXDoc").sValue != "").Select(t => t.AttributeI("FKizXDoc").sValue).ToList())
                        {
                            if (item.Contains(','))
                            {
                                var docsList = item.Split(',').ToList();
                                foreach (var docID in docsList)
                                {
                                    FilesList.AddRange(fileArray.Where(x => x.Contains(docID)).ToList());
                                }
                            }
                            else
                            {
                                FilesList.AddRange(fileArray.Where(x => x.Contains(item)).ToList());
                            }
                        }
                    }
                    else
                    {
                        FilesList = new List<string>();// fileArray;
                    }
                }
                FilesList.Insert(0, PhysicalPath + @"\Files");
                //var jsonValue = JsonConvert.SerializeObject(treeView.Nodes[0]);
                return Json(FilesList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog("GetDirectory:" + ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult MapDocument(string sPath)
        {
            string res = string.Empty;
            string PhysicalPath = System.Configuration.ConfigurationManager.AppSettings["SharedPath"];
            var Path = sPath.Substring((PhysicalPath + @"\Files").Length).Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            string sDatabase = SessionManager.CoreDatabase;
            XIIXI oIXI = new XIIXI();
            XIDXI oXIDXI = new XIDXI();
            XIIBO oRequirementBOI = new XIIBO();
            XIIBO oDocumentBOI = new XIIBO();
            List<CNV> oWhereNV = new List<CNV>();
            bool filter;
            int a;
            string myStr = Path[Path.Length - 1].Split('.')[0].Split('_').LastOrDefault();
            filter = int.TryParse(myStr, out a);
            if (filter == false)
            {
                var list = Path[Path.Length - 1].Split('.')[0].Split('_');
                myStr = list[list.Length - 2];
            }
            string Query = "select * from Requirement_T where FKizXDoc like '%" + myStr + "%'";
            XID1Click oXID1Click = new XID1Click();
            oXID1Click.Query = Query;
            oXID1Click.Name = "Requirement_T";
            oRequirementBOI = (XIIBO)oXID1Click.OneClick_Execute().Values.FirstOrDefault();
            if (oRequirementBOI != null)
            {
                oDocumentBOI = oIXI.BOI("Documents_T", Path[Path.Length - 1].Split('.')[0].Split('_').LastOrDefault());
                if (oDocumentBOI == null)
                {
                    string Extension = (Path[Path.Length - 1].Split('.')[1]).ToString();
                    List<XIDocTypes> DocTypes = dbContext.XIDocTypes.Where(m => m.Type == Extension).ToList();
                    int iDocTypeID = DocTypes.Select(x => x.ID).First();
                    XIIBO oBOI = new XIIBO();
                    XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Documents_T", null);
                    oBOI.BOD = oBOD;
                    var items = Path.ToList();
                    items.RemoveAt(items.Count - 1);
                    items.RemoveAt(0);
                    string sNewPathForSubDir = string.Join("/", items);
                    oBOI.Attributes["FileName"] = new XIIAttribute { sName = "FileName", sValue = Path[Path.Length - 1], bDirty = true };
                    oBOI.Attributes["FKiDocType"] = new XIIAttribute { sName = "FKiDocType", sValue = iDocTypeID.ToString(), bDirty = true };
                    oBOI.Attributes["SubDirectoryPath"] = new XIIAttribute { sName = "SubDirectoryPath", sValue = sNewPathForSubDir, bDirty = true };
                    oBOI.Attributes["sFullPath"] = new XIIAttribute { sName = "sFullPath", sValue = sNewPathForSubDir + "\\" + Path[Path.Length - 1], bDirty = true };
                    oBOI.Attributes["iSystemgeneratedorUploaded"] = new XIIAttribute { sName = "iSystemgeneratedorUploaded", sValue = "20", bDirty = true };
                    oBOI.Attributes["sAliasName"] = new XIIAttribute { sName = "sAliasName", sValue = oRequirementBOI.AttributeI("sName").sValue + "." + Extension, bDirty = true };
                    oBOI.Attributes["sName"] = new XIIAttribute { sName = "sName", sValue = Path[Path.Length - 1], bDirty = true };
                    oBOI.Attributes["FKiPolicyVersionID"] = new XIIAttribute { sName = "FKiPolicyVersionID", sValue = oRequirementBOI.AttributeI("FKiPolicyVersionID").sValue, bDirty = true };
                    oBOI.Attributes["bIsVisibleToUser"] = new XIIAttribute { sName = "bIsVisibleToUser", sValue = "1", bDirty = true };
                    oBOI.Attributes["FKiUserID"] = new XIIAttribute { sName = "FKiUserID", sValue = oRequirementBOI.AttributeI("FKiUserID").sValue, bDirty = true };
                    oBOI.Attributes["iInstanceID"] = new XIIAttribute { sName = "iInstanceID", sValue = oRequirementBOI.AttributeI("FKiPolicyID").sValue, bDirty = true };
                    var Result = oBOI.Save(oBOI);
                    if (Result.bOK == true)
                    {
                        var DocIDs = oRequirementBOI.AttributeI("FKizXDoc").sValue;
                        DocIDs = DocIDs.Replace(myStr, ((XIIBO)Result.oResult).AttributeI("id").sValue);
                        oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Requirement_T", null);
                        oRequirementBOI.BOD = oBOD;
                        oRequirementBOI.SetAttribute("FKizXDoc", DocIDs);
                        Result = oRequirementBOI.Save(oRequirementBOI);
                    }
                }
                else
                {
                    res = "This Document already exists.";
                }
            }
            else
            {
                res = "This Requirement doesn't exists.";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeFields(string sShowField, string sHideField, string BOID)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, BOID);
            List<Dictionary<string, XIDAttribute>> oRes = new List<Dictionary<string, XIDAttribute>>();
            Dictionary<string, XIDAttribute> Attributes = new Dictionary<string, XIDAttribute>();
            //hidden fields
            if (!string.IsNullOrEmpty(sHideField))
            {
                var HiddenGroupFields = oBOD.Groups.Where(x => x.Key == sHideField.ToLower()).Select(t => t.Value).FirstOrDefault();
                var sItems = HiddenGroupFields.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in sItems)
                {
                    var sValue = oBOD.Attributes.Where(x => x.Key == item.ToLower()).Select(t => t).First();
                    Attributes.Add(sValue.Key, sValue.Value);
                }
                oRes.Add(Attributes);
            }
            //show fields
            if (!string.IsNullOrEmpty(sShowField))
            {
                Attributes = new Dictionary<string, XIDAttribute>();
                var ShowGroupFields = oBOD.Groups.Where(x => x.Key == sShowField.ToLower()).Select(t => t.Value).FirstOrDefault();
                if (ShowGroupFields == null)
                {
                    oRes.Add(Attributes);
                }
                else
                {
                    var Items = ShowGroupFields.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var item in Items)
                    {
                        var sValue = oBOD.Attributes.Where(x => x.Key == item.ToLower()).Select(t => t).First();
                        Attributes.Add(sValue.Key, sValue.Value);
                    }
                    oRes.Add(Attributes);
                }
            }
            //hidden fields
            if (!string.IsNullOrEmpty(sHideField))
            {
                var HiddenGroupFields = oBOD.Groups.Where(x => x.Key == sHideField.ToLower()).Select(t => t.Value).FirstOrDefault();
                Attributes = new Dictionary<string, XIDAttribute>();
                var sItems = HiddenGroupFields.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var item in sItems)
                {
                    var sValue = oBOD.Attributes.Where(x => x.Key == item.ToLower()).Select(t => t).First();
                    Attributes.Add(sValue.Key, sValue.Value);
                }
                oRes.Add(Attributes);
            }
            return Json(oRes, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDependentFields(string i1ClickID, string sGUID, List<CNV> oNVParams = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            string sSessionID = HttpContext.Session.SessionID;
            XIInfraCache oCache = new XIInfraCache();
            XID1Click o1Click = new XID1Click();
            o1Click = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, o1Click.BOID.ToString());

            var oCopy = (XID1Click)o1Click.Clone(o1Click);
            oCopy.ReplaceFKExpressions(oNVParams);
            oCopy.Resolve1Click();
            if (oCopy.Query.Contains("{") || oCopy.Query.Contains("|"))
            {
                Common.SaveErrorLog("Error: Unresolved Query in GetOneClickResult Method: " + oCopy.Query, sDatabase);
            }
            oCopy.OneClick_Execute();
            foreach (var item in oCopy.oDataSet.Values.ToList())
            {
                if (!string.IsNullOrEmpty(item.AttributeI("ioneclickid").sValue))
                {
                    o1Click = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, item.AttributeI("ioneclickid").sValue);
                    //var oLIst = oIXI.BOI(o1Click.sBOName, oNVParams.Where(x => x.sName == "-iInstanceID").Select(t => t.sValue).FirstOrDefault());
                    CNV oParam = new CNV();
                    oParam.sName = "{XIP|AlgorithmID}";
                    oParam.sValue = oNVParams.Where(x => x.sName == "-iInstanceID").Select(t => t.sValue).FirstOrDefault();
                    oNVParams.Add(oParam);
                    XID1Click oCopyQuery = (XID1Click)o1Click.Clone(o1Click);
                    oCopyQuery.ReplaceFKExpressions(oNVParams);
                    var FKBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1Click.BOID.ToString());
                    XIDXI oXIDXI = new XIDXI();
                    var sBODataSource = oXIDXI.GetBODataSource(FKBOD.iDataSource, oBOD.FKiApplicationID);
                    var Con = new XIDBAPI(sBODataSource);
                    Dictionary<string, string> DDL = Con.GetDDLItems(CommandType.Text, oCopyQuery.Query, null);
                    var FKDDL = DDL.Select(n => new XIDropDown { text = n.Value, Expression = n.Key }).ToList();
                    item.AttributeI("sname").FieldDDL = FKDDL;
                }
            }
            var sValues = oCopy.oDataSet.Values.Select(t => t.Attributes).ToList();
            var Attributes = new List<XIIAttribute>();
            List<string> sOptions = new List<string>();
            foreach (var item in sValues)
            {
                if (item.ElementAt(0).Key == "sname")
                {
                    item.ElementAt(0).Value.Format = item.Where(x => x.Key == "itype").Select(t => t.Value.sValue).FirstOrDefault();
                    item.ElementAt(0).Value.sDisplayName = item.Where(x => x.Key == "svalue").Select(t => t.Value.sValue).FirstOrDefault();
                    item.ElementAt(0).Value.oParent = item.Where(x => x.Key == "smapfield").Select(t => t.Value.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(item.Where(x => x.Key == "soptions").Select(t => t.Value.sValue).FirstOrDefault()))
                    {
                        List<XIDropDown> oListXIDropDown = new List<XIDropDown>();
                        XIDropDown oXIDropDown = new XIDropDown();
                        sOptions = item.Where(x => x.Key == "soptions").Select(t => t.Value.sValue).FirstOrDefault().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (var sOption in sOptions)
                        {
                            oXIDropDown.text = sOption;
                            oListXIDropDown.Add(oXIDropDown);
                            oXIDropDown = new XIDropDown();
                        }
                        item.ElementAt(0).Value.FieldDDL.AddRange(oListXIDropDown);
                    }
                }
                Attributes.Add((XIIAttribute)item.Where(x => x.Key == "sname").Select(t => t.Value).FirstOrDefault());
            }
            Attributes.ForEach(m => m.BOI = null);
            return Json(Attributes, JsonRequestBehavior.AllowGet);
        }
        private void ListDirectory(System.Web.UI.WebControls.TreeView treeView, string path, string SearchText = null)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo, SearchText));
        }

        private static System.Web.UI.WebControls.TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo, string SearchText = null)
        {
            var directoryNode = new System.Web.UI.WebControls.TreeNode(directoryInfo.Name);
            if (string.IsNullOrEmpty(SearchText))
            {
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    //if (directory.FullName.Length >= 59)
                    //{
                    //directoryNode.ChildNodes.Add(CreateDirectoryNode(directory));
                    CreateDirectoryNode(directory);
                    //}
                }
            }
            else
            {
                foreach (var directory in directoryInfo.GetDirectories(SearchText))
                {
                    //if (directory.FullName.Length >= 59)
                    //{
                    //directoryNode.ChildNodes.Add(CreateDirectoryNode(directory));
                    CreateDirectoryNode(directory);
                    //}
                }
            }
            if (string.IsNullOrEmpty(SearchText))
            {
                foreach (var file in directoryInfo.GetFiles())
                {
                    //if (file.FullName.Length >= 59)
                    //{
                    directoryNode.ChildNodes.Add(new System.Web.UI.WebControls.TreeNode(file.Name));
                    //}
                }
            }
            else
            {
                foreach (var file in directoryInfo.GetFiles(SearchText))
                {
                    //if (file.FullName.Length >= 59)
                    //{
                    directoryNode.ChildNodes.Add(new System.Web.UI.WebControls.TreeNode(file.Name));
                    //}
                }
            }
            return directoryNode;
        }

        public void Test()
        {
            XIInfraCache oCache = new XIInfraCache();
            var oContentDef = (List<XIContentEditors>)oCache.GetObjectFromCache(XIConstant.CacheTemplate, "Tests", "0");
            XIContentEditors oDocumentContent = new XIContentEditors();
            oDocumentContent = oContentDef.FirstOrDefault();
            XIContentEditors oConent = new XIContentEditors();
            var oLIst = oIXI.BOI("ACPolicy_T", "1523");
            var oInstance = oLIst.Structure("New Policy").XILoad();
            var oRes = oConent.MergeContentTemplate(oDocumentContent, oInstance);
        }
        public ActionResult Get_XILinks()
        {
            Dictionary<string, string> XiLinks = new Dictionary<string, string>();
            int iApplicationID = SessionManager.ApplicationID;
            XIDXI oXID = new XIDXI();
            var oCR = oXID.Get_XILinks(iApplicationID);
            if (oCR.bOK && oCR.oResult != null)
            {
                XiLinks = (Dictionary<string, string>)oCR.oResult;
            }
            return Json(XiLinks, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult LoginUser(string UserName, string Password, string sGUID, string sType, List<CNV> oParams)
        {
            List<XIIBO> oBOIList = new List<XIIBO>();
            //string str = Singleton.Instance.oParentGUID[sGUID];
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIIBO Response = new XIIBO();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var iDataSourceID = 0;
                var iApplicationID = SessionManager.ApplicationID;
                var iOrgID = SessionManager.OrganizationID;
                CResult oResponse = new CResult();
                string sSessionID = HttpContext.Session.SessionID;
                XIInfraUsers oUserD = new XIInfraUsers();
                var IsTwoWay = oParams.Where(m => m.sName == "IsTwoWay").Select(m => m.sValue).FirstOrDefault();
                oUserD.sUserName = UserName;
                var UserDetails = oUserD.Get_UserDetails(sDatabase);
                XIInfraEncryption xifEncrypt = new XIInfraEncryption();
                if (UserDetails.xiStatus == 0 && UserDetails.oResult != null)
                {
                    oUserD = (XIInfraUsers)UserDetails.oResult;
                    //var Password = oBOI.Attributes["sPasswordHash"].sValue;
                    var DecryptedPwd = xifEncrypt.DecryptData(oUserD.sPasswordHash, true, oUserD.UserID.ToString());
                    if (Password == DecryptedPwd)
                    {
                        //Success
                        if (!string.IsNullOrEmpty(IsTwoWay) && (IsTwoWay.ToLower() == "yes" || IsTwoWay.ToLower() == "true"))
                        {
                            XIIBO oBO = new XIIBO();
                            foreach (var param in oParams)
                            {
                                oBO.SetAttribute(param.sName, param.sValue);
                            }
                            Accountcontroller.GenerateOTPToUser(oUserD, iApplicationID, oBO);
                            oUserD.Update_User(sDatabase);
                            oCache.Set_ParamVal(sSessionID, sGUID, null, "NextStep", "Confirm Email", null, null);
                            oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|sLoginType}", "Authentication", null, null);
                            if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "login")
                            {
                                //var sLayoutName = SessionManager.LayoutName;
                                //XIDLayout oLayout = new XIDLayout();
                                //var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, sLayoutName, null); //oDXI.Get_LayoutDefinition(sLayoutName, "0");
                                //if (oLayDef != null)
                                //{
                                //    oLayout = (XIDLayout)oLayDef;
                                //    oLayout.sGUID = Guid.NewGuid().ToString();
                                //}
                                //oResponse.oResult = oLayout;
                                oResponse.oResult = "Success";
                                //return View("~/Views/QuestionSet/Login.cshtml", oLayout); /*RedirectToAction("StartUP", "QuestionSet");*/
                            }
                        }
                        else
                        {
                            Singleton.Instance.sCoreDatabase = oUserD.sCoreDatabaseName;
                            Singleton.Instance.sUserID = oUserD.UserID.ToString();
                            Singleton.Instance.sOrgID = oUserD.FKiOrganisationID.ToString();
                            Singleton.Instance.sAppName = oUserD.sAppName;
                            SessionManager.sUserName = oUserD.sUserName;
                            SessionManager.sEmail = oUserD.sEmail;
                            SessionManager.sRoleName = oUserD.Role.sRoleName;
                            SessionManager.OrgDatabase = oUserD.sDatabaseName;
                            SessionManager.UserID = oUserD.UserID;
                            SessionManager.OrganisationName = "Org";
                            SessionManager.UserUniqueID = null;
                            SessionManager.OrganizationID = oUserD.FKiOrganisationID;
                            SessionManager.sName = oUserD.sFirstName + " " + oUserD.sLastName;
                            SessionManager.iRoleID = oUserD.RoleID.RoleID;
                            SessionManager.sCustomerRefNo = oUserD.sCustomerRefNo;

                            FormsAuthentication.SetAuthCookie(oUser.sUserName, false);
                            var authTicket = new FormsAuthenticationTicket(1, oUserD.sUserName, DateTime.Now, DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), false, "Admin", FormsAuthentication.FormsCookiePath);
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                            {
                                HttpOnly = true,
                                Secure = FormsAuthentication.RequireSSL,
                                Path = FormsAuthentication.FormsCookiePath,
                                Domain = FormsAuthentication.CookieDomain,
                                Expires = DateTime.Now.AddDays(1)
                            };
                            HttpContext.Response.Cookies.Add(authCookie);

                            // HERE SETTING ROLENAME AND USERNAME INTO CACHE.
                            XIInfraCache oUserCache = new XIInfraCache();
                            var oCacheduser = new VM_UserLoginCache { sUserName = oUserD.sUserName, sRole = oUserD.Role.sRoleName };
                            oUserCache.InsertIntoCache(oCacheduser, "SSS_" + sSessionID);
                            if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "login")
                            {
                                oResponse.oResult = "Login";
                                //return RedirectToAction("LandingPages", "Home");
                            }
                            else
                            {
                                oResponse.oResult = "Success";
                            }
                        }
                        oResponse.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;

                    }
                    else
                    {
                        //Failure
                        oResponse.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                        oResponse.oResult = "Invalid login credentials";
                    }
                }
                // int iUserID = 0;

                //var Responsea = oXiAPI.SaveFormData(Attributes, sGUID, sContext, sDatabase, iUserID, sOrgName);
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetApplicationLayout()
        {
            string sDatabase = SessionManager.CoreDatabase;
            CResult oResponse = new CResult();
            string sSessionID = HttpContext.Session.SessionID;
            try
            {
                var sLayoutName = SessionManager.LayoutName;
                XIDLayout oLayout = new XIDLayout();
                var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, sLayoutName, null); //oDXI.Get_LayoutDefinition(sLayoutName, "0");
                if (oLayDef != null)
                {
                    oLayout = (XIDLayout)oLayDef;
                    oLayout.sGUID = Guid.NewGuid().ToString();
                }
                oResponse.oResult = oLayout;
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveUser(string UserName, string Password, string sGUID, string sType = "", string sAccountType = "")
        {
            List<XIIBO> oBOIList = new List<XIIBO>();
            //string str = Singleton.Instance.oParentGUID[sGUID];
            string sDatabase = SessionManager.CoreDatabase;
            string sOrgDatabase = SessionManager.OrgDatabase;
            try
            {
                XIIBO Response = new XIIBO();
                int iUserID = 0;
                if (SessionManager.UserID > 0)
                {
                    iUserID = SessionManager.UserID;
                }
                var iDataSourceID = 0;
                var iOrgID = SessionManager.OrganizationID;
                var iApplicationID = SessionManager.ApplicationID;
                CResult oResponse = new CResult();
                string sSessionID = HttpContext.Session.SessionID;
                XIInfraUsers oUserD = new XIInfraUsers();
                string FirstName = oCache.Get_ParamVal(sSessionID, sGUID, "", "{XIP|sFirstName}");
                string LastName = oCache.Get_ParamVal(sSessionID, sGUID, "", "{XIP|sLastName}");
                string Phone = oCache.Get_ParamVal(sSessionID, sGUID, "", "{XIP|sPhone}");
                oUserD.sUserName = UserName;
                oUserD.sEmail = UserName;
                oUserD.sFirstName = FirstName;
                oUserD.sLastName = LastName;
                oUserD.FKiOrganisationID = iOrgID;
                oUserD.FKiApplicationID = iApplicationID;
                XIInfraUsers oUser = new XIInfraUsers();
                oUser.FKiOrganisationID = iOrgID;
                oUser.sUserName = UserName;
                var oUSerI = oUser.Get_UserDetails(sDatabase);
                string sDataBaseName = string.Empty;
                if (oUSerI.xiStatus == 0 && oUSerI.oResult != null)
                {
                    var oUserI = (XIInfraUsers)oUSerI.oResult;
                    sDataBaseName = oUserI.sDatabaseName;
                }
                oUserD.sDatabaseName = sOrgDatabase;
                oUserD.sCoreDatabaseName = sDatabase;
                oUserD.sPhoneNumber = Phone;
                oUserD.iReportTo = 0;
                oUserD.LockoutEndDateUtc = DateTime.Now;
                oUserD.sLocation = "";
                oUserD.iPaginationCount = 10;
                oUserD.sMenu = "Open,Open";
                oUserD.iInboxRefreshTime = 0;
                oUserD.CreatedTime = DateTime.Now;
                oUserD.UpdatedTime = DateTime.Now;
                XIInfraEncryption xifEncrypt = new XIInfraEncryption();
                var oUserData = oUserD.Save_User(sDatabase);
                if (oUserData.xiStatus == 0 && oUserData.oResult != null)
                {
                    oUserD = (XIInfraUsers)oUserData.oResult;
                    //bool IsSendMail = true;
                    string sContext = XIConstant.Email_Welcome_Online;
                    //if (string.IsNullOrEmpty(Password))
                    //{
                    //    sContext = XIConstant.Email_Welcome_Internal;
                    //    Password = RandomNumber(8);
                    //    //IsSendMail = false;
                    //}
                    //string sTemporaryPWD = RandomString(8);
                    var EncryptedPwd = xifEncrypt.EncryptData(Password, true, oUserD.UserID.ToString());
                    oUserD.sPasswordHash = EncryptedPwd;
                    oUserD.sTemporaryPasswordHash = EncryptedPwd;
                    oUserData = oUserD.Update_User(sDatabase);
                    if (oUserData.xiStatus == 0 && oUserData.oResult != null)
                    {
                        oUserD = (XIInfraUsers)oUserData.oResult;
                        if (oUserD.UserID > 0)
                        {
                            XIInfraRoles xifRole = new XIInfraRoles();
                            xifRole.sRoleName = "WebUsers";
                            var oUserRoleData = xifRole.Get_RoleDefinition(sDatabase);
                            int RoleID = 0;
                            if (oUserRoleData.xiStatus == 0 && oUserRoleData.oResult != null)
                            {
                                var Role = (XIInfraRoles)oUserRoleData.oResult;
                                RoleID = Role.RoleID;
                                XIInfraUserRoles xifUserRole = new XIInfraUserRoles();
                                xifUserRole.UserID = oUserD.UserID;
                                xifUserRole.RoleID = RoleID;
                                xifUserRole.Save_UserRole(sDatabase);
                            }
                            XIInfraActors xifActor = new XIInfraActors();
                            xifActor.sName = "Web user";
                            var oUserActorData = xifActor.Get_ActorDefinition(sDatabase);
                            if (oUserActorData.xiStatus == 0 && oUserActorData.oResult != null)
                            {
                                var Actor = (XIInfraActors)oUserActorData.oResult;
                                int ActorID = Actor.ID;
                                XIInfraActorsMapping XifActorMapping = new XIInfraActorsMapping();
                                XifActorMapping.FKiUserID = oUserD.UserID;
                                XifActorMapping.FKiActorID = ActorID;
                                XifActorMapping.iInstanceID = oUserD.UserID;
                                XifActorMapping.Save_UserActor(sDatabase);
                            }
                            Singleton.Instance.sCoreDatabase = oUserD.sCoreDatabaseName;
                            Singleton.Instance.sUserID = oUserD.UserID.ToString();
                            Singleton.Instance.sOrgID = oUserD.FKiOrganisationID.ToString();
                            Singleton.Instance.sAppName = oUserD.sAppName;
                            SessionManager.sUserName = oUserD.sUserName;
                            SessionManager.sEmail = oUserD.sEmail;
                            SessionManager.sRoleName = "Web user";
                            SessionManager.OrgDatabase = oUserD.sDatabaseName;
                            SessionManager.UserID = oUserD.UserID;
                            SessionManager.OrganisationName = "Org";
                            SessionManager.UserUniqueID = null;
                            SessionManager.OrganizationID = oUserD.FKiOrganisationID;
                            SessionManager.sName = oUserD.sFirstName + " " + oUserD.sLastName;
                            SessionManager.iRoleID = RoleID;
                            SessionManager.sCustomerRefNo = oUserD.sCustomerRefNo;
                            FormsAuthentication.SetAuthCookie(oUserD.sUserName, false);
                            var authTicket = new FormsAuthenticationTicket(1, oUserD.sUserName, DateTime.Now, DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), false, "Admin", FormsAuthentication.FormsCookiePath);
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                            {
                                HttpOnly = true,
                                Secure = FormsAuthentication.RequireSSL,
                                Path = FormsAuthentication.FormsCookiePath,
                                Domain = FormsAuthentication.CookieDomain,
                                Expires = DateTime.Now.AddDays(1)
                            };
                            HttpContext.Response.Cookies.Add(authCookie);
                            // HERE SETTING ROLENAME AND USERNAME INTO CACHE.
                            XIInfraCache oUserCache = new XIInfraCache();
                            var oCacheduser = new VM_UserLoginCache { sUserName = oUserD.sUserName, sRole = "Web user" };
                            oUserCache.InsertIntoCache(oCacheduser, "SSS_" + sSessionID);
                            oResponse.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            if (!string.IsNullOrEmpty(sAccountType) && sAccountType.ToLower() == "login")
                            {
                                oResponse.oResult = "Login";
                                //return RedirectToAction("LandingPages", "Home");
                            }
                            else
                            {
                                oResponse.oResult = "Success";
                            }
                        }
                    }
                }
                else
                {
                    //Failure
                    oResponse.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oResponse.oResult = "Failed";
                }
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }
        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckOTP(string UserName, string Password, string sGUID, string sType = "", string sAccountType = "")
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;

            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
            //if iTraceLevel>0 then 
            //oCResult.oTraceStack.Trace("Stage", "Begin query build",milliseconds)
            //oCResult.oTraceStack.Trace("Stage",sError)
            //end if

            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
                //oCResult.oTraceStack.Trace("Stage",sError)
            }
            int iInstanceID = 0;
            try
            {
                string sSessionID = HttpContext.Session.SessionID;
                string sDatabase = SessionManager.CoreDatabase;
                XIInfraUsers oUserD = new XIInfraUsers();
                XIInfraEncryption xifEncrypt = new XIInfraEncryption();
                oUserD.sUserName = UserName;
                var UserDetails = oUserD.Get_UserDetails(sDatabase);
                if (UserDetails.xiStatus == 0 && UserDetails.oResult != null)
                {
                    oUserD = (XIInfraUsers)UserDetails.oResult;
                    if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "authentication")
                    {
                        if (Password == oUserD.sOTP)
                        {
                            Singleton.Instance.sCoreDatabase = oUserD.sCoreDatabaseName;
                            Singleton.Instance.sUserID = oUserD.UserID.ToString();
                            Singleton.Instance.sOrgID = oUserD.FKiOrganisationID.ToString();
                            Singleton.Instance.sAppName = oUserD.sAppName;
                            SessionManager.sUserName = oUserD.sUserName;
                            SessionManager.sEmail = oUserD.sEmail;
                            SessionManager.sRoleName = oUserD.Role.sRoleName;
                            SessionManager.OrgDatabase = oUserD.sDatabaseName;
                            SessionManager.UserID = oUserD.UserID;
                            SessionManager.OrganisationName = "Org";
                            SessionManager.UserUniqueID = null;
                            SessionManager.OrganizationID = oUserD.FKiOrganisationID;
                            SessionManager.sName = oUserD.sFirstName + " " + oUserD.sLastName;
                            SessionManager.iRoleID = oUserD.RoleID.RoleID;
                            SessionManager.sCustomerRefNo = oUserD.sCustomerRefNo;

                            FormsAuthentication.SetAuthCookie(oUser.sUserName, false);
                            var authTicket = new FormsAuthenticationTicket(1, oUserD.sUserName, DateTime.Now, DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), false, "Admin", FormsAuthentication.FormsCookiePath);
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                            {
                                HttpOnly = true,
                                Secure = FormsAuthentication.RequireSSL,
                                Path = FormsAuthentication.FormsCookiePath,
                                Domain = FormsAuthentication.CookieDomain,
                                Expires = DateTime.Now.AddDays(1)
                            };
                            HttpContext.Response.Cookies.Add(authCookie);

                            // HERE SETTING ROLENAME AND USERNAME INTO CACHE.
                            XIInfraCache oUserCache = new XIInfraCache();
                            var oCacheduser = new VM_UserLoginCache { sUserName = oUserD.sUserName, sRole = oUserD.Role.sRoleName };
                            oUserCache.InsertIntoCache(oCacheduser, "SSS_" + sSessionID);
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult.oResult = "Success";
                            if (!string.IsNullOrEmpty(sAccountType) && sAccountType.ToLower() == "login")
                            {
                                oCResult.oResult = "Login";
                                //return RedirectToAction("LandingPages", "Home");
                            }
                        }
                        else
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                            oCResult.oResult = "Invalid OTP";
                        }
                    }
                    else
                    {
                        var DecryptedPwd = xifEncrypt.DecryptData(oUserD.sTemporaryPasswordHash, true, oUserD.UserID.ToString());
                        if (Password == DecryptedPwd)
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult.oResult = "Success";
                        }
                        else
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                            oCResult.oResult = "Invalid OTP";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Executing Tree Structure Component" });
                oCResult.sMessage = "ERROR: [QSInstanceID: " + iInstanceID + " " + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.oResult = "Failed";
                oCResult.LogToFile();
            }
            return Json(oCResult, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult ResetPassword(string UserName, string Password, string sGUID, string sType = "", string sAccountType = "")
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;

            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
            //if iTraceLevel>0 then 
            //oCResult.oTraceStack.Trace("Stage", "Begin query build",milliseconds)
            //oCResult.oTraceStack.Trace("Stage",sError)
            //end if

            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
                //oCResult.oTraceStack.Trace("Stage",sError)
            }
            int iInstanceID = 0;
            try
            {
                string sSessionID = HttpContext.Session.SessionID;
                string sDatabase = SessionManager.CoreDatabase;
                XIInfraUsers oUserD = new XIInfraUsers();
                XIInfraEncryption xifEncrypt = new XIInfraEncryption();
                oUserD.sUserName = UserName;
                var UserDetails = oUserD.Get_UserDetails(sDatabase);
                if (UserDetails.xiStatus == 0 && UserDetails.oResult != null)
                {
                    oUserD = (XIInfraUsers)UserDetails.oResult;
                    var EncryptedPwd = xifEncrypt.EncryptData(Password, true, oUserD.UserID.ToString());
                    oUserD.sPasswordHash = EncryptedPwd;
                    var oUserData = oUserD.Update_User(sDatabase);
                    Singleton.Instance.sCoreDatabase = oUserD.sCoreDatabaseName;
                    Singleton.Instance.sUserID = oUserD.UserID.ToString();
                    Singleton.Instance.sOrgID = oUserD.FKiOrganisationID.ToString();
                    Singleton.Instance.sAppName = oUserD.sAppName;
                    SessionManager.sUserName = oUserD.sUserName;
                    SessionManager.sEmail = oUserD.sEmail;
                    SessionManager.sRoleName = "Web user";
                    SessionManager.OrgDatabase = oUserD.sDatabaseName;
                    SessionManager.UserID = oUserD.UserID;
                    SessionManager.OrganisationName = "Org";
                    SessionManager.UserUniqueID = null;
                    SessionManager.OrganizationID = oUserD.FKiOrganisationID;
                    SessionManager.sName = oUserD.sFirstName + " " + oUserD.sLastName;
                    SessionManager.iRoleID = oUserD.RoleID.RoleID;
                    SessionManager.sCustomerRefNo = oUserD.sCustomerRefNo;
                    FormsAuthentication.SetAuthCookie(oUserD.sUserName, false);
                    var authTicket = new FormsAuthenticationTicket(1, oUserD.sUserName, DateTime.Now, DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), false, "Admin", FormsAuthentication.FormsCookiePath);
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                    {
                        HttpOnly = true,
                        Secure = FormsAuthentication.RequireSSL,
                        Path = FormsAuthentication.FormsCookiePath,
                        Domain = FormsAuthentication.CookieDomain,
                        Expires = DateTime.Now.AddDays(1)
                    };
                    HttpContext.Response.Cookies.Add(authCookie);
                    // HERE SETTING ROLENAME AND USERNAME INTO CACHE.
                    XIInfraCache oUserCache = new XIInfraCache();
                    var oCacheduser = new VM_UserLoginCache { sUserName = oUser.sUserName, sRole = "Web user" };
                    oUserCache.InsertIntoCache(oCacheduser, "SSS_" + sSessionID);
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = "Success";
                    if (!string.IsNullOrEmpty(sAccountType) && sAccountType.ToLower() == "login")
                    {
                        oCResult.oResult = "Login";
                    }
                }
                else
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oCResult.oResult = "Failed";
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Executing Tree Structure Component" });
                oCResult.sMessage = "ERROR: [QSInstanceID: " + iInstanceID + " " + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.oResult = "Failed";
                oCResult.LogToFile();
            }
            return Json(oCResult, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Check_LinkAccess(int iLinkID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Check 1Link access to allow execution ";//expalin about this method logic
            string sDatabase = SessionManager.CoreDatabase;
            bool bAllowed = true;
            try
            {
                var iOrgID = SessionManager.OrganizationID;
                var iAppID = SessionManager.ApplicationID;
                var iRoleID = SessionManager.iRoleID;
                oTrace.oParams.Add(new CNV { sName = "iLinkID", sValue = iLinkID.ToString() });
                if (iLinkID > 0)//check mandatory params are passed or not
                {
                    
                    string sKey = iLinkID + "_" + iRoleID + "_" + iOrgID + "_" + iAppID;
                    XIIBO oLinkI = new XIIBO();
                    XIInfraCache oCache = new XIInfraCache();
                    var oLinkAccess = (Dictionary<string, object>)oCache.GetObjectFromCache(XIConstant.CacheLinkAccess, "CacheLinkAccess");
                    if (oLinkAccess.ContainsKey(sKey))
                    {
                        oLinkI = (XIIBO)oLinkAccess[sKey];
                        if (oLinkI != null && oLinkI.Attributes.Count() > 0)
                        {

                        }
                        else
                        {
                            bAllowed = false;
                        }
                    }
                    else if (oLinkAccess.Keys.ToList().Where(m => m.StartsWith(iLinkID.ToString())).FirstOrDefault() != null)
                    {
                        bAllowed = false;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iLinkID:" + iLinkID + " is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                //SaveErrortoDB(oCResult);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return Json(bAllowed, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create(RegisterViewModel userViewModel)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIInfraUsers xifUser = new XIInfraUsers();
                XIInfraEncryption xifEncrypt = new XIInfraEncryption();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                userViewModel.UserName = userViewModel.Email;
                int iOrgID = oUser.FKiOrganisationID;
                List<RegisterViewModel> OrgLocation = new List<RegisterViewModel>();
                string LocIDs = "";
                if (userViewModel.Locs != null)
                {
                    foreach (var user in userViewModel.Locs)
                    {
                        LocIDs = LocIDs + user + ",";
                    }
                    LocIDs = LocIDs.Substring(0, LocIDs.Length - 1);
                }
                else
                {
                    userViewModel.Locs = new List<string>();
                    LocIDs = "0";
                }
                
                if (userViewModel.GroupID == null)
                {
                    return Json(new { Response = false, Error = "Please select role from left tree" });
                }

                XIDXI oDXI = new XIDXI();
                oDXI.sOrgDatabase = sDatabase;
                var xiOrgData = oDXI.Get_OrgDefinition(null, iOrgID.ToString());
                var noofusers = (int)oUser.Get_NoOfUsers(sDatabase).oResult;// UsersRepository.GetNoOfUsers(iOrgID, sDatabase);
                XIDOrganisation xiOrg = (XIDOrganisation)xiOrgData.oResult;
                if (noofusers < xiOrg.NoOfUsers)
                {
                    if (ModelState.IsValid)
                    {
                        //DateTime dt = DateTime.ParseExact(System.DateTime.Now.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        // dt = dt.Date.AddMonths(6);
                        //ModelDbContext dbContext = new ModelDbContext();
                        //ModelDbContext dbCore = new ModelDbContext(sDatabase);
                        //cXIAppUsers oUser = new cXIAppUsers();
                        xifUser.sUserName = userViewModel.Email;
                        xifUser.sEmail = userViewModel.Email;
                        xifUser.sFirstName = userViewModel.FirstName;
                        xifUser.sLastName = userViewModel.LastName;
                        xifUser.FKiOrganisationID = iOrgID;
                        xifUser.FKiApplicationID = oUser.FKiApplicationID;
                        //oUser.sDatabaseName = sDatabase;
                        xifUser.sDatabaseName = oUser.sDatabaseName; //dbCore.XIAppUsers.Where(m => m.FKiOrganisationID == iOrgID).Select(m => m.sDatabaseName).FirstOrDefault();
                        xifUser.sCoreDatabaseName = sDatabase;
                        xifUser.sPhoneNumber = userViewModel.PhoneNumber;
                        xifUser.iReportTo = userViewModel.ReportTo;
                        xifUser.LockoutEndDateUtc = DateTime.Now;
                        xifUser.sLocation = LocIDs;
                        xifUser.iPaginationCount = 10;
                        xifUser.sMenu = "Open,Open";
                        xifUser.iInboxRefreshTime = 0;
                        xifUser.CreatedBy = xifUser.UpdatedBy = iUserID;
                        xifUser.CreatedTime = DateTime.Now;
                        xifUser.CreatedBySYSID = xifUser.UpdatedBySYSID = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString();
                        xifUser.StatusTypeID = userViewModel.StatusTypeID;
                        xifUser.sHierarchy = xifUser.sHierarchy;
                        xifUser.sInsertDefaultCode = xifUser.sInsertDefaultCode;
                        xifUser.sUpdateHierarchy = xifUser.sUpdateHierarchy;
                        xifUser.sViewHierarchy = xifUser.sViewHierarchy;
                        xifUser.sDeleteHierarchy = xifUser.sDeleteHierarchy;
                        var oUserData = xifUser.Save_User(sDatabase);
                        if (oUserData.xiStatus == 0 && oUserData.oResult != null)
                        {
                            xifUser = (XIInfraUsers)oUserData.oResult;
                            var EncryptedPwd = xifEncrypt.EncryptData(userViewModel.Password, true, xifUser.UserID.ToString());
                            xifUser.sPasswordHash = EncryptedPwd;
                            oUserData = xifUser.Update_User(sDatabase);
                            if (oUserData.xiStatus == 0 && oUserData.oResult != null)
                            {
                                xifUser = (XIInfraUsers)oUserData.oResult;
                                if (xifUser.UserID > 0)
                                {
                                    var Roles = userViewModel.GroupID.Split(',').ToList();
                                    foreach (var items in Roles)
                                    {
                                        XIInfraRoles xifRole = new XIInfraRoles();
                                        xifRole.RoleID = Convert.ToInt32(items);
                                        var RoleData = xifRole.Get_RoleDefinition(sDatabase);
                                        if (RoleData.xiStatus == 0 && RoleData.oResult != null)
                                        {
                                            xifRole = (XIInfraRoles)RoleData.oResult;
                                            XIInfraUserRoles xifURole = new XIInfraUserRoles();
                                            xifURole.UserID = xifUser.UserID;
                                            xifURole.RoleID = xifRole.RoleID;
                                            var URoleData = xifURole.Save_UserRole(sDatabase);
                                            if (URoleData.xiStatus == 0 && URoleData.oResult != null)
                                            {

                                            }
                                            else
                                            {
                                                return Json(URoleData, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else
                                        {
                                            return Json(RoleData, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return Json(oUserData, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(oUserData, JsonRequestBehavior.AllowGet);
                        }

                        return Json(new { Response = true });
                        //return RedirectToAction("Index");
                    }
                    else
                    {
                        var errors = ModelState.Select(x => x.Value.Errors)
                                     .Where(y => y.Count > 0)
                                     .ToList();
                        return Json(new { Response = false, Error = errors.First().First().ErrorMessage });
                    }

                }
                else
                {
                    return Json(new { Response = false, Error = "Users limit reached. Unable to create new user" });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new { Response = false, Error = "Error while creating employee!!!<br/> Please try again" });
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteDynamicTreeNode(int iInstanceID, int iParentID, string sBOName)
        {
            XIIXI oXIIXI = new XIIXI();
            var oBOI = oXIIXI.BOI(sBOName, iInstanceID.ToString());
            try
            {
                var Response = oBOI.Delete(oBOI);
                XID1Click oD1Click = new XID1Click();
                string sQuery = string.Empty;
                sQuery = "select * from " + sBOName + " WHERE iParentID =" + iInstanceID + " and izXDeleted = 0";
                oD1Click.Query = sQuery;
                oD1Click.Name = sBOName;
                var oResult = oD1Click.OneClick_Run(false);
                if (oResult.Count() > 0)
                {
                    XIDBO oXIDBO = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName);
                    foreach (var item in oResult.Values)
                    {
                        item.SetAttribute("iParentID", iParentID.ToString());
                        item.BOD = oXIDBO;
                        item.Save(item);
                    }
                }
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }
        public FileResult GetFileFromDisk(string sGUID = "", string sID = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            CResult oCResult = new CResult();
            XIInfraDocs oDocs = new XIInfraDocs();
            XIInfraDocTypes oXIDocTypes = new XIInfraDocTypes();
            XIIXI oXII = new XIIXI();
            string SharedPath = System.Configuration.ConfigurationManager.AppSettings["SharedPath"];
            var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
            //oCResult.sMessage = "Virtual Path: " + sVirtualDir;
            //SaveErrortoDB(oCResult);

            string PhysicalPath = HostingEnvironment.MapPath("~\\" + sVirtualDir + "\\"); // System.Web.Hosting.HostingEnvironment.MapPath("~");
            //oCResult.sMessage = "Virtual Path: " + sVirtualDir + " , physicalPath:"+ PhysicalPath;
            //Common.SaveErrorLog(oCResult.sMessage, sDatabase);
            string sDocPath = "";
            string fileName = "";
            XIAPI oXIAPI = new XIAPI();
            if (!string.IsNullOrEmpty(sGUID) && string.IsNullOrEmpty(sID))
            {
                List<XIWhereParams> oWHParams = new List<XIWhereParams>();
                oWHParams.Add(new XIWhereParams { sField = "xiguid", sOperator = "=", sValue = sGUID });
                sID = oXIAPI.GetValue("Documents_T", "ID", oWHParams);
            }
            // var oDocI=oXII.BOI("Documents_T", iID.ToString());
            //string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
            //if (oDocI.Attributes["FKiFileTypeID"].iValue == 1)
            //{
            //    int iDocID = 0;
            //    //int.TryParse(item, out iDocID);
            //    if (iDocID > 0)
            //    {
            //        oDocs.ID = iID;
            //        var sImagePathDetails = (List<XIDropDown>)oDocs.Get_FilePathDetails().oResult;
            //        //if (sImagePathDetails != null)
            //        //{
            //        //    ImagePathDetails.AddRange(sImagePathDetails);
            //        //}
            //    }
            //    else
            //    {
            //        oXIDocTypes.ID = oDocI.Attributes["FKiFileTypeID"].iValue;
            //        var oXIDocDetails = (XIInfraDocTypes)oXIDocTypes.Get_FileDocTypes().oResult;
            //        if (oXIDocDetails != null)
            //        {
            //            //int pos = item.LastIndexOf("\\") + 1;
            //            //string sFileName = item.Substring(pos, item.Length - pos);
            //            //oXIDocDetails.Path = oXIDocDetails.Path.Replace("~", "");
            //            //sPDFPathDetails.Add(new XIDropDown { Expression = oXIDocDetails.Path + "//" + item, text = sFileName });
            //            //ImagePathDetails.AddRange(sPDFPathDetails);
            //        }
            //    }
            //}
            //else
            //{
            int iDocID = 0;
            int.TryParse(sID, out iDocID);
            if (iDocID > 0)
            {
                oDocs.ID = iDocID;
                var sImagePathDetails = (List<XIDropDown>)oDocs.Get_FilePathDetails().oResult;
                if (sImagePathDetails != null && sImagePathDetails.Count > 0 && sImagePathDetails.FirstOrDefault() != null)
                {
                    sDocPath = sImagePathDetails.FirstOrDefault().Expression;
                    fileName = sImagePathDetails.FirstOrDefault().text;
                    //ImagePathDetails.AddRange(sImagePathDetails);
                }
            }
            var FilePath = PhysicalPath + sDocPath;
            FilePath = FilePath.Replace("/", "\\");
            byte[] FileBytes = System.IO.File.ReadAllBytes(FilePath);
            string mimeType = System.Web.MimeMapping.GetMimeMapping(fileName);
            //oCResult.sMessage = "File Name: " + fileName + " , mimeType:" + mimeType;
            //Common.SaveErrorLog(oCResult.sMessage, sDatabase);
            return File(FileBytes, mimeType);
            //else
            //{
            //    oXIDocTypes.ID = oDocI.Attributes["FKiFileTypeID"].iValue;
            //    var oXIDocDetails = (XIInfraDocTypes)oXIDocTypes.Get_FileDocTypes().oResult;
            //    if (oXIDocDetails != null)
            //    {
            //        //int pos = item.LastIndexOf("\\") + 1;
            //        //string sFileName = item.Substring(pos, item.Length - pos);
            //        //oXIDocDetails.Path = oXIDocDetails.Path.Replace("~", "");
            //        //sPDFPathDetails.Add(new XIDropDown { Expression = oXIDocDetails.Path + "//" + item, text = sFileName });
            //        //ImagePathDetails.AddRange(sPDFPathDetails);
            //    }
            //}
            //}

            //return File(FilePath, "application/octet-stream", fileName);
        }
    }
}

