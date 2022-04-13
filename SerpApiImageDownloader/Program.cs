
using Newtonsoft.Json.Linq;

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

var apiKey = "15e3b817924a8e506eb3d48ef7099e5e37722f1748f43996fc2238b6bc3a1b17";
var baseUrl = $"https://serpapi.com/search.json?tbm=isch&ijn=0&api_key={apiKey}";

using (var client = new HttpClient())
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Safari/537.36");
    foreach (var searchTerm in searchTerms)
    {
        var url = baseUrl + $"&q={searchTerm}";

        try
        {
            Console.WriteLine("Searching: " + searchTerm + "\n");

            var content = await client.GetStringAsync(url);
            var jArray = JObject.Parse(content)["images_results"] as JArray;

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
                    Console.WriteLine("Downloading: " + item["link"]);

                    var imageUrl = item["original"];
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