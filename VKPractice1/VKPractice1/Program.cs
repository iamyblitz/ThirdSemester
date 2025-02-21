using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace VKPractice1;
public class User
{
    public  int Id { get; set; }
    public static string? Name { get; set; }
    public static string? Lastname { get; set; }
    public static string? can_access_closed { get; set; }
    public static string? is_closed { get; set; }
    
    
}

public class Response
{
    public List<User> Users { get; set; } = new List<User>();
}


class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        string accessToken = "4ff0cdda4ff0cdda4ff0cddad94cd1d2f044ff04ff0cdda28e1ed0f997ebe692608c1ae";
        string userId = "213202158"; 
        
        string url = $"https://api.vk.com/method/users.get?user_ids={userId}&access_token={accessToken}&v=5.131";

        try
        {
            var response = await client.GetStringAsync(url);
            Console.WriteLine(response);
            
            var responseUp = JsonSerializer.Deserialize<Response>(response);
            //var user = responseUp.Users[0];
            Console.WriteLine(responseUp);
            
            //Console.WriteLine($"User: {User.Id}");
            Console.WriteLine($"Name: {User.Name}");
            Console.WriteLine($"LastName: {User.Lastname}");
          
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка: {e.Message}");
        }
    }
}