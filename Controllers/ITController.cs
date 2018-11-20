using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ITSRV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ITController : ControllerBase
    {
        // GET: api/IT
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/IT/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/IT
        [HttpPost]
        public string Post(string type)
        {
            string str64;
            string question = "";
            string answer = "";
            string answer64 = "";
            string jstr = "";
            baidu bd = new baidu();
            tuling tl = new tuling();

                var reader = new StreamReader(Request.Body);
                str64 = reader.ReadToEnd();
                JObject ser = JObject.Parse(str64);
                reader.Close();
                string b64 = ser["b64"].ToString();
                string len = ser["len"].ToString();
                if (b64 != "")
                {
                    if (type == "1") //1-> Voice
                    {
                        question = bd.voicetotext(b64, System.Convert.ToInt32(len));
                    }
                    else //如果是文字，则无需转化
                    {
                        question = b64; 
                     }
                    answer = tl.getAI(question);
                    if (answer.Length < 200) //字数小于100汉字，就可以发音读出来
                        answer64 = bd.textToVoice(answer);
                }
                else
                    question = "NoInput";


                jstr = "{\"question\":\"" + question + "\",\"answer\":\"" + answer + "\",\"answer64\":\"" + answer64 + "\"}";
                Response.ContentType = "text/plain";//  "application/json"; 
                return jstr;
            
            
        }
        

    }
}
