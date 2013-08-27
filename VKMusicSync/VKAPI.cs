using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading;


namespace VKMusicSync
{
    public class VKAPI
    {
        public String AppID, AccessToken, UserID, Scope;

        public VKAPI(String AppID, String Scope)
        {
            this.AppID = AppID;
            this.Scope = Scope;

        }

        public Boolean OAuth()
        {
            FBAuthentication fbAuthenticate = new FBAuthentication(this.AppID, Scope);
            fbAuthenticate.ShowDialog();
            if (fbAuthenticate.ExitCode == true)
            {
                this.AccessToken = fbAuthenticate.AccessToken;
                this.UserID = fbAuthenticate.UserID;
                return true;
            }
            else return false;
        }

        public Int32 Audio_getCount(Int32 oid)
        {
            StringBuilder link = new StringBuilder("https://api.vk.com/method/audio.getCount?");
            link.Append("oid=" + oid);
            link.Append("&access_token=" + this.AccessToken);
            var Uri = new Uri(link.ToString());
            var wReq = HttpWebRequest.Create(Uri);
            var wResp = wReq.GetResponse();
            var wStream = wResp.GetResponseStream();
            var wStreamReader = new System.IO.StreamReader(wStream);
            var wString = wStreamReader.ReadToEnd();
            JObject jResp = JObject.Parse(wString);
            return Convert.ToInt32(jResp["response"]);
        }

        public List<Audio> Audio_get_all(Int32 oid)
        {
            List<Audio> audios = new List<Audio>();
            Int32 count = Audio_getCount(oid);
            audios = Audio_get(oid: oid, count: count);
            return audios;
        }

        public String[] Users_get_name(Int32 uid)
        {
            String[] name = {"",""};
            StringBuilder link = new StringBuilder("https://api.vk.com/method/users.get?");
            link.Append("uids=" + uid);
            link.Append("&access_token=" + this.AccessToken);
            var Uri = new Uri(link.ToString());
            var wReq = HttpWebRequest.Create(Uri);
            var wResp = wReq.GetResponse();
            var wStream = wResp.GetResponseStream();
            var wStreamReader = new System.IO.StreamReader(wStream);
            var wString = wStreamReader.ReadToEnd();
            JObject jResp = JObject.Parse(wString);
            name[0] = jResp["response"][0]["first_name"].ToString();
            name[1] = jResp["response"][0]["last_name"].ToString();
            return name;
        }

        public List<Audio> Audio_get(Int32 oid = 0, Int32[] aids = null, Int32 offset = 0, Int32 count = 0)
        {
            List<Audio> audios = new List<Audio>();

            StringBuilder link = new StringBuilder("https://api.vk.com/method/audio.get?");
            if (oid != 0) link.Append("oid=" + oid +"&");
            else link.Append("aids=" + aids + "&");
            if (offset != 0) link.Append("offset=" + offset + "&");
            if (count != 0) link.Append("count=" + count + "&");
            link.Append("v=4.99&access_token=" + this.AccessToken);

            var Uri = new Uri(link.ToString());
            var wReq = HttpWebRequest.Create(Uri);
            var wResp = wReq.GetResponse();
            var wStream = wResp.GetResponseStream();
            var wStreamReader = new System.IO.StreamReader(wStream);
            var wString = wStreamReader.ReadToEnd();
            JObject jResp = JObject.Parse(wString);

            for (Int32 i = 1; i < jResp["response"].Count(); i++) {
                Int32 aid = Convert.ToInt32(jResp["response"][i]["aid"]);
                Int32 owner_id = Convert.ToInt32(jResp["response"][i]["owner_id"]);
                String artist = jResp["response"][i]["artist"].ToString();
                String title = jResp["response"][i]["title"].ToString();
                Int32 duration = Convert.ToInt32(jResp["response"][i]["duration"]);
                String url = jResp["response"][i]["url"].ToString();
                Int32 lyrics_id = Convert.ToInt32(jResp["response"][i]["lyrics_id"]);
                audios.Add(new Audio(aid, owner_id, artist, title, duration, url, lyrics_id));
            }
                return audios;
        }

        public List<String> Audio_getLyrics(String query, Int32 resultsCount = 10)
        {
            List<String> lyrics_list = new List<string>();

            if (resultsCount < 1) resultsCount = 1;
            if (resultsCount > 300) resultsCount = 300;

            var searchLink = "https://api.vk.com/method/audio.search?q=" + query + "&auto_complete=1&lyrics=1&sort=2&count=" + resultsCount + "&access_token=" + AccessToken;
            var searchUri = new Uri(searchLink);
            var wReq = HttpWebRequest.Create(searchUri);
            var wResp = wReq.GetResponse();
            var wStream = wResp.GetResponseStream();
            var wStreamReader = new System.IO.StreamReader(wStream);
            var wString = wStreamReader.ReadToEnd();
            JObject jResp = JObject.Parse(wString);
            JArray jArr = JArray.Parse(jResp["response"].ToString());
            Int32 count = Int32.Parse(jArr[0].ToString());
            if (count == 0) throw (new Exception("Nothing Found"));

            List<String> lyrics_ids = new List<String>();
            for (Int32 i = 1; i < jArr.Count; i++) lyrics_ids.Add(jArr[i]["lyrics_id"].ToString());

            foreach (String id in lyrics_ids)
            {
                searchLink = "https://api.vk.com/method/audio.getLyrics?lyrics_id=" + id + "&access_token=" + AccessToken;
                searchUri = new Uri(searchLink);
                wReq = HttpWebRequest.Create(searchUri);
                wResp = wReq.GetResponse();
                wStream = wResp.GetResponseStream();
                wStreamReader = new System.IO.StreamReader(wStream);
                wString = wStreamReader.ReadToEnd();
                jResp = JObject.Parse(wString);
                jResp = JObject.Parse(jResp["response"].ToString());
                String lyrics = jResp["text"].ToString();
                int numLines = lyrics.Split('\n').Length;
                if (numLines >= 7) lyrics_list.Add(jResp["text"].ToString());
                Thread.Sleep(333);
            }

            return lyrics_list;
        }

        public class Audio
        {
            public readonly Int32 aid;
            public readonly Int32 owner_id;
            public readonly String artist;
            public readonly String title;
            public readonly Int32 duration;
            public readonly String url;
            public readonly Int32 lyrics_id;
            public Audio(Int32 aid, Int32 owner_id, String artist, String title, Int32 duration, String url, Int32 lyrics_id)
            {
                this.aid = aid;
                this.owner_id = owner_id;
                this.artist = artist;
                this.title = title;
                this.duration = duration;
                this.url = url;
                this.lyrics_id = lyrics_id;
            }
        }
    }
}
