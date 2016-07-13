using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;

namespace FindNewDocs
{
    public class Program
    {
        static void Main (string[] args)
        {
            string lastRun = "";
            using (StreamReader sr = new StreamReader(".\\LastRun.txt"))
            {
                lastRun = sr.ReadLine();
            }

            //Add 5 hours to account for GMT
            string currentRun = String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss'.'fffff}", DateTime.Now.AddHours(5));
            using (StreamWriter sw = new StreamWriter(".\\LastRun.txt"))
            {
                sw.WriteLine(currentRun);
            }

            RestClient client = new RestClient("http://localhost:8080/integrationserver/view/321Z06Q_000B6R6WY000001/result?category=DOCUMENT");
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("x-integrationserver-password", "dwa");
            request.AddHeader("x-integrationserver-username", "ckidd");
            request.AddParameter("application/xml",
                $"<viewParameters><vslText>[creationDate] > '{lastRun}'</vslText></viewParameters>", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            string hash = response.Headers[0].Value.ToString();

            root root = JsonConvert.DeserializeObject<root>(response.Content);

            using (StreamWriter sw = new StreamWriter(".\\DocID.txt"))
            {
                foreach (resultRows r in root.resultRows)
                {
                    string tempId = r.fields[0].value;
                    sw.WriteLine(tempId);
                }
            }
            
            EndRun(hash);
        }

        public static void EndRun(string hash)
        {
            RestClient client = new RestClient("http://localhost:8080/integrationserver/connection");
            RestRequest request = new RestRequest(Method.DELETE);
            request.AddHeader("x-integrationserver-session-hash", hash);
            request.AddHeader("x-integrationserver-password", "dwa");
            request.AddHeader("x-integrationserver-username", "ckidd");
            IRestResponse response = client.Execute(request);
        }
    }

    class fields
    {
        public string columnId { get; set; }
        public string value { get; set; }
    }

    class resultRows
    {
        public List<fields> fields { get; set; } = new List<fields>();
    }

    class root
    {
        public List<resultRows> resultRows { get; set; } = new List<resultRows>();
    }
}