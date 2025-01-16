using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json;
using System.Xml;
using System.Security.Cryptography.X509Certificates;

namespace Excel2_0
{
    public class App
    {
        public static async Task<ClientWebSocket> Init()
        {
            string host = "127.0.0.1"; // Replace with your actual host
            string path = "/IM/USER1/APP"; // Replace with your WebSocket path
            ClientWebSocket client = new ClientWebSocket();

           
            try
            {
                Uri uri = new Uri("wss://" + host + ":8005" + path);
                await client.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("Connected to the WebSocket server.");
                // IMPORTANTE
                // Handle messages and other logic here
                //await ProcessMessages(client);

                // Close the WebSocket when done
                //await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }

            return client;
        }


        public static async Task SendMessage(ClientWebSocket client, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message) ;
            //Console.WriteLine($"Client: {client}");
            //Console.WriteLine($"Sending message: {message}");
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            //Console.WriteLine($"Sent message: {message}");
        }

        public static Dictionary<string, string> NluExtractor(string message)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(message);

                // Get all command nodes
                var commandNodes = doc.SelectNodes("//command");
                if (commandNodes == null || commandNodes.Count == 0)
                {
                    throw new ArgumentException("Command nodes not found in XML");
                }

                var result = new Dictionary<string, string>();

                foreach (XmlNode commandNode in commandNodes)
                {
                    var command = JsonDocument.Parse(commandNode.InnerText);
                    var recognized = command.RootElement.GetProperty("recognized").EnumerateArray();
                    var commandType = recognized.ElementAtOrDefault(0).GetString();

                    switch (commandType)
                    {
                        case "FUSION":
                            var fusionIntent = recognized.ElementAtOrDefault(1).GetString();
                            if (fusionIntent != null)
                            {
                                result["intent"] = fusionIntent;
                            }
                            break;

                        case "GESTURES":
                            var gestureType = recognized.ElementAtOrDefault(1).GetString();
                            if (gestureType != null)
                            {
                                result["gesture"] = gestureType.ToLower();
                            }
                            break;

                        case "SPEECH":
                            if (command.RootElement.TryGetProperty("nlu", out var nluElement))
                            {
                                var entities = nluElement.GetProperty("entities");
                                foreach (var entity in entities.EnumerateArray())
                                {
                                    var entityType = entity.GetProperty("entity").GetString();
                                    var entityValue = entity.GetProperty("value").GetString();
                                    if (entityType != null && entityValue != null)
                                    {
                                        result[entityType] = entityValue;
                                    }
                                }
                            }
                            break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NluExtractor: {ex.Message}");
                throw;
            }
        }

        public static async Task<Dictionary<string, string>> ProcessMessages(ClientWebSocket client)
        {

            byte[] buffer = new byte[8192];
            var dic = new Dictionary<string, string>
        {
            { "intent", "ignore" }
        };
            //var tts = new TTS("http://localhost:8801/IM", "https://127.0.0.1:8000/IM/USER/APPSPEECH");
            //await tts.SendToVoice("Olá tudo bem");

            //Tts t = new Tts();
            //t.Speak("Welcome! I'm your excel voice assistant, how can I help you today?");

            Console.WriteLine("Waiting for message...");
            WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine("Message received!!!");
            
            //await tts.SendToVoice("Olá tudo bem!");

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (message == "OK")
                {
                    Console.WriteLine("Received message OK: " + message);
                    await SendMessage(client, messageMMI("Bom dia, sou o teu assistente de voz do excel, como te posso ajudar"));
                    return dic;
                }
                else if (message != null && message != "RENEW")
                {
                    //Console.WriteLine("Received new message");
                    //Console.WriteLine(message);
                   
                    try
                    {
                        var nlu = NluExtractor(message);
                        if (nlu != null)
                        {
                            //Console.WriteLine($"Parsed NLU data: {string.Join(", ", nlu.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                            //await SendMessage(client, messageMMI("boas como estas"));
                            return nlu;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        // Continue receiving messages even if processing fails
                        //continue;
                    }
                }
                else { 
                    Console.WriteLine("Ignore");
                    return dic;
                }
            }
            return dic;
        }
        
        public static string messageMMI(string msg)
        {
            return "<mmi:mmi xmlns:mmi=\"http://www.w3.org/2008/04/mmi-arch\" mmi:version=\"1.0\">" +
                        "<mmi:startRequest mmi:context=\"ctx-1\" mmi:requestId=\"text-1\" mmi:source=\"APPSPEECH\" mmi:target=\"IM\">" +
                            "<mmi:data>" +
                                "<emma:emma xmlns:emma=\"http://www.w3.org/2003/04/emma\" emma:version=\"1.0\">" +
                                    "<emma:interpretation emma:confidence=\"1\" emma:id=\"text-\" emma:medium=\"text\" emma:mode=\"command\" emma:start=\"0\">" +
                                        "<command>\"&lt;speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\" xml:lang=\"pt-PT\"&gt;&lt;p&gt;" + System.Security.SecurityElement.Escape(msg) + "&lt;/p&gt;&lt;/speak&gt;\"</command>" +
                                    "</emma:interpretation>" +
                                    "</emma:emma>" +
                            "</mmi:data>" +
                        "</mmi:startRequest>" +
                    "</mmi:mmi>";
        }
    }

    
}


