using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Web;
using XIDatabase;
using XISystem;
using static XIDatabase.XIDBAPI;

namespace XICore
{
    public class XIDGroup
    {
        public int ID { get; set; }
        public int BOID { get; set; }
        public string BOFieldIDs { get; set; }
        public string BOSqlFieldNames { get; set; }
        public string BOFieldNames { get; set; }
        public string GroupName { get; set; }
        public int TypeID { get; set; }
        public bool IsMultiColumnGroup { get; set; }
        public string Description { get; set; }
        [DapperIgnore]
        public string sBOName { get; set; }
        //My Code
        [NotMapped]
        public string Type { get; set; }
        [NotMapped]
        public string OldName { get; set; }
        [NotMapped]
        public List<ListBoxItems> AvailableFields { get; set; }
        [NotMapped]
        public List<ListBoxItems> AssignedFields { get; set; }
        public class ListBoxItems
        {
            public string ID { get; set; }
            public string FieldName { get; set; }
        }
        public bool bIsCrtdBy { get; set; }
        public bool bIsCrtdWhn { get; set; }
        public bool bIsUpdtdBy { get; set; }
        public bool bIsUpdtdWhn { get; set; }
        public List<XIDropDown> ddlAllBOs { get; set; }
        CResult oCResult = new CResult();

        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);

