using System.Web;
using System.Web.Mvc;
using XIDNA.Models;
using XIDNA.Repository;
using XIDNA.ViewModels;
using Microsoft.AspNet.Identity;
using XIDNA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XICore;
using XISystem;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using XIDNA.Hubs;

namespace XIDNA.Controllers
{
    [Authorize]
    [SessionTimeout]
    public class XIComponentsController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IXIComponentsRepository XIComponentsRepository;

        public XIComponentsController() : this(new XIComponentsRepository()) { }

        public XIComponentsController(XIComponentsRepository XIComponentsRepository)
        {
            this.XIComponentsRepository = XIComponentsRepository;
        }

        XIInfraUsers oUser = new XIInfraUsers();
        CommonRepository Common = new CommonRepository();
        XIInfraCache oCache = new XIInfraCache();
        ModelDbContext dbContext = new ModelDbContext();
        #region XIComponents

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult XiComponentsList(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var result = XIComponentsRepository.XiComponentsList(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddEditXiComponents(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                cXIComponents model = new cXIComponents();
                oUser.UserID = iUserID;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                var fkiApplicationID = oUser.FKiApplicationID;
                if (fkiApplicationID == 0)
                {
                    model.ddlApplications = Common.GetApplicationsDDL();
                }
                if (ID == 0)
                {
                    model.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiComponents", model);
                }
                else
                {
                    cXIComponents Result = XIComponentsRepository.GetXiComponentsByID(ID, null, null, 0, iUserID, sOrgName, sDatabase);
                    if (fkiApplicationID == 0)
                    {
                        Result.ddlApplications = Common.GetApplicationsDDL();
                    }
                    Result.FKiApplicationID = fkiApplicationID;
                    return PartialView("_AddEditXiComponents", Result);
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
        public ActionResult SaveEditXiComponents(int ID, string sName, string sType, string sClass, string sHTMLPage, int FKiApplicationID, string[] NVPairs, string[] Triggers, int Status)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMXIComponents Component = new VMXIComponents();
                Component.ID = ID;
                Component.FKiApplicationID = FKiApplicationID;
                Component.sName = sName;
                Component.sType = sType;
                Component.sClass = sClass;
                Component.sHTMLPage = sHTMLPage;
                Component.NVPairs = NVPairs;
                Component.TriggerPairs = Triggers;
                Component.StatusTypeID = Status;
                Component.CreatedBy = iUserID;
                Component.UpdatedBy = iUserID;
                var Result = XIComponentsRepository.SaveXiComponents(Component, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //IDE
        [HttpPost]
        public ActionResult Save_XIComponents(int ID, string sName, string sType, string sClass, string sHTMLPage, int FKiApplicationID, string[] NVPairs, string[] Triggers, int Status)
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
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Components");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", ID.ToString());
                oBOI.SetAttribute("sType", sType.ToString());
                oBOI.SetAttribute("sName", sName);
                oBOI.SetAttribute("sClass", sClass);
                oBOI.SetAttribute("sHTMLPage", sHTMLPage);
                oBOI.SetAttribute("StatusTypeID", Status.ToString());
                oBOI.SetAttribute("FKiApplicationID", FKiApplicationID.ToString());
                oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                if (ID == 0)
                {
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                }
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                if (ID > 0)
                {
                    dbContext.XIComponentsNVs.RemoveRange(dbContext.XIComponentsNVs.Where(m => m.FKiComponentID == ID));
                    dbContext.SaveChanges();
                }
                var XiComponents = oBOI.Save(oBOI);
                var XiComponentID = string.Empty;
                if (XiComponents.bOK && XiComponents.oResult != null)
                {
                    var oXL = (XIIBO)XiComponents.oResult;
                    XiComponentID = oXL.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                }

                if (NVPairs != null && NVPairs.Count() > 0)
                {
                    for (int i = 0; i < NVPairs.Count(); i++)
                    {
                        var Pairs = NVPairs[i].ToString().Split('_').ToList();
                        XIIBO oBOICom = new XIIBO();
                        XIDBO oBODCom = new XIDBO();
                        oBODCom = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIComponentNVs");
                        oBOICom.BOD = oBODCom;
                        oBOICom.SetAttribute("sName", Pairs[0].ToString());
                        oBOICom.SetAttribute("sValue", Pairs[1].ToString());
                        oBOICom.SetAttribute("sType", Pairs[2].ToString());
                        oBOICom.SetAttribute("FKiComponentID", XiComponentID.ToString());
                        oBOICom.SetAttribute("StatusTypeID", Status.ToString());
                        oBOICom.SetAttribute("CreatedBy", iUserID.ToString());
                        oBOICom.SetAttribute("CreatedTime", DateTime.Now.ToString());
                        oBOICom.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        oBOICom.SetAttribute("UpdatedBy", iUserID.ToString());
                        oBOICom.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                        oBOICom.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        var XiComponentNVs = oBOICom.SaveV2(oBOICom);
                    }
                }
                if (Triggers != null && Triggers.Count() > 0)
                {
                    for (int i = 0; i < Triggers.Count(); i++)
                    {
                        XIIBO oBOITri = new XIIBO();
                        XIDBO oBODTri = new XIDBO();
                        oBODTri = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIComponentTriggers");
                        oBOITri.BOD = oBODTri;
                        var Pairs = Triggers[i].ToString().Split('_').ToList();
                        oBOI.SetAttribute("sName", Pairs[0].ToString());
                        oBOI.SetAttribute("sValue", Pairs[1].ToString());
                        oBOI.SetAttribute("FKiComponentID", XiComponentID.ToString());
                        var XiComponentTriggers = oBOITri.Save(oBOITri);
                    }
                }
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

        public ActionResult GetXIComponentByID(int iXIComponentID, string sType, int ID, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                cXIComponents Result = XIComponentsRepository.GetXiComponentsByID(iXIComponentID, null, sType, ID, iUserID, sOrgName, sDatabase);
                ViewBag.IsValueSet = "True";
                return PartialView("_LoadComponentParams", Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult IDEGetXIComponentByID(int iXIComponentID, string sType, int ID, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                cXIComponents Result = XIComponentsRepository.GetXiComponentsByID(iXIComponentID, null, sType, ID, iUserID, sOrgName, sDatabase);
                ViewBag.IsValueSet = "True";
                if (Result.XIComponentParams != null && Result.XIComponentParams.Count() == 0)
                {
                    ViewBag.SectionID = 0;
                }
                else
                {
                    ViewBag.SectionID = Result.XIComponentParams.FirstOrDefault().iStepSectionID;
                }
                return PartialView("_IDELoadComponentParams", Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetXIComponentDetailsByID(int iXIComponentID, string sType)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                cXIComponents Result = XIComponentsRepository.GetXiComponentsByID(iXIComponentID, null, sType, 0, iUserID, sOrgName, sDatabase);
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
        public ActionResult SaveComponentParams(cXIComponents oComponent, string sType, int iLoadID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var Result = XIComponentsRepository.SaveLayoutComponentParams(oComponent, sType, iLoadID, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult UpdateMappingIDToParams(string sType, int iLoadID, string Params)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var Result = XIComponentsRepository.UpdateMappingIDToParams(sType, iLoadID, Params, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult XIInitialise(int iXIComponentID = 0, int MappingID = 0, string IsValueSet = "False")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return RedirectToAction("XIExecute", "XIComponents", new { iXIComponentID = iXIComponentID, MappingID = MappingID, IsValueSet = IsValueSet });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult XIExecute(int iXIComponentID, int MappingID, string IsValueSet)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                //iXIComponentID = 1;
                var oComponent = XIComponentsRepository.XIInitialise(iXIComponentID, iUserID, sOrgName, sDatabase);
                ViewBag.IsValueSet = IsValueSet;
                return PartialView("_XIComponent", oComponent);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult XIComponentExecute(cXIComponents oXIComponent)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                //Component = oXIComponent;
                var Class = oXIComponent.sClass.Split('/').ToList().Last();
                if (Class == "TreeViewComponent")
                {
                    var TreeNodes = XIComponentsRepository.GetTreeStructure(null, iUserID, sOrgName, sDatabase);
                    return PartialView("_ComponentTreeView", TreeNodes);
                }
                else if (Class == "FormComponent")
                {
                    var oBODisplay = XIComponentsRepository.GetFormComponent(oXIComponent, iUserID, sOrgName, sDatabase);
                    return PartialView("~/Views/XiLink/_InlineEdit.cshtml", oBODisplay);
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            //return RedirectToAction("TreeView", "XIComponents");
        }

        public ActionResult LoadXIComponent(int iXIComponentID, int iInstanceID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                cXIComponents oXIComponent = XIComponentsRepository.GetXiComponentsByID(iXIComponentID, null, null, 0, iUserID, sOrgName, sDatabase);
                if (oXIComponent.sClass == "TreeViewComponent")
                {
                    var TreeNodes = XIComponentsRepository.GetTreeStructure(null, iUserID, sOrgName, sDatabase);
                    return PartialView("_ComponentTreeView", TreeNodes);
                }
                else if (oXIComponent.sClass == "FormComponent")
                {
                    var oBODisplay = XIComponentsRepository.GetFormComponent(oXIComponent, iUserID, sOrgName, sDatabase);
                    return PartialView("~/Views/XiLink/_InlineEdit.cshtml", oBODisplay);
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

        [HttpPost]
        public ActionResult SetXIParams(List<CNV> oParams, string sGUID)
        {
            XIInfraCache oCache = new XIInfraCache();
            string sSessionID = HttpContext.Session.SessionID;
            foreach (var items in oParams)
            {
                if (!string.IsNullOrEmpty(items.sValue))
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, items.sContext, items.sName, items.sValue, items.sType, items.nSubParams);
                }
            }
            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateXIParams(string sGUID, string sCurrentGUID, List<CNV> nParams)
        {
            XIInfraCache oCache = new XIInfraCache();
            string sSessionID = HttpContext.Session.SessionID;
            if (!string.IsNullOrEmpty(sCurrentGUID) && sGUID != sCurrentGUID && sCurrentGUID != "0")
            {
                XICacheInstance sCurParams = oCache.GetAllParamsUnderGUID(sSessionID, sCurrentGUID, null);
                if (nParams == null)
                {
                    nParams = new List<CNV>();
                }
                //var oCacheInstance = oCache.Get_ParamVal(sSessionID, sGUID, "-listinstance", null);
                if (sCurParams.NMyInstance.Count() > 0)
                {
                    nParams.AddRange(sCurParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList());
                }
                if (sCurParams.Registers != null && sCurParams.Registers.Count() > 0)
                {
                    nParams.AddRange(sCurParams.Registers.Select(m => new CNV { sName = m.sName, sValue = m.sValue, sType = "register" }).ToList());
                }
            }
            if (nParams != null)
            {
                foreach (var items in nParams)
                {
                    if (items != null)
                    {
                        if (!string.IsNullOrEmpty(items.sName))
                        {
                            if (!string.IsNullOrEmpty(items.sValue))
                            {
                                if (items.sName.ToLower() == "iBOIID".ToLower())
                                {
                                    var ActiveBO = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}");
                                    if (!string.IsNullOrEmpty(ActiveBO))
                                    {
                                        oCache.Set_ParamVal(sSessionID, sGUID, items.sContext, "{XIP|" + ActiveBO + ".id}", items.sValue, items.sType, items.nSubParams);
                                    }
                                }
                                else
                                {
                                    oCache.Set_ParamVal(sSessionID, sGUID, items.sContext, items.sName, items.sValue, items.sType, items.nSubParams);
                                    var s1ClickID = items.nSubParams.Where(m => m.sName == "{XIP|i1ClickID}").Select(m => m.sValue).FirstOrDefault();
                                    if (s1ClickID != null)
                                    {
                                        oCache.Set_ParamVal(sSessionID, sGUID, items.sContext, XIConstant.XIP_1ClickID, s1ClickID, items.sType, items.nSubParams);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            XICacheInstance Cache = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
            if (nParams != null && nParams[0] != null && nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "iNodeStepID".ToLower()).FirstOrDefault() != null)
            {
                if (Cache.Registers != null && Cache.Registers.Where(m => m.sName == "iNodeStepID").FirstOrDefault() == null)
                {
                    var nodeType = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "iNodeStepID".ToLower()).Select(m => m.sType).FirstOrDefault();

                    if (nodeType != null && nodeType == "template")
                    {
                        var sNodeStep = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "iNodeStepID".ToLower()).FirstOrDefault();
                        var sOutputArea = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "sOutputArea".ToLower()).FirstOrDefault();
                        Cache.Registers.Add(new CNV { sName = "iNodeStepID", sValue = sOutputArea.sValue + "_" + sNodeStep.sValue, sType = "template" });
                    }
                }
                else
                {
                    if (Cache.NMyInstance.ContainsKey("-listinstance"))
                    {
                        var iListIns = Cache.NMyInstance["-listinstance"];
                        if (iListIns != null && iListIns.nSubParams != null)
                        {
                            var iInsID = iListIns.nSubParams.Where(m => m.sName == "{-iInstanceID}").FirstOrDefault();
                            if (iInsID != null)
                            {
                                iListIns.nSubParams.Where(m => m.sName == "{-iInstanceID}").FirstOrDefault().sValue = "0";
                            }
                        }
                    }

                    var sNodeStep = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "iNodeStepID".ToLower()).FirstOrDefault();
                    var sOutputArea = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "sOutputArea".ToLower()).FirstOrDefault();
                    if (Cache.Registers != null)
                    {
                        Cache.Registers.Where(m => m.sName == "iNodeStepID").FirstOrDefault().sValue = sOutputArea.sValue + "_" + sNodeStep.sValue;
                    }
                }
            }

            if (nParams != null && nParams.Count() > 0 && nParams[0] != null)
            {
                var App = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "{XIP|ActiveBO}".ToLower()).Select(m => m.sValue).FirstOrDefault();
                var AppID = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "iInstanceID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(App) && !string.IsNullOrEmpty(AppID) && App.ToLower() == "xi application")
                {
                    SessionManager.ApplicationID = Convert.ToInt32(AppID);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iAppID}", AppID, null, null);
                }
                //IDE
                var FKiVisualisationID = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "{XIP|FKiVisualisationID}".ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(FKiVisualisationID) && FKiVisualisationID != null)
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|FKiVisualisationID}", FKiVisualisationID, null, null);
                }
                //IDE
                var s1ClickID = nParams.FirstOrDefault().nSubParams.Where(m => m.sName == "{XIP|i1ClickID}").Select(m => m.sValue).FirstOrDefault();
                if (s1ClickID != null)
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, XIConstant.XIP_1ClickID, s1ClickID, null, null);
                }

                //IDE
                var sParentFKColumn = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "ParentFKColumn".ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (sParentFKColumn != null)
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "ParentFKColumn", sParentFKColumn, null, null);
                }
                //IDE
                var sParentInsID = nParams.FirstOrDefault().nSubParams.Where(m => m.sName.ToLower() == "ParentInsID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (sParentInsID != null)
                {
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "ParentInsID", sParentInsID, null, null);
                }
            }
            return Json(Cache, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCacheData(string sGUID)
        {
            string sSessionID = HttpContext.Session.SessionID;
            XICacheInstance Cache = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
            return Json(Cache, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [AllowAnonymous]
        public ActionResult LoadComponentByID(int iXIComponentID, string sGUID, List<CNV> nParams, string sName, string sType, int ID = 0, int iInstanceID = 0, string sContext = "", int iQSIID = 0, int BODID = 0, bool bIsStepLock = false)
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
                string sSessionID = HttpContext.Session.SessionID;
                XIInfraCache oCache = new XIInfraCache();
                var oParams = new List<CNV>();
                //var SessionItems = SessionManager.SessionItems;
                //oParams.AddRange(SessionItems);
                if (nParams != null && nParams.Count() > 0)
                {
                    oParams.AddRange(nParams);
                }
                var SessionItems = SessionManager.SessionItems();
                if (SessionItems != null && SessionItems.Count() > 0)
                {
                    oParams.AddRange(SessionItems);
                }
                //oParams.AddRange();
                XIDComponent oComponentD = new XIDComponent();
                oComponentD.ID = iXIComponentID;
                oComponentD.sGUID = sGUID;
                oComponentD.sName = sName;
                oComponentD.nParams = oParams;
                oComponentD.sContext = sContext;
                oComponentD.iQSIID = iQSIID;
                XIIComponent oCompI = new XIIComponent();

                var Response = oComponentD.LoadComponent(sType, ID, BODID, bIsStepLock);
                if (Response.bOK && Response.oResult != null)
                {
                    var MergeTemplate = "";
                    var oResult = Response.oResult;
                    if (iXIComponentID == 14)
                    {
                        var result = ((XID1Click)oResult).RepeaterResult;
                        if (result != null && result.Count > 0)
                        {
                            MergeTemplate = result[0];
                        }
                    }
                    var oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, sName, iXIComponentID.ToString());
                    //var XICompD = (XIDComponent)oXICom;
                    var Copy = (XIDComponent)oXIComponent.Clone(oXIComponent);
                    //var Copy = (XIDComponent)(oXIComponent.Clone(oXICom));  //(XIDComponent)oXIComponent.Clone(oXICom);
                    oXIComponent = oXIComponent.GetParamsByContext(Copy, sType, ID);
                    ViewBag.sGUID = sGUID;
                    ViewBag.Context = sContext;
                    ViewBag.iUserID = iUserID;
                    oCompI.sGUID = sGUID;
                    oCompI.oDefintion = oXIComponent;
                    List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                    XID1Click o1Click = new XID1Click();
                    string sVisualisation = oXIComponent.Params.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(sVisualisation))
                    {
                        var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, null);
                        var oXIVisualC = (XIVisualisation)o1Click.Clone(oXIvisual);
                        if (oXIVisualC != null)
                        {
                            foreach (var oVisualisation in oXIVisualC.XiVisualisationNVs)
                            {
                                if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("{XIP") >= 0 && !oVisualisation.sValue.StartsWith("xi.s"))
                                {
                                    oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                }
                                else if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("xi.s") >= 0)
                                {
                                    XIDScript oXIScript1 = new XIDScript();
                                    oXIScript1.sScript = oVisualisation.sValue;
                                    var oCR = oXIScript1.Execute_Script(sGUID, sSessionID);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        string sValue = (string)oCR.oResult;
                                        if (!string.IsNullOrEmpty(sValue))
                                        {
                                            oVisualisation.sValue = sValue;
                                        }
                                    }
                                }
                                if (oVisualisation.sName == "IsSaveContent" && oVisualisation.sValue == "yes")
                                {
                                    XIIBO oBOI = new XIIBO();
                                    XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "MergeQsSummary_T", null);
                                    oBOI.BOD = oBOD;
                                    oBOI.LoadBOI("create");
                                    oBOI.Attributes["scontent"].sValue = MergeTemplate;
                                    var oBOIResult = oBOI.Save(oBOI);
                                    if (oBOIResult.bOK && oBOIResult.oResult != null)
                                    {
                                        var oMergeBOI = (XIIBO)oBOIResult.oResult;
                                        if (oMergeBOI.Attributes != null && oMergeBOI.Attributes.ContainsKey(oBOD.sPrimaryKey.ToLower()))
                                        {
                                            oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|iMergeQSSummaryID}", oMergeBOI.Attributes[oBOD.sPrimaryKey.ToLower()].sValue, null, null);
                                        }
                                    }
                                }
                            }
                            oXIVisualisations.Add(oXIVisualC);
                        }
                    }
                    oCompI.oVisualisation = oXIVisualisations;
                    ViewBag.bIsStepLock = bIsStepLock;
                    //oCache.Set_ParamVal(sSessionID, sGUID, sContext, "{XIP|iUserID}", Convert.ToString(iUserID), null, null);
                    if (!string.IsNullOrEmpty(oXIComponent.sHTMLPage))
                    {
                        if (oXIComponent.sName == "Grid Component")
                        {
                            //List<XIBODisplay> oBODisplay = new List<XIBODisplay>();
                            //XIBODisplay oBOInstance = (XIBODisplay)oResult;
                            //oBODisplay.Add(oBOInstance);
                            if (nParams != null)
                            {
                                ViewBag.iCount = nParams.Where(x => x.sName.ToLower() == "iCount".ToLower()).Select(x => x.sValue).FirstOrDefault();
                            }
                            oCompI.oContent[XIConstant.GridComponent] = oResult;
                            return PartialView(oXIComponent.sHTMLPage, oCompI);
                        }
                        else
                        {
                            if (oXIComponent.sName == XIConstant.FormComponent)
                            {
                                oCompI.oContent[XIConstant.FormComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.OneClickComponent)
                            {
                                oCompI.oContent[XIConstant.OneClickComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.ListComponent)
                            {
                                oCompI.oContent[XIConstant.ListComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.HTMLComponent)
                            {
                                oCompI.oContent[XIConstant.HTMLComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.MenuComponent)
                            {
                                oCompI.oContent[XIConstant.MenuComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.ReportComponent)
                            {
                                oCompI.oContent[XIConstant.ReportComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.QuoteReportDataComponent)
                            {
                                oCompI.oContent[XIConstant.QuoteReportDataComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.XilinkComponent)
                            {
                                oCompI.oContent[XIConstant.XilinkComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.GroupComponent)
                            {
                                oCompI.oContent[XIConstant.GroupComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.PieChartComponent)
                            {
                                oCompI.oContent[XIConstant.PieChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.GaugeChartComponent)
                            {
                                oCompI.oContent[XIConstant.GaugeChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.DashBoardChartComponent)
                            {
                                oCompI.oContent[XIConstant.DashBoardChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.CombinationChartComponent)
                            {
                                oCompI.oContent[XIConstant.CombinationChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.MappingComponent)
                            {
                                oCompI.oContent[XIConstant.MappingComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.MultiRowComponent)
                            {
                                oCompI.oContent[XIConstant.MultiRowComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.DailerComponent)
                            {
                                oCompI.oContent[XIConstant.DailerComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.QSComponent)
                            {
                                oCompI.oContent[XIConstant.QSComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.QSComponent)
                            {
                                oCompI.oContent[XIConstant.QSComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.CheckboxComponent)
                            {
                                oCompI.oContent[XIConstant.CheckboxComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4PieChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4PieChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4BarChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4BarChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4LineChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4LineChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4GaugeChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4GaugeChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4SemiPieChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4SemiPieChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4PriceChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4PriceChartComponent] = oResult;
                            }
                            else if (oXIComponent.sName == XIConstant.AM4HeatChartComponent)
                            {
                                oCompI.oContent[XIConstant.AM4HeatChartComponent] = oResult;
                            }
                            if (oXIComponent.sName == XIConstant.AccountComponent)
                            {
                                oCompI.oContent[XIConstant.AccountComponent] = oResult;
                            }
                            ViewBag.RoleName = SessionManager.sRoleName;
                            return PartialView(oXIComponent.sHTMLPage, oCompI);
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

        public ActionResult TreeNodeClick(string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                cXICache oCache = new cXICache();
                string sSessionID = HttpContext.Session.SessionID;
                CInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                List<cNameValuePairs> oComponentParams = new List<cNameValuePairs>();
                foreach (var items in oGUIDParams.NMyInstance)
                {
                    cNameValuePairs Param = new cNameValuePairs();
                    Param.sName = items.Key;
                    Param.sValue = items.Value.sValue;
                    oComponentParams.Add(Param);
                }
                return Json(oComponentParams, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ListClick(string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                cXICache oCache = new cXICache();
                string sSessionID = HttpContext.Session.SessionID;
                CInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                List<cNameValuePairs> oComponentParams = new List<cNameValuePairs>();
                foreach (var items in oGUIDParams.NMyInstance)
                {
                    cNameValuePairs Param = new cNameValuePairs();
                    Param.sName = items.Key;
                    Param.sValue = items.Value.sValue;
                    oComponentParams.Add(Param);
                }
                return Json(oComponentParams, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateStructureTreeDetails(int NodeID, string sGUID)
        {
            cXICache oCache = new cXICache();
            string sSessionID = HttpContext.Session.SessionID;
            CInstance oParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
            var sCode = oParams.NMyInstance.Where(m => m.Key.ToLower() == "scode").Select(m => m.Value.sValue).FirstOrDefault();
            if (NodeID > 0 && !string.IsNullOrEmpty(sCode))
            {
                var Data = XIComponentsRepository.GetBOStructure1Click(sCode, NodeID);
                List<cNameValuePairs> Params = new List<cNameValuePairs>();
                Params.Add(new cNameValuePairs { sName = "1ClickID", sValue = Data.i1ClickID.ToString() });
                return Json(Params, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        [HttpPost]
        public ActionResult UpdateOneClickDetails(int BOIID, string sGUID)
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
                cXICache oCache = new cXICache();
                string sSessionID = HttpContext.Session.SessionID;
                CInstance oParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                var sBOName = oParams.NMyInstance.Where(m => m.Key.ToLower() == "bo").Select(m => m.Value.sValue).FirstOrDefault();
                var BODID = oParams.NMyInstance.Where(m => m.Key.ToLower() == "BODID".ToLower()).Select(m => m.Value.sValue).FirstOrDefault();

                if (BOIID > 0 && sBOName != null)
                {
                    CXiAPI oXIAPI = new CXiAPI();
                    var Data = oXIAPI.GetBONameAttributeValue(sBOName, BOIID, iUserID, sDatabase);
                    List<cNameValuePairs> Params = new List<cNameValuePairs>();
                    Params.Add(new cNameValuePairs { sName = "sInsName", sValue = Data.ToString() });
                    Params.Add(new cNameValuePairs { sName = "sInsID", sValue = BOIID.ToString() });
                    Params.Add(new cNameValuePairs { sName = "BO", sValue = sBOName.ToString() });
                    Params.Add(new cNameValuePairs { sName = "BODID", sValue = BODID.ToString() });
                    return Json(Params, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetComponentParmsByStep(int StepID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                List<XIDComponentParam> oParams = new List<XIDComponentParam>();
                XIDXI oXID = new XIDXI();
                var Params = oXID.Get_ComponentParamsByStep(StepID);
                if (Params.bOK && Params.oResult != null)
                {
                    oParams = (List<XIDComponentParam>)Params.oResult;
                }
                //var Response = XIComponentsRepository.GetComponentParmsByStep(StepID);
                return Json(oParams, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CallQSEvent(List<CNV> QSInfo, string QSEvents)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIDQS oQSD = new XIDQS();
                var nXILinks = oQSD.QSEvent(QSInfo, QSEvents);
                return Json(nXILinks, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public ActionResult GetRepeaterView(XIBODisplay BODisplay, int XIComponentID, int iOneClickID)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIDComponent oXIComponent = new XIDComponent();
            oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, XIComponentID.ToString());
            var Copy = (XIDComponent)oXIComponent.Clone(oXIComponent);
            oXIComponent = Copy.GetParamsByContext(Copy, "OneClick", iOneClickID);
            return PartialView(oXIComponent.sHTMLPage, BODisplay);
            // return PartialView("_GridComponent", BODisplay);
        }
        [AllowAnonymous]
        public ActionResult GetRepeaterGridView(List<XIBODisplay> BODisplay, int XIComponentID, int iOneClickID)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIDComponent oXIComponent = new XIDComponent();
            oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, XIComponentID.ToString());
            var Copy = (XIDComponent)oXIComponent.Clone(oXIComponent);
            oXIComponent = Copy.GetParamsByContext(Copy, "OneClick", iOneClickID);
            return PartialView(oXIComponent.sHTMLPage, BODisplay);
        }

        [AllowAnonymous]
        public ActionResult LoadDataByComponent(XIIBO oBOI, XIDComponent oComponentD)
        {
            XIInfraCache oCache = new XIInfraCache();
            if (oComponentD.sName.ToLower() == XIConstant.FormComponent.ToLower())
            {
                XIIComponent oCompI = new XIIComponent();
                XIBODisplay oBODisplay = new XIBODisplay();
                oBODisplay.BOInstance = oBOI;
                oBOI.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                oCompI.oContent[XIConstant.FormComponent] = oBODisplay;
                oCompI.oDefintion = oComponentD;
                List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                string sVisualisation = oComponentD.Params.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sVisualisation))
                {
                    var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, null);
                    if (oXIvisual != null)
                    {
                        oXIVisualisations.Add(oXIvisual);
                    }
                }
                oCompI.oVisualisation = oXIVisualisations;
                return PartialView(oComponentD.sHTMLPage, oCompI);
            }
            return PartialView(oComponentD.sHTMLPage, oBOI);
        }

        #endregion XIComponents

        #region LoadComponent

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [AllowAnonymous]
        public ActionResult LoadComponent(int iXIComponentID, List<XIDComponentParam> nParams, string sGUID)
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
                string sSessionID = HttpContext.Session.SessionID;
                XIInfraCache oCache = new XIInfraCache();
                var oParams = new List<CNV>();
                //var SessionItems = SessionManager.SessionItems;
                //oParams.AddRange(SessionItems);
                if (nParams != null && nParams.Count() > 0)
                {
                    oParams.AddRange(nParams.Select(m => new CNV { sName = m.sName, sValue = m.sValue }));
                }
                var SessionItems = SessionManager.SessionItems();
                if (SessionItems != null && SessionItems.Count() > 0)
                {
                    oParams.AddRange(SessionItems);
                }
                //oParams.AddRange();
                XIDComponent oComponentD = new XIDComponent();
                oComponentD.ID = iXIComponentID;
                oComponentD.nParams = oParams;

                var Response = new CResult();//oComponentD.Load();
                if (Response.bOK && Response.oResult != null)
                {
                    var oResult = Response.oResult;
                    var oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, iXIComponentID.ToString());
                    ViewBag.sGUID = sGUID;
                    ViewBag.iUserID = iUserID;
                    //oCache.Set_ParamVal(sSessionID, sGUID, sContext, "{XIP|iUserID}", Convert.ToString(iUserID), null, null);
                    if (!string.IsNullOrEmpty(oXIComponent.sHTMLPage))
                    {
                        if (oXIComponent.sName == "Grid Component")
                        {
                            List<XIBODisplay> oBODisplay = new List<XIBODisplay>();
                            XIBODisplay oBOInstance = (XIBODisplay)oResult;
                            oBODisplay.Add(oBOInstance);
                            ViewBag.iCount = nParams.Where(x => x.sName.ToLower() == "iCount".ToLower()).Select(x => x.sValue).FirstOrDefault();
                            return PartialView(oXIComponent.sHTMLPage, oBODisplay);
                        }
                        else
                        {
                            return PartialView(oXIComponent.sHTMLPage, oResult);
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

        [HttpPost]
        public ActionResult LoadStep(string sStep, string sGUID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
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
                            var StepContent = oStepI.Load();
                            if (StepContent.bOK && StepContent.oResult != null)
                            {
                                var oInstance = (XIInstanceBase)StepContent.oResult;

                                if (oInstance.oContent.ContainsKey(XIConstant.ContentStep))
                                {
                                    var StepIns = (XIIQSStep)oInstance.oContent[XIConstant.ContentStep];
                                    foreach (var sec in StepIns.Sections.Values)
                                    {
                                        var secdef = JsonConvert.SerializeObject(sec.oDefintion);
                                        sec.oDefintion = secdef.ToString().Replace("\r\n", "").Replace(@"\", "");
                                        if (sec.oContent.ContainsKey(XIConstant.ContentXIComponent))
                                        {
                                            var cop = (XIInstanceBase)sec.oContent[XIConstant.ContentXIComponent];
                                            var compI = (XIIComponent)cop.oContent[XIConstant.ContentXIComponent];
                                            var compJson = JsonConvert.SerializeObject(compI.oDefintion);
                                            compI.oDefintion = compJson.ToString().Replace("\r\n", "").Replace(@"\", "");
                                            XIInstanceBase oIns = new XIInstanceBase();
                                            oIns.oContent[XIConstant.ContentXIComponent] = compI;
                                            sec.oContent[XIConstant.ContentXIComponent] = oIns;
                                        }
                                    }
                                    string json = JsonConvert.SerializeObject(StepIns.oDefintion);
                                    StepIns.oDefintion = json.ToString().Replace("\r\n", "").Replace(@"\", "");//.Replace("  "," ").Replace("  ", " ").Replace("  ", " ").Replace(" ", "");
                                    oInstance.oContent[XIConstant.ContentStep] = StepIns;

                                }

                                return PartialView("~\\Views\\XiLink\\_XILinkContent.cshtml", oInstance);
                            }
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

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [AllowAnonymous]
        public ActionResult Load_Component(int iComponentID, string sType, int iInstanceID, string sGUID, string sInput)
        {
            if (sType == "section")
            {
                XIDXI oXID = new XIDXI();
                XIInfraCache oCache = new XIInfraCache();
                var oSecD = (XIDQSSection)oCache.GetObjectFromCache(XIConstant.CacheQSSection, null, iInstanceID.ToString());
                XIIQSSection oSecI = new XIIQSSection();
                oSecI.oDefintion = oSecD;
                oSecI.sGUID = sGUID;
                var oCR = oSecI.Load();
                var result = (XIIQSSection)oCR.oResult;
                if (result.oContent.ContainsKey("xicomponent"))
                {
                    var compI = (XIIComponent)result.oContent["xicomponent"];
                    if (compI.oContent.ContainsKey(XIConstant.OneClickComponent))
                    {
                        return PartialView("~/views/XIComponents/_OneClickComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.GroupComponent))
                    {
                        return PartialView("~/views/XIComponents/_GroupComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.ScriptComponent))
                    {
                        return PartialView("~/views/XIComponents/_ScriptComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XilinkComponent))
                    {
                        return PartialView("~/views/XIComponents/_XILinkComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.LayoutComponent))
                    {
                        return PartialView("~/views/XIComponents/_LayoutComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.LayoutDetailsComponent))
                    {
                        return PartialView("~/views/XIComponents/_LayoutDetailsComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.LayoutMappingComponent))
                    {
                        return PartialView("~/views/XIComponents/_LayoutMappingComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIApplicationComponent))
                    {
                        return PartialView("~/views/XIComponents/_ApplicationComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.MenuNodeComponent))
                    {
                        return PartialView("~/views/XIComponents/_MenuNodeComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.FormComponent))
                    {
                        return PartialView("~/views/XIComponents/_FormComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XITreeStructure))
                    {
                        return PartialView("~/views/XIComponents/_XITreeStructureView.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.TabComponent))
                    {
                        return PartialView("~/views/XIComponents/_TabComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.MenuComponent))
                    {
                        return PartialView("~/views/XIComponents/_MenuComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.GridComponent))
                    {
                        return PartialView("~/views/XIComponents/_GridComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIUrlMappingComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIUrlMappingComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.DialogComponent))
                    {
                        return PartialView("~/views/XIComponents/_DialogComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.FieldOriginComponent))
                    {
                        return PartialView("~/views/XIComponents/_FieldOriginComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIParameterComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIParameterComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.DataTypeComponent))
                    {
                        return PartialView("~/views/XIComponents/_DataTypeComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIComponentComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIComponentComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIBOComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIBOComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIBOAttributeComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIBOAttributeComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIBOScriptComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIBOScriptComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIBOStructureComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIBOStructureComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIInfraXIBOUIComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIBOUIDetailsComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.XIDataSourceComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIDataSourceComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.QueryManagementComponent))
                    {
                        return PartialView("~/views/XIComponents/_QueryManagementComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.QSConfigComponent))
                    {
                        return PartialView("~/views/XIComponents/_QSConfigComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.QSStepConfigComponent))
                    {
                        return PartialView("~/views/XIComponents/_QSStepConfigComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.QSSectionConfigComponent))
                    {
                        return PartialView("~/views/XIComponents/_QSSectionConfigComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.VisualisationComponent))
                    {
                        return PartialView("~/views/XIComponents/_VisualisationComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.HTMLComponent))
                    {
                        return PartialView("~/views/XIComponents/_HtmlRepeaterComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.InboxComponent))
                    {
                        return PartialView("~/views/XIComponents/_InboxComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.PieChartComponent))
                    {
                        return PartialView("~/views/XIComponents/_PieChartComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.CombinationChartComponent))
                    {
                        return PartialView("~/views/XIComponents/_CombinationChartComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.GaugeChartComponent))
                    {
                        return PartialView("~/views/XIComponents/_GaugeChartComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.DailerComponent))
                    {
                        return PartialView("~/views/XIComponents/_DailerComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.DashBoardChartComponent))
                    {
                        return PartialView("~/views/XIComponents/_DashBoardChartComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.MappingComponent))
                    {
                        return PartialView("~/views/XIComponents/_MappingComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.MultiRowComponent))
                    {
                        return PartialView("~/views/XIComponents/_MultiRowComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.ReportComponent))
                    {
                        return PartialView("~/views/XIComponents/_XIReportComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.QSLinkComponent))
                    {
                        return PartialView("~/views/XIComponents/_QSLinkComponentComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.QSLinkDefinationComponent))
                    {
                        return PartialView("~/views/XIComponents/_QSLinkDefinationComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.FeedComponent))
                    {
                        return PartialView("~/views/XIComponents/_FeedComponent.cshtml", compI);
                    }
                    else if (compI.oContent.ContainsKey(XIConstant.DocumentTreeComponent))
                    {
                        return PartialView("~/views/XIComponents/_DocumentTreeComponent.cshtml", compI);
                    }
                }
            }
            else
            {
                XIIComponent oXICompI = new XIIComponent();
                XIDComponent oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, iComponentID.ToString());
                oXIComponent = (XIDComponent)oXIComponent.Clone(oXIComponent);
                oXIComponent.GetComponentParams(sType, iInstanceID);
                if (oXIComponent != null && oXIComponent.Params != null && oXIComponent.Params.Count() > 0)
                {
                    oXICompI.sGUID = sGUID;
                    oXICompI.oDefintion = oXIComponent;
                    //oXICompI.sCallHierarchy = "Layout_" + oLayoutC.ID + ":LayoutMappingID_" + items.ID + ":Component_" + items.XiLinkID;
                    var Data = oXICompI.Load();
                    var compI = (XIIComponent)Data.oResult;
                    if (!string.IsNullOrEmpty(sInput))
                    {
                        List<CNV> RightValues = new List<CNV>();
                        var Data1 = (Dictionary<string, object>)compI.oContent[XIConstant.MappingComponent];
                        var LeftValues = (List<CNV>)Data1["LeftValues"];
                        var Values = sInput.Split(',').ToList();
                        foreach (var item in Values)
                        {
                            var LItem = LeftValues.Where(m => m.sValue == item).FirstOrDefault();
                            RightValues.Add(new CNV { sName = LeftValues.Where(m => m.sValue == item).Select(m => m.sName).FirstOrDefault(), sValue = item });
                            LeftValues.Remove(LItem);
                        }
                        Data1["RightValues"] = RightValues;
                        compI.oContent[XIConstant.MappingComponent] = Data1;
                        compI.oContent.Add("RightValues", RightValues);
                    }
                    return PartialView("~/views/XIComponents/_MappingComponent.cshtml", compI);
                }
            }


            return null;
        }

        #endregion LoadComponent

        #region FilterInstanceTree

        [HttpPost]
        public ActionResult FilterInstanceTree(string sSearchText, string sParentID, int iBODID, int iBuildingID, string sFolder)
        {
            var sFname = sFolder.Replace(@"//", @"\");
            var FName = sFname.LastIndexOf(@"\");
            var FolName = sFname.Substring(FName, sFname.Length - FName).Replace(@"\", "");
            XIInfraTreeStructureComponent oTree = new XIInfraTreeStructureComponent();
            List<CNV> oParams = new List<CNV>();
            oParams.Add(new CNV { sName = XIConstant.Param_Mode, sValue = "instancetreefilter" });
            oParams.Add(new CNV { sName = XIConstant.Param_SearchText, sValue = sSearchText });
            oParams.Add(new CNV { sName = XIConstant.Param_ParentID, sValue = sParentID });
            oParams.Add(new CNV { sName = XIConstant.Param_BODID, sValue = iBODID.ToString() });
            oParams.Add(new CNV { sName = "BuildingID", sValue = iBuildingID.ToString() });
            oParams.Add(new CNV { sName = "FolderName", sValue = FolName });
            var oCR = oTree.XILoad(oParams);
            if (oCR.bOK && oCR.oResult != null)
            {
                XIIComponent oCompI = new XIIComponent();
                oCompI.oContent[XIConstant.XITreeStructure] = oCR.oResult;
                return PartialView("~/views/XIComponents/_XITreeStructureView.cshtml", oCompI);
            }
            return null;
        }

        [HttpPost]
        public ActionResult GetInstanceTree(string sParentID, int iBODID, int iBuildingID, string sFolder)
        {
            var sFname = sFolder.Replace(@"//", @"\");
            var FName = sFname.LastIndexOf(@"\");
            var FolName = sFname.Substring(FName, sFname.Length - FName).Replace(@"\", "");
            XIInfraTreeStructureComponent oTree = new XIInfraTreeStructureComponent();
            List<CNV> oParams = new List<CNV>();
            oParams.Add(new CNV { sName = XIConstant.Param_Mode, sValue = "instancetree" });
            oParams.Add(new CNV { sName = XIConstant.Param_ParentID, sValue = sParentID });
            oParams.Add(new CNV { sName = XIConstant.Param_BODID, sValue = iBODID.ToString() });
            oParams.Add(new CNV { sName = "BuildingID", sValue = iBuildingID.ToString() });
            oParams.Add(new CNV { sName = "FolderName", sValue = FolName });
            var oCR = oTree.XILoad(oParams);
            if (oCR.bOK && oCR.oResult != null)
            {
                List<XIDStructure> oStr = new List<XIDStructure>();
                oStr = (List<XIDStructure>)oCR.oResult;
                var Data = oStr.FirstOrDefault().oStructureInstance;
                var Res = Data.Values.ToList().FirstOrDefault();
                return Json(Res, JsonRequestBehavior.AllowGet);
            }
            return null;
        }


        #endregion FilterInstanceTree


        #region NanoComponent

        public ActionResult Nano1()
        {
            return View("_NanoComponent");
        }


        public ActionResult Nano()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            XIIComponent sHTMLOutput = new XIIComponent();
            XIIComponent oCompI = new XIIComponent();
            try
            {

                oTrace.oParams.Add(new CNV { sName = "", sValue = "" });
                XIInfraHTMLBasicComponent oHTML = new XIInfraHTMLBasicComponent();
                List<CNV> oParams = new List<CNV>();
                oParams.Add(new CNV { sName = "sBO", sValue = "" });
                oParams.Add(new CNV { sName = "iInstanceID", sValue = "" });
                oParams.Add(new CNV { sName = "i1ClickID", sValue = "7874" });
                oParams.Add(new CNV { sName = "sHTML", sValue = "" });
                oCR = oHTML.XILoad(oParams);
                if (oCR.bOK && oCR.oResult != null)
                {

                    oCompI.oContent[XIConstant.FeedComponent] = oCR.oResult;
                    //sHTMLOutput = (XIIComponent)oCR.oResult;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
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
            return PartialView("_FeedComponent", oCompI);
        }

        #endregion NanoComponent
        [HttpPost]
        public ActionResult ValidateSaveScript(string Script, string RowID, string sBOName, List<CNV> oNVParams = null)
        {
            CResult oCR = new CResult();
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var Msg = "";
                CScriptController oXIScript = new CScriptController();
                oCR = oXIScript.API2_Serialise_From_String(Script);
                oCR = oXIScript.API2_ExecuteMyOM();
                if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
                {
                    Msg = oCR.sMessage;
                }
                else
                {
                    XIIBO oBOI = new XIIBO();
                    XIIXI oXI = new XIIXI();
                    if (RowID == "0")
                    {
                        if (sBOName.ToLower() == "xiboscripts")
                        {
                            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, null); //oXID.Get_BODefinition("Documents_T").oResult;
                            oBOI.BOD = oBOD;
                            oBOI.Attributes["sScript"] = new XIIAttribute { sName = "sScript", sValue = Script, bDirty = true };
                            oBOI.Attributes["sType"] = new XIIAttribute { sName = "sType", sValue = oNVParams.Any(x => x.sName == "sType") ? oNVParams.Where(x => x.sName == "sType").Select(t => t.sValue).FirstOrDefault() : "Postpersist", bDirty = true };
                            oBOI.Attributes["sLanguage"] = new XIIAttribute { sName = "sLanguage", sValue = "XIScript", bDirty = true };
                            oBOI.Attributes["OrganisationID"] = new XIIAttribute { sName = "OrganisationID", sValue = "5", bDirty = true };
                            oBOI.Attributes["FKiBOID"] = new XIIAttribute { sName = "FKiBOID", sValue = "0", bDirty = true };
                            oBOI.Attributes["sName"] = new XIIAttribute { sName = "sName", sValue = oNVParams.Where(x => x.sName == "sName").Select(t => t.sValue).FirstOrDefault(), bDirty = true };
                            oBOI.Attributes["sDescription"] = new XIIAttribute { sName = "sDescription", sValue = oNVParams.Where(x => x.sName == "sDescription").Select(t => t.sValue).FirstOrDefault(), bDirty = true };
                            oCR = oBOI.Save(oBOI);//to save
                        }
                        else
                        {
                            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, null); //oXID.Get_BODefinition("Documents_T").oResult;
                            oBOI.BOD = oBOD;
                            oBOI.Attributes["sScript"] = new XIIAttribute { sName = "sScript", sValue = Script, bDirty = true };
                            oCR = oBOI.Save(oBOI);//to save
                        }
                    }
                    else
                    {
                        if (sBOName.ToLower() == "xiboscripts")
                        {
                            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, null); //oXID.Get_BODefinition("Documents_T").oResult;
                            oBOI = oXI.BOI(sBOName, RowID);
                            oBOI.BOD = oBOD;
                            oBOI.Attributes["sScript"] = new XIIAttribute { sName = "sScript", sValue = Script, bDirty = true };
                            oBOI.Attributes["sType"] = new XIIAttribute { sName = "sType", sValue = oNVParams.Any(x => x.sName == "sType") ? oNVParams.Where(x => x.sName == "sType").Select(t => t.sValue).FirstOrDefault() : "Postpersist", bDirty = true };
                            oBOI.Attributes["sName"] = new XIIAttribute { sName = "sName", sValue = string.IsNullOrEmpty(oNVParams.Where(x => x.sName == "sName").Select(t => t.sValue).FirstOrDefault()) ? oBOI.AttributeI("sName").sValue : oNVParams.Where(x => x.sName == "sName" && x.sValue != "").Select(t => t.sValue).FirstOrDefault(), bDirty = true };
                            oBOI.Attributes["sDescription"] = new XIIAttribute { sName = "sDescription", sValue = string.IsNullOrEmpty(oNVParams.Where(x => x.sName == "sDescription" && x.sValue != "").Select(t => t.sValue).FirstOrDefault()) ? oBOI.AttributeI("sDescription").sValue : oNVParams.Where(x => x.sName == "sDescription" && x.sValue != "").Select(t => t.sValue).FirstOrDefault(), bDirty = true };
                            oCR = oBOI.Save(oBOI);//to save
                        }
                        else
                        {
                            string sNewScript = string.Empty;
                            var id = oNVParams.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            var iType = oNVParams.Where(m => m.sName.ToLower() == "iType".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            var sIndent = oNVParams.Where(m => m.sName.ToLower() == "sIndent".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            if (iType == "30")
                            {
                                var Items = Script.Split('_');
                                if (Script.Contains('_'))
                                {
                                    var indentString = "";
                                    if (!string.IsNullOrEmpty(sIndent))
                                    {
                                        for (int i = 0; i < sIndent.Length; i++)
                                        {
                                            indentString += "-";
                                        }
                                        Items[1] = indentString + ":" + Items[1];
                                    }
                                    sNewScript = string.Join("_", Items);
                                }
                                else
                                    sNewScript = Script;
                            }
                            if (!string.IsNullOrEmpty(id))
                            {
                                RowID = id;
                                oBOI = oXI.BOI(sBOName, RowID);
                            }
                            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, null); //oXID.Get_BODefinition("Documents_T").oResult;

                            oBOI.BOD = oBOD;
                            foreach (var item in oNVParams)
                            {
                                if (item.sName == "sScript")
                                {
                                    oBOI.SetAttribute(item.sName, sNewScript);
                                }
                                else
                                {
                                    oBOI.SetAttribute(item.sName, item.sValue);
                                }
                            }
                            oBOI.SetAttribute("FKiAlgorithmID", RowID.ToString());
                            oCR = oBOI.Save(oBOI);
                        }
                    }
                    Msg = "Script saved successfully.";
                }
                return Json(Msg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult LoadFeed(string SecID, string sGUID, string sPageNo)
        {
            string sSessionID = HttpContext.Session.SessionID;
            XIDXI oXID = new XIDXI();
            XIInfraCache oCache = new XIInfraCache();
            var oSecD = (XIDQSSection)oCache.GetObjectFromCache(XIConstant.CacheQSSection, null, SecID);
            XIIQSSection oSecI = new XIIQSSection();
            oSecI.oDefintion = oSecD;
            oSecI.sGUID = sGUID;
            oCache.Set_ParamVal(sSessionID, sGUID, null, "iPage", sPageNo, null, null);
            oCache.Set_ParamVal(sSessionID, sGUID, null, "sLoadType", "Scroll", null, null);
            var oCR = oSecI.Load();
            var result = (XIIQSSection)oCR.oResult;
            if (result.oContent.ContainsKey("xicomponent"))
            {
                var oComp = (XIIComponent)result.oContent[XIConstant.ContentXIComponent];
                var Feed = (XIIComponent)oComp.oContent[XIConstant.FeedComponent];
                if (Feed.oContent.ContainsKey("Left"))
                {
                    var oData = (List<Dictionary<string, object>>)Feed.oContent["Left"];
                    NotifyHub oHub = new NotifyHub();
                    XIDWidget oWid = new XIDWidget();
                    string sFeedMessge = string.Empty;
                    string iBODID = string.Empty;
                    string iBOIID = string.Empty;
                    string sMesID = string.Empty;
                    var Time = string.Empty;
                    foreach (var items in oData)
                    {
                        XIIBO oMesI = new XIIBO();
                        string sType = string.Empty;
                        if (items.ContainsKey("LeftMessage"))
                        {
                            sType = "Left";
                            oMesI = (XIIBO)items["LeftMessage"];
                        }
                        else if (items.ContainsKey("RightMessage"))
                        {
                            sType = "Right";
                            oMesI = (XIIBO)items["RightMessage"];
                        }
                        var SrcOrgID = "";
                        if (items.ContainsKey("iNannoOrgID"))
                        {
                            SrcOrgID = (string)items["iNannoOrgID"];
                        }
                        oWid = (XIDWidget)items["Widget"];
                        sFeedMessge = oMesI.AttributeI("sMessage").sValue;
                        iBOIID = oMesI.AttributeI("iBOIID").sValue;
                        iBODID = oMesI.AttributeI("iBODID").sValue;
                        sMesID = oMesI.AttributeI("ID").sValue;
                        Time = oMesI.AttributeI("XIUpdatedWhen").sResolvedValue;
                        oHub.UpdateFeed(sFeedMessge, Time, sMesID, oWid.FKiLayoutID, iBODID + "-" + iBOIID, SrcOrgID, SessionManager.iUserOrg, "Scroll", sType, false);
                    }
                }
            }
            //List<CNV> oParams = new List<CNV>();

            //var Params = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
            //var Group = Params.NMyInstance["ListClickparamname"].nSubParams;
            //var iGroupID = Group.Where(m => m.sName == "{XIP|iGroupID}").Select(m => m.sValue).FirstOrDefault();
            //oParams.Add(new CNV { sName = XIConstant.Param_SessionID, sValue = sSessionID });
            //oParams.Add(new CNV { sName = XIConstant.Param_GUID, sValue = sGUID });
            //oParams.Add(new CNV { sName = "{XIP|iGroupID}", sValue = iGroupID });
            //oParams.Add(new CNV { sName = "{XIP|iUserOrgID}", sValue = SessionManager.iUserOrg.ToString() });
            //CUserInfo oInfo = new CUserInfo();
            //oInfo = oUser.Get_UserInfo();
            //oParams.Add(new CNV { sName = "iOrgID", sValue = oInfo.iOrganizationID.ToString() });
            //oParams.Add(new CNV { sName = "{XIP|iOrgID}", sValue = oInfo.iOrganizationID.ToString() });
            //oParams.Add(new CNV { sName = "sCoreDatabase", sValue = oInfo.sCoreDataBase });
            //oParams.Add(new CNV { sName = "sMode", sValue = "nanno" });
            //oParams.Add(new CNV { sName = "iPage", sValue = sPageNo });
            //oParams.Add(new CNV { sName = "iCount", sValue = "5" });
            //oParams.Add(new CNV { sName = "sLoadType", sValue = "Scroll" });
            //XIInfraHTMLBasicComponent oFeed = new XIInfraHTMLBasicComponent();
            //oCR = oFeed.XILoad(oParams);
            //var oCompI = (XIIComponent)oCR.oResult;
            //if (oCompI.oContent.ContainsKey("Left"))
            //{
            //    var oData = (List<Dictionary<string, object>>)oCompI.oContent["Left"];

            //}
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeOrder(string iInstanceID, string sBOName, string Value)
        {
            string sDatabase = SessionManager.CoreDatabase;
            CResult oResult = new CResult();
            try
            {
                XIIXI oXIIXI = new XIIXI();
                XIIBO oBOI = new XIIBO();
                oBOI = oXIIXI.BOI(sBOName, iInstanceID);
                XIIBO oReferenceBOI = new XIIBO();
                List<CNV> oWhrParams = new List<CNV>();
                CNV oParam = new CNV();
                oParam.sName = "iOrder";
                oParam.sValue = Value == "increment" ? (Convert.ToInt32(oBOI.AttributeI("iOrder").sValue) + 1).ToString() : (Convert.ToInt32(oBOI.AttributeI("iOrder").sValue) - 1).ToString();
                oWhrParams.Add(oParam);
                oParam = new CNV();
                oParam.sName = "FKiAlgorithmID";
                oParam.sValue = oBOI.AttributeI("FKiAlgorithmID").sValue;
                oWhrParams.Add(oParam);
                oReferenceBOI = oXIIXI.BOI(sBOName, "", "", oWhrParams);
                string sIndent = oBOI.AttributeI("sIndent").sValue;
                string iOrder = oBOI.AttributeI("iOrder").sValue;
                if (Value == "increment")
                {
                    if (oReferenceBOI != null && oReferenceBOI.AttributeI("sIndent").sValue.Length != oBOI.AttributeI("sIndent").sValue.Length)
                    {
                        if (oBOI.AttributeI("sIndent").sValue.Length == 0)
                        {
                            var sScriptArray = oBOI.AttributeI("sScript").sValue.Split(':');
                            var sScript = string.Empty;
                            for (int i = 0; i < sScriptArray.Length; i++)
                            {
                                sScript = i == 0 ? sScript + sScriptArray[i] + oReferenceBOI.AttributeI("sIndent").sValue + ":" : sScript + sScriptArray[i] + ":";
                            }
                            oBOI.SetAttribute("sScript", sScript.Substring(0, sScript.Length - 2));

                        }
                        else
                            oBOI.SetAttribute("sScript", oBOI.AttributeI("sScript").sValue.Replace(sIndent, oReferenceBOI.AttributeI("sIndent").sValue));
                        oBOI.SetAttribute("sIndent", oReferenceBOI.AttributeI("sIndent").sValue);
                        if (oReferenceBOI.AttributeI("sIndent").sValue.Length == 0)
                        {
                            var sScriptArray = oReferenceBOI.AttributeI("sScript").sValue.Split(':');
                            var sScript = string.Empty;
                            for (int i = 0; i < sScriptArray.Length; i++)
                            {
                                sScript = i == 0 ? sScript + sScriptArray[i] + sIndent + ":" : sScript + sScriptArray[i] + ":";
                            }
                            oReferenceBOI.SetAttribute("sScript", sScript.Substring(0, sScript.Length - 2));
                        }
                        else
                            oReferenceBOI.SetAttribute("sScript", oReferenceBOI.AttributeI("sScript").sValue.Replace(oReferenceBOI.AttributeI("sIndent").sValue, sIndent));
                        oReferenceBOI.SetAttribute("sIndent", sIndent);
                        oReferenceBOI.SetAttribute("iOrder", (Convert.ToInt32(oReferenceBOI.AttributeI("iOrder").sValue) - 1).ToString());
                        var res = oReferenceBOI.Save(oReferenceBOI);
                    }
                    else if (oReferenceBOI != null && oReferenceBOI.AttributeI("sIndent").sValue.Length == oBOI.AttributeI("sIndent").sValue.Length)
                    {
                        oReferenceBOI.SetAttribute("iOrder", (Convert.ToInt32(oReferenceBOI.AttributeI("iOrder").sValue) - 1).ToString());
                        var res = oReferenceBOI.Save(oReferenceBOI);
                    }
                    oBOI.SetAttribute("iOrder", (Convert.ToInt32(oBOI.AttributeI("iOrder").sValue) + 1).ToString());
                }
                else
                {
                    if (oReferenceBOI != null && oReferenceBOI.AttributeI("sIndent").sValue.Length != oBOI.AttributeI("sIndent").sValue.Length)
                    {
                        if (oBOI.AttributeI("sIndent").sValue.Length == 0)
                        {
                            var sScriptArray = oBOI.AttributeI("sScript").sValue.Split(':');
                            var sScript = string.Empty;
                            for (int i = 0; i < sScriptArray.Length; i++)
                            {
                                sScript = i == 0 ? sScript + sScriptArray[i] + oReferenceBOI.AttributeI("sIndent").sValue + ":" : sScript + sScriptArray[i] + ":";
                            }
                            oBOI.SetAttribute("sScript", sScript.Substring(0, sScript.Length - 2));
                        }
                        else
                            oBOI.SetAttribute("sScript", oBOI.AttributeI("sScript").sValue.Replace(sIndent, oReferenceBOI.AttributeI("sIndent").sValue));
                        oBOI.SetAttribute("sScript", oBOI.AttributeI("sScript").sValue.Replace(sIndent, oReferenceBOI.AttributeI("sIndent").sValue));
                        oBOI.SetAttribute("sIndent", oReferenceBOI.AttributeI("sIndent").sValue);
                        if (oReferenceBOI.AttributeI("sIndent").sValue.Length == 0)
                        {
                            var sScriptArray = oReferenceBOI.AttributeI("sScript").sValue.Split(':');
                            var sScript = string.Empty;
                            for (int i = 0; i < sScriptArray.Length; i++)
                            {
                                sScript = i == 0 ? sScript + sScriptArray[i] + sIndent + ":" : sScript + sScriptArray[i] + ":";
                            }
                            oReferenceBOI.SetAttribute("sScript", sScript.Substring(0, sScript.Length - 2));
                        }
                        else
                            oReferenceBOI.SetAttribute("sScript", oReferenceBOI.AttributeI("sScript").sValue.Replace(oReferenceBOI.AttributeI("sIndent").sValue, sIndent));
                        oReferenceBOI.SetAttribute("sIndent", sIndent);
                        oReferenceBOI.SetAttribute("iOrder", (Convert.ToInt32(oReferenceBOI.AttributeI("iOrder").sValue) + 1).ToString());
                        var res = oReferenceBOI.Save(oReferenceBOI);
                    }
                    else if (oReferenceBOI != null && oReferenceBOI.AttributeI("sIndent").sValue.Length == oBOI.AttributeI("sIndent").sValue.Length)
                    {
                        oReferenceBOI.SetAttribute("iOrder", (Convert.ToInt32(oReferenceBOI.AttributeI("iOrder").sValue) + 1).ToString());
                        var res = oReferenceBOI.Save(oReferenceBOI);
                    }
                    oBOI.SetAttribute("iOrder", (Convert.ToInt32(oBOI.AttributeI("iOrder").sValue) - 1).ToString());
                }
                oResult = oBOI.Save(oBOI);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult LibraryScript(string iBODID, string sBOName, string sInstanceID, string sGUID, int iLength)
        {
            XIIComponent oCompI = new XIIComponent();
            XIDComponent oComponentD = new XIDComponent();
            oComponentD.ID = 5;
            oCompI.sGUID = sGUID;
            var oXIComponent = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, "", "5");
            oCompI.oDefintion = oXIComponent;
            XIInfraTreeStructureComponent oTSC = new XIInfraTreeStructureComponent();
            List<CNV> oParams = new List<CNV>(); //iinstanceid,ibodid
            CNV oParam = new CNV();
            oParam.sName = "iinstanceid";
            oParam.sValue = sInstanceID;
            oParams.Add(oParam);
            oParam = new CNV();
            oParam.sName = "ibodid";
            oParam.sValue = iBODID;
            oParams.Add(oParam);
            oParam = new CNV();
            oParam.sName = "sMode";
            oParam.sValue = "script";
            oParams.Add(oParam);
            object Response = oTSC.XILoad(oParams).oResult;
            oCompI.oContent[XIConstant.XITreeStructure] = Response;
            if (iLength < 2)
            {
                return PartialView(oXIComponent.sHTMLPage, oCompI);
            }
            else
            {
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
