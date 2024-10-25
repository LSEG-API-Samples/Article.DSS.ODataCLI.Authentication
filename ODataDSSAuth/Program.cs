using DSSAuthentication;
using Microsoft.OData.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace ODataDSSAuth
{
    internal class Program
    {
        private string DSSUsername = "<DSS Username>";
        private string DSSPassword = "<DSS Password>";
        static void Main(string[] args)
        {         
            Program prog = new Program();
            prog.UseOdataService();
            prog.UseHTTPClient();
        }

        public void UseHTTPClient()
        {
            Console.WriteLine("\n### Use HttpClient ###");
            Credentials creds = new Credentials();
            creds.Username = DSSUsername;
            creds.Password = DSSPassword;            

            var jsonBody = new
            {

                Credentials = creds
            };

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            var jsonStr = JsonSerializer.Serialize(jsonBody, options);

            
            HttpRequestMessage requestMessage = new HttpRequestMessage(
                HttpMethod.Post, 
                "https://selectapi.datascope.refinitiv.com/RestApi/v1/Authentication/RequestToken");

            requestMessage.Headers.Add("Prefer", "respond-async");           

            requestMessage.Content = new StringContent(jsonStr, Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            var response = client.Send(requestMessage);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                return;
            }

            JsonNode? authResponse = JsonSerializer.Deserialize<JsonNode>(response.Content.ReadAsStringAsync().Result);
            string? token = null;
            if (authResponse is not null)
                token = authResponse["value"].GetValue<string>();
            
            if(token is not null)
            {
                Console.WriteLine($"Token: {token}");
            }

            

        }

        public void UseOdataService()
        {
            Console.WriteLine("\n### Use DataServiceContext ###");
            try
            {
                Authentication auth = new Authentication(new Uri("https://selectapi.datascope.refinitiv.com/restapi/v1/Authentication"));
                var resp = auth.RequestToken(new Credentials { Username = DSSUsername, Password = DSSPassword });
                var token = resp.GetValue();

                Console.WriteLine($"Token: {token}");

               
            }
            catch (DataServiceQueryException ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                return;
            }
        }
    }
}
