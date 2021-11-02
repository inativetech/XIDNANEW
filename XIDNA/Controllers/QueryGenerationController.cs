using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XIDNA.Models;
using XIDNA.Repository;
using XIDNA.ViewModels;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using XIDNA.Common;
using XICore;
using XISystem;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;
using System.Diagnostics;

namespace XIDNA.Controllers
{
    [Authorize]
    [SessionTimeout]
    public class QueryGenerationController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IGenerateQueryRepository GenerateQueryRepository;

        public QueryGenerationController() : this(new GenerateQueryRepository()) { }

        public QueryGenerationController(IGenerateQueryRepository GenerateQueryRepository)
        {
            this.GenerateQueryRepository = GenerateQueryRepository;
        }
        XIInfraUsers oUser = new XIInfraUsers();
        CommonRepository Common = new CommonRepository();
        XISemanticsRepository XiSemanticRepo = new XISemanticsRepository();
        //
        // GET: /QueryGeneration/
        public ActionResult Index()
        {
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
            return View(OrgID);
        }
        //public ActionResult SelectFieldsTree()
        //{
        //    var res = GenerateQueryRepository.GetAllBos();
        //    return PartialView(res);
        //}
        public ActionResult GetQueryList(jQueryDataTableParamModel param, int? ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = GenerateQueryRepository.GetQueryList(param, iUserID, sOrgName, OrgID, sDatabase);
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
        [AllowAnonymous]
        public ActionResult SaveGrid(List<XIIAttribute> Attributes, string sGUID, string sContext, string sBOName, List<XIIBO> oBOIInstance)
        {
            XIInfraCache oCache = new XIInfraCache();
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
                oAttributes = Attributes;
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

        public ActionResult GetQueryForm()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return RedirectToAction("AddQuery");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult AddQuery()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var res1 = new List<string>();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                VMReports Model = GenerateQueryRepository.GetAllBos(OrgID, iUserID, sOrgName, sDatabase);
                Model.OrganizationID = OrgID;
                Model.StatusTypes = Common.GetStatusTypeDDL(sDatabase);
                Model.ddlBOGroups = new List<VMDropDown>();
                Model.ddlLayouts = Common.GetLayoutsDDL(iUserID, sOrgName, sDatabase);
                Model.ddlLayoutMappings = Common.GetLayoutMappingsDDL(0, sDatabase);
                Model.XILinks = XiSemanticRepo.GetXILinks(iUserID, sOrgName, sDatabase);
                Model.ddlVisualisations = Common.GetXIVisualisationsDDL(iUserID, sOrgName, sDatabase);
                var RolesTree = oUser.Role.Get_RolesTree(sDatabase, oUser.FKiOrganisationID);
                ViewBag.Group = (List<XIInfraRoles>)RolesTree.oResult;
                Model.ddlOneClicks = Model.ddlOneClicks;
                return View("QueryFormWindow", Model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetAvailableFields(int BOID = 0, int Type = 0, int ClassType = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var res = GenerateQueryRepository.GetAvailableFields(BOID, Type, ClassType, OrgID, iUserID, sOrgName, sDatabase);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetWhereValues(int FieldID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var res = GenerateQueryRepository.GetWhereValues(FieldID, OrgID, sDatabase);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetDBValuesForField(string Query)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var res = GenerateQueryRepository.GetDBValuesForField(Query, iUserID, sOrgName, sDatabase);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetOperators(string DataType)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var res = GenerateQueryRepository.GetOperators(DataType, sDatabase);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        [HttpPost]
        public ActionResult UpdateQuery(VMReports model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var res = GenerateQueryRepository.UpdateQuery(model, iUserID, sOrgName, sDatabase);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetXiLinkListByOrg()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                VMReports Result = GenerateQueryRepository.GetXiLinksList(sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetXiParameterListByOrg()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                VMReports Result = GenerateQueryRepository.GetXiParametersList(sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGroupsByBOID(int BOID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {

                List<VMDropDown> ddlCreateGroups = GenerateQueryRepository.GetGroupsByBOID(BOID, sDatabase);
                return Json(ddlCreateGroups, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpPost]
        public ActionResult QueryEditPopUP(int QueryID, string Type)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                if (Type == "Copy")
                {
                    var NewQueryID = GenerateQueryRepository.SaveQueryCopy(QueryID, OrgID, iUserID, sOrgName, sDatabase);
                    QueryID = NewQueryID;
                }
                VMReports model = GenerateQueryRepository.GetQueryByID(QueryID, sDatabase);
                VMReports Result = GenerateQueryRepository.GetAllBos(OrgID, iUserID, sOrgName, sDatabase);
                //model.Classes = GenerateQueryRepository.GetClasses();
                //model.OrganizationID = OrgID;
                model.AllBOs = Result.AllBOs;
                model.Classes = Result.Classes;
                model.ReportTypes = Result.ReportTypes;
                model.InnerReports = Result.InnerReports;
                //model.PopupsList = Result.PopupsList;
                model.XiLinksList = Result.XiLinksList;
                model.TargetUsersList = Result.TargetUsersList;
                model.Parent1Clicks = Result.Parent1Clicks;
                model.Type = Type;
                model.AllBOss = Result.AllBOss;
                model.EmailTemplates = Result.EmailTemplates;
                model.SMSTemplates = Result.SMSTemplates;
                model.StatusTypes = Common.GetStatusTypeDDL(sDatabase);
                model.ddlRoles = Result.ddlRoles;
                model.ddlBOGroups = new List<VMDropDown>();
                model.ddlLayouts = Common.GetLayoutsDDL(iUserID, sOrgName, sDatabase);
                model.ddlLayoutMappings = Common.GetLayoutMappingsDDL(0, sDatabase);
                model.XIComponentList = Common.GetXIComponentsDDL(iUserID, sOrgName, sDatabase);
                ModelDbContext dbContext = new ModelDbContext();
                List<XI1ClickNVs> NVs = new List<XI1ClickNVs>();
                NVs = dbContext.XI1ClickNVs.Where(x => x.FKi1ClickID == QueryID).ToList();
                model.NVs = NVs.Select(m => new VMNameValuePairs { ID = m.ID, sName = m.sName, sValue = m.sValue }).ToList();
                List<XI1ClickParameterNDVs> NDVs = new List<XI1ClickParameterNDVs>();
                NDVs = dbContext.XI1ClickParameterNDVs.Where(y => y.FKi1ClickID == QueryID).ToList();
                model.NDVs = NDVs.Select(m => new VMNameValuePairs { ID = m.ID, sName = m.sName, sContext = m.sDefault, sValue = m.sValue }).ToList();
                model.XILinks = XiSemanticRepo.GetXILinks(iUserID, sOrgName, sDatabase);
                model.OneClickXILinks = GenerateQueryRepository.XILinkValues(QueryID, iUserID, sOrgName, sDatabase);
                var RolesTree = oUser.Role.Get_RolesTree(sDatabase, oUser.FKiOrganisationID);
                ViewBag.Group = (List<XIInfraRoles>)RolesTree.oResult;
                model.GroupIDs = dbContext.XI1ClickPermissions.Where(m => m.FKi1ClickID == QueryID).Select(m => m.FKiRoleID).ToList();
                model.ddlVisualisations = Common.GetXIVisualisationsDDL(iUserID, sOrgName, sDatabase);
                model.ddlOneClicks = Result.ddlOneClicks;
                return PartialView("_QueryUpdateWindow", model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        [HttpPost]
        public ActionResult GetQueryByID(string Type, int QueryID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return RedirectToAction("QueryEditPopUP", "QueryGeneration", new { QueryID = QueryID, Type = Type });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        [HttpGet]
        public JsonResult GetSelFeildsByID(int QueryID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                VMReports model = GenerateQueryRepository.GetQueryByID(QueryID, sDatabase);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpGet]
        public JsonResult GetActionFeildsByID(int QueryID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                VMQueryActions model = GenerateQueryRepository.GetActionFeildsByID(QueryID, sDatabase);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult IsExistsQueryName(string Name, int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                return GenerateQueryRepository.IsExistsQueryName(Name, ID, OrgID, sDatabase) ? Json(true, JsonRequestBehavior.AllowGet)
                     : Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveQuery(VMReports model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int iOrgID = oUser.FKiOrganisationID;
                model.OrganizationID = iOrgID;
                model.CreatedByName = User.Identity.GetUserName();
                int id = GenerateQueryRepository.SaveQuery(model, iUserID, sOrgName, sDatabase);
                return Json(id, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveQuerySearchFields(int QueryID, string SearchFields, bool bIsMultiSearch, bool bIsXICreatedBy, bool bIsXIUpdatedBy, int FKiCrtd1ClickID, int FKiUpdtd1ClickID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                //VMQueryActions models = new VMQueryActions(); 
                int id = GenerateQueryRepository.SaveQuerySearchFields(QueryID, SearchFields, bIsMultiSearch, bIsXICreatedBy, bIsXIUpdatedBy, FKiCrtd1ClickID, FKiUpdtd1ClickID, sDatabase);
                return Json(id, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveQueryActions(VMQueryActions model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                //VMQueryActions models = new VMQueryActions();
                int id = GenerateQueryRepository.SaveQueryActions(model, sDatabase);
                return Json(id, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveQueryTargets(VMReports model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                model.OrganizationID = OrgID;
                int id = GenerateQueryRepository.SaveQueryTargets(model, iUserID, sOrgName, sDatabase);
                return Json(id, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveQueryScheduler(VMReports model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                model.OrganizationID = OrgID;
                int id = GenerateQueryRepository.SaveQueryScheduler(model, iUserID, sOrgName, sDatabase);
                return Json(id, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteQuery(int QueryID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int id = GenerateQueryRepository.DeleteQuery(QueryID, sDatabase);
                return Json(QueryID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetQueryPreview(int QueryID, int PageIndex)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                VMQueryPreview res = GenerateQueryRepository.GetQueryPreview(QueryID, PageIndex, Convert.ToInt32(User.Identity.GetUserId()), sOrgName, sDatabase, OrgID);
                var AutoLoadResultLlist = res.Rows;
                res.QueryID = QueryID;
                if (PageIndex >= 2)
                {
                    return Json(AutoLoadResultLlist, JsonRequestBehavior.AllowGet);
                }
                return PartialView("_QueryPreviewFromGrid", res);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult CheckQueryStatus(string Query, int BOID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                string CurrentGuestUser = null;
                if (SessionManager.UserUniqueID != null)
                {
                    CurrentGuestUser = SessionManager.UserUniqueID;
                }
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.CheckQueryStatus(Query, BOID, iUserID, OrgID, sOrgName, sDatabase, CurrentGuestUser);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPreviewInForm(int ID, string Query, string Fields, int BOID, string ResultIn, int DisplayAs = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var VisibleQuery = Query;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                VMReports model = new VMReports();
                model.UserID = iUserID;
                model.OrganizationID = OrgID;
                model.SelectFields = Fields;
                model.Query = Query;
                model.BOID = BOID;
                model.ID = ID;
                model.DisplayAs = DisplayAs;
                model.ResultIn = ResultIn;
                model.VisibleQuery = VisibleQuery;
                model.AllBOs = new List<VMDropDown>();
                VMQueryPreview res = GenerateQueryRepository.GetHeadingsOfQuery(model, iUserID, sOrgName, sDatabase);
                res.Select = Fields;
                res.BOID = BOID;
                res.ReportID = ID;
                res.PreviewType = ResultIn;
                res.VisibleQuery = VisibleQuery;
                return PartialView("_QueryPreviewFromForm", res);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPreviewResult(jQueryDataTableParamModel param, int ReportID, string Query, string Fields, int BOID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                string sCurrentGuestUser = null;
                if (SessionManager.UserUniqueID != null)
                {
                    sCurrentGuestUser = SessionManager.UserUniqueID;
                }
                var result = GenerateQueryRepository.GetPreviewInForm(param, ReportID, Query, Fields, BOID, iUserID, OrgID, sOrgName, sDatabase, sCurrentGuestUser);
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

        public ActionResult GetPreviewInFormEdited(string Query, int BOID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                VMQueryPreview res = GenerateQueryRepository.GetPreviewInFormEdited(Query, BOID, sDatabase, iUserID, OrgID, sOrgName);
                return PartialView("_QueryPreviewFromForm", res);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public JsonResult GetQueryStatus(int ID, string Query, string Fields, int BOID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                VMQueryPreview res = GenerateQueryRepository.GetQueryStatus(ID, Query, Fields, iUserID, sOrgName, sDatabase, BOID);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult DisplayQueryResult(VMQueryPreview model)
        {
            model.IsFilterSearch = true;
            return PartialView("_QueryPreviewFromForm", model);
        }

        [HttpPost]
        public ActionResult DisplayQueryResults(VMQueryPreview model)
        {
            return PartialView("_QueryPreviewFromForm", model);
            //return null;
        }
        public ActionResult IsPopupNameExists(string Name)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return GenerateQueryRepository.IsPopupNameExists(Name, sDatabase) ? Json(true, JsonRequestBehavior.AllowGet)
                     : Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //Structured 1-Clicks

        public ActionResult StructuredOneClicksTree(int ParentID, int ReportID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.GetAllOneClicks(OrgID, ParentID, ReportID, sDatabase);
                return View(Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetTargetsGrid(int ID, int OrgID)
        {
            List<int> IDs = new List<int>();
            IDs.Add(ID);
            IDs.Add(OrgID);
            return PartialView("_TargetsGrid", IDs);
        }

        public ActionResult GetTargetsList(jQueryDataTableParamModel param, int ID, int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = GenerateQueryRepository.GetTargetsGrid(param, ID, OrgID, iUserID, sOrgName, sDatabase);
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

        public ActionResult Scheduling()
        {
            List<int> IDs = new List<int>();
            IDs.Add(0);
            IDs.Add(0);
            return View("SchedulingList", IDs);
        }

        public ActionResult GetSchedulersGrid(int ID, int OrgID)
        {
            List<int> IDs = new List<int>();
            IDs.Add(ID);
            IDs.Add(OrgID);
            return PartialView("_SchedulersGrid", IDs);
        }

        public ActionResult GetSchedulersList(jQueryDataTableParamModel param, int ID, int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = GenerateQueryRepository.GetSchedulersList(param, ID, OrgID, iUserID, sOrgName, sDatabase);
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

        public ActionResult GetTargetUsers(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.GetTargetUsers(ID, OrgID, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }

        //SaveOneclickNameValuePairs
        [HttpPost]
        public ActionResult SaveOneclickNvs(int OneClickID, string[] NVPairs)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.SaveOneclickNvs(OneClickID, NVPairs, OrgID, iUserID, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //DeleteOneclickNameValuePairs
        public JsonResult DeleteOneclickNvs(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                ModelDbContext dbContext = new ModelDbContext();
                var Result = dbContext.XI1ClickNVs.Find(ID);
                dbContext.XI1ClickNVs.Remove(Result);
                dbContext.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //SaveParametersNameDefaultValuePairs
        [HttpPost]
        public ActionResult SaveParamerterNDVs(int OneClickID, string[] NDVPairs)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.SaveParamerterNDVs(OneClickID, NDVPairs, OrgID, iUserID, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //DeleteParameterNameDefaultValuePairs
        public JsonResult DeleteParameterNDVs(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                ModelDbContext dbContext = new ModelDbContext();
                var Result = dbContext.XI1ClickParameterNDVs.Find(ID);
                dbContext.XI1ClickParameterNDVs.Remove(Result);
                dbContext.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //SaveParametersNameDefaultValuePairs

        [HttpPost]
        public ActionResult Save1ClickLinks(VMReports OneClickXILinks)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.Save1ClickLinks(OneClickXILinks, OrgID, iUserID, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //DeleteParameterNameDefaultValuePairs
        public JsonResult DeleteXI1ClickLinkPair(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                ModelDbContext dbContext = new ModelDbContext();
                var Result = dbContext.XI1ClickLinks.Find(ID);
                dbContext.XI1ClickLinks.Remove(Result);
                dbContext.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult OneClickInsert(int iBODID = 0, int iGroupID = 0, int i1ClickID = 0, int iRecordCount = 0, int iInstanceID = 0, string sGUID = "", string sMode = "", string sOperator = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            string sSessionID = HttpContext.Session.SessionID;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                XID1Click o1ClickD = new XID1Click();
                o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, iBODID.ToString());
                XIIBO oBOI = new XIIBO();
                string sGroupName = string.Empty;
                sGroupName = oBOD.Groups.Values.Where(m => m.ID == iGroupID).Select(m => m.GroupName).FirstOrDefault();
                oBOI.BOD = oBOD;
                if (iInstanceID > 0)
                {
                    XIIXI oXII = new XIIXI();
                    oBOI = oXII.BOI(oBOD.Name, iInstanceID.ToString(), sGroupName);
                    if (!string.IsNullOrEmpty(sGroupName))
                    {
                        var GroupFields = oBOI.BOD.GroupD(sGroupName).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                        if (!string.IsNullOrEmpty(GroupFields))
                        {
                            var oGrpFields = GroupFields.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            oBOI.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName)).ToList().ForEach(m => m.bDirty = true);
                        }
                    }
                    else if (oBOI.Attributes.Values.Count() > 0)
                    {
                        oBOI.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                    }
                }
                else if (!string.IsNullOrEmpty(sGroupName))
                {
                    oBOI.LoadBOI(sGroupName);
                }
                oBOI.iBODID = oBOD.BOID;
                oBOI.BOD = null;
                XIIComponent oCompI = new XIIComponent();
                if (o1ClickD.DisplayAs == 50 || sMode == "grid")
                {
                    List<CNV> oParams = new List<CNV>();
                    XIInfraFormComponent oFormComponent = new XIInfraFormComponent();
                    if (o1ClickD.XIComponent == null && o1ClickD.FKiVisualisationID > 0 && sMode != "grid")
                    {
                        oParams.Add(new CNV { sName = "Visualisation", sValue = o1ClickD.FKiVisualisationID.ToString() });
                    }
                    else
                    {
                        if (o1ClickD.XIComponent != null && o1ClickD.XIComponent.Params != null)
                        {
                            oParams = o1ClickD.XIComponent.Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue }).ToList();
                        }
                    }
                    oParams = (List<CNV>)oCache.ResolveParameters(oParams, sSessionID, sGUID);
                    oParams.Add(new CNV { sName = XIConstant.Param_GUID, sValue = sGUID });
                    oParams.Add(new CNV { sName = XIConstant.Param_SessionID, sValue = sSessionID });
                    oParams.Add(new CNV { sName = XIConstant.Param_Group, sValue = sGroupName });
                    //oParams.Add(new CNV { sName = XIConstant.Param_LockGroup.ToLower(), sValue = "Lock" });
                    oParams.Add(new CNV { sName = XIConstant.Param_BO.ToLower(), sValue = oBOD.Name });
                    oParams.Add(new CNV { sName = "sOperator", sValue = sOperator });
                    if (iInstanceID > 0)
                    {
                        oParams.Add(new CNV { sName = XIConstant.Param_InstanceID, sValue = iInstanceID.ToString() });
                    }
                    var oResult = oFormComponent.XILoad(oParams);
                    if (oResult.bOK && oResult.oResult != null)
                    {
                        XIVisualisation oXIvisual = new XIVisualisation();
                        XIBODisplay oBODisplay = (XIBODisplay)oResult.oResult;
                        if (o1ClickD.FKiVisualisationID != 0)
                        {
                            oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, null, Convert.ToString(o1ClickD.FKiVisualisationID));
                        }
                        else
                        {
                            if (o1ClickD.XIComponent != null)
                            {
                                string sVisualisation = oParams.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, Convert.ToString(o1ClickD.FKiVisualisationID));
                            }
                        }
                        var oXIVisualC = (XIVisualisation)o1ClickD.Clone(oXIvisual);
                        oXIVisualC.XiVisualisationNVs = oXIVisualC.XiVisualisationNVs ?? new List<XIVisualisationNV>();
                        oXIVisualC.XiVisualisationNVs.Add(new XIVisualisationNV() { sName = "ListAdd", sValue = "yes", sType = sGUID });
                        if (sMode == "grid")
                        {
                            oXIVisualC.XiVisualisationNVs.Add(new XIVisualisationNV() { sName = "GridAdd", sValue = "yes" });
                        }
                        oCompI.oVisualisation = new List<XIVisualisation>();
                        oCompI.oVisualisation.Add(oXIVisualC);
                        oCompI.oDefintion = o1ClickD.XIComponent;
                        oCompI.oContent[XIConstant.FormComponent] = oBODisplay;
                    }

                    //oCompI.oDefintion = o1ClickD.XIComponent;
                    //List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                    //if (o1ClickD.XIComponent != null)
                    //{
                    //    string sVisualisation = o1ClickD.XIComponent.Params.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    //    if (!string.IsNullOrEmpty(sVisualisation))
                    //    {
                    //        var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, null);
                    //        var oXIVisualC = (XIVisualisation)o1ClickD.Clone(oXIvisual);
                    //        if (oXIVisualC != null)
                    //        {
                    //            foreach (var oVisualisation in oXIVisualC.XiVisualisationNVs)
                    //            {
                    //                if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("{XIP") >= 0)
                    //                {
                    //                    oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                    //                }
                    //            }
                    //            oXIVisualisations.Add(oXIVisualC);
                    //        }
                    //    }
                    //    oCompI.oVisualisation = oXIVisualisations;
                    //}
                    ViewBag.sGUID = sGUID;
                    return PartialView("~/Views/XIComponents/_FormComponent.cshtml", oCompI);
                }
                else if (o1ClickD.DisplayAs == 110)
                {
                    XIBODisplay oBODisplay = new XIBODisplay();
                    oBODisplay.BOInstance = oBOI;
                    oCompI.oDefintion = o1ClickD.XIComponent;
                    List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                    if (o1ClickD.XIComponent != null)
                    {
                        string sVisualisation = o1ClickD.XIComponent.Params.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                        if (!string.IsNullOrEmpty(sVisualisation))
                        {
                            var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, null);
                            var oXIVisualC = (XIVisualisation)o1ClickD.Clone(oXIvisual);
                            if (oXIVisualC != null)
                            {
                                foreach (var oVisualisation in oXIVisualC.XiVisualisationNVs)
                                {
                                    if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("{XIP") >= 0)
                                    {
                                        oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                    }
                                }
                                oXIVisualisations.Add(oXIVisualC);
                            }
                        }
                        oCompI.oVisualisation = oXIVisualisations;
                    }
                    else if (o1ClickD.FKiVisualisationID > 0)
                    {
                        var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, null, o1ClickD.FKiVisualisationID.ToString());
                        var oXIVisualC = (XIVisualisation)o1ClickD.Clone(oXIvisual);
                        if (oXIVisualC != null)
                        {
                            foreach (var oVisualisation in oXIVisualC.XiVisualisationNVs)
                            {
                                if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("{XIP") >= 0)
                                {
                                    oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                }
                                else if (oBOI.Attributes.ContainsKey(oVisualisation.sName.ToLower()) && string.IsNullOrEmpty(oBOI.AttributeI(oVisualisation.sName.ToLower()).sValue))
                                {
                                    oBOI.Attributes[oVisualisation.sName.ToLower()] = new XIIAttribute { sName = oVisualisation.sName.ToLower(), sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                }
                            }
                            oXIVisualisations.Add(oXIVisualC);
                        }
                        oCompI.oVisualisation = oXIVisualisations;
                    }
                    List<XIIBO> oBOIL = new List<XIIBO>();
                    oBOIL.Add(oBOI);
                    o1ClickD.oDataSet["0"] = oBOI;
                    oCompI.oContent[XIConstant.GridComponent] = o1ClickD;
                    ViewBag.iRecordCount = iRecordCount;

                    return PartialView("~/Views/XIComponents/_GridResult.cshtml", oCompI);
                }
                else if (o1ClickD.DisplayAs == 120 && o1ClickD.XIComponent.sName.ToLower() == XIConstant.FormComponent.ToLower())
                {
                    List<CNV> oParams = new List<CNV>();
                    XIInfraFormComponent oFormComponent = new XIInfraFormComponent();
                    if (o1ClickD.XIComponent == null && o1ClickD.FKiVisualisationID > 0)
                    {
                        oParams.Add(new CNV { sName = "Visualisation", sValue = o1ClickD.FKiVisualisationID.ToString() });
                    }
                    else
                    {
                        if (o1ClickD.XIComponent != null && o1ClickD.XIComponent.Params != null)
                        {
                            oParams = o1ClickD.XIComponent.Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue }).ToList();
                        }
                    }
                    oParams = (List<CNV>)oCache.ResolveParameters(oParams, sSessionID, sGUID);
                    oParams.Add(new CNV { sName = XIConstant.Param_GUID, sValue = sGUID });
                    oParams.Add(new CNV { sName = XIConstant.Param_SessionID, sValue = sSessionID });
                    var isGroup = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_Group.ToLower()).FirstOrDefault();
                    if (isGroup != null)
                    {
                        oParams.Where(m => m.sName.ToLower() == XIConstant.Param_Group.ToLower()).FirstOrDefault().sValue = sGroupName;  //oParams.Add(new CNV { sName = XIConstant.Param_Group, sValue = sGroupName });
                    }
                    else
                    {
                        oParams.Add(new CNV { sName = XIConstant.Param_Group, sValue = sGroupName });
                    }

                    //oParams.Add(new CNV { sName = XIConstant.Param_LockGroup.ToLower(), sValue = "Lock" });
                    if (oParams.Where(s => s.sName.ToLower() == XIConstant.Param_BO.ToLower()).FirstOrDefault() != null)
                    {
                        oParams.Where(s => s.sName.ToLower() == XIConstant.Param_BO.ToLower()).FirstOrDefault().sValue = oBOD.Name;
                    }
                    else
                    {
                        oParams.Add(new CNV { sName = XIConstant.Param_BO.ToLower(), sValue = oBOD.Name });
                    }
                    if (iInstanceID > 0)
                    {
                        oParams.Add(new CNV { sName = XIConstant.Param_InstanceID, sValue = iInstanceID.ToString() });
                    }
                    var oResult = oFormComponent.XILoad(oParams);
                    if (oResult.bOK && oResult.oResult != null)
                    {
                        XIVisualisation oXIvisual = new XIVisualisation();
                        XIBODisplay oBODisplay = (XIBODisplay)oResult.oResult;
                        if (o1ClickD.FKiVisualisationID != 0)
                        {
                            oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, null, Convert.ToString(o1ClickD.FKiVisualisationID));
                        }
                        else
                        {
                            if (o1ClickD.XIComponent != null)
                            {
                                string sVisualisation = oParams.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, Convert.ToString(o1ClickD.FKiVisualisationID));
                            }
                        }
                        oCompI.oVisualisation = new List<XIVisualisation>();
                        oCompI.oVisualisation.Add(oXIvisual);
                        oCompI.oDefintion = o1ClickD.XIComponent;
                        oCompI.oContent[XIConstant.FormComponent] = oBODisplay;
                    }
                    //string sHiddenGroup = o1ClickD.XIComponent.Params.Where(m => m.sName.ToLower() == "HiddenGroup".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    //XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                    //XIDXI oXID = new XIDXI();
                    //XIBODisplay oBODisplay = new XIBODisplay();
                    //var FKAttributes = oBOD.Attributes.Where(m => m.Value.FKiType > 0 && !string.IsNullOrEmpty(m.Value.FKTableName) && m.Value.iOneClickID > 0).ToList();
                    //foreach (var item in FKAttributes)
                    //{
                    //    if (item.Value.iOneClickID > 0)
                    //    {
                    //        string sBODataSource = string.Empty;
                    //        var sTableName = item.Value.FKTableName;
                    //        Dictionary<string, object> Params = new Dictionary<string, object>();
                    //        Params["TableName"] = sTableName;
                    //        string sSelectFields = string.Empty;
                    //        sSelectFields = "Name,BOID,iDataSource,sSize,TableName,sPrimaryKey,sType";
                    //        var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                    //        //var FKBOD = Load_BO(FKBO.Name, FKBO.BOID);
                    //        //var BO = AllBOs.Where(m => m.TableName == sTableName).FirstOrDefault();
                    //        sBODataSource = oXID.GetBODataSource(FKBOD.iDataSource);
                    //        oBOD.Attributes[item.Value.Name.ToLower()].sFKBOSize = FKBOD.sSize;
                    //        oBOD.Attributes[item.Value.Name.ToLower()].sFKBOName = FKBOD.Name;
                    //        if (FKBOD.sSize == "10")//maximum number of results in dropdown -- To Do
                    //        {
                    //            var Con = new XIDBAPI(sBODataSource);
                    //            if (FKBOD.sType != null && FKBOD.sType.ToLower() == "reference")
                    //            {
                    //                string suid = "1click-" + Convert.ToString(item.Value.iOneClickID);
                    //                XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                    //                List<CNV> nParms = new List<CNV>();
                    //                nParms = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                    //                var oResult = oXID.Get_AutoCompleteList(suid, "", nParms);
                    //                List<XIDropDown> FKDDL = new List<XIDropDown>();
                    //                if (oResult.bOK && oResult.oResult != null)
                    //                {
                    //                    var DDL = (List<XIDFieldOptionList>)oResult.oResult;
                    //                    FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                    //                }
                    //                if (oBOI.Attributes.ContainsKey(item.Value.Name.ToLower()))
                    //                {
                    //                    oBOI.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //oBODisplay.BOInstance = oBOI;
                    //if (!string.IsNullOrEmpty(sHiddenGroup))
                    //{
                    //    var GroupFields = oBOI.BOD.GroupD(sHiddenGroup).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                    //    if (!string.IsNullOrEmpty(GroupFields))
                    //    {
                    //        var oGrpFields = GroupFields.ToLower().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    //        oBODisplay.BOInstance.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName.ToLower())).ToList().ForEach(c => c.bIsHidden = true);
                    //    }
                    //}
                    //oCompI.oContent[XIConstant.FormComponent] = oBODisplay;
                    //oCompI.oDefintion = o1ClickD.XIComponent;
                    //if (o1ClickD.XIComponent != null)
                    //{
                    //    List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                    //    string sVisualisation = o1ClickD.XIComponent.Params.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    //    if (!string.IsNullOrEmpty(sVisualisation))
                    //    {
                    //        var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, null);
                    //        var oXIVisualC = (XIVisualisation)o1ClickD.Clone(oXIvisual);
                    //        if (oXIVisualC != null)
                    //        {
                    //            foreach (var oVisualisation in oXIVisualC.XiVisualisationNVs)
                    //            {
                    //                if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("{XIP") >= 0)
                    //                {
                    //                    oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                    //                }
                    //            }
                    //            oXIVisualisations.Add(oXIVisualC);
                    //        }
                    //    }
                    //    oCompI.oVisualisation = oXIVisualisations;
                    //}


                    return PartialView("~/Views/XIComponents/_FormComponent.cshtml", oCompI);
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
        #region Save1ClickPermission
        [HttpPost]
        public ActionResult Save1ClickPermission(int[] NVPairs, int i1ClickID = 0, string sType = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int OrgID = oUser.FKiOrganisationID;
                var Result = GenerateQueryRepository.Save1ClickPermission(NVPairs, i1ClickID, sType, OrgID, iUserID, sDatabase);
                if (sType == "Add")
                {
                    ModelDbContext dbContext = new ModelDbContext();
                    VMReports model = GenerateQueryRepository.GetQueryByID(i1ClickID, sDatabase);
                    var RolesTree = oUser.Role.Get_RolesTree(sDatabase, oUser.FKiOrganisationID);
                    ViewBag.Group = (List<XIInfraRoles>)RolesTree.oResult;
                    model.UserID = iUserID;
                    model.GroupIDs = dbContext.XI1ClickPermissions.Where(m => m.FKi1ClickID == i1ClickID).Select(m => m.FKiRoleID).ToList();
                    return PartialView("_OneClickPermission", model);
                }
                else
                {
                    return Json(Result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Save1ClickPermission
        public ActionResult ChangeFields(string sShowField, string sHideField, string BOID)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, BOID);
            //show fields
            var ShowGroupFields = oBOD.Groups.Where(x => x.Key == sShowField.ToLower()).Select(t => t.Value).FirstOrDefault();
            List<Dictionary<string, XIDAttribute>> oRes = new List<Dictionary<string, XIDAttribute>>();
            Dictionary<string, XIDAttribute> Attributes = new Dictionary<string, XIDAttribute>();
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
            //hidden fields
            var HiddenGroupFields = oBOD.Groups.Where(x => x.Key == sHideField.ToLower()).Select(t => t.Value).FirstOrDefault();
            Attributes = new Dictionary<string, XIDAttribute>();
            var sItems = HiddenGroupFields.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in sItems)
            {
                var sValue = oBOD.Attributes.Where(x => x.Key == item.ToLower()).Select(t => t).First();
                Attributes.Add(sValue.Key, sValue.Value);
            }
            oRes.Add(Attributes);
            return Json(oRes, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult GridInsert(int iBODID = 0, int iGroupID = 0, int i1ClickID = 0, int iRecordCount = 0, int iInstanceID = 0, string sGUID = "", int FKiVisualisationID = 0, string sCondition = "", string sQuery = "", string sLockGroup = "")
        {
            string sDatabase = SessionManager.CoreDatabase;
            string sSessionID = HttpContext.Session.SessionID;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                XID1Click o1ClickD = new XID1Click();
                o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
                var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, null, iBODID.ToString());
                XIIBO oBOI = new XIIBO();
                string sGroupName = string.Empty;
                sGroupName = oBOD.Groups.Values.Where(m => m.ID == iGroupID).Select(m => m.GroupName).FirstOrDefault();
                oBOI.BOD = oBOD;
                XIDAttribute oAttr = new XIDAttribute();
                oAttr.BOID = iBODID;
                var iAttrID = "0";
                if (sCondition.Contains("="))
                {
                    iAttrID = sCondition.Split('=')[1];
                    oAttr.ID = Convert.ToInt32(iAttrID);
                }
                var oAttrD = oAttr.Get_BOAttributeDefinition();
                if (oAttrD.bOK && oAttrD.oResult != null)
                {
                    oAttr = (XIDAttribute)oAttrD.oResult;
                }
                if (iInstanceID > 0)
                {
                    XIIXI oXII = new XIIXI();
                    oBOI = oXII.BOI(oBOD.Name, iInstanceID.ToString(), sGroupName);
                    if (!string.IsNullOrEmpty(sGroupName))
                    {
                        var GroupFields = oBOI.BOD.GroupD(sGroupName).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                        if (!string.IsNullOrEmpty(GroupFields))
                        {
                            var oGrpFields = GroupFields.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            oBOI.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName)).ToList().ForEach(m => m.bDirty = true);
                        }
                    }
                    else if (oBOI.Attributes.Values.Count() > 0)
                    {
                        oBOI.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                    }
                }
                else if (!string.IsNullOrEmpty(sGroupName))
                {
                    oBOI.LoadBOI(sGroupName);
                }
                oBOI.iBODID = oBOD.BOID;
                oBOI.BOD = null;
                XIIComponent oCompI = new XIIComponent();
                if (!string.IsNullOrEmpty(sCondition))
                {
                    o1ClickC.sParentWhere = sCondition;
                }
                if (o1ClickC.Query.Contains("{XIP|"))
                {
                    sQuery = sQuery.Replace("&#39;", "");
                    o1ClickC.Query = sQuery;
                }
                //Dictionary<string, XIIBO> o1ClickRes = o1ClickC.OneClick_Execute();
                int iDisplayAs = 110;
                if (iDisplayAs == 110)
                {
                    XIBODisplay oBODisplay = new XIBODisplay();
                    oBODisplay.BOInstance = oBOI;
                    oCompI.oDefintion = o1ClickC.XIComponent;
                    List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                    if (FKiVisualisationID > 0)
                    {
                        var oRes = o1ClickC.oDataSet.Values.FirstOrDefault();
                        var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, null, FKiVisualisationID.ToString());
                        var oXIDVisual = (XIVisualisation)oXIvisual.GetCopy();
                        if (oXIDVisual != null)
                        {
                            if (o1ClickC.oDataSet.Count() > 0)
                            {
                                foreach (var oVisualisation in oXIDVisual.XiVisualisationNVs)
                                {
                                    if (oRes.Attributes.ContainsKey(oVisualisation.sName.ToLower()) || string.IsNullOrEmpty(oVisualisation.sValue))
                                    {
                                        if (oRes != null)
                                        {
                                            var sValue = oRes.Attributes.Where(m => m.Key.ToLower() == oVisualisation.sName.ToLower()).Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(sValue))
                                            {
                                                oBOI.Attributes[oVisualisation.sName.ToLower()] = new XIIAttribute { sName = oVisualisation.sName, sValue = sValue.ToString(), sPreviousValue = sValue.ToString(), bDirty = true };
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oVisualisation in oXIDVisual.XiVisualisationNVs)
                                {
                                    if (oVisualisation.sValue.StartsWith("merge-"))
                                    {
                                        var Field = oVisualisation.sValue;
                                        var Data = Field.Replace("merge-", "");
                                        if (Data.ToLower().StartsWith("{xip|"))
                                        {
                                            var Value = oCache.Get_ParamVal(sSessionID, sGUID, null, Data);
                                            oBOI.Attributes[oVisualisation.sName.ToLower()].sValue = Value;
                                        }
                                        else
                                        {
                                            oBOI.Attributes[oVisualisation.sName.ToLower()].sValue = Data;
                                        }
                                    }
                                    else if (oVisualisation.sValue.ToLower().StartsWith("lock") && oBOI.Attributes.ContainsKey(oVisualisation.sName.ToLower()))
                                    {
                                        oBOI.Attributes[oVisualisation.sName.ToLower()].bLock = true;
                                    }
                                }
                            }
                        }
                        oXIVisualisations.Add(oXIDVisual);
                    }
                    var IDEParams = oCache.Get_Paramobject(sSessionID, sGUID, null, "-listinstance");
                    if (IDEParams.nSubParams != null && IDEParams.nSubParams.Count() > 0)
                    {
                        var OptionParams = IDEParams.nSubParams;
                        if (oBOD.Name == "XIBOOptionList")
                        {
                            var OptionAttr = OptionParams.Where(m => m.sName == "OptionAttr").Select(m => m.sValue).FirstOrDefault();
                            var OptionFieldID = OptionParams.Where(m => m.sName == "OptionFieldID").Select(m => m.sValue).FirstOrDefault();
                            var OptionBOID = OptionParams.Where(m => m.sName == "OptionBOID").Select(m => m.sValue).FirstOrDefault();
                            oBOI.SetAttribute("bofieldid", OptionFieldID);
                            oBOI.SetAttribute("name", OptionAttr);
                            oBOI.SetAttribute("boid", OptionBOID);
                        }
                        else
                        {
                            var FKCol = OptionParams.Where(m => m.sName == "ParentFKColumn").Select(m => m.sValue).FirstOrDefault();
                            var FKValue = OptionParams.Where(m => m.sName == "ParentInsID").Select(m => m.sValue).FirstOrDefault();
                            if (!string.IsNullOrEmpty(FKCol) && !string.IsNullOrEmpty(FKValue))
                            {
                                var AttrI = oBOI.AttributeI(FKCol);
                                if (AttrI != null)
                                {
                                    oBOI.Attributes.Values.ToList().Where(m => m.sName.ToLower() == FKCol.ToLower()).FirstOrDefault().sValue = FKValue;
                                }
                                else
                                {
                                    oBOI.SetAttribute(FKCol, FKValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        var parentparams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                        string sActiveBO = parentparams.NMyInstance.Where(m => m.Key == "{XIP|sBOName}").Select(m => m.Value.sValue).FirstOrDefault();
                        if (!string.IsNullOrEmpty(sActiveBO))
                        {
                            var FKName = oBOD.Attributes.Values.Where(m => m.sFKBOName == sActiveBO).Select(m => m.Name).FirstOrDefault();
                            var FKValue = parentparams.NMyInstance.Where(m => m.Key.ToLower() == ("{XIP|" + sActiveBO + ".id}").ToLower()).Select(m => m.Value.sValue).FirstOrDefault();
                            if (!string.IsNullOrEmpty(FKName) && !string.IsNullOrEmpty(FKValue))
                            {
                                var AttrI = oBOI.AttributeI(FKName);
                                if (AttrI != null)
                                {
                                    oBOI.Attributes.Values.ToList().Where(m => m.sName.ToLower() == FKName.ToLower()).FirstOrDefault().sValue = FKValue;
                                }
                                else
                                {
                                    oBOI.SetAttribute(FKName, FKValue);
                                }
                            }
                        }
                        
                    }
                    if (!string.IsNullOrEmpty(sLockGroup))
                    {
                        o1ClickC.sLockGroup = sLockGroup;
                        var GroupFields = oBOD.GroupD(sLockGroup).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                        if (!string.IsNullOrEmpty(GroupFields))
                        {
                            var oGrpFields = GroupFields.ToLower().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            oBOI.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName.ToLower())).ToList().ForEach(c => c.bLock = true);
                        }
                    }
                    List<XIIBO> oBOIL = new List<XIIBO>();
                    oBOIL.Add(oBOI);
                    o1ClickC.oDataSet["0"] = oBOI;
                    oCompI.oContent[XIConstant.GridComponent] = o1ClickC;
                    oCompI.oVisualisation = oXIVisualisations;
                    ViewBag.iRecordCount = iRecordCount;
                    return PartialView("~/Views/XIComponents/_GridResult.cshtml", oCompI);
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
        public ActionResult DeleteGridRow(int iBODID, int iBOIID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Soft deleting the record";//expalin about this method logic
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                oTrace.oParams.Add(new CNV { sName = "iBODID", sValue = iBODID.ToString() });
                oTrace.oParams.Add(new CNV { sName = "iBOIID", sValue = iBOIID.ToString() });
                string sStatus = string.Empty;
                if (iBODID > 0 && iBOIID > 0)
                {
                    XIInfraCache oCache = new XIInfraCache();
                    var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID.ToString());
                    XIIBO oBOI = new XIIBO();
                    XIIXI oXI = new XIIXI();
                    oBOI = oXI.BOI(BOD.Name, iBOIID.ToString());
                    if (oBOI != null && oBOI.Attributes.Count() > 0)
                    {
                        oBOI.BOD = BOD;
                        oCR = oBOI.Delete(oBOI);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            sStatus = "Success";
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
                watch.Stop();
                oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
                oCResult.oTrace = oTrace;
                return Json(sStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}