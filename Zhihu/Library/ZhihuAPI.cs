using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO.IsolatedStorage;

namespace ZhihuAPI
{
    public class ZhihuAPI
    {
        protected static string accessToken;
        protected static string tokenType    = " ";
        protected static int expiresIn       = 0;
        protected static string refreshToken = " ";
        protected static IsolatedStorageSettings settings;

        public ZhihuAPI()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("accessToken"))
                accessToken = (string)settings["accessToken"];
            if (settings.Contains("tokenType"))
                tokenType = (string)settings["tokenType"];
            if (settings.Contains("expiresIn"))
                expiresIn = (int)settings["expiresIn"];
            if (settings.Contains("refreshToken"))
                refreshToken = (string)settings["refreshToken"];
        }

        public class Author
        {
            public string headline  = " ";
            public string avatarUrl = " ";
            public string name      = " ";
            public string url       = " ";
            public int gender       = 0;
            public string type      = " ";
            public string id        = " ";

            public void Get(JObject author)
            {
                if (author["headline"] != null)
                    this.headline = (string)author["headline"];
                if (author["avatar_url"] != null)
                    this.avatarUrl = (string)author["avatar_url"];
                if (author["name"] != null)
                    this.name = (string)author["name"];
                if (author["url"] != null)
                    this.url = (string)author["url"];
                if (author["gender"] != null)
                    this.gender = (int)author["gender"];
                if (author["type"] != null)
                    this.type = (string)author["type"];
                if (author["id"] != null)
                    this.id = (string)author["id"];
            }
        }

        public class Question
        {
            public string url   = " ";
            public string type  = " ";
            public int id       = 0;
            public string title = " ";

            public void Get(JObject question)
            {
                this.url   = (string)question["url"];
                this.type  = (string)question["type"];
                this.id    = (int)question["id"];
                this.title = (string)question["title"];
            }
        }

        // zhihu api request url
        protected class ZhihuRequestUrl
        {
            // 登陆
            public static string Login
            {
                get { return "https://oauth.zhihu.com/token"; }
            }

            // 个人资料
            public static string PeopleSelf
            {
                get { return "https://api.zhihu.com/people/self"; }
            }

            // 最新动态
            public static string Feeds
            {
                get { return "https://api.zhihu.com/feeds?hash=" + new Random().Next(); }
            }

            // 热门问答
            public static string ExploreAnswers
            {
                get { return "https://api.zhihu.com/explore/answers?hash=" + new Random().Next(); }
            }

            // 热门收藏
            public static string ExploreCollections
            {
                get { return "https://api.zhihu.com/explore/collections"; }
            }

            // 随便看看
            public static string ExploreRandom
            {
                get { return "https://api.zhihu.com/explore/random_question"; }
            }

            // 回答
            public static string Answers
            {
                get { return "https://api.zhihu.com/answers/"; }
            }

            // unread messages count
            public static string UnreadCount
            {
                get { return "https://api.zhihu.com/unread_count"; }
            }

            // notifications content
            public static string NotificationsContent
            {
                get { return "https://api.zhihu.com/notifications/content"; }
            }

            // about me
            public static string NotificationsLove
            {
                get { return "https://api.zhihu.com/notifications/love"; }
            }

            // inbox
            public static string Inbox
            {
                get { return "https://api.zhihu.com/inbox"; }
            }

            // search
            public static string Search
            {
                get { return "https://api.zhihu.com/search"; }
            }
        }

        // auth strings
        protected class ZhihuRequestAuth
        {
            public static string clientId
            {
                get { return "4bc899f2695f417eb733f3d21a7be0"; }
            }

            public static string clientKey
            {
                get { return "d179a1e11f2347a48c7f928d354621"; }
            }

            public static string authBasic
            {
                get { return "Basic NGJjODk5ZjI2OTVmNDE3ZWI3MzNmM2QyMWE3YmUwOmQxNzlhMWUxMWYyMzQ3YTQ4YzdmOTI4ZDM1NDYyMQ=="; }
            }

            public static string authBearer
            {
                get { return "Bearer "; }
            }
        }
        
        // async post request
        protected static async Task<string> PostAsync(string RequestUrl, string Context)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(RequestUrl);
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.UserAgent = "ZhihuAppIrisRelease/96 (iPhone; iOS 6.1.2; Scale/2.00)";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                httpWebRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                httpWebRequest.Headers[HttpRequestHeader.Connection] = "Keep-Alive";
                httpWebRequest.Headers[HttpRequestHeader.Authorization] = ZhihuRequestAuth.authBasic;
                httpWebRequest.Headers["XAPIVERSION"] = "3.0";
                httpWebRequest.Headers["XAPPVERSION"] = "2.1.0";
                using (Stream stream = await httpWebRequest.GetRequestStreamAsync())
                {
                    byte[] entryBytes = Encoding.UTF8.GetBytes(Context);
                    stream.Write(entryBytes, 0, entryBytes.Length);
                    stream.Close();
                }

                WebResponse response = await httpWebRequest.GetResponseAsync();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                return streamReader.ReadToEnd();
            }
            catch
            {
                throw;
            }
        }

        // async get request
        protected static async Task<string> GetAsync(string RequestUrl)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(RequestUrl, UriKind.Absolute));
                httpWebRequest.Method = "GET";
                httpWebRequest.UserAgent = "ZhihuAppIrisRelease/96 (iPhone; iOS 6.1.2; Scale/2.00)";
                httpWebRequest.Headers[HttpRequestHeader.Pragma] = "no-cache";
                httpWebRequest.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                httpWebRequest.Headers[HttpRequestHeader.Authorization] = ZhihuRequestAuth.authBearer + accessToken;
                WebResponse response = await httpWebRequest.GetResponseAsync();
                Stream streamResult = response.GetResponseStream();
                StreamReader sr = new StreamReader(streamResult, Encoding.UTF8);
                string returnValue = sr.ReadToEnd();
                return DeleteLines(UnicodeToGB(returnValue));
            }
            catch
            {
                throw;
            }
        }

        // delete double extra lines
        protected static string DeleteLines(string text)
        {
            int index = text.IndexOf('\\');
            while (index != -1)
            {
                if (text[index + 1] == '/')
                {
                    text = text.Remove(index, 1);
                }
                index = text.IndexOf('\\', index + 1);
            }
            return text;
        }

        // unicode to GB encoding
        public static string UnicodeToGB(string text)
        {
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(text, "\\\\u([\\w]{4})");
            if (mc != null && mc.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match m2 in mc)
                {
                    string v = m2.Value;
                    string word = v.Substring(2);
                    byte[] codes = new byte[2];
                    int code = Convert.ToInt32(word.Substring(0, 2), 16);
                    int code2 = Convert.ToInt32(word.Substring(2), 16);
                    codes[0] = (byte)code2;
                    codes[1] = (byte)code;
                    text = text.Replace(v, Encoding.Unicode.GetString(codes, 0, codes.Length));
                }
            }
            return text;
        }
    }

    // 账号
    public class Account : ZhihuAPI
    {
        public async Task<bool> Login(string username, string password)
        {
            try
            {
                string requestContext = "client_id=" + ZhihuRequestAuth.clientId + "&client_key=" + ZhihuRequestAuth.clientKey
                    + "&email=" + username + "&grant_type=password&password=" + password;
                string result = await PostAsync(ZhihuRequestUrl.Login, requestContext);
                JObject json = JObject.Parse(result);
                tokenType = (string)json["token_type"];
                accessToken = (string)json["access_token"];
                expiresIn = (int)json["expires_in"];
                refreshToken = (string)json["refresh_token"];

                if (settings.Contains("tokenType"))
                    settings.Remove("tokenType");
                settings.Add("tokenType", tokenType);
                if (settings.Contains("accessToken"))
                    settings.Remove("accessToken");
                settings.Add("accessToken", accessToken);
                if (settings.Contains("expiresIn"))
                    settings.Remove("expiresIn");
                settings.Add("expiresIn", expiresIn);
                if (settings.Contains("refreshToken"))
                    settings.Remove("refreshToken");
                settings.Add("refreshToken", refreshToken);
                settings.Save();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool isLogined()
        {
            if (accessToken != null)
                return true;
            else
                return false;
        }

        public void Logout()
        {
            if (settings.Contains("accessToken"))
                settings.Remove("accessToken");
            accessToken = "";
            if (settings.Contains("tokenType"))
                settings.Remove("tokenType");
            tokenType = "";
            if (settings.Contains("expiresIn"))
                settings.Remove("expiresIn");
            expiresIn = 0;
            if (settings.Contains("refreshToken"))
                settings.Remove("refreshToken");
            settings.Save();
            refreshToken = "";
        }
    }

    // 个人资料
    public class Profile : ZhihuAPI
    {
        public class PeopleSelf
        {
            public class Education
            {
                public string introduction = " ";
                public string avatarUrl    = " ";
                public string name         = " ";
                public string experience   = " ";
                public string url          = " ";
                public string excerpt      = " ";
                public string type         = " ";
                public string id           = " ";

                public void Get(JObject education)
                {
                    if (education["introduction"] != null)
                        introduction = (string)education["introduction"];
                    if (education["avatarUrl"] != null)
                        avatarUrl = (string)education["avatarUrl"];
                    if (education["name"] != null)
                        name = (string)education["name"];
                    if (education["experience"] != null)
                        experience = (string)education["experience"];
                    if (education["url"] != null)
                        url = (string)education["url"];
                    if (education["excerpt"] != null)
                        excerpt = (string)education["excerpt"];
                    if (education["type"] != null)
                        type = (string)education["type"];
                    if (education["id"] != null)
                        id = (string)education["id"];
                }
            }

            public class Location
            {
                public string name       = " ";
                public string url        = " ";
                public string excerpt    = " ";
                public string experience = " ";
                public string avatarUrl  = " ";
                public string type       = " ";
                public string id         = " ";

                public void Get(JObject location)
                {
                    if (location["name"] != null)
                        name = (string)location["name"];
                    if (location["url"] != null)
                        url = (string)location["url"];
                    if (location["excerpt"] != null)
                        excerpt = (string)location["excerpt"];
                    if (location["experience"] != null)
                        experience = (string)location["experience"];
                    if (location["avatarUrl"] != null)
                        avatarUrl = (string)location["avatarUrl"];
                    if (location["type"] != null)
                        type = (string)location["type"];
                    if (location["id"] != null)
                        id = (string)location["id"];

                }
            }

            public class Business
            {
                public string name      = " ";
                public string url       = " ";
                public string excerpt   = " ";
                public string avatarUrl = " ";
                public string type      = " ";
                public string id        = " ";

                public void Get(JObject business)
                {
                    if (business["name"] != null)
                        name = (string)business["name"];
                    if (business["url"] != null)
                        url = (string)business["url"];
                    if (business["excerpt"] != null)
                        excerpt = (string)business["excerpt"];
                    if (business["avatarUrl"] != null)
                        avatarUrl = (string)business["avatarUrl"];
                    if (business["type"] != null)
                        type = (string)business["type"];
                    if (business["id"] != null)
                        id = (string)business["id"];
                }
            }

            public int followingCollectionCount = 0;
            public int sharedCount              = 0;
            public int askAboutCount            = 0;
            List<Education> education = new List<Education>();
            public string id                 = " ";
            public int favoriteCount         = 0;
            public int voteupCount           = 0;
            public int followingColumnsCount = 0;
            public int followingCount        = 0;
            public string headline           = " ";
            public int favoritedCount        = 0;
            List<Location> location = new List<Location>();
            public int followerCount       = 0;
            public string type             = " ";
            public string email            = " ";
            public int draftCount          = 0;
            public int followingTopicCount = 0;
            public string description      = " ";
            Business business = new Business();
            public int columnsCount           = 0;
            public bool isActive              = false;
            public int answerCount            = 0;
            public int questionCount          = 0;
            public string name                = " ";
            public string url                 = " ";
            public int gender                 = 0;
            public string avatarUrl           = " ";
            public int followingQuestionCount = 0;
            public int thankedCount           = 0;

            public void Get(JObject peopleSelf)
            {
                followingCollectionCount = (int)peopleSelf["following_collection_count"];
                sharedCount              = (int)peopleSelf["shared_count"];
                askAboutCount            = (int)peopleSelf["ask_about_count"];
                foreach (JObject item in (peopleSelf["education"] as JArray))
                {
                    Education education = new Education();
                    education.Get(item);
                    this.education.Add(education);
                }
                id                    = (string)peopleSelf["id"];
                favoriteCount         = (int)peopleSelf["favorite_count"];
                voteupCount           = (int)peopleSelf["voteup_count"];
                followingColumnsCount = (int)peopleSelf["following_columns_count"];
                followingCount        = (int)peopleSelf["following_count"];
                headline              = (string)peopleSelf["headline"];
                favoritedCount        = (int)peopleSelf["favoritedCount"];
                foreach (JObject item in (peopleSelf["location"] as JArray))
                {
                    Location location = new Location();
                    location.Get(item);
                    this.location.Add(location);
                }
                followerCount       = (int)peopleSelf["follower_count"];
                type                = (string)peopleSelf["type"];
                email               = (string)peopleSelf["email"];
                draftCount          = (int)peopleSelf["draft_count"];
                followingTopicCount = (int)peopleSelf["following_topic_count"];
                description         = (string)peopleSelf["description"];
                business.Get((JObject)peopleSelf["business"]);
                columnsCount           = (int)peopleSelf["columns_count"];
                isActive               = (bool)peopleSelf["is_active"];
                answerCount            = (int)peopleSelf["answer_count"];
                questionCount          = (int)peopleSelf["question_count"];
                name                   = (string)peopleSelf["name"];
                url                    = (string)peopleSelf["url"];
                gender                 = (int)peopleSelf["gender"];
                avatarUrl              = (string)peopleSelf["avatar_url"];
                followingQuestionCount = (int)peopleSelf["following_question_count"];
                thankedCount           = (int)peopleSelf["thanked_count"];
            }
        }

        public PeopleSelf peopleSelf = new PeopleSelf();

        public async Task<bool> GetPeopleSelf()
        {
            try
            {
                string result = await GetAsync(ZhihuRequestUrl.PeopleSelf);
                JObject json = JObject.Parse(result);
                Debug.WriteLine(json.ToString());
                peopleSelf.Get(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }

    // 热门问答
    public class HotAnswers : ZhihuAPI
    {
        public class Answer
        {
            public Author author     = new Author();
            public Question question = new Question();

            public string url       = " ";
            public string excerpt   = " ";
            public long updatedTime  = 0;
            public int commentCount = 0;
            public string type      = " ";
            public int id           = 0;
            public int voteupCount  = 0;
        }

        private static string nextUrl;
        private static string previousUrl;

        public static int pageNumber;
        public List<Answer> hotAnswers = new List<Answer>();

        public async Task<int> GetFirstPage()
        {
            try
            {
                hotAnswers.Clear();

                string result = await GetAsync(ZhihuRequestUrl.ExploreAnswers);
                JObject json = JObject.Parse(result);

                nextUrl = (string)json["paging"]["next"];
                previousUrl = (string)json["paging"]["previous"];

                foreach (JObject item in (json["data"] as JArray))
                {
                    Answer answer = new Answer();


                    JObject author = item["author"] as JObject;

                    answer.author.Get(author);

                    JObject question = item["question"] as JObject;
                    answer.question.url = (string)question["url"];
                    answer.question.type = (string)question["type"];
                    answer.question.id = (int)question["id"];
                    answer.question.title = (string)question["title"];

                    answer.url = (string)item["url"];
                    answer.excerpt = (string)item["excerpt"];
                    answer.updatedTime = (long)item["updated_time"];
                    answer.commentCount = (int)item["comment_count"];
                    answer.type = (string)item["type"];
                    answer.id = (int)item["id"];
                    answer.voteupCount = (int)item["voteup_count"];

                    hotAnswers.Add(answer);
                }
                pageNumber = 0;
                return hotAnswers.Count;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetNextPage()
        {
            try
            {
                hotAnswers.Clear();

                string result = await GetAsync(nextUrl + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);

                nextUrl = (string)json["paging"]["next"];
                previousUrl = (string)json["paging"]["previous"];

                foreach (JObject item in (json["data"] as JArray))
                {
                    Answer answer = new Answer();

                    JObject author = item["author"] as JObject;
                    if (author["headline"] != null)
                        answer.author.headline = (string)author["headline"];
                    if (author["avatar_url"] != null)
                        answer.author.avatarUrl = (string)author["avatar_url"];
                    if (author["name"] != null)
                        answer.author.name = (string)author["name"];
                    if (author["url"] != null)
                        answer.author.url = (string)author["url"];
                    if (author["gender"] != null)
                        answer.author.gender = (int)author["gender"];
                    if (author["type"] != null)
                        answer.author.type = (string)author["type"];
                    if (author["id"] != null)
                        answer.author.id = (string)author["id"];

                    JObject question = item["question"] as JObject;
                    answer.question.url = (string)question["url"];
                    answer.question.type = (string)question["type"];
                    answer.question.id = (int)question["id"];
                    answer.question.title = (string)question["title"];

                    answer.url = (string)item["url"];
                    answer.excerpt = (string)item["excerpt"];
                    answer.updatedTime = (long)item["updated_time"];
                    answer.commentCount = (int)item["comment_count"];
                    answer.type = (string)item["type"];
                    answer.id = (int)item["id"];
                    answer.voteupCount = (int)item["voteup_count"];

                    hotAnswers.Add(answer);
                }
                pageNumber++;
                return hotAnswers.Count;
            }
            catch
            {
                throw;
            }
        }
    }

    // 回答
    public class Answers : ZhihuAPI
    {

        public class AnswerDetail
        {
            public Author author = new Author();
            public Question question = new Question();
            public string url       = " ";
            public string excerpt   = " ";
            public long updatedTime  = 0;
            public string content   = " ";
            public int commentCount = 0;
            public string type      = " ";
            public int voteupCount  = 0;
        }

        public AnswerDetail answerDetail = new AnswerDetail();

        public async Task<bool> GetAnswerDetail(string url)
        {
            try
            {
                string result = await GetAsync(url);
                JObject json = JObject.Parse(result);

                JObject author = json["author"] as JObject;
                if (author["headline"] != null)
                    answerDetail.author.headline = (string)author["headline"];
                if (author["avatar_url"] != null)
                    answerDetail.author.avatarUrl = (string)author["avatar_url"];
                if (author["name"] != null)
                    answerDetail.author.name = (string)author["name"];
                if (author["url"] != null)
                    answerDetail.author.url = (string)author["url"];
                if (author["gender"] != null)
                    answerDetail.author.gender = (int)author["gender"];
                if (author["type"] != null)
                    answerDetail.author.type = (string)author["type"];
                if (author["id"] != null)
                    answerDetail.author.id = (string)author["id"];

                JObject question = json["question"] as JObject;
                answerDetail.question.url = (string)question["url"];
                answerDetail.question.type = (string)question["type"];
                answerDetail.question.id = (int)question["id"];
                answerDetail.question.title = (string)question["title"];

                answerDetail.url = (string)json["url"];
                answerDetail.excerpt = (string)json["excerpt"];
                answerDetail.updatedTime = (long)json["updated_time"];
                answerDetail.content = (string)json["content"];
                answerDetail.commentCount = (int)json["comment_count"];
                answerDetail.type = (string)json["type"];
                answerDetail.voteupCount = (int)json["voteup_count"];

                return true;
            }
            catch
            {
                throw;
            }
        }
    }

    // 问题
    public class Question : ZhihuAPI
    {
        public class QuestionDetail
        {
            public Author author = new Author();
            public string status     = " ";
            public string title      = " ";
            public string url        = " ";
            public string excerpt    = " ";
            public string detail     = " ";
            public int answerCount   = 0;
            public long updatedTime  = 0;
            public string to         = " ";
            public int commentCount  = 0;
            public int followerCount = 0;
            public string type       = " ";
            public int id            = 0;

            public void Get(JObject question)
            {
                author.Get(question["author"] as JObject);

                this.title         = (string)question["title"];
                this.url           = (string)question["url"];
                this.excerpt       = (string)question["excerpt"];
                this.detail        = (string)question["detail"];
                this.answerCount   = (int)question["answer_count"];
                this.updatedTime   = (long)question["updated_time"];
                this.commentCount  = (int)question["comment_count"];
                this.followerCount = (int)question["follower_count"];
                this.type          = (string)question["type"];
                this.id            = (int)question["id"];
            }
        }

        QuestionDetail questionDetail = new QuestionDetail();

        public async Task<bool> GetQuestionDetail(string url)
        {
            try
            {
                string result = await GetAsync(url);
                JObject json = JObject.Parse(result);
                questionDetail.Get(json);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetAnswersOfQuestion()
        {
            try
            {
                string result = await GetAsync("");
                JObject json = JObject.Parse(result);

                return 1;
            }
            catch
            {
                throw;
            }
        }

    }

    // 最新动态
    public class Timeline : ZhihuAPI
    {
        public class Feed
        {
            public class Target
            {
                public Author author     = new Author();
                public string url        = " ";
                public Question question = new Question();
                public string excerpt    = " ";
                public long updatedTime   = 0;
                public int commentCount  = 0;
                public string type       = " ";
                public int id            = 0;
                public string title      = " ";
                public int voteupCount   = 0;

                public void Get(JObject target)
                {
                    if (target["author"] != null)
                        this.author.Get(target["author"] as JObject);
                    if (target["url"] != null)
                        this.url = (string)target["url"];
                    if (target["question"] != null)
                        this.question.Get(target["question"] as JObject);
                    if (target["excerpt"] != null)
                        this.excerpt = (string)target["excerpt"];
                    if (target["updated_time"] != null)
                        this.updatedTime = (long)target["updated_time"];
                    if (target["commentCount"] != null)
                        this.commentCount = (int)target["comment_count"];
                    if (target["type"] != null)
                        this.type = (string)target["type"];
                    if (target["id"] != null)
                        this.id = (int)target["id"];
                    if (target["title"] != null)
                        this.title = (string)target["title"];
                    if (target["voteup_count"] != null)
                        this.voteupCount = (int)target["voteup_count"];
                }
            }

            public int count       = 0;
            public Target target   = new Target();
            public long updatedTime = 0;
            public string verb     = " ";
            public List<Author> actors = new List<Author>();
            public string type     = " ";
            public string id       = " ";

            public void Get(JObject feed)
            {
                this.count = (int)feed["count"];
                this.target.Get(feed["target"] as JObject);
                this.updatedTime = (long)feed["updated_time"];
                this.verb        = (string)feed["verb"];
                foreach (JObject item in (feed["actors"] as JArray))
                {
                    Author actor = new Author();
                    actor.Get(item);
                    actors.Add(actor);
                }
                this.type = (string)feed["type"];
                this.id  = (string)feed["id"];
            }
        }


        private static string previousUrl;
        private static string nextUrl;

        public List<Feed> timeline = new List<Feed>();

        public async Task<int> GetFirstPage()
        {
            try
            {
                timeline.Clear();

                string result = await GetAsync(ZhihuRequestUrl.Feeds);
                JObject json = JObject.Parse(result);

                previousUrl = (string)json["paging"]["previous"];
                nextUrl = (string)json["paging"]["next"];

                foreach (JObject item in (json["data"] as JArray))
                {
                    Feed feed = new Feed();
                    feed.Get(item);
                    timeline.Add(feed);
                }

                return timeline.Count;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetNextPage()
        {
            try
            {
                timeline.Clear();

                string result = await GetAsync(nextUrl + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);

                previousUrl = (string)json["paging"]["previous"];
                nextUrl = (string)json["paging"]["next"];

                foreach (JObject item in (json["data"] as JArray))
                {
                    Feed feed = new Feed();
                    feed.Get(item);
                    timeline.Add(feed);
                }

                return timeline.Count;
            }
            catch
            {
                throw;
            }
        }
    }

    // 随便看看
    public class RandomQuestion : ZhihuAPI
    {

    }

    // 搜索
    public class Search : ZhihuAPI
    {
        public void SearchAll(string keyword)
        {
            string url = ZhihuRequestUrl.Search + "?t=all&q=" + keyword;
            DoSearch(url);
        }

        public void SearchTopic(string keyword)
        {
            string url = ZhihuRequestUrl.Search + "?t=topic&q=" + keyword;
            DoSearch(url);
        }

        public void SearchQuestion(string keyword)
        {
            string url = ZhihuRequestUrl.Search + "?t=question&q=" + keyword;
            DoSearch(url);
        }

        public void SearchPeople(string keyword)
        {
            string url = ZhihuRequestUrl.Search + "?t=people&q=" + keyword;
            DoSearch(url);
        }

        private async void DoSearch(string Url)
        {
            string result = await GetAsync(Url);
            JObject json = JObject.Parse(result);

            Debug.WriteLine(json.ToString());
        }

        public void GetNextPage()
        {

        }
    }
    
    // 评论
    public class Comments : ZhihuAPI
    {
        public class Comment
        {
            public bool allowReply;
            public bool isAuthor;
            public Author author = new Author();
            public string url;
            public bool ancestor;
            public bool isParentAuthor;
            public string content;
            public bool allowVote;
            public int voteCount;
            public bool allowDelete;
            public long createdTime;
            public bool voting;
            public string type;
            public int id;

            public void Get(JObject comment)
            {
                allowReply     = (bool)comment["allow_reply"];
                isAuthor       = (bool)comment["is_author"];
                author.Get(comment["author"] as JObject);
                url            = (string)comment["url"];
                ancestor       = (bool)comment["ancestor"];
                isParentAuthor = (bool)comment["is_parent_author"];
                content        = (string)comment["content"];
                allowVote      = (bool)comment["allow_vote"];
                voteCount      = (int)comment["vote_count"];
                allowDelete    = (bool)comment["allow_delete"];
                createdTime    = (long)comment["created_time"];
                voting         = (bool)comment["voting"];
                type           = (string)comment["type"];
                id             = (int)comment["id"];
            }
        }


        public List<Comment> comment = new List<Comment>();

        private string url;

        private static string nextUrl;
        private static string previousUrl;

        public Comments(string url)
        {
            this.url = url + "/comments";
        }

        public async Task<int> GetFirstPage()
        {
            try
            {
                comment.Clear();

                string result = await GetAsync(url);
                JObject json = JObject.Parse(result);

                nextUrl     = (string)json["paging"]["next"];
                previousUrl = (string)json["paging"]["previous"];

                foreach (JObject item in (json["data"] as JArray))
                {
                    Comment cmt = new Comment();
                    cmt.Get(item);
                    comment.Add(cmt);
                }
                return comment.Count;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetNextPage()
        {
            try
            {
                comment.Clear();

                string result = await GetAsync(nextUrl);
                JObject json = JObject.Parse(result);

                nextUrl = (string)json["paging"]["next"];
                previousUrl = (string)json["paging"]["previous"];

                foreach (JObject item in (json["data"] as JArray))
                {
                    Comment cmt = new Comment();
                    cmt.Get(item);
                    comment.Add(cmt);
                }
                return comment.Count;
            }
            catch
            {
                throw;
            }
        }
    }

    public class test : ZhihuAPI
    {
        public async void DoTest()
        {
            try
            {
                string result = await GetAsync("https://api.zhihu.com/answers/voteup?id=19113274");
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

    public class HTMLOutput
    {
        private string html;

        public string OutHtml
        {
            get
            {
                return html;
            }
        }

        public HTMLOutput(string html)
        {
            this.html = ConvertExtendedASCII(html);
            this.html = "<html>" + 
                            "<head>" + 
                                "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>" + 
                                "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1.0,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no\"/>" +
                                "<link rel=\"stylesheet\" href=\"style.css\" type=\"text/css\"/>" +
                            "</head>" +
                            "<body>" +
                                html + "<br><br><br>" +
                            "</body>" + 
                        "</html>";
        }

        public void SetClickToLoadImg()
        {
        }

        public static string ConvertExtendedASCII(string html)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in html)
            {
                if (Convert.ToInt32(c) > 127)
                {
                    sb.Append("&#" + Convert.ToInt32(c) + ";");
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
/*
        // convert html to ascii encoding
        public static string ConvertExtendedASCII(string HTML)
        {
            StringBuilder str = new StringBuilder();
            char c;
            for (int i = 0; i < HTML.Length; i++)
            {
                c = HTML[i];
                if (Convert.ToInt32(c) > 127)
                {
                    str.Append("&#" + Convert.ToInt32(c) + ";");
                }
                else
                {
                    str.Append(c);
                }
            }
            return str.ToString();
        }
 * */
    }
}
