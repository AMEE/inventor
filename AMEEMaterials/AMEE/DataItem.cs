// Copyright (C) 2011 AMEE UK Ltd. - http://www.amee.com
// Released as Open Source Software under the BSD 3-Clause license. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Xml;
using RestSharp.Contrib;

namespace AMEE
{
    public class DataItem
    {
        internal string _path;

        internal List<List<string>> drills = new List<List<string>>();

        private string _uid = null;

        public DataItem(string path)
        {
            _path = path;
        }

        public void AddDrill(string name, string value) {
            List<string> data = new List<string>();
            data.Add(name);
            data.Add(value);
            drills.Add(data);
        }

        public string UID
        {
            get 
            {
                if (_uid == null) {
                    // Do drill and get UID
                    RestRequest request = new RestRequest(Method.GET);
                    string drillString = "";
                    foreach (List<string> x in drills)
                    {
                        if (drillString.Length != 0)
                            drillString += '&';
                        drillString += HttpUtility.UrlEncode(x[0]);
                        drillString += '=';
                        drillString += HttpUtility.UrlEncode(x[1]);
                    }
                    request.Resource = "/data" + _path + "/drill?" + drillString;
                    RestResponse response = Connection.Instance().DoRequest(request);
                    XmlNodeList nodes = Connection.Xpath(response.Content, "//amee:Choices/amee:Choice/amee:Name/text()");
                    if (nodes.Count > 0)
                    {
                        _uid = nodes.Item(0).Value;
                    }
                    else
                    {
                        throw new Exception("Bad drill: " + _path + " " + drillString);
                    }
                }
                return _uid;
            }
        }

        public string DiscoverLink()
        {
            string link = "http://discover.amee.com/categories" + _path + "/data";
            foreach (List<string> x in drills)
            {
                link += "/" + HttpUtility.UrlEncode(x[1]);
            }
            return link;
        }

    }
}
