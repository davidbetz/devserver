using System.Configuration;
//+
namespace DevServer.Configuration
{
    public static class DevServerConfigurationFacade
    {
        private static DevServerConfigurationSection cachedConfiguration;

        //- @GetWebDevServerConfiguration -//
        public static DevServerConfigurationSection GetWebDevServerConfiguration()
        {
            if (cachedConfiguration == null)
            {
                cachedConfiguration = (DevServerConfigurationSection)ConfigurationManager.GetSection("jampad.devServer");
            }
            return cachedConfiguration;
        }
    }
}