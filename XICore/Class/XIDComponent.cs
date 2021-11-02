using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using XIDatabase;
using XISystem;
using static XIDatabase.XIDBAPI;

namespace XICore
{
    public class XIDComponent : XIDefinitionBase
    {
        public int ID { get; set; }
        public string sName { get; set; }
        public string sType { get; set; }
        public string sClass { get; set; }
        public string sHTMLPage { get; set; }
        public int FKiApplicationID { get; set; }
        public bool bMatrix { get; set; }
        public string sCode { get; set; }
        [DapperIgnore]
        public int iQSIID { get; set; }
        [DapperIgnore]
        public string sContext { get; set; }
        [DapperIgnore]
        public string sGUID { get; set; }
        [ForeignKey("ID")]
        public virtual List<XIDComponentsNV> XIDComponentsNV { get; set; }
        public virtual List<XIDComponentTrigger> XIDComponentTrigger { get; set; }
        public List<XIDropDown> ddlApplications { get; set; }
        private List<CNV> oMynParams = new List<CNV>();
        public List<CNV> nParams
        {
            get
            {
                return oMynParams;
            }
            set
            {
                oMynParams = value;
            }
        }

        private List<XIDComponentsNV> oMyNVs = new List<XIDComponentsNV>();
        public List<XIDComponentsNV> NVs
        {
            get
            {
                return oMyNVs;
            }
            set
            {
                oMyNVs = value;
            }
        }

        private List<XIDComponentParam> oMyParams = new List<XIDComponentParam>();
        public List<XIDComponentParam> Params
        {
            get
            {
                return oMyParams;
            }
            set
            {
                oMyParams = value;
            }
        }

        private List<XIDComponentTrigger> oMyTriggers = new List<XIDComponentTrigger>();
        public List<XIDComponentTrigger> Triggers
        {
            get
            {
                return oMyTriggers;
            }
            set
            {
                oMyTriggers = value;
            }
        }
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        public CResult LoadComponent(string sContextType, int iContextID, int BODID = 0, bool bIsStepLock = false)
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

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?

            try
            {
                XIDXI oXID = new XIDXI();
                XIDComponent oXIComponent = new XIDComponent();
                XIInfraCache oCache = new XIInfraCache();
                XIDComponent oXICom = new XIDComponent();
                if (!string.IsNullOrEmpty(sContext) && sContext.ToLower() == "fixedtemplate")
                {
                    var oComDef = oXID.Get_ComponentDefinition(sName, ID.ToString());
                    if (oComDef.bOK)
                    {
                        oXICom = (XIDComponent)oComDef.oResult;
                    }
                }
                else
                {
                    oXICom = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, sName, ID.ToString()); //oXID.Get_ComponentDefinition(sName, ID.ToString());
                }
                //(XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, sName, ID.ToString());
                if (oXICom == null)
                {
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading component definition" });
                    return oCResult;
                }
                var XICompD = (XIDComponent)oXICom;
                var Copy = (XIDComponent)XICompD.Clone(XICompD);
                //var Copy = (XIDComponent)(oXIComponent.Clone(oXICom));  //(XIDComponent)oXIComponent.Clone(oXICom);
                oXIComponent = GetParamsByContext(Copy, sContextType, iContextID);
                oXIComponent.Params.Where(m => m.sValue == "{XIP|BODID}").ToList().ForEach(m => m.sValue = BODID.ToString());
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Component Definition Loaded Successfully" });

                List<CNV> Params = new List<CNV>();
                if (nParams != null && nParams.Count() > 0)
                {
                    Params.AddRange(nParams.Select(m => new CNV { sName = m.sName, sValue = m.sValue }));
                    var register = Params.Where(m => m.sName.ToLower() == "register").FirstOrDefault();
                    if (register != null)
                    {
                        if (!string.IsNullOrEmpty(register.sValue))
                        {
                            Params.Add(new CNV { sName = "name", sValue = oXIComponent.sName + "_" + iContextID, sType = "register" });
                        }
                    }
                }

