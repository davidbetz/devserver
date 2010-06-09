using System;
using System.Collections.Generic;
using System.Configuration;
using DevServer.Configuration;
//+
namespace DevServer.Client
{
    internal static class ServerConfiguration
    {
        //- $FindServerConfiguration -//
        private static ServerElement FindServerConfiguration(String serverKey, ServerCollection servers)
        {
            try
            {
                return FindServerConfiguration(serverKey, servers, String.Empty);
            }
            catch (ConfigurationErrorsException)
            {
                throw new ConfigurationErrorsException(String.Format("Server '{0}' does not exist.", serverKey));
            }
        }

        //- $FindServerConfiguration -//
        private static ServerElement FindServerConfiguration(String serverKey, ServerCollection servers, String profileName)
        {
            String lowerCaseServerKey = serverKey.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            for (Int32 i = 0; i < servers.Count; i++)
            {
                if (servers[i].Key.ToLower(System.Globalization.CultureInfo.CurrentCulture) == lowerCaseServerKey)
                {
                    return servers[i];
                }
            }
            throw new ConfigurationErrorsException(String.Format("Server '{0}' referenced by profile '{1}' does not exist.", serverKey, profileName));
        }

        //- $PullServersFromActiveProfile -//
        private static List<ServerElement> PullServersFromActiveProfile(String activeProfile, DevServerConfigurationSection cs)
        {
            Int32 activeProfileIndex = -1;
            String lowerCaseActiveProfile = activeProfile.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            StartupProfileCollection profiles = cs.StartupProfiles;
            for (Int32 i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Name.ToLower(System.Globalization.CultureInfo.CurrentCulture) == lowerCaseActiveProfile)
                {
                    activeProfileIndex = i;
                    break;
                }
            }
            //+
            List<ServerElement> servers = new List<ServerElement>();
            if (activeProfileIndex > -1)
            {
                for (Int32 i = 0; i < profiles[activeProfileIndex].Count; i++)
                {
                    if (!String.IsNullOrEmpty(profiles[activeProfileIndex][i].Key))
                    {
                        servers.Add(FindServerConfiguration(profiles[activeProfileIndex][i].Key, cs.Servers, profiles[activeProfileIndex].Name));
                    }
                }
            }
            else
            {
                throw new ConfigurationErrorsException(String.Format("Profile '{0}' was not found.", activeProfile));
            }
            return servers;
        }

        //- ~ReadServerInstances -//
        internal static List<Instance> ReadServerInstances(CommandLineDictionary dictionary)
        {
            List<Instance> instances = new List<Instance>();
            DevServerConfigurationSection cs = DevServerConfigurationFacade.GetWebDevServerConfiguration();
            //+
            List<String> types = new List<String>();
            for (int i = 0; i < cs.RequestTracing.AllowedContentTypes.Count; i++)
            {
                types.Add(cs.RequestTracing.AllowedContentTypes[i].Value);
            }
            //+ default documents
            List<String> documents = new List<String>(new String[] { "default.aspx", "default.htm", "default.html" });
            for (int i = 0; i < cs.WebServer.DefaultDocuments.Count; i++)
            {
                if (!documents.Contains(cs.WebServer.DefaultDocuments[i].Name))
                {
                    documents.Add(cs.WebServer.DefaultDocuments[i].Name);
                }
            }
            //+ content type mappings
            Dictionary<String, String> contentTypeMappings = new Dictionary<String, String>();
            contentTypeMappings.Add(".bmp", "image/bmp");
            contentTypeMappings.Add(".css", "text/css");
            contentTypeMappings.Add(".gif", "image/gif");
            contentTypeMappings.Add(".ico", "image/x-icon");
            contentTypeMappings.Add(".htm", "text/html");
            contentTypeMappings.Add(".html", "text/html");
            contentTypeMappings.Add(".jpe", "image/jpeg");
            contentTypeMappings.Add(".jpeg", "image/jpeg");
            contentTypeMappings.Add(".jpg", "image/jpeg");
            contentTypeMappings.Add(".js", "text/javascript");
            for (int i = 0; i < cs.WebServer.ContentTypeMappings.Count; i++)
            {
                if (!contentTypeMappings.ContainsKey(cs.WebServer.ContentTypeMappings[i].Extension))
                {
                    contentTypeMappings.Add(cs.WebServer.ContentTypeMappings[i].Extension, cs.WebServer.ContentTypeMappings[i].Type);
                }
                else if (cs.WebServer.ContentTypeMappings[i].Override)
                {
                    contentTypeMappings.Remove(cs.WebServer.ContentTypeMappings[i].Extension);
                    contentTypeMappings.Add(cs.WebServer.ContentTypeMappings[i].Extension, cs.WebServer.ContentTypeMappings[i].Type);
                }
            }
            //+ check command line arguments
            Boolean usingProfile = false;
            Boolean usingSpecifiedServerKey = false;
            String activeProfile = String.Empty;
            String serverKey = String.Empty;
            if (dictionary.ContainsKey("activeProfile"))
            {
                activeProfile = dictionary["activeProfile"];
            }
            else if (dictionary.ContainsKey("serverKey"))
            {
                serverKey = dictionary["serverKey"];
            }
            else if (cs.StartupProfiles.Count > 0 && !String.IsNullOrEmpty(cs.StartupProfiles.ActiveProfile) && cs.StartupProfiles.ActiveProfile.ToLower(System.Globalization.CultureInfo.CurrentCulture) != "none")
            {
                activeProfile = cs.StartupProfiles.ActiveProfile;
            }
            List<ServerElement> servers = new List<ServerElement>();
            if (!String.IsNullOrEmpty(activeProfile) && activeProfile != "none")
            {
                servers = PullServersFromActiveProfile(activeProfile, cs);
            }
            //+
            if (servers.Count > 0)
            {
                usingProfile = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(serverKey))
                {
                    servers.Add(FindServerConfiguration(serverKey, cs.Servers));
                    usingSpecifiedServerKey = true;
                }
                else
                {
                    for (int i = 0; i < cs.Servers.Count; i++)
                    {
                        servers.Add(cs.Servers[i]);
                    }
                }
            }
            //+
            if (servers.Count > 0)
            {
                for (int i = 0; i < servers.Count; i++)
                {
                    if (!servers[i].Disabled || usingProfile || usingSpecifiedServerKey)
                    {
                        instances.Add(new Instance
                        {
                            Name = servers[i].Name,
                            Port = servers[i].Port,
                            VirtualPath = servers[i].VirtualPath,
                            PhysicalPath = servers[i].PhysicalPath,

                            BoundIPAddress = servers[i].Binding.Address,
                            EnableIPAddressBinding = !String.IsNullOrEmpty(servers[i].Binding.Address),

                            HostConfiguration = new HostConfiguration
                            {
                                AllowedContentTypes = types,
                                EnableFaviconTracing = servers[i].RequestTracing.EnableFaviconTracing,
                                EnableTracing = servers[i].RequestTracing.Enable,
                                EnableVerboseTypeTracing = servers[i].RequestTracing.EnableVerboseTypeTracing,
                                DefaultDocuments = documents,
                                ContentTypeMappings = contentTypeMappings
                            }
                        });
                    }
                }
            }
            return instances;
        }
    }
}