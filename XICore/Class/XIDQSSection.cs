using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using XISystem;
using XIDatabase;
using System.Configuration;

namespace XICore
{
    public class XIDQSSection : XIDefinitionBase
    {
        public XIDQSSection()
        {

        }

        public int ID { get; set; }
        public int FKiStepDefinitionID { get; set; }
        public int iDisplayAs { get; set; }
        public string sName { get; set; }
        public string sCode { get; set; }
        public string sIsHidden { get; set; }
        public bool bIsGroup { get; set; }
        public string sGroupDescription { get; set; }
        public string sGroupLabel { get; set; }
        public int iXIComponentID { get; set; }
        public int i1ClickID { get; set; }
        public string HTMLContent { get; set; }
        public decimal iOrder { get; set; }
        public bool bIsHidden { get; set; }
        public List<XIDFieldDefinition> FieldDefinitions { get; set; }
        public List<string> QSLinkCodes { get; set; }        //auto complete XiLink Codes
        public List<XIDropDown> ddlXIComponents { get; set; }
        public List<XIDropDown> ddlOneClicks { get; set; }
        public List<XIDQSSection> Sections { get; set; }
        public List<XIDropDown> oQSStepddl { get; set; }
        public int FKiAppID { get; set; }
        public int iOrgID { get; set; }
        public Dictionary<string, string> XICodes { get; set; }  //Auto Complete XILink Codes
        public List<XIDropDown> XIFields { get; set; }//auto complete fields
        public Dictionary<string, string> XILinks { get; set; }//auto complete fields
        public List<XIDropDown> ddlContent { get; set; }
        public List<string> XIFieldValues { get; set; }
        public List<XIDropDown> ddlLayouts { get; set; }
        public int FKiParentSectionID { get; set; }

        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);

        private XIDComponent oMyCompoent = new XIDComponent();
        public XIDComponent ComponentDefinition
        {
            get
            {
                return oMyCompoent;
            }
            set
            {
                oMyCompoent = value;
            }
        }

        private Dictionary<string, XIDFieldDefinition> oMyFieldDefs = new Dictionary<string, XIDFieldDefinition>();
        public Dictionary<string, XIDFieldDefinition> FieldDefs
        {
            get
            {
                return oMyFieldDefs;
            }
            set
            {
                oMyFieldDefs = value;
            }
        }

        private Dictionary<string, XIQSLink> oMyQSLinks = new Dictionary<string, XIQSLink>();
        public Dictionary<string, XIQSLink> QSLinks
        {
            get
            {
                return oMyQSLinks;
            }
            set
            {
                oMyQSLinks = value;
            }
        }

        private Dictionary<string, XIDValue> oMyXIValues = new Dictionary<string, XIDValue>();

        public Dictionary<string, XIDValue> XIValues
        {
            get
            {
                return oMyXIValues;
            }
            set
            {
                oMyXIValues = value;
            }
        }
        public XIDValue XIValueD(string sValueName)
        {
            XIDValue oThisValueD = null/* TODO Change to default(_) if this is not a reference type */;

            // Ravi should have done a lot of this


            sValueName = sValueName.ToLower();


            if (oMyXIValues.ContainsKey(sValueName))
            {
            }
            else
            {
            }

            return oThisValueD;
        }

        private Dictionary<string, XIDScript> oMyScripts = new Dictionary<string, XIDScript>();
        public Dictionary<string, XIDScript> Scripts
        {
            get
            {
                return oMyScripts;
            }
            set
            {
                oMyScripts = value;
            }
        }

