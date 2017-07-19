using Sitecore.Buckets.Extensions;
using Sitecore.Buckets.Managers;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Web.UI.Sheer;
using Sitecore.Shell.Framework.Commands;

namespace Sitecore.Support.Shell.Framework.Commands
{
    /// <summary>
    /// Represents the AddMaster command.
    /// </summary>
    [System.Serializable]
    public class AddMaster : Command
    {
        /// <summary>
        /// Executes the command in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length == 1 && context.Items[0].Access.CanCreate())
            {
                Item item = context.Items[0];
                System.Collections.Specialized.NameValueCollection nameValueCollection = new System.Collections.Specialized.NameValueCollection();
                nameValueCollection["Master"] = context.Parameters["master"];
                nameValueCollection["ItemID"] = item.ID.ToString();
                nameValueCollection["Language"] = item.Language.ToString();
                nameValueCollection["Version"] = item.Version.ToString();
                Context.ClientPage.Start(this, "Add", nameValueCollection);
            }
        }

        /// <summary>
        /// Queries the state of the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The state of the command.</returns>
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

        /// <summary>
        /// Adds the specified args.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void Add(ClientPipelineArgs args)
        {
            if (!SheerResponse.CheckModified())
            {
                return;
            }
            Item item = Context.ContentDatabase.GetItem(args.Parameters["Master"]);
            if (item == null)
            {
                SheerResponse.Alert(Translate.Text("Branch \"{0}\" not found.", new object[]
                {
                    args.Parameters["Master"]
                }), new string[0]);
                return;
            }
            if (!(item.TemplateID == TemplateIDs.CommandMaster))
            {
                if (args.IsPostBack)
                {
                    if (!args.HasResult)
                    {
                        return;
                    }
                    string @string = StringUtil.GetString(new string[]
                    {
                        args.Parameters["ItemID"]
                    });
                    string string2 = StringUtil.GetString(new string[]
                    {
                        args.Parameters["Language"]
                    });
                    Item item2 = Context.ContentDatabase.Items[@string, Language.Parse(string2)];
                    if (item2 == null)
                    {
                        SheerResponse.Alert("Parent item not found.", new string[0]);
                        return;
                    }
                    if (!item2.Access.CanCreate())
                    {
                        Context.ClientPage.ClientResponse.Alert("You do not have permission to create items here.");
                        return;
                    }
                    try
                    {
                        if (item.TemplateID == TemplateIDs.BranchTemplate)
                        {
                            BranchItem branch = item;
                            // Sitecore.Support.93207
                            Item item3 = Context.Workflow.AddItem(args.Result, branch, item2);
                            if (BucketManager.IsBucketable(item3))
                            {
                                BucketManager.Sync(item3.Parent);
                            }
                        }
                        else
                        {
                            TemplateItem template = item;
                            Context.Workflow.AddItem(args.Result, template, item2);
                        }
                        return;
                    }
                    catch (WorkflowException ex)
                    {
                        Log.Error("Workflow error: could not add item from master", ex, this);
                        SheerResponse.Alert(ex.Message, new string[0]);
                        return;
                    }
                }
                SheerResponse.Input("Enter a name for the new item:", item.DisplayName, Settings.ItemNameValidation, "'$Input' is not a valid name.", Settings.MaxItemNameLength);
                args.WaitForPostBack();
                return;
            }
            string text = item["Command"];
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            Context.ClientPage.SendMessage(this, text);
        }
    }
}