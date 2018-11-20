using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web;
using System.Net;

namespace ITSRV
{
    public class tuling
    {
        private string apikey = "f013e1e1fba645279fe8d203991434db";
        private string userid = "111243331";
        public string getAI(string Question)
        {
            string sendurl = "http://openapi.tuling123.com/openapi/api/v2";
            System.Net.HttpWebRequest r = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sendurl);
            r.Method = "POST";
            r.Accept = "application/json";
            string postdata = "{\"reqType\":0,\"perception\": {\"inputText\": {\"text\": \"" + Question + "\"},\"selfInfo\": {\"location\": {\"city\": \"北京\"}}},\"userInfo\": {\"apiKey\": \"" + apikey + "\",\"userId\": \"" + userid +"\"}}";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postdata);
            r.ContentLength = byteArray.Length;
            System.IO.Stream dataStream = r.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //get response from tuling.
            System.Net.WebResponse res = r.GetResponse();
            string status = ((HttpWebResponse)res).StatusDescription;
            dataStream = res.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JObject ser = JObject.Parse(responseFromServer);
            int iamount = ser["results"].Count();
            string replystr = "";
            for (var i = 0; i <= iamount - 1; i++) // 遍历所有结果
            {
                if (System.Convert.ToString(ser["results"][i]["resultType"]) == "text")
                    replystr += ser["results"][i]["values"]["text"].ToString();

                if (System.Convert.ToString(ser["results"][i]["resultType"]) == "url")
                    replystr += ser["results"][i]["values"]["url"].ToString();
                    
            }
            reader.Close();
            dataStream.Close();
            res.Close();
            return replystr;
        }
    }
}
