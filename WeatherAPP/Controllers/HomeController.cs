using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using WeatherAPP.Models;
using System.Net;

namespace WeatherAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient HttpClient;
        private readonly IConfiguration configuration;
        private static string postUrl = "a&fbclid=IwAR0WkICmGBIoxd2uOLMAy5Jeh4UD1nuB1TRyz83oGNuKY6KToeJ_kYRXWyM";

        public IHttpClientFactory HttpClientFactory { get; }

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory )
        {
            _logger = logger;
            HttpClientFactory = httpClientFactory;
        
        }

        public IActionResult Index()
        {
            return View();
         
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();  
        }

         public ActionResult GetJsonData()
        {
            return Json("OK");
        }



        [HttpPost]
        public async Task<IActionResult> GetWeather([FromBody] string location = "")
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return RedirectToAction("Index");
            }
            var returnTemp = "";
            var returnWindSpeed = "";
            var returnLocation = "";
            var returnCountry = "";
            var returnError = "";
            var returnFeelLike = "";
            var returnCombineLocation = "";
            float temper = 0;
            float wind = 0;

            //var data1 = new { returnWindSpeed, returnTemp, returnError };

            //return Json(data1);

            String URLString = $"https://opendata.fmi.fi/wfs?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi%3A%3Aforecast%3A%3Aedited%3A%3Aweather%3A%3Ascandinavia%3A%3Apoint%3A%3Atimevaluepair&place={location}&fbclid=IwAR0WkICmGBIoxd2uOLMAy5Jeh4UD1nuB1TRyz83oGNuKY6KToeJ_kYRXWyM";
            string url = $"/wfs?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi%3A%3Aforecast%3A%3Aedited%3A%3Aweather%3A%3Ascandinavia%3A%3Apoint%3A%3Atimevaluepair&place={location.Trim().ToLower()}&fbclid=IwAR0WkICmGBIoxd2uOLMAy5Jeh4UD1nuB1TRyz83oGNuKY6KToeJ_kYRXWyM";

            try
            {

                //using (HttpClient client = new HttpClient())
                using (var client = HttpClientFactory.CreateClient("WeatherAPI"))
                {

                    var response = await client.GetAsync(url);
                    //if (response == null || response.StatusCode != HttpStatusCode.OK)
                    //{
                    //    returnError = response?.ReasonPhrase ?? "API Error";

                    //}
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync() ?? "";

                        XmlReader xmlReader = XmlReader.Create(new StringReader(content));
                        string currTempurature = "mts-1-1-Temperature";
                        string currWindSpeed = "mts-1-1-windspeedms";
                        string xmlLocation = $"http://xml.fmi.fi/namespace/locationcode/name";
                        string xmlCountry = $"http://xml.fmi.fi/namespace/location/country";
                        DateTime now = new DateTime();
                        XElement el = new XElement("root");


                        while (xmlReader.Read() && (string.IsNullOrEmpty(returnLocation) || string.IsNullOrEmpty(returnCountry) || string.IsNullOrEmpty(returnTemp) || string.IsNullOrEmpty(returnWindSpeed)))
                        {
                            //to findout statename
                            if (string.IsNullOrEmpty(returnLocation))
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "gml:name")
                                {
                                    if (xmlReader.HasAttributes)
                                    {
                                        if (xmlReader.GetAttribute(name: "codeSpace") != null)
                                        {
                                            string atrr = xmlReader.GetAttribute("codeSpace").ToLower().Trim();
                                            if (atrr == xmlLocation)
                                            {

                                                el = (XElement)XNode.ReadFrom(xmlReader);
                                                returnLocation = el.Value;
                                            }
                                        }
                                    }
                                }
                            }

                            //to findout countryname 
                            //to findout statename
                            if (string.IsNullOrEmpty(returnCountry))
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "target:country")
                                {
                                    if (xmlReader.HasAttributes)
                                    {
                                        if (xmlReader.GetAttribute(name: "codeSpace") != null)
                                        {
                                            string atrr = xmlReader.GetAttribute("codeSpace").ToLower().Trim();
                                            if (atrr == xmlCountry)
                                            {

                                                el = (XElement)XNode.ReadFrom(xmlReader);
                                                returnCountry = el.Value;
                                            }
                                        }
                                    }
                                }
                            }


                            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "wml2:MeasurementTimeseries")
                            {
                                if (xmlReader.HasAttributes)
                                {
                                    if (xmlReader.GetAttribute("gml:id") != null)
                                    {
                                        string atrr = xmlReader.GetAttribute("gml:id").ToLower().Trim();

                                        if (string.IsNullOrEmpty(returnWindSpeed))
                                        {
                                            if (atrr == currWindSpeed.ToLower().Trim())
                                            {
                                                el = (XElement)XNode.ReadFrom(xmlReader);
                                                IEnumerable<XElement> names =
                                                            (from e in el.Elements()
                                                             select e).ToList();
                                                foreach (XElement n in names)
                                                {
                                                    string a = n.Value;
                                                    a = a.Replace("\n", "").Replace("\t", "").Trim();

                                                    now = DateTime.Parse(a.Split(' ')[0]).AddHours(-4);
                                                    int ssnow = now.Hour;
                                                    if (ssnow == DateTime.Now.Hour && now.Date == DateTime.Now.Date)
                                                    {
                                                        wind = float.Parse(a.Split('Z')[1].Trim());
                                                    
                                                        returnWindSpeed = a.Split('Z')[1].Trim() + "m/s";
                                                    }
                                                }

                                            }
                                        }

                                        if (string.IsNullOrWhiteSpace(returnTemp))
                                        {

                                            if (atrr == currTempurature.ToLower().Trim())
                                            {
                                                el = (XElement)XNode.ReadFrom(xmlReader);
                                                IEnumerable<XElement> names =
                                                            (from e in el.Elements()
                                                             select e).ToList();
                                                foreach (XElement n in names)
                                                {
                                                    string a = n.Value;
                                                    a = a.Replace("\n", "").Replace("\t", "").Trim();

                                                    now = DateTime.Parse(a.Split(' ')[0]).AddHours(-4);
                                                    int ssnow = now.Hour;
                                                    if (ssnow == DateTime.Now.Hour && now.Date == DateTime.Now.Date)
                                                    {
                                                        temper = float.Parse(a.Split('Z')[1].Trim());
                                                    
                                                        returnTemp = a.Split('Z')[1].Trim() + " °C";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        returnError = response?.ReasonPhrase ?? "API Error";
                    }
                }
                }
                catch (Exception ex) 
                {

                     returnError = ex?.InnerException?.Message??"Connection Error";
                }
                  

            returnWindSpeed = string.IsNullOrEmpty(returnWindSpeed) ? "N/A" : returnWindSpeed;
            returnTemp = string.IsNullOrEmpty(returnTemp) ? "N/A" : returnTemp;
            returnFeelLike =   (wind ==0 && temper==0)?"N/A":(13.2 + 0.6215 * temper - 11.37 * Math.Pow(wind, 0.16) + 0.3965 * temper * Math.Pow(wind, 0.16)).ToString("F2")  + "°C";
            //returnFeelLike = CAL.ToString("F2")+;  // (13.2 + 0.6215 * temper - 11.37 * Math.Pow(wind, 0.16) + 0.3965 * temper * Math.Pow(wind, 0.16).ToString("F2") + "°C";
             returnCombineLocation =   (string.IsNullOrEmpty(returnLocation) && string.IsNullOrEmpty(returnCountry))?"N/A": returnLocation + (string.IsNullOrWhiteSpace(returnCountry) ? "" : ", " + returnCountry);
            var data = new { returnWindSpeed ,returnTemp, returnCombineLocation, returnFeelLike, returnError };

            return Json(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
