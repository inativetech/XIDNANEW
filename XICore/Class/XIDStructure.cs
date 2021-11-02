﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using XIDatabase;
using XISystem;
using static XIDatabase.XIDBAPI;

namespace XICore
{
    public class XIStructure
    {
        public string sName { get; set; }
        public string sCode { get; set; }

        private List<XIDStructure> oMyNodes;
        public List<XIDStructure> oNodes
        {
            get
            {
                return oMyNodes;
            }
            set
            {
                oMyNodes = value;
            }
        }
    }
    public class XICopyParams
    {
        public string sBOName { get; set; }
        private Dictionary<string, XIIAttribute> oMyAttributes = new Dictionary<string, XIIAttribute>(StringComparer.CurrentCultureIgnoreCase);

        public Dictionary<string, XIIAttribute> Attributes
        {
            get
            {
                return oMyAttributes;
            }
            set
            {
                oMyAttributes = value;
            }
        }
        public XIIAttribute AttributeI(string sAttributeName)
        {
            XIIAttribute oThisAttrI = new XIIAttribute();
            if (oMyAttributes.ContainsKey(sAttributeName))
            {
                oThisAttrI = oMyAttributes[sAttributeName];
            }
            else
            {
            }
            return oThisAttrI;
        }
    }

    public class XIDStructure : XIInstanceBase
    {
        public long ID { get; set; }
        public string FKiParentID { get; set; }
        public string sStructureName { get; set; }
        public string sName { get; set; }
        public string sMode { get; set; }
        public int BOID { get; set; }
        public string sBO { get; set; }
        public int FKi1ClickID { get; set; }
        public int FKiXIApplicationID { get; set; }
        public bool bMasterEntity { get; set; }
        public bool bIsAutoCreateDone { get; set; }
        public string sCode { get; set; }
        public string sType { get; set; }
        public int iOrder { get; set; }
        public bool bIsVisible { get; set; }
        public string sParentFKColumn { get; set; }
        public string sLinkingType { get; set; }
        public int StatusTypeID { get; set; }
        public string FKiStepDefinitionID { get; set; }
        public string sOutputArea { get; set; }
        //IDE
        public int FKiVisualisationID { get; set; }
        [DapperIgnore]
        public string sContext { get; set; }
        [DapperIgnore]
        public int i1ClickID { get; set; }
        [DapperIgnore]
        public string sInsName { get; set; }
        [DapperIgnore]
        public string sInsID { get; set; }
        [DapperIgnore]
        public string sOutputContent { get; set; }
        [DapperIgnore]
        public string sSavingType { get; set; }
        [DapperIgnore]
        public List<XIDropDown> BOList { get; set; }
        [DapperIgnore]
        public string FKiStepDefinitionName { get; set; }
        [DapperIgnore]
        public Dictionary<string, string> AllQSSteps { get; set; }
        public List<XIDropDown> ddlAllBOs { get; set; }
        public string sConnectionString { get; set; }
        public string Query { get; set; }
        public int iInstanceID { get; set; }
        public string sSubStructName { get; set; }
        public string sNameAttribute { get; set; }
        public string sSearchText { get; set; }

        public string sCoreDataBase { get; set; }
        public string sOrgDataBase { get; set; }
        public int iOrgId { get; set; }
        //public XIBOInstance oStructureInstance { get; set; }
        public Dictionary<string, List<XIIBO>> oStructureInstance { get; set; }
        public CResult oCResult = new CResult();
        public Dictionary<string, XIIValue> XIIValues { get; set; }
        public XIDStructureDetail oStructureDetails { get; set; }
        private int iMyParentQSIID;
        public int iParentQSIID
        {
            get
            {

                return iMyParentQSIID;
            }
            set
            {
                iMyParentQSIID = value;
            }
        }
        public bool bIsLoadParent { get; set; }
        
        public XIDStructure oSubStructD { get; set; }

        private XID1Click My1ClickDef;
        public XID1Click OneClickD
        {
            get
            {
                return My1ClickDef;
            }
            set
            {
                My1ClickDef = value;
            }
        }

        public List<XICopyParams> xiCopyParams = new List<XICopyParams>();

        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        public CResult Get_XIStructureDefinition()
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?

