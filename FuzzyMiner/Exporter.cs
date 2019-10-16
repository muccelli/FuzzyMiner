using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FuzzyMinerModel;

namespace IO
{
    static class Exporter
    {
        public static void ExportToJSON(FuzzyModel fm, string outputName)
        {
            string filename = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + System.IO.Path.DirectorySeparatorChar + "Output" + System.IO.Path.DirectorySeparatorChar + outputName + ".json";
            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(filename))
                {
                    sw.WriteLine("{");
                    sw.WriteLine("  \"FuzzyModel\" : {");

                    sw.WriteLine("      \"nodes\": [");
                    foreach (FuzzyNode fn in fm.GetNodes())
                    {
                        sw.WriteLine("           {");
                        sw.WriteLine("              \"label\" : \"" + fn.GetLabel() + "\",");
                        sw.WriteLine("              \"frequencySignificance\" : " + fn.GetFrequencySignificance() + ",");
                        sw.WriteLine("              \"attributes\" : [");
                        sw.WriteLine("                  {");
                        foreach (string s in fn.GetAttributes().Keys)
                        {
                            if (s == fn.GetAttributes().Keys.Last<string>())
                            {
                                sw.WriteLine("                      \"" + s + "\" : \"" + fn.GetAttributes()[s] + "\"");
                                sw.WriteLine("                      }");
                                sw.WriteLine("                   ]");
                            }
                            else
                            {
                                sw.WriteLine("                      \"" + s + "\" : \"" + fn.GetAttributes()[s] + "\",");
                            }
                        }
                        if (fn == fm.GetNodes().Last<FuzzyNode>())
                        {
                            sw.WriteLine("          }");
                        }
                        else
                        {
                            sw.WriteLine("          },");
                        }
                    }
                    sw.WriteLine("      ],");
                    sw.WriteLine("      \"edges\": [");
                    foreach (FuzzyEdge fe in fm.GetEdges())
                    {
                        sw.WriteLine("           {");
                        sw.WriteLine("              \"label\" : \"" + fe.ToString() + "\",");
                        sw.WriteLine("              \"fromNode\" : \"" + fe.GetFromNode().GetLabel() + "\",");
                        sw.WriteLine("              \"toNode\" : \"" + fe.GetToNode().GetLabel() + "\",");
                        sw.WriteLine("              \"frequencySignificance\" : " + fe.GetFrequencySignificance());
                        if (fe == fm.GetEdges().Last<FuzzyEdge>())
                        {
                            sw.WriteLine("          }");
                        }
                        else
                        {
                            sw.WriteLine("          },");
                        }
                    }
                    sw.WriteLine("      ]");

                    sw.WriteLine("  }");
                    sw.WriteLine("}");
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }
    }
}
