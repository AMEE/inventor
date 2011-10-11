// Copyright (C) 2011 AMEE UK Ltd. - http://www.amee.com
// Released as Open Source Software under the BSD 3-Clause license. See LICENSE.txt for details.

using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using AMEE;
using System.Collections;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using AMEEMaterials;
using System.Collections.Generic;

namespace AMEEMaterials
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("94c1fe0f-61c6-4777-9d3e-bac595f91141")]
    public class AddInServer : Inventor.ApplicationAddInServer
    {

        // Inventor application object.
        public static Inventor.Application m_inventorApplication;

        private AMEE.Profile m_AMEEProfile;

        private MaterialInfoButton m_materialInfoButton;
        private RibbonPanel m_ribbonPanel;

        private CarbonSummaryForm m_summaryForm;

        // Assign window owners
        class InventorWindow : IWin32Window
        {
            public IntPtr Handle
            {
                get { return System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle; }
            }
        }

        public AddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.

            // Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application;

            // Connect to AMEE and make a profile
            AMEE.Connection.Instance().Connect("AMEEMaterials.Resources.AMEE.xml");
            m_AMEEProfile = new AMEE.Profile();
            try
            {
                ConfigureButtons();
                m_summaryForm = new CarbonSummaryForm();
                if (firstTime == true)
                {
                    ConfigureRibbon();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            m_inventorApplication.ApplicationEvents.OnOpenDocument += new ApplicationEventsSink_OnOpenDocumentEventHandler(ApplicationEvents_OnOpenDocument);
            m_inventorApplication.ApplicationEvents.OnDocumentChange += new ApplicationEventsSink_OnDocumentChangeEventHandler(ApplicationEvents_OnDocumentChange);

        }

        void ApplicationEvents_OnOpenDocument(_Document DocumentObject, string FullDocumentName, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            try
            {
                if (BeforeOrAfter == EventTimingEnum.kAfter)
                    UpdatePartData();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                HandlingCode = HandlingCodeEnum.kEventNotHandled;
            }
        }

        void ApplicationEvents_OnDocumentChange(_Document DocumentObject, EventTimingEnum BeforeOrAfter, CommandTypesEnum ReasonsForChange, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            try
            {
                if (BeforeOrAfter == EventTimingEnum.kAfter)
                    UpdatePartData();
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                HandlingCode = HandlingCodeEnum.kEventNotHandled;
            }
        }

        void UpdatePartData()
        {

            PartDocument doc = (PartDocument)m_inventorApplication.ActiveDocument;

            // calculate mass
            string material = doc.ComponentDefinition.Material.InternalName.Trim();
            double mass = doc.ComponentDefinition.MassProperties.Mass;

            if (mass > 0)
            {
                // Create a profile item
                Hashtable parameters = new Hashtable();
                parameters.Add("mass", mass.ToString());
                parameters.Add("massUnit", "g");
                parameters.Add("name", Guid.NewGuid().ToString());
                parameters.Add("representation", "full");
                Hashtable options = new Hashtable();
                options.Add("returnUnit", "g");
                ProfileItem item = m_AMEEProfile.CreateItem(DataMapping.Instance().Item(material), parameters, options);

                // Show the info window and shove data in it
                if (!m_summaryForm.Visible)
                    m_summaryForm.Show(new InventorWindow());
                m_summaryForm.UpdateData(item);
            }
        }

        public void ConfigureButtons()
        {
            GuidAttribute addInCLSID;
            addInCLSID = (GuidAttribute)GuidAttribute.GetCustomAttribute(typeof(AddInServer), typeof(GuidAttribute));
            string addInCLSIDString;
            addInCLSIDString = "{" + addInCLSID.Value + "}";

            Icon materialInfoIcon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AMEEMaterials.AMEE.AMEE.ico"));
            m_materialInfoButton = new MaterialInfoButton(
                "Material Info", "AMEEMaterials:MaterialInfoCmdBtn", CommandTypesEnum.kShapeEditCmdType,
                addInCLSIDString, "View material information",
                "Material Info", materialInfoIcon, materialInfoIcon, ButtonDisplayEnum.kDisplayTextInLearningMode);
        }

        public void ConfigureRibbon()
        {
			UserInterfaceManager userInterfaceManager = m_inventorApplication.UserInterfaceManager;
            if (userInterfaceManager.InterfaceStyle == InterfaceStyleEnum.kRibbonInterface)
            {
                //get the ribbon associated with part document
                Inventor.Ribbons ribbons;
                ribbons = userInterfaceManager.Ribbons;

                Inventor.Ribbon partRibbon;
                partRibbon = ribbons["Part"];

                //get the tabs associated with part ribbon
                RibbonTabs ribbonTabs;
                ribbonTabs = partRibbon.RibbonTabs;

                RibbonTab partSketchRibbonTab;
                partSketchRibbonTab = ribbonTabs["id_TabModel"];

                //create a new panel with the tab
                RibbonPanels ribbonPanels;
                ribbonPanels = partSketchRibbonTab.RibbonPanels;

                m_ribbonPanel = ribbonPanels.Add("AMEE", "AMEEMaterials:RibbonPanel", "{E2C578A6-F01F-11E0-8504-D4FD4724019B}", "", false);

                //add controls to the slot panel
                CommandControls ribbonPanelCtrls;
                ribbonPanelCtrls = m_ribbonPanel.CommandControls;

                //add the buttons to the ribbon panel
                CommandControl materialInfoCmdBtnCmdCtrl;
                materialInfoCmdBtnCmdCtrl = ribbonPanelCtrls.AddButton(m_materialInfoButton.ButtonDefinition);

            }
        }

        public void Deactivate()
        {
            m_AMEEProfile.Delete();
            
            // Release objects.
            Marshal.ReleaseComObject(m_inventorApplication);
            m_inventorApplication = null;

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void ExecuteCommand(int commandID)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }

        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        #endregion

        #region COM Registration functions

        /// <summary>
        /// Registers this class as an Add-In for Autodesk Inventor.
        /// This function is called when the assembly is registered for COM.
        /// </summary>
        [ComRegisterFunctionAttribute()]
        public static void Register(Type t)
        {
            RegistryKey clssRoot = Registry.ClassesRoot;
            RegistryKey clsid = null;
            RegistryKey subKey = null;

            try
            {
                clsid = clssRoot.CreateSubKey("CLSID\\" + AddInGuid(t));
                clsid.SetValue(null, "AMEEMaterials");
                subKey = clsid.CreateSubKey("Implemented Categories\\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                subKey.Close();

                subKey = clsid.CreateSubKey("Settings");
                subKey.SetValue("AddInType", "Standard");
                subKey.SetValue("LoadOnStartUp", "1");

                //subKey.SetValue("SupportedSoftwareVersionLessThan", "");
                subKey.SetValue("SupportedSoftwareVersionGreaterThan", "12..");
                //subKey.SetValue("SupportedSoftwareVersionEqualTo", "");
                //subKey.SetValue("SupportedSoftwareVersionNotEqualTo", "");
                //subKey.SetValue("Hidden", "0");
                //subKey.SetValue("UserUnloadable", "1");
                subKey.SetValue("Version", 0);
                subKey.Close();

                subKey = clsid.CreateSubKey("Description");
                subKey.SetValue(null, "AMEEMaterials");
            }
            catch
            {
                System.Diagnostics.Trace.Assert(false);
            }
            finally
            {
                if (subKey != null) subKey.Close();
                if (clsid != null) clsid.Close();
                if (clssRoot != null) clssRoot.Close();
            }

        }

        /// <summary>
        /// Unregisters this class as an Add-In for Autodesk Inventor.
        /// This function is called when the assembly is unregistered.
        /// </summary>
        [ComUnregisterFunctionAttribute()]
        public static void Unregister(Type t)
        {
            RegistryKey clssRoot = Registry.ClassesRoot;
            RegistryKey clsid = null;

            try
            {
                clssRoot = Microsoft.Win32.Registry.ClassesRoot;
                clsid = clssRoot.OpenSubKey("CLSID\\" + AddInGuid(t), true);
                clsid.SetValue(null, "");
                clsid.DeleteSubKeyTree("Implemented Categories\\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                clsid.DeleteSubKeyTree("Settings");
                clsid.DeleteSubKeyTree("Description");
            }
            catch { }
            finally
            {
                if (clsid != null) clsid.Close();
                if (clssRoot != null) clssRoot.Close();
            }
        }

        // This function uses reflection to get the value for the GuidAttribute attached to the class.
        private static String AddInGuid(Type t)
        {
            string guid = "";

            try
            {
                Object[] customAttributes = t.GetCustomAttributes(typeof(GuidAttribute), false);
                GuidAttribute guidAttribute = (GuidAttribute)customAttributes[0];
                guid = "{" + guidAttribute.Value.ToString() + "}";
            }
            catch
            {
            }

            return guid;

        }

        #endregion

    }
}
