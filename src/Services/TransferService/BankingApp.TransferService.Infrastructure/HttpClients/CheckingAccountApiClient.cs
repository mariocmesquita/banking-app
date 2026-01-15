using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BankingApp.TransferService.Application.DTOs;
using BankingApp.TransferService.Application.Interfaces;

namespace BankingApp.TransferService.Infrastructure.HttpClients;

public class CheckingAccountApiClient : ICheckingAccountApiClient
{
    private readonly HttpClient _httpClient;

    public CheckingAccountApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateMovementAsync(
        long? targetAccountNumber,
        decimal amount,
        char movementType,
        string jwtToken,
        string idempotencyKey)
    {
        var request = new CreateMovementRequest(targetAccountNumber, amount, movementType);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/checking-accounts/movements");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        httpRequest.Headers.Add("Idempotency-Key", idempotencyKey);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Falha ao criar movimento. Status: {response.StatusCode}, Erro: {errorContent}");
        }
    }

    public async Task<Guid?> GetAccountIdByNumberAsync(long accountNumber, string jwtToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/checking-accounts/by-number/{accountNumber}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new HttpRequestException(
                $"Falha ao obter conta {accountNumber}. Status: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var accountData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

        if (accountData != null && accountData.TryGetValue("id", out var idElement))
        {
            var accountId = Guid.Parse(idElement.GetString()!);
            return accountId;
        }

        return null;
    }
}