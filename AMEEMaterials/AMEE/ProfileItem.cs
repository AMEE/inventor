using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using RestSharp;
using System.Xml;

namespace AMEE
{
    public class ProfileItem
    {

        public class Amount
        {
            public string Type;
            public string Value;
            public string Unit;
            public Amount(string type, string value, string unit)
            {
                Type = type;
                Value = value;
                Unit = unit;
            }
        }

        public List<Amount> Amounts = new List<Amount>();

        public ProfileItem(string xml)
        {
            XmlNodeList nodes = Connection.Xpath(xml, "//amee:ProfileItem/amee:Amounts/amee:Amount");
            foreach (XmlNode x in nodes)
            {
                Amounts.Add(new Amount(x.Attributes["type"].Value, x.InnerText, x.Attributes["unit"].Value));
            }
        }

    }
}
