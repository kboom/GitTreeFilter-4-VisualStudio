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
            GitTreeFilterOutput.WriteLine($"Initialized all commands");
        }
        catch (Exception ex)
        {
            GitTreeFilterOutput.WriteLine($"Failed to register commands: {ex}");
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
        GitTreeFilterOutput.WriteLine($"Registering command {command.GetType()} with id {rawCommandId}");

        var commandId = new CommandID(GitFiltersControls.GitFiltersControlsCmdSet, rawCommandId);
        if (commandService.FindCommand(commandId) == null)
        {
            var menuCommand = new OleMenuCommand(command.OnExecute, commandId);

            menuCommand.BeforeQueryStatus += delegate (object sender, EventArgs e)
            {
                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    menuCommand.Visible = command.IsVisible;
                    return Task.CompletedTask;
                });
            };

            command.VisibilityChanged += delegate
            {
                ThreadHelper.JoinableTaskFactory.Run(() =>
                {
                    menuCommand.Visible = command.IsVisible;
                    return Task.CompletedTask;
                });
            };

            commandService.AddCommand(menuCommand);

            GitTreeFilterOutput.WriteLine($"Command {command.GetType()} with id {rawCommandId} registered");
        }
        else
        {
            GitTreeFilterOutput.WriteLine($"Command {command.GetType()} with id {rawCommandId} already registered");
        }
    }
}
