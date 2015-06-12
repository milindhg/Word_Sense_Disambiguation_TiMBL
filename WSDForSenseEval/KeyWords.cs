using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WSDForSenseEval
{
    public class KeyWords
    {
        public static string State { get; set; }

        public void CreateTrainFile()
        {
            State = "Train";
            // This text is added only once to the file. 
            if (File.Exists(FileData.Train_Keyword_Count))
            {
                File.Delete(FileData.Train_Keyword_Count);
            }
            ParseTrainXML();
        }

        public void CreateTestFile()
        {
            State = "Test";
            string path = FileData.Test_Keyword_Count;
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
            ParseLexelt(po, FileData.Train_Keyword_Count);
        }

        public void ParseTestXML()
        {
            XElement po = XElement.Load(FileData.Test_File);
            ParseLexelt(po, FileData.Test_Keyword_Count);




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
                    ExtractFeatures(str, senseIds, opFile , "Train");

                }
            }
            else
            {
                //iterating on instance element
                String id = (string)ins.Attribute("id");
                List<String> senseids = GetSenseIdFromKeyFile(id);
                IEnumerable<XElement> context =
                  from cont in ins.Descendants("context")
                  select cont;
                foreach (XElement cont in context)
                {   //iterating on context element
                    String str = cont.Value.ToString();
                    ExtractFeatures(str, senseids, opFile, "Test");
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

        public static void ExtractFeatures(String context, List<string> senseIds, String fileName, string state)
        {
            string[] nounwords = File.ReadAllLines(@"G:\GitHub\Word-Sense-Disambiguation-New\Output Files for Timbl\POSDisctionaries\allNouns_"+state+".txt");
            string[] adjwords = File.ReadAllLines(@"G:\GitHub\Word-Sense-Disambiguation-New\Output Files for Timbl\POSDisctionaries\allAdjectives_" + state + ".txt");
            string[] advwords = File.ReadAllLines(@"G:\GitHub\Word-Sense-Disambiguation-New\Output Files for Timbl\POSDisctionaries\allAdverbs_" + state + ".txt");
            string[] vrbwords = File.ReadAllLines(@"G:\GitHub\Word-Sense-Disambiguation-New\Output Files for Timbl\POSDisctionaries\allVerbs_" + state + ".txt");

            string[] words = context.Split(' ');
            int len = words.Length;
            int j = 0;
            int keyCnt = 0;
            for (int i = 0; i < words.Length; i++)
            {
                if (nounwords.Contains(words[i]))
                    keyCnt++;
                else if (adjwords.Contains(words[i]))
                    keyCnt++;
                else if (advwords.Contains(words[i]))
                    keyCnt++;
                else if (vrbwords.Contains(words[i]))
                    keyCnt++;
            }

            foreach (var item in senseIds)
            {
                FileData.WriteToFile(keyCnt.ToString(), fileName);
            }
        }
    }
}
