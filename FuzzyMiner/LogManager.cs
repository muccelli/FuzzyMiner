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

            string root = "start_node";
            string end = "end_node";
            fm.AddNode(root);

            foreach (XTrace xt in traces)
            {
                fm.GetNode(root).IncreaseFrequencySignificance();
                var xamTrace = xt.GetAttributes();
                foreach (string key in xamTrace.Keys)
                {
                    if (key != "concept:name")
                    {
                        fm.GetNode(root).AddSignificantAttribute(key, xamTrace[key].ToString());
                    }
                }
                string previousEvent = root;
                string previousState = "";
                double eventDuration = 0;
                double traceDuration = 0;
                double previousTime = 0;
                foreach (XEvent xe in xt)
                {
                    // find event name
                    XAttributeMap xam = (XAttributeMap)xe.GetAttributes();
                    string currentEvent;
                    string currentState = "";
                    if (xam.Keys.Contains<string>("lifecycle:transition"))
                    {
                        currentState = xam["lifecycle:transition"].ToString();
                    }
                    double currentTime = 0;
                    if (xam.Keys.Contains<string>("time:timestamp"))
                    {
                        currentTime = Convert.ToDateTime(xam["time:timestamp"].ToString()).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    }
                    currentEvent = xam["concept:name"].ToString();
                    
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
                            // if it's not the start node, compute the duration of the transition
                            if (previousEvent != "start_node")
                            {
                                fm.GetEdge(e).AddDuration(currentTime - previousTime);
                                traceDuration += currentTime - previousTime;
                            }
                            fm.GetEdge(e).IncreaseFrequencySignificance();
                        }
                        fm.GetNode(currentEvent).IncreaseFrequencySignificance();
                    }
                    else
                    {
                        // if it is not the first event in the trace, add edges
                        if (previousEvent != null)
                        {
                            FuzzyEdge e = new FuzzyEdge(fm.GetNode(previousEvent), fm.GetNode(currentEvent));
                            // if the edge is new add it to the list
                            if (!fm.GetNode(previousEvent).GetOutEdges().Contains(e))
                            {
                                fm.AddEdge(e);
                            }
                            // if it's not the start node, compute the duration of the transition
                            if (previousEvent != "start_node")
                            {
                                fm.GetEdge(e).AddDuration(currentTime - previousTime);
                                traceDuration += currentTime - previousTime;
                            }
                            fm.GetEdge(e).IncreaseFrequencySignificance();
                        }
                        // if the event is the same but the state is different, compute the event duration
                        if (previousEvent == currentEvent && previousState != currentState)
                        {
                            eventDuration += currentTime - previousTime;
                        }
                        else
                        {
                            fm.GetNode(currentEvent).IncreaseFrequencySignificance();
                        }
                    }
                    // if the event is complete add its duration to the node
                    if (currentState == "complete")
                    {
                        fm.GetNode(currentEvent).AddDuration(eventDuration);
                        traceDuration += eventDuration;
                        eventDuration = 0;
                    }
                    // add the event attributes to the node
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
                    previousEvent = currentEvent;
                    previousState = currentState;
                    previousTime = currentTime;
                }
                if (!fm.GetEvents().Contains(end))
                {
                    fm.AddNode(end);
                    fm.GetNode(end).IncreaseFrequencySignificance();
                }
                else
                {
                    fm.GetNode(end).IncreaseFrequencySignificance();
                }
                FuzzyEdge fe = new FuzzyEdge(fm.GetNode(previousEvent), fm.GetNode(end));
                if (!fm.GetEdges().Contains(fe))
                {
                    fm.AddEdge(fe);
                    fm.GetEdge(fe).IncreaseFrequencySignificance();
                }
                else
                {
                    fm.GetEdge(fe).IncreaseFrequencySignificance();
                }

                fm.GetNode(root).AddDuration(traceDuration);
            }

            foreach (XTrace xt in traces)
            {
                foreach (FuzzyNode fn in fm.GetNodes())
                {
                    foreach (XEvent xe in xt)
                    {
                        if (xe.GetAttributes()["concept:name"].ToString().Equals(fn.GetLabel()))
                        {
                            fn.IncreaseCaseFrequencySignificance();
                            break;
                        }
                    }
                }
                foreach (FuzzyEdge fe in fm.GetEdges())
                {
                    string previousEvent = "";
                    foreach (XEvent xe in xt)
                    {
                        string currentEvent = xe.GetAttributes()["concept:name"].ToString();
                        if (previousEvent.Equals(fe.GetFromNode().GetLabel()) && currentEvent.Equals(fe.GetToNode().GetLabel()))
                        {
                            fe.IncreaseCaseFrequencySignificance();
                            break;
                        }
                        previousEvent = currentEvent;
                    }
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
