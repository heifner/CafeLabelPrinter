using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace LabelPrinter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Avery5160 avery_ = new Avery5160();
        private OrderCollector orders_ = new OrderCollector();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // read config file
            readConfigFile();

            this.documentViewer.Document = avery_.CreateDocument(orders_, Config.layoutTopToBottom); 
        }

        private void readConfigFile()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("config.xml");

                XmlNode layoutNode = xmlDoc.SelectSingleNode("/config/layout-top-to-bottom");
                if (layoutNode == null) {
                    throw new FormatException("Unable to find /config/layout-top-to-bottom in config.xml");
                }
                Config.layoutTopToBottom = Convert.ToBoolean(layoutNode.InnerText.Trim());

                XmlNode mainRegExNode = xmlDoc.SelectSingleNode("/config/main-regex");
                if (mainRegExNode == null)
                {
                    throw new FormatException("Unable to find /config/main-regex in config.xml");
                }
                Config.mainRegEx = mainRegExNode.InnerText.Trim();

                XmlNode extraRegExNode = xmlDoc.SelectSingleNode("/config/extra-regex");
                if (extraRegExNode == null)
                {
                    throw new FormatException("Unable to find /config/extra-regex in config.xml");
                }
                Config.extraRegEx = extraRegExNode.InnerText.Trim();

                XmlNode extraLineRegExNode = xmlDoc.SelectSingleNode("/config/extra-line-regex");
                if (extraLineRegExNode == null)
                {
                    throw new FormatException("Unable to find /config/extra-line-regex in config.xml");
                }
                Config.extraLineRegEx = extraLineRegExNode.InnerText.Trim();

                XmlNode mealStrNode = xmlDoc.SelectSingleNode("/config/meal-str");
                if (mealStrNode == null) {
                    throw new FormatException("Unable to find /config/meal-str in config.xml");
                }
                Config.mealStr = mealStrNode.InnerText.Trim();

                XmlNodeList classReplaceList = xmlDoc.SelectNodes("/config/class-replace-str");
                foreach (XmlNode x in classReplaceList) {
                    XmlNode matchNode = x.SelectSingleNode("match");
                    XmlNode replaceNode = x.SelectSingleNode("replace");
                    Config.classReplacements.Add(matchNode.InnerText.Trim(), replaceNode.InnerText.Trim());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Application.Current.Shutdown();                
            }
        }

        private void mainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            parseText();
        }

        private void extraTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            parseText();
        }

        private void parseText()
        {
            try
            {
                orders_.clear();
                String mtext = mainTextBox.Text;
                String etext = extraTextBox.Text;

                if (mtext.Count() == 0) return;

                // Parse main text
                Regex re = new Regex(Config.mainRegEx);
                MatchCollection mc = re.Matches(mtext);
                foreach (Match m in mc)
                {
                    string clss = m.Groups["class"].Value.Trim();
                    string clssValue;
                    if (Config.classReplacements.TryGetValue(clss, out clssValue)) {
                        clss = clssValue;
                    }
                    string firstName = m.Groups["firstname"].Value.Trim();
                    string lastName = m.Groups["lastname"].Value.Trim();
                    string mealCode = m.Groups["meal_code"].Value.Trim();
                    int numMeals = Convert.ToInt32(m.Groups["num_meals"].Value.Trim());
                    string meal = m.Groups["meal"].Value.Trim();

                    orders_.addOrder(numMeals, clss, firstName, lastName, mealCode, meal);
                }

                // Parse extra text
                Regex reExtra = new Regex(Config.extraRegEx, RegexOptions.Compiled);
                Regex reExtraLine = new Regex(Config.extraLineRegEx, RegexOptions.Compiled);
                string[] lines = Regex.Split(etext, "\r?\n");
                string currentFirstName = "";
                string currentLastName = "";
                foreach (string line in lines) {
                    if (line.Count() == 0) continue;

                    Match extraMatch = reExtra.Match(line);
                    if (extraMatch.Success) {
                        currentFirstName = extraMatch.Groups["firstname"].Value.Trim();
                        currentLastName = extraMatch.Groups["lastname"].Value.Trim();
                        continue;
                    }

                    Match extraLineMatch = reExtraLine.Match(line);
                    if (extraLineMatch.Success) {
                        string num = extraLineMatch.Groups["num"].Value.Trim();
                        string mealCode = extraLineMatch.Groups["meal_code"].Value.Trim();
                        string extra = extraLineMatch.Groups["extra"].Value.Trim();
                        bool isMeal = extra.IndexOf(Config.mealStr, StringComparison.OrdinalIgnoreCase) >= 0;
                        orders_.addExtra(currentFirstName, currentLastName, num, mealCode, extra, isMeal);
                    }
                }

                // update the view
                this.documentViewer.Document = avery_.CreateDocument(orders_, Config.layoutTopToBottom);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }

    public class Config
    {
        public static bool layoutTopToBottom;
        public static string mainRegEx;
        public static string extraRegEx;
        public static string extraLineRegEx;
        public static string mealStr;
        public static Dictionary<string, string> classReplacements = new Dictionary<string, string>();
    }

}
