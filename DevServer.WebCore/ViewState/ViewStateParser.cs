using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Xml;

namespace DevServer.WebCore.ViewState
{
    internal static class ViewStateParser
    {
        internal static String GetViewStateDataFromTree(XmlDocument viewStateTree)
        {
            StringBuilder b = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(b);
            viewStateTree.WriteTo(xmlWriter);
            return b.ToString();
        }

        internal static State ExtractViewStateTree(String data)
        {
            //+ WebForm ViewState
            Regex regex = new Regex("name=\"__VIEWSTATE\" id=\"__VIEWSTATE\" value=\"(?<vsdata>[=/+%a-z0-9_]+)\"", RegexOptions.IgnoreCase);
            Match m = regex.Match(data);
            State stateTrees = null;
            if (m.Groups.Count > 1)
            {
                stateTrees = PullParsedViewStateData(m.Groups["vsdata"].Value);
            }
            else
            {
                //+ Postback and Callback Viewstate
                regex = new Regex("__VIEWSTATE=(?<vsdata>[+%a-z0-9_]+)", RegexOptions.IgnoreCase);
                m = regex.Match(data);
                if (m.Groups.Count > 1)
                {
                    stateTrees = PullParsedViewStateData(m.Groups["vsdata"].Value);
                }
            }
            return stateTrees;
        }

        private static State PullParsedViewStateData(String data)
        {
            XmlDocument controlState = new XmlDocument();
            XmlDocument viewState = new XmlDocument();
            //+
            String stateData = data.Replace("%2B", "+").Replace("%2F", "/").Replace("%3D", "=");
            LosFormatter formatter = new LosFormatter();
            Object unformattedViewState = formatter.Deserialize(stateData);
            try
            {
                viewState = ViewStateXmlBuilder.BuildXml(unformattedViewState, out controlState);
            }
            catch
            {
                //++
                //+ if this doesn't work for some reason, there's NO reason
                //+ to kill the entire application.  the feature isn't THAT
                //+ important.
                //++ 
            }
            //+
            return new State
            {
                 ViewState = viewState,
                 ControlState = controlState
            };
        }
    }
}