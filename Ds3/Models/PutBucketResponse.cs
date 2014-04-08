﻿using System.Net;

using Ds3.Runtime;

namespace Ds3.Models
{
    public class PutBucketResponse : Ds3Response
    {
        internal PutBucketResponse(IWebResponse response)
            : base(response)
        {
            HandleStatusCode(HttpStatusCode.OK);
        }
    }
}