/*using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.Xml.Linq;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json;
using System.Xml;

class Program
{
    static async Task Main(string[] args)
    {
        string host = "127.0.0.1"; // Replace with your actual host
        string path = "/IM/USER1/APP"; // Replace with your WebSocket path

        // Receive Messages from rasa
        using (ClientWebSocket client = new ClientWebSocket())
        {
            Uri uri = new Uri("wss://" + host + ":8005" + path);

            try
            {
                await client.ConnectAsync(uri, CancellationToken.None);

                Console.WriteLine("Connected to the WebSocket server.");

                // Handle messages and other logic here
                await ProcessMessages(client);

                // Close the WebSocket when done
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
        }
    }
    private static async Task SendMessage(ClientWebSocket client, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"Sent message: {message}");
    }

    private static Dictionary<string, string> NluExtractor(string message)
    {
        try
        {
            //Console.WriteLine("In NluExtractor");

            // Parse the XML to find <command> tag
            var doc = new XmlDocument();
            doc.LoadXml(message);
            //Console.WriteLine($"{doc.OuterXml}");
            var commandNode = doc.SelectSingleNode("//command");

            if (commandNode == null)
            {
                throw new ArgumentException("Command node not found in XML");
            }

            // Debug output to see the actual JSON content
            //Console.WriteLine("Command node content:");
            //Console.WriteLine(commandNode.InnerText);

            // First JSON parse - get the outer JSON object
            using var outerJson = JsonDocument.Parse(commandNode.InnerText);

            // Get the nlu string property
            var nluString = outerJson.RootElement.GetProperty("nlu").GetString();
            if (nluString == null)
            {
                throw new ArgumentException("NLU string is null");
            }

            // Second JSON parse - parse the nlu string into a JSON object
            using var nluJson = JsonDocument.Parse(nluString);
            var nluRoot = nluJson.RootElement;

            // Extract intent
            var intent = nluRoot.GetProperty("intent").GetProperty("name").GetString();
            Console.WriteLine($"Found intent: {intent}");

            // Initialize the result dictionary with the intent
            var result = new Dictionary<string, string>
        {
            { "intent", intent }
        };

            // Extract entities if they exist
            if (nluRoot.TryGetProperty("entities", out var entities) && entities.GetArrayLength() > 0)
            {
                foreach (var entity in entities.EnumerateArray())
                {
                    var entityType = entity.GetProperty("entity").GetString();
                    var entityValue = entity.GetProperty("value").GetString();
                    result[entityType] = entityValue;
                }
            }

            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            throw new ArgumentException($"Failed to parse JSON data: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR on NluExtractor: {ex.Message}");
            throw new ArgumentException($"Failed to parse NLU data: {ex.Message}", ex);
        }
    }

    static async Task ProcessMessages(ClientWebSocket client)
    {

        byte[] buffer = new byte[8192];

        //ChromeOptions options = new ChromeOptions();
        //options.AddArgument("2>&1");    // skip warnings 
        //IWebDriver driver = new ChromeDriver(@"C:\Users\35191\Downloads\chromedriver-win64\chromedriver-win64");
        //WebDriver driver = new ChromeDriver(options);

        bool website_open = false;  // check if website is already opened
        bool myaccount = false;     // check if we already are on our account
        bool authn_open = false;    // check if we are in the authentication page
        bool notf_open = false;
        bool confirm_new_event = false;

        while (client.State == WebSocketState.Open)
        {
            Console.WriteLine("Waiting for message");
            WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine("Message received!!!");

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (message == "OK")
                {
                    Console.WriteLine("Received message OK: " + message);
                }
                else if (message != null && message != "RENEW")
                {
                    //Console.WriteLine("Got here");
                    //Console.WriteLine("Received message: " + message);

                    //var doc = XDocument.Parse(message);
                    //var com = doc.Descendants("command").FirstOrDefault().Value;
                    //dynamic messageJSON = JsonConvert.DeserializeObject(com);
                    //Console.WriteLine(messageJSON);
                    try
                    {
                        var nlu = NluExtractor(message);
                        if (nlu != null)
                        {
                            Console.WriteLine($"Parsed NLU data: {string.Join(", ", nlu.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        // Continue receiving messages even if processing fails
                        continue;
                    }
                }
                else { Console.WriteLine("Something is wrong"); }
            }
        }
    }
} */