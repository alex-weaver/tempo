using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickrSample
{
    public class Photo
    {
        public readonly string farmId;
        public readonly string serverId;
        public readonly string secret;
        public readonly string id;

        public Photo(string farmId, string serverId, string secret, string id)
        {
            this.farmId = farmId;
            this.serverId = serverId;
            this.secret = secret;
            this.id = id;
        }
    }
}
