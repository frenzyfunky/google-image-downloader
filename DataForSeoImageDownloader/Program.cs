using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

var searchTerms = new List<string>()
{
    "Banana",
    "Frog",
    "Computer",
    "Aspen",
    "Pine tree"
};

// you can change this
var saveDir = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads"));

var login = "API_LOGIN_KEY"; // can be obtained from dashboard. Usually login email format
var password = "API_PASSWORD"; // this one is also from dashboard

using (var client = new HttpClient())
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Safari/537.36");
    foreach (var searchTerm in searchTerms)
    {
        try
        {
            Console.WriteLine("Searching: " + searchTerm + "\n");

            var postData = new List<object>();
            postData.Add(new
            {
                language_code = "en",
                location_code = "2840",
                keyword = searchTerm
            });

            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}"))}");
            var response = await client.PostAsync("https://api.dataforseo.com/v3/serp/google/images/live/advanced", new StringContent(JsonConvert.SerializeObject(postData)));
            var content = await response.Content.ReadAsStringAsync();

            var jArray = JObject.Parse(content)["tasks"][0]["result"][0]["items"] as JArray;

            int i = 1;
            foreach (var item in jArray)
            {
                if (i > 5)
                {
                    break;
                }

                var directoryInfo = Directory.CreateDirectory(Path.Combine(saveDir.FullName, searchTerm));

                try
                {
                    Console.WriteLine("Downloading: " + item["source_url"]);
                    client.DefaultRequestHeaders.Authorization = null;
                    var imageUrl = item["source_url"];
                    var imageReponse = await client.GetAsync(imageUrl.ToString());
                    var extension = imageReponse.Content.Headers.FirstOrDefault(h => h.Key == "Content-Type").Value.FirstOrDefault().Split("/")[1];

                    var bytes = await imageReponse.Content.ReadAsByteArrayAsync();

                    using (var fs = new FileStream(Path.Combine(directoryInfo.FullName, $"{i}.{extension}"), FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception)
                {
                    continue;
                }

                i++;
            }
        }
        catch (Exception)
        {
            continue;
        }
    }
}