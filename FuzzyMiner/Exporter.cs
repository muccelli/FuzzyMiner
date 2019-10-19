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

                    Console.WriteLine("Printing nodes...");
                    sw.WriteLine("      \"nodes\": [");
                    foreach (FuzzyNode fn in fm.GetNodes())
                    {
                        sw.WriteLine("           {");
                        sw.WriteLine("              \"label\" : \"" + fn.GetLabel() + "\",");
                        sw.WriteLine("              \"frequencySignificance\" : " + fn.GetFrequencySignificance() + ",");
                        sw.WriteLine("              \"attributes\" : [");
                        sw.WriteLine("                  {");
                        foreach (string s in fn.GetSignificantAttributes().Keys)
                        {
                            Dictionary<string, double> attributesValues = ComputeOverallAttribute(s, fn.GetSignificantAttributes()[s]);
                            if (attributesValues.Keys.Count != 0)
                            {
                                sw.WriteLine("                      \"" + s + "\" : [");
                                sw.WriteLine("                          {");
                                foreach (string key in attributesValues.Keys)
                                {
                                    if (key.Equals(attributesValues.Keys.Last<string>()))
                                    {
                                        sw.WriteLine("                          \"" + key + "\" : \"" + attributesValues[key] + "\"");
                                        sw.WriteLine("                          }");
                                        if (s.Equals(fn.GetSignificantAttributes().Keys.Last<string>()))
                                        {
                                            sw.WriteLine("                      ]");
                                        }
                                        else
                                        {
                                            sw.WriteLine("                      ],");
                                        }
                                    }
                                    else
                                    {
                                        sw.WriteLine("                          \"" + key + "\" : \"" + attributesValues[key] + "\",");
                                    }
                                }
                            }
                        }
                        sw.WriteLine("                      }");
                        sw.WriteLine("                   ]");
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

                    Console.WriteLine("Printing edges...");
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

        public static Dictionary<string, double> ComputeOverallAttribute(string attribute, List<string> attributeValues)
        {
            Console.WriteLine("Computing overall attribute");
            Dictionary<string, double> overallAttributes = new Dictionary<string, double>();
            var isNumeric = double.TryParse(attributeValues[0], out double n);
            if (isNumeric)
            {
                double totalValue = 0;
                foreach (string s in attributeValues)
                {
                    totalValue += Convert.ToDouble(s);
                }
                double meanValue = totalValue / attributeValues.Count;
                overallAttributes.Add("Total", totalValue);
                overallAttributes.Add("Arithmetic mean", meanValue);
            }
            else
            {
                if (!attribute.Equals("time:timestamp") && !attribute.Equals("EventID") && !attribute.Equals("OfferID"))
                {
                    foreach (string s in attributeValues)
                    {
                        if (overallAttributes.Keys.Contains<string>(s))
                        {
                            overallAttributes[s] += 1;
                        }
                        else
                        {
                            overallAttributes.Add(s, 1);
                        }
                    }
                }
            }
            Console.WriteLine("Returning overall attribute");
            return overallAttributes;
        }
    }
}