        public string ConcatanateGroupFields(string sJoinString)
        {
            try
            {
                if (!string.IsNullOrEmpty(sJoinString))
                {
                    List<string> strFields = new List<string>();
                    if (!string.IsNullOrEmpty(BOFieldNames))
                    {
                        List<string> Fields = BOFieldNames.Split(',').Select(split => split.ToString()).ToList();
                        Fields.ToList().ForEach(f =>
                        {
                            if (!string.IsNullOrEmpty(sBOName))
                            {
                                string str = "Convert(varchar(256), " + "[" + sBOName + "]." + f.Trim() + ")";
                                strFields.Add(str);
                            }
                            else
                            {
                                string str = "Convert(varchar(256), ISNULL(" + f + ",''))";
                                strFields.Add(str);
                            }

                        });
                    }
                    return strFields.Count > 0 ? string.Join("+'" + sJoinString + "'+", strFields) : string.Empty;
                }
                else
                {
                    return BOFieldNames;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ConcatanateFields(string sFields, string sJoinString)
        {
            try
            {
                if (!string.IsNullOrEmpty(sJoinString))
                {
                    List<string> strFields = new List<string>();
                    if (!string.IsNullOrEmpty(sFields))
                    {
                        List<string> Fields = sFields.Split(',').Select(split => split.ToString()).ToList();
                        Fields.ToList().ForEach(f =>
                        {
                            string str = "Convert(varchar(256), " + f + ")";
                            strFields.Add(str);
                        });
                    }
                    return strFields.Count > 0 ? string.Join("+'" + sJoinString + "'+", strFields) : string.Empty;
                }
                else
                {
                    return BOFieldNames;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //My Code
        public CResult Get_XIBOGroupDetails()
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
            try
            {
                XIDGroup oXIDGroup = new XIDGroup();
                if (ID > 0)
                {
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["ID"] = ID;
                    oXIDGroup = Connection.Select<XIDGroup>("XIBOGroup_T_N", Params).FirstOrDefault();
                    Dictionary<string, object> BOFieldsParams = new Dictionary<string, object>();
                    BOFieldsParams["BOID"] = oXIDGroup.BOID;
                    var BOFields = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", BOFieldsParams).ToList();
                    List<string> ids = oXIDGroup.BOFieldIDs.Split(',').ToList();
                    List<int> AssignedIds = new List<int>();
                    if (oXIDGroup.BOFieldIDs == "0")
                    {
                        oXIDGroup.AssignedFields = new List<ListBoxItems>();
                    }
                    else
                    {
                        var NewIDs = oXIDGroup.BOFieldIDs.Split(new string[] { ", " }, StringSplitOptions.None);

                        List<ListBoxItems> AssignedFields = new List<ListBoxItems>();
                        foreach (var items in NewIDs)
                        {
                            int iFieldID = Convert.ToInt32(items);
                            Dictionary<string, object> BOFieldParams = new Dictionary<string, object>();
                            BOFieldParams["ID"] = iFieldID;
                            XIDAttribute XIBOFields = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", BOFieldParams).FirstOrDefault();
                            ListBoxItems Items = new ListBoxItems();
                            Items.ID = XIBOFields.ID + "-" + XIBOFields.Name;
                            Items.FieldName = XIBOFields.Name;
                            AssignedFields.Add(Items);
                            AssignedIds.Add(Convert.ToInt32(items));
                        }
                        oXIDGroup.AssignedFields = AssignedFields;
                    }

                    //List<int> AllIds = BOFields.Where(m => m.BOID == BOID).Select(m => m.ID).ToList();
                    List<int> AllIds = BOFields.Select(m => m.ID).ToList();
                    IEnumerable<int> RemainingIDs = AllIds.Except(AssignedIds).ToList();
                    List<ListBoxItems> list = new List<ListBoxItems>();
                    foreach (var iFieldID in RemainingIDs)
                    {
                        //BOFields bo = BOFields.Where(m => m.ID == field).FirstOrDefault();
                        Dictionary<string, object> BOFieldParam = new Dictionary<string, object>();
                        BOFieldParam["ID"] = iFieldID;
                        XIDAttribute oXIBOAttr = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", BOFieldParam).FirstOrDefault();

                        ListBoxItems Items = new ListBoxItems();
                        Items.ID = oXIBOAttr.ID + "-" + oXIBOAttr.Name;
                        Items.FieldName = oXIBOAttr.Name;
                        list.Add(Items);
                    }
                    oXIDGroup.AvailableFields = list;
                    oXIDGroup.IsMultiColumnGroup = oXIDGroup.IsMultiColumnGroup;

                }
                oCResult.oResult = oXIDGroup;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Menu definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                XIDXI oXIDXI = new XIDXI();
                oXIDXI.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public XIDGroup Get_GroupDetails()
        {
            XIDGroup oXIDGroup = new XIDGroup();
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                if (ID != 0)
                {
                    Dictionary<string, object> BOFieldsParams = new Dictionary<string, object>();
                    BOFieldsParams["ID"] = ID;
                    var oGroupD = Connection.Select<XIDGroup>("XIBOGroup_T_N", BOFieldsParams).FirstOrDefault();
                    
                    var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, oGroupD.BOID.ToString());
                    oXIDGroup = BOD.Groups.Values.ToList().Where(m => m.ID == ID).FirstOrDefault();
                    var BOAttrs = BOD.Attributes.Values.ToList();
                    List<string> ids = oXIDGroup.BOFieldIDs.Split(',').ToList();
                    List<int> AssignedIds = new List<int>();
                    if (string.IsNullOrEmpty(oXIDGroup.BOFieldIDs) && string.IsNullOrEmpty(oXIDGroup.BOFieldNames))
                    {
                        oXIDGroup.AssignedFields = new List<ListBoxItems>();
                    }
                    else if (!string.IsNullOrEmpty(oXIDGroup.BOFieldIDs))
                    {
                        var NewIDs = oXIDGroup.BOFieldIDs.Split(new string[] { ", " }, StringSplitOptions.None);
                        List<ListBoxItems> AssignedFields = new List<ListBoxItems>();
                        foreach (var items in NewIDs)
                        {
                            if (!string.IsNullOrEmpty(items))
                            {
                                int iFieldID = Convert.ToInt32(items);
                                XIDAttribute XIBOFields = BOAttrs.Where(m => m.ID == iFieldID).FirstOrDefault();
                                if (XIBOFields != null)
                                {
                                    ListBoxItems Items = new ListBoxItems();
                                    Items.ID = XIBOFields.ID + "-" + XIBOFields.Name;
                                    Items.FieldName = XIBOFields.Name;
                                    AssignedFields.Add(Items);
                                    AssignedIds.Add(XIBOFields.ID);
                                }
                            }                            
                        }
                        oXIDGroup.AssignedFields = AssignedFields;
                    }
                    else if (!string.IsNullOrEmpty(oXIDGroup.BOFieldNames))
                    {
                        var NewIDs = oXIDGroup.BOFieldNames.Split(new string[] { ", " }, StringSplitOptions.None);

                        List<ListBoxItems> AssignedFields = new List<ListBoxItems>();
                        foreach (var items in NewIDs)
                        {
                            XIDAttribute XIBOFields = BOAttrs.Where(m => m.Name.ToLower() == items.ToLower()).FirstOrDefault();
                            if (XIBOFields != null)
                            {
                                ListBoxItems Items = new ListBoxItems();
                                Items.ID = XIBOFields.ID + "-" + XIBOFields.Name;
                                Items.FieldName = XIBOFields.Name;
                                AssignedFields.Add(Items);
                                AssignedIds.Add(XIBOFields.ID);
                            }                            
                        }
                        oXIDGroup.AssignedFields = AssignedFields;
                    }

                    //List<int> AllIds = BOFields.Where(m => m.BOID == BOID).Select(m => m.ID).ToList();
                    List<int> AllIds = BOAttrs.Select(m => m.ID).ToList();
                    IEnumerable<int> RemainingIDs = AllIds.Except(AssignedIds).ToList();
                    List<ListBoxItems> list = new List<ListBoxItems>();
                    foreach (var iFieldID in RemainingIDs)
                    {
                        XIDAttribute oXIBOAttr = BOAttrs.Where(m => m.ID == iFieldID).FirstOrDefault();
                        if (oXIBOAttr != null)
                        {
                            ListBoxItems Items = new ListBoxItems();
                            Items.ID = oXIBOAttr.ID + "-" + oXIBOAttr.Name;
                            Items.FieldName = oXIBOAttr.Name;
                            list.Add(Items);
                        }
                    }
                    oXIDGroup.AvailableFields = list;
                    oXIDGroup.IsMultiColumnGroup = oXIDGroup.IsMultiColumnGroup;

                }
                else if (BOID != 0)
                {
                    var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BOID.ToString());
                    var BOFields = BOD.Attributes.Values.ToList();
                    List<int> AllIds = BOFields.Select(m => m.ID).ToList();
                    List<int> AssignedIds = new List<int>();
                    IEnumerable<int> RemainingIDs = AllIds.Except(AssignedIds).ToList();
                    List<ListBoxItems> list = new List<ListBoxItems>();
                    foreach (var iFieldID in RemainingIDs)
                    {
                        XIDAttribute oXIBOAttr = BOFields.Where(m=>m.ID == iFieldID).FirstOrDefault();
                        if (oXIBOAttr != null)
                        {
                            ListBoxItems Items = new ListBoxItems();
                            Items.ID = oXIBOAttr.ID + "-" + oXIBOAttr.Name;
                            Items.FieldName = oXIBOAttr.Name;
                            list.Add(Items);
                        }                        
                    }
                    oXIDGroup.AvailableFields = list;
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Menu definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                XIDXI oXIDXI = new XIDXI();
                oXIDXI.SaveErrortoDB(oCResult);
            }
            return oXIDGroup;
        }
    }
}