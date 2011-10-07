using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using RestSharp.Contrib;
using AMEE;

namespace AMEEMaterials
{
    class DataMapping
    {
        private static DataMapping m_dataMappingInstance;

        private Hashtable m_materials;

        private DataMapping()
        {
            m_materials = new Hashtable();
            // Load data from XML

            XmlDocument doc = new XmlDocument();
            XmlTextReader reader = new XmlTextReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AMEEMaterials.Resources.Materials.xml"));
            doc.Load(reader);
            XmlNodeList materials = doc.SelectNodes("//Material");
            foreach (XmlNode material in materials)
            {
                string internalName = material.SelectNodes("InternalName").Item(0).InnerText;
                AMEE.DataItem item = new AMEE.DataItem(material.SelectNodes("AMEE/Path").Item(0).InnerText);
                XmlNodeList drills = material.SelectNodes("AMEE/Drills/Drill");
                foreach (XmlNode drill in drills)
                {
                    item.AddDrill(drill.Attributes["name"].Value, drill.InnerText);
                }
                m_materials.Add(internalName, item);
            }
        }

        public static DataMapping Instance() {
            if (m_dataMappingInstance == null)
                m_dataMappingInstance = new DataMapping();
            return m_dataMappingInstance;
        }

        public DataItem Item(string material)
        {
            if (m_materials.ContainsKey(material))
                return (DataItem)m_materials[material];
            throw new Exception("Unknown material: " + material);
        }        
    }
}
