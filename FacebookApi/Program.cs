using System;
using System.Data.SqlTypes;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

string cookie = "";

string access_token = "";

string fields = "id,created_time,from,full_picture,message,reactions.limit(1).summary(true)";

int limit = 100;

string URL = "https://graph.facebook.com/v15.0/1988191741413952/feed?fields=" + fields + "&limit=" + limit + "&access_token=" + access_token;

string useragent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";

WebClient client = new WebClient();
client.Headers.Add("user-agent", useragent);
client.Headers.Add("cookie", cookie);

string folder_post = "vietnamquora";

string json_post = "posts.json";
List<MyPost> list_posts = new List<MyPost>();

if (!Directory.Exists(folder_post))
{
    Directory.CreateDirectory(folder_post);
}

for (int i = 0; i < 1000; i++)
{
    string content = client.DownloadString(URL);

    File.WriteAllText("json.txt", content);

    JObject objs = JObject.Parse(content);

    foreach (var item in objs["data"])
    {
        string post_id = string.Empty;
        string created_time = string.Empty;
        string message = string.Empty;
        string full_picture = string.Empty;
        int reactions = 0;

        try
        {
            post_id = item["id"].ToString();
            created_time = item["created_time"].ToString();
            message = item["message"].ToString();
            full_picture = item["full_picture"].ToString();
            reactions = Convert.ToInt32(item["reactions"]["summary"]["total_count"]);
        }
        catch (Exception ex)
        {
        }
        if (string.IsNullOrEmpty(message))
        {
            continue;
        }
        if (message.Contains("𝐅𝐄𝐓𝐔𝐒"))
        {
            continue;
        }
        if (post_id.Equals("1988191741413952_2514591055440682"))
        {
            continue;
        }

        /*
        int index = message.IndexOf("Copy xin vui lòng giữ nguyên credit");

        if (index < 0)
        {
            index = message.IndexOf("Vui lòng giữ nguyên credit sau");
        }
        if (index < 0)
        {
            index = message.IndexOf("Khi copy vui lòng giữ nguyên credit");
        }
        if (index < 0)
        {
            index = message.IndexOf("Khi chia sẻ vui lòng giữ nguyên credit");
        }
        if (index < 0)
        {
            index = message.IndexOf("Khi copy bài viết, xin vui lòng giữ nguyên credit sau");
        }
        if (index < 0)
        {
            index = message.IndexOf("Khi copy, xin vui lòng giữ nguyên credit sau");
        }
        if (index > 0)
        {
            message = message.Substring(0, index);
        }*/

        // title
        string title = post_id;
        foreach (var line in message.Split("\n"))
        {
            if (line.Contains("#") && !line.Contains("?") && line.Length < 50) continue;
            if (line.Contains("[") && line.Contains("]")) continue;
            if (string.IsNullOrEmpty(line)) continue;
            if (line.Length < 10) continue;

            title = line;

            if (title.Length > 100)
            {
                title = title.Substring(0, 100);
            }
            if (TextToSlug(title).Length < 3)
            {
                continue;
            }
            break;
        }

        MyPost mypost = new MyPost();
        mypost.source = "vietnamquora";
        mypost.category = "QRVN";
        mypost.tags = "";
        mypost.title = title;
        mypost.slug = TextToSlug(title);

        mypost.vote = reactions;
        mypost.link = "https://www.facebook.com/" + post_id.Replace("_", "/posts/");
        mypost.created = DateTime.Parse(created_time).ToString("yyyy-MM-dd");

        mypost.creator = "quora.com";
        mypost.image = "";

        // HTML
        mypost.file = "<div>" + message.Replace("\n", "<br>") + "<br><br>" + (string.IsNullOrEmpty(full_picture) ? "" : "<img src='" + full_picture + "'>") + "</div>";

        File.WriteAllText(folder_post + "/" + mypost.slug + ".html", mypost.file);

        mypost.file = mypost.slug + ".html";

        // write json file
        list_posts.Add(mypost);

    }

    try
    {
        URL = objs["paging"]["next"].ToString();
    }
    catch (Exception ex)
    {
        break;
    }
    Thread.Sleep(10);

}

var json = JsonConvert.SerializeObject(list_posts, Formatting.Indented);
File.WriteAllText(json_post, json);






string TextToSlug(string text)
{
    text = text.ToLower();
    text = Regex.Replace(text, @"(à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ)", "a");
    text = Regex.Replace(text, @"(è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ)", "e");
    text = Regex.Replace(text, @"(ì|í|ị|ỉ|ĩ)", "i");
    text = Regex.Replace(text, @"(ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ)", "o");
    text = Regex.Replace(text, @"(ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ)", "u");
    text = Regex.Replace(text, @"(ỳ|ý|ỵ|ỷ|ỹ)", "y");
    text = Regex.Replace(text, @"(đ)", "d");

    text = Regex.Replace(text, @"(À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ)", "A");
    text = Regex.Replace(text, @"(È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ)", "E");
    text = Regex.Replace(text, @"(Ì|Í|Ị|Ỉ|Ĩ)", "I");
    text = Regex.Replace(text, @"(Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ)", "O");
    text = Regex.Replace(text, @"(Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ)", "U");
    text = Regex.Replace(text, @"(Ỳ|Ý|Ỵ|Ỷ|Ỹ)", "Y");
    text = Regex.Replace(text, @"(Đ)", "D");

    text = Regex.Replace(text, @"[^a-zA-Z0-9\s-]", "");
    text = Regex.Replace(text, @"\s+", " ").Trim();
    text = Regex.Replace(text, @"\s", "-");
    text = Regex.Replace(text, @"-+", "-");
    return text;
}


class MyPost
{
    public string source { get; set; }
    public string category { get; set; }
    public string tags { get; set; }
    public string title { get; set; }
    public string slug { get; set; }
    public int vote { get; set; }
    public string creator { get; set; }
    public string created { get; set; }
    public string image { get; set; }
    public string link { get; set; }
    public string file { get; set; }
}

