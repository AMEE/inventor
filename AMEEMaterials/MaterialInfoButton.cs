// Copyright (C) 2011 AMEE UK Ltd. - http://www.amee.com
// Released as Open Source Software under the BSD 3-Clause license. See LICENSE.txt for details.

using System;
using System.Windows.Forms;
using System.Drawing;
using Inventor;
using AMEE;
using System.Diagnostics;

namespace AMEEMaterials
{

    internal class MaterialInfoButton : Button
    {
        #region "Methods"

        public MaterialInfoButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, Icon standardIcon, Icon largeIcon, ButtonDisplayEnum buttonDisplayType)
            : base(displayName, internalName, commandType, clientId, description, tooltip, standardIcon, largeIcon, buttonDisplayType)
        {

        }

        public MaterialInfoButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, ButtonDisplayEnum buttonDisplayType)
            : base(displayName, internalName, commandType, clientId, description, tooltip, buttonDisplayType)
        {

        }

        override protected void ButtonDefinition_OnExecute(NameValueMap context)
        {
            try
            {
                PartDocument doc = (PartDocument)AddInServer.m_inventorApplication.ActiveDocument;
                DataItem material = DataMapping.Instance().Item(doc.ComponentDefinition.Material.InternalName.Trim());
                Process.Start(material.DiscoverLink());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion
    }
}
