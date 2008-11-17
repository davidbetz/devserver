using System.Xml;
//+
namespace DevServer.WebCore.ViewState
{
    internal class State
    {
        //- ~ViewState -//
        internal XmlDocument ViewState { get; set; }

        //- ~ControlState -//
        internal XmlDocument ControlState { get; set; }
    }
}