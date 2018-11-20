using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;  //need install RestSharp
using System.Web;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace ITSRV
{
    public class baidu
    {
        // for official usage : Please refresh token every 24 hours, and save it in application envirable, do not use getAccessToken() in each call. 
        public string getAccesstoken()
        {
            string apikey = "CCxYUHCLKRvZ4trcnGjwYg6B";
            string secretkey = "c6YMA1Lp9aULLahG8mkqSrU7evk6rFaO";
            string tokenurl = "https://aip.baidubce.com/oauth/2.0/token";
            tokenurl += "?grant_type=client_credentials&client_id=" + apikey + "&client_secret=" + secretkey;
            // clientid -> apikey
            // clientsecret ->secretkey
            WebRequest r = WebRequest.Create(tokenurl);
            r.Method = "GET";
            WebResponse res = r.GetResponse();
            string status = ((HttpWebResponse)res).StatusDescription;
            StreamReader sr = new StreamReader(res.GetResponseStream());
            string json = sr.ReadToEnd();
            JObject ser = JObject.Parse(json);
            string accesstoken = ser["access_token"].ToString();
            return accesstoken;
        }

        public string voicetotext(string str64, int len)
        {
            //gettoken();
            string token = getAccesstoken(); 

            //send POST request to Baidu Voice Recoginzation 
            string sendurl = "http://vop.baidu.com/server_api";
            System.Net.HttpWebRequest r = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sendurl);
            r.Method = "POST";
            r.Accept = "application/json";
            string postdata = @"{""format"": ""amr"",""rate"":16000,""dev_pid"":1536,""channel"":1,""token"":""" + token + @""",""cuid"":""sap-itrbo123"",""len"":" + len + @",""speech"":""" + str64 + @"""}";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postdata);
            r.ContentLength = byteArray.Length;
            System.IO.Stream dataStream = r.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //get Response from Baidu API
            System.Net.WebResponse res = r.GetResponse();
            string status = ((HttpWebResponse)res).StatusDescription;
            dataStream = res.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JObject ser = JObject.Parse(responseFromServer);
            reader.Close();
            dataStream.Close();
            res.Close();
            if (ser["err_no"].ToString() != "0")
                return "0"; //Failed to get Text
            else
            {
                string s = ser["result"][0].ToString();
                return s; //return the Text
            }

        }
        public string textToVoice(string itext)
        {
            WebClient client = new WebClient();

            string token = getAccesstoken(); //get the access token from Baidu API

            //send request to Baidu TTS API
            string sendurl = "http://tsn.baidu.com/text2audio";
            System.Net.HttpWebRequest r = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sendurl);
            r.Method = "POST";
            r.Accept = "application/json";

            string postdata = "tex=" + itext + "&lan=zh&cuid=e6625421&ctp=1&aue=3&tok=" + token;
            // 能否再专业一点。。。。
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postdata);
            r.ContentLength = byteArray.Length;
            System.IO.Stream dataStream = r.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Get Response from Baidu
            System.Net.WebResponse res = r.GetResponse();
            string status = ((HttpWebResponse)res).StatusDescription;
            dataStream = res.GetResponseStream();
            if (res.ContentType != "audio/mp3")
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                JObject ser = JObject.Parse(responseFromServer);
                reader.Close();
                dataStream.Close();
                res.Close();
                if (ser["err_no"].ToString() != "0")
                    return "0"; //failed to convert voice.
                else
                    return ser["result"][0].ToString();
                //return error message
            }
            else
            {
                MemoryStream ms = new MemoryStream();
                dataStream.CopyTo(ms);
                var audioBytes = ms.ToArray();
                string b64 = ConvertToBase64(ms);
                return b64; // return base64 string of voice.
            }
        }
        public string ConvertToBase64(MemoryStream ms) //convert any stream to Base64 String. 
        {

            var audioBytes = ms.ToArray();
            
            var base64String = Convert.ToBase64String(audioBytes); 

            return base64String;

        }
        public string addFace(string face64, string uname)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string token = getAccesstoken();
            string sendurl = "https://aip.baidubce.com/rest/2.0/face/v3/faceset/user/add?access_token=" + token;
            System.Net.HttpWebRequest r = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sendurl);
            r.ContentType = "application/json";
            r.Method = "POST";
            r.KeepAlive = true;
            string userid = (Convert.ToInt32(getUid("group1")) + 1).ToString();
            string str = "{\"image\":\"" + face64 + "\",\"image_type\":\"BASE64\",\"group_id\":\"group1\",\"user_id\":\"" + userid + "\",\"user_info\":\"" + uname + "\",\"quality_control\":\"LOW\",\"liveness_control\":\"NORMAL\"}";
            byte[] buffer = Encoding.Default.GetBytes(str);
            r.ContentLength = buffer.Length;
            r.GetRequestStream().Write(buffer, 0, buffer.Length);
            var resp = r.GetResponse();

            StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            JObject ser = JObject.Parse(result);
            reader.Close();
            resp.Close();

            return "OK";
        }
        public string getUid(string groupnanme)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string token = getAccesstoken();
            string sendurl = "https://aip.baidubce.com/rest/2.0/face/v3/faceset/group/getusers?access_token=" + token;
            System.Net.HttpWebRequest r = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sendurl);
            r.ContentType = "application/json";
            r.Method = "POST";
            r.KeepAlive = true;
            string str = "{\"group_id\":\"group1\"}"; //默认组为group1
            byte[] buffer = Encoding.Default.GetBytes(str);
            r.ContentLength = buffer.Length;
            r.GetRequestStream().Write(buffer, 0, buffer.Length);
            var resp = r.GetResponse();

            StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            JObject ser = JObject.Parse(result);
            string lastuid = ser["result"]["user_id_list"].Last.ToString();
            reader.Close();
            resp.Close();

            return lastuid;
        }

    }
}
