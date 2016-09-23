Assembly ASP.NET OAuth Example
==============================

Hi there. This repository is a really simple example of how to build a basic OAuth flow. The Assembly Platform uses OAuth tokens to protect school data and your application will, therefore, need to implement the Assembly Platform OAuth flow in order to let schools see to which of their data your application is requesting access.

We've created this repo because first time users of OAuth are often unsure about how to implement the necessary steps and browser testing really isn't the best way to kick the tyres on the Assembly Platform's OAuth because we need to call your appliction back once the school (or, in this case, probably your test school) has approved your application for access to their data.

*IMPORTANT NOTE ABOUT MIDDLEWARE:* It's important to note that we're using the standard OAuth middleware here for demonstration purposes. This middleware is really intended for user authentication delegation to OAuth enabled sites like Google, Facebook and Twitter. As such the middleware uses end-points like `/login` and `/logout` as standard so you may find you want to build a custom middleware to best suit the needs of your application.

Up and Running
--------------
To begin with you're going to need a working ASP.NET. I'm sure there are many way to achieve that and we're not going to try and tell you how you should/how best to do that.

In order to make this sample we used .NET Core. You can install that by following [this page](https://www.microsoft.com/net/core#windows) if you want to.

Also, before you get started on this you'll need to create an application on the Assembly Platform sandbox environment. To do that read [this](http://help.assembly.education/article/38-signing-up-to-the-platform).

Once you've successfully finished the steps above you can clone this repo and run:

	cd oauth-sample-aspcore
	dotnet restore

After the necessary dependencies have been restored you'll need to substitute your Assembly App's ID and secret into `appsettings.json`. Then you can run:

	dotnet run
  
And visit the sample in your browser at [http://localhost:5000](http://localhost:5000)
