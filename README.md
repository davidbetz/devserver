# NetFXHarmonics DevServer

**Copyright 2008 David Betz**

## Description

NetFXHarmonics DevServer is a web server hosting environment built on WPF and WCF technologies that allows multiple instances of Cassini-like web servers to run in parallel. DevServer also includes tracing capabilities for monitoring requests and responses, request filtering, automatic ViewState and ControlState parsing, visually enhanced HTTP status codes, IP binding modes for both local-only as well as remote access, and easy to use XML configuration. 

DevServer Announcement and Overview
[http://www.netfxharmonics.com/2008/04/netfxharmonics-devserver-released](http://www.netfxharmonics.com/2008/04/netfxharmonics-devserver-released)

Scott Hanselmen mention: [http://www.hanselman.com/blog/TheWeeklySourceCode23BigSolutionEdition.aspx](http://www.hanselman.com/blog/TheWeeklySourceCode23BigSolutionEdition.aspx)

![](https://github.com/davidbetz/devserver/raw/master/readme.png)

## As a Training Tool

NetFXHarmonics DevServer is built on .NET 3.5 using WPF, WCF, and LINQ. DevServer could be used as a training tool to help teach WPF binding, WCF communication, LINQ collection transformation and querying, and .NET 2.0 custom configuration. See the "As a Training Tool" in the "DevServer Announcement and Overview" blog entry for more detailed information how using DevServer as a training tool.

## Instructions

Download the code, compile, and modify the web site paths in your app.config in the DevServer.Client project. Make sure you change app.config to point to the correct web site physical paths. You may also need to adjust the path to gacutil.exe in your Post Build Events in DevServer.Service, DevServer.ServiceImpl, and DevServer.WebCore.
