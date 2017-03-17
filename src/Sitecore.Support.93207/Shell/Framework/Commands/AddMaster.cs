using Sitecore.Buckets.Managers;
using Sitecore.Configuration;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Sitecore.Support.Shell.Framework.Commands
{
  [Serializable]
  public class AddMaster : Command
  {
    [field: CompilerGenerated]
    protected event ItemCreatedDelegate ItemCreated;

    protected void Add(ClientPipelineArgs args)
    {
      if (SheerResponse.CheckModified())
      {
        Item item = Context.ContentDatabase.GetItem(args.Parameters["Master"]);
        if (item == null)
        {
          object[] parameters = new object[] { args.Parameters["Master"] };
          SheerResponse.Alert(Translate.Text("Branch \"{0}\" not found.", parameters), new string[0]);
        }
        else if (item.TemplateID == TemplateIDs.CommandMaster)
        {
          string str = item["Command"];
          if (!string.IsNullOrEmpty(str))
          {
            Context.ClientPage.SendMessage(this, str);
          }
        }
        else if (args.IsPostBack)
        {
          if (args.HasResult)
          {
            string[] values = new string[] { args.Parameters["ItemID"] };
            string str2 = StringUtil.GetString(values);
            string[] textArray2 = new string[] { args.Parameters["Language"] };
            string name = StringUtil.GetString(textArray2);
            Item parent = Context.ContentDatabase.Items[str2, Language.Parse(name)];
            if (parent == null)
            {
              SheerResponse.Alert("Parent item not found.", new string[0]);
            }
            else if (!parent.Access.CanCreate())
            {
              Context.ClientPage.ClientResponse.Alert("You do not have permission to create items here.");
            }
            else
            {
              Item item3 = null;
              try
              {
                if (item.TemplateID == TemplateIDs.BranchTemplate)
                {
                  BranchItem branch = item;
                  item3 = Context.Workflow.AddItem(args.Result, branch, parent);
                  BucketManager.Sync(item3.Parent);
                }
                else
                {
                  TemplateItem template = item;
                  item3 = Context.Workflow.AddItem(args.Result, template, parent);
                }
              }
              catch (WorkflowException exception)
              {
                Log.Error("Workflow error: could not add item from master", exception, this);
                SheerResponse.Alert(exception.Message, new string[0]);
              }
              if ((item3 != null) && (this.ItemCreated != null))
              {
                this.ItemCreated(this, new ItemCreatedEventArgs(item3));
              }
            }
          }
        }
        else
        {
          SheerResponse.Input("Enter a name for the new item:", item.DisplayName, Settings.ItemNameValidation, "'$Input' is not a valid name.", Settings.MaxItemNameLength);
          args.WaitForPostBack();
        }
      }
    }

    public override void Execute(CommandContext context)
    {
      if ((context.Items.Length == 1) && context.Items[0].Access.CanCreate())
      {
        Item item = context.Items[0];
        NameValueCollection collection1 = new NameValueCollection();
        collection1["Master"] = context.Parameters["master"];
        collection1["ItemID"] = item.ID.ToString();
        collection1["Language"] = item.Language.ToString();
        collection1["Version"] = item.Version.ToString();
        NameValueCollection parameters = collection1;
        Context.ClientPage.Start(this, "Add", parameters);
      }
    }

    public override CommandState QueryState(CommandContext context)
    {
      Error.AssertObject(context, "context");
      if (context.Items.Length != 1)
      {
        return CommandState.Hidden;
      }
      if (!context.Items[0].Access.CanCreate())
      {
        return CommandState.Disabled;
      }
      return base.QueryState(context);
    }
  }
}