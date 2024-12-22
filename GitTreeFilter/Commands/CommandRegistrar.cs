using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace GitTreeFilter.Commands;

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
