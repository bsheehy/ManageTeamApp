using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;
using TeamManage.Application.DTOs.Managers;

namespace TeamManage.App.ViewModels;

public partial class ManagersViewModel : BaseViewModel
{
    private readonly IManagerApiService _api;
    private Guid? _editingId;

    public ObservableCollection<TeamManagerSummaryDto> Managers { get; } = [];

    [ObservableProperty]
    private string formFullName = string.Empty;

    [ObservableProperty]
    private string formEmail = string.Empty;

    [ObservableProperty]
    private string? infoMessage;

    [ObservableProperty]
    private string saveButtonText = "Add Manager & Send Invite";

    public ManagersViewModel(IManagerApiService api)
    {
        _api = api;
        Title = "Team Managers";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunBusyAsync(async () =>
        {
            var items = await _api.GetManagersAsync();
            Managers.Clear();
            foreach (var m in items)
            {
                Managers.Add(m);
            }
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        InfoMessage = null;
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(FormFullName) || string.IsNullOrWhiteSpace(FormEmail))
            {
                ErrorMessage = "Please enter a name and email.";
                return;
            }

            var request = new SaveTeamManagerRequest { FullName = FormFullName.Trim(), Email = FormEmail.Trim() };
            if (_editingId is { } id)
            {
                await _api.UpdateAsync(id, request);
                InfoMessage = "Manager updated.";
            }
            else
            {
                await _api.CreateAsync(request);
                InfoMessage = $"Invite email sent to {request.Email}.";
            }

            ResetForm();
            await LoadInnerAsync();
        });
    }

    private async Task LoadInnerAsync()
    {
        var items = await _api.GetManagersAsync();
        Managers.Clear();
        foreach (var m in items)
        {
            Managers.Add(m);
        }
    }

    [RelayCommand]
    private void Edit(TeamManagerSummaryDto manager)
    {
        _editingId = manager.Id;
        FormFullName = manager.FullName;
        FormEmail = manager.Email;
        SaveButtonText = "Save Changes";
        InfoMessage = null;
    }

    [RelayCommand]
    private void CancelEdit() => ResetForm();

    [RelayCommand]
    private async Task DeleteAsync(TeamManagerSummaryDto manager)
    {
        if (!await Shell.Current.DisplayAlertAsync("Delete manager", $"Delete {manager.FullName}?", "Delete", "Cancel"))
        {
            return;
        }

        InfoMessage = null;
        await RunBusyAsync(async () =>
        {
            await _api.DeleteAsync(manager.Id);
            await LoadInnerAsync();
        });
    }

    [RelayCommand]
    private async Task ResendInviteAsync(TeamManagerSummaryDto manager)
    {
        InfoMessage = null;
        await RunBusyAsync(async () =>
        {
            await _api.ResendInviteAsync(manager.Id);
            InfoMessage = $"Invite re-sent to {manager.Email}.";
        });
    }

    private void ResetForm()
    {
        _editingId = null;
        FormFullName = string.Empty;
        FormEmail = string.Empty;
        SaveButtonText = "Add Manager & Send Invite";
    }
}
