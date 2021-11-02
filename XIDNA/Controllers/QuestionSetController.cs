using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using XIDNA.Models;
using XIDNA.Repository;
using XIDNA.ViewModels;
using XICore;
using XISystem;

namespace XIDNA.Controllers
{

    public class QuestionSetController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHomeRepository HomeRepository;

        public QuestionSetController() : this(new HomeRepository())
        {

        }

        public QuestionSetController(IHomeRepository HomeRepository)
        {
            this.HomeRepository = HomeRepository;
        }
        XIDXI oDXI = new XIDXI();
        CommonRepository Common = new CommonRepository();
        string sSearchUrlstrig = string.Empty;
        #region NonAuthenticated

        [AllowAnonymous]
        public ActionResult StartUP()
        {
            string sDatabase = string.Empty;
            if (string.IsNullOrEmpty(SessionManager.CoreDatabase))
            {
                SessionManager.CoreDatabase = System.Configuration.ConfigurationManager.AppSettings["CoreApplicatoinDatabase"];
            }
            if (string.IsNullOrEmpty(SessionManager.AppName))
            {
                SessionManager.AppName = System.Configuration.ConfigurationManager.AppSettings["CoreApplicatoinName"];
            }
            sDatabase = SessionManager.CoreDatabase;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                oDXI.sCoreDatabase = sDatabase;
                //SessionManager.UserID = 0;
                sSearchUrlstrig = Request.Url.AbsolutePath;
                string sValidateKey = string.Empty;
                string sExternalRefID = string.Empty;
                if (!string.IsNullOrEmpty(Request.Url.Query))
                {
                    char[] chars1 = { '/', '\\', '@', ':', '*', '&', '?' };
                    //string sUrlName = sSearchUrlstrig.TrimStart(chars);
                    var sRequest = Request.Url.Query.TrimStart(chars1);
                    if (sRequest.Contains('&'))
                    {
                        var ReqParams = sRequest.Split('&').ToList();
                        if (ReqParams.Count() > 1)
                        {
                            foreach (var param in ReqParams)
                            {
                                if (param.Contains('='))
                                {
                                    var singleparam = param.Split('=').ToList();
                                    if (singleparam.Count() > 1)
                                    {

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var ReqParams = sRequest.Split('=').ToList();
                        if (ReqParams.Count() > 1)
                        {
                            sValidateKey = ReqParams[0];
                            sExternalRefID = ReqParams[1];
                        }
                    }
                }
                var HostingPath = System.Configuration.ConfigurationManager.AppSettings["HostingPath"];
                if (!string.IsNullOrEmpty(HostingPath))
                {
                    var regex = new Regex(Regex.Escape(HostingPath.ToLower()));
                    sSearchUrlstrig = regex.Replace(sSearchUrlstrig.ToLower(), "", 1);
                    //sSearchUrlstrig = sSearchUrlstrig.ToLower().Replace(HostingPath.ToLower(), "");
                }
                char[] chars = { '/', '\\', '@', ':', '*', '&' };
                string sUrlName = sSearchUrlstrig.TrimStart(chars);
                //Common.SaveErrorLog(sSearchUrlstrig.ToString(), sDatabase);

                //var oURLInfo = Common.GetURLInfo(sUrlName);// dbcontext.XIUrlMappings.Where(x => x.sUrlName.ToLower() == sUrlName.ToLower()).Select(x => x.sActualUrl).FirstOrDefault();
                XIURLMappings oURL = new XIURLMappings();
                var oURLDef = oCache.GetObjectFromCache(XIConstant.CacheURL, sUrlName, null); //oDXI.Get_URLDefinition(sUrlName);
                if (oURLDef != null)
                {
                    oURL = (XIURLMappings)oURLDef;
                    if (oURL != null && oURL.StatusTypeID == 10)
                    {
                        if (oURL.bIsValidateKeyMandatory || (!oURL.bIsValidateKeyMandatory && !string.IsNullOrEmpty(sValidateKey)))
                        {
                            if (!string.IsNullOrEmpty(sValidateKey) || !string.IsNullOrEmpty(oURL.sValidateKey))
                            {
                                if (oURL.sValidateKey.ToLower() == sValidateKey.ToLower() && !string.IsNullOrEmpty(sExternalRefID))
                                {
                                    SessionManager.sExternalRefID = sExternalRefID;
                                }
                                else
                                {
                                    Common.SaveErrorLog("URL: sValidateKey is not matching", sDatabase);
                                    return PartialView("_NotFound");
                                }
                            }
                        }
                        if (oURL.sType.ToLower() == xiEnumSystem.EnumURLType.Page.ToString().ToLower() && !string.IsNullOrEmpty(oURL.sPage))
                        {
                            return PartialView(oURL.sPage);
                        }
                        else if (oURL.sType.ToLower() == xiEnumSystem.EnumURLType.QuestionSet.ToString().ToLower())
                        {
                            if (oURL.FKiSourceID > 0)
                            {
                                SessionManager.QSSourceID = oURL.FKiSourceID.ToString();
                            }
                            else
                            {
                                SessionManager.QSSourceID = "0";
                            }
                            if (!string.IsNullOrEmpty(oURL.sUrlName))
                            {
                                SessionManager.QSName = oURL.sUrlName;
                            }
                            XIDApplication oAPP = new XIDApplication();
                            string sAppName = GetUrlName(oURL.sActualUrl, sDatabase);
                            var oAppDef = oCache.GetObjectFromCache(XIConstant.CacheApplication, sAppName, null); //oDXI.Get_ApplicationDefinition(sAppName);

                            if (oAppDef != null)
                            {
                                oAPP = (XIDApplication)oAppDef;
                                SessionManager.AppName = oAPP.sApplicationName;
                                SessionManager.ApplicationID = oAPP.ID;
                                var sOrgName = SessionManager.OrganisationName;
                                XIDOrganisation oOrg = new XIDOrganisation();
                                oDXI.sOrgDatabase = SessionManager.CoreDatabase;
                                var oOrgDef = oDXI.Get_OrgDefinition(sOrgName);
                                if (oOrgDef.xiStatus == 0 && oOrgDef.oResult != null)
                                {
                                    oOrg = (XIDOrganisation)oOrgDef.oResult;
                                    SessionManager.OrgDatabase = oOrg.DatabaseName;
                                    SessionManager.OrganizationID = oOrg.ID;
                                    SessionManager.sGUID = Guid.NewGuid().ToString();
                                    var sLayoutName = SessionManager.LayoutName;
                                    XIDLayout oLayout = new XIDLayout();
                                    var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, sLayoutName, null); //oDXI.Get_LayoutDefinition(sLayoutName, "0");
                                    if (oLayDef != null)
                                    {
                                        oLayout = (XIDLayout)oLayDef;
                                        oLayout.sGUID = Guid.NewGuid().ToString();
                                        if (!string.IsNullOrEmpty(oLayout.sSiloAccess))
                                        {
                                            var sWebSilo = System.Configuration.ConfigurationManager.AppSettings["SiloAccess"];
                                            var sWebSilos = sWebSilo.Split('|').ToList().ConvertAll(m => m.ToLower());
                                            var sLayoutSilos = oLayout.sSiloAccess.Split('|').ToList().ConvertAll(m => m.ToLower());
                                            var IsMatch = sLayoutSilos.Intersect(sWebSilos);
                                            if (IsMatch.Count() > 0)
                                            {

                                            }
                                            else
                                            {
                                                Common.SaveErrorLog("URL: Silo access denied", sDatabase);
                                                return RedirectToAction("NotFound", "QuestionSet");
                                            }
                                        }
                                        if (oLayout.ID > 0)
                                        {
                                            var sUniqueID = CheckCookie(sDatabase);
                                            SessionManager.UserUniqueID = sUniqueID;
                                            //var Layout = Common.GetLayoutDetails(iLayoutID, 0, 0, 0, null, 0, sOrgName, sDatabase);
                                            if (!string.IsNullOrEmpty(oLayout.sThemeName))
                                            {
                                                SessionManager.Theme = oLayout.sThemeName;
                                            }
                                            var sReferenceID = string.Empty;
                                            List<CNV> oWhereParams = new List<CNV>();
                                            XIIXI oIXI = new XIIXI();
                                            oWhereParams.Add(new CNV { sName = "sKey", sValue = "public id reference" });
                                            oWhereParams.Add(new CNV { sName = "FKiAppID", sValue = oAPP.ID.ToString() });
                                            oWhereParams.Add(new CNV { sName = "FKiOrgID", sValue = oOrg.ID.ToString() });
                                            var oBOI = oIXI.BOI("XIConfig_T", null, null, oWhereParams);
                                            if (oBOI != null && oBOI.Attributes != null)
                                            {
                                                sReferenceID = oBOI.AttributeI("sValue").sValue;
                                                SessionManager.sReference = sReferenceID;
                                            }
                                            if (oLayout.Authentication.ToLower() == "NonAuthenticated".ToLower())
                                            {
                                                //Add client cookie
                                                var sUserCookie = AddUserCookie(sDatabase);
                                                return View("QuestionSet", oLayout);
                                            }
                                            else
                                            {
                                                return RedirectToAction("Login", "Account");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return RedirectToAction("Login", "Account");
                                }
                            }
                            else
                            {
                                return RedirectToAction("Login", "Account");
                            }
                        }
                        else if (oURL.sType.ToLower() == xiEnumSystem.EnumURLType.Application.ToString().ToLower())
                        {

                            XIDApplication oAPP = new XIDApplication();
                            var Keywords = oURL.sActualUrl.Substring(1, oURL.sActualUrl.Length - 1).Split('/').ToList();
                            if (Keywords.Count > 1)
                            {
                                string sAppName = GetUrlName(oURL.sActualUrl, sDatabase);
                                var oAppDef = oCache.GetObjectFromCache(XIConstant.CacheApplication, sAppName, null); //oDXI.Get_ApplicationDefinition(sAppName);
                                if (oAppDef != null)
                                {
                                    oAPP = (XIDApplication)oAppDef;
                                    SessionManager.AppName = oAPP.sApplicationName;
                                    SessionManager.ApplicationID = oAPP.ID;
                                    var sOrgName = SessionManager.OrganisationName;
                                    XIDOrganisation oOrg = new XIDOrganisation();
                                    oDXI.sOrgDatabase = SessionManager.CoreDatabase;
                                    ViewBag.Logo = oAPP.sLogo;
                                    var oOrgDef = oDXI.Get_OrgDefinition(sOrgName);
                                    if (oOrgDef.xiStatus == 0 && oOrgDef.oResult != null)
                                    {
                                        oOrg = (XIDOrganisation)oOrgDef.oResult;
                                        SessionManager.OrgDatabase = oOrg.DatabaseName;
                                        var sLayoutName = SessionManager.LayoutName;
                                        XIDLayout oLayout = new XIDLayout();
                                        var oLayDef = oCache.GetObjectFromCache(XIConstant.CacheLayout, sLayoutName, null); //oDXI.Get_LayoutDefinition(sLayoutName, "0");
                                        if (oLayDef != null)
                                        {
                                            oLayout = (XIDLayout)oLayDef;
                                            oLayout.sGUID = Guid.NewGuid().ToString();
                                            if (oLayout.ID > 0)
                                            {
                                                var sUniqueID = CheckCookie(sDatabase);
                                                SessionManager.UserUniqueID = sUniqueID;
                                                SessionManager.Logo = oAPP.sLogo;
                                                //var Layout = Common.GetLayoutDetails(iLayoutID, 0, 0, 0, null, 0, sOrgName, sDatabase);
                                                if (!string.IsNullOrEmpty(oAPP.sTheme))
                                                {
                                                    SessionManager.Theme = oAPP.sTheme;
                                                }
                                                //Add client cookie
                                                var sUserCookie = AddUserCookie(sDatabase);
                                                return View("Login", oLayout);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var oAppDef = oCache.GetObjectFromCache(XIConstant.CacheApplication, oURL.sActualUrl, null); //oDXI.Get_ApplicationDefinition(sUrlName);
                                if (oAppDef != null)
                                {
                                    oAPP = (XIDApplication)oAppDef;
                                    SessionManager.CoreDatabase = oAPP.sDatabaseName;
                                    SessionManager.Logo = oAPP.sLogo;
                                    SessionManager.AppName = oURL.sUrlName;
                                    SessionManager.ApplicationID = oAPP.ID;
                                    SessionManager.OrganizationID = oURL.OrganisationID;
                                    SessionManager.bNannoApp = oAPP.bNannoApp;
                                    if (!string.IsNullOrEmpty(oAPP.sTheme))
                                    {
                                        SessionManager.Theme = oAPP.sTheme;
                                    }
                                }
                                var sReferenceID = string.Empty;
                                List<CNV> oWhereParams = new List<CNV>();
                                XIIXI oIXI = new XIIXI();
                                oWhereParams.Add(new CNV { sName = "sKey", sValue = "internal id reference" });
                                oWhereParams.Add(new CNV { sName = "FKiAppID", sValue = oAPP.ID.ToString() });
                                oWhereParams.Add(new CNV { sName = "FKiOrgID", sValue = oURL.OrganisationID.ToString() });
                                var oBOI = oIXI.BOI("XIConfig_T", null, null, oWhereParams);
                                if (oBOI != null && oBOI.Attributes != null)
                                {
                                    sReferenceID = oBOI.AttributeI("sValue").sValue;
                                    SessionManager.sReference = sReferenceID;
                                }
                                return RedirectToAction("Login", "Account");
                            }
                            
                        }
                        else if (oURL.sType.ToLower() == xiEnumSystem.EnumURLType.Organisation.ToString().ToLower())
                        {
                            XIDApplication oAPP = new XIDApplication();
                            var oAppDef = oCache.GetObjectFromCache(XIConstant.CacheApplication, null, oURL.FKiApplicationID.ToString()); //oDXI.Get_ApplicationDefinition(sUrlName);
                            if (oAppDef != null)
                            {
                                oAPP = (XIDApplication)oAppDef;
                                SessionManager.CoreDatabase = oAPP.sDatabaseName;
                                SessionManager.Logo = oAPP.sLogo;
                                SessionManager.AppName = oURL.sUrlName;// oAPP.sApplicationName;
                                SessionManager.ApplicationID = oAPP.ID;
                                SessionManager.OrganizationID = oURL.OrganisationID;
                                SessionManager.bNannoApp = oAPP.bNannoApp;
                                if (!string.IsNullOrEmpty(oAPP.sTheme))
                                {
                                    SessionManager.Theme = oAPP.sTheme;
                                }
                                if (!string.IsNullOrEmpty(oURL.sPage))
                                {
                                    SessionManager.sHomePage = oURL.sPage;
                                    return RedirectToAction("Page", "Home");
                                }
                            }
                            return RedirectToAction("Login", "Account");
                        }
                        else if (oURL.sType.ToLower() == xiEnumSystem.EnumURLType.Client.ToString().ToLower())
                        {
                            XIDApplication oAPP = new XIDApplication();
                            var oAppDef = oCache.GetObjectFromCache(XIConstant.CacheApplication, sUrlName, null); //oDXI.Get_ApplicationDefinition(sUrlName);
                            if (oAppDef != null)
                            {
                                oAPP = (XIDApplication)oAppDef;
                                SessionManager.CoreDatabase = oAPP.sDatabaseName;
                                SessionManager.Logo = oAPP.sLogo;
                                SessionManager.AppName = oAPP.sApplicationName;
                                if (!string.IsNullOrEmpty(oAPP.sTheme))
                                {
                                    SessionManager.Theme = oAPP.sTheme;
                                }
                            }
                            //Add client cookie
                            var sUserCookie = AddUserCookie(sDatabase);
                            return RedirectToAction("ClientLogin", "Account");
                        }
                    }
                    else if(sUrlName == "XIDNA")
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        Common.SaveErrorLog("URL: URL not found or status is inactive, URL Name: " + sUrlName, sDatabase);
                        return PartialView("_NotFound");
                    }
                }
                Common.SaveErrorLog("Call to Error file " + sSearchUrlstrig.ToString(), sDatabase);
                return PartialView("~/Views/Shared/Error.cshtml");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }

        }

        private string CheckCookie(string sDatabase)
        {
            try
            {
                if (Request.Cookies["UserQuestionSet"] != null)
                {
                    var sUniqueID = Request.Cookies["UserQuestionSet"].Value;
                    return sUniqueID;
                }
                else
                {
                    var sUniqueID = Guid.NewGuid().ToString();
                    HttpCookie UserCookie = new HttpCookie("UserQuestionSet");
                    UserCookie.Value = sUniqueID;
                    UserCookie.Expires = DateTime.Now.AddYears(1);
                    Response.Cookies.Add(UserCookie);

                    //SaveUserCookieDetails(sUniqueID, sDatabase);
                    return sUniqueID;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        private string AddUserCookie(string sDatabase)
        {
            try
            {
                if (Request.Cookies["mhclientportal"] != null)
                {
                    SessionManager.UserCookie = "1";
                    var sUniqueID = Request.Cookies["mhclientportal"].Value;
                    //Common.SaveErrorLog("Cookie Yes", sDatabase);
                    return sUniqueID;
                }
                else
                {
                    var sUniqueID = Guid.NewGuid().ToString();
                    HttpCookie UserCookie = new HttpCookie("mhclientportal");
                    UserCookie.Value = System.Configuration.ConfigurationManager.AppSettings["ApplicationPath"] + "_" + 0;
                    UserCookie.Expires = DateTime.Now.AddYears(1);
                    Response.Cookies.Add(UserCookie);
                    SessionManager.UserCookie = "0";
                    //Common.SaveErrorLog("Cookie No", sDatabase);
                    //SaveUserCookieDetails(sUniqueID, sDatabase);
                    return sUniqueID;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        private VMCustomResponse SaveUserCookieDetails(string sUniqueID, string sDatabase)
        {
            try
            {
                var sOrgName = SessionManager.OrganisationName;

                var Repsonse = HomeRepository.SaveUserCookieDetails(sUniqueID, sDatabase, sOrgName);
                return Repsonse;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public string GetUrlName(string InputUrl, string sDatabase)
        {
            try
            {
                if (!string.IsNullOrEmpty(InputUrl))
                {
                    var HostingPath = System.Configuration.ConfigurationManager.AppSettings["HostingPath"];
                    if (!string.IsNullOrEmpty(HostingPath))
                    {
                        InputUrl = InputUrl.Replace(HostingPath, "");
                    }
                    var Keywords = InputUrl.Substring(1, InputUrl.Length - 1).Split('/').ToList();
                    SessionManager.OrganisationName = RemoveSpecialCharacters(Keywords[1], sDatabase);
                    SessionManager.LayoutName = Keywords[2].ToString(); //RemoveSpecialCharacters(Keywords[2], sDatabase);
                    //remove Last special characters
                    string result = string.Empty;
                    char[] chars = { '/', '\\', '@', ':', '*', '&' };
                    result = InputUrl.Trim(chars);
                    int Index_ = result.IndexOf('/');
                    string strfinal = result.Substring(0, Index_);
                    XIInfraCache oCache = new XIInfraCache();
                    var sAppName = RemoveSpecialCharacters(strfinal, sDatabase);
                    XIDApplication oAPP = new XIDApplication();
                    var oAppDef = oCache.GetObjectFromCache(XIConstant.CacheApplication, sAppName, null);
                    if (oAppDef != null)
                    {
                        oAPP = (XIDApplication)oAppDef;
                        SessionManager.CoreDatabase = oAPP.sDatabaseName;
                    }
                    return RemoveSpecialCharacters(strfinal, sDatabase);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                throw ex;
            }
        }

        public string RemoveSpecialCharacters(string str, string sDatabase)
        {
            try
            {
                return Regex.Replace(str, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                throw ex;
            }
        }

        public ActionResult NotFound()
        {
            return PartialView("_NotFound");
        }

        #endregion NonAuthenticated

    }
}