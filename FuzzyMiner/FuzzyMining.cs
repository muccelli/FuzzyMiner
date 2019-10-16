using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuzzyMinerModel;

namespace FuzzyMiningAlgorithm
{
    class FuzzyMining
    {
        public static void ConflictResolution(FuzzyModel fm, float preserveThr, float ratioThr)
        {
            List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
            foreach (FuzzyEdge AB in fm.GetEdges())
            {
                FuzzyEdge BA = fm.GetEdge(AB.GetToNode(), AB.GetFromNode());
                float relAB = 0;
                float relBA = 0;
                if (BA != null)
                {
                    if (AB.Equals(BA))
                    {
                        toRemove.Add(AB);
                    }
                    if (toRemove.Contains(AB) || toRemove.Contains(BA))
                    {
                        continue;
                    }
                    FuzzyNode A = AB.GetFromNode();
                    FuzzyNode B = AB.GetToNode();
                    // compute relative significance of edge A->B
                    float sigAB = AB.GetFrequencySignificance();
                    float sigAX = 0;
                    foreach (FuzzyEdge AX in A.GetOutEdges())
                    {
                        sigAX += AX.GetFrequencySignificance();
                    }
                    float sigXB = 0;
                    foreach (FuzzyEdge XB in B.GetInEdges())
                    {
                        sigXB += XB.GetFrequencySignificance();
                    }
                    relAB = (0.5F * (sigAB / sigAX)) + (0.5F * (sigAB / sigXB));
                    Console.WriteLine("{0}, Relative significance: {1}", AB.ToString(), relAB);

                    // compute relative significance of edge B->A
                    float sigBA = BA.GetFrequencySignificance();
                    float sigBX = 0;
                    foreach (FuzzyEdge BX in B.GetOutEdges())
                    {
                        sigBX += BX.GetFrequencySignificance();
                    }
                    float sigXA = 0;
                    foreach (FuzzyEdge XA in A.GetInEdges())
                    {
                        sigXA += XA.GetFrequencySignificance();
                    }
                    relBA = (0.5F * (sigBA / sigBX)) + (0.5F * (sigBA / sigXA));
                    Console.WriteLine("{0}, Relative significance: {1}", BA.ToString(), relBA);


                    // Decide preservation
                    if (relAB < preserveThr || relBA < preserveThr)
                    {
                        float ofsAB = Math.Abs(relAB - relBA);
                        if (ofsAB < ratioThr)
                        {
                            toRemove.Add(AB);
                            toRemove.Add(BA);
                        }
                        else
                        {
                            if (relAB > relBA)
                            {
                                toRemove.Add(BA);
                            }
                            else
                            {
                                toRemove.Add(AB);
                            }
                        }
                    }
                }
            }

            foreach (FuzzyEdge fe in toRemove)
            {
                fm.RemoveEdge(fe);
            }
        }

