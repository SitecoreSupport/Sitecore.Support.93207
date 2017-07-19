using Sitecore.Buckets.Extensions;
using Sitecore.Buckets.Managers;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Support.Shell.Framework.Pipelines
{
    /// <summary>
    /// Represents the Add From Template pipeline.
    /// </summary>
    public class AddFromTemplate
    {
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
            Item item2 = database.GetItem(path);
            if (item2 == null)
            {
                SheerResponse.Alert("Item not found.", new string[0]);
                args.AbortPipeline();
                return;
            }
            History.Template = item2.ID.ToString();
            if (item2.TemplateID == TemplateIDs.Template)
            {
                Log.Audit(this, "Add from template: {0}", new string[]
                {
                    AuditFormatter.FormatItem(item2)
                });
                TemplateItem template = item2;
                Context.Workflow.AddItem(name, template, item);
                return;
            }
            Log.Audit(this, "Add from branch: {0}", new string[]
            {
                AuditFormatter.FormatItem(item2)
            });
            BranchItem branch = item2;
            // Sitecore.Support.93207
            Item item3 = Context.Workflow.AddItem(name, branch, item);
            if (BucketManager.IsBucketable(item3))
            {
                BucketManager.Sync(item3.Parent);
            }
        }
    }
}