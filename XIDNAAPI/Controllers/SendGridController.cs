using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using XICore;
using XIDatabase;
using xiEnumSystem;
using XISystem;

namespace XIDNAAPI.Controllers
{
    [RoutePrefix("api/SendGrid")]
    public class SendGridController : ApiController
    {
        public SendGridController()
        {

        }
        [HttpPost]
        [Route("Events")]
        public IHttpActionResult Events(SendGridEvents[] events)
        {
            string sMessageID = string.Empty;
            int StatusID = 0;
            XIIBO oBOI = new XIIBO();
            XIIXI oXII = new XIIXI();
            XIInfraCache oCache = new XIInfraCache();
            XIDBO BOD = new XIDBO();
            foreach (SendGridEvents item in events)
            {
                List<CNV> oWhrParams = new List<CNV>();
                sMessageID = item.sg_message_id.Split('.')[0];
                oWhrParams.Add(new CNV { sName = "sReference", sValue = sMessageID });

                oBOI = oXII.BOI("communicationinstance", null, null, oWhrParams);
                if (oBOI?.Attributes != null)
                {
                    StatusID = (int)Enum.Parse(typeof(EnumEmailStatus), item.@event);
                    string sEventDate = oBOI.TimeStampToDateTime(Convert.ToDouble(item.timestamp));
                    bool bIsOpened = oBOI.Attributes["bIsOpened"].sValue == "0" ? true : false;
                    if (item.@event == EnumEmailStatus.open.ToString() && bIsOpened)
                    {
                        oBOI.SetAttribute("bIsOpened", "1");
                        oBOI.SetAttribute("OpenDT", sEventDate);
                    }
                    oBOI.SetAttribute("iInteractionStatus", StatusID.ToString());
                    //oBOI.SetAttribute();
                    var oResult = oBOI.Save(oBOI);
                    if (oResult.bOK)
                    {
                        int ID = 0;
                        int.TryParse(oBOI.Attributes["ID"].sValue, out ID);
                        if (ID > 0)
                        {

                            BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "commsTransaction");
                            oBOI = new XIIBO();
                            oBOI.BOD = BOD;
                            oBOI.SetAttribute("FKiCommInstanceID", ID.ToString());
                            oBOI.SetAttribute("iType", StatusID.ToString());
                            oBOI.SetAttribute("CreateDate", sEventDate);
                            oBOI.Save(oBOI);
                        }
                    }
                }
            }
            return Ok();
        }

        //[Route("Events")]
        //[HttpPost]
        //public IHttpActionResult Events([FromBody]SendGridEvents[] events)
        //{
        //    string connection = ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString;
        //    List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
        //    CResult oCResult = new CResult();
        //    CResult oCR = new CResult();
        //    long iTraceLevel = 10;
        //    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
        //    if (iTraceLevel > 0)
        //    {
        //        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
        //    }
        //    if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
        //    {
        //        oCResult.xiStatus = oCR.xiStatus;
        //        //oCResult.oTraceStack.Trace("Stage",sError)
        //    }
        //    //in the case of
        //    //xiEnumSystem.xiFuncResult.xiLogicalError
        //    oCResult.sMessage = "someone tried to do something they shouldnt";
        //    //var DetailRequest = Request.Content;

        //    //var DetailReuqest = JsonConvert.SerializeObject(Request, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        //    using (SqlConnection con = new SqlConnection(connection))
        //    {
        //        con.Open();
        //        try
        //        {
        //            foreach (var data in events)
        //            {
        //                tbl_sendGrid grid = new tbl_sendGrid();
        //                grid.sEvent = data.@event;
        //                grid.sAttempt = data.attempt;
        //                grid.sCategory = data.category;
        //                grid.sEmailAddress = data.email;
        //                grid.sEventDate = TimeStampToDateTime(Convert.ToDouble(data.timestamp));
        //                grid.sReason = data.reason;
        //                grid.sResponse = data.response;
        //                //grid.SendGridEventID = data.;
        //                grid.sStatus = data.status;
        //                grid.sUrl = data.url;
        //                grid.sSG_Event_ID = data.sg_event_id;
        //                grid.sSG_Message_ID = data.sg_message_id;
        //                grid.sUserAgent = data.useragent;
        //                grid.sIP = data.ip;


        //                values.Clear();
        //                foreach (var item in grid.GetType().GetProperties())
        //                {
        //                    values.Add(new KeyValuePair<string, string>(item.Name, item?.GetValue(grid)?.ToString()));
        //                }

        //                string xQry = getInsertCommand("SendGridEvents", values);
        //                SqlCommand cmdi = new SqlCommand(xQry, con);
        //                cmdi.ExecuteNonQuery();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            oCResult.sMessage = ex.Message + "____Inner___" + ex.InnerException + "____trace__" + ex.StackTrace;
        //            XIDefinitionBase xibase = new XIDefinitionBase();
        //            oCResult.LogToFile();
        //            xibase.SaveErrortoDB(oCResult);
        //        }
        //    }
        //    return Ok();
        //}
        //private static string getInsertCommand(string table, List<KeyValuePair<string, string>> values)
        //{
        //    string query = null;
        //    query += "INSERT INTO " + table + " ( ";
        //    foreach (var item in values)
        //    {
        //        query += item.Key;
        //        query += ", ";
        //    }
        //    query = query.Remove(query.Length - 2, 2);
        //    query += ") VALUES ( ";
        //    foreach (var item in values)
        //    {
        //        if (item.Key.GetType().Name == "System.Int") // or any other numerics
        //        {
        //            query += item.Value;
        //        }
        //        else
        //        {
        //            query += "'";
        //            query += item.Value;
        //            query += "'";
        //        }
        //        query += ", ";
        //    }
        //    query = query.Remove(query.Length - 2, 2);
        //    query += ")";
        //    return query;
        //}
        //private string TimeStampToDateTime(double timeStamp)
        //{
        //    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //    dateTime = dateTime.AddSeconds(timeStamp).ToLocalTime();
        //    return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //}
    }

    public class SendGridEvents
    {
        public string @event { get; set; }
        public string email { get; set; }
        public string category { get; set; }
        public string response { get; set; }
        public string attempt { get; set; }
        public string timestamp { get; set; }
        public string url { get; set; }
        public string status { get; set; }
        public string reason { get; set; }
        public string type { get; set; }
        public string useragent { get; set; }
        public string ip { get; set; }
        public string sg_event_id { get; set; }
        public string sg_message_id { get; set; }
    }
    public class tbl_sendGrid
    {

        //public int SendGridEventID { get; set; }

        public string sEvent { get; set; }

        public string sEmailAddress { get; set; }

        public string sCategory { get; set; }

        public string sResponse { get; set; }

        public string sAttempt { get; set; }

        public string sEventDate { get; set; }

        public string sUrl { get; set; }

        public string sStatus { get; set; }

        public string sReason { get; set; }

        public string sType { get; set; }
        public string sUserAgent { get; set; }
        public string sIP { get; set; }
        public string sSG_Event_ID { get; set; }
        public string sSG_Message_ID { get; set; }
        public string sDetailRequest { get; set; }
    }
}
