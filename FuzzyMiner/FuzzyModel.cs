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
    }

    public class FuzzyNode : IEquatable<FuzzyNode>
    {
        private string label;
        private List<FuzzyEdge> inEdges;
        private List<FuzzyEdge> outEdges;
        private List<double> durationsList;
        private int frequencySignificance;
        private int caseFrequencySignificance;
        private Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> significantAttributes = new Dictionary<string, List<string>>();

        public FuzzyNode(string label)
        {
            this.label = label;
            inEdges = new List<FuzzyEdge>();
            outEdges = new List<FuzzyEdge>();
            this.durationsList = new List<double>();
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

        public List<double> GetDurationsList()
        {
            return durationsList;
        }

        public void AddDuration(double d)
        {
            durationsList.Add(d);
        }

        public int GetFrequencySignificance()
        {
            return frequencySignificance;
        }

        public int GetCaseFrequencySignificance()
        {
            return caseFrequencySignificance;
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

        public void IncreaseCaseFrequencySignificance()
        {
            caseFrequencySignificance++;
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
        private List<double> durationsList;
        private int frequencySignificance;
        private int caseFrequencySignificance;
        private float endpointCorrelation;
        private float proximityCorrelation;
        private float originatorCorrelation;

        public FuzzyEdge(FuzzyNode from, FuzzyNode to)
        {
            this.fromNode = from;
            this.toNode = to;
            this.durationsList = new List<double>();
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

        public List<double> GetDurationsList()
        {
            return durationsList;
        }

        public void AddDuration(double d)
        {
            durationsList.Add(d);
        }

        public int GetFrequencySignificance()
        {
            return frequencySignificance;
        }

        public int GetCaseFrequencySignificance()
        {
            return caseFrequencySignificance;
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

        public void IncreaseCaseFrequencySignificance()
        {
            caseFrequencySignificance++;
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
