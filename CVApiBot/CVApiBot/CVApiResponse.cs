using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVApiBot
{
    public class CVApiResponse
    {
        public description description;
        public string requestId;
        public metadata metadata;
    }

    public class description
    {
        public string[] tags;
        public captions[] captions;
    }

    public class captions
    {
        public string text;
        public float confidence;
    }

    public class metadata
    {
        public int width;
        public int height;
        public string format;
    }
}