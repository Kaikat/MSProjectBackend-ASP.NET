using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Utilities
{
    public class BasicResponse
    {
        public string id;
        public string message;
        public bool error;

        public BasicResponse()
        {
            id = "";
            message = "";
            error = true;
        }

        public BasicResponse(string responseID)
        {
            id = responseID;
            message = "";
            error = true;
        }

        public BasicResponse(string responseID, string responseMessage)
        {
            id = responseID;
            message = responseMessage;
            error = true;
        }
    }
}