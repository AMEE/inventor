// Copyright (C) 2011 AMEE UK Ltd. - http://www.amee.com
// Released as Open Source Software under the BSD 3-Clause license. See LICENSE.txt for details.

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
