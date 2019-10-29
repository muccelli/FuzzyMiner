using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace FuzzyMinerModel
{
    public class FuzzyModel
    {
        private List<FuzzyNode> nodes;
        private Dictionary<string, FuzzyNode> eventToNode;
        private List<FuzzyEdge> edges;
        private List<FuzzyNodeCluster> clusters;

        public FuzzyModel()
        {
            nodes = new List<FuzzyNode>();
            eventToNode = new Dictionary<string, FuzzyNode>();
            edges = new List<FuzzyEdge>();
            clusters = new List<FuzzyNodeCluster>();
        }

        public List<FuzzyNode> GetNodes()
        {
            return nodes;
        }

        public FuzzyNode GetNode(string label)
        {
            return eventToNode[label];
        }

        public List<string> GetEvents()
        {
            return eventToNode.Keys.ToList();
        }

        public List<FuzzyEdge> GetEdges()
        {
            return edges;
        }

        public FuzzyEdge GetEdge(FuzzyNode from, FuzzyNode to)
        {
            foreach (FuzzyEdge fe in edges)
            {
                if ((fe.GetFromNode() == from) && (fe.GetToNode() == to))
                {
                    return fe;
                }
            }
            return null;
        }

        public FuzzyEdge GetEdge(FuzzyEdge fedge)
        {
            foreach (FuzzyEdge fe in edges)
            {
                if (fe.Equals(fedge))
                {
                    return fe;
                }
            }
            return null;
        }

        public List<FuzzyNodeCluster> GetClusters()
        {
            return clusters;
        }

        public FuzzyNodeCluster GetCluster(FuzzyNode fn)
        {
            foreach (FuzzyNodeCluster fnc in clusters)
            {
                if (fnc.Equals(fn))
                {
                    return fnc;
                }
            }
            return null;
        }

        public void AddNode(string label)
        {
            FuzzyNode node = new FuzzyNode(label);
            nodes.Add(node);
            eventToNode.Add(label, node);
        }

        public void AddNode(FuzzyNode node)
        {
            nodes.Add(node);
            eventToNode.Add(node.GetLabel(), node);
        }

        public void AddEdge(FuzzyNode from, FuzzyNode to)
        {
            FuzzyEdge e = new FuzzyEdge(from, to);
            edges.Add(e);
            from.AddOutEdges(e);
            to.AddInEdges(e);
        }

        public void AddEdge(FuzzyEdge e)
        {
            edges.Add(e);
            e.GetFromNode().AddOutEdges(e);
            e.GetToNode().AddInEdges(e);
        }

        public void AddCluster(FuzzyNodeCluster fnc)
        {
            clusters.Add(fnc);
            AddNode(fnc);
        }

        public void RemoveNode(FuzzyNode fn)
        {
            nodes.Remove(fn);
            foreach (FuzzyEdge fe in edges)
            {
                if (fe.GetFromNode() == fn || fe.GetToNode() == fn)
                {
                    RemoveEdge(fe);
                }
            }
        }

        public void RemoveEdge(FuzzyEdge fe)
        {
            edges.Remove(fe);
            foreach (FuzzyNode fn in nodes)
            {
                fn.RemoveEdge(fe);
            }
        }

        public void RemoveCluster(FuzzyNodeCluster fnc)
        {
            clusters.Remove(fnc);
            nodes.Remove(fnc);
        }

        public bool IsCluster(FuzzyNode fn)
        {
            if (clusters.Contains(fn))
            {
                return true;
            }
            return false;
        }

        //public void ConflictResolution(float preserveThr, float ratioThr)
        //{
        //    List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
        //    foreach (FuzzyEdge AB in edges)
        //    {
        //        FuzzyEdge BA = GetEdge(AB.GetToNode(), AB.GetFromNode());
        //        if (toRemove.Contains(AB) || toRemove.Contains(BA))
        //        {
        //            continue;
        //        }
        //        float relAB = 0;
        //        float relBA = 0;
        //        if (BA != null)
        //        {
        //            FuzzyNode A = AB.GetFromNode();
        //            FuzzyNode B = AB.GetToNode();
        //            // compute relative significance of edge A->B
        //            float sigAB = AB.GetFrequencySignificance();
        //            float sigAX = 0;
        //            foreach (FuzzyEdge AX in A.GetOutEdges())
        //            {
        //                sigAX += AX.GetFrequencySignificance();
        //            }
        //            float sigXB = 0;
        //            foreach (FuzzyEdge XB in B.GetInEdges())
        //            {
        //                sigXB += XB.GetFrequencySignificance();
        //            }
        //            relAB = (0.5F * (sigAB / sigAX)) + (0.5F * (sigAB / sigXB));
        //            Console.WriteLine("{0}, Relative significance: {1}", AB.ToString(), relAB);

        //            // compute relative significance of edge B->A
        //            float sigBA = BA.GetFrequencySignificance();
        //            float sigBX = 0;
        //            foreach (FuzzyEdge BX in B.GetOutEdges())
        //            {
        //                sigBX += BX.GetFrequencySignificance();
        //            }
        //            float sigXA = 0;
        //            foreach (FuzzyEdge XA in A.GetInEdges())
        //            {
        //                sigXA += XA.GetFrequencySignificance();
        //            }
        //            relBA = (0.5F * (sigBA / sigBX)) + (0.5F * (sigBA / sigXA));
        //            Console.WriteLine("{0}, Relative significance: {1}", BA.ToString(), relBA);


        //            // Decide preservation
        //            if (relAB < preserveThr || relBA < preserveThr)
        //            {
        //                float ofsAB = Math.Abs(relAB - relBA);
        //                if (ofsAB < ratioThr)
        //                {
        //                    toRemove.Add(AB);
        //                    toRemove.Add(BA);
        //                }
        //                else
        //                {
        //                    if (relAB > relBA)
        //                    {
        //                        toRemove.Add(BA);
        //                    }
        //                    else
        //                    {
        //                        toRemove.Add(AB);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    foreach (FuzzyEdge fe in toRemove)
        //    {
        //        RemoveEdge(fe);
        //    }
        //}

        ////public void EdgeFiltering(float edgeCutoff)
        ////{
        ////    float utilityRatio = 1;
        ////    List<FuzzyEdge> toRemove = new List<FuzzyEdge>();

        ////    foreach (FuzzyNode fn in nodes)
        ////    {
        ////        Dictionary<FuzzyEdge, float> utilValues = new Dictionary<FuzzyEdge, float>();
        ////        Dictionary<FuzzyEdge, float> normUtilValues = new Dictionary<FuzzyEdge, float>();
        ////        if (fn.GetInEdges().Count > 1)
        ////        {
        ////            foreach (FuzzyEdge fe in fn.GetInEdges())
        ////            {
        ////                float utility = utilityRatio * fe.GetFrequencySignificance();
        ////                utilValues.Add(fe, utility);
        ////            }

        ////            Console.WriteLine("Edges to {0}", fn.GetLabel());
        ////            // normalize utility
        ////            foreach (FuzzyEdge fe in utilValues.Keys)
        ////            {
        ////                float f = (utilValues[fe] - utilValues.Values.Min()) / (utilValues.Values.Max() - utilValues.Values.Min());
        ////                Console.WriteLine("{0} Utility value: {1}", fe.ToString(), f);
        ////                if (fe.GetFromNode().GetOutEdges().Count > 1)
        ////                {
        ////                    normUtilValues.Add(fe, f);
        ////                }
        ////            }

        ////            // filter edges
        ////            foreach (FuzzyEdge fe in normUtilValues.Keys)
        ////            {
        ////                if (normUtilValues[fe] < edgeCutoff)
        ////                {
        ////                    toRemove.Add(fe);
        ////                }
        ////            }
        ////        }

        ////        utilValues = new Dictionary<FuzzyEdge, float>();
        ////        normUtilValues = new Dictionary<FuzzyEdge, float>();
        ////        if (fn.GetOutEdges().Count > 1)
        ////        {
        ////            foreach (FuzzyEdge fe in fn.GetOutEdges())
        ////            {
        ////                float utility = utilityRatio * fe.GetFrequencySignificance();
        ////                utilValues.Add(fe, utility);
        ////            }

        ////            Console.WriteLine("Edges from {0}", fn.GetLabel());
        ////            // normalize utility
        ////            foreach (FuzzyEdge fe in utilValues.Keys)
        ////            {
        ////                float f = (utilValues[fe] - utilValues.Values.Min()) / (utilValues.Values.Max() - utilValues.Values.Min());
        ////                Console.WriteLine("{0} Utility value: {1}", fe.ToString(), f);
        ////                if (fe.GetToNode().GetInEdges().Count > 1)
        ////                {
        ////                    normUtilValues.Add(fe, f);
        ////                }
        ////            }

        ////            // filter edges
        ////            foreach (FuzzyEdge fe in normUtilValues.Keys)
        ////            {
        ////                if (normUtilValues.Count != 1)
        ////                {
        ////                    if (normUtilValues[fe] < edgeCutoff)
        ////                    {
        ////                        toRemove.Add(fe);
        ////                    }
        ////                }
        ////            }
        ////        }

        ////        foreach (FuzzyEdge fe in toRemove)
        ////        {
        ////            RemoveEdge(fe);
        ////        }
        ////    }
        ////}

        //public void EdgeFiltering(float edgeCutoff)
        //{
        //    float utilityRatio = 1;
        //    List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
        //    List<FuzzyEdge> toPreserve = new List<FuzzyEdge>();

        //    foreach (FuzzyNode fn in nodes)
        //    {
        //        Dictionary<FuzzyEdge, float> utilValues = new Dictionary<FuzzyEdge, float>();
        //        Dictionary<FuzzyEdge, float> normUtilValues = new Dictionary<FuzzyEdge, float>();
        //        // if only one edge brings to this node, it has to be preserved
        //        if (fn.GetInEdges().Count > 1)
        //        {
        //            foreach (FuzzyEdge fe in fn.GetInEdges())
        //            {
        //                float utility = utilityRatio * fe.GetFrequencySignificance();
        //                utilValues.Add(fe, utility);
        //            }

        //            Console.WriteLine("Edges to {0}", fn.GetLabel());
        //            // normalize utility
        //            foreach (FuzzyEdge fe in utilValues.Keys)
        //            {
        //                float f = (utilValues[fe] - utilValues.Values.Min()) / (utilValues.Values.Max() - utilValues.Values.Min());
        //                Console.WriteLine("{0} Utility value: {1}", fe.ToString(), f);
        //                if (fe.GetFromNode().GetOutEdges().Count > 1)
        //                {
        //                    normUtilValues.Add(fe, f);
        //                }
        //                else
        //                {
        //                    toPreserve.Add(fe);
        //                }
        //            }

        //            // filter edges
        //            foreach (FuzzyEdge fe in normUtilValues.Keys)
        //            {
        //                if (normUtilValues[fe] > edgeCutoff)
        //                {
        //                    toPreserve.Add(fe);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            foreach (FuzzyEdge fe in fn.GetInEdges())
        //            {
        //                toPreserve.Add(fe);
        //            }
        //        }

        //        utilValues = new Dictionary<FuzzyEdge, float>();
        //        normUtilValues = new Dictionary<FuzzyEdge, float>();
        //        if (fn.GetOutEdges().Count > 1)
        //        {
        //            foreach (FuzzyEdge fe in fn.GetOutEdges())
        //            {
        //                float utility = utilityRatio * fe.GetFrequencySignificance();
        //                utilValues.Add(fe, utility);
        //            }

        //            Console.WriteLine("Edges from {0}", fn.GetLabel());
        //            // normalize utility
        //            foreach (FuzzyEdge fe in utilValues.Keys)
        //            {
        //                float f = (utilValues[fe] - utilValues.Values.Min()) / (utilValues.Values.Max() - utilValues.Values.Min());
        //                Console.WriteLine("{0} Utility value: {1}", fe.ToString(), f);
        //                //if (fe.GetToNode().GetInEdges().Count > 1)
        //                //{
        //                    normUtilValues.Add(fe, f);
        //                //}
        //            }

        //            // filter edges
        //            foreach (FuzzyEdge fe in normUtilValues.Keys)
        //            {
        //                //if (normUtilValues.Count != 1)
        //                //{
        //                    if (normUtilValues[fe] > edgeCutoff)
        //                    {
        //                        toPreserve.Add(fe);
        //                    }
        //                //}
        //            }
        //        }
        //    }

        //    foreach (FuzzyEdge fe in edges)
        //    {
        //        if (!toPreserve.Contains(fe))
        //        {
        //            toRemove.Add(fe);
        //        }
        //    }
        //    foreach (FuzzyEdge fe in toRemove)
        //    {
        //        RemoveEdge(fe);
        //    }
        //}

        //public void NodeAggregation(float nodeCutoff)
        //{
        //    BuildClusters(nodeCutoff);
        //    Console.WriteLine("Edges remaining after building clusters:" + System.Environment.NewLine);
        //    foreach (FuzzyNode fn in GetNodes())
        //    {
        //        foreach (FuzzyEdge fe in fn.GetOutEdges())
        //        {
        //            Console.WriteLine(fe.ToString());
        //        }
        //    }


        //    // merge clusters
        //    MergeClusters();

        //    // abstraction
        //    AbstractionClusters();
        //}

        //private void BuildClusters(float nodeCutoff)
        //{
        //    // TODO normalize significance
        //    // find victims
        //    HashSet<FuzzyNode> victims = new HashSet<FuzzyNode>();
        //    foreach (FuzzyNode fn in nodes)
        //    {
        //        if (fn.GetFrequencySignificance() < nodeCutoff)
        //        {
        //            victims.Add(fn);
        //        }
        //    }

        //    // build clusters
        //    foreach (FuzzyNode fn in victims)
        //    {
        //        // find the most correlated neighbor
        //        float max = 0;
        //        FuzzyNode chosenOne = null;
        //        foreach (FuzzyEdge fe in fn.GetInEdges())
        //        {
        //            if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
        //            {
        //                max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
        //                chosenOne = fe.GetFromNode();
        //            }
        //        }
        //        foreach (FuzzyEdge fe in fn.GetOutEdges())
        //        {
        //            if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
        //            {
        //                max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
        //                chosenOne = fe.GetToNode();
        //            }
        //        }

        //        // if it is a cluster, then add the victim to it
        //        if (IsCluster(GetCluster(chosenOne)))
        //        {
        //            FuzzyNodeCluster fnc = GetCluster(chosenOne);
        //            fnc.AddNodeToCluster(fn);
        //            List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
        //            List<FuzzyEdge> toAdd = new List<FuzzyEdge>();
        //            foreach (FuzzyEdge fe in fn.GetInEdges())
        //            {
        //                FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), fnc);
        //                toRemove.Add(fe);
        //                toAdd.Add(newFe);
        //            }
        //            foreach (FuzzyEdge fe in fn.GetOutEdges())
        //            {
        //                FuzzyEdge newFe = new FuzzyEdge(fnc, fe.GetToNode());
        //                toRemove.Add(fe);
        //                toAdd.Add(newFe);
        //            }
        //            foreach (FuzzyEdge fe in toRemove)
        //            {
        //                RemoveEdge(fe);
        //            }
        //            foreach (FuzzyEdge fe in toAdd)
        //            {
        //                if (fe.GetFromNode() != fe.GetToNode())
        //                {
        //                    AddEdge(fe);
        //                }
        //            }
        //            RemoveNode(fn);
        //        }
        //        else
        //        {
        //            FuzzyNodeCluster fnc = new FuzzyNodeCluster(fn, "Cluster " + (clusters.Count() + 1));
        //            AddCluster(fnc);
        //            //Console.WriteLine(fnc.GetLabel());
        //            List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
        //            List<FuzzyEdge> toAdd = new List<FuzzyEdge>();
        //            foreach (FuzzyEdge fe in fn.GetInEdges())
        //            {
        //                FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), fnc);
        //                //Console.WriteLine(newFe.ToString());
        //                toRemove.Add(fe);
        //                toAdd.Add(newFe);
        //            }
        //            foreach (FuzzyEdge fe in fn.GetOutEdges())
        //            {
        //                FuzzyEdge newFe = new FuzzyEdge(fnc, fe.GetToNode());
        //                //Console.WriteLine(newFe.ToString());
        //                toRemove.Add(fe);
        //                toAdd.Add(newFe);
        //            }
        //            foreach (FuzzyEdge fe in toRemove)
        //            {
        //                RemoveEdge(fe);
        //            }
        //            foreach (FuzzyEdge fe in toAdd)
        //            {
        //                AddEdge(fe);
        //            }
        //            RemoveNode(fn);
        //        }
        //    }
        //}

        //private void MergeClusters()
        //{
        //    List<FuzzyNodeCluster> clustersToRemove = new List<FuzzyNodeCluster>();
        //    foreach (FuzzyNodeCluster fnc in clusters)
        //    {
        //        // are all predecessors clusters?
        //        bool allClustersIn = true;
        //        if (fnc.GetInEdges().Count == 0)
        //        {
        //            allClustersIn = false;
        //        }
        //        else
        //        {
        //            foreach (FuzzyEdge fe in fnc.GetInEdges())
        //            {
        //                if (!IsCluster(fe.GetFromNode()))
        //                {
        //                    allClustersIn = false;
        //                    break;
        //                }
        //            }
        //        }
        //        // if they are
        //        if (allClustersIn)
        //        {
        //            // find the most correlated
        //            float max = 0;
        //            FuzzyNodeCluster chosenOne = null;
        //            foreach (FuzzyEdge fe in fnc.GetInEdges())
        //            {
        //                if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
        //                {
        //                    max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
        //                    chosenOne = GetCluster(fe.GetFromNode());
        //                }
        //            }

        //            // merge the clusters
        //            foreach (FuzzyNode fn in fnc.GetNodesInCluster())
        //            {
        //                chosenOne.AddNodeToCluster(fn);
        //            }
        //            List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
        //            foreach (FuzzyEdge fe in fnc.GetInEdges())
        //            {
        //                if (fe.Equals(GetEdge(chosenOne, fnc)))
        //                {
        //                    //RemoveEdge(fe);
        //                    toRemove.Add(fe);
        //                }
        //                else
        //                {
        //                    FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), chosenOne);
        //                    //RemoveEdge(fe);
        //                    toRemove.Add(fe);
        //                    AddEdge(newFe);
        //                }
        //            }
        //            foreach (FuzzyEdge fe in fnc.GetOutEdges())
        //            {
        //                FuzzyEdge newFe = new FuzzyEdge(chosenOne, fe.GetToNode());
        //                //RemoveEdge(fe);
        //                toRemove.Add(fe);
        //                AddEdge(newFe);
        //            }
        //            foreach (FuzzyEdge fe in toRemove)
        //            {
        //                RemoveEdge(fe);
        //            }
        //            //RemoveCluster(fnc);
        //            clustersToRemove.Add(fnc);
        //        }
        //        else
        //        {
        //            // are all successors clusters?
        //            bool allClustersOut = true;
        //            if (fnc.GetOutEdges().Count == 0)
        //            {
        //                allClustersOut = false;
        //            }
        //            else
        //            {
        //                foreach (FuzzyEdge fe in fnc.GetOutEdges())
        //                {
        //                    if (!IsCluster(fe.GetToNode()))
        //                    {
        //                        allClustersOut = false;
        //                        break;
        //                    }
        //                }
        //            }
                    
        //            // if they are
        //            if (allClustersOut)
        //            {
        //                // find the most correlated
        //                float max = 0;
        //                FuzzyNodeCluster chosenOne = null;
        //                foreach (FuzzyEdge fe in fnc.GetOutEdges())
        //                {
        //                    if ((fe.GetEndpointCorrelation() + fe.GetProximityCorrelation()) > max)
        //                    {
        //                        max = fe.GetEndpointCorrelation() + fe.GetProximityCorrelation();
        //                        chosenOne = GetCluster(fe.GetToNode());
        //                    }
        //                }

        //                // merge the clusters
        //                foreach (FuzzyNode fn in fnc.GetNodesInCluster())
        //                {
        //                    chosenOne.AddNodeToCluster(fn);
        //                }
        //                List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
        //                foreach (FuzzyEdge fe in fnc.GetOutEdges())
        //                {
        //                    if (fe.Equals(GetEdge(chosenOne, fnc)))
        //                    {
        //                        toRemove.Add(fe);
        //                    }
        //                    else
        //                    {
        //                        FuzzyEdge newFe = new FuzzyEdge(chosenOne, fe.GetToNode());
        //                        toRemove.Add(fe);
        //                        AddEdge(newFe);
        //                    }
        //                }
        //                foreach (FuzzyEdge fe in fnc.GetInEdges())
        //                {
        //                    FuzzyEdge newFe = new FuzzyEdge(fe.GetFromNode(), chosenOne);
        //                    toRemove.Add(fe);
        //                    AddEdge(newFe);
        //                }
        //                foreach (FuzzyEdge fe in toRemove)
        //                {
        //                    RemoveEdge(fe);
        //                }
        //                clustersToRemove.Add(fnc);
        //            }
        //        }
        //    }
        //    foreach (FuzzyNodeCluster fnc in clustersToRemove)
        //    {
        //        RemoveCluster(fnc);
        //    }
        //}

        //private void AbstractionClusters()
        //{
        //    List<FuzzyNodeCluster> clustersToRemove = new List<FuzzyNodeCluster>();
        //    List<FuzzyEdge> edgesToRemove = new List<FuzzyEdge>();
        //    foreach (FuzzyNodeCluster fnc in clusters)
        //    {
        //        if (fnc.GetInEdges().Count == 0 && fnc.GetOutEdges().Count == 0)
        //        {
        //            clustersToRemove.Add(fnc);
        //        }
        //        else if (fnc.GetNodesInCluster().Count == 1)
        //        {
        //            foreach (FuzzyEdge fei in fnc.GetInEdges())
        //            {
        //                foreach (FuzzyEdge feo in fnc.GetOutEdges())
        //                {
        //                    FuzzyEdge newFe = new FuzzyEdge(fei.GetFromNode(), feo.GetToNode());
        //                    if (!edges.Contains(newFe))
        //                    {
        //                        AddEdge(newFe);
        //                    }
        //                }
        //                edgesToRemove.Add(fei);
        //            }
        //            clustersToRemove.Add(fnc);
        //        }
        //    }
        //    foreach (FuzzyNodeCluster fnc in clustersToRemove)
        //    {
        //        RemoveCluster(fnc);
        //    }
        //    foreach (FuzzyEdge fe in edgesToRemove)
        //    {
        //        RemoveEdge(fe);
        //    }
        //}
    }

    public class FuzzyNode : IEquatable<FuzzyNode>
    {
        private string label;
        private List<FuzzyEdge> inEdges;
        private List<FuzzyEdge> outEdges;
        private List<float> durationsList;
        private int frequencySignificance;
        private Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> significantAttributes = new Dictionary<string, List<string>>();

        public FuzzyNode(string label)
        {
            this.label = label;
            inEdges = new List<FuzzyEdge>();
            outEdges = new List<FuzzyEdge>();
            this.durationsList = new List<float>();
            frequencySignificance = 0;
        }

        public string GetLabel()
        {
            return label;
        }

        public List<FuzzyEdge> GetInEdges()
        {
            return inEdges;
        }

        public List<FuzzyEdge> GetOutEdges()
        {
            return outEdges;
        }

        public List<float> GetDurationsList()
        {
            return durationsList;
        }

        public void AddDuration(float d)
        {
            durationsList.Add(d);
        }

        public int GetFrequencySignificance()
        {
            return frequencySignificance;
        }

        public Dictionary<string, List<string>> GetAttributes()
        {
            return attributes;
        }

        public Dictionary<string, List<string>> GetSignificantAttributes()
        {
            return significantAttributes;
        }

        public void AddInEdges(FuzzyEdge inEdge)
        {
            inEdges.Add(inEdge);
        }

        public void AddOutEdges(FuzzyEdge outEdge)
        {
            outEdges.Add(outEdge);
        }

        public void AddAttribute(string key, string value)
        {
            if (attributes.Keys.Contains<string>(key))
            {
                attributes[key].Add(value);
            }
            else
            {
                attributes.Add(key, new List<string>());
                attributes[key].Add(value);
            }
        }

        public void AddSignificantAttribute(string key, string value)
        {
            if (significantAttributes.Keys.Contains<string>(key))
            {
                significantAttributes[key].Add(value);
            }
            else
            {
                significantAttributes.Add(key, new List<string>());
                significantAttributes[key].Add(value);
            }
        }

        public void SetFrequencySignificance(int x)
        {
            frequencySignificance = x;
        }

        public void IncreaseFrequencySignificance()
        {
            frequencySignificance++;
        }

        public float ComputeRoutingSignificance()
        {
            float inValue = 0;
            float outValue = 0;

            foreach (FuzzyEdge fe in inEdges)
            {
                inValue += fe.GetFrequencySignificance();
            }
            foreach (FuzzyEdge fe in outEdges)
            {
                outValue += fe.GetFrequencySignificance();
            }

            float routingSignificance = Math.Abs(inValue - outValue) / (inValue + outValue);
            return routingSignificance;
        }

        public void RemoveEdge(FuzzyEdge fe)
        {
            //List<FuzzyEdge> toRemove = new List<FuzzyEdge>();
            //foreach (FuzzyEdge f in inEdges)
            //{
            //    if (f.Equals(fe))
            //    {
            //        toRemove.Add(f);
            //    }
            //}
            //foreach (FuzzyEdge fe in toRemove)
            //{
            //    inEdges.Remove(fe);
            //}
            //foreach (FuzzyEdge f in outEdges)
            //{
            //    if (f.Equals(fe))
            //    {
            //        toRemove.Add(f);
            //    }
            //}
            inEdges.Remove(fe);
            outEdges.Remove(fe);
        }

        public bool Equals(FuzzyNode fn)
        {
            return this.label == fn.label;
        }
    }

    public class FuzzyNodeCluster : FuzzyNode
    {
        private HashSet<FuzzyNode> nodesClustered = new HashSet<FuzzyNode>();

        public FuzzyNodeCluster(FuzzyNode fn, string label) : base(label)
        {
            nodesClustered.Add(fn);
            ComputeSignificance();
        }

        public HashSet<FuzzyNode> GetNodesInCluster()
        {
            return nodesClustered;
        }

        public void AddNodeToCluster(FuzzyNode fn)
        {
            nodesClustered.Add(fn);
            ComputeSignificance();
        }

        public void ComputeSignificance()
        {
            int frequencySignificance = 0;
            foreach (FuzzyNode fn in nodesClustered)
            {
                frequencySignificance += fn.GetFrequencySignificance();
            }
            SetFrequencySignificance(frequencySignificance / nodesClustered.Count);
        }
    }

    public class FuzzyEdge : IEquatable<FuzzyEdge>
    {
        private FuzzyNode fromNode;
        private FuzzyNode toNode;
        private List<float> durationsList;
        private int frequencySignificance;
        private float endpointCorrelation;
        private float proximityCorrelation;
        private float originatorCorrelation;

        public FuzzyEdge(FuzzyNode from, FuzzyNode to)
        {
            this.fromNode = from;
            this.toNode = to;
            this.durationsList = new List<float>();
            ComputeEndpointCorrelation();
            if (fromNode.GetAttributes().Keys.Contains<string>("time:timestamp") && toNode.GetAttributes().Keys.Contains<string>("time:timestamp"))
            {
                ComputeProximityCorrelation();
            }
            if (fromNode.GetAttributes().Keys.Contains<string>("org:resource") && toNode.GetAttributes().Keys.Contains<string>("org:resource"))
            {
                ComputeOriginatorCorrelation();
            }
        }

        public FuzzyNode GetFromNode()
        {
            return fromNode;
        }

        public FuzzyNode GetToNode()
        {
            return toNode;
        }

        public List<float> GetDurationsList()
        {
            return durationsList;
        }

        public void AddDuration(float d)
        {
            durationsList.Add(d);
        }

        public int GetFrequencySignificance()
        {
            return frequencySignificance;
        }

        public float GetEndpointCorrelation()
        {
            return endpointCorrelation;
        }

        public float GetProximityCorrelation()
        {
            return proximityCorrelation;
        }

        public float GetOriginatorCorrelation()
        {
            return originatorCorrelation;
        }

        public void IncreaseFrequencySignificance()
        {
            frequencySignificance++;
        }

        public float ComputeDistanceSignificance()
        {
            float fromSignificance = fromNode.GetFrequencySignificance();
            float toSignificance = toNode.GetFrequencySignificance();
            float distanceSignificance = 1 - Math.Abs((fromSignificance - frequencySignificance) + (toSignificance - frequencySignificance)) / (fromSignificance + toSignificance);

            return distanceSignificance;
        }

        public void ComputeEndpointCorrelation()
        {
            string fromName = fromNode.GetLabel();
            string toName = toNode.GetLabel();
            int stringDistance = Util.Similarity(fromName, toName);
            int maxLength = Math.Max(fromName.Length, toName.Length);
            endpointCorrelation = ((maxLength - stringDistance) / maxLength);
        }

        // TODO con quale timestamp si calcola?
        public void ComputeProximityCorrelation()
        {
            DateTime fromDate = new DateTime();
            DateTime toDate = new DateTime();
            if (fromNode is FuzzyNodeCluster)
            {
                FuzzyNodeCluster fcn = (FuzzyNodeCluster)fromNode;
                DateTime max = new DateTime();
                foreach (FuzzyNode fn in fcn.GetNodesInCluster())
                {
                    if (Convert.ToDateTime(fn.GetAttributes()["time:timestamp"][0]) > max)
                    {
                        max = Convert.ToDateTime(fn.GetAttributes()["time:timestamp"][0]);
                    }
                }
                fromDate = max;
            }
            else
            {
                fromDate = Convert.ToDateTime(fromNode.GetAttributes()["time:timestamp"][0]);
            }
            if (toNode is FuzzyNodeCluster)
            {
                FuzzyNodeCluster fcn = (FuzzyNodeCluster)toNode;
                DateTime min = new DateTime();
                foreach (FuzzyNode fn in fcn.GetNodesInCluster())
                {
                    if (Convert.ToDateTime(fn.GetAttributes()["time:timestamp"][0]) < min)
                    {
                        min = Convert.ToDateTime(fn.GetAttributes()["time:timestamp"][0]);
                    }
                }
                fromDate = min;
            }
            else
            {
                toDate = Convert.ToDateTime(toNode.GetAttributes()["time:timestamp"][0]);
            }
            if ((fromDate != null) && (toDate != null))
            {
                long timeFrom = fromDate.Ticks;
                long timeTo = toDate.Ticks;
                if (timeFrom != timeTo)
                {
                    proximityCorrelation = (1 / (timeFrom - timeTo));
                }
                else
                {
                    proximityCorrelation = 1;
                }
            }
            else
            {
                proximityCorrelation = 0;
            }
        }

        // TODO con quale valore tra le risorse si calcola?
        public void ComputeOriginatorCorrelation()
        {
            String fromOriginator = fromNode.GetAttributes()["org:resource"][0];
            String toOriginator = toNode.GetAttributes()["org:resource"][0];
            if ((fromOriginator == null) || (toNode == null))
            {
                originatorCorrelation = 0.0F;
            }
            float editDistance = Util.Similarity(fromOriginator, toOriginator);
            float maxLength = Math.Max(fromOriginator.Length, toOriginator.Length);
            originatorCorrelation = (maxLength - editDistance) / maxLength;
        }

        public void SetProximityCorrelation(float pc)
        {
            proximityCorrelation = pc;
        }

        public bool Equals(FuzzyEdge fe)
        {
            return (this.fromNode.Equals(fe.fromNode)) && (this.toNode.Equals(fe.toNode));
        }

        public override string ToString()
        {
            return fromNode.GetLabel() + " -> " + toNode.GetLabel();
        }
    }
}
