﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XIDNA.Models;
using XIDNA.ViewModels;

namespace XIDNA.Repository
{
    public interface IGenerateQueryRepository
    {
        DTResponse GetQueryList(jQueryDataTableParamModel param, int iUserID, string sOrgName, int OrgID, string sDatabase);
        int UpdateQuery(VMReports model, int iUserID, string sOrgName, string sDatabase);
        VMReports GetQueryByID(int QueryID, string sDatabase);
        VMReports GetAllBos(int OrgID, int iUserID, string sOrgName, string sDatabase);
        List<BOFields> GetAvailableFields(int BOID, int Type, int ClassType, int OrgID, int iUserID, string sOrgName, string sDatabase);
        bool IsExistsQueryName(string QueryName, int ID, int OrgID, string sDatabase);
        int SaveQuery(VMReports model, int iUserID, string sOrgName, string sDatabase);
        int SaveQueryCopy(int ReportID, int OrgID, int iUserID, string sOrgName, string sDatabase);
        BOFields GetWhereValues(int FieldID, int OrgID, string sDatabase);
        List<VMDropDown> GetDBValuesForField(string Query, int iUserID, string sOrgName, string sDatabase);
        List<DisplayName> GetOperators(string DataType, string sDatabase);
        VMQueryPreview GetQueryPreview(int QueryID, int PageIndex, int UserID, string sOrgName, string sDatabase, int orgid);
        DTResponse GetPreviewInForm(jQueryDataTableParamModel param, int ID, string Query, string Fields, int BOID, int iUserID, int OrgID, string sOrgName, string sDatabase, string sCurrentGuestUser);
        VMQueryPreview GetHeadingsOfQuery(VMReports model, int iUserID, string sOrgName, string sDatabase);
        VMQueryPreview GetPreviewInFormEdited(string Query, int BOID, string sDatabase, int iUserID, int OrgID, string sOrgName);
        VMQueryPreview GetQueryStatus(int ID, string Query, string Fields, int iUserID, string sOrgName, string sDatabase, int BOID);
        int DeleteQuery(int QueryID, string sDatabase);
        VMQueryPreview QueryDynamicForm(int QueryID, int UserID, string sOrgName, string sDatabase, int Orgid);
        VMQueryActions GetActionFeildsByID(int QueryID, string sDatabase);
        List<Classes> GetClasses(string sDatabase);
        bool IsPopupNameExists(string Name, string sDatabase);
        List<Reports> GetAllOneClicks(int OrgID, int ParentID, int ID, string sDatabase);
        int SaveQueryTargets(VMReports model, int iUserID, string sOrgName, string sDatabase);
        int SaveQueryScheduler(VMReports model, int iUserID, string sOrgName, string sDatabase);
        int SaveQueryActions(VMQueryActions model, string sDatabase);
        int SaveQuerySearchFields(int QueryID, string SearchFields, bool bIsMultiSearch, bool bIsXICreatedBy, bool bIsXIUpdatedBy, int FKiCrtd1ClickID, int FKiUpdtd1ClickID, string sDatabase);
        DTResponse GetTargetsGrid(jQueryDataTableParamModel model, int ID, int OrgID, int iUserID, string sOrgName, string sDatabase);
        DTResponse GetSchedulersList(jQueryDataTableParamModel model, int ID, int OrgID, int iUserID, string sOrgName, string sDatabase);
        List<VMDropDown> GetTargetUsers(int ID, int OrgID, string sDatabase);
        string CheckQueryStatus(string Query, int BOID, int iUserID, int OrgID, string sOrgName, string sDatabase, string CurrentGuestUser);
        VMReports GetXiLinksList(string sDatabase);
        VMReports GetXiParametersList(string sDatabase);
        List<VMDropDown> GetGroupsByBOID(int BOID, string sDatabase);
        VMCustomResponse SaveOneclickNvs(int OneClickID, string[] NVPairs, int OrgID, int iUserID, string sDatabase);
        VMCustomResponse SaveParamerterNDVs(int OneClickID, string[] NDVPairs, int OrgID, int iUserID, string sDatabase);
        Dictionary<string, string> AllBusinessObjects(int iUserID, string sOrgName, string sDatabase);
        //SaveXI1ClickLinkPairs
        VMCustomResponse Save1ClickLinks(VMReports OneClickXILinks, int OrgID, int iUserID, string sDatabase);
        List<VMXI1ClickLinks> XILinkValues(int QueryID, int iUserID, string sOrgName, string sDatabase);
        #region Save1ClickPermission
        VMCustomResponse Save1ClickPermission(int[] NVPairs, int i1ClickID, string sType, int OrgID, int iUserID, string sDatabase);
        #endregion Save1ClickPermission
    }
}
