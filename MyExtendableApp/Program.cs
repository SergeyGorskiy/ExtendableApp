using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using CommonSnappableTypes;

namespace MyExtendableApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            do
            {
                Console.WriteLine("\nWould you like to load a snapin? [Y,N]");
                string answer = Console.ReadLine();
                if (!answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                try
                {
                    LoadSnapin();
                }
                catch (Exception)
                {
                    Console.WriteLine("Sorry, can not find snapin.");
                }
            } 
            while (true);
        }
        static void LoadSnapin()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Filter = "assemblies (*.dll) | *.dll | All files (*.*) | *.*",
                FilterIndex = 1
            };
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                Console.WriteLine("User cancelled out of the open file dialog.");
                return;
            }
            if (dlg.FileName.Contains("CommonSnappableTypes"))
            {
                Console.WriteLine("CommonSnappableTypes has no snap-ins!");
            }
            else if (!LoadExternalModule(dlg.FileName))
            {
                Console.WriteLine("Nothing implements IAppFunctionality!");
            }
        }
        private static bool LoadExternalModule(string path)
        {
            bool foundSnapIn = false;
            Assembly theSnapInAsm = null;
            try
            {
                theSnapInAsm = Assembly.LoadFrom(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred loading the snapin: {e.Message}");
                return foundSnapIn;
            }
            var theClassTypes = theSnapInAsm.GetTypes()
                .Where(t=>t.IsClass && t.GetInterface("IAppFunctionality") != null)
                .Select(t=>t);
            foreach (Type t in theClassTypes)
            {
                foundSnapIn = true;
                IAppFunctionality itfApp = (IAppFunctionality) theSnapInAsm.CreateInstance(t.FullName, true);
                itfApp?.DoIt();
                DisplayCompanyData(t);
            }
            return foundSnapIn;
        }
        private static void DisplayCompanyData(Type t)
        {
            var compInfo = t.GetCustomAttributes(false).Where(ci => ci is CompanyInfoAttribute).Select(ci => ci);
            foreach (CompanyInfoAttribute c in compInfo)
            {
                Console.WriteLine($"More info about {c.CompanyName} can be found at {c.CompanyUrl}");
            }
        }
    }
}