        public CResult Preview()
        {
            CResult oCResult = new CResult(); // always
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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?

            try
            {
                XIInfraCache oCache = new XIInfraCache();
                XIDQSSection oSecD = new XIDQSSection();
                oSecD = (XIDQSSection)oDefintion;
                XIDQSSection oSecC = new XIDQSSection();
                oSecC = (XIDQSSection)oSecD.Clone(oSecD);
                if (oSecC.iXIComponentID > 0)
                {
                    XIDComponent oCompD = new XIDComponent();
                    XIDComponent oCompC = new XIDComponent();
                    if (oSecC.iXIComponentID > 0)
                    {
                        oCompD = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, oSecC.iXIComponentID.ToString());
                        oCompC = (XIDComponent)oCompD.Clone(oCompD);
                        oCompC.GetComponentParams("section", oSecC.ID);
                    }
                    else
                    {
                        oCompC = (XIDComponent)oSecC.ComponentDefinition.Clone(oSecC);
                    }

                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oSecC.oContent[XIConstant.ContentXIComponent] = oCompC;
                    //var oCompContent = oCompC.Preview();
                    //if(oCompContent.bOK && oCompContent.oResult != null)
                    //{
                    //    oSecC.oContent[XIConstant.ContentXIComponent] = oCompContent.oResult;
                    //    //oDefinition.oContent[XIConstant.ContentSection] = oSecC;
                    //}
                    oCResult.oResult = oSecC;
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading XiLink Definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #region QSSectionConfigComponent

        public CResult Get_QSSectionConfigDef()
        {
            CResult oCResult = new CResult(); // always
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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                XIDQSSection oQSSec = new XIDQSSection();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (ID != 0)
                {
                    Params["ID"] = ID;
                    oQSSec = Connection.Select<XIDQSSection>("XIStepSectionDefinition_T", Params).FirstOrDefault();
                }

                //All QuestionSet Steps DropDowns
                List<XIDropDown> oQSStepDDL = new List<XIDropDown>();
                Dictionary<string, object> oQSParam = new Dictionary<string, object>();
                //oQSParam["FKiApplicationID"] = FKiAppID;
                //oQSParam["OrganisationID"] = iOrgID;
                var oQSStepDef = Connection.Select<XIDQSStep>("XIQSStepDefinition_T", oQSParam).ToList();
                oQSStepDDL = oQSStepDef.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();
                oQSStepDDL.Insert(0, new XIDropDown
                {
                    text = "--Select--",
                    Value = 0
                });
                oQSSec.oQSStepddl = oQSStepDDL;

                //AutoComplete For QSLink
                Dictionary<string, string> QSLink = new Dictionary<string, string>();
                Dictionary<string, object> oQSLinks = new Dictionary<string, object>();
                var QSLinkDef = Connection.Select<XIQSLink>("XIQSLinkDefinition_T", oQSLinks).ToList();
                //var oQSLinkDef = QSLinkDef.ToList().Select(m => m.sCode).Distinct().ToList();
                QSLink = QSLinkDef.ToList().Select(m => m.sCode).Distinct().ToDictionary(x => x, x => x);
                //foreach (var items in oQSLinkDef)
                //{
                //    QSLink[items] = items;
                //}
                oQSSec.XICodes = QSLink;

                //FieldOrigins DropDowns
                List<XIDropDown> oFieldDDL = new List<XIDropDown>();
                Dictionary<string, object> oFO = new Dictionary<string, object>();
                //oFO["FKiApplicationID"] = FKiAppID;
                //oFO["FKiOrganisationID"] = iOrgID;
                var oFODef = Connection.Select<XIDFieldOrigin>("XIFieldOrigin_T", oFO).ToList();
                oFieldDDL = oFODef.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();
                oQSSec.XIFields = oFieldDDL;

                //AutoComplete For Xilinks
                Dictionary<string, string> XiLinks = new Dictionary<string, string>();
                Dictionary<string, object> oLinks = new Dictionary<string, object>();
                oLinks["FKiApplicationID"] = FKiAppID;
                oLinks["OrganisationID"] = iOrgID;
                var oXiLinkDef = Connection.Select<XILink>("XILink_T", oLinks).ToList();
                foreach (var items in oXiLinkDef)
                {
                    XiLinks[items.Name] = items.Name;
                }
                oQSSec.XILinks = XiLinks;

                //FieldDefination DropDowns
                List<string> FieldValues = new List<string>();
                Dictionary<string, object> oFields = new Dictionary<string, object>();
                oFields["FKiApplicationID"] = FKiAppID;
                oFields["OrganisationID"] = iOrgID;
                oFields["FKiXIStepDefinitionID"] = ID;
                oFields["FKiStepSectionID"] = 0;
                var oFieldDef = Connection.Select<XIDFieldDefinition>("XIFieldDefinition_T", oFields).ToList();
                var lFieldDef = oFieldDef.Select(m => m.FKiXIFieldOriginID).ToList();
                foreach (var item in lFieldDef)
                {
                    var lXiFields = oFODef.Where(m => m.ID == item).ToList();
                    foreach (var items in lXiFields)
                    {
                        FieldValues.Add(items.sName);
                    }
                }
                oQSSec.XIFieldValues = FieldValues;

                if (ID > 0)
                {
                    Dictionary<string, object> Param1 = new Dictionary<string, object>();
                    Param1["FKiStepDefinitionID"] = oQSSec.FKiStepDefinitionID;
                    var Sections = Connection.Select<XIDQSSection>("XIStepSectionDefinition_T", Param1).ToList();
                    oQSSec.Sections = new List<XIDQSSection>();
                    if (Sections != null && Sections.Count() > 0)
                    {
                        foreach (var items in Sections)
                        {
                            if (((xiEnumSystem.xiSectionContent)items.iDisplayAs).ToString() == xiEnumSystem.xiSectionContent.Fields.ToString())
                            {
                                Dictionary<string, object> oField = new Dictionary<string, object>();
                                oField["FKiStepSectionID"] = items.ID;
                                var oFieldDef1 = Connection.Select<XIDFieldDefinition>("XIFieldDefinition_T", oField).ToList();
                                foreach (var item in oFieldDef1)
                                {
                                    var FieldDef = oFODef.Where(m => m.ID == item.FKiXIFieldOriginID).FirstOrDefault();
                                    item.FieldOrigin = FieldDef;
                                }
                                items.FieldDefinitions = oFieldDef1;
                            }
                        }
                        oQSSec.Sections = Sections.OrderBy(m => m.iOrder).ToList();
                    }
                }
                else
                {
                    oQSSec.Sections = new List<XIDQSSection>();
                }

                //XILayout DropDowns
                List<XIDropDown> oLayoutDDL = new List<XIDropDown>();
                Dictionary<string, object> oLayout = new Dictionary<string, object>();
                oFO["FKiApplicationID"] = FKiAppID;
                oFO["OrganisationID"] = iOrgID;
                var oLayDef = Connection.Select<XIDLayout>("XILayout_T", oLayout).ToList();
                oLayoutDDL = oLayDef.Select(m => new XIDropDown { Value = m.ID, text = m.LayoutName }).ToList();
                oLayoutDDL.Insert(0, new XIDropDown
                {
                    text = "--Select--",
                    Value = 0
                });
                oQSSec.ddlLayouts = oLayoutDDL;
                oQSSec.ddlContent = new List<XIDropDown>();

                //XIComponents DropDowns
                List<XIDropDown> oComponentDDL = new List<XIDropDown>();
                Dictionary<string, object> oCompo = new Dictionary<string, object>();
                var oComDef = Connection.Select<XIDComponent>("XIComponents_XC_T", oCompo).ToList();
                oComponentDDL = oComDef.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();
                oComponentDDL.Insert(0, new XIDropDown
                {
                    text = "--Select--",
                    Value = 0
                });
                oQSSec.ddlXIComponents = oComponentDDL;

                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oQSSec;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult;
        }

        #endregion QSSectionConfigComponent
    }
}