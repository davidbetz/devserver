# NetFXHarmonics DevServer

**Copyright 2008 David Betz**

## Description

NetFXHarmonics DevServer is a web server hosting environment built on WPF and WCF technologies that allows multiple instances of Cassini-like web servers to run in parallel. DevServer also includes tracing capabilities for monitoring requests and responses, request filtering, automatic ViewState and ControlState parsing, visually enhanced HTTP status codes, IP binding modes for both local-only as well as remote access, and easy to use XML configuration. 

DevServer Announcement and Overview
[http://www.netfxharmonics.com/2008/04/netfxharmonics-devserver-released](http://www.netfxharmonics.com/2008/04/netfxharmonics-devserver-released)

Scott Hanselmen mention: [http://www.hanselman.com/blog/TheWeeklySourceCode23BigSolutionEdition.aspx](http://www.hanselman.com/blog/TheWeeklySourceCode23BigSolutionEdition.aspx)

![](https://github.com/davidbetz/devserver/raw/master/readme.png)


NetFXHarmonics DevServer is built on .NET 3.5 using WPF, WCF, and LINQ. DevServer could be used as a training tool to help teach WPF binding, WCF communication, LINQ collection transformation and querying, and .NET 2.0 custom configuration. See the "As a Training Tool" in the "DevServer Announcement and Overview" blog entry for more detailed information how using DevServer as a training tool.

## Getting Started

Download the code, compile, and modify the web site paths in your app.config in the DevServer.Client project. Make sure you change app.config to point to the correct web site physical paths. You may also need to adjust the path to gacutil.exe in your Post Build Events in DevServer.Service, DevServer.ServiceImpl, and DevServer.WebCore.

# Documentation

*(following was written circa 2008 by David Betz; initially at [netfxharmonics.com](https://netfxharmonics.com/2008/04/netfxharmonics-devserver-released))*

Using this development server, I am able to simultaneously start multiple web sites to very quickly view everything that happens over the wire and therefore easily debug JSON and SOAP messages flying back and forth between client and server and between services.  This tool have been a tremendous help for me in the past few months to discover exactly why my services are tripping out without having to enable WCF tracing.  It's also been a tremendous help in managing my own web development server instances for all my projects, each having 3-5 web sites (or segregated service endpoints) each.

Let me give you a quick run down of the various features in NetFXHarmonics DevServer with a little discussion of each feature's usage:

## XML Configuration
NetFXHarmonics DevServer has various projects (and therefore assemblies) with the primary being DevServer.Client, the client application which houses the application's configuration.

In the app.config of DevServer.Client, you have a structure that looks something like the following:
	
	<jampad.devServer>
	</jampad.devServer>
This is where all your configuration lives and the various parts of this will be explained in their appropriate contexts in the discussions that follow.

## Multiple Web Site Hosting
In side of the jampad.devServer configuration section in the app.config file, there is a branch called <servers /> which allows you to declare the various web servers you would like to load.  This is all that's required to configure servers.  Each server requires a friendly name, a port, a virtual path, and the physical path.  Given this information, DevServer will know how to load your particular servers.
	
	<servers>
	  <server key="SampleWS1" name="Sample Website 1" port="2001"
	          virtualPath="/" physicalPath="C:\Project\DevServer\SampleWebsite1">
	  </server>
	  <server key="SampleWS2" name="Sample Website 2" disabled="true" port="2003"
	          virtualPath="/" physicalPath="C:\Project\DevServer\SampleWebsite2">
	  </server>
	</servers>
If you want to disable a specific server from loading, use the "disabled" attribute.  All disabled servers will be completely skipped in the loading process.  On the other hand, if you would like to load a single server, you can actually do this from the command line by setting a server key on the <server /> element and by accessing it via a command line argument:
	
	DevServer.Client.exe -serverKey:SampleWS1

In most scenarios you will probably want to load various sets of servers at once.  This is especially true in properly architected service-oriented solutions.  Thus, DevServer includes a concept of startup profiles.  Each profile will include links to a number of keyed servers.  You configure these startup profiles in the `<startupProfiles />` section.
	
	<startupProfiles activeProfile="Sample">
	  <profile name="Sample">
	    <server key="SampleWS1" />
	    <server key="SampleWS2" />
	  </profile>
	</startupProfiles>

This configuration block lives parallel to the <servers /> block and the inclusion of servers should be fairly self-explanatory.  When DevServer starts it will load the profile in the `activeProfile` attribute.  If the `activeProfile` block is missing, it will be ignored.  If the `activeProfile` states a profile that does not exist, `DevServer` will not load.  When using a startup profile, the "disabled" attribute on each server instance is ignored.  That attribute is only for non-startup profile usage.  An `activeProfile` may also be set via command line:
	
	DevServer.Client.exe -activeProfile:Sample

This will override any setting in the `activeProfile` attribute of `<startupProfiles/>`.  In fact, the `serverKey` command line argument overrides the `activeProfile` attribute of `<startupProfiles />` as well.  Therefore, the order of priority is is as follows: **command line argument override profile configuration and profile configuration overrides the "disabled" attribute.**

Most developers don't work on one project and with only client.  Or, even if they do, they surely have their own projects as well.  Therefore, you may have even more servers in your configuration:
	
	<server key="ABCCorpMainWS" name="Main Website" port="7001"
	        virtualPath="/" physicalPath="C:\Project\ABCCorp\Website">
	</server>
	<server key="ABCCorpKBService" name="KB Service" port="7003"
	        virtualPath="/" physicalPath="C:\Project\ABCCorp\KnowledgeBaseService">
	</server>
	<server key="ABCCorpProductService" name="Product Service" port="7005"
	        virtualPath="/" physicalPath="C:\Project\ABCCorp\ProductService">
	</server>
These would be grouped together in their own profile with the `activeProfile` set to that profile.
	
	<startupProfiles activeProfile="ABCCorp">
	  <profile name="ABCCorp">
	    <server key="ABCCorpMainWS" />
	    <server key="ABCCorpKBService" />
	    <server key="ABCCorpProductService" />
	  </profile>
	  <profile name="Sample">
	    <server key="SampleWS1" />
	    <server key="SampleWS2" />
	  </profile>
	</startupProfiles>
What about loading servers from different profiles?  Well, think about it... that's a different profile:
	
	<startupProfiles activeProfile="ABCCorpWithSampleWS1">
	  <profile name="ABCCorpWithSampleWS1">
	    <server key="SampleWS1" />
	    <server key="ABCCorpMainWS" />
	    <server key="ABCCorpKBService" />
	    <server key="ABCCorpProductService" />
	  </profile>
	</startupProfiles>
One of the original purposes of DevServer was to allow remote non-IIS access to development web sites.  Therefore, in DevServer you can use the <binding /> configuration element to set either "loopback" (or "localhost") to only allow access to your machine, "any" to allow web access from all addresses, or you can specific a specific IP address to bind the web server to a single IP address so that only systems with access to that IP on that interface can access the web site.

In the following example the first web site is only accessible by the local machine and the second is accessible by others.  This comes in handy for both testing in a virtual machine as well as quickly doing demos.  If your evil project manager (forgive the redundancy) wants to see something, bring the web site up on all interface and he can poke around from his desk and then have all his complains and irrational demands ready when he comes to your desk (maybe you want to keep this feature secret).
	
	<server key="SampleWS1" name="Sample Website 1" port="2001"
	        virtualPath="/" physicalPath="C:\Project\DevServer\SampleWebsite1">
	  <binding address="loopback" />
	</server>
	<server key="SampleWS2" name="Sample Website 2" port="2003"
	        virtualPath="/" physicalPath="C:\Project\DevServer\SampleWebsite2">
	  <binding address="any" />
	</server>

## Web Site Settings

In addition to server configuration, there is also a bit of general configuration that apply to all instances.  As you can see from the following example, you can add default documents to the existing defaults and you can also setup content type mappings.  A few content types already exist, but you can override as the example shows.  In this example, where ".js" is normally sent as text/javascript, you can override it to go to "application/x-javascript" or to something else.
	
	<webServer>
	  <defaultDocuments>
	    <add name="index.jsx" />
	  </defaultDocuments>
	  <contentTypeMappings>
	    <add extension=".jsx" type="application/x-javascript" />
	    <add extension=".js" type="application/x-javascript" override="true" />
	  </contentTypeMappings>
	</webServer>

## Request/Response Tracing

One of the core features of DevServer is the ability to do tracing on the traffic in each server.  Tracing is enabled by adding a `<requestTracing />` configuration element to a server and setting the "enabled" attribute to true.
	
	<server key="SampleWS1" name="Sample Website 1" port="2001"
	        virtualPath="/" physicalPath="C:\Project\DevServer\SampleWebsite1">
	  <binding address="loopback" />
	  <requestTracing enabled="true" enableVerboseTypeTracing="false" enableFaviconTracing="true" />
	</server>
This will have request/response messages show up in DevServer which will allow you to view status code, date/time, URL, POST data (if any), response data, request headers, response headers, as well as parsed ViewState and Control state for both the request and response.  In addition, each entry is color coded based on it's status code.  Different colors will show for 301/302, 500+, and 404.

image

When working with the web, you don't always want to see every little thing that happens all the time.  Therefore, by default, you only trace common text specific file like HTML, CSS, JavaScript, JSON, XAML, Text, and SOAP and their content.  If you want to trace images and other things going across, then set `enableVerboseTypeTracing` to true.  However, since there is no need to see the big blob image data, the data of binary types are not sent to the trace viewer even with `enableVerboseTypeTracing`.  You can also toggle both tracing as well as verbose type tracing on each server as each is running.

There's also the ability to view custom content types without seeing all the images and extra types.  This is the purpose of the `<allowedConntetTypes />` configuration block under `<requestTracing />`, which is parallel to `<servers />`.
	
	<requestTracing>
	  <allowedContentTypes>
	    <add value="application/x-custom-type" />
	  </allowedContentTypes>
	</requestTracing>
In this case, responses of content-type "application/x-custom-type" are also traced without needing to turn on verbose type tracing.

However, there is another way to control this information.  If you want to see all requests, but want the runtime ability to see various content types, then you can use a client-side filter in the request/response list.  In the box immediately above the request/response list, you can type something like the following:
	
	verb:POST;statuscode:200;file:css;contentType:text/css

Filtering will occur as you type, allowing you to find the particular request you are looking for.  The filter is NOT case sensitive.  You can also clear the request/response list with the clear button.  There is also the ability to copy/paste the particular headers that you want from the headers list by using typical SHIFT (for range) and CTRL-clicking (for single choosing).

Request/Response monitoring actually goes a bit further by automatically parsing both `ViewState` and `ControlState` for both request (POST) and response data.

*Thanks goes to Fritz Onion for granting me permission to use his ViewState parser class in DevServer.*

## Internals

DevServer relies heavily on WCF for all inner-process communication via named-pipes.  The web servers are actually hosted inside of a WCF service, thus segregating the web server loader from the client application in a very SOA friendly manner.  The client application loads the service and then acts as a client to the service calling on it to start, stop, and kill server instances.  WCF is also used to communicate the HTTP requests inside the web server back to the client, which is itself a WCF service to which the HTTP request is a client.  Therefore, DevServer is an example of how you can use WCF to communicate between AppDomains.

The entire interface in DevServer is a WPF application that relies heavy on WPF binding for all visual information.  All status information is in a collection to which WPF binds.  Not only that all, but all request/response information is also in a collection.  WPF simply binds to the data.  Using WPF, no eventhandling was required to say "on a click event, obtain `SelectedIndex`, pull data, then text these TextBox instances".  In WPF, you simply have normal every day data and WPF controls bind directly to that data being automatically updated via special interfaces (i.e `INotifyPropertyChanged` and `INotifyCollectionChanged`) or the special generic `ObservableCollection<T>`.

Since the bindings are completely automated, there also needs to be ways to "transform" data.  For example, in the TabItem header I have a little green or red icon showing the status of that particular web server instance.  There was no need to handle this manually.  There is already a property on my web server instance that has a status.  All I need to do is bind the image to my status enumeration and set a `TypeConverter` which transforms the enumeration value to a specific icon.  When the enumeration is set to Started, the icon is green, when it says "Stopped", the icon is red.  No events are required and the only code required for this scenario is the quick creation of a `TypeConverter`.
