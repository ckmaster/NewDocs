using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.IO;

namespace FindNewDocs
{
    public class Program
    {
        static void Main (string[] args)
        {
            RestClient client = new RestClient("http://localhost:8080/integrationserver/view/321Z06Q_000B6R6WY000001/result?category=DOCUMENT");
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/xml");
            request.AddHeader("x-integrationserver-password", "dwa");
            request.AddHeader("x-integrationserver-username", "ckidd");
            request.AddParameter("application/xml", 
                "<viewParameters><vslText>[creationDate] > '2016-06-21 05:00:00.000000'</vslText></viewParameters>", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            string hash = response.Headers[0].Value.ToString();
            string view = response.Content;
            string[] delims = { "\"value\":\"", "\"},{\"" };
            string[] split = view.Split(delims, StringSplitOptions.None);
            string newestDoc = split[1];

            string previousDoc = "";
            using (StreamReader sr = new StreamReader(".\\DocID.txt"))
            {
                previousDoc = sr.ReadLine();
            }

            using (StreamWriter sw = new StreamWriter(".\\DocID.txt"))
            {
                sw.WriteLine(newestDoc);
            }

            if (!previousDoc.Equals(newestDoc))
            {
                Console.WriteLine("A new document has been added to the system since the last run:  " + newestDoc);
            }
            else
            {
                Console.WriteLine("No new documents have been added since the last run.  The newest doc is:  " + previousDoc);
            }

            EndRun(hash);
            Console.ReadLine();
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
}