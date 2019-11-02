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

            string root = "start";
            fm.AddNode(root);

            foreach (XTrace xt in traces)
            {
                fm.GetNode(root).IncreaseFrequencySignificance();
                string previousEvent = root;
                string previousState = "";
                float eventDuration = 0;
                long previousTime = 0;
                foreach (XEvent xe in xt)
                {
                    // find event name
                    XAttributeMap xam = (XAttributeMap)xe.GetAttributes();
                    string currentEvent;
                    string currentState = xam["lifecycle:transition"].ToString();                    
                    long currentTime = 0;
                    if (xam.Keys.Contains<string>("time:timestamp"))
                    {
                        currentTime = Convert.ToDateTime(xam["time:timestamp"].ToString()).Ticks;
                    }
                    //if (xam.Keys.Contains<string>("lifecycle:transition"))
                    //{
                    //    currentEvent = xam["concept:name"].ToString() + " : " + xam["lifecycle:transition"].ToString();
                    //}
                    //else
                    //{
                        currentEvent = xam["concept:name"].ToString();
                    //}

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
                            if (previousEvent != "start")
                            {
                                fm.GetEdge(e).AddDuration(currentTime - previousTime);
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
                            if (previousEvent != "start")
                            {
                                fm.GetEdge(e).AddDuration(currentTime - previousTime);
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
                //TODO Add end node
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
