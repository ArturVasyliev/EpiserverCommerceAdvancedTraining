using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    [Serializable()]
    public class MyMessage
    {

        public string poNr { get; set; }
        public string status { get; set; }
        public int orderGroupId { get; set; }


        public MyMessage()
        {


        }
    }
}