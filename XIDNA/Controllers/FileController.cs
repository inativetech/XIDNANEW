using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XIDNA.Models;
using System.Web.Mvc;
using XIDNA.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using XIDNA.ViewModels;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using XIDNA.Hubs;
using XIDNA.Common;
using XICore;
using XISystem;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace XIDNA.Controllers
{
   // [Authorize]
    public class FileController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IFileRepository FileRepository;

        public FileController() : this(new FileRepository()) { }

        public FileController(IFileRepository FileRepository)
        {
            this.FileRepository = FileRepository;
        }
        CommonRepository Common = new CommonRepository();
        private readonly string xiconstant;

        //[HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetXIFileDetails(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = FileRepository.GetXIFileDetails(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddXIFileType(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            XIFileTypes FileDetails = FileRepository.AddXIFileType(ID, iUserID, sOrgName, sDatabase);
            if (FileDetails == null)
            {
                return null;
            }
            else
            {

                return PartialView("AddXIFileConfigSettings", FileDetails);
            }
        }
        public ActionResult CreateFileSettings(XIFileTypes model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Create = FileRepository.CreateFileSettings(model, iUserID, sOrgName, sDatabase);
                return Json(Create, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteFileDetails(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                int iStatus = FileRepository.DeleteFileDetails(ID, iUserID, sOrgName, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }

        public ActionResult ViewXIDocDetails()
        {
            return View();
        }
        public ActionResult GetXIDocDetails(jQueryDataTableParamModel param)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                param.iSortCol = Convert.ToInt32(Request["iSortCol_0"]);
                param.sSortDir = Request["sSortDir_0"].ToString();
                var result = FileRepository.GetXIDocDetails(param, iUserID, sOrgName, sDatabase);
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

        public ActionResult AddXIDocDetails(int ID = 0)
        {
            string sDatabase = SessionManager.CoreDatabase;
            int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
            XIDocTypes DocSettings = FileRepository.AddXIDocDetails(ID, iUserID, sOrgName, sDatabase);
            return PartialView("AddXIDocDetails", DocSettings);
        }

        public ActionResult DeleteDocDetails(int ID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                int iStatus = FileRepository.DeleteDocDetails(ID, iUserID, sOrgName, sDatabase);
                return Json(iStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return null;
            }
        }
        public ActionResult CreateDocSettings(XIDocTypes model)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                int iUserID = SessionManager.UserID; string sOrgName = SessionManager.OrganisationName;
                var Create = FileRepository.CreateDocSettings(model, iUserID, sOrgName, sDatabase);
                return Json(Create, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(new VMCustomResponse { Status = false, ResponseMessage = ServiceConstants.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        #region FolerOperations

        [HttpPost]
        public ActionResult FolderOperations(string sParentID, string sFolderName, string sType, string sOldFolder, string ID, string sFileType, string iBuildingID)
        {
            string sDatabase = SessionManager.CoreDatabase;
            try
            {
                sFolderName = sFolderName.Replace(@"//", @"\");
                if (!string.IsNullOrEmpty(sOldFolder))
                {
                    sOldFolder = sOldFolder.Replace(@"//", @"\");
                    sOldFolder = sOldFolder.Replace("&amp;", "&");
                }
                XIInfraCache oCache = new XIInfraCache();
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "xidocumenttree");
                string sPath = string.Empty;
                var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                var sVirtualPath = @"~\" + sVirtualDir + @"\Createif\PDF\Client1\Project1\";
                sFolderName = sFolderName.Replace("&amp;", "&");
                string sNewFolder = string.Empty;
                //Create new folder or Add level in a directory
                if (sType.ToLower() == "create" || sType.ToLower() == "add level")
                {
                    var sFoldName = string.Empty;
                    sFoldName = sFolderName;
                    if (sFolderName.Contains(@"\"))
                    {
                        var FName = sFolderName.LastIndexOf(@"\");
                        var FolName = sFolderName.Substring(FName, sFolderName.Length - FName).Replace(@"\", "");
                        sFoldName = FolName;
                        sNewFolder = FolName;
                    }

                    sPath = Server.MapPath(sVirtualPath) + sFolderName;
                    System.IO.Directory.CreateDirectory(sPath);
                    //Save folder to XIDocumentTree BO
                    XIIBO oBOI = new XIIBO();
                    oBOI.BOD = BOD;
                    oBOI.SetAttribute("sname", sFoldName);
                    oBOI.SetAttribute("sparentid", sParentID);
                    oBOI.SetAttribute("sType", "10");
                    oBOI.SetAttribute("sPageNo", "1");
                    oBOI.SetAttribute("iBuildingID", iBuildingID.ToString());
                    oBOI.SetAttribute("iApprovalStatus", "20");
                    var oCR = oBOI.Save(oBOI);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        var iID = oBOI.AttributeI(BOD.sPrimaryKey).sValue;
                        if (sType.ToLower() == "add level")
                        {
                            //Get all child nodes and change parentid
                            XID1Click o1Click = new XID1Click();
                            o1Click.BOID = BOD.BOID;
                            o1Click.Query = "select * from xidocumenttree_t where sparentid=" + sParentID;
                            var ChildNodes = o1Click.OneClick_Run();
                            if (ChildNodes != null && ChildNodes.Values.Count() > 0)
                            {
                                foreach (var Child in ChildNodes.Values)
                                {
                                    var PID = Child.AttributeI(BOD.sPrimaryKey).sValue;
                                    if (PID != iID)
                                    {
                                        Child.BOD = BOD;
                                        Child.SetAttribute("sparentid", iID);
                                        oCR = Child.Save(Child);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {

                                        }
                                    }
                                }
                                //Copy the files and directories from old to new directory
                                //var sFoldName = string.Empty;
                                sFoldName = sFolderName;
                                if (sFolderName.Contains(@"\"))
                                {
                                    var FName = sFolderName.LastIndexOf(@"\");
                                    var FolName = sFolderName.Substring(0, FName);
                                    sFoldName = FolName;
                                }
                                var targetDirectory = Server.MapPath(sVirtualPath) + sFolderName;
                                string sourceDirectory = Server.MapPath(sVirtualPath) + sFoldName;
                                Copy(sourceDirectory, targetDirectory);
                                //System.IO.Directory.Move(sOldPath, sNewPath);
                            }
                            //Change the full path of files/docs in Documents_T table after adding the new level
                            if (sType.ToLower() == "add level")
                            {
                                List<CNV> oParms = new List<CNV>();
                                oParms.Add(new CNV { sName = "ID", sValue = sParentID });
                                XIIXI oXI = new XIIXI();
                                var BOI = oXI.BOI("xidocumenttree", null, null, oParms);
                                var sParentFolder = BOI.AttributeI("sname").sValue;
                                XIDStructure oStr = new XIDStructure();
                                var oResult = oStr.Get_SelfStructure("", sParentID, 1232, "", iBuildingID, "treeamend");
                                if (oResult != null && oResult.oResult != null)
                                {
                                    var Childs = (Dictionary<string, XIIBO>)oResult.oResult;
                                    if (Childs.Count() > 0)
                                    {
                                        GetFiles(Childs.Values.ToList(), sParentFolder, sParentFolder + @"\" + sNewFolder);
                                    }
                                }
                            }
                        }
                        return Json(iID, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (sType.ToLower() == "rename")
                {
                    //Rename the directory or Doc
                    var sFoldName = string.Empty;
                    sFoldName = sFolderName;
                    if (sFolderName.Contains(@"\"))
                    {
                        var FName = sFolderName.LastIndexOf(@"\");
                        var FolName = sFolderName.Substring(FName, sFolderName.Length - FName).Replace(@"\", "");
                        sFoldName = FolName;
                    }

                    //if (oCR.bOK && oCR.oResult != null)
                    //{
                    //Renaming the directory with Move command
                    sPath = Server.MapPath(sVirtualPath) + sFolderName;
                    string sOldPath = Server.MapPath(sVirtualPath) + sOldFolder;
                    System.IO.Directory.Move(sOldPath, sPath);
                    XIIBO oBOI = new XIIBO();
                    oBOI.BOD = BOD;
                    oBOI.SetAttribute("id", ID);
                    oBOI.SetAttribute("sname", sFoldName);
                    var oCR = oBOI.Save(oBOI);
                    if (oCR.bOK && oCR.oResult != null)
                    {

                    }
                    else
                    {
                        return Json(0, JsonRequestBehavior.AllowGet);
                    }
                    var sOldFoldName = string.Empty;
                    if (sOldFolder.Contains(@"\"))
                    {
                        var FName = sOldFolder.LastIndexOf(@"\");
                        var FolName = sOldFolder.Substring(FName, sOldFolder.Length - FName).Replace(@"\", "");
                        sOldFoldName = FolName;
                    }
                    //Change the full path of files/docs in Documents_T table after adding the new level
                    List<CNV> oParms = new List<CNV>();
                    oParms.Add(new CNV { sName = "sname", sValue = sFoldName });
                    if (!string.IsNullOrEmpty(iBuildingID))
                    {
                        oParms.Add(new CNV { sName = "ibuildingid", sValue = iBuildingID.ToString() });
                    }
                    XIIXI oXI = new XIIXI();
                    var BOI = oXI.BOI("xidocumenttree", null, null, oParms);
                    var PID = BOI.AttributeI(BOD.sPrimaryKey).sValue;
                    XIDStructure oStr = new XIDStructure();
                    var oResult = oStr.Get_SelfStructure("", PID, 1232, "", iBuildingID, "treeamend");
                    if (oResult != null && oResult.oResult != null)
                    {
                        var Childs = (Dictionary<string, XIIBO>)oResult.oResult;
                        if (Childs.Count() > 0)
                        {
                            GetFiles(Childs.Values.ToList(), sOldFoldName, sFoldName);
                        }
                    }
                    var iID = oBOI.AttributeI(BOD.sPrimaryKey).sValue;
                    return Json(iID, JsonRequestBehavior.AllowGet);
                    //}
                    //return Json(0, JsonRequestBehavior.AllowGet);
                }
                else if (sType.ToLower() == "delete" || sType.ToLower() == "remove level")
                {
                    //Delete or remove level of directory
                    //Delete means not the physical delete, updating XIDeleted to 1
                    var oCR = new CResult();
                    if (sType.ToLower() == "remove level")
                    {
                        //Get the child dictories or docs and change the parentid
                        XID1Click o1Click = new XID1Click();
                        o1Click.BOID = BOD.BOID;
                        o1Click.Query = "select * from xidocumenttree_t where sparentid=" + ID + " and "+XIConstant.Key_XIDeleted+"=0";
                        var oRes = o1Click.OneClick_Run();
                        if (oRes != null && oRes.Values.Count() > 0)
                        {
                            foreach (var item in oRes.Values)
                            {
                                item.BOD = BOD;
                                item.SetAttribute("sparentid", sParentID);
                                oCR = item.Save(item);
                            }
                            //Copy the files from old to new directory
                            var sFoldNam = sFolderName;
                            if (sFolderName.Contains(@"\"))
                            {
                                var FName = sFolderName.LastIndexOf(@"\");
                                var FolName = sFolderName.Substring(0, FName);
                                sFoldNam = FolName;
                            }
                            var sourceDirectory = Server.MapPath(sVirtualPath) + sFolderName;
                            string targetDirectory = Server.MapPath(sVirtualPath) + sFoldNam;
                            Copy(sourceDirectory, targetDirectory);
                        }
                    }
                    //Change the directory or doc name by adding Delete_CurrentDateTime
                    var sFoldName = string.Empty;
                    sFoldName = sFolderName;
                    if (sFolderName.Contains(@"\"))
                    {
                        var FName = sFolderName.LastIndexOf(@"\");
                        var FolName = sFolderName.Substring(FName, sFolderName.Length - FName).Replace(@"\", "");
                        sFoldName = FolName;
                    }
                    var sDeleteFolderName = "_Delete_" + DateTime.Now.ToString("dd-MMM-yyyy HHmmss");
                    XIIBO oBOI = new XIIBO();
                    oBOI.BOD = BOD;
                    oBOI.SetAttribute("id", ID);
                    oBOI.SetAttribute("sname", sFoldName + sDeleteFolderName);
                    oBOI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                    oCR = oBOI.Save(oBOI);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        //Renaming the directory
                        if (!string.IsNullOrEmpty(sFileType) && sFileType == "10")
                        {
                            sPath = Server.MapPath(sVirtualPath) + sFolderName;
                            string sOldPath = sPath + sDeleteFolderName;
                            System.IO.Directory.Move(sPath, sOldPath);
                        }
                        //Renaming the Doc
                        else if (!string.IsNullOrEmpty(sFileType) && sFileType == "20")
                        {
                            if (!string.IsNullOrEmpty(sFolderName))
                            {
                                var sOldPath = Server.MapPath(sVirtualPath) + sFolderName + ".pdf";
                                sPath = Server.MapPath(sVirtualPath) + sFolderName + sDeleteFolderName + ".pdf";
                                System.IO.File.Move(sOldPath, sPath);
                                //System.IO.File.Delete(sPath);
                                //System.IO.Directory.Delete(sPath);
                            }
                        }
                        if (sType.ToLower() == "remove level")
                        {
                            //Change the full path of files/docs in Documents_T table after removing the level
                            List<CNV> oParms = new List<CNV>();
                            oParms.Add(new CNV { sName = "ID", sValue = sParentID });
                            XIIXI oXI = new XIIXI();
                            var BOI = oXI.BOI("xidocumenttree", null, null, oParms);
                            var sParentFolder = BOI.AttributeI("sname").sValue;
                            XIDStructure oStr = new XIDStructure();
                            var oResult = oStr.Get_SelfStructure("", sParentID, 1232, "", iBuildingID, "treeamend");
                            if (oResult != null && oResult.oResult != null)
                            {
                                var Childs = (Dictionary<string, XIIBO>)oResult.oResult;
                                if (Childs.Count() > 0)
                                {
                                    GetFiles(Childs.Values.ToList(), sParentFolder + @"\" + sFoldName, sParentFolder);
                                }
                            }
                        }
                        return Json(ID, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), sDatabase);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        //Copy fiels source to target directory
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        //Copy fiels source to target directory recursively
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                string sOldPath = fi.FullName;
                string sNewPath = Path.Combine(target.FullName, fi.Name);
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                //Delete(Path.Combine(source.FullName, fi.Name));
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                if (diSourceSubDir.Name != "New folder")
                {
                    DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                    //Delete(diSourceSubDir.FullName);
                }
            }

        }

        //Delete Directory or File physically
        public static void Delete(string diSourceSubDir)
        {
            var Path = diSourceSubDir;
            FileAttributes attr = System.IO.File.GetAttributes(Path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                System.IO.Directory.Delete(Path);
            }
            else
            {
                System.IO.File.Delete(Path);
            }

        }

        //Update the document full path in Documents_T Table
        public void GetFiles(List<XIIBO> Files, string sOldFolder, string sNewFolder)
        {
            foreach (var items in Files)
            {
                if (items.SubChildI != null && items.SubChildI.Values.Count() > 0)
                {
                    var SUbs = items.SubChildI.Values.ToList().FirstOrDefault();
                    if (SUbs.Count() > 0)
                    {
                        GetFiles(SUbs, sOldFolder, sNewFolder);

                    }
                    if (items.AttributeI("stype").sValue == "20")
                    {
                        var DocID = items.AttributeI("spath").sValue;
                        if (!string.IsNullOrEmpty(DocID))
                        {
                            XIIXI oXI = new XIIXI();
                            var oBOI = oXI.BOI("Documents_T", DocID);
                            var sfullPath = oBOI.AttributeI("sfullPath").sValue;
                            sfullPath = sfullPath.Replace(@"\" + sOldFolder + @"\", @"\" + sNewFolder + @"\");
                            oBOI.SetAttribute("sfullpath", sfullPath);
                            var oCR = oBOI.Save(oBOI);
                        }
                    }
                }
                else
                {

                }
            }
        }

        [HttpPost]
        public ActionResult GetChildData(string nodeID, string nodeName, string iBuildingID, string sLoadType, string sSearchText, string sFilterType)
        {
            try
            {
                nodeName = nodeName.Replace(@"//", @"\");
                nodeName = nodeName.Replace("&amp;", "&");
                XIDStructure oStr = new XIDStructure();
                var Data = oStr.Get_SelfStructure(nodeName, nodeID, 1232, sSearchText, iBuildingID, sLoadType, sFilterType);
                var Res = (Dictionary<string, XIIBO>)Data.oResult;
                var Childs = Res.Values.ToList();
                // create the first list by using a specific "template" type.;

                // start adding "actual" values.
                List<CCIFNode> Datas = new List<CCIFNode>();
                List<Dictionary<string, string>> Nodes = new List<Dictionary<string, string>>();
                foreach (var items in Childs)
                {
                    CCIFNode CNode = new CCIFNode();
                    CNode.bHasChilds = items.bHasChilds;
                    Dictionary<string, CNV> Node = new Dictionary<string, CNV>();
                    Node["id"] = new CNV { sName = "id", sValue = items.AttributeI("id").sValue };
                    Node["sname"] = new CNV { sName = "sname", sValue = items.AttributeI("sname").sValue };
                    Node["stype"] = new CNV { sName = "stype", sValue = items.AttributeI("stype").sValue };
                    Node["iversionbatchid"] = new CNV { sName = "iversionbatchid", sValue = items.AttributeI("iversionbatchid").sValue };
                    Node["sparentid"] = new CNV { sName = "sparentid", sValue = items.AttributeI("sparentid").sValue };
                    Node["brestrict"] = new CNV { sName = "brestrict", sValue = items.AttributeI("brestrict").sValue };
                    Node["bproject"] = new CNV { sName = "bproject", sValue = items.AttributeI("bproject").sValue };
                    Node["sfoldername"] = new CNV { sName = "sfoldername", sValue = items.AttributeI("sfoldername").sValue };
                    Node["iapprovalstatus"] = new CNV { sName = "iapprovalstatus", sValue = items.AttributeI("iapprovalstatus").sValue };
                    Dictionary<string, List<CCIFNode>> SubChildI = new Dictionary<string, List<CCIFNode>>();
                    if (items.SubChildI != null && items.SubChildI.Count() > 0)
                    {
                        List<CCIFNode> Subs = new List<CCIFNode>();
                        foreach (var sub in items.SubChildI)
                        {
                            var sParent = "";
                            foreach (var BOIs in sub.Value.ToList())
                            {
                                sParent = BOIs.AttributeI("sparentid").sValue;
                                CCIFNode CSubNode = new CCIFNode();
                                CSubNode.bHasChilds = BOIs.bHasChilds;
                                Dictionary<string, CNV> SubNode = new Dictionary<string, CNV>();
                                SubNode["id"] = new CNV { sName = "id", sValue = BOIs.AttributeI("id").sValue };
                                SubNode["sname"] = new CNV { sName = "sname", sValue = BOIs.AttributeI("sname").sValue };
                                SubNode["stype"] = new CNV { sName = "stype", sValue = BOIs.AttributeI("stype").sValue };
                                SubNode["iversionbatchid"] = new CNV { sName = "iversionbatchid", sValue = BOIs.AttributeI("iversionbatchid").sValue };
                                SubNode["sparentid"] = new CNV { sName = "sparentid", sValue = BOIs.AttributeI("sparentid").sValue };
                                SubNode["brestrict"] = new CNV { sName = "brestrict", sValue = BOIs.AttributeI("brestrict").sValue };
                                SubNode["bproject"] = new CNV { sName = "bproject", sValue = BOIs.AttributeI("bproject").sValue };
                                SubNode["sfoldername"] = new CNV { sName = "sfoldername", sValue = BOIs.AttributeI("sfoldername").sValue };
                                SubNode["iapprovalstatus"] = new CNV { sName = "iapprovalstatus", sValue = BOIs.AttributeI("iapprovalstatus").sValue };
                                CSubNode.Attributes = SubNode;
                                Subs.Add(CSubNode);
                            }
                            //for (int i = 0; i < 1000; i++)
                            //{
                            //    CCIFNode CtestNode = new CCIFNode();
                            //    CNode.bHasChilds = false;
                            //    Dictionary<string, CNV> test = new Dictionary<string, CNV>();
                            //    test["id"] = new CNV { sName = "id", sValue = i.ToString() };
                            //    test["sname"] = new CNV { sName = "sname", sValue = i.ToString() };
                            //    test["stype"] = new CNV { sName = "stype", sValue = "" };
                            //    test["iversionbatchid"] = new CNV { sName = "iversionbatchid", sValue = "" };
                            //    test["sparentid"] = new CNV { sName = "sparentid", sValue = sParent };
                            //    test["brestrict"] = new CNV { sName = "brestrict", sValue = "" };
                            //    test["bproject"] = new CNV { sName = "bproject", sValue ="" };
                            //    CtestNode.Attributes = test;
                            //    Subs.Add(CtestNode);
                            //}
                            SubChildI[sub.Key] = Subs;
                        }

                    }

                    CNode.Attributes = Node;
                    CNode.SubChildI = SubChildI;
                    Datas.Add(CNode);
                }

                return Json(Datas, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetProjects()
        {
            try
            {
                int iUserID = SessionManager.UserID;

                List<CNV> Data = new List<CNV>();
                XID1Click o1Click = new XID1Click();
                o1Click.BOID = 1237;
                o1Click.Query = "Select * from CR_UserProject_T where fkiuserid=" + iUserID;
                var Res = o1Click.OneClick_Run();
                if (Res != null && Res.Values.Count() > 0)
                {
                    var Projects = Res.Values.ToList();
                    foreach (var Pro in Projects)
                    {
                        var Name = Pro.AttributeI("sproject").sValue;
                        var Projs = Name.Split(',').ToList();
                        foreach(var item in Projs)
                        {
                            XIIXI oXI = new XIIXI();
                            var Proji = oXI.BOI("project", item);
                            if(Proji != null && Proji.Attributes.Count() > 0)
                            {
                                var name = Proji.AttributeI("sname").sValue;
                                var ID1 = Proji.AttributeI("fkinodeid").sValue;
                                Data.Add(new CNV { sName = name, sValue = ID1 });
                            }
                        }
                    }
                }
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion FolderOperations

        #region WaterMark

        [HttpPost]
        public ActionResult PrintPDF(int iDocID)
        {
            try
            {
                XIIXI oXI = new XIIXI();
                var oBOI = oXI.BOI("xidocumenttree", iDocID.ToString());
                if (oBOI != null && oBOI.Attributes.Count() > 0)
                {
                    var sFolderName = oBOI.AttributeI("sFolderName").sValue;
                    var sDocName = oBOI.AttributeI("sName").sValue + ".pdf";
                    var sVirtualDir = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectoryPath"];
                    var sVirtualPath = @"~\" + sVirtualDir + @"\Createif\PDF\Client1\Project1\";
                    var sDocPath = Server.MapPath(sVirtualPath) + sFolderName + "\\" + sDocName;
                    var sNewFile = WriteToPdf(sDocPath, "CreateIF-Space", sDocName);
                    string NewPath = System.Configuration.ConfigurationManager.AppSettings["SharedPath"] + @"\Createif\PDF\Client1\Project1\" + "\\" + sFolderName + "\\" + sNewFile;
                    return Json(NewPath, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Common.SaveErrorLog(ex.ToString(), "");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public string WriteToPdf(string sFilePath, string sWaterMark, string sFileName)
        {
            sWaterMark = "This Document is an Uncontrolled Copy @ and may not be the Latest Revision.@ Please check the Document Portal @ using the QR code below";
            sWaterMark = sWaterMark.Replace("@", "@" + System.Environment.NewLine);
            string line1 = "This Document is an Uncontrolled Copy";
            string line2 = "and may not be the Latest Revision.";
            string line3 = "Please check the Document Portal";
            string line4 = "using the QR code below";
            var sourceFile = Adding1stPageEmpty(sFilePath);
            PdfReader reader = new PdfReader(sourceFile);
            //Rectangle cropbox = reader.GetCropBox(0);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                iTextSharp.text.pdf.BarcodeQRCode qrcode = new BarcodeQRCode("http://portal.create-if.space", 200, 200, null);
                iTextSharp.text.Image img1 = qrcode.GetImage();
                //
                // PDFStamper is the class we use from iTextSharp to alter an existing PDF.
                //
                PdfStamper pdfStamper = new PdfStamper(reader, memoryStream);

                for (int i = 1; i <= reader.NumberOfPages; i++) // Must start at 1 because 0 is not an actual page.
                {
                    //
                    // If you ask for the page size with the method getPageSize(), you always get a
                    // Rectangle object without rotation (rot. 0 degrees)—in other words, the paper size
                    // without orientation. That’s fine if that’s what you’re expecting; but if you reuse
                    // the page, you need to know its orientation. You can ask for it separately with
                    // getPageRotation(), or you can use getPageSizeWithRotation(). - (Manning Java iText Book)
                    //   
                    //
                    iTextSharp.text.Rectangle pageSize = reader.GetPageSizeWithRotation(i);

                    //
                    // Gets the content ABOVE the PDF, Another option is GetUnderContent(...)  
                    // which will place the text below the PDF content. 
                    //
                    PdfContentByte pdfPageContents = pdfStamper.GetUnderContent(i);
                    pdfPageContents.BeginText(); // Start working with text.

                    //
                    // Create a font to work with 
                    //
                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, Encoding.ASCII.EncodingName, false);
                    pdfPageContents.SetFontAndSize(baseFont, 20); // 40 point font
                    pdfPageContents.SetRGBColorFill(240, 240, 240); // Sets the color of the font, Light Gray in this instance


                    //
                    // Angle of the text. This will give us the angle so we can angle the text diagonally 
                    // from the bottom left corner to the top right corner through the use of simple trigonometry. 
                    //
                    float textAngle = (float)FooTheoryMath.GetHypotenuseAngleInDegreesFrom(pageSize.Height, pageSize.Width);

                    //
                    // Note: The x,y of the Pdf Matrix is from bottom left corner. 
                    // This command tells iTextSharp to write the text at a certain location with a certain angle.
                    // Again, this will angle the text from bottom left corner to top right corner and it will 
                    // place the text in the middle of the page. 
                    //
                    pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line1,
                                                    (pageSize.Width / 2) + 10,
                                                    (pageSize.Height / 2) + 10,
                                                    textAngle);
                    pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line2,
                                                   (pageSize.Width / 2) + 30,
                                                    (pageSize.Height / 2) - 0,
                                                    textAngle);
                    pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line3,
                                                    (pageSize.Width / 2) + 50,
                                                    (pageSize.Height / 2) - 10,
                                                    textAngle);
                    pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line4,
                                                    (pageSize.Width / 2) + 70,
                                                    (pageSize.Height / 2) - 20,
                                                    textAngle);
                    //img1.setAbsolutePosition(0f, 0f);
                    Rectangle currentPageRectangle = reader.GetPageSizeWithRotation(i);
                    if (currentPageRectangle.Width > currentPageRectangle.Height)
                    {
                        //page is landscape
                        img1.SetAbsolutePosition(1091, 10);
                    }
                    else
                    {
                        //page is portrait
                        img1.SetAbsolutePosition(500, 15);
                    }

                    img1.ScalePercent(20f);
                    img1.Alignment = Element.ALIGN_CENTER;
                    //img1.ScaleToFit(180f, 250f);
                    pdfPageContents.AddImage(img1);
                    pdfPageContents.EndText(); // Done working with text
                }
                pdfStamper.FormFlattening = true; // enable this if you want the PDF flattened. 
                pdfStamper.Close(); // Always close the stamper or you'll have a 0 byte stream. 
                                    //DOwnload PDF
                var sNewFilePath = DownloadPDF(memoryStream, sFileName, sFilePath);
                return sNewFilePath;
                //return Json(fileresult, JsonRequestBehavior.AllowGet);
            }
        }
        public static class FooTheoryMath
        {
            public static double GetHypotenuseAngleInDegreesFrom(double opposite, double adjacent)
            {
                //http://www.regentsprep.org/Regents/Math/rtritrig/LtrigA.htm
                // Tan <angle> = opposite/adjacent
                // Math.Atan2: http://msdn.microsoft.com/en-us/library/system.math.atan2(VS.80).aspx 

                double radians = Math.Atan2(opposite, adjacent); // Get Radians for Atan2
                double angle = radians * (180 / Math.PI); // Change back to degrees
                return angle;
            }
        }
        public string Adding1stPageEmpty(string src)
        {
            string outputFileName = Path.GetTempFileName();
            PdfReader reader = new PdfReader(src);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(outputFileName, FileMode.Create));
            int total = reader.NumberOfPages + 1;
            for (int pageNumber = total; pageNumber > 0; pageNumber--)
            {
                if (pageNumber == 1)
                {
                    stamper.InsertPage(pageNumber, PageSize.A4);
                }
            }
            stamper.Close();
            reader.Close();
            return outputFileName;
        }

        public string DownloadPDF(MemoryStream memoryStream, string sFileName, string sFilePath)
        {
            var fileresult = new FileContentResult((byte[])(memoryStream.ToArray()), "application/pdf");
            var NewFile = "Print-" + sFileName;
            fileresult.FileDownloadName = "Print-" + sFileName;
            sFilePath = sFilePath.Replace(sFileName, "Print-" + sFileName);
            System.IO.File.WriteAllBytes(sFilePath, fileresult.FileContents);

            //var binary = fileresult.BuildPdf(ControllerContext);
            //System.IO.File.WriteAllBytes(@"c:\foobar.pdf", binary);
            return NewFile;
        }

        #endregion WaterMark
        public ActionResult GenerateQuotes()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadFile(int FKiOriginID, int FKiSourceID, int FKiClassID, HttpPostedFileBase file)
        {
            XIInfraCache oCache = new XIInfraCache();
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            List<string> Info = new List<string>();
            try
            {
                oTrace.oTrace.Add(oCR.oTrace);
                if (file.ContentLength > 0)
                {
                    List<string> csvData = new List<string>();
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(file.InputStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            csvData.Add(reader.ReadLine());
                        }
                    }
                    XIIXI oXI = new XIIXI();
                    XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "xileadimport", null);
                    XIIBO oBOI = new XIIBO();
                    oBOI.BOD = oBOD;
                    oBOI.SetAttribute("sLeadData", string.Join("", csvData));
                    oBOI.SetAttribute("FKiClassID", FKiClassID.ToString());
                    oBOI.SetAttribute("FKiOriginID", FKiOriginID.ToString());
                    oBOI.SetAttribute("FKiSourceID", FKiSourceID.ToString());
                    var res = oBOI.Save(oBOI);
                    string sSessionID = HttpContext.Session.SessionID;
                    var resp = GetRecordByID<CResult>("api/XMLHandler/PostMHXML/" + ((XIIBO)res.oResult).AttributeI("id").iValue+"/'"+ sSessionID+"'/0/null");
                    XIIBO oQuoteI = new XIIBO();
                    QueryEngine oQE = new QueryEngine();
                    string sWhereCondition = "FKiQSInstanceID=" + resp.oResult.ToString() + ","+ XIConstant.Key_XIDeleted + "=0";
                    oCResult = oQE.Execute_QueryEngine("Aggregations", "sGUID,ID,sInsurer,iQuoteStatus,rCompulsoryExcess, rVoluntaryExcess, rTotalExcess, rMonthlyPrice, rMonthlyTotal, zDefaultDeposit, rFinalQuote, bIsFlood, bIsApplyFlood", sWhereCondition);
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
            return PartialView("GenerateQuotes", ((Dictionary<string, XIIBO>)oCResult.oResult).Select(x => x.Value).ToList());
        }        
        public static T GetRecordByID<T>(string path)
        {
            string token = null;
            var client = new RestSharp.RestClient();
            client.BaseUrl = new Uri(ConfigurationManager.AppSettings["APIBaseUrl"].ToString());
            var req = new RestSharp.RestRequest(path, RestSharp.Method.GET);
            req.AddHeader(System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("Bearer {0}", token));
            req.Timeout = 300000;
            var res = client.Execute(req).Content;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            T collection = serializer.Deserialize<T>(res);
            return collection;
        }
    }
}