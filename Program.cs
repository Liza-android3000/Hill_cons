using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static string baseUrl = "http://localhost:5011/api";
    private static string token = string.Empty;

    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Регистрация");
            Console.WriteLine("2. Вход");
            Console.WriteLine("3. Просмотр истории запросов");
            Console.WriteLine("4. Удаление истории запросов");
            Console.WriteLine("5. Изменение пароля");
            Console.WriteLine("6. Добавить текст");
            Console.WriteLine("7. Изменить текст");
            Console.WriteLine("8. Удалить текст");
            Console.WriteLine("9. Просмотреть один текст");
            Console.WriteLine("10. Просмотреть все тексты");
            Console.WriteLine("11. Зашифровать текст");
            Console.WriteLine("12. Расшифровать текст");
            Console.WriteLine("13. Выход");

            string choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await Register();
                        break;
                    case "2":
                        await Login();
                        break;
                    case "3":
                        await GetHistory();
                        break;
                    case "4":
                        await DeleteHistory();
                        break;
                    case "5":
                        await ChangePassword();
                        break;
                    case "6":
                        await AddText();
                        break;
                    case "7":
                        await UpdateText();
                        break;
                    case "8":
                        await DeleteText();
                        break;
                    case "9":
                        await GetText();
                        break;
                    case "10":
                        await GetAllTexts();
                        break;
                    case "11":
                        await EncryptText();
                        break;
                    case "12":
                        await DecryptText();
                        break;
                    case "13":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }

    private static async Task Register()
    {
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        var user = new { Username = username, Password = password };

        try
        {
            var response = await httpClient.PostAsJsonAsync($"{baseUrl}/users/register", user);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Регистрация успешна.");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Console.WriteLine("Ошибка: Пользователь с таким именем уже существует.");
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task Login()
    {
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        var user = new { Username = username, Password = password };

        try
        {
            var response = await httpClient.PostAsJsonAsync($"{baseUrl}/users/login", user);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                token = result.GetProperty("token").GetString();
                Console.WriteLine("Вход выполнен.");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Ошибка: Неверное имя пользователя или пароль.");
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task GetHistory()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/users/history"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var history = JsonSerializer.Deserialize<List<RequestHistoryItem>>(responseContent, options);

                    Console.WriteLine("История запросов:");
                    if (history != null && history.Any())
                    {
                        foreach (var item in history)
                        {
                            Console.WriteLine($"URL: {item.RequestUrl}, Время: {item.RequestTime}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("История запросов пуста.");
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("Ошибка: Необходимо авторизоваться.");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка при десериализации ответа: {ex.Message}");
        }
    }

    private static async Task DeleteHistory()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/users/history"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("История запросов удалена.");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("Ошибка: Необходимо авторизоваться.");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task ChangePassword()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите новый пароль: ");
        string newPassword = Console.ReadLine();

        var request = new { NewPassword = newPassword };

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{baseUrl}/users/password"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = JsonContent.Create(request);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    token = result.GetProperty("token").GetString();
                    Console.WriteLine("Пароль успешно изменен.");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("Ошибка: Необходимо авторизоваться.");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task AddText()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите текст: ");
        string content = Console.ReadLine();

        var request = new { Content = content };

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/hillcipher/texts"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = JsonContent.Create(request);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine("Текст успешно добавлен. ID: " + result.GetProperty("id"));
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task UpdateText()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        int id = int.Parse(Console.ReadLine());
        Console.Write("Введите новый текст: ");
        string content = Console.ReadLine();

        var request = new { Content = content };

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{baseUrl}/hillcipher/texts/{id}"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = JsonContent.Create(request);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine("Текст успешно изменен. ID: " + result.GetProperty("id"));
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task DeleteText()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        int id = int.Parse(Console.ReadLine());

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/hillcipher/texts/{id}"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Текст успешно удален.");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task GetText()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        int id = int.Parse(Console.ReadLine());

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/hillcipher/texts/{id}"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine("Текст:");
                    Console.WriteLine($"ID: {result.GetProperty("id")}");
                    Console.WriteLine($"User ID: {result.GetProperty("userId")}");
                    Console.WriteLine($"Content: {result.GetProperty("content")}");
                    Console.WriteLine($"Is Encrypted: {result.GetProperty("isEncrypted")}");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task GetAllTexts()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        try
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/hillcipher/texts"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine("Все тексты:");
                    foreach (var text in result.EnumerateArray())
                    {
                        Console.WriteLine($"ID: {text.GetProperty("id")}");
                        Console.WriteLine($"User ID: {text.GetProperty("userId")}");
                        Console.WriteLine($"Content: {text.GetProperty("content")}");
                        Console.WriteLine($"Is Encrypted: {text.GetProperty("isEncrypted")}");
                        Console.WriteLine();
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
        }
    }

    private static async Task EncryptText()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        int id = int.Parse(Console.ReadLine());
        Console.Write("Введите ключ (матрицу через запятую, например, 5,8,3,7): ");
        string key = Console.ReadLine();

        try
        {
            var request = new { Key = key };

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/hillcipher/texts/{id}/encrypt"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = JsonContent.Create(request);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine("Текст успешно зашифрован:");
                    Console.WriteLine($"ID: {result.GetProperty("id")}");
                    Console.WriteLine($"User ID: {result.GetProperty("userId")}");
                    Console.WriteLine($"Content: {result.GetProperty("content")}");
                    Console.WriteLine($"Is Encrypted: {result.GetProperty("isEncrypted")}");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static async Task DecryptText()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        int id = int.Parse(Console.ReadLine());
        Console.Write("Введите ключ (матрицу через запятую): ");
        string key = Console.ReadLine();

        try
        {
            var request = new { Key = key };

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/hillcipher/texts/{id}/decrypt"))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = JsonContent.Create(request);

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    Console.WriteLine("Текст успешно расшифрован:");
                    Console.WriteLine($"ID: {result.GetProperty("id")}");
                    Console.WriteLine($"User ID: {result.GetProperty("userId")}");
                    Console.WriteLine($"Content: {result.GetProperty("content")}");
                    Console.WriteLine($"Is Encrypted: {result.GetProperty("isEncrypted")}");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}. {errorResponse}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}

public class RequestHistoryItem
{
    public required string RequestUrl { get; set; }
    public DateTime RequestTime { get; set; }
}