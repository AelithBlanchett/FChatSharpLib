using FChatSharpLib.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace FChatSharpLib.Services
{
    public static class FListClient
    {
        public static string GetTicket(string username, string password)
        {
            var jsonData = JsonConvert.SerializeObject(new
            {
                account = username,
                password
            }, Formatting.Indented);

            var jsonResult = "";
            using (WebClient wc = new WebClient())
            {
                NameValueCollection vals = new NameValueCollection();
                vals.Add("account", username);
                vals.Add("password", password);
                var response = wc.UploadValues("https://www.f-list.net/json/getApiTicket.php", vals);
                jsonResult = Encoding.UTF8.GetString(response);
            }

            var jsonObject = JsonConvert.DeserializeObject<GetTicketResponse>(jsonResult);

            if (string.IsNullOrEmpty(jsonObject.ticket))
            {
                throw new Exception("Couldn't get authentication info from F-List API. Please restart. Reason: " + jsonResult.ToString());
            }
            return jsonObject.ticket;
        }
    }
}
