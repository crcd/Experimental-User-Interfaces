using UnityEngine;
using System.Collections;
using System.Xml;

public class XmlImportExport {
    public static bool ImportInputManager(RUISInputManager inputManager, string filename, TextAsset xmlSchema)
    {
        return true;
    }

    public static bool ExportInputManager(RUISInputManager inputManager, string filename){
       
        return true;
    }

    public static bool ImportDisplay(RUISDisplay display, string filename, TextAsset displaySchema, bool loadFromFileInEditor)
    {
        return true;
    }

    public static bool ExportDisplay(RUISDisplay display, string xmlFilename)
    {
      
        return true;
    }

    public static bool ImportKeystoningConfiguration(RUISKeystoningConfiguration keystoningConfiguration, XmlDocument xmlDoc)
    {
   
        return true;
    }

    public static bool ExportKeystoningConfiguration(RUISKeystoningConfiguration keystoningConfiguration, XmlElement displayXmlElement)
    {
        
        return true;
    }

    public static bool ExportKeystoning(RUISKeystoning.KeystoningCorners keystoningCorners, XmlElement xmlElement)
    {
        return true;
    }
}
