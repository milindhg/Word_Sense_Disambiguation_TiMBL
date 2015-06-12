using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WSDForSenseEval
{
    public class XMLParser
    {
        public static string State { get; set; }

        public void CreateTrainFile()
        {
            State = "Train";
            // This text is added only once to the file. 
            if (File.Exists(FileData.Train_Output))
            {
                File.Delete(FileData.Train_Output);
            }
            ParseTrainXML();
        }

        public void GenerateKeyXml()
        {
            string path = FileData.Key_Output;
            // This text is added only once to the file. 
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("<home>");
            }
            String[] lines = File.ReadAllLines(FileData.Key_File);
            foreach (var line in lines)
            {
                string[] words = line.Split(' ');
                String senseid = "";
                for (int i = 2; i < words.Length; i++)
                {
                    senseid = senseid + words[i] + " ";
                }
                senseid = senseid.Trim();
                string keyLine = string.Concat("<key item=\"", words[0], "\"", " id=\"", words[1], "\" senseid=\"", senseid, "\" />");

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(keyLine);
                }

            }
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("</home>");
            }

        }

        public void CreateTestFile()
        {
            State = "Test";
            string path = FileData.Test_Output;
            // This text is added only once to the file. 
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            ParseTestXML();
            
        }

        public void ParseTrainXML()
        {
            XElement po = XElement.Load(FileData.Train_File);
            ParseLexelt(po, FileData.Train_Output);
        }

        public void ParseTestXML()
        {
            XElement po = XElement.Load(FileData.Test_File);
            ParseLexelt(po, FileData.Test_Output);
            
            
            
            
        }

        public void ParseLexelt(XElement po, string opFile)
        {
            IEnumerable<XElement> lexelts =
                from el in po.Descendants("lexelt")
                select el;
            //Iterating on lexelt
            foreach (XElement el in lexelts)
            {
                ParseInstance(el, opFile);
            }
        }

        public void ParseInstance(XElement el, string opFile)
        {
            IEnumerable<XElement> instance =
                  from ins in el.Descendants("instance")
                  select ins;
            foreach (XElement ins in instance)
            {   //iterating on instance element

                ParseAnswerAndContext(ins, opFile);
            }

            //-----------------
           
               
            
        }

        public void ParseAnswerAndContext(XElement ins, string opFile)
        {
            if (string.Equals(State, "Train", StringComparison.InvariantCultureIgnoreCase))
            {
                IEnumerable<XElement> answer =
                      from ans in ins.Descendants("answer")
                      select ans;
                List<string> senseIds = GetSenseIds(answer);
                IEnumerable<XElement> context =
                  from cont in ins.Descendants("context")
                  select cont;
                foreach (XElement cont in context)
                {   //iterating on context element
                    String str = cont.Value.ToString();
                    ExtractFeatures(str, senseIds, opFile);

                } 
            }
            else{
                //iterating on instance element
                String id = (string)ins.Attribute("id");
                List<String> senseids = GetSenseIdFromKeyFile(id);
                IEnumerable<XElement> context =
                  from cont in ins.Descendants("context")
                  select cont;
                foreach (XElement cont in context)
                {   //iterating on context element
                    String str = cont.Value.ToString();
                    ExtractFeatures(str, senseids, opFile);
                }
            }
        }

        public List<string> GetSenseIds(IEnumerable<XElement> answer)
        {
            List<string> senseIds = new List<string>();
            foreach (XElement ans in answer)
            {   //iterating on answer element
                senseIds.Add((String)ans.Attribute("senseid"));
            }
            return senseIds;
        }

        static List<String> GetSenseIdFromKeyFile(string id)
        {
            List<String> senseids = new List<string>();
            XElement root = XElement.Load(FileData.Key_Output);
            IEnumerable<XElement> keys =
                from el in root.Elements("key")
                where string.Equals((string)el.Attribute("id"), id)
                select el;
            foreach (XElement el in keys)
            {
                String str = (string)el.Attribute("senseid");
                senseids.AddRange(str.Split(' ').ToList());
            }
            return senseids;

        }

        public static void ExtractFeatures(String context, List<string> senseIds, String fileName)
        {
            string[] words = context.Split(' ');
            int len = words.Length;
            string wl3 = "_", wl2 = "_", wl1 = "_", w0 = "_", wr1 = "_", wr2 = "_", wr3 = "_";
            int j = 0;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Contains("%%"))
                {
                    j = i;

                    break;
                }
            }
            #region BuildMap
            if (j == 0)
            {

                wr1 = words[1];
                wr2 = words[2];
            }
            else if (j == 1)
            {
                wl1 = words[0];
                wr1 = words[2];
                wr2 = words[3];
            }
            else if (j == 2)
            {
                wl2 = words[0];
                wl1 = words[1];
                wr1 = words[3];
                wr2 = words[4];
                wr3 = words[5];
            }
            else if (j == len - 1)
            {
                wl2 = words[len - 3];
                wl1 = words[len - 2];

            }
            else if (j == len - 2)
            {
                wl2 = words[len - 4];
                wl1 = words[len - 3];
                wr1 = words[len - 1];

            }
            else if (j == len - 3)
            {
                wl3 = words[len - 6];
                wl2 = words[len - 5];
                wl1 = words[len - 4];
                wr1 = words[len - 2];
                wr2 = words[len - 1];
            }
            else
            {
                wl3 = words[j - 3];
                wl2 = words[j - 2];
                wl1 = words[j - 1];
                wr1 = words[j + 1];
                wr2 = words[j + 2];
                wr3 = words[j + 3];
            }

            #endregion
            //THIS PART OF CODE MIGHT CHANGE
            string[] endOfLine = { ".", "!", "?" };
            if (endOfLine.Contains(wl3))
            {
                wl3 = "_";
            }
            if (endOfLine.Contains(wl2))
            {
                wl3 = "_";
                wl2 = "_";
            }
            if (endOfLine.Contains(wl1))
            {
                wl3 = "_";
                wl2 = "_";
                wl1 = "_";
            }
            if (endOfLine.Contains(wr1))
            {
                wr1 = "_";
                wr2 = "_";
                wr3 = "_";
            }
            if (endOfLine.Contains(wr2))
            {
                wr2 = "_";
                wr3 = "_";
            }
            if (endOfLine.Contains(wr3))
            {
                wr3 = "_";
            }

            foreach (var item in senseIds)
            {
                string features = string.Concat(
                                                  wl3, " ",
                                                  wl2, " ",
                                                  wl1, " ",
                                                  wr1, " ",
                                                  wr2, " ",
                                                  wr3, " ",
                    
                                                  wl3, wl2, " ",
                                                  wl2, wl1, " ",
                                                  wl1, wr1, " ",
                                                  wr1, wr2, " ",
                                                  wr2, wr3, " ",
                                                  
                                                  wl3, wl2, wl1, " ",
                                                  wl2, wl1, wr1, " ",
                                                  wl1, wr1, wr2, " ",
                                                  wr1, wr2, wr3, " ",
                                                  
                                                  item);
                Console.WriteLine(features);
                FileData.WriteToFile(features, fileName);
            }

        }

    }
}
