using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LentaPerser
{
    internal class Program
    {
        const string requestStr = @"{'nodeCode':'gd6dd9b5e854cf23f28aa622863dd6913','filters':[],'typeSearch':1,'sortingType':'ByPopularity','offset':0,'limit':200,'updateFilters':true}";
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            WebRequest req = WebRequest.Create("https://lenta.com/api/v1/skus/list");
            req.ContentType = "application/json";
            req.Method = "POST";
            req.Headers.Add(HttpRequestHeader.UserAgent, "dotnet");
            req.ContentLength = requestStr.Length;
            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write(requestStr);
            }
            WebResponse resp = req.GetResponse();
            JToken token;
            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
            using (JsonTextReader JReader = new JsonTextReader(reader))
            {
                token = JToken.Load(JReader);
            }
            JArray arr = token["skus"] as JArray;
            foreach (JToken sku in arr)
            {
                string weight;
                string subTitle = sku["subTitle"].Value<string>().Trim();
                JArray weightOptions = sku["weightOptionsMax"] as JArray;
                if (subTitle.Length == 0)
                    weight = "1";
                else if (weightOptions != null && weightOptions.Count != 0)
                {
                    weight = weightOptions[0].Value<string>();
                }
                else
                {
                    string[] separated = subTitle.Replace(", ", ",").Split(",");
                    string[] weightFromSeparated;
                    if (separated.Length == 1 && !char.IsDigit(separated[0][0]))
                    {
                        weight = "1";
                    }
                    else
                    {
                        if (separated.Length == 2)
                            weightFromSeparated = separated[1].Split(" ");
                        else
                            weightFromSeparated = separated[0].Split(" ");
                        if (weightFromSeparated[1] == "шт")
                            weight = "1";
                        else
                        {

                            float weightF = float.Parse(weightFromSeparated[0]);
                            if (weightFromSeparated[1] == "г")
                                weightF /= 1000;
                            weight = weightF.ToString();
                        }
                    }
                    
                }
                Console.WriteLine(sku["title"] + ";1;" + weight);
            }
        }
    }
}
