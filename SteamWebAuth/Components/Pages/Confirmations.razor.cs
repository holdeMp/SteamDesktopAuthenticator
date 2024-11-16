using Microsoft.AspNetCore.Components;

namespace SteamWebAuth.Components.Pages;

public partial class Confirmations : ComponentBase, IDisposable
{
    protected override Task OnInitializedAsync()
    {
        AccountService.OnChange += StateHasChanged;
        Refresh();
        return Task.CompletedTask;
    }
    
    private static bool TryParsePrice(string price, out decimal actualPrice)
    {
        var regex = MyRegex();
        var match = regex.Match(price);

        if (!match.Success)
        {
            actualPrice = 0;
            return false;
        }

        var numericValue = match.Value.Replace('\u20b4', ' ');
        return decimal.TryParse(numericValue, out actualPrice);
    }

    private string GetTotalPrice()
    {
        var result = (decimal)0.00;
        foreach (var headLine in AccountService.SelectedAccount!.Confirmations.Select(confirmation => confirmation.Headline))
        {
            if (!TryParsePrice(headLine, out var confPrice)) return $"Invalid price {headLine}";
            result += confPrice;
        }

        return (result / 100).ToString("0.00");
    }
    
    private void SelectAllConfirmations()
    {
        if (AccountService.SelectedAccount!.Confirmations.Any(c => c.IsSelected))
        {
            foreach (var confirmation in AccountService.SelectedAccount.Confirmations)
            {
                confirmation.IsSelected = false;
            }
            StateHasChanged();
            return;
        }
        if (AccountService.SelectedAccount == null) return;
        foreach (var confirmation in AccountService.SelectedAccount.Confirmations)
        {
            confirmation.IsSelected = true;
        }
    }

    private async void AcceptAll()
    {
        if (AccountService.SelectedAccount == null) return;
        if (AccountService.SelectedAccount.Confirmations.Count == 1)
        {
            var res = await AccountService.AcceptConfirmationAsync(AccountService.SelectedAccount.Confirmations.First());
            if (res != true) Refresh();
        }
        var result = await AccountService.AcceptMultipleConfirmationsAsync(AccountService.SelectedAccount.Confirmations.ToList());
        if (result != true) Refresh();
    }
    
    private async void AcceptSelected()
    {
        AccountService.IsConfirmationsLoading = true;
        StateHasChanged();
        if (AccountService.SelectedAccount == null || !AccountService.SelectedAccount.Confirmations
                .Any(c => c.IsSelected))
        {
            return;
        }
        var selectedConfirmations = AccountService.SelectedAccount.Confirmations
            .Where(c => c.IsSelected).ToList();
        if (selectedConfirmations.Count == 1)
        {
            await AccountService.AcceptConfirmationAsync(selectedConfirmations.First());
            Refresh();
            AccountService.IsConfirmationsLoading = false;
            StateHasChanged();
            return;
        }
        await AccountService.AcceptMultipleConfirmationsAsync(selectedConfirmations);
        Refresh();
        StateHasChanged();
        AccountService.IsConfirmationsLoading = false;
    }

    private async void Refresh()
    {
        StateHasChanged();
        await AccountService.FetchConfirmationsAsync();
        StateHasChanged();
    }
    
    public void Dispose()
    {
        AccountService.OnChange -= StateHasChanged;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"[0-9]+,\d\d₴")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}
