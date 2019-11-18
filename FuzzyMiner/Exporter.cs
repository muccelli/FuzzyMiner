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
                        if (fn.GetLabel() != "end_node")
                        {
                            sw.WriteLine("              \"durations\" : [");
                            sw.WriteLine("                  {");
                            Dictionary<string, double> durations = ComputeDurations(fn);
                            sw.WriteLine("                      \"TotalDuration\" : " + durations["TotalDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MeanDuration\" : " + durations["MeanDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MedianDuration\" : " + durations["MedianDuration"].ToString().Replace(",", ".") + ",");
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

                        if (fe.GetFromNode().GetLabel() != "start_node" && fe.GetToNode().GetLabel() != "end_node")
                        {
                            sw.WriteLine("              \"frequencySignificance\" : " + fe.GetFrequencySignificance() + ",");
                            sw.WriteLine("              \"durations\" : [");
                            sw.WriteLine("                  {");
                            Dictionary<string, double> durations = ComputeDurations(fe);
                            sw.WriteLine("                      \"TotalDuration\" : " + durations["TotalDuration"].ToString().Replace(",",".") + ",");
                            sw.WriteLine("                      \"MeanDuration\" : " + durations["MeanDuration"].ToString().Replace(",", ".") + ",");
                            sw.WriteLine("                      \"MedianDuration\" : " + durations["MedianDuration"].ToString().Replace(",", ".") + ",");
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

            var isNumeric = true;
            foreach (string s in overallAttributes.Keys)
            {
                if (!double.TryParse(s, out double n))
                {
                    isNumeric = false;
                    break;
                }
            }
            
            if (isNumeric && overallAttributes.Count > 70)
            {
                List<double> numericAttributes = new List<double>();
                double totalValue = 0;
                foreach (string s in overallAttributes.Keys)
                {
                    for (int i = 0; i < overallAttributes[s]; i++)
                    {
                        numericAttributes.Add(Convert.ToDouble(s));
                        totalValue += Convert.ToDouble(s);
                    }
                }
                numericAttributes.Sort();
                double minValue = numericAttributes[0];
                double maxValue = numericAttributes[numericAttributes.Count - 1];

                int m = numericAttributes.Count / 2;
                double medianValue = 0;
                if (numericAttributes.Count % 2 != 0)
                {
                    medianValue = numericAttributes[m];
                }
                else
                {
                    medianValue = (numericAttributes[m] + numericAttributes[m + 1]) / 2;
                }

                //double totalValue = 0;
                //foreach (string s in overallAttributes.Keys)
                //{
                //    totalValue += Convert.ToDouble(s) * overallAttributes[s];
                //}
                double meanValue = totalValue / attributeValues.Count;
                overallAttributes = new Dictionary<string, double>();
                overallAttributes.Add("Total", totalValue);
                overallAttributes.Add("Mean", meanValue);
                overallAttributes.Add("Median", medianValue);
                overallAttributes.Add("Min", minValue);
                overallAttributes.Add("Max", maxValue);
            }
            Console.WriteLine("Returning overall attribute");
            return overallAttributes;
        }

        public static Dictionary<string, double> ComputeDurations(FuzzyEdge fe)
        {
            Dictionary<string, double> durations = new Dictionary<string, double>();
            List<double> edgeDurations = fe.GetDurationsList();
            double totalDuration = 0;
            foreach (double f in edgeDurations)
            {
                totalDuration += f;
            }
            double meanDuration = totalDuration / edgeDurations.Count;

            edgeDurations.Sort();
            double minDuration = edgeDurations[0];
            double maxDuration = edgeDurations[edgeDurations.Count - 1];

            int m = edgeDurations.Count / 2;
            double medianDuration = 0;
            if (edgeDurations.Count % 2 != 0)
            {
                medianDuration = edgeDurations[m];
            }
            else
            {
                medianDuration = (edgeDurations[m] + edgeDurations[m + 1]) / 2;
            }

            durations.Add("TotalDuration", totalDuration);
            durations.Add("MeanDuration", meanDuration);
            durations.Add("MedianDuration", medianDuration);
            durations.Add("MinDuration", minDuration);
            durations.Add("MaxDuration", maxDuration);

            return durations;
        }

        public static Dictionary<string, double> ComputeDurations(FuzzyNode fn)
        {
            Dictionary<string, double> durations = new Dictionary<string, double>();
            List<double> nodeDurations = fn.GetDurationsList();
            if (nodeDurations.Count > 0)
            {
                double totalDuration = 0;
                foreach (double f in nodeDurations)
                {
                    totalDuration += f;
                }
                double meanDuration = totalDuration / nodeDurations.Count;

                nodeDurations.Sort();
                double minDuration = nodeDurations[0];
                double maxDuration = nodeDurations[nodeDurations.Count - 1];

                int m = nodeDurations.Count / 2;
                double medianDuration = 0;
                if (nodeDurations.Count % 2 != 0)
                {
                    medianDuration = nodeDurations[m];
                }
                else
                {
                    medianDuration = (nodeDurations[m] + nodeDurations[m + 1]) / 2;
                }

                durations.Add("TotalDuration", totalDuration);
                durations.Add("MeanDuration", meanDuration);
                durations.Add("MedianDuration", medianDuration);
                durations.Add("MinDuration", minDuration);
                durations.Add("MaxDuration", maxDuration);
            }
            else
            {
                durations.Add("TotalDuration", 0);
                durations.Add("MeanDuration", 0);
                durations.Add("MedianDuration", 0);
                durations.Add("MinDuration", 0);
                durations.Add("MaxDuration", 0);
            }

            return durations;
        }
    }
}
