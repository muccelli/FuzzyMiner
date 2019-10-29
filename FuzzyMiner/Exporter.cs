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
                        if (fn.GetLabel() != "start")
                        {
                            sw.WriteLine("              \"durations\" : [");
                            sw.WriteLine("                  {");
                            Dictionary<string, float> durations = ComputeDurations(fn);
                            sw.WriteLine("                      \"TotalDuration\" : " + durations["TotalDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MeanDuration\" : " + durations["MeanDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MinDuration\" : " + durations["MinDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MaxDuration\" : " + durations["MaxDuration"].ToString().Replace(",", "."));
                            sw.WriteLine("                   }");
                            sw.WriteLine("               ],");
                        }
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

                        if (fe.GetFromNode().GetLabel() != "start")
                        {
                            sw.WriteLine("              \"frequencySignificance\" : " + fe.GetFrequencySignificance() + ",");
                            sw.WriteLine("              \"durations\" : [");
                            sw.WriteLine("                  {");
                            Dictionary<string, float> durations = ComputeDurations(fe);
                            sw.WriteLine("                      \"TotalDuration\" : " + durations["TotalDuration"].ToString().Replace(",",".") + ",");
                            sw.WriteLine("                      \"MeanDuration\" : " + durations["MeanDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MinDuration\" : " + durations["MinDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MaxDuration\" : " + durations["MaxDuration"].ToString().Replace(",", "."));
                            sw.WriteLine("                   }");
                            sw.WriteLine("               ]");
                        }
                        else
                        {

                            sw.WriteLine("              \"frequencySignificance\" : " + fe.GetFrequencySignificance());
                        }

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
                overallAttributes.Add("Arithmetic_Mean", meanValue);
            }
            else
            {
                if (!attribute.Equals("EventID") && !attribute.Equals("OfferID"))
                {
                    if (!attribute.Equals("time:timestamp"))
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
            }
            Console.WriteLine("Returning overall attribute");
            return overallAttributes;
        }

        public static Dictionary<string, float> ComputeDurations(FuzzyEdge fe)
        {
            Dictionary<string, float> durations = new Dictionary<string, float>();
            List<float> edgeDurations = fe.GetDurationsList();
            float totalDuration = 0;
            foreach (float f in edgeDurations)
            {
                totalDuration += f;
            }
            float meanDuration = totalDuration / edgeDurations.Count;
            float minDuration = edgeDurations.Min();
            float maxDuration = edgeDurations.Max();

            durations.Add("TotalDuration", totalDuration);
            durations.Add("MeanDuration", meanDuration);
            durations.Add("MinDuration", minDuration);
            durations.Add("MaxDuration", maxDuration);

            return durations;
        }

        public static Dictionary<string, float> ComputeDurations(FuzzyNode fn)
        {
            Dictionary<string, float> durations = new Dictionary<string, float>();
            List<float> nodeDurations = fn.GetDurationsList();
            if (nodeDurations.Count > 0)
            {
                float totalDuration = 0;
                foreach (float f in nodeDurations)
                {
                    totalDuration += f;
                }
                float meanDuration = totalDuration / nodeDurations.Count;
                float minDuration = nodeDurations.Min();
                float maxDuration = nodeDurations.Max();

                durations.Add("TotalDuration", totalDuration);
                durations.Add("MeanDuration", meanDuration);
                durations.Add("MinDuration", minDuration);
                durations.Add("MaxDuration", maxDuration);
            }
            else
            {
                durations.Add("TotalDuration", 0);
                durations.Add("MeanDuration", 0);
                durations.Add("MinDuration", 0);
                durations.Add("MaxDuration", 0);
            }

            return durations;
        }
    }
}
