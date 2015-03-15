using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tempo;

namespace FlickrSample
{
    public static class FlickrSearch
    {
        public static void Search(string key, string query, int perPage, int page, Action<IEnumerable<Photo>> callback)
        {
            var queryScope = CurrentThread.AnyCurrentScope();

            var url = string.Format("http://flickr.com/services/rest/?method=flickr.photos.search&api_key={0}&text={1}&sort=relevance&per_page={2}&page={3}",
                key,
                UriEscape(query),
                perPage,
                page);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.BeginGetResponse(result =>
            {
                var response = request.EndGetResponse(result);
                var doc = XDocument.Load(response.GetResponseStream());

                var photos =
                    doc.Descendants(@"photo")
                    .Select(x => new Photo(
                        (string)x.Attribute("farm"),
                        (string)x.Attribute("server"),
                        (string)x.Attribute("secret"),
                        (string)x.Attribute("id")));

                queryScope.RunSequential(() =>
                    {
                        callback(photos.ToList());
                    });
            }, null);
        }

        private static string UriEscape(string unescaped)
        {
            // This works for most strings that are likely to be placed in URLs. We cannot use
            // System.Net.WebUtility.UrlEncode, because that function is only in .NET 4.5+.
            // We also cannot use System.Web.HttpUtility.UrlEncode, because that requires the full framework instead of the client profile.
            return Uri.EscapeDataString(unescaped).Replace("%20", "+");
        }


        public static void GetPhotoThumb(Photo photo, Action<byte[]> callback)
        {
            var callingScope = CurrentThread.AnyCurrentScope();

            var url = string.Format("https://farm{0}.staticflickr.com/{1}/{2}_{3}_q.jpg",
                photo.farmId,
                photo.serverId,
                photo.id,
                photo.secret);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.BeginGetResponse(result =>
            {
                try
                {
                    var response = request.EndGetResponse(result);

                    using (var stream = response.GetResponseStream())
                    {
                        using (var copy = new MemoryStream())
                        {
                            stream.CopyTo(copy);
                            var data = copy.ToArray();

                            callingScope.RunSequential(() =>
                                {
                                    callback(data);
                                });
                        }
                    }

                    response.Close();
                }
                catch
                {
                    // Ignore exceptions - callback is never invoked on error
                }

            }, null);
        }
    }
}
