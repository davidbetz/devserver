/*
 * Copyright (c) 2008 Fritz Onion (http://www.pluralsight.com/fritz/)
 
Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:
 
The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.Xml;

namespace DevServer.WebCore.ViewState
{
    internal class ViewStateXmlBuilder
    {
        /*
         * 
         * Usage:
               XmlDocument document;
               LosFormatter formatter = new LosFormatter();
               XmlDocument dom = ViewStateXmlBuilder.BuildXml(formatter.Deserialize(viewState), out document);
         */

        internal static XmlDocument BuildXml(Object tree, out XmlDocument controlstateDom)
        {
            XmlDocument dom = new XmlDocument();
            controlstateDom = new XmlDocument();
            dom.AppendChild(dom.CreateElement("viewstate"));
            controlstateDom.AppendChild(controlstateDom.CreateElement("controlstate"));
            BuildElement(dom, dom.DocumentElement, tree, ref controlstateDom);
            return dom;
        }

        private static void BuildElement(XmlDocument dom, XmlElement elem, Object treeNode, ref XmlDocument controlstateDom)
        {
            if (treeNode != null)
            {
                XmlElement element;
                Type type = treeNode.GetType();
                if (type == typeof(Triplet))
                {
                    element = dom.CreateElement(GetShortTypename(treeNode));
                    elem.AppendChild(element);
                    BuildElement(dom, element, ((Triplet)treeNode).First, ref controlstateDom);
                    BuildElement(dom, element, ((Triplet)treeNode).Second, ref controlstateDom);
                    BuildElement(dom, element, ((Triplet)treeNode).Third, ref controlstateDom);
                }
                else if (type == typeof(Pair))
                {
                    element = dom.CreateElement(GetShortTypename(treeNode));
                    elem.AppendChild(element);
                    BuildElement(dom, element, ((Pair)treeNode).First, ref controlstateDom);
                    BuildElement(dom, element, ((Pair)treeNode).Second, ref controlstateDom);
                }
                else if (type == typeof(ArrayList))
                {
                    element = dom.CreateElement(GetShortTypename(treeNode));
                    elem.AppendChild(element);
                    foreach (Object obj2 in (ArrayList)treeNode)
                    {
                        BuildElement(dom, element, obj2, ref controlstateDom);
                    }
                }
                else if (treeNode is Array)
                {
                    element = dom.CreateElement("Array");
                    elem.AppendChild(element);
                    foreach (Object obj3 in (Array)treeNode)
                    {
                        BuildElement(dom, element, obj3, ref controlstateDom);
                    }
                }
                else if (treeNode is HybridDictionary)
                {
                    element = controlstateDom.CreateElement(GetShortTypename(treeNode));
                    controlstateDom.DocumentElement.AppendChild(element);
                    foreach (Object obj4 in (HybridDictionary)treeNode)
                    {
                        BuildElement(controlstateDom, element, obj4, ref controlstateDom);
                    }
                }
                else if (treeNode is DictionaryEntry)
                {
                    element = dom.CreateElement(GetShortTypename(treeNode));
                    elem.AppendChild(element);
                    DictionaryEntry entry = (DictionaryEntry)treeNode;
                    BuildElement(dom, element, entry.Key, ref controlstateDom);
                    DictionaryEntry entry2 = (DictionaryEntry)treeNode;
                    BuildElement(dom, element, entry2.Value, ref controlstateDom);
                }
                else
                {
                    element = dom.CreateElement(GetShortTypename(treeNode));
                    if (type == typeof(IndexedString))
                    {
                        element.InnerText = ((IndexedString)treeNode).Value;
                    }
                    else
                    {
                        element.InnerText = treeNode.ToString();
                    }
                    elem.AppendChild(element);
                }
            }
        }
        
        private static string GetShortTypename(Object obj)
        {
            string str = obj.GetType().ToString();
            return str.Substring(str.LastIndexOf(".") + 1);
        }
    }
}