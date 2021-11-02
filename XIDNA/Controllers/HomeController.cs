using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;
using System.Linq;
using System;
using XIDNA.ViewModels;
using XIDNA.Repository;
using XIDNA.Models;
using XIDNA.Common;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using XICore;
using XISystem;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

namespace XIDNA.Controllers
{
    [Authorize]
    //[SessionTimeout]
    public class HomeController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHomeRepository HomeRepository;

        public HomeController() : this(new HomeRepository())
        {

        }

        public HomeController(IHomeRepository HomeRepository)
        {
            this.HomeRepository = HomeRepository;
        }
        XIInfraUsers oUser = new XIInfraUsers();
        CommonRepository Common = new CommonRepository();
        XIInfraCache oCache = new XIInfraCache();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult LandingPages(string XilinkId = null, string LeadID = null, string Userid = null)
        {
            string sDatabase = SessionManager.CoreDatabase;
            var sSessionID = HttpContext.Session.SessionID;
            try
            {
                var AppName = SessionManager.AppName;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                if (oUser != null && oUser.UserID > 0)
                {

                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
                int OrgID = oUser.FKiOrganisationID;
                XIDLayout oLayout = new XIDLayout();
                if (AppName.ToLower() == "zeeinsurance")
                {
                    //LoadEarly();
                }
                if (oUser.Role.sRoleName.ToLower() == EnumRoles.WebUsers.ToString().ToLower())
                {
                    XIInfraCache oCache = new XIInfraCache();
                    string sFunction = "xi.s|{if|{gt|{xi.a|'ACPolicy_T',{xi.p|-iUserID},'ID','','FKiUserID'},'0'},'true','false'}";
                    List<CNV> oNVList = new List<CNV>();
                    string sGUID = Guid.NewGuid().ToString();
                    CNV oParam = new CNV();
                    oParam.sName = "sFunction";
                    oParam.sValue = sFunction;
                    oNVList.Add(oParam);
                    oParam = new CNV();
                    oParam.sName = "-iUserID";
                    oParam.sValue = iUserID.ToString();
                    oNVList.Add(oParam);
                    oCache.SetXIParams(oNVList, sGUID, sSessionID);
                    CResult oCRes = new CResult();
                    XIDScript oXIScript = new XIDScript();
                    oXIScript.sScript = sFunction.ToString();
                    oCRes = oXIScript.Execute_Script(sGUID, sSessionID);
                    string sValue = string.Empty;
                    if (oCRes.bOK && oCRes.oResult != null)
                    {
                        sValue = (string)oCRes.oResult;
                        if (sValue == "false")
                        {
                            oUser.Role.iLayoutID = 2253;
                            Singleton.Instance.ActiveMenu = "Quotes";
                        }
                        else
                        {
                            Singleton.Instance.ActiveMenu = "Policies";
                        }
                    }

                    if (oUser.Role.iLayoutID > 0)
                    {
                        //var oLayDef = oXID.Get_LayoutDefinition(null, iLayoutID.ToString());
                        oLayout.ID = oUser.Role.iLayoutID;
                        //var oLayDef = oLayout.Load();
                        //if (oLayDef.bOK && oLayDef.oResult != null)
                        //{
                        //    oLayout = (XIDLayout)oLayDef.oResult;
                        //}
                        //var oLayDef = oDXI.Get_LayoutDefinition(null, oUser.Role.iLayoutID.ToString());
                        //var Layout = Common.GetLayoutDetails(oUser.Role.iLayoutID, 0, 0, 0, null, iUserID, sOrgName, sDatabase);
                        //SessionManager.sGUID = oLayDef.sGUID;
                        //oLayout = (XIDLayout)oLayDef.oResult;
                        oCache.Set_ParamVal(sSessionID, oLayout.sGUID, null, "{XIP|sUserName}", oUser.sFirstName, null, null);
                        oCache.Set_ParamVal(sSessionID, oLayout.sGUID, null, "{XIP|iUserID}", iUserID.ToString(), null, null);

                    }
                    //XIDStructure oXIDStructure = new XIDStructure();
                    //string sOneClickName = "Client Policy List for DropDown";
                    //XID1Click o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, sOneClickName, null);
                    ////o1ClickD.ReplaceFKExpressions(nParms);
                    ////o1ClickD.Query = oXIDStructure.ReplaceExpressionWithCacheValue(o1ClickD.Query, nParms);
                    //Dictionary<string, XIIBO> oRes = o1ClickD.OneClick_Execute();
                    //if (oRes != null && oRes.Count() > 0)
                    //{
                    //    var oBOIList = oRes.Values.ToList();
                    //    Dictionary<int, string> nPolicyDetails = new Dictionary<int, string>();
                    //    Session["PolicyCount"] = oBOIList.Count();
                    //    if (oBOIList != null && oBOIList.Count() > 0)
                    //    {
                    //        foreach (var oBOI in oBOIList)
                    //        {
                    //            int iInstanceID = 0; string sPolicyNo = string.Empty; string sRegistrationNo = string.Empty;
                    //            if (oBOI.Attributes.ContainsKey("id"))
                    //            {
                    //                iInstanceID = Convert.ToInt32(oBOI.AttributeI("id").sValue);
                    //            }
                    //            if (oBOI.Attributes.ContainsKey("sPolicyNo"))
                    //            {
                    //                sPolicyNo = oBOI.AttributeI("sPolicyNo").sValue;
                    //            }
                    //            if (oBOI.Attributes.ContainsKey("sRegNo"))
                    //            {
                    //                sRegistrationNo = oBOI.AttributeI("sRegNo").sValue;
                    //            }
                    //            nPolicyDetails[iInstanceID] = sPolicyNo + "_" + sRegistrationNo;
                    //        }
                    //        Session["PolicyCollection"] = nPolicyDetails;
                    //        if(nPolicyDetails!=null && nPolicyDetails.Count()>0)
                    //        {
                    //            Session["sRegistrationNo"] = nPolicyDetails.FirstOrDefault().Value.Split('_')[1];
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //foreach (var oBOI in oBOIList)
                    //        //{
                    //        //    Session["iPolicyNo"] = Convert.ToInt32(oBOI.AttributeI("id").sValue);
                    //        //}
                    //    }
                    //}
                    if (string.IsNullOrEmpty(Userid))
                    {
                        ViewBag.XilinkId = XilinkId;
                        ViewBag.LeadID = LeadID;
                        ViewBag.sGUID = sGUID;
                    }
                    else
                    {
                        ViewBag.Userid = Userid;
                        ViewBag.XilinkId = XilinkId;
                        ViewBag.LeadID = LeadID;
                        ViewBag.sGUID = sGUID;
                    }
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|sBOName}", "Lead_T", null, null);
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|iInstanceID}", LeadID, null, null);

                    return View("UserPage", oLayout);
                }
                else
                {
                    if (oUser.Role.iLayoutID > 0)
                    {

                        var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, null, oUser.Role.iLayoutID.ToString()); //oDXI.Get_LayoutDefinition(null, oUser.Role.iLayoutID.ToString());
                        //var Layout = Common.GetLayoutDetails(oUser.Role.iLayoutID, 0, 0, 0, null, iUserID, sOrgName, sDatabase);
                        //SessionManager.sGUID = oLayDef.sGUID;
                        if (oLayDef != null)
                        {
                            oLayout = (XIDLayout)oLayDef;
                        }
                        oCache.Set_ParamVal(sSessionID, oLayout.sGUID, null, "{XIP|sUserName}", oUser.sFirstName, null, null);
                        oCache.Set_ParamVal(sSessionID, oLayout.sGUID, null, "{XIP|iUserID}", iUserID.ToString(), null, null);
                    }
                    if (oUser.Role.sRoleName.ToLower() == EnumRoles.XISuperAdmin.ToString().ToLower() || oUser.Role.sRoleName.ToLower() == EnumRoles.OrgIDE.ToString().ToLower() || oUser.Role.sRoleName.ToLower() == EnumRoles.DeveloperStudio.ToString().ToLower() || oUser.Role.sRoleName.ToLower() == EnumRoles.AppAdmin.ToString().ToLower() || oUser.Role.sRoleName.ToLower() == EnumRoles.OrgAdmin.ToString().ToLower())
                    {
                        if (oUser.Role.iLayoutID > 0 && oLayout != null)
                        {
                            return View("LandingPage", oLayout);
                        }
                        else
                        {
                            return RedirectToAction("Index", "XIApplications");
                        }
                    }
                    else if (oUser.Role.sRoleName.ToLower() == EnumRoles.SuperAdmin.ToString().ToLower())
                    {
                        return RedirectToAction("Index", "QueryGeneration");
                    }
                    else if (oUser.Role.sRoleName.ToLower() == EnumRoles.Admin.ToString().ToLower())
                    {
                        return RedirectToAction("Index", "QueryGeneration");
                    }
                    else
                    {
                        //var oLayDef = oLayout.Load();
                        //if (oLayDef.bOK && oLayDef.oResult != null)
                        //{
                        //    oLayout = (XIDLayout)oLayDef.oResult;
                        //}
                        return View("InternalUsers", oLayout);
                    }
                }

            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //[HttpPost]
        [AllowAnonymous]
        public ActionResult Page()
        {
            var sHomePage = SessionManager.sHomePage;
            return View(sHomePage);
        }

