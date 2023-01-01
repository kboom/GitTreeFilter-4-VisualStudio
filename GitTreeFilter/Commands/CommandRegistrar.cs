using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using GitTreeFilter.Core;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace GitTreeFilter.Commands
{
    internal class GitFiltersControls
    {
        public const string GuidGitFiltersControlsCmdSet = "04ed3748-c3e2-4f02-8aaf-552c892418b8";

        public readonly static Guid GitFiltersControlsCmdSet = new Guid(GuidGitFiltersControlsCmdSet);

        public const uint SelectReferenceObjectButtonId = 0x1022;

        public const int CommandIdPhysicalFileDiffMenuCommand = 0x0100;
        public const int CommandIdProjectFileDiffMenuCommand = 0x0200;
        public const int CommandIdSelectReferenceObjectCommand = 0x1022;
    }

    internal class CommandRegistrar
    {
        public static async Task InitializeAsync(IGitFiltersPackage package)
        {
            Assumes.NotNull(package);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync<IMenuCommandService>()
                ?? throw new ArgumentNullException(nameof(package));

            try
            {
                await RegisterAsync(package, commandService);
            }
            catch (Exception ex)
            {
                ActivityLog.LogError(nameof(CommandRegistrar), $"Failed to register commands - {ex}");
                throw ex;
            }
        }

        private static async Task RegisterAsync(IGitFiltersPackage package, IMenuCommandService commandService)
        {
            var commands = await CreateCommandsAsync(package);
            foreach (var commandEntry in commands)
            {
                var rawCommandId = commandEntry.Key;
                var command = commandEntry.Value;
                RegisterCommand(commandService, rawCommandId, command);
            }
        }

        private static async Task<IDictionary<int, GitFiltersCommand>> CreateCommandsAsync(IGitFiltersPackage package) => new Dictionary<int, GitFiltersCommand>()
        {
            [GitFiltersControls.CommandIdPhysicalFileDiffMenuCommand] = new OpenPhysicalFileDiffCommand(await OpenDiffCommandManager.CreateAsync(package), package.Service),
            [GitFiltersControls.CommandIdProjectFileDiffMenuCommand] = new OpenProjectFileDiffCommand(await OpenDiffCommandManager.CreateAsync(package), package.Service),
            [GitFiltersControls.CommandIdSelectReferenceObjectCommand] = await ConfigureReferenceObjectCommand.CreateAsync(package)
        }.ToImmutableDictionary();

        private static void RegisterCommand(IMenuCommandService commandService, int rawCommandId, GitFiltersCommand command)
        {
            var commandId = new CommandID(GitFiltersControls.GitFiltersControlsCmdSet, rawCommandId);
            var menuCommand = new OleMenuCommand(command.OnExecute, commandId);

            menuCommand.BeforeQueryStatus += delegate (object sender, EventArgs e)
            {
                menuCommand.Visible = command.IsVisible;
            };

            command.VisibilityChanged += delegate
            {
                menuCommand.Visible = command.IsVisible;
            };

            commandService.RemoveCommand(menuCommand);
            commandService.AddCommand(menuCommand);
        }
    }
}
