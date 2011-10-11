// Copyright (C) 2011 AMEE UK Ltd. - http://www.amee.com
// Released as Open Source Software under the BSD 3-Clause license. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Xml;
using System.IO;
using System.Collections;

namespace AMEE
{
  public class Connection
  {

    private RestClient amee;

    private Connection()
    {
    }

    private static Connection m_instance = null;

    public static Connection Instance()
    {
      if (m_instance == null)
      {
        m_instance = new Connection();
      }
      return m_instance;
    }

    public void Connect(string embeddedResourcePath)
    {
        XmlDocument doc = new XmlDocument();
        XmlTextReader reader = new XmlTextReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourcePath));
        doc.Load(reader);
        Connect(
            doc.SelectNodes("/AMEE/Server").Item(0).InnerText,
            doc.SelectNodes("/AMEE/Username").Item(0).InnerText,
            doc.SelectNodes("/AMEE/Password").Item(0).InnerText);
    }

    public void Connect(string server, string username, string password)
    {
      amee = new RestClient();
      amee.BaseUrl = "https://" + server;
      amee.Authenticator = new HttpBasicAuthenticator(username, password);
    }

    public RestResponse DoRequest(RestRequest request)
    {
      request.AddHeader("Accept", "application/xml");
      return amee.Execute(request);
    }

    public static XmlNodeList Xpath(string xml, string xpath)
    {
      XmlDocument Doc = new XmlDocument();
      XmlNamespaceManager ns = new XmlNamespaceManager(Doc.NameTable);
      ns.AddNamespace("amee", "http://schemas.amee.cc/2.0");
      Doc.LoadXml(xml);
      return Doc.SelectNodes(xpath, ns);
    }

  }
}