        private void LoadEarly()
        {
            XIInfraCache oCache = new XIInfraCache();
            Thread threadObj = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO, "ACPolicy_T"); }));
            threadObj.SetApartmentState(ApartmentState.MTA);
            threadObj.IsBackground = true;
            threadObj.Start();
            Thread threadObj1 = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO, "Policy Version"); }));
            threadObj1.SetApartmentState(ApartmentState.MTA);
            threadObj1.IsBackground = true;
            threadObj1.Start();
            Thread threadObj2 = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO, "QS Instance"); }));
            threadObj2.SetApartmentState(ApartmentState.MTA);
            threadObj2.IsBackground = true;
            threadObj2.Start();
            Thread threadObj3 = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO, "Driver_T"); }));
            threadObj3.SetApartmentState(ApartmentState.MTA);
            threadObj3.IsBackground = true;
            threadObj3.Start();
            Thread threadObj4 = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO, "Claim_T"); }));
            threadObj4.SetApartmentState(ApartmentState.MTA);
            threadObj4.IsBackground = true;
            threadObj4.Start();
            Thread threadObj5 = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO, "Conviction_T"); }));
            threadObj5.SetApartmentState(ApartmentState.MTA);
            threadObj5.IsBackground = true;
            threadObj5.Start();
            Thread threadObj6 = new Thread(new ThreadStart(() => { oCache.GetObjectFromCache(XIConstant.CacheBO_All, "ACPolicy_T"); }));
            threadObj6.SetApartmentState(ApartmentState.MTA);
            threadObj6.IsBackground = true;
            threadObj6.Start();
        }

        #region Menu Management

        public ActionResult EditRootMenu(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                ViewBag.DetailsID = ID;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                RightMenuTrees model = HomeRepository.EditRootMenu(ID, OrgID, iUserID, sDatabase);
                if (OrgID == 0)
                {
                    model.Organisations.Insert(0, new VMDropDown { text = "Super Admin", Value = 0 });
                }
                else
                {
                    model.Organisations = model.Organisations.Where(m => m.Value == OrgID).ToList();
                }
                return PartialView("_EditMenuForm", model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult DeleteRootMenu(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                int iStatus = HomeRepository.DeleteRootMenu(ID, OrgID, iUserID, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //25/11/2017
        public ActionResult MenuWithTree()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                RightMenuTrees Model = HomeRepository.GetOrganisation(OrgID, iUserID, sOrgName, sDatabase);
                if (OrgID == 0)
                {
                    Model.Organisations.Insert(0, new VMDropDown { text = "Super Admin", Value = 0 });
                }
                else
                {
                    Model.Organisations = Model.Organisations.Where(m => m.Value == OrgID).ToList();
                }
                return PartialView("_MenuWithTree", Model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult SaveMenuTreeDetails(string RootNode, string ParentNode, string NodeID, string NodeTitle, string Type, int ID = 0, int iRoleID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                int DBstatus = HomeRepository.SaveMenuTreeDetails(ID, RootNode, ParentNode, NodeID, NodeTitle, Type, iRoleID, iUserID, OrgID, sDatabase, sOrgName);
                return Json(DBstatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetMenuTreeDetails()
        //{
        //    int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //    oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //    List<RightMenuTrees> lResult = HomeRepository.GetMenuTreeDetails(iUserID, OrgID, sDatabase);
        //    return Json(lResult, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult DeleteNodeDetails(string ParentNode, string NodeID, string ChildrnIDs, string Type, int iRoleID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                List<RightMenuTrees> lResult = HomeRepository.DeleteNodeDetails(ParentNode, NodeID, ChildrnIDs, Type, iRoleID, iUserID, OrgID, sDatabase);
                return Json(lResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        //26/10/2017
        public ActionResult AddDetailsForMenu(string ParentNode, string NodeID, int iRoleID, string DetailsID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                //ViewBag.DetID = DetailsID;
                ViewBag.DetID = NodeID;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                RightMenuTrees model = HomeRepository.AddDetailsForMenu(ParentNode, NodeID, iRoleID, OrgID, iUserID, sDatabase);
                //var AllXiLinkLists = (XILink)oDXI.Get_XILinkDefinition(0).oResult;
                //model.VMXILink = AllXiLinkLists.XiLinkDDLs.Select(m => new VMDropDown { text = m.Expression, Value = m.ID }).ToList();
                model.VMXILink = Common.GetXiLinksDDL(sDatabase);
                //ModelDbContext dbContext = new ModelDbContext();
                Dictionary<string, string> XiLinks = new Dictionary<string, string>();
                //var lXiLinks = dbContext.XiLinks.Where(m => m.FKiApplicationID == UserDetais.FKiApplicationID).ToList();
                foreach (var items in model.VMXILink)
                {
                    XiLinks[items.Value.ToString()] = items.text;
                }
                model.XILinks = XiLinks;
                return PartialView("_AddDetailsForMenu", model);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult SaveAddedDetails(RightMenuTrees model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                //if(model.XiLinkID==0)
                //{
                //    model.ActionType = 10;
                //}
                //else
                //{
                //    model.ActionType = 20;
                //}
                var Status = HomeRepository.SaveAddedDetails(iUserID, model, sDatabase);
                //var result= HomeRepository.SaveAddedDetails(iUserID, model, sDatabase);
                //return null;
                return Json(Status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                //return null;
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SaveEditedMenuDetails(int RoleID, int OrgID, string NewRootName, string OldRootName)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iStatus = HomeRepository.SaveEditedMenuDetails(RoleID, OrgID, NewRootName, OldRootName, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult ShowMenuTreeDetails()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                List<RightMenuTrees> Models = HomeRepository.ShowMenuTreeDetails(iUserID, OrgID, sOrgName, sDatabase);
                return Json(Models, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        //public ActionResult GetChildForMenu(int ID)
        //{
        //    try
        //    {
        //        oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //        List<RightMenuTrees> Models = HomeRepository.GetChildForMenu(ID, OrgID);
        //        return Json(Models, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        Common.SaveErrorLog(ex.ToString(), sDatabase);
        //        return null;
        //    }
        //}

        //public ActionResult GetXILinkDetails(int XilinkID)
        //{
        //    oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //    string sDatabase = SessionManager.CoreDatabase;
        //    int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //    VMXiLinks Details = HomeRepository.GetXILinkDetails(XilinkID, iUserID, OrgID, sDatabase);
        //    return Json(Details, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult ShowRightMenu()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var model = HomeRepository.ShowMenuTreeDetails(iUserID, OrgID, sOrgName, sDatabase);
                return PartialView("_HomeRightMenu", model);
            }
            catch (Exception ex)
            {
                if (SessionManager.CoreDatabase == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        //public ActionResult DragAndDropNodes(string NodeID, string OldParentID, string NewParentID)
        //{
        //    oUser.UserID = iUserID;oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;int OrgID = oUser.FKiOrganisationID;
        //    string sDatabase = SessionManager.CoreDatabase;
        //    int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
        //    int Status = HomeRepository.DragAndDropNodes(NodeID, OldParentID, NewParentID, iUserID, OrgID, sDatabase);
        //    return Json(Status, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult DragAndDropNodes(string NodeID, string OldParentID, string NewParentID, int Oldposition, int Newposition)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                var Tab = HomeRepository.DragAndDropNodes(NodeID, OldParentID, NewParentID, iUserID, OrgID, sDatabase, Oldposition, Newposition);
                return Json(Tab, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult IDEGetRolesForMenu(string OrgName, int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIMenu oXIM = new XIMenu();
                oXIM.sCoreDatabase = SessionManager.CoreDatabase;
                oXIM.OrgID = OrgID;
                var List = oXIM.Get_XIRolesDDL();
                List<XIDropDown> oRoleName = new List<XIDropDown>();
                if (List.bOK && List.oResult != null)
                {
                    oRoleName = (List<XIDropDown>)List.oResult;
                }
                return Json(List, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //25/11/2017
        public ActionResult GetRolesForMenu(string OrgName, int OrgID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                List<VMDropDown> sRoleName = HomeRepository.GetRolesForMenu(OrgName, OrgID, sDatabase);
                return Json(sRoleName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        [HttpPost]
        public ActionResult IsExistsRoot(int ID, string RootName, int OrgID, int RoleID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                return HomeRepository.IsExistsRootName(ID, RootName, OrgID, RoleID, sDatabase) ? Json(true, JsonRequestBehavior.AllowGet)
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
        public ActionResult SaveMenuDetails(int OrgID, string RootName, int RoleID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                var Result = HomeRepository.SaveMenuDetails(iUserID, RoleID, OrgID, RootName, sDatabase);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetMenuTreeDetails(string RootName, int OrgID, int RoleID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                List<RightMenuTrees> lResult = HomeRepository.GetMenuTreeDetails(RootName, OrgID, RoleID, sDatabase);
                return Json(lResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult GetChildForRootMenu(string NodeID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                List<RightMenuTrees> Model = HomeRepository.GetChildForRootMenu(NodeID, iUserID, OrgID, sDatabase);
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult DisplayMenuDetails(string sType)
        {
            //var sDatabase = sDatabase;
            ViewBag.sType = sType;
            return View();
        }

        public ActionResult GetMenuDetails(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = HomeRepository.GetMenuDetails(param, iUserID, sOrgName, OrgID, sDatabase);
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

        #endregion Menu Management

        #region CacheDetails

        public ActionResult GetCachedDetails()
        {
            List<XIInfraCache> oCacheList = new List<XIInfraCache>();
            try
            {
                var oCurrentCache = System.Web.HttpContext.Current.Cache;
                if (oCurrentCache != null)
                {
                    var oCachedEnumr = oCurrentCache.GetEnumerator();
                    while (oCachedEnumr.MoveNext())
                    {
                        if (oCachedEnumr.Key.ToString().Contains("bo"))
                        {
                            XIInfraCache oCache = new XIInfraCache();
                            oCache.sKey = oCachedEnumr.Key.ToString();
                            oCache.oCachedObject = oCurrentCache[oCachedEnumr.Key.ToString()] == null ? "" : oCurrentCache[oCachedEnumr.Key.ToString()].ToString();
                            //oCache.sSize = "0";
                            var sSingleKey = oCachedEnumr;
                            string json = JsonConvert.SerializeObject(sSingleKey, Newtonsoft.Json.Formatting.Indented);
                            var bf = new BinaryFormatter();
                            var ms = new MemoryStream();
                            bf.Serialize(ms, json);
                            oCache.sSize = ms.Length.ToString() + " bytes";
                            oCacheList.Add(oCache);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_CacheInformation", oCacheList);
        }
        public string CacheClear(string sKey)
        {
            var sAppName = SessionManager.AppName;
            //if (!string.IsNullOrEmpty(sAppName))
            //{
            //    sKey = sAppName + "_" + sKey + "_";
            //}
            sKey = sKey + "_";
            var oCurrentCache = System.Web.HttpContext.Current.Cache;
            if (oCurrentCache != null)
            {
                //List<string> CacheRemove = new List<string>();
                var oCachedEnumr = oCurrentCache.GetEnumerator();
                while (oCachedEnumr.MoveNext())
                {
                    if (!string.IsNullOrEmpty(sKey))
                    {
                        if (oCachedEnumr.Key.ToString().StartsWith(sKey))
                        {
                            //CacheRemove.Add(oCachedEnumr.Key.ToString());
                            oCurrentCache.Remove(oCachedEnumr.Key.ToString());
                        }
                    }
                }
                //foreach (string CRemove in CacheRemove)
                //{
                //    oCurrentCache.Remove(CRemove);
                //}
            }
            return null;
        }

        public ActionResult ClearCache()
        {
            return PartialView("_CacheInformation");
        }

        #endregion CacheDetails


        #region AddRolesToMenus

        //Getting All the Menus
        public ActionResult RoleMenusTree()
        {
            string sDatabase = SessionManager.CoreDatabase;
            ModelDbContext dbContext = new ModelDbContext();
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                List<XIRoleMenu> Menus = new List<XIRoleMenu>();
                XIRoleMenu oMenu = new XIRoleMenu();
                XIMenu lMenu = new XIMenu();
                lMenu.sCoreDatabase = sDatabase;
                Menus = oMenu.Get_AllRoleMenuTreeDetails(); //For Role Based All Menus
                var oRolesList = (List<XIDropDown>)lMenu.Get_XIRolesDDL().oResult;
                oMenu.Roles = oRolesList.ToList().Select(m => new XIDropDown { text = m.text, Value = m.Value }).ToList();
                ViewBag.Menus = Menus;
                var GroupMenus = dbContext.XIMenuMappings.Where(m => m.ParentID != "#").ToList();
                //if (GroupMenus.Count() > 0)
                //{
                //    oMenu.GroupIDs = GroupMenus.Select(m => Convert.ToInt32(m.FKiMenuID)).Distinct().ToList();
                //}
                return View(oMenu);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //Saving Checked Menus into Database
        [HttpPost]
        public XIMenuMappings oXIMenusParams(List<XIMenuMappings> oMenuParams, string RootName, int iRoleID, string RoleName)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult;
                int iOrgID = oUser.FKiOrganisationID;
                var oMenu = HomeRepository.SaveRoleMappings(oMenuParams, RootName, iRoleID, RoleName, iOrgID, iUserID, sDatabase);
                return oMenu;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //For Getting Mapped MenuID's From Database
        [HttpPost]
        public ActionResult GetRoleMenusTree(string RootName, int iRoleID)
        {
            ModelDbContext dbContext = new ModelDbContext();
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int iOrgID = oUser.FKiOrganisationID;
                List<int> GroupIDs = new List<int>();
                var GroupMenus = dbContext.XIMenuMappings.Where(m => m.ParentID != "#").ToList();
                //if (GroupMenus.Count() > 0)
                //{
                //    GroupIDs = GroupMenus.Select(m => Convert.ToInt32(m.FKiMenuID)).Distinct().ToList();
                //}
                return Json(GroupIDs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //Adding New Node
        public ActionResult AddTreeNode(XIRoleMenus node)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase, iUserID).oResult; int iOrgID = oUser.FKiOrganisationID; int fkiApplicationID = oUser.FKiApplicationID;
                node.FKiApplicationID = oUser.FKiApplicationID;
                var role = HomeRepository.AddTreeNode(node, sOrgName, iOrgID, iUserID, sDatabase);
                return Json(role, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        //Creation and Rename of Menu
        public ActionResult CreateandRenameMenu(XIRoleMenus RootNode)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int iOrgID = oUser.FKiOrganisationID;
                RootNode.FKiApplicationID = oUser.FKiApplicationID;
                int dbStatus = HomeRepository.CreateandRenameMenu(RootNode, iOrgID, iUserID, sDatabase);
                return Json(dbStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //Deleting the Menu by ID
        public ActionResult DeleteTreeMenu(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int iOrgID = oUser.FKiOrganisationID;
                int dbStatus = HomeRepository.DeleteTreeMenu(ID);
                return Json(dbStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        #endregion AddRolesToMenus

        public ActionResult UserLevelCache(string sKey)
        {
            List<VM_UserCacheKeyValue> oCacheKeyValue = new List<VM_UserCacheKeyValue>();
            var oUserCache = HttpRuntime.Cache["XICache"];
            var oCacheobj = (XICacheInstance)oUserCache;
            if (oCacheobj != null)
            {
                var sSessionDetails = oCacheobj.NMyInstance;
                var sUDetails = sSessionDetails.FirstOrDefault().Value.NMyInstance.FirstOrDefault().Value.NMyInstance;
                foreach (var items in sUDetails)
                {
                    var sGUID = string.Empty;
                    if (items.Key.StartsWith("UID_"))
                    {
                        sGUID = items.Key.Replace("UID_", "");
                    }
                    foreach (var item in items.Value.NMyInstance)
                    {
                        var lVals = new VM_UserCacheKeyValue();
                        if (item.Key != "sSessionID" && item.Key != "sGUID")
                        {
                            lVals.sKey = item.Key;
                            lVals.sValue = item.Value.sValue;
                            lVals.sGUID = sGUID;
                            oCacheKeyValue.Add(lVals);
                        }
                    }
                }
            }
            return View("_HtmlCacheList", oCacheKeyValue);
        }

        [AllowAnonymous]
        public ActionResult RetrieveDetails()
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                var sSessionID = HttpContext.Session.SessionID;
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                if (iUserID > 0)
                {
                    oUser.UserID = iUserID; oUser = (XIInfraUsers)oUser.Get_UserDetails(sDatabase).oResult; int OrgID = oUser.FKiOrganisationID;
                    XIDLayout oLayout = new XIDLayout();
                    LoadEarly();
                    if (oUser.Role.sRoleName.ToLower() == EnumRoles.WebUsers.ToString().ToLower())
                    {
                        oUser.Role.iLayoutID = 2253;
                        Singleton.Instance.ActiveMenu = "Quotes";
                        if (oUser.Role.iLayoutID > 0)
                        {
                            oLayout.ID = oUser.Role.iLayoutID;
                            oCache.Set_ParamVal(sSessionID, oLayout.sGUID, null, "{XIP|sUserName}", oUser.sFirstName, null, null);
                            oCache.Set_ParamVal(sSessionID, oLayout.sGUID, null, "{XIP|iUserID}", iUserID.ToString(), null, null);
                        }
                        return View("UserPage", oLayout);
                    }
                }
                else
                {
                    return RedirectToAction("ClientLogin", "Account");
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

        [AllowAnonymous]
        public ActionResult RequestHandler()
        {
            var sServerKey = System.Configuration.ConfigurationManager.AppSettings["ServerKey"];
            //Common.SaveErrorLog("Request forwarded to " + sServerKey, "");
            return Json(sServerKey, JsonRequestBehavior.AllowGet);
        }

        #region AssignMenu

        public ActionResult AssignMenu(string RootNode, string ParentNode, string NodeID, string NodeTitle, string Type, int ID = 0, int iRoleID = 0, int iOrgID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID;
                XIAssignMenu oAs = new XIAssignMenu();
                SignalR oSignalR = new SignalR();
                XIConfigs oConfig = new XIConfigs(oSignalR);
                oConfig.iUserID = iUserID;
                oConfig.sSessionID = HttpContext.Session.SessionID;
                if (ID == 0)
                {
                    oAs.ID = ID;
                    oAs.Name = NodeTitle;
                    oAs.RootName = RootNode;
                    oAs.RoleID = iRoleID;
                    oAs.sType = "AssignMenu";
                    oAs.OrgID = iOrgID;
                    if (ParentNode == "#")
                    {
                        oAs.ParentID = "#";
                        oAs.Name = RootNode;
                    }
                }
                else if (ID > 0)
                {
                    oAs = oAs.Get_AssignedTreeDetails(ID);
                    oAs.sType = "AssignMenu";
                    oAs.ParentID = ParentNode;
                    if (Type == "rename")
                    {
                        oAs.Name = NodeTitle;
                    }
                    else if (Type == "Assign")
                    {
                        //Passing FKiMenuID Value through NodeID
                        oAs.FKiMenuID = Convert.ToInt32(NodeID);
                    }
                }
                var oMenuDef = oConfig.Save_AssignMenu(oAs);
                if (oMenuDef.bOK && oMenuDef.oResult != null)
                {
                    int iID = 0;
                    var oMDef = (XIIBO)oMenuDef.oResult;
                    var sMenuID = oMDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sMenuID, out iID);
                    return Json(iID);
                }
                return Json(0);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAssignedMenu(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            XIAssignMenu oAs = new XIAssignMenu();
            try
            {
                int iUserID = SessionManager.UserID;
                SignalR oSignalR = new SignalR();
                XIConfigs oConfig = new XIConfigs(oSignalR);
                oConfig.iUserID = iUserID;
                oConfig.sSessionID = HttpContext.Session.SessionID;
                if (ID > 0)
                {
                    var oAsDef = oAs.Get_AssingedMenuDetails(ID);
                    if (oAsDef.bOK && oAsDef.oResult != null)
                    {
                        oAs = (XIAssignMenu)oAsDef.oResult;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(oAs, JsonRequestBehavior.AllowGet);
            }
            return Json(oAs, JsonRequestBehavior.AllowGet);
        }

        #endregion AssignMenu

        #region MultiOrg

        public ActionResult Get_Organisations()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";//expalin about this method logic
            List<CNV> nOrg = new List<CNV>();
            try
            {
                var iUserID = SessionManager.UserID;
                oTrace.oParams.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                if (iUserID > 0)//check mandatory params are passed or not
                {
                    XIIXI oXI = new XIIXI();
                    XID1Click o1Click = new XID1Click();
                    o1Click.BOID = 1302;
                    o1Click.Query = "Select * from XIUserOrgMapping_T where FKiUserID=" + iUserID;
                    var Data = o1Click.OneClick_Run();
                    if (Data != null && Data.Count() > 0)
                    {
                        foreach (var BOI in Data.Values)
                        {
                            var OrgID = BOI.AttributeI("FKiOrgID").sValue;
                            var Org = oXI.BOI("Organisations", OrgID);
                            if (Org != null && Org.Attributes.Count() > 0)
                            {
                                var OrgName = Org.AttributeI("name").sValue;
                                nOrg.Add(new CNV { sName = OrgName, sValue = OrgID });
                            }
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iUserID is missing";
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
            return Json(nOrg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Landing(int ID)
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
                oTrace.oParams.Add(new CNV { sName = "ID", sValue = ID.ToString() });
                var sDatabase = SessionManager.CoreDatabase;
                var iUserID = SessionManager.UserID;
                List<CNV> WhrPrms = new List<CNV>();
                WhrPrms.Add(new CNV { sName = "FKiUserID", sValue = iUserID.ToString() });
                WhrPrms.Add(new CNV { sName = "FKiOrgID", sValue = ID.ToString() });
                SessionManager.iUserOrg = ID;
                var RoleID = string.Empty;
                XIIXI oXI = new XIIXI();
                var oBOI = oXI.BOI("XIUserOrgMapping", null, null, WhrPrms);
                if (oBOI != null && oBOI.Attributes.Count() > 0)
                {
                    RoleID = oBOI.AttributeI("FKiRoleID").sValue;
                    var OrgID = oBOI.AttributeI("FKiOrgID").sValue;
                }
                XIInfraRoles oRoleD = new XIInfraRoles();
                oRoleD.RoleID = Convert.ToInt32(RoleID);
                oCR = oRoleD.Get_RoleDefinition(sDatabase);
                oRoleD = (XIInfraRoles)oCR.oResult;
                var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, null, oRoleD.iLayoutID.ToString()); //oDXI.Get_LayoutDefinition(null, oUser.Role.iLayoutID.ToString());
                XIDLayout oLayout = new XIDLayout();
                if (oLayDef != null)
                {
                    oLayout = (XIDLayout)oLayDef;
                    if (oLayout != null && oLayout.ID > 0)
                    {
                        return View("LandingPage", oLayout);
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
            return null;
        }

        #endregion MultiOrg


        #region MultiApp

        public ActionResult Get_Applications()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";//expalin about this method logic
            List<CNV> nOrg = new List<CNV>();
            try
            {
                var iUserID = SessionManager.UserID;
                oTrace.oParams.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                if (iUserID > 0)//check mandatory params are passed or not
                {
                    XIIXI oXI = new XIIXI();
                    XID1Click o1Click = new XID1Click();
                    o1Click.BOID = 681;
                    o1Click.Query = "Select * from XIApplication_T";
                    var Data = o1Click.OneClick_Run();
                    if (Data != null && Data.Count() > 0)
                    {
                        foreach (var BOI in Data.Values)
                        {
                            var ID = BOI.AttributeI("ID").sValue;
                            var Name = BOI.AttributeI("sApplicationName").sValue;
                            nOrg.Add(new CNV { sName = Name, sValue = ID });
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iUserID is missing";
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
            return Json(nOrg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AppLanding(int ID)
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
                XIInfraCache oCache = new XIInfraCache();
                SessionManager.ApplicationID = ID;
                var AppD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, ID.ToString());
                SessionManager.AppName = AppD.sApplicationName;
                var sDatabase = SessionManager.CoreDatabase;
                var RoleID = SessionManager.iRoleID;
                XIInfraRoles oRoleD = new XIInfraRoles();
                oRoleD.RoleID = Convert.ToInt32(RoleID);
                oCR = oRoleD.Get_RoleDefinition(sDatabase);
                oRoleD = (XIInfraRoles)oCR.oResult;
                var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, null, oRoleD.iLayoutID.ToString()); //oDXI.Get_LayoutDefinition(null, oUser.Role.iLayoutID.ToString());
                XIDLayout oLayout = new XIDLayout();
                if (oLayDef != null)
                {
                    oLayout = (XIDLayout)oLayDef;
                    if (oLayout != null && oLayout.ID > 0)
                    {
                        return View("LandingPage", oLayout);
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
            return null;
        }

        #endregion MultiApp

        #region Campaign

        public ActionResult Get_Campaigns()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";//expalin about this method logic
            List<CNV> nOrg = new List<CNV>();
            try
            {
                var iUserID = SessionManager.UserID;
                oTrace.oParams.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                if (iUserID > 0)//check mandatory params are passed or not
                {
                    XIInfraCache oCache = new XIInfraCache();
                    var o1ClickD =(XID1Click) oCache.GetObjectFromCache(XIConstant.Cache1Click, "All Campaign");
                    var Data = o1ClickD.OneClick_Run();
                    if (Data != null && Data.Count() > 0)
                    {
                        int i = 0;
                        foreach (var BOI in Data.Values)
                        {
                            var ID = BOI.AttributeI("ID").sValue;
                            var Name = BOI.AttributeI("sName").sValue;
                            nOrg.Add(new CNV { sName = Name, sValue = ID });
                            if (i == 0)
                            {
                                SessionManager.iCampaignID = Convert.ToInt32(ID);
                            }
                            i++;
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iUserID is missing";
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
            return Json(nOrg, JsonRequestBehavior.AllowGet);
        }

        #endregion Campaign

    }
}

