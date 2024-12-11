using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Models;

namespace SteamWebAuth.Components.Pages;

public partial class FirstVisitDialog
{
    [Parameter] public bool IsVisible { get; set; }

    [Parameter] public required string Title { get; set; }

    [Parameter] public required string Message { get; set; }

    [Parameter] public EventCallback<bool> OnClose { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }

    private async Task OnFilesSelected(InputFileChangeEventArgs e)
    {
        var accountFiles = e.GetMultipleFiles();
        if (accountFiles.Count == 0)
        {
            return;
        }

        var accounts = new List<Account>();
        foreach (var accountFile in accountFiles)
        {
            await using var accountFileStream = accountFile.OpenReadStream();
            using var reader = new StreamReader(accountFileStream);
            var accountContent = await reader.ReadToEndAsync();
            var account = await accountContent.FromJsonAsync<Account>();
            account = await account.CreateOrLoadAsync(account.Username + ".json");
            accounts.Add(account);
        }

        await AccountService.SetAccountsListAsync(accounts);
        await OnChange.InvokeAsync(true);
        IsVisible = false;
        StateHasChanged();
    }

    private void Cancel()
    {
        IsVisible = false;
        OnClose.InvokeAsync(false);
    }
}