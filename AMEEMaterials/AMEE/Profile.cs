using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Xml;
using System.Collections;

namespace AMEE
{
    public class Profile
    {

        readonly string UID;

        public Profile(string uid)
        {
            UID = uid;
        }

        public Profile()
        {
            RestRequest request = new RestRequest(Method.POST);
            request.Resource = "/profiles";
            request.AddParameter("profile", "true");

            string body = Connection.Instance().DoRequest(request).Content;

            XmlNodeList nodes = Connection.Xpath(body, "//amee:Profile/@uid");
            if (nodes.Count > 0) {
                UID = nodes.Item(0).Value;
            }
        }

        public void Delete()
        {
            RestRequest request = new RestRequest(Method.DELETE);
            request.Resource = "/profiles/" + UID;
            string body = Connection.Instance().DoRequest(request).Content;
        }

        public ProfileItem CreateItem(DataItem item, Hashtable parameters, Hashtable options)
        {
            RestRequest request = new RestRequest(Method.POST);
            // url
            request.Resource = "/profiles/" + UID + item._path;
            // query params
            string query = "";
            foreach (DictionaryEntry d in options)
            {
                query += d.Key.ToString() + "=" + d.Value.ToString();
            }
            if (query != "")
                request.Resource += "?" + query;
            // body params
            foreach(DictionaryEntry d in parameters)
            {
                request.AddParameter(d.Key.ToString(), d.Value.ToString());
            }
            request.AddParameter("dataItemUid", item.UID);
            // DO IT
            return new ProfileItem(Connection.Instance().DoRequest(request).Content);
        }
    }
}