        public static void EdgeFiltering(FuzzyModel fm, float edgeCutoff)
        {
            float utilityRatio = 1;
            List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
            List<FuzzyEdge> toPreserve = new List<FuzzyEdge>();

            foreach (FuzzyNode fn in fm.GetNodes())
            {
                Dictionary<FuzzyEdge, float> utilValues = new Dictionary<FuzzyEdge, float>();
                Dictionary<FuzzyEdge, float> normUtilValues = new Dictionary<FuzzyEdge, float>();
                // if only one edge brings to this node, it has to be preserved
                if (fn.GetInEdges().Count > 1)
                {
                    foreach (FuzzyEdge fe in fn.GetInEdges())
                    {
                        float utility = utilityRatio * fe.GetFrequencySignificance();
                        utilValues.Add(fe, utility);
                    }

                    Console.WriteLine("Edges to {0}", fn.GetLabel());
                    // normalize utility
                    foreach (FuzzyEdge fe in utilValues.Keys)
                    {
                        float f = (utilValues[fe] - utilValues.Values.Min()) / (utilValues.Values.Max() - utilValues.Values.Min());
                        Console.WriteLine("{0} Utility value: {1}", fe.ToString(), f);
                        if (fe.GetFromNode().GetOutEdges().Count > 1)
                        {
                            normUtilValues.Add(fe, f);
                        }
                        else
                        {
                            toPreserve.Add(fe);
                        }
                    }

                    // filter edges
                    foreach (FuzzyEdge fe in normUtilValues.Keys)
                    {
                        if (normUtilValues[fe] > edgeCutoff)
                        {
                            toPreserve.Add(fe);
                        }
                    }
                }
                else
                {
                    foreach (FuzzyEdge fe in fn.GetInEdges())
                    {
                        toPreserve.Add(fe);
                    }
                }

                utilValues = new Dictionary<FuzzyEdge, float>();
                normUtilValues = new Dictionary<FuzzyEdge, float>();
                if (fn.GetOutEdges().Count > 1)
                {
                    foreach (FuzzyEdge fe in fn.GetOutEdges())
                    {
                        float utility = utilityRatio * fe.GetFrequencySignificance();
                        utilValues.Add(fe, utility);
                    }

                    Console.WriteLine("Edges from {0}", fn.GetLabel());
                    // normalize utility
                    foreach (FuzzyEdge fe in utilValues.Keys)
                    {
                        float f = (utilValues[fe] - utilValues.Values.Min()) / (utilValues.Values.Max() - utilValues.Values.Min());
                        Console.WriteLine("{0} Utility value: {1}", fe.ToString(), f);
                        //if (fe.GetToNode().GetInEdges().Count > 1)
                        //{
                        normUtilValues.Add(fe, f);
                        //}
                    }

                    // filter edges
                    foreach (FuzzyEdge fe in normUtilValues.Keys)
                    {
                        //if (normUtilValues.Count != 1)
                        //{
                        if (normUtilValues[fe] > edgeCutoff)
                        {
                            toPreserve.Add(fe);
                        }
                        //}
                    }
                }
            }

            foreach (FuzzyEdge fe in fm.GetEdges())
            {
                if (!toPreserve.Contains(fe))
                {
                    toRemove.Add(fe);
                }
            }
            foreach (FuzzyEdge fe in toRemove)
            {
                fm.RemoveEdge(fe);
            }
        }

        public static void NodeAggregation(FuzzyModel fm, float nodeCutoff)
        {
            BuildClusters(fm, nodeCutoff);
            Console.WriteLine("Edges remaining after building clusters:" + System.Environment.NewLine);
            foreach (FuzzyNode fn in fm.GetNodes())
            {
                foreach (FuzzyEdge fe in fn.GetOutEdges())
                {
                    Console.WriteLine(fe.ToString());
                }
            }


            // merge clusters
            MergeClusters(fm);

            // abstraction
            AbstractionClusters(fm);
        }

