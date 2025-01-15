using System;
using System.IO;
using Excel2_0;
using Microsoft.Office.Interop.Excel;
using System.Net.WebSockets;


class Program
{
    public static async Task Main()
    {
        
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "test", "Test.xlsx");
        ClientWebSocket client = await App.Init();
        Xcel excel = new Xcel(filePath, 1, client);



        /*Console.WriteLine(excel.ReadCell(1, 1));

        excel.WriteToCell(1, 2, "Olá");

        for (int i = 3; i <= 10; i++)
        {
            excel.WriteToCell(i, 1, "Vajjhgvhnbricas");
            System.Threading.Thread.Sleep(500); // Pequeno atraso para ver a mudança gradual
        }

        Console.WriteLine("Pressione Enter para fechar o Excel.");
        Console.ReadLine();

        excel.Close();


        Console.WriteLine("Excel fechado.");
        */


        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Start processing WebSocket messages in a separate task
        var messageProcessingTask = Task.Run(async () =>
        {
            while (client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await App.ProcessMessages(client);
                Console.WriteLine($"Parsed NLU data: {string.Join(", ", result.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                await excel.ProcessInstruction(result);
            }
        });

        // Wait for the user to press Enter
        Console.WriteLine("Press Enter to close the connection...");
        Console.ReadLine();

        // Signal cancellation and close the WebSocket
        cancellationTokenSource.Cancel();

        if (client.State == WebSocketState.Open)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "User requested closure", CancellationToken.None);
            Console.WriteLine("WebSocket connection closed.");

            //excel
            try{
                excel.Close();
                Console.WriteLine("Excel fechado.");
            } catch {
                Console.WriteLine("Excel já encerrado");
            }
        }

        // Wait for the message processing task to complete
        await messageProcessingTask;

    }
}