                //Params.Add(new cNameValuePairs { sName = "Context", sValue = sType, sContext = sContext });
                if (sContextType == "QSStep")
                {
                    Params.Add(new CNV { sName = "FKiQSInstanceID", sValue = iQSIID.ToString(), sType = "autoset", sContext = sContext });
                }
                else if (sContextType == "QSStepSection" && sContext.ToLower() != "fixedtemplate")
                {
                    if (iQSIID > 0)
                    {
                        Params.Add(new CNV { sName = "FKiQSInstanceID", sValue = iQSIID.ToString(), sType = "autoset", sContext = sContext });
                    }

                }
                else if (sContextType == "QS")
                {
                    Params.Add(new CNV { sName = "FKiQSInstanceID", sValue = iQSIID.ToString(), sType = "autoset", sContext = sContext });
                }
                if (oXIComponent.Params.Count() > 0)
                {
                    var register = oXIComponent.Params.Where(m => m.sName.ToLower() == "register").FirstOrDefault();
                    if (register != null)
                    {
                        if (!string.IsNullOrEmpty(register.sValue))
                        {
                            Params.Add(new CNV { sName = "name", sValue = oXIComponent.sName + "_" + iContextID, sType = "register" });
                        }
                    }
                }
                //Merger params

                var sSessionID = Params.Where(m => m.sName == "sSessionID").Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sGUID))
                {
                    if (oXIComponent.Params.Count() > 0)
                    {
                        Params.AddRange(oXIComponent.Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue, sContext = sContext }));
                        oCache.SetXIParams(Params, sGUID, sSessionID);
                    }
                    else
                    {
                        Params.AddRange(oXIComponent.NVs.Select(m => new CNV { sName = m.sName, sValue = m.sValue, sContext = sContext }));
                        oCache.SetXIParams(Params, sGUID, sSessionID);
                    }
                }
                else
                {
                    sGUID = "ComponentDirectLoad";
                    Params.AddRange(oXIComponent.Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue, sContext = sContext }));
                    oCache.SetXIParams(Params, sGUID, sSessionID);
                }
                var oComponentParams = new List<CNV>();
                oComponentParams.AddRange(Params.Select(m => new CNV { sName = m.sName, sValue = m.sValue, sType = m.sType, nSubParams = m.nSubParams }));
                oComponentParams.Add(new CNV { sName = "sGUID", sValue = sGUID });
                object Response = null;
                switch (oXIComponent.sName)
                {
                    case "Form Component":
                        XIInfraFormComponent oFC = new XIInfraFormComponent();
                        Response = oFC.XILoad(oComponentParams);
                        break;
                    case "OneClickComponent":
                        XIInfraOneClickComponent oOCC = new XIInfraOneClickComponent();
                        Response = oOCC.XILoad(oComponentParams, bIsStepLock);
                        break;
                    case "XITreeStructure":
                        XIInfraTreeStructureComponent oTSC = new XIInfraTreeStructureComponent();
                        Response = oTSC.XILoad(oComponentParams);
                        break;
                    case "QSComponent":
                        XIInfraQSComponent oQSC = new XIInfraQSComponent();
                        Response = oQSC.XILoad(oComponentParams);
                        break;
                    case "Tab Component":
                        XIInfraTabComponent oTC = new XIInfraTabComponent();
                        Response = oTC.XILoad(oComponentParams);
                        break;
                    case "MenuComponent":
                        XIInfraMenuComponent oMC = new XIInfraMenuComponent();
                        Response = oMC.XILoad(oComponentParams);
                        break;
                    case "Grid Component":
                        XIInfraGridComponent oGC = new XIInfraGridComponent();
                        Response = oGC.XILoad(oComponentParams);
                        break;
                    case "HTML Component":
                        XIInfraHtmlComponent oHC = new XIInfraHtmlComponent();
                        Response = oHC.XILoad(oComponentParams);
                        break;
                    case "InboxComponent":
                        XIInfraInboxComponent oIC = new XIInfraInboxComponent();
                        Response = oIC.XILoad(oComponentParams);
                        break;
                    case "ReportComponent":
                        XIInfraReportComponent oRC = new XIInfraReportComponent();
                        Response = oRC.XILoad(oComponentParams);
                        break;
                    case "XilinkComponent":
                        XIInfraXILinkComponent oXC = new XIInfraXILinkComponent();
                        Response = oXC.XILoad(oComponentParams);
                        break;
                    case "FieldOriginComponent":
                        XIInfraFieldOriginComponent oXFO = new XIInfraFieldOriginComponent();
                        Response = oXFO.XILoad(oComponentParams);
                        break;
                    case "PieChartComponent":
                        XIInfraPieChartComponent oPC = new XIInfraPieChartComponent();
                        Response = oPC.XILoad(oComponentParams);
                        break;
                    case "CombinationChartComponent":
                        XIInfraCombinationChartComponent oCC = new XIInfraCombinationChartComponent();
                        Response = oCC.XILoad(oComponentParams);
                        break;
                    case "GaugeChartComponent":
                        XIInfraGaugeChartComponent oGCC = new XIInfraGaugeChartComponent();
                        Response = oGCC.XILoad(oComponentParams);
                        break;
                    case "DashBoardChartComponent":
                        XIInfraDashBoardChartComponent oDC = new XIInfraDashBoardChartComponent();
                        Response = oDC.XILoad(oComponentParams);
                        break;
                    case "DailerComponent":
                        XIInfraDailerComponent oDaC = new XIInfraDailerComponent();
                        Response = oDaC.XILoad(oComponentParams);
                        break;
                    case "GroupComponent":
                        XIInfraGroupComponent oPG = new XIInfraGroupComponent();
                        Response = oPG.XILoad(oComponentParams);
                        break;
                    case "MappingComponent":
                        XIInfraMappingComponent oMPC = new XIInfraMappingComponent();
                        Response = oMPC.XILoad(oComponentParams);
                        break;
                    case "MultiRowComponent":
                        XIInfraMultiRowComponent oMRC = new XIInfraMultiRowComponent();
                        Response = oMRC.XILoad(oComponentParams);
                        break;
                    case "QuoteReportDataComponent":
                        XIInfraQuoteReportDataComponent oRDC = new XIInfraQuoteReportDataComponent();
                        Response = oRDC.XILoad(oComponentParams);
                        break;
                    case "CheckboxComponent":
                        XIInfraCheckboxComponent oCBC = new XIInfraCheckboxComponent();
                        Response = oCBC.XILoad(oComponentParams);
                        break;
                    case "AM4PriceChartComponent":
                        XIInfraAM4PriceChartComponent oAMPr = new XIInfraAM4PriceChartComponent();
                        Response = oAMPr.XILoad(oComponentParams);
                        break;
                    case "AM4PieChartComponent":
                        XIInfraAM4PieChartComponent oAMP = new XIInfraAM4PieChartComponent();
                        Response = oAMP.XILoad(oComponentParams);
                        break;
                    case "AM4SemiPieChartComponent":
                        XIInfraAM4SemiPieChartComponent oAMSP = new XIInfraAM4SemiPieChartComponent();
                        Response = oAMSP.XILoad(oComponentParams);
                        break;
                    case "AM4BarChartComponent":
                        XIInfraAM4BarChartComponent oAMB = new XIInfraAM4BarChartComponent();
                        Response = oAMB.XILoad(oComponentParams);
                        break;
                    case "AM4HeatChartComponent":
                        XIInfraAM4HeatChartComponent oAMH = new XIInfraAM4HeatChartComponent();
                        Response = oAMH.XILoad(oComponentParams);
                        break;
                    case "AM4LineChartComponent":
                        XIInfraAM4LineChartComponent oAML = new XIInfraAM4LineChartComponent();
                        Response = oAML.XILoad(oComponentParams);
                        break;
                    case "AM4GaugeChartComponent":
                        XIInfraAM4GaugeChartComponent oAMG = new XIInfraAM4GaugeChartComponent();
                        Response = oAMG.XILoad(oComponentParams);
                        break;
                    case "AccountComponent":
                        XIInfraAccountComponent oAC = new XIInfraAccountComponent();
                        Response = oAC.XILoad(oComponentParams);
                        break;
                    case "DynamicTreeComponent":
                        XIInfraDynamicTreeComponent oDT = new XIInfraDynamicTreeComponent();
                        Response = oDT.XILoad(oComponentParams);
                        break;
                    default:
                        break;
                }

                //var URL = oXIComponent.sClass.Split('/').ToList().First();
                //var sClass = oXIComponent.sClass.Split('/').ToList().Last();

                //if (!string.IsNullOrEmpty(sClass))
                //{
                //Creating Instance
                //Assembly exceutable;
                //Type Ltype;
                //object objclass;
                //if (oXIComponent.sType.ToLower() == "External".ToLower())
                //{
                //    exceutable = Assembly.Load(URL);
                //    Ltype = exceutable.GetType(URL + "." + sClass);
                //    objclass = Activator.CreateInstance(Ltype);
                //}
                //else
                //{
                //    exceutable = Assembly.GetExecutingAssembly();
                //    Ltype = exceutable.GetType("XIInfrastructure.XIInfraQSComponent");
                //    objclass = Activator.CreateInstance(Ltype);
                //}

                //Invoking Method
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Component XILoad Method Invoke Started" });
                //MethodInfo method = Ltype.GetMethod("XILoad");
                //object[] parametersArray = new object[] { oComponentParams };
                //object Response = (object)method.Invoke(objclass, parametersArray);
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Component XILoad Method Invoke Completed" });
                if (Response != null)
                {
                    oCR = (CResult)Response;
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oCResult.oResult = oCR.oResult;
                    }
                    else
                    {
                        oCResult.oResult = null;
                    }
                }

                //}
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Executing Component" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult; // always            
        }

        public XIDComponent GetParamsByContext(XIDComponent oXIComponent, string sType, int ID)
        {
            if (string.IsNullOrEmpty(sType))
            {
                oXIComponent.Params.Clear();
                return oXIComponent;
            }
            if (sType.ToLower() == "QSStep".ToLower())
            {
                if (ID != 0)
                {
                    var nParams = oXIComponent.Params.Where(m => m.iStepDefinitionID == ID).ToList();
                    var newParams = oXIComponent.NVs.Select(m => m.sName.ToLower()).ToList().Except(nParams.Select(m => m.sName.ToLower()).ToList()).ToList();
                    foreach (var items in newParams)
                    {
                        var Newparm = new XIDComponentParam();
                        Newparm.ID = 0;
                        Newparm.sName = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sName).FirstOrDefault();
                        Newparm.sValue = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sValue).FirstOrDefault();
                        Newparm.FKiComponentID = oXIComponent.ID;
                        Newparm.iStepDefinitionID = ID;
                        nParams.Add(Newparm);
                    }
                    oXIComponent.Params = nParams;
                }
                else
                {
                    oXIComponent.Params.Clear();
                }
            }
            else if (sType.ToLower() == "QSStepSection".ToLower())
            {
                if (ID != 0)
                {
                    List<XIDComponentParam> Params = new List<XIDComponentParam>();
                    var nParams = oXIComponent.Params.Where(m => m.iStepSectionID == ID).ToList();
                    var Except = oXIComponent.NVs.Select(m => m.sName.ToLower()).ToList().Except(nParams.Select(m => m.sName.ToLower()).ToList()).ToList();
                    Params.AddRange(nParams);
                    foreach (var items in Except)
                    {
                        var NewParam = new XIDComponentParam();
                        NewParam.ID = 0;
                        NewParam.FKiComponentID = oXIComponent.ID;
                        NewParam.iStepSectionID = ID;
                        NewParam.sName = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sName).FirstOrDefault();
                        NewParam.sValue = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sValue).FirstOrDefault();
                        Params.Add(NewParam);
                    }
                    oXIComponent.Params = Params;
                }
                else
                {
                    oXIComponent.Params.Clear();
                }
            }
            else if (sType.ToLower() == "XiLink".ToLower())
            {
                if (ID > 0)
                {
                    var nParams = oXIComponent.Params.Where(m => m.iXiLinkID == ID).ToList();
                    var newParams = oXIComponent.NVs.Select(m => m.sName.ToLower()).ToList().Except(nParams.Select(m => m.sName.ToLower()).ToList()).ToList();
                    foreach (var items in newParams)
                    {
                        var Newparm = new XIDComponentParam();
                        Newparm.ID = 0;
                        Newparm.sName = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sName).FirstOrDefault();
                        Newparm.sValue = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sValue).FirstOrDefault();
                        Newparm.FKiComponentID = oXIComponent.ID;
                        Newparm.iXiLinkID = ID;
                        nParams.Add(Newparm);
                    }
                    oXIComponent.Params = nParams;
                }
                else
                {
                    oXIComponent.Params.Clear();
                }
            }
            else if (sType.ToLower() == "Layout".ToLower())
            {
                if (ID > 0)
                {
                    var nParams = oXIComponent.Params.Where(m => m.iLayoutMappingID == ID).ToList();
                    var newParams = oXIComponent.NVs.Select(m => m.sName.ToLower()).ToList().Except(nParams.Select(m => m.sName.ToLower()).ToList()).ToList();
                    foreach (var items in newParams)
                    {
                        var Newparm = new XIDComponentParam();
                        Newparm.ID = 0;
                        Newparm.sName = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sName).FirstOrDefault();
                        Newparm.sValue = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sValue).FirstOrDefault();
                        Newparm.FKiComponentID = oXIComponent.ID;
                        Newparm.iLayoutMappingID = ID;
                        nParams.Add(Newparm);
                    }
                    oXIComponent.Params = nParams;
                }
                else
                {
                    oXIComponent.Params.Clear();
                }
            }
            else if (sType.ToLower() == "OneClick".ToLower())
            {
                if (ID > 0)
                {
                    var nParams = oXIComponent.Params.Where(m => m.iQueryID == ID).ToList();
                    var newParams = oXIComponent.NVs.Select(m => m.sName.ToLower()).ToList().Except(nParams.Select(m => m.sName.ToLower()).ToList()).ToList();
                    foreach (var items in newParams)
                    {
                        var Newparm = new XIDComponentParam();
                        Newparm.ID = 0;
                        Newparm.sName = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sName).FirstOrDefault();
                        Newparm.sValue = oXIComponent.NVs.Where(m => m.sName.ToLower() == items).Select(m => m.sValue).FirstOrDefault();
                        Newparm.FKiComponentID = oXIComponent.ID;
                        Newparm.iQueryID = ID;
                        nParams.Add(Newparm);
                    }
                    oXIComponent.Params = nParams;
                }
                else
                {
                    oXIComponent.Params.Clear();
                }
            }
            return oXIComponent;
        }

        public void GetComponentParams(string Type, int iID)
        {
            Dictionary<string, object> oParams = new Dictionary<string, object>();
            oParams["FKiComponentID"] = ID;
            if (Type.ToLower() == "layout")
            {
                oParams["iLayoutMappingID"] = iID;
            }
            else if (Type.ToLower() == "xilink")
            {
                oParams["iXiLinkID"] = iID;
            }
            else if (Type.ToLower() == "step")
            {
                oParams["iStepDefinitionID"] = iID;
            }
            else if (Type.ToLower() == "section")
            {
                oParams["iStepSectionID"] = iID;
            }
            else if (Type.ToLower() == "1click")
            {
                oParams["iQueryID"] = iID;
            }
            else if (Type.ToLower() == "Attribute".ToLower())
            {
                oParams["FKiAttributeID"] = iID;
            }
            Params = Connection.Select<XIDComponentParam>("XIComponentParams_T", oParams).ToList();
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
                XIDQS oQSD = new XIDQS();
                if (sName.ToLower() == XIConstant.QSComponent.ToLower())
                {
                    XIDQSStep oStepD = new XIDQSStep();
                    var iQSDID = Params.Where(m => m.sName.ToLower() == "iqsdid").Select(m => m.sValue).FirstOrDefault();
                    oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDID.ToString());
                    if (oQSD.Steps != null && oQSD.Steps.Count() > 0)
                    {
                        //oStepD = oQSD.Steps.Values.Where(m => m.sIsHidden == "off" || m.sIsHidden == null).OrderBy(m => m.iOrder).FirstOrDefault();
                        oStepD = oQSD.Steps.Values.Where(m => m.sIsHidden == "off" || m.sIsHidden == null).OrderBy(m => m.ID).FirstOrDefault();
                        if (oStepD != null)
                        {
                            oStepD.oDefintion = oStepD;
                            var oStepContent = oStepD.Preview();
                            if (oStepContent.bOK && oStepContent.oResult != null)
                            {
                                //oQSD.oContent[XIConstant.ContentStep] = oStepContent.oResult;
                                var oSteDef = (XIDQSStep)oStepContent.oResult;
                                if (oSteDef.iLayoutID > 0)
                                {
                                    XIDLayout oLayoutD = new XIDLayout();
                                    oLayoutD.ID = oSteDef.iLayoutID;
                                    var oLayContent = oLayoutD.Preview();
                                    if (oLayContent.bOK && oLayContent.oResult != null)
                                    {
                                        oStepD.oContent[XIConstant.ContentLayout] = oLayContent.oResult;
                                    }
                                    oQSD.oContent[XIConstant.ContentQuestionSet] = oStepD;
                                }
                            }
                        }
                    }
                    this.oContent[XIConstant.ContentQuestionSet] = oQSD;
                }
                else
                {
                    this.oContent[XIConstant.ContentQuestionSet] = this;
                }
                oCResult.oResult = this;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
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

    }

    public class XIDComponentsNV
    {
        public int ID { get; set; }
        public int FKiComponentID { get; set; }
        public string sName { get; set; }
        public string sValue { get; set; }
        public string sType { get; set; }
    }

    public class XIDComponentParam
    {
        public int ID { get; set; }
        public int FKiComponentID { get; set; }
        public string sName { get; set; }
        public string sValue { get; set; }
        public int iLayoutMappingID { get; set; }
        public int iXiLinkID { get; set; }
        public int iStepDefinitionID { get; set; }
        public int iStepSectionID { get; set; }
        public int iQueryID { get; set; }

    }

    public class XIDComponentTrigger
    {
        public int ID { get; set; }
        public int FKiComponentID { get; set; }
        public string sName { get; set; }
        public string sValue { get; set; }
    }
}