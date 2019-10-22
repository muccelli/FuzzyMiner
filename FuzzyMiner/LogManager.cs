using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenXesNet.io;
using OpenXesNet.model;
using FuzzyMinerModel;

namespace IO
{
    static class LogManager
    {
        public static FuzzyModel parseLogFile(string file)
        {
            FuzzyModel fm = new FuzzyModel();
            List<XTrace> traces = getTracesXES(file);

            foreach (XTrace xt in traces)
            {
                //XAttributeMap xamRoot = (XAttributeMap)xt[0].GetAttributes();
                //string traceRoot;
                //if (xamRoot.Keys.Contains<string>("lifecycle:transition"))
                //{
                //    traceRoot = xamRoot["concept:name"].ToString() + " : " + xamRoot["lifecycle:transition"].ToString();
                //}
                //else
                //{
                //    traceRoot = xamRoot["concept:name"].ToString();
                //}

                string previousEvent = null;
                foreach (XEvent xe in xt)
                {
                    // find event name
                    XAttributeMap xam = (XAttributeMap)xe.GetAttributes();
                    string currentEvent;
                    if (xam.Keys.Contains<string>("lifecycle:transition"))
                    {
                        currentEvent = xam["concept:name"].ToString() + " : " + xam["lifecycle:transition"].ToString();
                    }
                    else
                    {
                        currentEvent = xam["concept:name"].ToString();
                    }
                    
                    // if the event is new, add it to the graph
                    if (!fm.GetEvents().Contains(currentEvent))
                    {
                        FuzzyNode node = new FuzzyNode(currentEvent);
                        fm.AddNode(node);

                        // if it is not the first event in the trace, add edges
                        if (previousEvent != null)
                        {
                            FuzzyEdge e = new FuzzyEdge(fm.GetNode(previousEvent), node);
                            // if the edge is new add it to the list
                            if (!fm.GetNode(previousEvent).GetOutEdges().Contains(e))
                            {
                                fm.AddEdge(e);
                            }
                            fm.GetEdge(e).IncreaseFrequencySignificance();
                        }
                    }
                    else
                    {
                        if (previousEvent != null)
                        {
                            FuzzyEdge e = new FuzzyEdge(fm.GetNode(previousEvent), fm.GetNode(currentEvent));
                            if (!fm.GetNode(previousEvent).GetOutEdges().Contains(e))
                            {
                                fm.AddEdge(e);
                            }
                            fm.GetEdge(e).IncreaseFrequencySignificance();
                        }
                    }

                    foreach (string key in xam.Keys)
                    {
                        if (key != "concept:name" && key != "lifecycle:transition")
                        {
                            if (key != "time:timestamp" && key.IndexOf("id", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                fm.GetNode(currentEvent).AddSignificantAttribute(key, xam[key].ToString());
                            }
                            fm.GetNode(currentEvent).AddAttribute(key, xam[key].ToString());
                        }
                    }
                    fm.GetNode(currentEvent).IncreaseFrequencySignificance();
                    previousEvent = currentEvent;
                }
            }
            return fm;
        }

        private static XLog getLogsXES(string file)
        {
            XesXmlParser parser = new XesXmlParser();
            XLog xLog = null;

            xLog = (XLog)parser.Parse(file);

            return xLog;
        }

        public static List<XTrace> getTracesXES(string file)
        {
            XLog xLog = getLogsXES(file);
            List<XTrace> traces = new List<XTrace>();
            
            foreach (XTrace xt in xLog)
            {
                traces.Add(xt);
            }

            return traces;
        }
    }
}