        private static void BuildClusters(FuzzyModel fm, float nodeCutoff)
        {
            // TODO normalize significance
            // find victims
            HashSet<FuzzyNode> victims = new HashSet<FuzzyNode>();
            foreach (FuzzyNode fn in fm.GetNodes())
            {
                if (fn.GetFrequencySignificance() < nodeCutoff)
                {
                    victims.Add(fn);
                }
            }

            // build clusters
            foreach (FuzzyNode fn in victims)
            {
                // find the most correlated neighbor
                float max = 0;
                FuzzyNode chosenOne = null;
                foreach (FuzzyEdge fe in fn.GetInEdges())
                {
                    if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
                    {
                        max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
                        chosenOne = fe.GetFromNode();
                    }
                }
                foreach (FuzzyEdge fe in fn.GetOutEdges())
                {
                    if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
                    {
                        max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
                        chosenOne = fe.GetToNode();
                    }
                }

                // if it is a cluster, then add the victim to it
                if (fm.IsCluster(fm.GetCluster(chosenOne)))
                {
                    FuzzyNodeCluster fnc = fm.GetCluster(chosenOne);
                    fnc.AddNodeToCluster(fn);
                    List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
                    List<FuzzyEdge> toAdd = new List<FuzzyEdge>();
                    foreach (FuzzyEdge fe in fn.GetInEdges())
                    {
                        FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), fnc);
                        toRemove.Add(fe);
                        toAdd.Add(newFe);
                    }
                    foreach (FuzzyEdge fe in fn.GetOutEdges())
                    {
                        FuzzyEdge newFe = new FuzzyEdge(fnc, fe.GetToNode());
                        toRemove.Add(fe);
                        toAdd.Add(newFe);
                    }
                    foreach (FuzzyEdge fe in toRemove)
                    {
                        fm.RemoveEdge(fe);
                    }
                    foreach (FuzzyEdge fe in toAdd)
                    {
                        if (fe.GetFromNode() != fe.GetToNode())
                        {
                            fm.AddEdge(fe);
                        }
                    }
                    fm.RemoveNode(fn);
                }
                else
                {
                    FuzzyNodeCluster fnc = new FuzzyNodeCluster(fn, "Cluster " + (fm.GetClusters().Count() + 1));
                    fm.AddCluster(fnc);
                    //Console.WriteLine(fnc.GetLabel());
                    List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
                    List<FuzzyEdge> toAdd = new List<FuzzyEdge>();
                    foreach (FuzzyEdge fe in fn.GetInEdges())
                    {
                        FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), fnc);
                        //Console.WriteLine(newFe.ToString());
                        toRemove.Add(fe);
                        toAdd.Add(newFe);
                    }
                    foreach (FuzzyEdge fe in fn.GetOutEdges())
                    {
                        FuzzyEdge newFe = new FuzzyEdge(fnc, fe.GetToNode());
                        //Console.WriteLine(newFe.ToString());
                        toRemove.Add(fe);
                        toAdd.Add(newFe);
                    }
                    foreach (FuzzyEdge fe in toRemove)
                    {
                        fm.RemoveEdge(fe);
                    }
                    foreach (FuzzyEdge fe in toAdd)
                    {
                        fm.AddEdge(fe);
                    }
                    fm.RemoveNode(fn);
                }
            }
        }

        private static void MergeClusters(FuzzyModel fm)
        {
            List<FuzzyNodeCluster> clustersToRemove = new List<FuzzyNodeCluster>();
            foreach (FuzzyNodeCluster fnc in fm.GetClusters())
            {
                // are all predecessors clusters?
                bool allClustersIn = true;
                if (fnc.GetInEdges().Count == 0)
                {
                    allClustersIn = false;
                }
                else
                {
                    foreach (FuzzyEdge fe in fnc.GetInEdges())
                    {
                        if (!fm.IsCluster(fe.GetFromNode()))
                        {
                            allClustersIn = false;
                            break;
                        }
                    }
                }
                // if they are
                if (allClustersIn)
                {
                    // find the most correlated
                    float max = 0;
                    FuzzyNodeCluster chosenOne = null;
                    foreach (FuzzyEdge fe in fnc.GetInEdges())
                    {
                        if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
                        {
                            max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
                            chosenOne = fm.GetCluster(fe.GetFromNode());
                        }
                    }

                    // merge the clusters
                    foreach (FuzzyNode fn in fnc.GetNodesInCluster())
                    {
                        chosenOne.AddNodeToCluster(fn);
                    }
                    List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
                    foreach (FuzzyEdge fe in fnc.GetInEdges())
                    {
                        if (fe.Equals(fm.GetEdge(chosenOne, fnc)))
                        {
                            //RemoveEdge(fe);
                            toRemove.Add(fe);
                        }
                        else
                        {
                            FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), chosenOne);
                            //RemoveEdge(fe);
                            toRemove.Add(fe);
                            fm.AddEdge(newFe);
                        }
                    }
                    foreach (FuzzyEdge fe in fnc.GetOutEdges())
                    {
                        FuzzyEdge newFe = new FuzzyEdge(chosenOne, fe.GetToNode());
                        //RemoveEdge(fe);
                        toRemove.Add(fe);
                        fm.AddEdge(newFe);
                    }
                    foreach (FuzzyEdge fe in toRemove)
                    {
                        fm.RemoveEdge(fe);
                    }
                    //RemoveCluster(fnc);
                    clustersToRemove.Add(fnc);
                }
                else
                {
                    // are all successors clusters?
                    bool allClustersOut = true;
                    if (fnc.GetOutEdges().Count == 0)
                    {
                        allClustersOut = false;
                    }
                    else
                    {
                        foreach (FuzzyEdge fe in fnc.GetOutEdges())
                        {
                            if (!fm.IsCluster(fe.GetToNode()))
                            {
                                allClustersOut = false;
                                break;
                            }
                        }
                    }

                    // if they are
                    if (allClustersOut)
                    {
                        // find the most correlated
                        float max = 0;
                        FuzzyNodeCluster chosenOne = null;
                        foreach (FuzzyEdge fe in fnc.GetOutEdges())
                        {
                            if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
                            {
                                max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
                                chosenOne = fm.GetCluster(fe.GetToNode());
                            }
                        }

                        // merge the clusters
                        foreach (FuzzyNode fn in fnc.GetNodesInCluster())
                        {
                            chosenOne.AddNodeToCluster(fn);
                        }
                        List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
                        foreach (FuzzyEdge fe in fnc.GetOutEdges())
                        {
                            if (fe.Equals(fm.GetEdge(chosenOne, fnc)))
                            {
                                toRemove.Add(fe);
                            }
                            else
                            {
                                FuzzyEdge newFe = new FuzzyEdge(chosenOne, fe.GetToNode());
                                toRemove.Add(fe);
                                fm.AddEdge(newFe);
                            }
                        }
                        foreach (FuzzyEdge fe in fnc.GetInEdges())
                        {
                            FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), chosenOne);
                            toRemove.Add(fe);
                            fm.AddEdge(newFe);
                        }
                        foreach (FuzzyEdge fe in toRemove)
                        {
                            fm.RemoveEdge(fe);
                        }
                        clustersToRemove.Add(fnc);
                    }
                }
            }
            foreach (FuzzyNodeCluster fnc in clustersToRemove)
            {
                fm.RemoveCluster(fnc);
            }
        }

        private static void AbstractionClusters(FuzzyModel fm)
        {
            List<FuzzyNodeCluster> clustersToRemove = new List<FuzzyNodeCluster>();
            List<FuzzyEdge> edgesToRemove = new List<FuzzyEdge>();
            foreach (FuzzyNodeCluster fnc in fm.GetClusters())
            {
                if (fnc.GetInEdges().Count == 0 && fnc.GetOutEdges().Count == 0)
                {
                    clustersToRemove.Add(fnc);
                }
                else if (fnc.GetNodesInCluster().Count == 1)
                {
                    foreach (FuzzyEdge fei in fnc.GetInEdges())
                    {
                        foreach (FuzzyEdge feo in fnc.GetOutEdges())
                        {
                            FuzzyEdge newFe = new FuzzyEdge(fei.GetFromNode(), feo.GetToNode());
                            if (!fm.GetEdges().Contains(newFe))
                            {
                                fm.AddEdge(newFe);
                            }
                        }
                        edgesToRemove.Add(fei);
                    }
                    clustersToRemove.Add(fnc);
                }
            }
            foreach (FuzzyNodeCluster fnc in clustersToRemove)
            {
                fm.RemoveCluster(fnc);
            }
            foreach (FuzzyEdge fe in edgesToRemove)
            {
                fm.RemoveEdge(fe);
            }
        }

    }
}
