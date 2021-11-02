using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XIDNA.Models;
using XIDNA.ViewModels;
using System.Data;
using System.Data.SqlClient;
using XIDNA.Repository;
using XIDNA.Common;
using Microsoft.AspNet.Identity;
using XICore;
using XISystem;
using System.Net;

namespace XIDNA.Controllers
{
    [Authorize]
    [SessionTimeout]
    public class XISemanticsController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IXISemanticsRepository XISemanticsRepository;

        public XISemanticsController() : this(new XISemanticsRepository()) { }

        public XISemanticsController(IXISemanticsRepository XISemanticsRepository)
        {
            this.XISemanticsRepository = XISemanticsRepository;
        }
        XIInfraUsers oUser = new XIInfraUsers();
        CommonRepository Common = new CommonRepository();
        XIInfraCache oCache = new XIInfraCache();
        XIComponentsRepository oXIComponet = new XIComponentsRepository();
        XIConfigs oConfig = new XIConfigs();

        #region XISemantics

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetXISemanticsDetails(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var result = XISemanticsRepository.GetXISemanticsDetails(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddXISemantics(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID;
                cQSDefinition oXISemantic = new cQSDefinition();
                if (ID > 0)
                {
                    oXISemantic = XISemanticsRepository.EditXISemanticsByID(ID, iUserID, sOrgName, sDatabase);
                }
                oXISemantic.ddlXIVisualisations = Common.GetXIVisualisationsDDL(iUserID, sOrgName, sDatabase);
                oXISemantic.ddlXIParameters = Common.GetXiParametersDDL(iUserID, sOrgName, sDatabase);
                oXISemantic.ddlXIStructures = Common.GetXIBOStructuresDDL(iUserID, sOrgName, sDatabase);
                oXISemantic.ddlSourceList = Common.GetSourceDDL(iUserID, sOrgName, sDatabase);
                if (fkiApplicationID == 0)
                {
                    oXISemantic.ddlApplications = Common.GetApplicationsDDL();
                }
                oXISemantic.ddlLayouts = Common.GetLayoutsDDL(iUserID, sOrgName, sDatabase);
                if (fkiApplicationID > 0)
                {
                    oXISemantic.FKiApplicationID = fkiApplicationID;
                }
                if (oXISemantic.FKiParameterID > 0 || oXISemantic.FKiBOStructureID > 0)
                {
                    oXISemantic.bIsContextObject = true;
                }
                return View(oXISemantic);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult CreateXISemantics(cQSDefinition XISmtcs)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XISmtcs.CreatedBy = SessionManager.UserID;
                XISmtcs.CreatedTime = DateTime.Now;
                XISmtcs.UpdatedBy = SessionManager.UserID;
                XISmtcs.UpdatedTime = DateTime.Now;
                VMCustomResponse Status = XISemanticsRepository.CreateXISemantics(XISmtcs, iUserID, sOrgName, sDatabase);
                return Json(Status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        //SaveQuestionSet
        [HttpPost]
        public ActionResult SaveQuestionSet(XIDQS oQSDef)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var UserDetais = Common.GetUserDetails(iUserID, sOrgName, sDatabase);
                var FKiAppID = Common.GetUserDetails(iUserID, sOrgName, sDatabase).FKiApplicationID;
                var iOrgID = UserDetais.FKiOrgID;
                XIIBO XIScs = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSDefinition");
                XIScs.BOD = oBOD;
                XIScs.SetAttribute("ID", oQSDef.ID.ToString());
                XIScs.SetAttribute("FKiApplicationID", oQSDef.FKiApplicationID.ToString());
                XIScs.SetAttribute("FKiOrganisationID", iOrgID.ToString());
                XIScs.SetAttribute("sName", oQSDef.sName.ToString());
                XIScs.SetAttribute("SaveType", oQSDef.SaveType.ToString());
                XIScs.SetAttribute("sDescription", oQSDef.sDescription.ToString());
                XIScs.SetAttribute("sMode", oQSDef.sMode.ToString());
                XIScs.SetAttribute("iVisualisationID", oQSDef.iVisualisationID.ToString());
                XIScs.SetAttribute("iLayoutID", oQSDef.iLayoutID.ToString());
                XIScs.SetAttribute("FKiSourceID", oQSDef.FKiSourceID.ToString());
                //XIScs.SetAttribute("iThemeID", oQSDef.iThemeID.ToString());
                if (oQSDef.bIsContextObject == true)
                {
                    XIScs.SetAttribute("FKiParameterID", oQSDef.FKiParameterID.ToString());
                    XIScs.SetAttribute("FKiBOStructureID", oQSDef.FKiBOStructureID.ToString());
                }
                else
                {
                    XIScs.SetAttribute("FKiParameterID", "0");
                    XIScs.SetAttribute("FKiBOStructureID", "0");
                }
                XIScs.SetAttribute("FKiOriginID", oQSDef.FKiOriginID.ToString());
                XIScs.SetAttribute("StatusTypeID", oQSDef.StatusTypeID.ToString());
                if (oQSDef.ID == 0)
                {
                    XIScs.SetAttribute("CreatedBy", iUserID.ToString());
                    XIScs.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    XIScs.SetAttribute("CreatedTime", DateTime.Now.ToString());
                }
                XIScs.SetAttribute("UpdatedBy", oQSDef.iVisualisationID.ToString());
                XIScs.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                XIScs.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                var XiBO = XIScs.Save(XIScs);
                //VMCustomResponse Status = XISemanticsRepository.CreateXISemantics(XISmtcs, iUserID, sOrgName, sDatabase);
                var Status = Common.ResponseMessage();
                return Json(Status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult DeleteXISemanticsDetailsByID(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                int iStatus = XISemanticsRepository.DeleteXISemanticsDetailsByID(ID, iUserID, sOrgName, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetSemanticDetails(int iXISemanticID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                cQSDefinition oXISemantic = new cQSDefinition();
                oXISemantic = XISemanticsRepository.GetXISemanticsByID(iXISemanticID, iUserID, sOrgName, sDatabase);
                return Json(oXISemantic, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }


        #endregion XISemantics

        #region XISemanticsSteps

        public ActionResult GridStepDetails(int iXISemanticID = 0)
        {
            return View(iXISemanticID);
        }

        public ActionResult GetStepDetails(jQueryDataTableParamModel param, int iXISemanticID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GetStepDetails(param, iXISemanticID, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddXISemanticsSteps(int iXISemanticID = 0, int ID = 0)
        {
            cQSStepDefiniton model = new cQSStepDefiniton();
            string sDatabase = SessionManager.CoreDatabase;
            string sClientDB = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                model.FKiQSDefintionID = iXISemanticID;
                if (ID > 0)
                {
                    model = XISemanticsRepository.EditXISemanticsStepsByID(ID, iUserID, sOrgName, sDatabase);
                }
                //model.ddlXILinks = Common.GetXiLinksDDL(sDatabase);
                //model.ddlXIComponents = Common.GetXIComponentsDDL(sDatabase);
                model.ddlContent = new List<VMDropDown>();
                model.XIFields = XISemanticsRepository.GetXIFields(iUserID, sOrgName, sDatabase);
                model.XILinks = XISemanticsRepository.GetXILinks(iUserID, sOrgName, sDatabase);
                model.XICodes = XISemanticsRepository.GetQSXiLinkCodes(iUserID, sOrgName, sDatabase);
                model.XIFieldValues = XISemanticsRepository.XIFieldValues(ID, iUserID, sOrgName, sDatabase);
                model.ddlQuoteStage = XISemanticsRepository.GetQuoteStages(iUserID, sOrgName, sDatabase);
                if (model.ID == 0 || model.QSNavigations == null || model.QSNavigations.Count() == 0)
                {
                    var Steps = new List<VMDropDown>();
                    Steps = XISemanticsRepository.AddXISemanticsNavigations(ID, iUserID, sOrgName, sDatabase);
                    var Navs = new List<cQSNavigations>();
                    Navs.Add(new cQSNavigations { });
                    model.QSNavigations = Navs;
                    model.QSNavigations[0].SematicSteps = Steps;
                }
                if (model.Sections == null)
                {
                    model.Sections = new List<cStepSectionDefinition>();
                }
                model.ddlLayouts = Common.GetLayoutsDDL(iUserID, sOrgName, sDatabase);
                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateXISemanticsSteps(cQSStepDefiniton XISmtcsStps)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XISmtcsStps.CreatedBy = SessionManager.UserID;
                XISmtcsStps.CreatedTime = DateTime.Now;
                XISmtcsStps.UpdatedBy = SessionManager.UserID;
                XISmtcsStps.UpdatedTime = DateTime.Now;
                VMCustomResponse Status = XISemanticsRepository.CreateXISemanticsSteps(XISmtcsStps, iUserID, sOrgName, sDatabase);
                return Json(Status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //SaveQuestionSetSteps
        [HttpPost, ValidateInput(false)]
        public ActionResult SaveQuestionSetSteps(XIDQSStep oQSStep)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var UserDetais = Common.GetUserDetails(iUserID, sOrgName, sDatabase);
                var FKiAppID = Common.GetUserDetails(iUserID, sOrgName, sDatabase).FKiApplicationID;
                var iOrgID = UserDetais.FKiOrgID;
                XIIBO XIScs = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSStepDefinition");
                XIScs.BOD = oBOD;
                XIScs.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = oQSStep.ID.ToString(), bDirty = true };
                XIScs.Attributes["FKiQSDefintionID".ToLower()] = new XIIAttribute { sName = "FKiQSDefintionID", sValue = oQSStep.FKiQSDefintionID.ToString(), bDirty = true };
                XIScs.Attributes["sName".ToLower()] = new XIIAttribute { sName = "sName", sValue = oQSStep.sName, bDirty = true };
                XIScs.Attributes["sDisplayName".ToLower()] = new XIIAttribute { sName = "sDisplayName", sValue = oQSStep.sDisplayName, bDirty = true };
                XIScs.Attributes["iOrder".ToLower()] = new XIIAttribute { sName = "iOrder", sValue = oQSStep.iOrder.ToString(), bDirty = true };
                XIScs.Attributes["sCode".ToLower()] = new XIIAttribute { sName = "sCode", sValue = oQSStep.sCode, bDirty = true };
                XIScs.Attributes["iDisplayAs".ToLower()] = new XIIAttribute { sName = "iDisplayAs", sValue = oQSStep.iDisplayAs.ToString(), bDirty = true };
                if (oQSStep.iDisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.XIComponent.ToString()))
                {
                    XIScs.Attributes["iXIComponentID".ToLower()] = new XIIAttribute { sName = "iXIComponentID", sValue = oQSStep.FKiContentID.ToString(), bDirty = true };
                    XIScs.Attributes["FKiContentID".ToLower()] = new XIIAttribute { sName = "FKiContentID", sValue = oQSStep.FKiContentID.ToString(), bDirty = true };
                    XIScs.Attributes["i1ClickID".ToLower()] = new XIIAttribute { sName = "i1ClickID", sValue = "0", bDirty = true };
                }
                else if (oQSStep.iDisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.OneClick.ToString()))
                {
                    XIScs.Attributes["i1ClickID".ToLower()] = new XIIAttribute { sName = "i1ClickID", sValue = oQSStep.FKiContentID.ToString(), bDirty = true };
                    XIScs.Attributes["iXIComponentID".ToLower()] = new XIIAttribute { sName = "iXIComponentID", sValue = "0", bDirty = true };
                    XIScs.Attributes["FKiContentID".ToLower()] = new XIIAttribute { sName = "FKiContentID", sValue = "0", bDirty = true };
                }
                else if (oQSStep.iDisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.Html.ToString()))
                {
                    XIScs.Attributes["HTMLContent".ToLower()] = new XIIAttribute { sName = "HTMLContent", sValue = oQSStep.HTMLContent.ToString(), bDirty = true };
                }
                if (oQSStep.bIsHidden)
                {
                    XIScs.Attributes["sIsHidden".ToLower()] = new XIIAttribute { sName = "sIsHidden", sValue = "on", bDirty = true };
                }
                else
                {
                    XIScs.Attributes["sIsHidden".ToLower()] = new XIIAttribute { sName = "sIsHidden", sValue = "off", bDirty = true };
                }
                XIScs.Attributes["bInMemoryOnly".ToLower()] = new XIIAttribute { sName = "bInMemoryOnly", sValue = oQSStep.bInMemoryOnly.ToString(), bDirty = true };
                XIScs.Attributes["bIsSaveNext".ToLower()] = new XIIAttribute { sName = "bIsSaveNext", sValue = oQSStep.bIsSaveNext.ToString(), bDirty = true };
                XIScs.Attributes["bIsSave".ToLower()] = new XIIAttribute { sName = "bIsSave", sValue = oQSStep.bIsSave.ToString(), bDirty = true };
                XIScs.Attributes["bIsBack".ToLower()] = new XIIAttribute { sName = "bIsBack", sValue = oQSStep.bIsBack.ToString(), bDirty = true };
                if (oQSStep.bIsSaveNext == true)
                {
                    XIScs.Attributes["sSaveBtnLabel".ToLower()] = new XIIAttribute { sName = "sSaveBtnLabel", sValue = oQSStep.sSaveBtnLabelSaveNext.ToString(), bDirty = true };
                }
                else
                {
                    XIScs.Attributes["sSaveBtnLabel".ToLower()] = new XIIAttribute { sName = "sSaveBtnLabel", sValue = null, bDirty = true };
                }
                if (oQSStep.bIsSave == true)
                {
                    XIScs.Attributes["sSaveBtnLabel".ToLower()] = new XIIAttribute { sName = "sSaveBtnLabel", sValue = oQSStep.sSaveBtnLabelSave.ToString(), bDirty = true };
                }
                else
                {
                    XIScs.Attributes["sSaveBtnLabel".ToLower()] = new XIIAttribute { sName = "sSaveBtnLabel", sValue = null, bDirty = true };
                }
                if (oQSStep.bIsBack == true)
                {
                    XIScs.Attributes["sBackBtnLabel".ToLower()] = new XIIAttribute { sName = "sBackBtnLabel", sValue = oQSStep.sBackBtnLabel.ToString(), bDirty = true };
                }
                else
                {
                    XIScs.Attributes["sBackBtnLabel".ToLower()] = new XIIAttribute { sName = "sBackBtnLabel", sValue = null, bDirty = true };
                }
                XIScs.Attributes["bIsContinue".ToLower()] = new XIIAttribute { sName = "bIsContinue", sValue = oQSStep.bIsContinue.ToString(), bDirty = true };
                XIScs.Attributes["bIsHistory".ToLower()] = new XIIAttribute { sName = "bIsHistory", sValue = oQSStep.bIsHistory.ToString(), bDirty = true };
                XIScs.Attributes["bIsCopy".ToLower()] = new XIIAttribute { sName = "bIsCopy", sValue = oQSStep.bIsCopy.ToString(), bDirty = true };
                XIScs.Attributes["XILinkID".ToLower()] = new XIIAttribute { sName = "XILinkID", sValue = "0", bDirty = true };
                XIScs.Attributes["iLayoutID".ToLower()] = new XIIAttribute { sName = "iLayoutID", sValue = oQSStep.iLayoutID.ToString(), bDirty = true };
                XIScs.Attributes["OrganisationID".ToLower()] = new XIIAttribute { sName = "OrganisationID", sValue = iOrgID.ToString(), bDirty = true };
                XIScs.Attributes["FKiApplicationID".ToLower()] = new XIIAttribute { sName = "FKiApplicationID", sValue = FKiAppID.ToString(), bDirty = true };
                XIScs.Attributes["StatusTypeID".ToLower()] = new XIIAttribute { sName = "StatusTypeID", sValue = oQSStep.StatusTypeID.ToString(), bDirty = true };
                if (oQSStep.ID == 0)
                {
                    XIScs.Attributes["CreatedBy".ToLower()] = new XIIAttribute { sName = "CreatedBy", sValue = iUserID.ToString(), bDirty = true };
                    XIScs.Attributes["CreatedBySYSID".ToLower()] = new XIIAttribute { sName = "CreatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                    XIScs.Attributes["CreatedTime".ToLower()] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                }
                XIScs.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = iUserID.ToString(), bDirty = true };
                XIScs.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                XIScs.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                var XiBO = XIScs.Save(XIScs);
                //VMCustomResponse Status = XISemanticsRepository.CreateXISemantics(XISmtcs, iUserID, sOrgName, sDatabase);
                var Status = Common.ResponseMessage();
                return Json(Status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult DeleteXISemanticsStepsDetailsByID(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                int iStatus = XISemanticsRepository.DeleteXISemanticsStepsDetailsByID(ID, iUserID, sOrgName, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveSectionFields(int StepID = 0, string[] SecNVPairs = null, int DisplayAs = 0, int SecID = 0, string SectionName = "", bool bIsGroup = false, string sGroupDescription = "", string sGroupLabel = "", bool bIsHidden = false, decimal iOrder = 0, string sSectionCode = "", string[] QSCodes = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                cStepSectionDefinition model = new cStepSectionDefinition();
                model.FKiStepDefinitionID = StepID;
                model.SecNVPairs = SecNVPairs;
                model.QSCodes = QSCodes;
                model.iDisplayAs = DisplayAs;
                model.ID = SecID;
                model.sName = SectionName;
                model.bIsGroup = bIsGroup;
                model.sGroupDescription = sGroupDescription;
                model.sGroupLabel = sGroupLabel;
                model.bIsHidden = bIsHidden;
                model.iOrder = iOrder;
                model.sCode = sSectionCode;
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var Response = XISemanticsRepository.SaveSectionFields(model, iUserID, sOrgName, sDatabase);
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        [HttpPost]
        public ActionResult SaveSectionContent(int StepID = 0, int DisplayAs = 0, int ContentID = 0, int SecID = 0, string SectionName = "", string sParams = null, bool bIsHidden = false, decimal iOrder = 0, string sSectionCode = null, string[] QSCodes = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var Response = XISemanticsRepository.SaveSectionContent(StepID, DisplayAs, ContentID, SecID, SectionName, sParams, bIsHidden, iOrder, sSectionCode, QSCodes, iUserID, sOrgName, sDatabase);
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        [HttpPost]
        public ActionResult SaveSectionHTMLContent(int StepID = 0, int DisplayAs = 0, string sHTMLContent = "", int SecID = 0, string SectionName = "", bool bIsHidden = false, decimal iOrder = 0, string sSectionCode = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var Response = XISemanticsRepository.SaveSectionHTMLContent(StepID, DisplayAs, sHTMLContent, SecID, SectionName, bIsHidden, iOrder, sSectionCode, iUserID, sOrgName, sDatabase);
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        [HttpPost]
        public ActionResult GetSectionContentDialog(int StepDefID = 0, int SectionID = 0, int DisplayAs = 0, int SecID = 0, string SectionName = "",string Type="")
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                ViewBag.SectionID = SectionID;
                ViewBag.DisplayAs = DisplayAs;
                ViewBag.SecID = SecID;
                ViewBag.SectionName = SectionName;
                cQSStepDefiniton oStepDef = new cQSStepDefiniton();
                if (StepDefID > 0)
                {
                    if(Type == "IDE")
                    {
                        oStepDef = XISemanticsRepository.EditXISemanticsSectionByID(SectionID, iUserID, sOrgName, sDatabase);
                    }
                    else
                    {
                        oStepDef = XISemanticsRepository.EditXISemanticsStepsByID(StepDefID, iUserID, sOrgName, sDatabase);
                    }
                }
                cStepSectionDefinition oSection = new cStepSectionDefinition();
                if (DisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.Fields.ToString()))
                {
                    var ComponentsDDL = Common.PopulateDDL("XI Component", iUserID, sOrgName, sDatabase);
                    if (SectionID > 0)
                    {
                        if (StepDefID > 0)
                        {
                            oSection = oStepDef.Sections.Where(m => m.ID == SectionID).FirstOrDefault();
                        }
                        else
                        {
                            oSection = XISemanticsRepository.ShowXISemanticsStepBySecID(StepDefID, SectionID, DisplayAs, iUserID, sOrgName, sDatabase);
                        }
                        //oSection.QSLink = XISemanticsRepository.XILinkValues(SectionID, iUserID, sOrgName, sDatabase);
                        oSection.QSLinkCodes = XISemanticsRepository.XILinkCodes(SectionID, iUserID, sOrgName, sDatabase);
                        oSection.ddlXIComponents = ComponentsDDL;
                    }
                }
                else if (DisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.Sections.ToString()))
                {
                    var ComponentsDDL = Common.PopulateDDL("XI Component", iUserID, sOrgName, sDatabase);
                    if (SectionID > 0)
                    {
                        if (StepDefID > 0)
                        {
                            oSection = oStepDef.Sections.Where(m => m.ID == SectionID).FirstOrDefault();
                        }
                        else
                        {
                            oSection = XISemanticsRepository.ShowXISemanticsStepBySecID(StepDefID, SectionID, DisplayAs, iUserID, sOrgName, sDatabase);
                        }
                        //oSection.QSLink = XISemanticsRepository.XILinkValues(SectionID, iUserID, sOrgName, sDatabase);
                        //oSection.QSLinkCodes = XISemanticsRepository.XILinkCodes(SectionID, iUserID, sOrgName, sDatabase);
                        //oSection.ddlXIComponents = ComponentsDDL;
                        //XIDXI oXID = new XIDXI();
                        //var oCR = oXID.Get_QSSectionsAll();
                        //if(oCR.bOK && oCR.oResult != null)
                        //{
                        //    List<XIDQSSection> Secs = new List<XIDQSSection>();
                        //    Secs = (List<XIDQSSection>)oCR.oResult;
                        //    var ddlSections = Secs.Select(m => new VMDropDown { ID = m.ID, text = m.sName }).ToList();
                        //    oSection.ddlSections = ddlSections;
                        //}
                    }
                }
                else if (DisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.XIComponent.ToString()))
                {
                    var ComponentsDDL = Common.PopulateDDL("XI Component", iUserID, sOrgName, sDatabase);
                    if (SectionID > 0)
                    {
                        if (StepDefID > 0)
                        {
                            oSection = oStepDef.Sections.Where(m => m.ID == SectionID).FirstOrDefault();
                        }
                        else
                        {
                            oSection = XISemanticsRepository.ShowXISemanticsStepBySecID(StepDefID, SectionID, DisplayAs, iUserID, sOrgName, sDatabase);
                        }
                        //oSection.QSLink = XISemanticsRepository.XILinkValues(SectionID, iUserID, sOrgName, sDatabase);
                        oSection.QSLinkCodes = XISemanticsRepository.XILinkCodes(SectionID, iUserID, sOrgName, sDatabase);
                    }
                    oSection.ddlXIComponents = ComponentsDDL;
                }
                else if (DisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.OneClick.ToString()))
                {
                    var OneClicksDDL = Common.PopulateDDL("1-Click", iUserID, sOrgName, sDatabase);
                    if (SectionID > 0)
                    {
                        if (StepDefID > 0)
                        {
                            oSection = oStepDef.Sections.Where(m => m.ID == SectionID).FirstOrDefault();
                        }
                        else
                        {
                            oSection = XISemanticsRepository.ShowXISemanticsStepBySecID(StepDefID, SectionID, DisplayAs, iUserID, sOrgName, sDatabase);
                        }
                    }
                    oSection.ddlOneClicks = OneClicksDDL;
                }
                else if (DisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.Html.ToString()))
                {
                    var ComponentsDDL = Common.PopulateDDL("XI Component", iUserID, sOrgName, sDatabase);
                    if (SectionID > 0)
                    {
                        if (StepDefID > 0)
                        {
                            oSection = oStepDef.Sections.Where(m => m.ID == SectionID).FirstOrDefault();
                        }
                        else
                        {
                            oSection = XISemanticsRepository.ShowXISemanticsStepBySecID(StepDefID, SectionID, DisplayAs, iUserID, sOrgName, sDatabase);
                        }
                        //oSection.QSLink = XISemanticsRepository.XILinkValues(SectionID, iUserID, sOrgName, sDatabase);
                        oSection.QSLinkCodes = XISemanticsRepository.XILinkCodes(SectionID, iUserID, sOrgName, sDatabase);
                        oSection.ddlXIComponents = ComponentsDDL;
                    }
                }
                if (oSection.sIsHidden == "on")
                {
                    oSection.bIsHidden = true;
                }
                else if (oSection.sIsHidden == "off")
                {
                    oSection.bIsHidden = false;
                }
                return PartialView("_StepSections", oSection);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult DeleteSectionBySectionID(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Result = XISemanticsRepository.DeleteSectionBySectionID(ID, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult DeleteSectionFieldsBySectionID(int ID, int FieldID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Result = XISemanticsRepository.DeleteSectionFieldsBySectionID(ID, FieldID, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        #endregion XISemanticsSteps

        #region XISemanticsNavigation

        public ActionResult GridNavigationDetails(int iXISemanticID = 0)
        {
            return View(iXISemanticID);
        }

        public ActionResult GetNavigationDetails(jQueryDataTableParamModel param, int iXISemanticID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GetNavigationDetails(param, iXISemanticID, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddXISemanticsNavigation(int ID = 0)
        {
            cQSNavigations model = new cQSNavigations();
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            if (ID > 0)
            {
                model = XISemanticsRepository.EditXISemanticsNavigationByID(ID, iUserID, sOrgName, sDatabase);
            }
            List<VMDropDown> semantics = new List<VMDropDown>();
            semantics = XISemanticsRepository.AddXISemanticsNavigations(ID, iUserID, sOrgName, sDatabase);
            model.SematicSteps = semantics;
            return View("AddXISemanticsNavigation", model);
        }
        [HttpPost]
        public ActionResult CreateXISemanticsNavigation(cQSStepDefiniton XISmtcsNav)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XISmtcsNav.CreatedBy = iUserID;
                XISmtcsNav.CreatedTime = DateTime.Now;
                XISmtcsNav.UpdatedBy = iUserID;
                XISmtcsNav.UpdatedTime = DateTime.Now;
                VMCustomResponse Status = XISemanticsRepository.CreateXISemanticsNavigation(XISmtcsNav, iUserID, sOrgName, sDatabase);
                return Json(Status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult DeleteXISemanticsNavigationDetailsByID(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                int iStatus = XISemanticsRepository.DeleteXISemanticsNavigationDetailsByID(ID, iUserID, sOrgName, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #endregion XISemanticsNavigation


        public ActionResult GetStepContent(int iStepID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var oStep = XISemanticsRepository.GetStepDetailsByID(iStepID, iUserID, sOrgName, sDatabase);
                if (oStep.FKiContentID > 0)
                {
                    if (oStep.iDisplayAs == (Int32)Enum.Parse(typeof(EnumSemanticsDisplayAs), EnumSemanticsDisplayAs.XIComponent.ToString()))
                    {
                        return RedirectToAction("XIInitialise", "XIComponents", new { iXIComponentID = oStep.FKiContentID });
                    }
                }
                return PartialView("");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        public ActionResult PopulateDDL(string sDDLType)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var DDL = Common.PopulateDDL(sDDLType, iUserID, sOrgName, sDatabase);
                return Json(DDL, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        #region QuestionSet

        public ActionResult QuestionSetFieldList()
        {
            return View("GridQSField");
        }

        public ActionResult GetQSFieldsList(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var result = XISemanticsRepository.GetQSFieldsList(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddEditQuestionSetField(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID; var iOrgID = oUser.FKiOrganisationID;
                if (ID == 0)
                {
                    cFieldOrigin oQSField = new cFieldOrigin();
                    oQSField.ddlDataTypes = Common.GetXIDataTypesDDL(sDatabase, iUserID, sOrgName);
                    oQSField.ddlOneClicks = XISemanticsRepository.GetOneClicks(sDatabase);
                    oQSField.ddlMasterTypes = Common.GetSystemTypesDDL(sDatabase);
                    oQSField.ddlBOs = XISemanticsRepository.GetDDLBOs();
                    if (fkiApplicationID == 0)
                    {
                        oQSField.ddlApplications = Common.GetApplicationsDDL();
                    }
                    oQSField.FKiApplicationID = fkiApplicationID;
                    oQSField.FKiOrganisationID = iOrgID;
                    oQSField.XIFields = XISemanticsRepository.GetXIFields(iUserID, sOrgName, sDatabase);
                    return View("_AddEditQSField", oQSField);
                }
                else
                {
                    cFieldOrigin oQSField = new cFieldOrigin();
                    oQSField = XISemanticsRepository.GetQSFieldByID(ID, iUserID, sOrgName, sDatabase);
                    oQSField.ddlDataTypes = Common.GetXIDataTypesDDL(sDatabase, iUserID, sOrgName);
                    oQSField.ddlOneClicks = XISemanticsRepository.GetOneClicks(sDatabase);
                    oQSField.ddlMasterTypes = Common.GetSystemTypesDDL(sDatabase);
                    oQSField.ddlBOs = XISemanticsRepository.GetDDLBOs();
                    if (fkiApplicationID == 0)
                    {
                        oQSField.ddlApplications = Common.GetApplicationsDDL();
                    }
                    oQSField.FKiApplicationID = fkiApplicationID;
                    oQSField.FKiOrganisationID = iOrgID;
                    oQSField.XIFields = XISemanticsRepository.GetXIFields(iUserID, sOrgName, sDatabase);
                    return View("_AddEditQSField", oQSField);
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
        public ActionResult SaveQSField(cFieldOrigin model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sOrgName = SessionManager.OrganisationName;
                model.CreatedBy = model.UpdatedBy = SessionManager.UserID;
                model.CreatedTime = model.UpdatedTime = DateTime.Now;
                var oResponse = XISemanticsRepository.SaveQSField(model, SessionManager.UserID, sOrgName, sDatabase);
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        //Save_FieldOrigin
        [HttpPost]
        public ActionResult Save_FieldOrigin(cFieldOrigin model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                ModelDbContext CoredbContext = new ModelDbContext();
                string sOrgName = SessionManager.OrganisationName;
                int iUserID = SessionManager.UserID;
                var UserDetais = Common.GetUserDetails(iUserID, sOrgName, sDatabase);
                var FKiAppID = Common.GetUserDetails(iUserID, sOrgName, sDatabase).FKiApplicationID;
                var iOrgID = UserDetais.FKiOrgID;
                var sOrgDB = UserDetais.sUserDatabase;
                if (iOrgID > 0)
                {
                    sDatabase = sOrgDB;
                }
                ModelDbContext dbContext = new ModelDbContext();
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIFieldOrigin_T");
                oBOI.BOD = oBOD;
                //oBOI.Attributes["PlaceHolderID".ToLower()] = new XIIAttribute { sName = "PlaceHolderID", sValue = Details.PlaceHolderID.ToString(), bDirty = true };
                if (!string.IsNullOrEmpty(model.sOneClickName))
                {
                    model.FK1ClickID = CoredbContext.Reports.Where(m => m.Name == model.sOneClickName).Select(m => m.ID).FirstOrDefault();
                    oBOI.SetAttribute("bIsOptionList", false.ToString());
                    oBOI.SetAttribute("iMasterDataID", "0");
                    oBOI.SetAttribute("FKiBOID", "0");
                }
                else if (model.iMasterDataID > 0)
                {
                    oBOI.SetAttribute("FK1ClickID", "0");
                    oBOI.SetAttribute("bIsOptionList", false.ToString());
                    oBOI.SetAttribute("FKiBOID", "0");
                }
                else if (model.bIsOptionList)
                {
                    oBOI.SetAttribute("FK1ClickID", "0");
                    oBOI.SetAttribute("iMasterDataID", "0");
                    oBOI.SetAttribute("FKiBOID", "0");
                }
                else if (!string.IsNullOrEmpty(model.sBOName))
                {
                    oBOI.SetAttribute("FK1ClickID", "0");
                    oBOI.SetAttribute("iMasterDataID", "0");
                    oBOI.SetAttribute("bIsOptionList", false.ToString());
                    model.FKiBOID = dbContext.BOs.Where(s => s.Name == model.sBOName).Select(m => m.BOID).FirstOrDefault();
                }
                else
                {
                    oBOI.SetAttribute("FK1ClickID", "0");
                    oBOI.SetAttribute("iMasterDataID", "0");
                    oBOI.SetAttribute("bIsOptionList", model.bIsOptionList.ToString());
                    oBOI.SetAttribute("FKiBOID", "0");
                }
                if (model.bIsCompare == false)
                {
                    oBOI.SetAttribute("sCompareField", null);
                }
                else
                {
                    oBOI.SetAttribute("sCompareField", model.sCompareField.ToString());
                }
                if (model.ID > 0)
                {
                    var OptionList = dbContext.XIFieldOptionList.Where(m => m.FKiQSFieldID == model.ID).ToList();
                    if (OptionList != null && OptionList.Count() > 0)
                    {
                        dbContext.XIFieldOptionList.RemoveRange(OptionList);
                    }
                }
                if (model.bIsHidden)
                {
                    oBOI.SetAttribute("sIsHidden", "on");
                }
                else
                {
                    oBOI.SetAttribute("sIsHidden", "off");
                }
                oBOI.SetAttribute("FKiApplicationID", model.FKiApplicationID.ToString());
                oBOI.SetAttribute("FKiOrganisationID", iOrgID.ToString());
                oBOI.SetAttribute("ID", model.ID.ToString());
                oBOI.SetAttribute("sName", model.sName);
                oBOI.SetAttribute("sDisplayName", model.sDisplayName);
                oBOI.SetAttribute("sAdditionalText", model.sAdditionalText);
                oBOI.SetAttribute("sDisplayHelp", model.sDisplayHelp);
                oBOI.SetAttribute("iLength", model.iLength.ToString());
                oBOI.SetAttribute("FKiDataType", model.FKiDataType.ToString());
                oBOI.SetAttribute("sDefaultValue", model.sDefaultValue);
                oBOI.SetAttribute("sPlaceHolder", model.sPlaceHolder);
                oBOI.SetAttribute("sScript", model.sScript);
                oBOI.SetAttribute("sFieldDefaultValue", model.sFieldDefaultValue);
                oBOI.SetAttribute("bIsMandatory", model.bIsMandatory.ToString());
                oBOI.SetAttribute("iValidationType", model.iValidationType.ToString());
                oBOI.SetAttribute("iValidationDisplayType", model.iValidationDisplayType.ToString());
                oBOI.SetAttribute("StatusTypeID", model.StatusTypeID.ToString());
                oBOI.SetAttribute("sMinDate", model.sMinDate);
                oBOI.SetAttribute("sMaxDate", model.sMaxDate);
                oBOI.SetAttribute("bIsDisable", model.bIsDisable.ToString());
                oBOI.SetAttribute("bIsMerge", model.bIsMerge.ToString());
                oBOI.SetAttribute("sMergeField", model.sMergeField);
                oBOI.SetAttribute("bIsCompare", model.bIsCompare.ToString());
                oBOI.SetAttribute("sMergeBo", model.sMergeBo);
                oBOI.SetAttribute("sMergeBoField", model.sMergeBoField);
                oBOI.SetAttribute("sValidationMessage", model.sValidationMessage);
                oBOI.SetAttribute("bIsUpperCase", model.bIsUpperCase.ToString());
                oBOI.SetAttribute("bIsLowerCase", model.bIsLowerCase.ToString());
                oBOI.SetAttribute("bIsHelpIcon", model.bIsHelpIcon.ToString());
                oBOI.SetAttribute("sMergeVariable", model.sMergeVariable);
                oBOI.SetAttribute("sCode", model.sCode);
                oBOI.SetAttribute("sQSCode", model.sQSCode);
                oBOI.SetAttribute("bIsOptionList", model.bIsOptionList.ToString());
                oBOI.SetAttribute("bIsDisplay", model.bIsDisplay.ToString());
                if (model.ID == 0)
                {
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                }
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                if (model.ID > 0)
                {
                    dbContext.Entry(model).State = System.Data.Entity.EntityState.Modified;
                }
                var QSField = oBOI.Save(oBOI);
                var Response = string.Empty;
                if (QSField.bOK && QSField.oResult != null)
                {
                    var oQSFdef = (XIIBO)QSField.oResult;
                    Response = oQSFdef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                }
                return Json(Response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        //IDESaveQSFieldOptionList
        [HttpPost]
        public ActionResult IDESaveQSFieldOptionList(int ID, string[] NVPairs)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                XIConfigs oConfig = new XIConfigs();
                var oResponse = oConfig.Save_FieldOptionList(ID, NVPairs);
                if (oResponse.bOK && oResponse.oResult != null)
                {

                }
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult IsExistsFieldName(string sName, int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                string sOrgName = SessionManager.OrganisationName;
                return XISemanticsRepository.IsExistsFieldName(sName, ID, SessionManager.UserID, sOrgName, sDatabase) ? Json(true, JsonRequestBehavior.AllowGet)
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
        public ActionResult SaveQSFieldOptionList(int ID, string[] NVPairs)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                string sOrgName = SessionManager.OrganisationName;
                var oResponse = XISemanticsRepository.SaveQSFieldOptionList(ID, NVPairs, iUserID, sOrgName, sDatabase);
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveStepXIFields(int iStepID, string[] XIFields)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var oResponse = XISemanticsRepository.SaveStepXIFields(iStepID, XIFields, iUserID, sOrgName, sDatabase);
                return Json(oResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNextStepInstance(int iStepID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            //var Response = XISemanticsRepository.GetNextStepInstance(iStepID, sDatabase);
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        // Copy QuestionSet Field By ID
        public ActionResult CopyQSFieldByID(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID;
                var CopyQSField = XISemanticsRepository.CopyQSFieldByID(ID, OrgID, iUserID, sDatabase);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #endregion QuestionSet

        #region XIDataTypes
        public ActionResult XIDataTypes()
        {
            return View();
        }
        public ActionResult GetXIDataTypeGrid(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GetXIDataTypeGrid(param, iUserID, sOrgName, sDatabase);
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
        public ActionResult AddEditXIDataType(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                cXIDataTypes oXIData = new cXIDataTypes();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID;
                if (ID > 0)
                {
                    oXIData = XISemanticsRepository.GetXIDataTypeByID(ID, iUserID, sOrgName, sDatabase);
                }
                if (fkiApplicationID == 0)
                {
                    oXIData.ddlApplications = Common.GetApplicationsDDL();
                }
                oXIData.FKiApplicationID = fkiApplicationID;
                return PartialView("_AddEditXIDataTypes", oXIData);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveEditXIDataType(cXIDataTypes model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Result = XISemanticsRepository.SaveXIDataType(model, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        //Save_DataType
        [HttpPost]
        public ActionResult Save_DataType(cXIDataTypes model)
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
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI DataTypes");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", model.ID.ToString());
                oBOI.SetAttribute("FKiOrganisationID", iOrgID.ToString());
                oBOI.SetAttribute("FKiApplicationID", model.FKiApplicationID.ToString());
                oBOI.SetAttribute("sBaseDataType", model.sBaseDataType.ToString());
                oBOI.SetAttribute("sName", model.sName);
                oBOI.SetAttribute("sStartRange", model.sStartRange);
                oBOI.SetAttribute("sEndRange", model.sEndRange);
                oBOI.SetAttribute("sRegex", model.sRegex);
                oBOI.SetAttribute("sScript", model.sScript);
                oBOI.SetAttribute("sValidationMessage", model.sValidationMessage);
                oBOI.SetAttribute("StatusTypeID", model.StatusTypeID.ToString());
                if (model.ID == 0)
                {
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                }
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                var oDataDef = oBOI.Save(oBOI);
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
        #endregion XIDataTypes


        #region QSVisualisations

        public ActionResult QSVisualisations()
        {
            return View("GridQSVisualisations");
        }

        public ActionResult GetQSVisualisationsGrid(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GetQSVisualisationsGrid(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddEditQSVisualisations(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                cQSVisualisations oXIData = new cQSVisualisations();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID;
                if (ID > 0)
                {
                    oXIData = XISemanticsRepository.GetQSVisualisationsByID(ID, iUserID, sOrgName, sDatabase);
                }
                oXIData.ddlQuestionSets = Common.GetQuestionsetsDDL(iUserID, sOrgName, sDatabase);
                if (ID == 0)
                {
                    oXIData.ddlQSStteps = Common.GetQSStepsDDL(0, iUserID, sOrgName, sDatabase);
                }
                else
                {
                    oXIData.ddlQSStteps = Common.GetQSStepsDDL(oXIData.FKiQSDefinitionID, iUserID, sOrgName, sDatabase);
                }
                //oXIData.ddlQSStteps = Common.GetQSStepsDDL(0, iUserID, sDatabase);
                oXIData.ddlSections = Common.GetQSSectionsDDL(0, iUserID, sOrgName, sDatabase);
                if (ID == 0)
                {
                    oXIData.ddlFields = Common.GetQSFieldsDDL(0, iUserID, sOrgName, sDatabase);
                }
                else
                {
                    oXIData.ddlFields = Common.GetQSFieldsDDL(oXIData.FKiQSStepDefinitionID, iUserID, sOrgName, sDatabase);
                }
                //oXIData.ddlFields = Common.GetQSFieldsDDL(0, iUserID, sDatabase);
                if (fkiApplicationID == 0)
                {
                    oXIData.ddlApplications = Common.GetApplicationsDDL();
                }
                oXIData.FKiApplicationID = fkiApplicationID;
                return PartialView("_AddEditQSVisualisations", oXIData);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetQSData(int ID, string Type)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.CoreDatabase;
                List<VMDropDown> Data = new List<VMDropDown>();
                if (Type == "QS")
                {
                    Data = Common.GetQSStepsDDL(ID, iUserID, sOrgName, sDatabase);
                }
                //else if (Type == "Step")
                //{
                //    Data = Common.GetQSSectionsDDL(ID, iUserID, sDatabase);
                //}
                else if (Type == "Step")
                {
                    Data = Common.GetQSFieldsDDL(ID, iUserID, sOrgName, sDatabase);
                }
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SaveQSVisualisations(cQSVisualisations model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Result = XISemanticsRepository.SaveQSVisualisations(model, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage });
            }
        }

        #endregion QSVisualisations


        #region XIQSScripts

        public ActionResult XIQSScripts()
        {
            return View("XIQSScripts");
        }

        public ActionResult GetXIQSScriptsGrid(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GetXIQSScriptsGrid(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddEditXIQSScripts(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIQSScripts oXIQSData = new XIQSScripts();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                if (ID > 0)
                {
                    oXIQSData = XISemanticsRepository.GetXIQSScriptsByID(ID, iUserID, sOrgName, sDatabase);
                }
                if (ID == 0)
                {
                    oXIQSData.ddlScripts = Common.GetQSScriptsDDL(0, iUserID, sDatabase);
                }
                else
                {
                    oXIQSData.ddlScripts = Common.GetQSScriptsDDL(Convert.ToInt32(oXIQSData.FKiScriptID), iUserID, sDatabase);
                }
                oXIQSData.ddlQuestionSets = Common.GetQuestionsetsDDL(iUserID, sOrgName, sDatabase);
                if (ID == 0)
                {
                    oXIQSData.ddlQSStteps = Common.GetQSStepsDDL(0, iUserID, sOrgName, sDatabase);
                }
                else
                {
                    oXIQSData.ddlQSStteps = Common.GetQSStepsDDL(oXIQSData.FKiQSDefinitionID, iUserID, sOrgName, sDatabase);
                }
                if (ID == 0)
                {
                    oXIQSData.ddlSections = Common.GetQSSectionsDDL(0, iUserID, sOrgName, sDatabase);
                }
                else
                {
                    oXIQSData.ddlSections = Common.GetQSSectionsDDL(oXIQSData.FKiStepDefinitionID, iUserID, sOrgName, sDatabase);
                }
                return PartialView("_AddEditQSScripts", oXIQSData);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult GetXIQSData(int ID, string Type)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                List<VMDropDown> Data = new List<VMDropDown>();
                if (Type == "QS")
                {
                    Data = Common.GetQSStepsDDL(ID, iUserID, sOrgName, sDatabase);
                }
                else if (Type == "Step")
                {
                    Data = Common.GetQSSectionsDDL(ID, iUserID, sOrgName, sDatabase);
                }
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult IDEGetXIQSData(int ID, string Type)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIQSLink oLink = new XIQSLink();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                List<XIDropDown> Data = new List<XIDropDown>();
                if (Type == "QS")
                {
                    oLink.FKiQSDefinitionID = ID;
                }
                else if (Type == "Step")
                {
                    oLink.FKiStepDefinitionID = ID;
                }
                var oLinkDef = oLink.Get_QSLinkDetails();
                if (oLinkDef.bOK && oLinkDef.oResult != null)
                {
                    oLink = (XIQSLink)oLinkDef.oResult;
                }
                if (Type == "QS")
                {
                    Data = oLink.ddlQSStteps;
                }
                else if (Type == "Step")
                {
                    Data = oLink.ddlSections;
                }
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SaveXIQSScripts(XIQSScripts model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Result = XISemanticsRepository.SaveXIQSScripts(model, iUserID, sOrgName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage });
            }
        }

        #endregion XIQSScripts


        #region Links

        public ActionResult GridQSLinks()
        {
            return View();
        }

        public ActionResult GridQSLinksList(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GridQSLinks(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddQSLinks(string Code)
        {
            XIQSLinkDefinition model = new XIQSLinkDefinition();
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                if (Code != null)
                {
                    model = XISemanticsRepository.GetQSXiLinkByID(Code, iUserID, sOrgName, sDatabase);
                }
                model.XILinks = XISemanticsRepository.GetXILinks(iUserID, sOrgName, sDatabase);
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
        public ActionResult DeleteXIQSLinkByID(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIConfigs oConf = new XIConfigs();
                var oCR = oConf.Delete_QSLink(ID);
                //int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                //var Result = XISemanticsRepository.DeleteXIQSLinkByID(ID, iUserID, sOrgName, sDatabase);
                if(oCR.bOK && oCR.oResult != null)
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
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveEditQSLinks(XIQSLinkDefinition model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XIQSLinkDefinition Link = new XIQSLinkDefinition();
                model.CreatedBy = iUserID;
                model.UpdatedBy = iUserID;
                return Json(XISemanticsRepository.SaveEditQSLinks(model, sDatabase), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Save_QSLinks(XIQSLinkDefintion model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            ModelDbContext dbContext = new ModelDbContext();
            try
            {
                //if (model.ID > 0)
                //{
                //    var Link = dbContext.QSXiLink.Find(model.ID);
                //    var oQSDefintion = dbContext.QSXiLink.Where(m => m.sCode == Link.sCode).ToList();
                //    dbContext.QSXiLink.RemoveRange(oQSDefintion);
                //    dbContext.SaveChanges();
                //}
                model.FKiApplicationID = SessionManager.ApplicationID;
                var oLinkDef = oConfig.Save_QSLink(model);
                if(oLinkDef.bOK && oLinkDef.oResult != null)
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
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult IsExistNameOrCode(string sName = "", string sCode = "", int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return Json(XISemanticsRepository.IsExistNameOrCode(sName, sCode, ID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Get_QSLinks()
        {
            List<XIDropDown> QSLinks = new List<XIDropDown>();
            int iApplicationID = SessionManager.ApplicationID;
            XIDXI oXID = new XIDXI();
            var oCR = oXID.Get_QSLinks(iApplicationID);
            if (oCR.bOK && oCR.oResult != null)
            {
                QSLinks = (List<XIDropDown>)oCR.oResult;
            }
            return Json(QSLinks, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_QuestionSets()
        {
            List<XIDropDown> QuestionSets = new List<XIDropDown>();
            int iApplicationID = SessionManager.ApplicationID;
            XIDXI oXID = new XIDXI();
            var oCR = oXID.Get_QuestionSets(iApplicationID);
            if (oCR.bOK && oCR.oResult != null)
            {
                QuestionSets = (List<XIDropDown>)oCR.oResult;
            }
            return Json(QuestionSets, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Get_QSSteps(int iQSDID)
        {
            List<XIDropDown> QSSteps = new List<XIDropDown>();
            int iApplicationID = SessionManager.ApplicationID;
            XIDXI oXID = new XIDXI();
            var oCR = oXID.Get_QSSteps(iQSDID);
            if (oCR.bOK && oCR.oResult != null)
            {
                QSSteps = (List<XIDropDown>)oCR.oResult;
            }
            return Json(QSSteps, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Get_QSSections(int iQSStepID)
        {
            List<XIDropDown> QSSections = new List<XIDropDown>();
            int iApplicationID = SessionManager.ApplicationID;
            XIDXI oXID = new XIDXI();
            var oCR = oXID.Get_QSSections(iQSStepID);
            if (oCR.bOK && oCR.oResult != null)
            {
                QSSections = (List<XIDropDown>)oCR.oResult;
            }
            return Json(QSSections, JsonRequestBehavior.AllowGet);
        }

        #endregion Links

        #region XIQSLinksMap
        public ActionResult XIQSLinks()
        {
            return View("XIQSLinks");
        }
        public ActionResult GetXIQSLinksGrid(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = XISemanticsRepository.GetXIQSLinksGrid(param, iUserID, sOrgName, sDatabase);
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
        public ActionResult AddEditXIQSLinks(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIQSLinks oXIQSLink = new XIQSLinks();
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                if (ID > 0)
                {
                    oXIQSLink = XISemanticsRepository.GetXIQSXiLinkByID(ID, iUserID, sOrgName, sDatabase);
                }
                if (ID == 0)
                {
                    oXIQSLink.ddlQSLinkCodes = Common.GetQSXiLinksDDL(0, iUserID, sDatabase);
                }
                else
                {
                    oXIQSLink.ddlQSLinkCodes = Common.GetQSXiLinksDDL(Convert.ToInt32(oXIQSLink.ID), iUserID, sDatabase);
                }
                oXIQSLink.ddlQuestionSets = Common.GetQuestionsetsDDL(iUserID, sOrgName, sDatabase);
                if (ID == 0)
                {
                    oXIQSLink.ddlQSStteps = Common.GetQSStepsDDL(0, iUserID, sOrgName, sDatabase);
                }
                else
                {
                    oXIQSLink.ddlQSStteps = Common.GetQSStepsDDL(oXIQSLink.FKiQSDefinitionID, iUserID, sOrgName, sDatabase);
                }
                if (ID == 0)
                {
                    oXIQSLink.ddlSections = Common.GetQSSectionsDDL(0, iUserID, sOrgName, sDatabase);
                }
                else
                {
                    oXIQSLink.ddlSections = Common.GetQSSectionsDDL(oXIQSLink.FKiStepDefinitionID, iUserID, sOrgName, sDatabase);
                }
                return PartialView("_AddEditQSXiLinks", oXIQSLink);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetXIQSLinkData(int ID, string Type)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                List<VMDropDown> Data = new List<VMDropDown>();
                if (Type == "QS")
                {
                    Data = Common.GetQSStepsDDL(ID, iUserID, sOrgName, sDatabase);
                }
                else if (Type == "Step")
                {
                    Data = Common.GetQSSectionsDDL(ID, iUserID, sOrgName, sDatabase);
                }
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveXIQSLinks(XIQSLink model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                XIConfigs oConf = new XIConfigs();
                var oCR = oConf.Save_QSLinkMapping(model);
                if (oCR.bOK && oCR.oResult != null)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Failure", JsonRequestBehavior.AllowGet);
                }
                //var Result = XISemanticsRepository.SaveXIQSLinks(model, iUserID, sOrgName, sDatabase);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse() { ID = 0, Status = false, ResponseMessage = ServiceConstants.ErrorMessage });
            }
        }

        [HttpPost]
        public ActionResult DeleteQSLinkMapping(int SectionID, string sCode)
        {
            XIConfigs oConf = new XIConfigs();
            var oCR = oConf.Delete_QSLinkMapping(SectionID, sCode);
            if (oCR.bOK && oCR.oResult != null)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion XIQSLinksMap

        //CopyXISemanticsByXISemanticID
        public ActionResult CopyXISemantics(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; var fkiApplicationID = oUser.FKiApplicationID;
                var CopyXISemantics = XISemanticsRepository.CopyXISemanticsByID(ID, OrgID, iUserID, sDatabase);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #region XIQSDefinitionStages

        public ActionResult XIQSStageDetails(int iXISemanticID = 0)
        {
            cXIQSStages oQSStage = new cXIQSStages();

            if (iXISemanticID != 0)
            {
                oQSStage = XISemanticsRepository.GetStagesByQSDefID(iXISemanticID);
            }
            oQSStage.FKiQSDefinitionID = iXISemanticID;
            return View(oQSStage);
        }

        [HttpPost]
        public ActionResult SaveEditXIQSStages(cXIQSStages oQSStage)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oQSStage.CreatedBy = iUserID;
                oQSStage.UpdatedBy = iUserID;
                return Json(XISemanticsRepository.SaveEditXIQSStages(oQSStage, sDatabase), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteXIQSStageByID(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Result = XISemanticsRepository.DeleteXIQSStageByID(ID);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #endregion XIQSDefinitionStages

        #region QuestionSet Configs

        [HttpPost]
        public ActionResult View_QSConfigs(int iQSDID = 1337)
        {
            var sDatabase = SessionManager.CoreDatabase;
            try
            {
                if (iQSDID > 0)
                {
                    XIInfraCache oCache = new XIInfraCache();
                    var oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDID.ToString());
                    if (oQSD != null)
                    {
                        return PartialView("_ViewQSConfig", oQSD);
                    }
                }

            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                throw ex;
            }
            return null;
        }

        #endregion QuestionSet Configs

        public ActionResult Get_QSSectionsALL()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIDXI oXID = new XIDXI();
                List<XIDQSSection> Secs = new List<XIDQSSection>();
                List<VMDropDown> ddlSecs = new List<VMDropDown>();
                var oCR = oXID.Get_QSSectionsAll();
                if (oCR.bOK && oCR.oResult != null)
                {                    
                    Secs = (List<XIDQSSection>)oCR.oResult;
                    ddlSecs = Secs.Select(m => new VMDropDown { ID = m.ID, text = m.sName }).ToList();
                }
                return Json(ddlSecs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

    }
}