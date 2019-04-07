using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace RedditBackgroundChanger
{
    class Token
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public string Token_type { get; set; }
    }

    class Program
    {
        private static string downloadDirectory = @"C:\Users\jkana\AppData\Local\Temp\DesktopChanger\";
        static bool inDebug = true;
        public static Token GetToken()
        {
            NameValueCollection settings = ConfigurationManager.AppSettings;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://www.reddit.com/api/v1/access_token");
            wr.Method = "POST";
            NetworkCredential credentials = new NetworkCredential("5naxj1FRo3ooIQ", settings["secret"]);
            wr.Credentials = credentials;


            using (Stream dataStream = wr.GetRequestStream())
            {
                string postData = $"grant_type=password&username={settings["username"]}&password={settings["password"]}";
                byte[] postDataByteArray = Encoding.UTF8.GetBytes(postData);
                dataStream.Write(postDataByteArray, 0, postDataByteArray.Length);
            }

            try
            {
                using (WebResponse response = wr.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            string responseFromServer = reader.ReadToEnd();
                            JObject redditToken = JObject.Parse(responseFromServer);
                            JToken jToken = redditToken;
                            Token myToken = jToken.ToObject<Token>();
                            return myToken;
                        }
                    }
                }

            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                return new Token();
            }
        }

        public static List<string> GetPosts(Token token, bool over18Allowed = false)
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://oauth.reddit.com/r/wallpapers/new?limit=100");
            wr.Method = "GET";
            wr.UserAgent = "DesktopWallpaperChanger/0.1 by Airspace";
            wr.Headers["Authorization"] = $"{token.Token_type} {token.Access_token}";

            try
            {
                using (WebResponse response = wr.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            string responseFromServer = reader.ReadToEnd();
                            JObject redditPosts = JObject.Parse(responseFromServer);
                            List<JToken> results = redditPosts["data"]["children"].Children().ToList();
                            List<string> posts = new List<string>();
                            foreach(JToken result in results)
                            {
                                bool postIsOver18 = (bool)result["data"]["over_18"];
                                if(over18Allowed || !postIsOver18)
                                {
                                    bool isSelfPosted = (bool)result["data"]["is_self"];
                                    bool possiblyAlbum = (result["data"]["thumbnail_height"].ToString() == "");
                                    if (!isSelfPosted && !possiblyAlbum)
                                    {
                                        
                                        int height = (int)result["data"]["preview"]["images"][0]["source"]["height"];
                                        int width = (int)result["data"]["preview"]["images"][0]["source"]["width"];
                                        if (width % 16 == 0 && height % 9 == 0)
                                            posts.Add(result["data"]["url"].ToString());
                                    }
                                }
                            }

                            return posts;
                        }
                    }
                }
            }
            catch(WebException)
            {
                return new List<string>();
            }
        }

        public static int ChooseRandomPost(int numberOfPosts)
        {
            Random rnd = new Random();
            return rnd.Next(0, numberOfPosts);
        }

        public static void DownloadImageFromPost(string address, string fileType)
        {
            WebClient webClient = new WebClient();
            Directory.CreateDirectory(downloadDirectory);
            webClient.DownloadFile(address, $"{downloadDirectory}background." + fileType);
        }

        static void Main(string[] args)
        {
            Token myToken = GetToken();
            if(!String.IsNullOrEmpty(myToken.Access_token) && myToken.Expires_in > 0)
            {
                
                List<string> posts = GetPosts(myToken, true);

                if(posts.Count > 0)
                {
                    int postNumber = ChooseRandomPost(posts.Count);
                    string fileType = posts[postNumber].Substring(posts[postNumber].Length - 3);
                    DownloadImageFromPost(posts[postNumber], fileType);

                    if (inDebug)
                    {
                        DebugLogging.WriteToLog($"Number of Posts: {posts.Count}\nDownloaded Post: {posts[postNumber]}\nFile Type: {fileType}");
                    }
                    Wallpaper.SetDesktopWallpaper($"{downloadDirectory}background." + fileType, WallpaperStyle.Stretch);
                }
            }
        }
    }
}