            try
            {
                List<XIDStructure> oXIStructure = new List<XIDStructure>();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (sCode != null)
                {
                    Params["sCode"] = sCode;
                }
                if (BOID != 0)
                {
                    Params["BOID"] = BOID;
                }
                if (FKiParentID != null)
                {
                    Params["FKiParentID"] = FKiParentID;
                }
                if (sBO != null)
                {
                    Params["sBO"] = sBO;
                }
                oXIStructure = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
                oCResult.oResult = oXIStructure;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult; // always

            //return oURL;
        }
        public XIDStructure oSubStruct(string sSubStructureName)
        {
            XIDStructure oSubstructureDef = new XIDStructure();
            try
            {
                if (this.oCResult.xiStatus != xiEnumSystem.xiFuncResult.xiError)
                {
                    XIIQS oXIIQS = new XIIQS();
                    Dictionary<string, List<XIIBO>> oStructureInstance = new Dictionary<string, List<XIIBO>>();
                    if (this.oParent != null)
                    {
                        oStructureInstance = (Dictionary<string, List<XIIBO>>)this.oParent;
                    }
                    Dictionary<string, List<XIIBO>> oSubStructureInstance = new Dictionary<string, List<XIIBO>>();
                    sBO = sSubStructureName;
                    var oBOI = this.BOI;
                    List<XIDStructure> oThisValueI = (List<XIDStructure>)this.oDefintion;
                    oSubstructureDef = oThisValueI.Where(x => x.sBO.ToLower() == sSubStructureName.ToLower()).FirstOrDefault();
                    oSubstructureDef.BOI = oBOI;
                    if (oStructureInstance.ContainsKey(sSubStructureName))
                    {
                        oSubStructureInstance.Add(sSubStructureName, oStructureInstance[sSubStructureName]);
                        var oSubStructureInstanceResult = oStructureInstance[sSubStructureName].FirstOrDefault();
                        oSubstructureDef.XIIValues = oSubStructureInstanceResult.XIIValues;
                        oSubstructureDef.oParent = oSubStructureInstance;
                    }
                    else
                    {
                        var oSubStructureInstanceValuesList = oStructureInstance.Values.FirstOrDefault().Select(x => x.SubChildI).FirstOrDefault();
                        if (oSubStructureInstanceValuesList.ContainsKey(sSubStructureName))
                        {
                            var oSubStructureInstanceValue = oSubStructureInstanceValuesList[sSubStructureName];
                            oSubStructureInstance.Add(sSubStructureName, oSubStructureInstanceValuesList[sSubStructureName]);
                            oSubstructureDef.oParent = oSubStructureInstance;
                            oSubstructureDef.BOI = oSubStructureInstanceValue.FirstOrDefault();
                            oSubstructureDef.XIIValues = oSubStructureInstanceValue.FirstOrDefault().XIIValues;
                        }
                    }
                    oSubstructureDef.oDefintion = oThisValueI;
                    oXIIQS.oParent = oSubstructureDef.oParent;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oSubstructureDef;
        }

        public XIBOInstance XILoad(string sLoadType = "", bool bIsXILoad = false)
        {
            // CResult oCResult = new CResult();
            XIBOInstance oBOI = new XIBOInstance();
            try
            {
                List<XIIBO> oBIList = new List<XIIBO>();
                List<XIDStructure> oStruture = (List<XIDStructure>)this.oDefintion;
                var oBI = this.BOI;
                oBIList.Add(oBI);
                var oMainstructureList = oStruture.Where(m => m.BOID == BOID).FirstOrDefault();
                oStruture = oStruture.Where(m => m.BOID != BOID).ToList();
                var oStructureInstance = GetStructureBoInstance(oBIList, oMainstructureList.sBO, Convert.ToInt32(oMainstructureList.ID), oStruture, sLoadType, bIsXILoad);
                if (oStructureInstance.bOK && oStructureInstance.oResult != null)
                {
                    var oStructureInstanceResult = (Dictionary<string, List<XIIBO>>)oStructureInstance.oResult;
                    oBOI.oStructureInstance = oStructureInstanceResult;
                    oBOI.oParent = oStructureInstanceResult.Values.FirstOrDefault().Select(x => x.SubChildI).FirstOrDefault();
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    if (oBOI.BOI == null)
                    {
                        oBOI.BOI = oBI;
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oBOI;
        }
        public XIIBOList OneClick_Execute()
        {
            int iDataSourceID = 0; XIIBOList oBOIIList = new XIIBOList();
            var oBOIIns = this.BOI;
            var oSubstructure = this;
            string sInstanceID = oBOIIns.Attributes.Where(x => x.Key.ToLower() == "id".ToLower()).Select(x => x.Value.sValue).FirstOrDefault();

            Dictionary<string, object> Params = new Dictionary<string, object>();
            Params["Name"] = oSubstructure.sBO;
            var iDataSource = Connection.SelectString("iDataSource", "XIBO_T_N", Params);
            XIDXI oDXI = new XIDXI();
            //oDXI.sCoreDatabase = sCoreDataBase;
            //oDXI.sOrgDatabase = sOrgDataBase;
            //oDXI.iOrgID = iOrgId;
            string sMainBOName = oBOIIns.BOD.Name;
            if (int.TryParse(iDataSource, out iDataSourceID))
            {
                var DataSource = oDXI.GetBODataSource(iDataSourceID,oBOIIns.BOD.FKiApplicationID);
                sConnectionString = DataSource;
            }
            var oOneclickDef = oDXI.Get_1ClickDefinition(null, oSubstructure.i1ClickID.ToString());
            if (oOneclickDef.xiStatus == 0 && oOneclickDef.oResult != null)
            {
                XID1Click oXID1Click = (XID1Click)oOneclickDef.oResult;
                List<CNV> nParams = new List<CNV>();
                CNV oNVPairs = new CNV();
                oNVPairs.sName = "{XIP|" + sMainBOName + ".id}";
                oNVPairs.sValue = sInstanceID;
                nParams.Add(oNVPairs);
                Query = ReplaceExpressionWithCacheValue(oXID1Click.Query, nParams);
                XIDBAPI oConnection = new XIDBAPI(sConnectionString);
                XIIBO oBOII = new XIIBO();

                var oBOIns = (DataTable)oConnection.ExecuteQuery(Query);
                Dictionary<string, XIIBO> nBOIns = new Dictionary<string, XIIBO>();
                Dictionary<string, XIIAttribute> dictionary = new Dictionary<string, XIIAttribute>();
                var j = 0;
                foreach (DataRow row in oBOIns.Rows)
                {
                    XIIBO oBOI = new XIIBO();
                    dictionary = Enumerable.Range(0, oBOIns.Columns.Count)
                        .ToDictionary(i => oBOIns.Columns[i].ColumnName, i => new XIIAttribute { sValue = row.ItemArray[i].ToString() });
                    oBOI.Attributes = dictionary;
                    XIDBO oBOD = null;
                    if (oBOI.BOD == null)
                    {
                        Dictionary<string, object> BOParams = new Dictionary<string, object>();
                        BOParams["Name"] = oSubstructure.sBO;
                        oBOD = Connection.Select<XIDBO>("XIBO_T_N", BOParams).FirstOrDefault();
                        oBOI.BOD = oBOD;
                    }
                    nBOIns[j.ToString()] = oBOI;
                    j++;
                }

                oBOII.sSubStructName = sBO;
                oBOIIList.sSubStructName = sBO;
                oBOII.ODictionaryList = nBOIns;
                oBOIIList.oDictionaryBOI = nBOIns;
            }
            //sConnectionString = "Data Source=192.168.7.8;Initial Catalog=xishared;User ID=crqauser;Password=crqauser;MultipleActiveResultSets=True";
            return oBOIIList;
        }

        public XIStructure Structure(string sCode)
        {
            XIStructure oStructureD = new XIStructure();
            try
            {
                int iBODID = 0;
                var sBODID = (string)this.oParent;
                int.TryParse(sBODID, out iBODID);
                if (!string.IsNullOrEmpty(sCode) && iBODID > 0)
                {
                    List<XIDStructure> oTree = new List<XIDStructure>();
                    var oStrNodes = GetXITreeStructure(iBODID, sCode);
                    if (oStrNodes.bOK && oStrNodes.oResult != null)
                    {
                        oTree = (List<XIDStructure>)oStrNodes.oResult;
                        if (oTree != null && oTree.Count() > 0)
                        {
                            oStructureD.oNodes = oTree;
                            oStructureD.sName = oTree.FirstOrDefault().sName;
                            oStructureD.sCode = oTree.FirstOrDefault().sCode;
                        }
                    }
                    this.oParent = oStructureD;
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                SaveErrortoDB(oCResult);
            }
            return oStructureD;
        }

        public CResult GetXITreeStructure(int iBODID, string sCode)
        {
            CResult oCResult = new CResult();
            List<XIDStructure> oTree = new List<XIDStructure>();
            try
            {
                XIDStructure sMainNode = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["BOID"] = iBODID;
                Params["sCode"] = sCode;
                Params["FKiParentID"] = "#";
                sMainNode = Connection.Select<XIDStructure>("XIBOStructure_T", Params).FirstOrDefault();
                var sNodesAdded = new List<string>();
                //Check if the structure table for code
                List<XIDStructure> oXITree = new List<XIDStructure>();
                oXITree.Add(sMainNode);
                if (sMainNode != null)
                {
                    oTree = (List<XIDStructure>)XIStructTree(oXITree, sCode, oTree, sNodesAdded).oResult;
                }
                else
                {
                    //oXITree.Add(new XIDStructure { ID = sStructure.ID, FKiParentID = "#", sName = oBO.Name, BOID = iBOID });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oTree;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }

            return oCResult;
        }
        public CResult XIStructTree(List<XIDStructure> XIStrTrees, string sCode, List<XIDStructure> XITree, List<string> sNodesAdded)
        {
            CResult oCResult = new CResult();
            XIInfraCache oCache = new XIInfraCache();
            try
            {
                foreach (var items in XIStrTrees)
                {
                    if (!string.IsNullOrEmpty(items.sBO))
                    {
                        string sCheckNodeName = sNodesAdded.FirstOrDefault(stringToCheck => stringToCheck.Contains(items.sBO));
                        if (sCheckNodeName == null || (sCheckNodeName != null && items.FKiParentID != "#"))
                        {
                            XIDStructureDetail NodeDetails = null;
                            Dictionary<string, object> Params = new Dictionary<string, object>();
                            Params["FKiStructureID"] = items.ID;
                            NodeDetails = Connection.Select<XIDStructureDetail>("XIBOStructureDetail_T", Params).FirstOrDefault();
                            //var NodeDetails = dbContext.XIStructureDetails.Where(m => m.FKiStructureID == items.ID).FirstOrDefault();
                            if (NodeDetails != null)
                            {
                                items.i1ClickID = NodeDetails.i1ClickID;
                                if (items.i1ClickID > 0)
                                {
                                    XID1Click o1ClickD = null;
                                    o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, items.i1ClickID.ToString());
                                    items.OneClickD = o1ClickD;
                                }
                            }
                            XITree.Add(items);

                            //Aspect
                            List<XIDBOAspect> oAspect = new List<XIDBOAspect>();
                            Dictionary<string, object> AParams = new Dictionary<string, object>();
                            AParams["sCode"] = sCode;
                            AParams["FKiBOID"] = items.BOID;
                            oAspect = Connection.Select<XIDBOAspect>("XIBOAspect_T", AParams).ToList();
                            if(oAspect != null && oAspect.Count() > 0)
                            {
                                foreach(var asp in oAspect)
                                {
                                    XIDStructure oasp = new XIDStructure();
                                    oasp.ID = asp.ID;
                                    oasp.sName = asp.sName;
                                    oasp.FKi1ClickID = asp.FKi1ClickID;
                                    oasp.FKiParentID = items.ID.ToString();
                                    oasp.sMode = "Aspect";
                                    oasp.sSearchText = asp.sWhere;
                                    XITree.Add(oasp);
                                }
                            }
                            var ID = items.ID;
                            //var SubXITreeNodes = dbContext.XIStructure.Where(m => m.FKiParentID == ID.ToString() && m.StatusTypeID == 10 && m.sCode == sCode).ToList();

                            List<XIDStructure> SubXITreeNodes = null;
                            Dictionary<string, object> SubParams = new Dictionary<string, object>();
                            SubParams["StatusTypeID"] = 10.ToString();
                            SubParams["sCode"] = sCode;
                            SubParams["FKiParentID"] = ID.ToString();
                            SubXITreeNodes = Connection.Select<XIDStructure>("XIBOStructure_T", SubParams).ToList();

                            sNodesAdded.Add(items.sBO);
                            if (SubXITreeNodes.Count() > 0)
                            {
                                XIStructTree(SubXITreeNodes, sCode, XITree, sNodesAdded);
                            }
                            else if(items.bIsLoadParent)
                            {
                                //var XIStructure = dbContext.XIStructure.Where(m => m.FKiParentID == "#" && m.BOID == items.BOID && m.StatusTypeID == 10 && m.sCode == sCode).ToList();
                                List<XIDStructure> XIStructure = null;
                                Dictionary<string, object> SubNodeParams = new Dictionary<string, object>();
                                SubNodeParams["StatusTypeID"] = 10.ToString();
                                SubNodeParams["sCode"] = sCode;
                                SubNodeParams["BOID"] = items.BOID;
                                SubNodeParams["FKiParentID"] = "#";
                                XIStructure = Connection.Select<XIDStructure>("XIBOStructure_T", SubNodeParams).ToList();

                                if (XIStructure.Count() > 0)
                                {
                                    int iReplaceID = Convert.ToInt32(XIStructure.Where(m => m.sCode.ToLower() == sCode.ToLower()).Select(m => m.ID).FirstOrDefault());
                                    //Replace the ID
                                    XITree.Where(m => m.sName == items.sName).ToList().ForEach(s => s.ID = iReplaceID);
                                    XIStructTree(XIStructure, sCode, XITree, sNodesAdded);
                                }
                            }
                        }
                        else
                        {
                            var iID = items.ID;

                            //var XIStructure = dbContext.XIStructure.Where(m => m.FKiParentID == iID.ToString() && m.StatusTypeID == 10 && m.sCode == sCode).ToList();
                            List<XIDStructure> XIStructure = null;
                            Dictionary<string, object> SubNodeParams = new Dictionary<string, object>();
                            SubNodeParams["StatusTypeID"] = 10.ToString();
                            SubNodeParams["sCode"] = sCode;
                            SubNodeParams["FKiParentID"] = iID.ToString();
                            XIStructure = Connection.Select<XIDStructure>("XIBOStructure_T", SubNodeParams).ToList();

                            if (XIStructure.Count() > 0)
                            {
                                XIStructTree(XIStructure, sCode, XITree, sNodesAdded);
                            }
                        }
                    }
                    else
                    {
                        XITree.Add(items);
                    }
                }
                oCResult.oResult = XITree;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public string ReplaceExpressionWithCacheValue(string NewQuery, List<CNV> nParams)
        {
            //var sSessionID = HttpContext.Current.Session.SessionID;
            Regex regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(NewQuery);
            if (matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    var Prm = "{" + match.ToString() + "}";
                    var Matches = match.ToString();
                    var sExpr = match.ToString().Replace("{", "").Replace("}", "");
                    var Value = nParams.Where(m => m.sName.ToString().ToLower() == Prm.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(Value))
                    {
                        NewQuery = NewQuery.Replace("{" + match.ToString() + "}", Value);
                    }
                }
            }
            return NewQuery;
        }
        public CResult GetStructureBoInstance(List<XIIBO> oBOIParent, string sBOName, int iParentStructureID, List<XIDStructure> oStruture, string sLoadType, bool bIsXILoad = false)
        {
            CResult oCResult = new CResult();
            Dictionary<string, List<XIIBO>> oresult = new Dictionary<string, List<XIIBO>>(StringComparer.CurrentCultureIgnoreCase);
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                List<XIIBO> oTempList = new List<XIIBO>(); XIDXI oDXI = new XIDXI();
                Dictionary<string, XIIBO> oDictionaryBOI = null;
                Dictionary<string, List<XIIBO>> oMyDictClass = new Dictionary<string, List<XIIBO>>();
                CNV nvpairs = new CNV();
                List<CNV> nParams = new List<CNV>();
                //laod structure def
                Dictionary<string, object> NodeParams = new Dictionary<string, object>();
                List<XIDStructure> oXIDStructure = new List<XIDStructure>();
                if (oBOIParent != null)
                {
                    oXIDStructure = oStruture.Where(x => x.FKiParentID == iParentStructureID.ToString()).ToList();
                    if (oXIDStructure != null)
                    {
                        foreach (var instance in oBOIParent)
                        {
                            var sPrimaryKey = "";
                            if (instance.iBODID == 0)
                            {
                                XIDBO oBOD = (XIDBO)oDXI.Get_BODefinition(null, instance.iBODID.ToString()).oResult;
                                sPrimaryKey = oBOD.sPrimaryKey.ToLower();
                            }
                            else
                            {
                                sPrimaryKey = "id";
                            }
                            foreach (var structure in oXIDStructure)
                            {
                                nvpairs.sName = "{XIP|" + sBOName + ".id}";
                                nvpairs.sValue = instance.Attributes.Where(x => x.Key.ToLower() == sPrimaryKey).Select(x => x.Value.sValue).FirstOrDefault();
                                nParams.Add(nvpairs);
                                long iMainStructureid = structure.ID;
                                string sMainStructureid = Convert.ToString(iMainStructureid);
                                int imainBOID = structure.BOID;
                                string MainBoName = structure.sBO;
                                int iMainOneClickID = structure.i1ClickID;
                                //Dictionary<string, object> Param = new Dictionary<string, object>();
                                //Param["BOID"] = imainBOID;
                                //XIDBAPI oXIDBAPI = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                                //var iDataSource = oXIDBAPI.SelectString("iDataSource", "XIBO_T_N", Param);                                
                                string sParentFKColumn = structure.sParentFKColumn;
                                if (string.IsNullOrEmpty(sLoadType) || structure.sMode == "Single")
                                {
                                    var iDataSource = ((XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, imainBOID.ToString())).iDataSource;
                                    XID1Click oD1ClickD = new XID1Click();
                                    //list data loading
                                    oD1ClickD.ID = iMainOneClickID;
                                    oD1ClickD = structure.OneClickD; //(XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, iMainOneClickID.ToString());
                                    XID1Click oD1Click = new XID1Click();
                                    oD1Click = (XID1Click)oD1ClickD.Clone(oD1ClickD);
                                    oD1Click.Name = MainBoName;
                                    if (structure.OneClickD != null)
                                    {
                                        XID1Click oXID1Click = structure.OneClickD;
                                        if (structure.sLinkingType == "CtoP")
                                        {
                                            if (instance.Attributes.ContainsKey(structure.sParentFKColumn))
                                            {
                                                string iParentFKID = instance.Attributes[sParentFKColumn].sValue;
                                                nvpairs.sName = "{XIP|" + structure.sParentFKColumn + "}";
                                                nvpairs.sValue = iParentFKID;
                                                nParams.Add(nvpairs);
                                            }
                                        }
                                        oD1Click.Query = ReplaceExpressionWithCacheValue(oXID1Click.Query, nParams);
                                        var DataSource = oDXI.GetBODataSource(Convert.ToInt32(iDataSource),oXID1Click.FKiAppID);
                                        oD1Click.sConnectionString = DataSource;
                                        oD1Click.BOID = imainBOID;
                                        oD1Click.bIsResolveFK = true;
                                        oD1Click.bIsXILoad = bIsXILoad;
                                        oDictionaryBOI = oD1Click.OneClick_Run(false);
                                        //oDictionaryBOI = oD1Click.OneClick_Execute();
                                    }
                                }
                                if (oBOIParent != null)
                                {
                                    if (structure.sMode == "Single")
                                    {
                                        XIIBO obi = new XIIBO();
                                        if (oDictionaryBOI != null && oDictionaryBOI.Count() > 0)
                                        {
                                            int iCount = oDictionaryBOI.Count();
                                            obi = oDictionaryBOI[Convert.ToString(iCount - 1)];
                                            obi.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, MainBoName);
                                            if (MainBoName == "QS Instance")
                                            {
                                                string iInsatnceID = obi.Attributes.Where(x => x.Key.ToLower() == "id".ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                                                XIIQS oQsInstance = new XIIQS();
                                                obi.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, MainBoName);
                                                if (!string.IsNullOrEmpty(iInsatnceID))
                                                {
                                                    XIIXI oXII = new XIIXI();
                                                    XIIQS oXIQS = new XIIQS();
                                                    //var oQSD = oXII.GetQSInstanceByID(Convert.ToInt32(iInsatnceID));
                                                    //oQsInstance = oXII.GetQuestionSetInstanceByID(0, Convert.ToInt32(iInsatnceID), null, 0, 0, null);
                                                    oQsInstance = oXII.GetQSXIValuesByQSIID(Convert.ToInt32(iInsatnceID));
                                                    if (oQsInstance != null)
                                                    {
                                                        obi.XIIValues = oQsInstance.XIValues;
                                                        foreach (var item in obi.XIIValues)
                                                        {
                                                            obi.BOD.Attributes[item.Key.ToLower()] = new XIDAttribute { IsOptionList = item.Value.IsOptionList, OptionList = item.Value.FieldOptionList, iEnumBOD = item.Value.iEnumBOD };
                                                            obi.Attributes[item.Key.ToLower()] = new XIIAttribute { sName = item.Key.ToLower(), sValue = item.Value.sValue };
                                                        }
                                                    }
                                                    //var oQSNVPairs = GetQSNamevaluepairs(oQsInstance);
                                                    //if (oQSNVPairs.xiStatus == 0 && oQSNVPairs.oResult != null)
                                                    //{
                                                    //    Dictionary<string, XIIValue> oXIIList = (Dictionary<string, XIIValue>)oQSNVPairs.oResult;
                                                    //    obi.XIIValues = oXIIList;
                                                    //    foreach (var item in oXIIList)
                                                    //    {
                                                    //        obi.BOD.Attributes[item.Key] = new XIDAttribute { IsOptionList = item.Value.IsOptionList, OptionList = item.Value.FieldOptionList, iEnumBOD = item.Value.iEnumBOD };
                                                    //    }
                                                    //}
                                                }
                                            }
                                        }
                                        List<XIIBO> OBOIList = new List<XIIBO>();
                                        List<XIIBO> boList = new List<XIIBO>();
                                        Dictionary<string, List<XIIBO>> ParentDict = new Dictionary<string, List<XIIBO>>();
                                        boList.Add(instance);
                                        ParentDict[sBOName] = boList;
                                        obi.oParent = ParentDict;
                                        obi.iBODID = structure.BOID;
                                        OBOIList.Add(obi);
                                        if (!instance.SubChildI.ContainsKey(MainBoName))
                                        {
                                            instance.SubChildI.Add(MainBoName, OBOIList);
                                        }
                                        oTempList = OBOIList;
                                    }
                                    else
                                    {
                                        if (oDictionaryBOI != null && oDictionaryBOI.Values.Count() > 0)
                                        {
                                            oDictionaryBOI.Values.ToList().ForEach(m =>
                                            {
                                                List<XIIBO> boList = new List<XIIBO>();
                                                Dictionary<string, List<XIIBO>> ParentDict = new Dictionary<string, List<XIIBO>>();
                                                boList.Add(instance);
                                                ParentDict[sBOName] = boList;
                                                m.oParent = ParentDict;
                                                m.iBODID = structure.BOID;
                                            });
                                            if (!instance.SubChildI.ContainsKey(MainBoName))
                                                instance.SubChildI.Add(MainBoName, oDictionaryBOI.Values.Reverse().ToList());
                                            oTempList = oDictionaryBOI.Values.ToList();
                                        }
                                    }
                                }
                                //check for sub stru
                                var oSubChildBOList = oStruture.Where(x => x.FKiParentID == sMainStructureid).ToList();
                                if (oSubChildBOList.Count > 0)
                                {
                                    GetStructureBoInstance(oTempList, structure.sBO, Convert.ToInt32(iMainStructureid), oStruture, sLoadType, bIsXILoad);
                                }
                            }
                        }
                    }
                }
                if (!oresult.ContainsKey(sBOName))
                {
                    oresult.Add(sBOName, oBOIParent);
                }
                oCResult.oResult = oresult;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        public CResult GetQSNamevaluepairs(XIIQS oQsInstance)
        {
            CResult oResult = new CResult();
            try
            {
                // CNV oCNV = new CNV();
                // XIIValue oXIValues = new XIIValue();
                Dictionary<string, XIIValue> oXIIValuesList = new Dictionary<string, XIIValue>(StringComparer.CurrentCultureIgnoreCase);
                //Dictionary<string, CNV> oNVParirs = new Dictionary<string, CNV>();
                //List<CNV> cNVPairsList = new List<CNV>();
                //CNV cNVPairs = new CNV();
                if (oQsInstance != null)
                {
                    foreach (var StepDefinition in oQsInstance.QSDefinition.Steps)
                    {
                        foreach (var FieldDefinition in StepDefinition.Value.FieldDefs)
                        {
                            //cNVPairs = new CNV();
                            //cNVPairs.sName = FieldDefinition.Key;
                            //cNVPairs.sType =Convert.ToString(def1.Value.ID);
                            foreach (var StepInstance in oQsInstance.Steps)
                            {
                                if (StepInstance.Value.XIValues.Count() > 0)
                                {
                                    foreach (var FieldInstance in StepInstance.Value.XIValues)
                                    {
                                        if (FieldInstance.Value.FKiFieldDefinitionID == FieldDefinition.Value.ID)
                                        {
                                            //cNVPairs.sValue = FieldInstance.Value.sValue;
                                            //cNVPairsList.Add(cNVPairs);
                                            XIIValue XIValues = new XIIValue();
                                            if (FieldDefinition.Value.FieldOrigin.bIsOptionList || FieldDefinition.Value.FieldOrigin.FK1ClickID > 0)
                                            {
                                                XIValues.IsOptionList = FieldDefinition.Value.FieldOrigin.bIsOptionList;
                                                XIValues.FieldOptionList = FieldDefinition.Value.FieldOrigin.FieldOptionList.Select(m => new XIDOptionList
                                                {
                                                    ID = m.ID,
                                                    sOptionName = m.sOptionName,
                                                    sValues = m.sOptionValue
                                                }).ToList();
                                                XIValues.iEnumBOD = FieldDefinition.Value.FieldOrigin.FK1ClickID;
                                                //XIValues.sValue = FieldDefinition.Value.FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == FieldInstance.Value.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                            }
                                            //else
                                            //{
                                            XIValues.sValue = FieldInstance.Value.sValue;
                                            //}
                                            XIValues.FKiFieldDefinitionID = FieldInstance.Value.FKiFieldDefinitionID;
                                            oXIIValuesList.Add(FieldDefinition.Key, XIValues);
                                            // oCNV.NNVs.Add(def1.Key, oCnv);
                                        }
                                    }

                                }
                                else
                                {
                                    foreach (var SecInstance in StepInstance.Value.Sections)
                                    {
                                        foreach (var FieldInstance in SecInstance.Value.XIValues)
                                        {
                                            if (FieldInstance.Value.FKiFieldDefinitionID == FieldDefinition.Value.ID)
                                            {
                                                //cNVPairs.sValue = FieldInstance.Value.sValue;
                                                //cNVPairsList.Add(cNVPairs);
                                                XIIValue XIValues = new XIIValue();
                                                if (FieldDefinition.Value.FieldOrigin.bIsOptionList || FieldDefinition.Value.FieldOrigin.FK1ClickID > 0)
                                                {
                                                    XIValues.IsOptionList = FieldDefinition.Value.FieldOrigin.bIsOptionList;
                                                    XIValues.FieldOptionList = FieldDefinition.Value.FieldOrigin.FieldOptionList.Select(m => new XIDOptionList
                                                    {
                                                        ID = m.ID,
                                                        sOptionName = m.sOptionName,
                                                        sValues = m.sOptionValue
                                                    }).ToList();
                                                    XIValues.iEnumBOD = FieldDefinition.Value.FieldOrigin.FK1ClickID;
                                                    //XIValues.sValue = FieldDefinition.Value.FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == FieldInstance.Value.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                                }
                                                //else
                                                //{
                                                XIValues.sValue = FieldInstance.Value.sValue;
                                                //}
                                                XIValues.FKiFieldDefinitionID = FieldInstance.Value.FKiFieldDefinitionID;
                                                oXIIValuesList.Add(FieldDefinition.Key, XIValues);
                                                // oCNV.NNVs.Add(def1.Key, oCnv);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    oResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oResult.oResult = oXIIValuesList;
                }
            }
            catch (Exception ex)
            {
                oResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oResult.LogToFile();
            }
            return oResult;
        }

        public List<XIDStructure> GetXIStructureTreeDetails(int BOID, string sCode, long iID = 0)
        {

            List<XIDStructure> XIStrTrees = new List<XIDStructure>();
            List<XIDStructure> Tree = new List<XIDStructure>();
            //XIStrTrees = dbContext.XIStructure.Where(m => m.FKiParentID == iMainID.ToString()).ToList();

            //XID1Click o1Click = null;
            Dictionary<string, object> Params = new Dictionary<string, object>();
            Params["FKiParentID"] = "#";
            if (!string.IsNullOrEmpty(sCode))
            {
                Params["sCode"] = sCode;
            }
            else if (iID > 0)
            {
                Params["ID"] = iID;
            }
            Params["BOID"] = BOID;

            XIStrTrees = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
            //XIStrTrees = dbContext.XIStructure.Where(m => m.FKiParentID == "#" && m.sCode.ToLower() == sCode.ToLower() && m.BOID == BOID).ToList();

            //XIStrTrees.Add(XIParntTrees);
            if (XIStrTrees != null && XIStrTrees.Count() > 0)
            {
                Tree = XITree(XIStrTrees, new List<XIDStructure>());
            }
            else
            {
                Tree = new List<XIDStructure>();
            }
            // XIStrTrees.Add(XIParntTrees);
            return Tree;
        }

        public List<XIDStructure> XITree(List<XIDStructure> XIStrTrees, List<XIDStructure> Tree)
        {
            //ModelDbContext dbContext = new ModelDbContext();
            //List<cXIStructure> Strut = new List<cXIStructure>();
            foreach (var items in XIStrTrees)
            {
                Tree.Add(items);
                var ID = items.ID;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["FKiParentID"] = ID.ToString();
                Params["StatusTypeID"] = 10.ToString();
                var SubXITreeNodes = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
                XIDStructureDetail NodeDetails = null;
                Dictionary<string, object> oParams = new Dictionary<string, object>();
                oParams["FKiStructureID"] = items.ID;
                NodeDetails = Connection.Select<XIDStructureDetail>("XIBOStructureDetail_T", oParams).FirstOrDefault();


                //var NodeDetails = dbContext.XIStructureDetails.Where(m => m.FKiStructureID == items.ID).FirstOrDefault();
                if (NodeDetails != null)
                {
                    items.i1ClickID = NodeDetails.i1ClickID;
                }
                //var SubXITreeNodes = dbContext.XIStructure.Where(m => m.FKiParentID == ID.ToString() && m.StatusTypeID == 10).OrderBy(m => m.iOrder).ToList();
                if (SubXITreeNodes.Count() > 0)
                {
                    XITree(SubXITreeNodes, Tree);
                }
            }
            return Tree;
        }

        private void AddCacheParameters(List<CNV> copyparams, List<XIIBO> sourcexi)
        {
            CResult oCResult = new CResult();
            try
            {
                if (sourcexi != null && copyparams != null && copyparams.Count() > 0)
                {
                    var distinctparam = copyparams.Select(t => t.sType).Distinct().ToList();
                    if (sourcexi.Any(d => distinctparam.Contains(d.BOD.Name)))
                    {
                        foreach (var singleBo in sourcexi)
                        {
                            string sBoName = singleBo.BOD.Name;
                            XICopyParams xinbo = new XICopyParams();
                            xinbo.sBOName = sBoName;
                            //search in copylist with BoName
                            var filteredwithbo = copyparams.Where(d => d.sType.ToLower() == sBoName.ToLower()).ToList();
                            for (int i = 0; i < filteredwithbo.Count(); i++)
                            {
                                //Add into BO
                                xinbo.Attributes.Add(filteredwithbo[i].sName, new XIIAttribute { sValue = singleBo.AttributeI(filteredwithbo[i].sName).sValue });
                            }
                            if (xinbo.Attributes.Count() > 0)
                            {
                                xiCopyParams.Add(xinbo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Adding Cache paramters in AddCacheParameters" });
                oCResult.sMessage = "ERROR : [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
        }

        public Dictionary<string, long> oNewPK = new Dictionary<string, long>();
        public CResult StructureCopy(string sBOName, string sUID, string sStructureCode, List<CNV> CopyParams, bool bIsCopyMainBo = false)
        {
            CResult oCResult = new CResult();
            XIInfraCache oCache = new XIInfraCache();
            try
            {
                oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
                XIIXI oIXI = new XIIXI(); XIDStructure oXIDStructure = new XIDStructure(); int iInstanceID = 0;
                var oBOIList = oIXI.BOI(sBOName, sUID, null, null, true);//Load BOI with sUID
                if (oBOIList.Attributes.Count() > 0)
                {
                    int sPrimaryKeyValue = Convert.ToInt32(oBOIList.Attributes.Where(n => n.Key.ToLower().Equals(oBOIList.BOD.sPrimaryKey.ToLower())).Select(s => s.Value).FirstOrDefault().sValue);
                    if (sPrimaryKeyValue != 0)
                    {
                        oBOIList.Attributes.Where(n => n.Key.ToLower().Equals(oBOIList.BOD.sPrimaryKey.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });//set primary column to null
                        oBOIList.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
                    }
                    if (CopyParams != null && CopyParams.Count() > 0)
                    {
                        var oBOParams = CopyParams.Where(m => m.sType.ToLower() == oBOIList.BOD.Name.ToLower()).ToList();
                        if (oBOParams != null && oBOParams.Count() > 0)
                        {
                            foreach (var items in oBOParams)
                            {
                                if (oBOIList.Attributes.Values.Where(m => m.sName.ToLower() == items.sName.ToLower()).FirstOrDefault() != null && items.sContext == "Save")
                                {
                                    oBOIList.Attributes.Values.Where(m => m.sName.ToLower() == items.sName.ToLower()).FirstOrDefault().sValue = items.sValue;
                                }
                                //else
                                //{
                                //    XIIAttribute oAttrI = new XIIAttribute();
                                //    oAttrI.sName = items.sName;
                                //    oAttrI.sValue = items.sValue;
                                //    oBOIList.Attributes[items.sName.ToLower()] = oAttrI;
                                //}
                            }
                        }
                    }
                    if (bIsCopyMainBo)
                    {
                        var Response = oBOIList.Save(oBOIList,false);//Copy the BOI 
                        XIIBO oCopyBOI = new XIIBO();
                        if (Response.bOK && Response.oResult != null)
                        {
                            oCopyBOI = oBOIList;
                        }
                    }
                    // Add Required Cache Parameters
                    if (CopyParams != null && CopyParams.Count() > 0 && CopyParams.Any(d => d.sType == oBOIList.BOD.Name))
                    {
                        AddCacheParameters(CopyParams, new List<XIIBO> { oBOIList });
                    }

                    iInstanceID = Convert.ToInt32(sUID);
                    var oStructD = (List<XIDStructure>)oCache.GetObjectFromCache(XIConstant.CacheStructure, sStructureCode, oBOIList.BOD.BOID.ToString());
                    //var oNodes = oXIDStructure.GetXITreeStructure(oBOIList.BOD.BOID, sStructureCode);//get the structure with sStructureCode
                    if (oStructD != null && oStructD.Count() > 0)
                    {
                        var oMainstructureList = oStructD.Where(m => m.BOID == oBOIList.BOD.BOID).FirstOrDefault();
                        var oStructure = oStructD.Where(x => x.BOID != oBOIList.BOD.BOID).ToList();
                        List<XIIBO> oBIList = new List<XIIBO>();
                        oBIList.Add(oBOIList);
                        List<XIIBO> oUpdatedBOIList = new List<XIIBO>();
                        oUpdatedBOIList.Add(oBOIList);
                        var oResult = SaveBOICopy(CopyParams, iInstanceID, Convert.ToInt32(oMainstructureList.ID), oMainstructureList.sBO, oStructure, oBIList, sUID, oUpdatedBOIList, "", 1);//Load Substructure BOI 
                        if (oResult.bOK && oResult.oResult != null)
                        {
                            //int iID = (int)oResult.oResult;
                            oCResult.oResult = iInstanceID;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line1 = frame.GetFileLineNumber();
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while copying structure instance at line number: " + line + "_" + line1 });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        //public object SaveCopyObject(XIIBO oBOI)
        //{
        //    XIIXI oIXI = new XIIXI();int iRefID = 0;
        //    if (oBOI.Attributes.Count() > 0)
        //    {
        //        int sPrimaryKeyValue = Convert.ToInt32(oBOI.Attributes.Where(n => n.Key.ToLower().Equals("id")).Select(s => s.Value).FirstOrDefault().sValue);
        //        if (sPrimaryKeyValue != 0)
        //        {
        //            oBOI.Attributes.Where(n => n.Key.ToLower().Equals("id")).Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
        //            oBOI.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
        //        }
        //        var Response = oBOI.Save(oBOI);
        //         iRefID = Convert.ToInt32(Response.Attributes.Where(s => s.Key.ToLower().Equals("id")).Select(s => s.Value).FirstOrDefault().sValue);
        //    }
        //    return iRefID;
        //}
        public CResult SaveBOICopy(List<CNV> CopyParams, int iInstanceID, int iParentStructureID, string sBOName, List<XIDStructure> oStructure, List<XIIBO> oBOIInstance, string sUID, List<XIIBO> sUpdatedBOIInstance, string sParentFKColumn, int i = 0)
        {
            CResult oCResult = new CResult();
            XIInfraCache oCache = new XIInfraCache();            
            try
            {
                oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
                List<XIDStructure> oXIDStructure = new List<XIDStructure>(); List<XIIBO> oBOIList = new List<XIIBO>(); int iInsertedID = 0; List<XIIBO> oBOIListC = new List<XIIBO>();
                List<XIIBO> oUpdatedBOI = new List<XIIBO>();
                oUpdatedBOI = sUpdatedBOIInstance;
                CNV nvpairs = new CNV(); List<CNV> nParams = new List<CNV>();
                Dictionary<string, XIIBO> nDictionaryBOI = new Dictionary<string, XIIBO>();
                if (oBOIInstance != null)
                {
                    oXIDStructure = oStructure.Where(x => x.FKiParentID == iParentStructureID.ToString()).ToList();
                    if (oXIDStructure != null)
                    {
                        if (oXIDStructure.Count() > 0)
                        {
                            foreach (var instance in oBOIInstance)
                            {
                                bool IsHistory = true;
                                if (sBOName.ToLower() == "xiqsstepinstance_t" || sBOName.ToLower() == "xifieldinstance_t")
                                {
                                    var QsStepDefID = instance.Attributes["fkiqsstepdefinitionid"].sValue;
                                    int iQsStepDefID = 0;
                                    if (int.TryParse(QsStepDefID, out iQsStepDefID))
                                    {
                                        XIIXI oIXI = new XIIXI();
                                        var oStepI = oIXI.BOI("XI QS Step Definition", iQsStepDefID.ToString());
                                        var bIsHistory = oStepI.Attributes["bIsCopy"].sValue;
                                        if (bIsHistory.ToLower() == "false")
                                        {
                                            IsHistory = false;
                                        }
                                    }
                                }
                                if (IsHistory)
                                {
                                    nvpairs.sName = "{XIP|" + sBOName + ".id}";

                                    if (!string.IsNullOrEmpty(sUID))
                                    {
                                        nvpairs.sValue = sUID;
                                    }
                                    else
                                    {
                                        nvpairs.sValue = instance.Attributes.Where(x => x.Key.ToLower() == instance.BOD.sPrimaryKey.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
                                    }
                                    //if (nvpairs.sName.Trim() == "{XIP|QS Instance.id}".Trim())
                                    //{
                                    //    if (!string.IsNullOrEmpty(nvpairs.sValue))
                                    //        int.TryParse(nvpairs.sValue, out iMyParentQSIID);
                                    //}

                                    nParams.Add(nvpairs);
                                    if (i != 1)
                                    {
                                        if (instance.Attributes.Count() > 0)
                                        {
                                            foreach (var item in instance.Attributes)
                                            {
                                                if (instance.BOD.Attributes.ContainsKey(item.Key))
                                                {
                                                    var value = sUpdatedBOIInstance.Where(m => m.BOD.Name == instance.BOD.Attributes[item.Key].sFKBOName).Select(m => m.Attributes[m.BOD.sPrimaryKey.ToLower()].sValue).FirstOrDefault();
                                                    if (!String.IsNullOrEmpty(value))
                                                    {
                                                        instance.Attributes.Where(n => n.Key.ToLower().Equals(item.Value.sName.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = value; });
                                                    }
                                                }
                                            }
                                            string sInstanceID = Convert.ToString(iInstanceID);
                                            if (!string.IsNullOrEmpty(sParentFKColumn))
                                            {
                                                if (instance.Attributes.Any(x => x.Key.ToLower().Contains(sParentFKColumn.ToLower())))
                                                {
                                                    instance.Attributes.Where(n => n.Key.ToLower().Equals(sParentFKColumn.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = sInstanceID; });
                                                }
                                            }
                                            int sPrimaryKeyValue = Convert.ToInt32(instance.Attributes.Where(n => n.Key.ToLower().Equals(instance.BOD.sPrimaryKey.ToLower())).Select(s => s.Value).FirstOrDefault().sValue);
                                            if (sPrimaryKeyValue != 0)
                                            {
                                                instance.Attributes.Where(n => n.Key.ToLower().Equals(instance.BOD.sPrimaryKey.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                instance.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
                                            }
                                            updateDefaultData(instance);
                                            instance.bIsCreatedBy = false;
                                            var Response = instance.Save(instance, false);
                                            XIIBO oBOI = new XIIBO();
                                            if (Response.bOK && Response.oResult != null)
                                            {
                                                oBOI = (XIIBO)Response.oResult;
                                            }

                                            //Add Cache paramters
                                            if (CopyParams.Any(d => d.sType == oBOI.BOD.Name))
                                            {
                                                AddCacheParameters(CopyParams, new List<XIIBO> { oBOI });
                                            }
                                            iInsertedID = Convert.ToInt32(oBOI.Attributes.Where(s => s.Key.ToLower().Equals(instance.BOD.sPrimaryKey.ToLower())).Select(s => s.Value).FirstOrDefault().sValue);
                                            oUpdatedBOI.Add(oBOI);
                                            if (iInsertedID > 0)
                                            {
                                                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Copy of " + instance.BOD.Name + "substructure instance completed successfully" });
                                            }
                                            else
                                            {
                                                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while copying " + instance.BOD.Name + " substructure instance" });
                                            }
                                            //iResponseID = (int)SaveCopyObject(instance);
                                        }
                                    }
                                    foreach (var structure in oXIDStructure)
                                    {
                                        bool bIsUpdateParent = false;
                                        XIDXI oDXI = new XIDXI();
                                        //Dictionary<string, object> nParam = new Dictionary<string, object>();
                                        //nParam["BOID"] = structure.BOID;
                                        //var iDataSource = Connection.SelectString("iDataSource", "XIBO_T_N", nParam);
                                        var iDataSource = ((XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, structure.BOID.ToString())).iDataSource;
                                        XID1Click oD1Click = new XID1Click();
                                        //loading structure BOI
                                        oD1Click.ID = structure.i1ClickID;
                                        oD1Click.Name = structure.sBO;                                        
                                        if (structure.OneClickD != null)
                                        {
                                            XID1Click oXID1Click = structure.OneClickD;
                                            if (structure.sLinkingType == "CtoP")
                                            {
                                                if (!string.IsNullOrEmpty(structure.sParentFKColumn)&&instance.Attributes.ContainsKey(structure.sParentFKColumn))
                                                {
                                                    string iParentFKID = instance.Attributes[structure.sParentFKColumn].sValue;
                                                    nvpairs.sName = "{XIP|" + structure.sParentFKColumn + "}";
                                                    nvpairs.sValue = iParentFKID;
                                                    nParams.Add(nvpairs);
                                                    if (structure.sMode == "Single")
                                                    {
                                                        bIsUpdateParent = true;
                                                    }
                                                }                                               
                                            }
                                            oD1Click.Query = ReplaceExpressionWithCacheValue(oXID1Click.Query, nParams);
                                            oD1Click.Query = oD1Click.ReplaceSelectWithStar(oD1Click.Query);
                                            var DataSource = oDXI.GetBODataSource(Convert.ToInt32(iDataSource),oD1Click.FKiAppID);
                                            oD1Click.sConnectionString = DataSource;
                                            nDictionaryBOI = oD1Click.OneClick_Execute();//Execute structure OneClick
                                            oBOIList = new List<XIIBO>();
                                            oBOIListC = new List<XIIBO>();
                                            if (structure.sMode == "Single")
                                            {
                                                XIIBO oBOI = new XIIBO();
                                                if (nDictionaryBOI.Count() > 0)
                                                {
                                                    int iCount = nDictionaryBOI.Count();
                                                    oBOI = nDictionaryBOI[Convert.ToString(iCount - 1)];
                                                    oBOI.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, structure.sBO, null);

                                                    if (CopyParams.Count() > 0)
                                                    {
                                                        var oBOParams = CopyParams.Where(m => m.sType.ToLower() == oBOI.BOD.Name.ToLower()).ToList();
                                                        if (oBOParams != null && oBOParams.Count() > 0)
                                                        {
                                                            foreach (var items in oBOParams)
                                                            {
                                                                if (items.sContext == "Save" && oBOI.Attributes.Values.Where(m => m.sName.ToLower() == items.sName.ToLower()).FirstOrDefault() != null)
                                                                {
                                                                    oBOI.Attributes.Values.Where(m => m.sName.ToLower() == items.sName.ToLower()).FirstOrDefault().sValue = items.sValue;
                                                                }
                                                            }
                                                        }
                                                    }                                                    
                                                    updateDefaultData(oBOI);
                                                    oBOIList.Add(oBOI);
                                                }
                                            }
                                            else
                                            {
                                                if (nDictionaryBOI != null && nDictionaryBOI.Count() > 0 && nDictionaryBOI.Values.Count() > 0)
                                                {
                                                    var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, structure.sBO, null);
                                                    oBOIList = nDictionaryBOI.Values.Reverse().ToList();
                                                    oBOIListC = nDictionaryBOI.Values.Reverse().ToList();
                                                    if (CopyParams.Count() > 0)
                                                    {
                                                        var oBOParams = CopyParams.Where(m => m.sType.ToLower() == oBOD.Name.ToLower()).ToList();
                                                        if (oBOParams != null && oBOParams.Count() > 0)
                                                        {
                                                            foreach (var items in oBOParams)
                                                            {
                                                                if (items.sContext == "Save")
                                                                {
                                                                    foreach (var boi in oBOIList)
                                                                    {
                                                                        if (boi.Attributes.Values.Where(m => m.sName.ToLower() == items.sName.ToLower()).FirstOrDefault() != null)
                                                                        {
                                                                            boi.Attributes.Values.Where(m => m.sName.ToLower() == items.sName.ToLower()).FirstOrDefault().sValue = items.sValue;
                                                                        }
                                                                        if (structure.sBO.ToLower() == "xiqsstepinstance_t" || structure.sBO.ToLower() == "xifieldinstance_t")
                                                                        {
                                                                            var QsStepDefID = boi.Attributes["fkiqsstepdefinitionid"].sValue;
                                                                            int iQsStepDefID = 0;
                                                                            if (int.TryParse(QsStepDefID, out iQsStepDefID))
                                                                            {
                                                                                XIIXI oIXI = new XIIXI();
                                                                                var oStepI = oIXI.BOI("XI QS Step Definition", iQsStepDefID.ToString());
                                                                                var bIsHistory = oStepI.Attributes["bIsCopy"].sValue;
                                                                                if (bIsHistory.ToLower() == "false")
                                                                                {
                                                                                    oBOIListC.Remove(oBOIList.ToList().Where(m => m.Attributes["id"].sValue == boi.Attributes["id"].sValue).FirstOrDefault());
                                                                                }
                                                                            }
                                                                            var bIsModify = boi.Attributes["bismodify"].sValue;
                                                                            if (bIsModify.ToLower() == "true")
                                                                            {
                                                                                var sFieldID = boi.Attributes["fkifieldoriginid"].sValue;
                                                                                int iFieldOriginID = 0;
                                                                                if (int.TryParse(sFieldID, out iFieldOriginID))
                                                                                {
                                                                                    XIIXI oIXI = new XIIXI();
                                                                                    var oFieldD = oIXI.BOI("XIFieldOrigin_T", iFieldOriginID.ToString());
                                                                                    var sFieldName = oFieldD.Attributes["sname"].sValue;
                                                                                    if (sFieldName.ToLower() == items.sName.ToLower())
                                                                                    {
                                                                                        boi.Attributes["svalue"].sValue = items.sValue;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        updateDefaultData(boi);
                                                                    }
                                                                    oBOIList = oBOIListC;
                                                                }
                                                            }
                                                        }
                                                        else if (structure.sBO.ToLower() == "xiqsstepinstance_t" || structure.sBO.ToLower() == "xifieldinstance_t")
                                                        {
                                                            foreach (var boi in oBOIList)
                                                            {
                                                                var QsStepDefID = boi.Attributes["fkiqsstepdefinitionid"].sValue;
                                                                int iQsStepDefID = 0;
                                                                if (int.TryParse(QsStepDefID, out iQsStepDefID))
                                                                {
                                                                    XIIXI oIXI = new XIIXI();
                                                                    var oStepI = oIXI.BOI("XI QS Step Definition", iQsStepDefID.ToString());
                                                                    var bIsHistory = oStepI.Attributes["bIsCopy"].sValue;
                                                                    if (bIsHistory.ToLower() == "false")
                                                                    {
                                                                        oBOIListC.Remove(oBOIList.ToList().Where(m => m.Attributes["id"].sValue == boi.Attributes["id"].sValue).FirstOrDefault());
                                                                    }
                                                                }
                                                                updateDefaultData(boi);
                                                            }
                                                            oBOIList = oBOIListC;
                                                        }
                                                    }


                                                    oBOIList.ForEach(m => m.BOD = oBOD);
                                                }
                                            }
                                        }
                                        long iMainStructureid = structure.ID;
                                        string sMainStructureid = Convert.ToString(iMainStructureid);
                                        var oSubChildBOList = oStructure.Where(x => x.FKiParentID == sMainStructureid).ToList();
                                        if (oSubChildBOList.Count > 0)
                                        {
                                            if (iInsertedID == 0)
                                            {
                                                iInsertedID = iInstanceID;
                                            }
                                            SaveBOICopy(CopyParams, iInsertedID, Convert.ToInt32(iMainStructureid), structure.sBO, oStructure, oBOIList, "", oUpdatedBOI, structure.sParentFKColumn, 2);
                                        }
                                        else
                                        {
                                            int occuranceCount = oBOIList.Select(d => d.iBODID).Distinct().Count();
                                            int Totalcount = oBOIList.Select(d => d.iBODID).Count();
                                            var sType = CopyParams.Where(x => x.sName == "stype").Select(t => t.sValue).FirstOrDefault();
                                            if (occuranceCount == 1 && Totalcount > 1)
                                            {
                                                // Then do Bulk Insertion
                                                foreach (var oInstance in oBOIList) //start for oBOIList
                                                {
                                                    if (oInstance.Attributes.Count() > 0)
                                                    {
                                                        foreach (var item in oInstance.Attributes)
                                                        {
                                                            if (oInstance.BOD.Attributes.ContainsKey(item.Key))
                                                            {
                                                                var value = sUpdatedBOIInstance.Where(m => m.BOD.Name == oInstance.BOD.Attributes[item.Key].sFKBOName).Select(m => m.Attributes[m.BOD.sPrimaryKey.ToLower()].sValue).FirstOrDefault();
                                                                if (!String.IsNullOrEmpty(value))
                                                                {
                                                                    oInstance.Attributes.Where(n => n.Key.ToLower().Equals(item.Value.sName.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = value; });
                                                                }
                                                            }
                                                        }
                                                        string sInstanceID = Convert.ToString(iInsertedID);
                                                        string sSubParentFKColumn = structure.sParentFKColumn;
                                                        if (!string.IsNullOrEmpty(sSubParentFKColumn))
                                                        {
                                                            if (oInstance.Attributes.Any(x => x.Key.ToLower().Contains(sSubParentFKColumn.ToLower())))
                                                            {
                                                                oInstance.Attributes.Where(n => n.Key.ToLower().Equals(sSubParentFKColumn.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = sInstanceID; });
                                                            }
                                                        }
                                                        int sPrimaryKeyValue = Convert.ToInt32(oInstance.Attributes.Where(n => n.Key.ToLower().Equals(oInstance.BOD.sPrimaryKey.ToLower())).Select(s => s.Value).FirstOrDefault().sValue);
                                                        if (sPrimaryKeyValue != 0)
                                                        {                                                           
                                                            oInstance.Attributes.Where(n => n.Key.ToLower().Equals(oInstance.BOD.sPrimaryKey.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                            oInstance.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
                                                            if (sType == "copyqs")
                                                            {
                                                                oInstance.Attributes.Where(n => n.Key.ToLower() == "fkiacpolicyid").Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                                oInstance.Attributes.Where(n => n.Key.ToLower() == "fkipolicyversionid").Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                            }
                                                        }
                                                        updateDefaultData(oInstance);
                                                    }
                                                } //end for oBOIList

                                                // DO BULK INSERTION HERE
                                                XIIBO xibulk = new XIIBO();
                                                DataTable dtbulk = xibulk.MakeBulkSqlTable(oBOIList);
                                                xibulk.SaveBulk(dtbulk, oBOIList[0].BOD.iDataSource, oBOIList[0].BOD.TableName);

                                                //Add Cache paramters
                                                if (CopyParams.Any(d => d.sType == oBOIList[0].BOD.TableName))
                                                {
                                                    AddCacheParameters(CopyParams, oBOIList);
                                                }
                                            }
                                            else
                                            {
                                                foreach (var oInstance in oBOIList) //start for oBOIList
                                                {
                                                    if (oInstance.Attributes.Count() > 0)
                                                    {
                                                        foreach (var item in oInstance.Attributes)
                                                        {
                                                            if (oInstance.BOD.Attributes.ContainsKey(item.Key))
                                                            {
                                                                var value = sUpdatedBOIInstance.Where(m => m.BOD.Name == oInstance.BOD.Attributes[item.Key].sFKBOName).Select(m => m.Attributes[m.BOD.sPrimaryKey.ToLower()].sValue).FirstOrDefault();
                                                                if (!String.IsNullOrEmpty(value))
                                                                {
                                                                    oInstance.Attributes.Where(n => n.Key.ToLower().Equals(item.Value.sName.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = value; });
                                                                }
                                                            }
                                                        }
                                                        string sInstanceID = Convert.ToString(iInsertedID);
                                                        string sSubParentFKColumn = structure.sParentFKColumn;
                                                        if (!string.IsNullOrEmpty(sSubParentFKColumn))
                                                        {
                                                            if (oInstance.Attributes.Any(x => x.Key.ToLower().Contains(sSubParentFKColumn.ToLower())))
                                                            {
                                                                oInstance.Attributes.Where(n => n.Key.ToLower().Equals(sSubParentFKColumn.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = sInstanceID; });
                                                            }
                                                        }
                                                        int sPrimaryKeyValue = Convert.ToInt32(oInstance.Attributes.Where(n => n.Key.ToLower().Equals(oInstance.BOD.sPrimaryKey.ToLower())).Select(s => s.Value).FirstOrDefault().sValue);
                                                        if (sPrimaryKeyValue != 0)
                                                        {
                                                            oInstance.Attributes.Where(n => n.Key.ToLower().Equals(oInstance.BOD.sPrimaryKey.ToLower())).Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                            oInstance.Attributes.ToList().ForEach(m => m.Value.bDirty = true);
                                                            if (sType == "copyqs")
                                                            {
                                                                oInstance.Attributes.Where(n => n.Key.ToLower() == "fkiacpolicyid").Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                                oInstance.Attributes.Where(n => n.Key.ToLower() == "fkipolicyversionid").Select(m => m.Value).ToList().ForEach(m => { m.sValue = null; });
                                                            }
                                                        }
                                                        updateDefaultData(oInstance);
                                                        instance.bIsCreatedBy = false;
                                                        var Response = oInstance.Save(oInstance, false);
                                                        XIIBO oBOI = new XIIBO();
                                                        if (Response.bOK && Response.oResult != null)
                                                        {
                                                            oBOI = (XIIBO)Response.oResult;
                                                        }

                                                        //Add Cache paramters
                                                        if (CopyParams.Any(t => t.sType == oBOI.BOD.Name))
                                                        {
                                                            AddCacheParameters(CopyParams, new List<XIIBO> { oBOI });
                                                        }
                                                        int iInsertedId = Convert.ToInt32(oBOI.Attributes.Where(s => s.Key.ToLower().Equals(oInstance.BOD.sPrimaryKey.ToLower())).Select(s => s.Value).FirstOrDefault().sValue);
                                                        if (iInsertedId > 0)
                                                        {
                                                            oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Copy of substructure instance completed successfully" });
                                                        }
                                                        else
                                                        {
                                                            oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while copying substructure instance" });
                                                        }
                                                    }
                                                } //end for oBOIList
                                            }
                                        }
                                        if(bIsUpdateParent)
                                        {
                                            if(!string.IsNullOrEmpty(structure.sParentFKColumn)&&instance.Attributes.ContainsKey(structure.sParentFKColumn.ToLower()) && oBOIList.Count>0)
                                            {
                                                instance.Attributes[structure.sParentFKColumn.ToLower()].sValue = oBOIList.FirstOrDefault().Attributes[oBOIList.FirstOrDefault().BOD.sPrimaryKey].sValue;
                                                instance.Attributes[structure.sParentFKColumn.ToLower()].bDirty = true;
                                                instance.Attributes[instance.BOD.sPrimaryKey].bDirty = true;
                                                instance.Save(instance);
                                            }
                                        }
                                    } // end for oXIDStructure
                                }
                            }
                        }
                    }
                }
                oCResult.oResult = sUID;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line1 = frame.GetFileLineNumber();
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while copying structure instance at line number: " + line + "_" + line1 + " and BOName is " + sBOName + " and iParentStructureID is " + iParentStructureID });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        public void updateDefaultData(XIIBO oBOI)
        {
            XIInfraUsers oUser = new XIInfraUsers();
            CUserInfo oInfo = new CUserInfo();
            oInfo = oUser.Get_UserInfo();
            //oInfo = oInfo.GetUserInfo();
            string sUserName = oInfo.sName == null ? null : (oInfo.sName.Length >= 15 ? oInfo.sName.Substring(0, 14) : oInfo.sName);
            if (oBOI.Attributes.ContainsKey(XIConstant.Key_XICrtdWhn.ToLower()))
            {
                oBOI.Attributes.Values.Where(m => m.sName.ToLower() == XIConstant.Key_XICrtdWhn.ToLower()).FirstOrDefault().sValue = DateTime.Now.ToString();
            }
            if (oBOI.Attributes.ContainsKey(XIConstant.Key_XIUpdtdWhn.ToLower()))
            {
                oBOI.Attributes.Values.Where(m => m.sName.ToLower() == XIConstant.Key_XIUpdtdWhn.ToLower()).FirstOrDefault().sValue = DateTime.Now.ToString();
            }
            if (oBOI.Attributes.ContainsKey(XIConstant.Key_XICrtdBy.ToLower()))
            {
                oBOI.Attributes.Values.Where(m => m.sName.ToLower() == XIConstant.Key_XICrtdBy.ToLower()).FirstOrDefault().sValue = sUserName;
            }
            if (oBOI.Attributes.ContainsKey(XIConstant.Key_XIUpdtdBy.ToLower()))
            {
                oBOI.Attributes.Values.Where(m => m.sName.ToLower() == XIConstant.Key_XIUpdtdBy.ToLower()).FirstOrDefault().sValue = sUserName;
            }
        }
        #region Inline Tree Definition

        public CResult Get_SelfStructure(string sFolderName, string sUID = "", int iBODID = 0, string sSearchText = "", string BuildingID = "", string sLoadType = "", string sFilterType = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }

                if ((!string.IsNullOrEmpty(sFolderName) || !string.IsNullOrEmpty(sUID)) && iBODID > 0)
                {
                    var Data = Get_SelfStructureRecurssive(sFolderName, sUID, iBODID, sSearchText, BuildingID, sLoadType, sFilterType);
                    if (Data != null && Data.Values.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(sSearchText))
                        {
                            oCResult.oResult = FilteredData;
                        }
                        else
                        {
                            oCResult.oResult = Data;
                        }
                        //Data.Values.FirstOrDefault().FilteredData = FilteredData;
                    }
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading self structure" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }
        Dictionary<string, XIIBO> FilteredData = new Dictionary<string, XIIBO>();
        List<CNV> FilteredFiles = new List<CNV>();
        public int FolderCount = 0;
        public Dictionary<string, XIIBO> Get_SelfStructureRecurssive_Old(string sFolderName, string sParentID, int iBODID, string sSearchText, string BuildingID, string sLoadType)
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
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID.ToString());
                XIIXI oXI = new XIIXI();
                XIIBO oBOI = new XIIBO();
                List<CNV> oParams = new List<CNV>();
                if (!string.IsNullOrEmpty(sFolderName))
                {
                    oParams.Add(new CNV { sName = "sname", sValue = sFolderName });
                    oParams.Add(new CNV { sName = "iBuildingID", sValue = BuildingID });
                    oBOI = oXI.BOI(oBOD.Name, null, null, oParams);
                    if (oBOI != null && oBOI.Attributes.Values.Count() > 0)
                    {
                        var sPID = oBOI.AttributeI(oBOD.sPrimaryKey).sValue;
                        if (!string.IsNullOrEmpty(sPID))
                        {
                            sParentID = sPID;
                        }
                    }
                }

                var sTableName = oBOD.TableName;
                XID1Click oD1Click = new XID1Click();
                string sQuery = string.Empty;
                //if (!string.IsNullOrEmpty(sFolderName))
                //{
                //    sQuery = "select * from " + sTableName + " WHERE sname ='" + sFolderName + "' and spageno=1 and "+XIConstant.Key_XIDeleted+" = 0 and iBuildingID=" + BuildingID + " order by rOrder asc";
                //}
                //else
                //{
                if (sLoadType == "folder")
                {
                    sQuery = "select * from " + sTableName + " WHERE sParentID ='" + sParentID + "' and spageno=1 and "+ XIConstant.Key_XIDeleted + " = 0 order by rOrder asc";
                }
                else
                {
                    sQuery = "select * from " + sTableName + " WHERE sParentID ='" + sParentID + "' and spageno=1 and "+ XIConstant.Key_XIDeleted + " = 0 and iBuildingID=" + BuildingID + " and iApprovalStatus=10 order by rOrder asc";
                }
                //}
                oD1Click.Query = sQuery;
                oD1Click.Name = oBOD.Name;
                var oResult = oD1Click.OneClick_Run(false);
                if (!string.IsNullOrEmpty(sLoadType) && (sLoadType == "folder" || sLoadType.ToLower() == "instancetreefilter"))
                {
                    if (FolderCount < 2)
                    {
                        foreach (var items in oResult.Values)
                        {
                            FolderCount++;
                            if (sLoadType.ToLower() == "instancetreefilter")
                            {
                                FolderCount = 0;
                            }
                            var ID = items.Attributes["id"].sValue;
                            if (!string.IsNullOrEmpty(sSearchText))
                            {
                                if (items.Attributes["sname"].sValue.IndexOf(sSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    //FilteredData = FilteredData ?? new List<XIIBO>();
                                    FilteredData.Add(ID, items);
                                }
                                Get_SelfStructureRecurssive("", ID, iBODID, sSearchText, BuildingID, sLoadType, "");
                            }
                            else
                            {
                                items.SubChildI.Add(oBOD.Name, Get_SelfStructureRecurssive("", ID, iBODID, sSearchText, BuildingID, sLoadType, "").Values.ToList());
                            }

                        }
                    }
                    else
                    {
                        foreach (var items in oResult.Values)
                        {
                            FolderCount++;
                            var ID = items.Attributes["id"].sValue;
                            XID1Click oC1Click = new XID1Click();
                            string sCQuery = string.Empty;
                            sCQuery = "select * from " + sTableName + " WHERE sParentID ='" + ID + "' and spageno=1 and "+ XIConstant.Key_XIDeleted + " = 0 order by rOrder asc";
                            oC1Click.Query = sCQuery;
                            oC1Click.Name = oBOD.Name;
                            var oCRes = oC1Click.OneClick_Run(false);
                            if (oCRes != null && oCRes.Values.Count() > 0)
                            {
                                items.bHasChilds = true;
                            }
                        }

                    }
                }

                return oResult;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading self structure" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return null;
        }

        public Dictionary<string, XIIBO> Get_SelfStructureRecurssive(string sFolderName, string sParentID, int iBODID, string sSearchText, string BuildingID, string sLoadType, string sFilterType)
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
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID.ToString());
                XIIXI oXI = new XIIXI();
                XIIBO oBOI = new XIIBO();
                List<CNV> oParams = new List<CNV>();
                if (!string.IsNullOrEmpty(sLoadType) && sLoadType.ToLower() == "instancetree")
                {
                    if (!string.IsNullOrEmpty(sFolderName))
                    {
                        oParams.Add(new CNV { sName = "sname", sValue = sFolderName });
                        oParams.Add(new CNV { sName = "iBuildingID", sValue = BuildingID });
                        oBOI = oXI.BOI(oBOD.Name, null, null, oParams);
                        if (oBOI != null && oBOI.Attributes.Values.Count() > 0)
                        {
                            var sPID = oBOI.AttributeI(oBOD.sPrimaryKey).sValue;
                            if (!string.IsNullOrEmpty(sPID))
                            {
                                sParentID = sPID;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(sFolderName) && string.IsNullOrEmpty(sParentID))
                {
                    oParams.Add(new CNV { sName = "sname", sValue = sFolderName });
                    oParams.Add(new CNV { sName = "iBuildingID", sValue = BuildingID });
                    oBOI = oXI.BOI(oBOD.Name, null, null, oParams);
                    if (oBOI != null && oBOI.Attributes.Values.Count() > 0)
                    {
                        var sPID = oBOI.AttributeI(oBOD.sPrimaryKey).sValue;
                        if (!string.IsNullOrEmpty(sPID))
                        {
                            sParentID = sPID;
                        }
                    }
                }

                var sTableName = oBOD.TableName;
                XID1Click oD1Click = new XID1Click();
                string sQuery = string.Empty;
                if (sLoadType == "folder" || sLoadType == "treeamend")
                {
                    sQuery = "select * from " + sTableName + " WHERE sParentID ='" + sParentID + "' and spageno=1 and "+ XIConstant.Key_XIDeleted + " = 0 order by rOrder asc";
                }
                else
                {
                    if (!string.IsNullOrEmpty(sSearchText))
                    {
                        sQuery = "select * from " + sTableName + " WHERE sParentID ='" + sParentID + "' and "+ XIConstant.Key_XIDeleted + " = 0 and iBuildingID=" + BuildingID + " order by rOrder asc";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sFilterType))
                        {
                            if(sFilterType.ToLower() == "red documents")
                            {
                                sQuery = "select * from " + sTableName + " WHERE sType = 20 and iBuildingID=" + BuildingID + " and "+ XIConstant.Key_XIDeleted + " = 0 and iApprovalStatus=20 order by rOrder asc";
                            }
                            else if (sFilterType.ToLower() == "amber documents")
                            {
                                sQuery = "select * from " + sTableName + " WHERE sType = 20 and iBuildingID=" + BuildingID + " and "+ XIConstant.Key_XIDeleted + " = 0 and iApprovalStatus=30 order by rOrder asc";
                            }
                        }
                        else
                        {
                            sQuery = "select * from " + sTableName + " WHERE sParentID ='" + sParentID + "' and spageno=1 and "+ XIConstant.Key_XIDeleted + " = 0 and iBuildingID=" + BuildingID + " order by rOrder asc";
                        }                        
                    }
                }
                oD1Click.Query = sQuery;
                oD1Click.Name = oBOD.Name;
                var oResult = oD1Click.OneClick_Run(true);
                if (!string.IsNullOrEmpty(sLoadType) && (sLoadType == "folder" || sLoadType.ToLower() == "instancetree" || sLoadType.ToLower() == "instancetreefilter" || sLoadType == "treeamend"))
                {
                    //if (FolderCount < 2)
                    //{
                    //    foreach (var items in oResult.Values)
                    //    {
                    //        FolderCount++;
                    //        if (sLoadType.ToLower() == "instancetreefilter")
                    //        {
                    //            FolderCount = 0;
                    //        }
                    //        var ID = items.Attributes["id"].sValue;
                    //        if (!string.IsNullOrEmpty(sSearchText))
                    //        {
                    //            if (items.Attributes["sname"].sValue.IndexOf(sSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    //            {
                    //                //FilteredData = FilteredData ?? new List<XIIBO>();
                    //                FilteredData.Add(ID, items);
                    //            }
                    //            Get_SelfStructureRecurssive("", ID, iBODID, sSearchText, BuildingID, sLoadType);
                    //        }
                    //        else
                    //        {
                    //            items.SubChildI.Add(oBOD.Name, Get_SelfStructureRecurssive("", ID, iBODID, sSearchText, BuildingID, sLoadType).Values.ToList());
                    //        }

                    //    }
                    //}
                    //else
                    //{


                    //}

                    foreach (var items in oResult.Values)
                    {
                        FolderCount++;
                        var ID = items.Attributes["id"].sValue;
                        XID1Click oC1Click = new XID1Click();
                        string sCQuery = string.Empty;
                        sCQuery = "select * from " + sTableName + " WHERE sParentID ='" + ID + "' and spageno=1 and "+ XIConstant.Key_XIDeleted + " = 0 order by rOrder asc";
                        if (!string.IsNullOrEmpty(sSearchText))
                        {
                            sCQuery = "select * from " + sTableName + " WHERE sParentID ='" + ID + "' and "+ XIConstant.Key_XIDeleted + " = 0 order by rOrder asc";
                        }
                        oC1Click.Query = sCQuery;
                        oC1Click.Name = oBOD.Name;
                        var oCRes = oC1Click.OneClick_Run(false);
                        if (oCRes != null && oCRes.Values.Count() > 0)
                        {
                            items.bHasChilds = true;
                        }
                        if (!string.IsNullOrEmpty(sSearchText))
                        {
                            if (items.Attributes["sname"].sValue.IndexOf(sSearchText, StringComparison.OrdinalIgnoreCase) >= 0 || items.Attributes["stags"].sValue.IndexOf(sSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //FilteredData = FilteredData ?? new List<XIIBO>();
                                var sName = items.AttributeI("sname").sValue;
                                var sBatchID = items.AttributeI("iVersionBatchID").sValue;
                                var Exists = FilteredFiles.Where(m => m.sName.ToLower() == sName.ToLower() && m.sType == sBatchID).FirstOrDefault();
                                if (Exists == null)
                                {
                                    FilteredFiles.Add(new CNV { sName = items.AttributeI("sname").sValue, sType = items.AttributeI("iVersionBatchID").sValue });
                                    FilteredData.Add(ID, items);
                                }
                            }
                            Get_SelfStructureRecurssive("", ID, iBODID, sSearchText, BuildingID, sLoadType, sFilterType);
                        }
                        else if (!string.IsNullOrEmpty(sLoadType) && (sLoadType == "treeamend" || sLoadType == "instancetreefilter"))
                        {
                            items.SubChildI.Add(oBOD.Name, Get_SelfStructureRecurssive("", ID, iBODID, sSearchText, BuildingID, sLoadType, sFilterType).Values.ToList());
                        }
                    }
                }
                Dictionary<string, XIIBO> Data = new Dictionary<string, XIIBO>();
                if (sLoadType == "folder" && !string.IsNullOrEmpty(sFolderName))
                {
                    oBOI.SubChildI.Add("0", oResult.Values.ToList());
                    Data.Add("0", oBOI);
                    return Data;
                }
                //oResult.FirstOrDefault().Value.AttributeI("sparentid").sValue = "#";
                return oResult;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading self structure" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return null;
        }

        #endregion Section Definition
    }   
    public class XIDStructureDetail
    {
        public long ID { get; set; }
        public long iParentStructureID { get; set; }
        public long FKiStructureID { get; set; }
        public int iTabXiLinkID { get; set; }
        public int i1ClickID { get; set; }
        public int iCreateDialogID { get; set; }
        public int iEditDialogID { get; set; }
        public int iCreateFormXiLinkID { get; set; }
        public int iEditFormXiLinkID { get; set; }
        public int iCreateLayoutID { get; set; }
        public int iEditLayoutID { get; set; }

        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        public CResult Get_XIStructureDetailsDefinition(string sStructureName = "")
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?

            try
            {
                List<XIDStructureDetail> oXIStructureDetails = new List<XIDStructureDetail>();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (iParentStructureID != 0)
                {
                    Params["iParentStructureID"] = iParentStructureID;
                }
                if (FKiStructureID != 0)
                {
                    Params["FKiStructureID"] = FKiStructureID;
                }
                oXIStructureDetails = Connection.Select<XIDStructureDetail>("XIBOStructureDetail_T", Params).ToList();
                oCResult.oResult = oXIStructureDetails;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult; // always

            //return oURL;
        }

    }
    public class XIBOInstance : XIInstanceBase
    {
        public Dictionary<string, List<XIIBO>> oStructureInstance { get; set; }
        public XIIBO oSubStructureI(string sSubStructureName)
        {
            XIIBO oBOI = new XIIBO();
            List<XIIBO> oBOIList = null;
            Dictionary<string, List<XIIBO>> oSUblist = (Dictionary<string, List<XIIBO>>)this.oStructureInstance;
            oBOIList = GetStructureInstance(sSubStructureName, oSUblist);
            // oBOI = oSUblist[sSubStructureName];
            oBOI.oBOIList = oBOIList;
            return oBOI;
        }

        public List<XIIBO> GetStructureInstance(string sSubStructureName, Dictionary<string, List<XIIBO>> oSUblist)
        {
            List<XIIBO> oBOI = null;
            if (oSUblist.Any(i => i.Key.ToLower().Contains(sSubStructureName.ToLower())))
            {
                oBOI = oSUblist.Where(m => m.Key.ToLower() == sSubStructureName.ToLower()).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                if (oSUblist != null && oSUblist.Count > 0)
                {
                    foreach (var item in oSUblist)
                    {
                        var oSubSructureList = item.Value.Select(x => x.SubChildI).FirstOrDefault();
                        if (oSubSructureList != null && oSubSructureList.Count > 0)
                        {
                            if (oBOI == null)
                            {
                                oBOI = GetStructureInstance(sSubStructureName, oSubSructureList);
                            }
                        }
                    }
                }
            }
            return oBOI;
        }

        public List<XIIBO> oChildBOI(string sNodeName)
        {
            var oStr = oStructureInstance;
            List<XIIBO> oNodeData = CheckRecurrsive(oStr, sNodeName);
            return oNodeData;
        }

        //public List<XIIBO> CheckRecurrsive(Dictionary<string, List<XIIBO>> oStr, string sNodeName)
        //{
        //    List<XIIBO> oBOI = new List<XIIBO>();
        //    oStr.TryGetValue(sNodeName, out oBOI);
        //    if (oBOI != null && oBOI.Count() > 0)
        //    {
        //        return oBOI;
        //    }
        //    else
        //    {
        //        if (oStr.Count() > 0)
        //        {
        //            oBOI = CheckRecurrsive(oStr.FirstOrDefault().Value.FirstOrDefault().SubChildI, sNodeName);
        //        }
        //        return oBOI;
        //    }
        //}

        public List<XIIBO> CheckRecurrsive(Dictionary<string, List<XIIBO>> oStr, string sNodeName)
        {
            List<XIIBO> oBOI = new List<XIIBO>();
            if (oStr == null)
            {
                return null;
            }
            oStr.TryGetValue(sNodeName, out oBOI);
            if (oBOI != null && oBOI.Count() > 0)
            {
                return oBOI;
            }
            else
            {
                if (oStr.Count > 0)
                {
                    List<XIIBO> list = new List<XIIBO>();
                    foreach (var item in oStr)
                    {
                        if (item.Value.Count() > 0)
                        {
                            foreach (var subItem in item.Value)
                            {
                                if (subItem.SubChildI != null && subItem.SubChildI.Count() > 0)
                                {
                                    var oBOIList = CheckRecurrsive(subItem.SubChildI, sNodeName);
                                    if (oBOIList != null && oBOIList.Count() > 0)
                                    {
                                        list.AddRange(oBOIList);
                                    }
                                }

                            }
                        }
                    }
                    if (list != null && list.Count() > 0)
                    {
                        oBOI = list;
                    }

                }
                return oBOI;
            }
        }
        public XIIBO oParentI(string sSubStructureName)
        {
            XIIBO oBOI = new XIIBO();
            List<XIIBO> oBOIList = null;
            Dictionary<string, List<XIIBO>> oSUblist = (Dictionary<string, List<XIIBO>>)this.oStructureInstance;
            oBOIList = GetOperantInstance(sSubStructureName, oSUblist);
            // oBOI = oSUblist[sSubStructureName];
            oBOI.oBOIList = oBOIList;
            return oBOI;
        }
        public List<XIIBO> GetOperantInstance(string sSubStructureName, Dictionary<string, List<XIIBO>> oSUblist)
        {
            List<XIIBO> oBOI = null;
            if (oSUblist.Any(i => i.Key.ToLower().Contains(sSubStructureName.ToLower())))
            {
                oBOI = oSUblist.Where(m => m.Key.ToLower() == sSubStructureName.ToLower()).Select(m => m.Value).FirstOrDefault();
            }
            else
            {
                if (oSUblist != null && oSUblist.Count > 0)
                {
                    var oSubSructureList = oSUblist.FirstOrDefault().Value.Select(x => x.oParent).FirstOrDefault();
                    oBOI = GetOperantInstance(sSubStructureName, (Dictionary<string, List<XIIBO>>)oSubSructureList);
                }
            }
            return oBOI;
        }

    }
}