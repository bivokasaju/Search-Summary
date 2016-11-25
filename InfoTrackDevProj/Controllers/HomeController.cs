using InfoTrackDevProj.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;


namespace InfoTrackDevProj.Controllers
{
    public class HomeController : Controller
    {
        public WebResponse response;
        public StreamReader reader;
        public StringBuilder content;
        public string pattern = "";
        public string result = "";
        public string data = "";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(inputModel model)
        {
            model.keyword = model.keyword != null ? model.keyword.Replace(" ", "+") : null;
            string URL = "https://www.google.com.au/search?num=100&q=" + model.keyword;

            SetConnection(URL);

            data = content.ToString();
            data = data.Replace("<b>", "");
            data = data.Replace("</b>", "");

            pattern = String.Format("(<cite>)(.*?){0}(.*?)(<\\/cite>)", model.url);

            SetCountAndPosition(model);
                        
            ViewBag.Response = data;

            reader.Close();
            response.Close();

            return View(model);
        }

        #region Set connection and get content
        public void SetConnection(string URL)
        {
            WebRequest request = WebRequest.Create(URL);
            request.Credentials = CredentialCache.DefaultCredentials;

            response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            reader = new StreamReader(dataStream);

            string responseFromServer = "";
            content = new StringBuilder();

            while ((responseFromServer = reader.ReadLine()) != null)
            {
                content.Append(responseFromServer);
            }
        }
        #endregion

        #region Set count and position
        public void SetCountAndPosition(inputModel model)
        {
            MatchCollection matches = Regex.Matches(data, pattern);

            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    model.count++;
                    result += m;
                }
            }

            string[] stringSeparators = new string[] { "</cite>" };
            string[] list = result.Split(stringSeparators, StringSplitOptions.None);

            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    if (list[i].Contains(model.url))
                    {
                        model.position += (i + 1).ToString() + ", ";
                    }
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            model.position = model.position != null ? model.position.Substring(0, model.position.Length - 2) : "0";
        }
        #endregion
    }
}