namespace Sitecore.Support.Shell.Framework.Pipelines
{

  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Shell.Framework;
  using Sitecore.Text;
  using Sitecore.Web.UI.Sheer;
  using System;

  public class AddFromTemplate
  {
    // Methods
    protected virtual Item AddItemFromBranch(string name, BranchItem branch, Item parent) =>
        Context.Workflow.AddItem(name, branch, parent);

    protected virtual Item AddItemFromTemplate(string name, TemplateItem template, Item parent) =>
        Context.Workflow.AddItem(name, template, parent);

    protected virtual void AuditMessage(object owner, string format, params string[] parameters)
    {
      Log.Audit(owner, format, parameters);
    }

    public void Execute(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (args.HasResult)
      {
        int index = args.Result.IndexOf(',');
        Assert.IsTrue(index >= 0, "Invalid return value from dialog");
        string path = StringUtil.Left(args.Result, index);
        string name = StringUtil.Mid(args.Result, index + 1);
        Database database = this.GetDatabase(args.Parameters["database"]);
        string str3 = args.Parameters["id"];
        string str4 = args.Parameters["language"];
        Item parent = database.GetItem(str3, Language.Parse(str4));
        if (parent == null)
        {
          SheerResponse.Alert("Parent item not found.", Array.Empty<string>());
          args.AbortPipeline();
        }
        else if (!parent.Access.CanCreate())
        {
          SheerResponse.Alert("You do not have permission to create items here.", Array.Empty<string>());
          args.AbortPipeline();
        }
        else
        {
          Item item = database.GetItem(path, Language.Parse(str4));
          if (item == null)
          {
            SheerResponse.Alert("Item not found.", Array.Empty<string>());
            args.AbortPipeline();
          }
          else
          {
            Item item3;
            this.SetHistoryTemplate(item.ID);
            if (item.TemplateID == TemplateIDs.Template)
            {
              string[] parameters = new string[] { AuditFormatter.FormatItem(item) };
              this.AuditMessage(this, "Add from template: {0}", parameters);
              TemplateItem template = item;
              item3 = this.AddItemFromTemplate(name, template, parent);
            }
            else
            {
              string[] parameters = new string[] { AuditFormatter.FormatItem(item) };
              this.AuditMessage(this, "Add from branch: {0}", parameters);
              BranchItem branch = item;
              item3 = this.AddItemFromBranch(name, branch, parent);
            }
            args.CarryResultToNextProcessor = true;
            if (item3 == null)
            {
              args.AbortPipeline();
            }
            else
            {
              args.Result = item3.ID.ToString();
            }
          }
        }
      }
    }

    protected virtual Database GetDatabase(string databaseName)
    {
      Assert.ArgumentNotNullOrEmpty(databaseName, "databaseName");
      return Factory.GetDatabase(databaseName);
    }

    public void GetTemplate(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (SheerResponse.CheckModified())
      {
        UrlString str = new UrlString(UIUtil.GetUri("control:AddFromTemplate"));
        string template = History.Template;
        if ((template != null) && (template.Length > 0))
        {
          str.Append("fo", template);
        }
        Context.ClientPage.ClientResponse.ShowModalDialog(str.ToString(), "1200px", "700px", "", true);
        args.WaitForPostBack(false);
      }
    }

    protected virtual void SetHistoryTemplate(ID templateId)
    {
      History.Template = templateId.ToString();
    }
  }

}