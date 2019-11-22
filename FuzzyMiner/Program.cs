using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO;
using FuzzyMinerModel;
using FuzzyMiningAlgorithm;

namespace FuzzyMiner
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = null;
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter the log file name in the Input folder or the complete path after the argument '-f'");
                return;
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string a = args[i];
                    switch (a)
                    {
                        case "-f":
                            inputFile = args[i + 1];
                            if (inputFile == null)
                            {
                                Console.WriteLine("Please enter the log file name in the Input folder or the complete path after the argument '-f'");
                                return;
                            }
                            break;
                    }
                }
            }
            // Get log from file - Build model from log
            FuzzyModel fm;
            if (inputFile.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                fm = LogManager.parseLogFile(inputFile);
            }
            else
            {
                fm = LogManager.parseLogFile(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + System.IO.Path.DirectorySeparatorChar + "Input" + System.IO.Path.DirectorySeparatorChar + inputFile);
            }

            // Print the attributes
            //foreach (FuzzyNode fn in fm.GetNodes())
            //{
            //    Console.WriteLine(fn.GetLabel());
            //    for (int i = 0; i < fn.GetAttributes().Count; i++)
            //    {
            //        Console.WriteLine("Key = {0}, Value = {1}", fn.GetAttributes().ElementAt(i).Key, fn.GetAttributes().ElementAt(i).Value[0]);
            //        for (int j = 1; j < 5; j++)
            //        {
            //            Console.WriteLine("                        {0}", fn.GetAttributes().ElementAt(i).Value[j]);
            //        }
            //    }
            //    Console.WriteLine();
            //}

            Console.WriteLine("Number of edges: {0}", fm.GetEdges().Count);
            Console.WriteLine("Number of nodes: {0}", fm.GetNodes().Count);

            foreach (FuzzyNode n in fm.GetNodes())
            {
                Console.WriteLine(n.GetLabel());
                Console.WriteLine("Frequency: {0}", n.GetFrequencySignificance());
                Console.WriteLine("Edges entering:");
                foreach (FuzzyEdge e in n.GetInEdges())
                {
                    Console.WriteLine("{0}   ,   Frequency: {1}", e.ToString(), e.GetFrequencySignificance());
                }
                Console.WriteLine("Edges exiting:");
                foreach (FuzzyEdge e in n.GetOutEdges())
                {
                    Console.WriteLine("{0}   ,   Frequency: {1}", e.ToString(),  e.GetFrequencySignificance());
                }
                Console.WriteLine();
            }

            // Conflict resolution
            float preserveThr = 0.5F;
            float ratioThr = 0.01F;
            Console.WriteLine(System.Environment.NewLine + "Conflict resolution:" + System.Environment.NewLine);
            FuzzyMining.ConflictResolution(fm, preserveThr, ratioThr);
            Console.WriteLine("Number of edges: {0}", fm.GetEdges().Count);
            Console.WriteLine("Number of nodes: {0}", fm.GetNodes().Count);

            // Edge filtering
            float edgeCutoff = 0.5F;
            Console.WriteLine(System.Environment.NewLine + "Edge filtering:" + System.Environment.NewLine);
            FuzzyMining.EdgeFiltering(fm, edgeCutoff);
            Console.WriteLine("Number of edges: {0}", fm.GetEdges().Count);
            Console.WriteLine("Number of nodes: {0}" + System.Environment.NewLine, fm.GetNodes().Count);
            Console.WriteLine("Edges remaining after edge filtering:" + System.Environment.NewLine);
            foreach (FuzzyNode fn in fm.GetNodes())
            {
                foreach (FuzzyEdge fe in fn.GetOutEdges())
                {
                    Console.WriteLine(fe.ToString());
                }
            }

            // Aggregation and abstraction
            float nodeCutoff = 0.0F;
            Console.WriteLine(System.Environment.NewLine + "Aggregation and abstraction:" + System.Environment.NewLine);
            FuzzyMining.NodeAggregation(fm, nodeCutoff);
            Console.WriteLine("Number of edges: {0}", fm.GetEdges().Count);
            Console.WriteLine("Number of nodes: {0}", fm.GetNodes().Count);
            Console.WriteLine("Number of clusters: {0}" + System.Environment.NewLine, fm.GetClusters().Count);
            Console.WriteLine("Edges remaining after aggregation:" + System.Environment.NewLine);
            foreach (FuzzyNode fn in fm.GetNodes())
            {
                foreach (FuzzyEdge fe in fn.GetOutEdges())
                {
                    Console.WriteLine(fe.ToString());
                }
            }
            // Export xml
            Console.WriteLine("Preparing JSON file...");
            Exporter.ExportToJSON(fm, Path.GetFileNameWithoutExtension(inputFile));
            
        }
    }
}
