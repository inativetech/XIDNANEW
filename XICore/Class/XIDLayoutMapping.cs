using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XICore
{
    [Table("XILayoutMapping_T")]
    public class XIDLayoutMapping : XIInstanceBase
    {
        [Key]
        public int ID { get; set; }
        public int PopupLayoutID { get; set; }
        public int XiLinkID { get; set; }
        public int PopupID { get; set; }
        public string Type { get; set; }
        public string ContentType { get; set; }
        public string HTMLCode { get; set; }
        public bool IsValueSet { get; set; }
        public int FKiApplicationID { get; set; }

        private int oMyPlaceHolderID;

        public int PlaceHolderID
        {
            get
            {
                return oMyPlaceHolderID;
            }
            set
            {
                oMyPlaceHolderID = value;
            }
        }
        //My Code
        public int StatusTypeID { get; set; }
    }
}