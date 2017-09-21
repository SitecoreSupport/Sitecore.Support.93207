using Sitecore.Diagnostics;
using System;

namespace Sitecore.Shell.Framework.Commands
{
    /// <summary>
    /// Represents the AddFromTemplate command.
    /// </summary>
    [Serializable]
    public class AddFromTemplate : Command
    {
        /// <summary>
        /// Executes the command in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <contract>
        ///   <requires name="context" condition="not null" />
        /// </contract>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1 || !context.Items[0].Access.CanCreate())
            {
                return;
            }
            Items.AddFromTemplate(context.Items[0]);
        }

        /// <summary>
        /// Queries the state of the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The state of the command.</returns>
        /// <contract>
        ///   <requires name="context" condition="not null" />
        /// </contract>
        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Hidden;
            }
            if (!context.Items[0].Access.CanCreate() || !context.Items[0].Access.CanWriteLanguage())
            {
                return CommandState.Disabled;
            }
            return base.QueryState(context);
        }
    }
}