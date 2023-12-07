using System;
using System.IO;

namespace WLO_Translator_WPF
{
    public static class Paths
    {
        public static string RoamingFolder { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            + "\\WLO Translator\\";
        public static string AssociatedStoredItemData { get; private set; } = RoamingFolder + "\\AssociatedStoredItemData\\";

        public static void GenerateRoamingFolders()
        {
            Directory.CreateDirectory(AssociatedStoredItemData);
        }
    }
}
