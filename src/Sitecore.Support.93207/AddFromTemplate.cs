using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using System;

namespace Sitecore.Shell.Framework.Pipelines
{
    /// <summary>
    /// Represents the Add From Template pipeline.
    /// </summary>
    public class AddFromTemplate
    {
        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <contract>
        ///   <requires name="args" condition="not null" />
        /// </contract>
        public void GetTemplate(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!SheerResponse.CheckModified())
            {
                return;
            }
            UrlString urlString = new UrlString(UIUtil.GetUri("control:AddFromTemplate"));
            string template = History.Template;
            if (template != null && template.Length > 0)
            {
                urlString.Append("fo", template);
            }
            Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", "", true);
            args.WaitForPostBack(false);
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <contract>
        ///   <requires name="args" condition="not null" />
        /// </contract>
        public void Execute(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!args.HasResult)
            {
                return;
            }
            int num = args.Result.IndexOf(',');
            Assert.IsTrue(num >= 0, "Invalid return value from dialog");
            string path = StringUtil.Left(args.Result, num);
            string name = StringUtil.Mid(args.Result, num + 1);
            Database database = Factory.GetDatabase(args.Parameters["database"]);
            string itemPath = args.Parameters["id"];
            string name2 = args.Parameters["language"];
            Item item = database.Items[itemPath, Language.Parse(name2)];
            if (item == null)
            {
                SheerResponse.Alert("Parent item not found.", new string[0]);
                args.AbortPipeline();
                return;
            }
            if (!item.Access.CanCreate())
            {
                SheerResponse.Alert("You do not have permission to create items here.", new string[0]);
                args.AbortPipeline();
                return;
            }
            Item item2 = database.GetItem(path, Language.Parse(name2));
            if (item2 == null)
            {
                SheerResponse.Alert("Item not found.", new string[0]);
                args.AbortPipeline();
                return;
            }
            History.Template = item2.ID.ToString();
            Item item3;
            if (item2.TemplateID == TemplateIDs.Template)
            {
                Log.Audit(this, "Add from template: {0}", new string[]
                {
                    AuditFormatter.FormatItem(item2)
                });
                TemplateItem template = item2;
                item3 = Context.Workflow.AddItem(name, template, item);
            }
            else
            {
                Log.Audit(this, "Add from branch: {0}", new string[]
                {
                    AuditFormatter.FormatItem(item2)
                });
                BranchItem branch = item2;
                item3 = Context.Workflow.AddItem(name, branch, item);
            }
            args.CarryResultToNextProcessor = true;
            if (item3 == null)
            {
                args.AbortPipeline();
                return;
            }
            args.Result = item3.ID.ToString();
        }
    }
}