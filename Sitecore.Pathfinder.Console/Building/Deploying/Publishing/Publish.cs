namespace Sitecore.Pathfinder.Building.Deploying.Publishing
{
  using System;
  using System.ComponentModel.Composition;
  using System.Net;
  using System.Web;
  using Sitecore.Pathfinder.Diagnostics;

  [Export(typeof(ITask))]
  public class Publish : TaskBase
  {
    public Publish() : base("publish")
    {
    }

    public override void Execute(IBuildContext context)
    {
      if (!context.IsDeployable)
      {
        throw new BuildException(ConsoleTexts.Text3011);
      }

      context.Trace.TraceInformation(ConsoleTexts.Text1009);

      var hostName = context.Configuration.Get(Constants.HostName).TrimEnd('/');
      var publishUrl = context.Configuration.Get(Constants.PublishUrl).TrimStart('/');
      var url = hostName + "/" + publishUrl + HttpUtility.UrlEncode(context.Configuration.Get(Constants.Database));

      var webClient = new WebClient();
      try
      {
        var output = webClient.DownloadString(url).Trim();
        if (!string.IsNullOrEmpty(output))
        {
          context.Trace.Writeline(output);
        }
      }
      catch (Exception ex)
      {
        context.Trace.TraceError(ConsoleTexts.Text3008, ex.Message);
      }
    }
  }
}