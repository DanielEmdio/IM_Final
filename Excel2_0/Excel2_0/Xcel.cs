using System;
using System.Data.Common;
using System.Drawing;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using _Excel = Microsoft.Office.Interop.Excel;
using ExcelRange = Microsoft.Office.Interop.Excel.Range;

namespace Excel2_0
{
    class Xcel

    {
        string path = "";
        _Application excel = new _Excel.Application();
        readonly Dictionary<string, Func<Dictionary<string, string>, Task>> commandHandlers;
        Workbook wb;
        Worksheet ws;
        private ExcelRange selectedCell;
        private ExcelRange previousSelectedCell; // Store reference to previously selected cell
        private string LastIntent;
        ClientWebSocket client;

        double lastResult;
        List<(int Row, int Column)> foundCells;

        public Xcel(string path, int sheet, ClientWebSocket client)
        {
            this.path = path;
            commandHandlers = InitializeCommandHandlers();
            ///selectedCell = (0, 0);
            previousSelectedCell = null;
            LastIntent = null;
            this.client = client;
            foundCells = new List<(int Row, int Column)>();

            try
            {
                excel.Visible = true;
                wb = excel.Workbooks.Open(path);
                ws = wb.Worksheets[sheet];
                selectedCell = null;
               
            }
            catch (COMException ex)
            {
                Console.WriteLine($"Error opening Excel file: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw;
            }
        }
        private Dictionary<string, Func<Dictionary<string, string>, Task>> InitializeCommandHandlers()
        {
            return new Dictionary<string, Func<Dictionary<string, string>, Task>>
            {
                //----------------Versão VOZ----------------------
                ["escrever_conteudo"] = HandleWriteCommand,
                ["selecionar_celulas"] = HandleSelectCommand,
                ["alterar_tamanho_texto"] = HandleChangeSizeCommand,
                ["aumentar_tamanho_texto"] = HandleIncreaseSizeCommand,
                ["diminuir_tamanho_texto"] = HandleDecreaseSizeCommand,
                ["estilo_texto"] = HandleStyleCommand,
                ["selecionar_area"] = HandleSelectAreaCommand,
                ["change_color"] = HandleColorCommand,
                ["salvar"] = HandleSave,
                ["fechar"] = HandleTryClosing,
                ["copiar"] = HandleCopy,
                ["colar"] = HandlePaste,
                ["apagar"] = HandleDelete,
                ["ajuda"] = HandleHelp,
                ["limpar"] = HandleClearStyle,
                ["direcionar"] = HandleDirect,
                ["orientar"] = HandleLocate,
                ["matematica"] = HandleCalc,
                ["definir_limites"] = HandleBorders,

                //----------------------Versão GESTOS-----------------------
                ["scrollup"] = HandleScrollUp,
                ["scrolldown"] = HandleScrollDown,
                ["scrollleft"] = HandleScrollLeft,
                ["scrollright"] = HandleScrollRight,
                ["cortar"] = HandleCut,
                //["nextws"] = HandleNextWS,
                //["previousws"] = HandlePreviousWS,
                //["zoomin"] = HandleZoomIn,
                //["zoomout"] = HandleZoomOut,
                //Posso tambem por negrito,italico,sublinhado

                //---------------------Versão Fusion--------------------------
                ["bold_escrever_conteudo"] = HandleBoldWrite,
                ["italico_escrever_conteudo"] = HandleItalicWrite,
                ["sublinhado_escrever_conteudo"] = HandleUnderlineWrite,

                //["copiar_selecionar_celulas"] = HandleCopySelectedCell,
                ["corte_selecionar_celulas"] = HandleCutSelectedCell,
                ["colar_selecionar_celulas"] = HandlePasteSelectedCell,
                ["apagar_selecionar_celulas"] = HandleDeleteSelectedCell,

                //["copiar_selecionar_area"] = HandleCopySelectedArea,
                //["corte_selecionar_area"] = HandleCutSelectedArea,
                //["colar_selecionar_area"] = HandlePasteSelectedArea,
                //["apagar_selecionar_area"] = HandleDeleteSelectedArea

                ["lock_selecionar_celulas"] = HandleLockSelectedCell,
                ["procurar"] = HandleSearch,
                ["selecionar_coluna"] = HandleColSelect,
                ["selecionar_linha"] = HandleRowSelect,


            };
        }

        public async Task ProcessInstruction(Dictionary<string, string> nluData)
        {
            if (!nluData.TryGetValue("intent", out string intent))
            {
                await App.SendMessage(client, App.messageMMI("Nao percebi o que disse pode repetir por favor."));
                Console.WriteLine("No action specified in NLU data");
                return;
            }

            intent = intent.ToLower();

            if(LastIntent != null)
            {
                switch(LastIntent)
                {
                    case "fechar":
                        if (intent == "deny")
                        {
                            Console.WriteLine("Ok, I wont't close the file.");
                            await App.SendMessage(client, App.messageMMI("Ok, eu nao vou fechar o excel."));
                            LastIntent = null;
                        }
                        else if (intent == "affirm")
                        {
                            Console.WriteLine("Closing file");
                            await HandleConfirmClosing();
                        }
                        else
                        {
                            Console.WriteLine("Sorry I didn't get that, Do you still want to close the file?");
                            await App.SendMessage(client, App.messageMMI("Desculpa eu nao percebi, ainda queres fechar o programa?"));
                        }
                        return;
                    case "matematica":
                        if (intent == "deny")
                        {
                            Console.WriteLine("Ok vou descartar o valor calculado.");
                            await App.SendMessage(client, App.messageMMI("Ok vou descartar o valor calculado"));
                            LastIntent = null;
                            lastResult = 0;
                        }
                        else if (intent == "selecionar_celulas")
                        {
                            await HandleCelConfirmation(nluData);
                            LastIntent = null;
                            lastResult = 0;
                        }
                        else
                        {
                            Console.WriteLine("Sorry I didn't get that, Do you still want to paste the calculated val?");
                            await App.SendMessage(client, App.messageMMI("Desculpa eu nao percebi, se queres diz onde queres que coloque, se não, diz que não"));
                        }
                        return;
                    case "search":
                        if (intent == "deny")
                        {
                            if (foundCells.Count > 1)
                            {
                                foundCells.RemoveAt(0);
                                ExcelRange excelRange = ws.Cells[foundCells[0].Item1, foundCells[0].Item2];
                                SelectCell(excelRange);
                                await App.SendMessage(client, App.messageMMI($"Encontrei na linha {foundCells[0].Item1} coluna {GetColumnLetter(foundCells[0].Item2)}"));
                                await App.SendMessage(client, App.messageMMI($"É esta a celula que tu queres?"));

                            }
                            else if (foundCells.Count == 1)
                            {
                                ExcelRange excelRange = ws.Cells[foundCells[0].Item1, foundCells[0].Item2];
                                SelectCell(excelRange);
                                await App.SendMessage(client, App.messageMMI($"Ultimo celula encontrada, linha {foundCells[0].Item1} coluna {GetColumnLetter(foundCells[0].Item2)}"));
                                LastIntent = null;
                                foundCells.Clear();
                            }
                            else
                            {
                                await App.SendMessage(client, App.messageMMI("Não encontrei mais nenhuma célula?"));
                                Console.WriteLine("Error on foundCells");
                                LastIntent = null;
                                foundCells.Clear();
                            }

                        }
                        else if (intent == "affirm")
                        {
                            Console.WriteLine($"Ok, ficamos na celula {foundCells[0].Item1}, {foundCells[0].Item2} ");
                            foundCells.Clear();
                            LastIntent = null;
                        }
                        else
                        {
                            Console.WriteLine("Sorry I didn't get that, Do you still want to do the search?");
                            await App.SendMessage(client, App.messageMMI("Desculpa eu nao percebi, é esta a célula que queres?"));
                        }
                        return;
                }
            }

            switch (intent)
            {
                case "ignore":
                    Console.WriteLine("Ignoring current instruction and waiting for next message...");
                    return;

                case "greet":
                    Console.WriteLine("Hello :)");
                    await App.SendMessage(client, App.messageMMI("Olá como posso ajudar?"));
                    return;

                case "goodbye":
                    Console.WriteLine("Do you want to close the program?");
                    LastIntent = "fechar";
                    await App.SendMessage(client, App.messageMMI("Queres sair do programa? Eu posso encerralo se quiseres"));
                    return;

                case "denny":
                case "affirm":
                    Console.WriteLine("Nothing to affirm or denny I think...");
                    return;

                default:
                    if (commandHandlers.TryGetValue(intent, out var handler))
                    {
                        await handler(nluData);
                    }
                    else
                    {
                        Console.WriteLine($"Unknown intent: {intent}");
                        await App.SendMessage(client, App.messageMMI("Desculpa nao percebi o que disseste"));
                    }
                    return;
            }
        }

        public string ReadCell(int row, int column)
        {
            if (ws.Cells[row, column].Value2 != null)
            {
                return ws.Cells[row, column].Value2.ToString();
            }
            return "";
        }

        public void SelectCell(ExcelRange cell)
        {
            try
            {
                // Null check
                if (cell == null)
                {
                    throw new ArgumentNullException(nameof(cell), "Cell cannot be null");
                }

                // Clear previous cell's formatting if it exists
                if (previousSelectedCell != null)
                {
                    try
                    {
                        // Safe color reset
                        dynamic prevCell = previousSelectedCell;
                        if (prevCell.Interior.ColorIndex == 36)
                        {
                            prevCell.Interior.ColorIndex = 0; // Reset to no color
                        }
                        Marshal.ReleaseComObject(previousSelectedCell);
                    }
                    catch (Exception resetEx)
                    {
                        Console.WriteLine($"Error resetting previous cell: {resetEx.Message}");
                    }
                }

                // Select the cell and make it active
                dynamic dynamicCell = cell;
                dynamicCell.Select();
                excel.ActiveWindow.ScrollRow = dynamicCell.Row;
                excel.ActiveWindow.ScrollColumn = dynamicCell.Column;

                // Store the selected cell coordinates
                selectedCell = cell;

                /* Highlight the new cell
                try
                {
                    // Console.WriteLine("vou mudar a cor");
                    if (dynamicCell.Interior.ColorIndex != 0) { }
                    else
                    {
                        dynamicCell.Interior.ColorIndex = 36; // Light yellow
                        Console.WriteLine("mudei a cor");
                    }
                }
                catch (Exception colorEx)
                {
                    Console.WriteLine($"Error coloring cell: {colorEx.Message}");
                } */

                // Store reference to current cell for future cleanup
                previousSelectedCell = cell;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error selecting cell at row {cell?.Row ?? 0}, column {cell?.Column ?? 0}: {ex.Message}", ex);
            }
        }

        public void SelectCellLock(ExcelRange cell)
        {
            try
            {
                // Null check
                if (cell == null)
                {
                    throw new ArgumentNullException(nameof(cell), "Cell cannot be null");
                }

                // Clear previous cell's formatting if it exists
                if (previousSelectedCell != null)
                {
                    try
                    {
                        // Safe color reset
                        dynamic prevCell = previousSelectedCell;
                        if (prevCell.Interior.ColorIndex == 36)
                        {
                            prevCell.Interior.ColorIndex = 0; // Reset to no color
                        }
                        Marshal.ReleaseComObject(previousSelectedCell);
                    }
                    catch (Exception resetEx)
                    {
                        Console.WriteLine($"Error resetting previous cell: {resetEx.Message}");
                    }
                }

                // Select the cell and make it active
                dynamic dynamicCell = cell;
                dynamicCell.Select();
                

                // Store the selected cell coordinates
                selectedCell = cell;
                previousSelectedCell = cell;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error selecting cell at row {cell?.Row ?? 0}, column {cell?.Column ?? 0}: {ex.Message}", ex);
            }
        }

        public void SelectCellRange(ExcelRange startCell, ExcelRange endCell)
        {
            try
            {
                /* Clear previous cell's formatting if it exists
                if (previousSelectedCell != null)
                {
                    if (previousSelectedCell.Interior.ColorIndex = 36)
                    {
                        previousSelectedCell.Interior.ColorIndex = 0; // Reset to no color
                    }
                    Marshal.ReleaseComObject(previousSelectedCell);
                } */

                // Get the range of cells
                ExcelRange range = ws.Range[startCell, endCell];

                // Select the range and make it active
                range.Select();

                // Scroll to the start of the range
                excel.ActiveWindow.ScrollRow = range.Row;
                excel.ActiveWindow.ScrollColumn = range.Column;

                /* Highlight the range
                if (range.Interior.ColorIndex = 0)
                {
                    range.Interior.ColorIndex = 36; // Light yellow
                } */

                // Store reference to current range for future cleanup
                previousSelectedCell = range;

                // Update selected cell coordinates (use start cell)
                selectedCell = range;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error selecting cell range from ({startCell.Row}, {startCell.Column}) to ({endCell.Row}, {endCell.Column}): {ex.Message}");
            }
        }

        public void ClearSelection()
        {
            if (previousSelectedCell != null)
            {
                try
                {
                    // Clear the highlighting of the previously selected cell
                    //previousSelectedCell.Interior.ColorIndex = 0; // Reset to no color
                    Marshal.ReleaseComObject(previousSelectedCell);
                    previousSelectedCell = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing selection: {ex.Message}");
                }
            }
            selectedCell = null;
        }

        public ExcelRange GetSelectedCellCoordinates()
        {
            return selectedCell;
        }

        // Helper method to check if a cell is currently selected
        public bool IsCellSelected()
        {
            return selectedCell != null;
        }

        public void WriteToCell(ExcelRange range, string value)
        {
            range.Value2 = value;
        }

        public double GetFontSize(ExcelRange cell)
        {
            //ExcelRange cell = ws.Cells[row, column];
            double fontSize = cell.Font.Size;
            return fontSize;
        }

        public void ChangeSizeText(ExcelRange range, int size) { 
            //ExcelRange cell = ws.Cells[row, column];
            range.Font.Size = size;
        }

        async public Task ChangeTextStyle(ExcelRange cell, string style)
        {
            //ExcelRange cell = ws.Cells[row, column];
            switch (style.ToLower())
            {
                case "negrito":
                    cell.Font.Bold = true;
                    Console.WriteLine($"Changed style to bold at row {cell.Row}, column {cell.Column}");
                    await App.SendMessage(client, App.messageMMI($"Apliquei negrito na linha {cell.Row}, coluna {GetColumnLetter(cell.Column)}"));
                    break;
                case "itálico":
                    cell.Font.Italic = true;
                    Console.WriteLine($"Changed style to italic at row {cell.Row}, column {cell.Column}");
                    await App.SendMessage(client, App.messageMMI($"Apliquei itálico na linha {cell.Row}, coluna {GetColumnLetter(cell.Column)}"));
                    break;
                case "sublinhado":
                    cell.Font.Underline = XlUnderlineStyle.xlUnderlineStyleSingle;
                    await App.SendMessage(client, App.messageMMI($"Apliquei sublinhado na linha {cell.Row}, coluna {GetColumnLetter(cell.Column)}"));
                    Console.WriteLine($"Changed style to underline at row {cell.Row}, column {GetColumnLetter(cell.Column)}");
                    break;
                default:
                    Console.WriteLine("Unknown style.");
                    await App.SendMessage(client, App.messageMMI($"Desculpa nao percebi que estilo queres aplicar, aceito negrito italico e sublinhado"));
                    break;
            }
        }

        async public Task ChangeCellColor(ExcelRange cell, string colorType, Color color, string corName)
        {
            // Seleciona a célula especificada
            // ExcelRange cell = ws.Cells[row, column];


            // Aplica a cor com base no tipo especificado
            switch (colorType.ToLower())
            {
                case "texto":
                    cell.Font.Color = ColorTranslator.ToOle(color);
                    Console.WriteLine($"Changed text color at {cell.Row}, column {cell.Column}");
                    await App.SendMessage(client, App.messageMMI($"Mudei a cor do {colorType} para {corName} na linha {cell.Row}, coluna {GetColumnLetter(cell.Column)}"));
                    break;
                case "preenchimento":
                    cell.Interior.Color = ColorTranslator.ToOle(color);
                    Console.WriteLine($"Changed preechimento color at row {cell.Row}, column {cell.Column}");
                    await App.SendMessage(client, App.messageMMI($"Mudei a cor do {colorType} para {corName} na linha {cell.Row}, coluna {GetColumnLetter(cell.Column)}"));
                    break;
                default:
                    Console.WriteLine("Unknown type. Use 'texto' ou 'preenchimento'.");
                    await App.SendMessage(client, App.messageMMI("Desculpa nao percebi onde queres aplicar a cor, posso mudar a cor do texto ou do preenchimento"));
                    break;
            }
        }

        public void Copy(ExcelRange cell) {
            cell.Copy();
        }

        public void Delete(ExcelRange cell)
        {
            cell.Value2 = "";
        }

        public void Paste(ExcelRange cell)
        {
            cell.PasteSpecial();
        }

        public void ClearStyle(ExcelRange cell)
        {
            // Clear number formatting
            // Clear number formatting
            cell.NumberFormat = "@";

            // Clear font styling
            cell.Font.Bold = false;
            cell.Font.Italic = false;
            cell.Font.Underline = false;
            cell.Font.Name = "Calibri";
            cell.Font.Size = 11;
            cell.Font.Color = Color.Black;

            // Clear cell interior (background)
            cell.Interior.Pattern = XlPattern.xlPatternNone;
            Console.WriteLine("NÂO passei  o branvo");
            cell.Interior.ColorIndex = 0;

            Console.WriteLine("Passei o branvo");

            // Clear border styles
            cell.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlLineStyleNone;
            cell.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlLineStyleNone;
            cell.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlLineStyleNone;
            cell.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlLineStyleNone;
            cell.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlLineStyleNone;
            cell.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlLineStyleNone;

            // Clear text alignment
            cell.HorizontalAlignment = XlHAlign.xlHAlignGeneral;
            cell.VerticalAlignment = XlVAlign.xlVAlignBottom;

            // Clear text wrapping
            cell.WrapText = false;

            // Reset text orientation
            cell.Orientation = 0;
        }

        async public Task ChangePosition(ExcelRange cell, string direcao)
        {
            ExcelRange range;
            switch (direcao.ToLower())
            {
                case "cima":
                case "acima":
                    try {
                        range = ws.Cells[cell.Row - 1, cell.Column];
                        SelectCell(range);
                        Console.WriteLine($"Moved to row {range.Row}, column {range.Column}");
                        await App.SendMessage(client, App.messageMMI("Movi me uma célula para cima"));
                        break;
                    }
                    catch {
                        Console.WriteLine("Invalid can't go up any more");
                        await App.SendMessage(client, App.messageMMI("Estamos na linha A, já não posso subir mais"));
                        break;
                    }
                case "baixo":
                case "abaixo":
                    range = ws.Cells[cell.Row + 1, cell.Column];
                    SelectCell(range);
                    Console.WriteLine($"Moved to row {range.Row}, column {range.Column}");
                    await App.SendMessage(client, App.messageMMI("Movi me uma célula para baixo"));
                    break;

                case "esquerda":
                case "anterior":
                    try
                    {
                        range = ws.Cells[cell.Row, cell.Column - 1];
                        SelectCell(range);
                        Console.WriteLine($"Moved to row {range.Row}, column {range.Column}");
                        await App.SendMessage(client, App.messageMMI("Movi me uma célula para a esquerda"));
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Invalid can't go left any more");
                        await App.SendMessage(client, App.messageMMI("Estamos na coluna 1, já não posso ir mais para a esquerda"));
                        break;
                    }
                case "direita":
                case "próximo":
                case "seguinte":
                    range = ws.Cells[cell.Row, cell.Column + 1];
                    SelectCell(range);
                    Console.WriteLine($"Moved to row {range.Row}, column {range.Column}");
                    await App.SendMessage(client, App.messageMMI("Movi me uma célula para direita"));
                    break;

                default:
                    Console.WriteLine("Unknown style.");
                    await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que direção queres ir"));
                    break;
            }

        }
        async public Task ChangeBorder(ExcelRange range, string borda)
        {
            switch (borda.ToLower())
            {
                case "inferior":
                    range.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlThick;
                    Console.WriteLine("Lower border aply");
                    await App.SendMessage(client, App.messageMMI("Inseri limite inferior a tua secção"));
                    break;

                case "superior":
                    range.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlThick;
                    Console.WriteLine("Top border aply");
                    await App.SendMessage(client, App.messageMMI("Inseri limite superior a tua secção"));
                    break;

                case "sem":
                    range.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlLineStyleNone;
                    range.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlLineStyleNone;
                    range.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlLineStyleNone;
                    range.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlLineStyleNone;
                    range.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlLineStyleNone;
                    range.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlLineStyleNone;
                    Console.WriteLine("No border aply");
                    await App.SendMessage(client, App.messageMMI("Apliquei sem limites na tua secção"));
                    break;
                   
                case "todos":
                    range.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlThick;
                    range.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlThick;
                    range.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlThick;
                    range.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlThick;
                    // Set thinner internal borders for cells inside the range
                    range.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlInsideHorizontal].Weight = XlBorderWeight.xlThin;
                    range.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlInsideVertical].Weight = XlBorderWeight.xlThin;
                    await App.SendMessage(client, App.messageMMI("Apliquei todos os limites na tua secção"));
                    break;

                case "esquerda":
                case "esquerdo":
                    range.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlThick;
                    Console.WriteLine("Left border aply");
                    await App.SendMessage(client, App.messageMMI("Inseri limite esquedo a tua secção"));
                    break;

                case "direita":
                case "direito":
                    range.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlThick;
                    Console.WriteLine("Right border aply");
                    await App.SendMessage(client, App.messageMMI("Inseri limite direito a tua secção"));
                    break;
                case "exteriores":
                    range.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlThick;
                    range.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlThick;
                    range.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlThick;
                    range.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlThick;
                    Console.WriteLine("Outside border aply");
                    await App.SendMessage(client, App.messageMMI("Inseri limite exteriores a tua secção"));
                    break;

                default:
                    Console.WriteLine("Unknown style.");
                    await App.SendMessage(client, App.messageMMI("Desculpa nao percebi que tipo de borda queres que aplique"));
                    break;
            }

        }
        async public Task Calculate(ExcelRange range, string operacao)
        {
            try
            {
                switch(operacao.ToLower())
                {
                    case "soma":
                        double sum = (double)range.Application.WorksheetFunction.Sum(range);
                        lastResult = sum;
                        LastIntent = "matematica";
                        Console.WriteLine("Sum done operation");
                        await App.SendMessage(client, App.messageMMI("Calculei a tua soma onde queres que a coloque?"));
                        break;
                    case "média":
                        double average = (double)range.Application.WorksheetFunction.Average(range);
                        lastResult = average;
                        LastIntent = "matematica";
                        Console.WriteLine("Sum done operation");
                        await App.SendMessage(client, App.messageMMI("Calculei a tua média onde queres que a coloque?"));
                        break;
                    case "mediana":
                        double median = (double)range.Application.WorksheetFunction.Median(range);
                        lastResult = median;
                        LastIntent = "matematica";
                        Console.WriteLine("Sum done operation");
                        await App.SendMessage(client, App.messageMMI("Calculei a tua mediana onde queres que a coloque?"));
                        break;
                    case "maximo":
                    case "máximo":
                        double max = (double)range.Application.WorksheetFunction.Max(range);
                        lastResult = max;
                        LastIntent = "matematica";
                        Console.WriteLine("Sum done operation");
                        await App.SendMessage(client, App.messageMMI("Calculei o teu máximo onde queres que a coloque?"));
                        break;
                    case "mínimo":
                    case "minimo":
                        double min = (double)range.Application.WorksheetFunction.Min(range);
                        lastResult = min;
                        LastIntent = "matematica";
                        Console.WriteLine("Sum done operation");
                        await App.SendMessage(client, App.messageMMI("Calculei o teu minimo onde queres que a coloque?"));
                        break;
                    default:
                        Console.WriteLine("Unknow operation");
                        await App.SendMessage(client, App.messageMMI("Desculpa nao percebi que operação queres fazer"));
                        break;
                }
            }
            catch {
                await App.SendMessage(client, App.messageMMI("Alguma coisa deu errado, se calhar estas a passar argumentos inválidos"));
            }
        }

        public bool IsSingleCell(ExcelRange range)
        {
            // Check if the range is a single cell (1 row and 1 column)
            return range.Rows.Count == 1 && range.Columns.Count == 1;
        }
        public void Save()
        {
            wb.Save();
        }
        public void Close() {
            ClearSelection(); // Clear any selection highlighting before closing
            wb.Close(false);
            excel.Quit();

            // Release COM objects
            if (previousSelectedCell != null)
            {
                Marshal.ReleaseComObject(previousSelectedCell);
            }
            Marshal.ReleaseComObject(ws);
            Marshal.ReleaseComObject(wb);
            Marshal.ReleaseComObject(excel);
        }

        public List<(int Row, int Column)> FindAllCellsWithValue(string searchValue)
        {
            List<(int Row, int Column)> foundCells = new List<(int Row, int Column)>();

            // Get the used range of the worksheet
            ExcelRange usedRange = ws.UsedRange;

            // Get the values into a 2D array for faster processing
            object[,] values = usedRange.Value2;

            // Get the dimensions
            int rows = usedRange.Rows.Count;
            int cols = usedRange.Columns.Count;

            // Search through all cells
            for (int row = 1; row <= rows; row++)
            {
                for (int col = 1; col <= cols; col++)
                {
                    if (values[row, col]?.ToString()?.Equals(searchValue, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Add the cell position to our results
                        // Adding the offset if the used range doesn't start at A1
                        foundCells.Add((
                            row + usedRange.Row - 1,
                            col + usedRange.Column - 1
                        ));
                    }
                }
            }

            return foundCells;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private async Task HandleSelectCommand(Dictionary<string, string> data)
        {
            if (!ValidateRequiredFields(data, "celula")) {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que celula queres mudar"));
                Console.WriteLine("Missing cell value");
                return; 
            
            } // Exit and wait for next message
            try { 
                ExcelRange range = ParseExcelReference(data["celula"]);
                SelectCell(range);
                Console.WriteLine($"Selected cell at row {range.Row}, column {range.Column}");
                await App.SendMessage(client, App.messageMMI($"Selecionada a celula na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                await Task.CompletedTask;
            }
            catch{ 
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }

            //execution
            
        }

        private async Task HandleWriteCommand(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "valor")) {
                await App.SendMessage(client, App.messageMMI("Desculpe nao percebi o que queres escrever"));
                Console.WriteLine("Invalid cell");
                return; 
            } 
           
            ExcelRange range = GetSelectedCellCoordinates();
            
         
            if (!IsCellSelected()){
                await App.SendMessage(client, App.messageMMI($"Antes de poderes escrever tens de indicar uma celula"));
                Console.WriteLine($"No cell provided to write on");
                return;
            }
            string value = data["valor"];


            //execution
            WriteToCell(range, value);
            Console.WriteLine($"Writed {value} at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Escrevi {value} na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }
  
        private async Task HandleChangeSizeCommand(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "tamanho")) {
                await App.SendMessage(client, App.messageMMI($"Desculpa nao percebi para que tamanho queres mudar"));
                return; 
            } // Exit and wait for next message
            ExcelRange range = GetSelectedCellCoordinates();

            //if (data.ContainsKey("celula"))
            //{
                //range = ParseExcelReference(data["celula"]);
            //}

            if (!IsCellSelected())
            {
                Console.WriteLine($"No cell provided to write on");
                await App.SendMessage(client, App.messageMMI($"Por favor escolha primeiro a celula que quer mudar o tamanho"));
                return;
            }

            //ou é aumenta ou é diminuir ou é um valor expecífico ;)
            int value = ParseInt(data["tamanho"]);


            //execution
            ChangeSizeText(range, value);
            Console.WriteLine($"Changed size to {value} at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Mudei o tamanho  da celula {range.Row}, {GetColumnLetter(range.Column)} para {value}"));
            await Task.CompletedTask;
        }

        private async Task HandleIncreaseSizeCommand(Dictionary<string, string> data)
        {
            //validation
            //if (!ValidateRequiredFields(data, "tamanho")) { return; } // Exit and wait for next message
            ExcelRange range = GetSelectedCellCoordinates();
            //if (data.ContainsKey("celula"))
            //{
            //    range = ParseExcelReference(data["celula"]);
            //}
            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Por favor escolha primeiro a celula que quer mudar o tamanho"));
                return;
            }

            //ou é aumenta ou é diminuir ou é um valor expecífico 
            int value;
            if (data.ContainsKey("tamanho"))
            {
                value = ParseInt(data["tamanho"]);
            }
            else
            {
                value = (int)GetFontSize(range) + 2 ;

            }
                //execution
            ChangeSizeText(range, value);
            Console.WriteLine($"Changed size to {value} at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Mudei o tamanho  da celula {range.Row}, {GetColumnLetter(range.Column)} para {value}"));
            await Task.CompletedTask;
        }

        private async Task HandleDecreaseSizeCommand(Dictionary<string, string> data)
        {
            //validation
            //if (!ValidateRequiredFields(data, "tamanho")) { return; } // Exit and wait for next message
            //(int row, int column) = GetSelectedCellCoordinates();
            ExcelRange range = GetSelectedCellCoordinates();

            //if (data.ContainsKey("celula"))
            //{
            //    range = ParseExcelReference(data["celula"]);
            //}

            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Por favor escolha primeiro a celula que quer mudar o tamanho"));
                return;
            }

            //ou é aumenta ou é diminuir ou é um valor expecífico 
            int value;
            if (data.ContainsKey("tamanho"))
            {
                value = ParseInt(data["tamanho"]);
            }
            else
            {
                value = (int)GetFontSize(range) - 2;

            }
            if (value  <= 0) {
                await App.SendMessage(client, App.messageMMI($"Tamanho nao pode ser zero ou negativo"));
                return;
            }

            //execution
            ChangeSizeText(range, value);
            Console.WriteLine($"Changed size to {value} at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Mudei o tamanho  da celula {range.Row}, {GetColumnLetter(range.Column)} para {value}"));
            await Task.CompletedTask;
        }

        private async Task HandleStyleCommand(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "estilo")) {
                await App.SendMessage(client, App.messageMMI($"Desculpa nao percebi que estilo queres aplicar, aceito negrito italico e sublinhado"));
                return; 
            } // Exit and wait for next message

            ExcelRange range = GetSelectedCellCoordinates();
            if (data.ContainsKey("celula"))
            {
                range = ParseExcelReference(data["celula"]);
            }
            else if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Por favor escolha primeiro a celula que quer mudar o estilo"));
                return;
            }

            string value = data["estilo"];

            //execution
            await ChangeTextStyle(range, value);
            await Task.CompletedTask;
        }

        private async Task HandleColorCommand(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "color")) {
                await App.SendMessage(client, App.messageMMI($"Desculpa nao percebi a cor, eu sei apenas as cores do arco iris"));
                return; 
            } // Exit and wait for next message
            else if (!ValidateRequiredFields(data, "shape")){
                await App.SendMessage(client, App.messageMMI($"Desculpa nao percebi onde queres aplicar a cor, posso mudar a cor do texto ou do preenchimento"));
                return;
            }
            ExcelRange range = GetSelectedCellCoordinates();
            /*if (data.ContainsKey("celula"))
            {
                range = ParseExcelReference(data["celula"]);
            }*/
            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Por favor escolha primeiro a celula que quer mudar a cor"));
                return;
            }

            string corName = data["color"];
            string colorType = data["shape"];

            Color cor = ConvertPortugueseColorToColor(corName);

            if(cor == Color.Brown)
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi a cor, eu sei apenas as cores do arco iris"));
                return;
            }

            //execution
            await ChangeCellColor(range, colorType, cor, corName);
            // await App.SendMessage(client, App.messageMMI($"Mudei a cor do {colorType} para {corName} na linha {range.Row}, coluna {range.Column}"));
            await Task.CompletedTask;
        }

        private async Task HandleSelectAreaCommand(Dictionary<string, string> data)
        { 
            // Execution
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi ate que celula queres criar uma area"));
                Console.WriteLine("Missing cell value");
                return;

            } 
            try{
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro a celula onde a area vai comecar"));
                    return;
                }
                ExcelRange endCell = ParseExcelReference(data["celula"]);
                ExcelRange startCell = GetSelectedCellCoordinates();

                int startRow = Math.Min(startCell.Row, endCell.Row);
                int startColumn = Math.Min(startCell.Column, endCell.Column);
                int endRow = Math.Max(startCell.Row, endCell.Row);
                int endColumn = Math.Max(startCell.Column, endCell.Column);

                ExcelRange range = ws.Range[ws.Cells[startRow, startColumn], ws.Cells[endRow, endColumn]];

                SelectCell(range);
                Console.WriteLine($"Selected cell range from ({startRow}, {startColumn}) to ({endRow}, {endColumn})");
                await App.SendMessage(client, App.messageMMI($"Area selecionada da linha {startRow}, coluna {GetColumnLetter(startColumn)} ate a linha {endRow}, coluna {GetColumnLetter(endColumn)}"));
                await Task.CompletedTask;
            }
            catch{
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }
        }

        private async Task HandleCopy(Dictionary<string, string> data)
        {
            ExcelRange range;
            if (data.ContainsKey("celula"))
            {
                try
                {
                    range = ParseExcelReference(data["celula"]);
                } catch {
                    await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                    Console.WriteLine("Invalid cell");
                    return;
                }
            } else {
                range = GetSelectedCellCoordinates();
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro a celula que queres copiar"));
                    return;
                }
            }

            //execution
            Copy(range);
            Console.WriteLine($"Copied value at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Copiei o conteudo da celula {range.Row}, {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }

        private async Task HandlePaste(Dictionary<string, string> data)
        {
            ExcelRange range;
            if (data.ContainsKey("celula"))
            {
                try
                {
                    range = ParseExcelReference(data["celula"]);
                }
                catch
                {
                    await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                    Console.WriteLine("Invalid cell");
                    return;
                }
            }
            else
            {
                range = GetSelectedCellCoordinates();
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro a celula onde queres colar"));
                    return;
                }
            }

            //execution
            Paste(range);
            Console.WriteLine($"Pasted value at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Colei na celula {range.Row}, {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }

        private async Task HandleDelete(Dictionary<string, string> data)
        {
            ExcelRange range;
            if (data.ContainsKey("celula"))
            {
                try
                {
                    range = ParseExcelReference(data["celula"]);
                }
                catch
                {
                    await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                    Console.WriteLine("Invalid cell");
                    return;
                }
            }
            else
            {
                range = GetSelectedCellCoordinates();
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolhe primeiro de que celula queres que eu apague"));
                    return;
                }
            }

            //execution
            Console.WriteLine($"Deleted value at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Apaguei o que estava na celula da linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
            Delete(range);
            await Task.CompletedTask;
        }

        private async Task HandleHelp(Dictionary<string, string> data)
        {
            Console.WriteLine("Help command received");

            string projectRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            projectRoot = Directory.GetParent(projectRoot).Parent.Parent.FullName;
            string filePath = Path.Combine(projectRoot, "help.txt");

            if (File.Exists(filePath))
            {
                string helpText = File.ReadAllText(filePath);
                await App.SendMessage(client, App.messageMMI(helpText));
            }
            else
            {
                // Handle the case where the file does not exist
                Console.WriteLine("The file help.txt was not found.");
                Console.WriteLine(filePath);
                await App.SendMessage(client, App.messageMMI("Desculpa, não consegui carregar o texto de ajuda"));
                return;
            }

            await Task.CompletedTask;
        }

        private async Task HandleClearStyle(Dictionary<string, string> data)
        {
            ExcelRange range = GetSelectedCellCoordinates();
            if (data.ContainsKey("celula"))
            {
                range = ParseExcelReference(data["celula"]);
            }
            else if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Por favor escolha primeiro a celula que queres remover os estilos"));
                return;
            }

            ClearStyle(range);

            Console.WriteLine($"Cleared all styles at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Removi os estilos da celula {range.Row}, {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;

        }

        private async Task HandleDirect(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "direcao"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para onde queres te mover"));
                return;
            } // Exit and wait for next message

           
           if (!IsCellSelected())
           {
                await App.SendMessage(client, App.messageMMI("Para começar primeiro tens de selecionar uma célula"));
                return;
           }

            ExcelRange range = GetSelectedCellCoordinates();

            string value = data["direcao"];
            await ChangePosition(range, value);
            await Task.CompletedTask;

        }

        private async Task HandleLocate(Dictionary<string, string> data) {
            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI("Para começar primmeiro tens de selecionar uma célula"));
                return;
            }           
            ExcelRange range = GetSelectedCellCoordinates();

            if (IsSingleCell(range))
            {
                await App.SendMessage(client, App.messageMMI($"Estás com uma única célula selecionada linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                Console.WriteLine($" {range.Row}, {range.Column}");
            } else {
                string address = range.Address;
                string upperLeft = address.Split(':')[0];  // "A1"
                string lowerRight = address.Split(':')[1];  // "B10"
                Console.WriteLine($"Upper-left: {upperLeft}");
                Console.WriteLine($"Lower-right: {lowerRight}");
                await App.SendMessage(client, App.messageMMI($"Estás com várias célula selecionadas, o teu bloco começa na célula {upperLeft} e acaba na {lowerRight}"));
            }
            await Task.CompletedTask;
        }

        private async Task HandleBorders(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "borda"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que tipo de borda queres"));
                return;
            } // Exit and wait for next message

            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI("Para começar primeiro tens de selecionar uma célula"));
                return;
            }

            ExcelRange range = GetSelectedCellCoordinates();

            string value = data["borda"];
            await ChangeBorder(range, value);
            await Task.CompletedTask;
        }

        private async Task HandleCalc(Dictionary<string, string> data) {
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Preciso de uma área para calculos, diz-me até que célula queres calcular"));
                Console.WriteLine("Missing cell value");
                return;
            }
            if (!ValidateRequiredFields(data, "formula"))
            {
                await App.SendMessage(client, App.messageMMI("Não apanhei que tipo conta queres fazer"));
                Console.WriteLine("Missing formula value");
                return;
            }

            try
            {
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro a celula onde vai comecar"));
                    return;
                }
                ExcelRange endCell = ParseExcelReference(data["celula"]);
                ExcelRange startCell = GetSelectedCellCoordinates();

                int startRow = Math.Min(startCell.Row, endCell.Row);
                int startColumn = Math.Min(startCell.Column, endCell.Column);
                int endRow = Math.Max(startCell.Row, endCell.Row);
                int endColumn = Math.Max(startCell.Column, endCell.Column);

                ExcelRange range = ws.Range[ws.Cells[startRow, startColumn], ws.Cells[endRow, endColumn]];
                string operation = data["formula"];

                await Calculate(range, operation);

                //Console.WriteLine($"Selected cell range from ({startRow}, {startColumn}) to ({endRow}, {endColumn})");
                //await App.SendMessage(client, App.messageMMI($"Area selecionada da linha {startRow}, coluna {startColumn} ate a linha {endRow}, coluna {endColumn}"));
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Ocurreu um erro"));
                Console.WriteLine("Invalid cell");
                return;
            }

        }

        async public Task HandleCelConfirmation(Dictionary<string, string> data) {
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que celula queres mudar"));
                Console.WriteLine("Missing cell value");
                return;

            } // Exit and wait for next message
            try
            {
                ExcelRange range = ParseExcelReference(data["celula"]);
                WriteToCell(range, lastResult.ToString());
                await App.SendMessage(client, App.messageMMI($"Coloquei o resultado na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }
        }

        private async Task HandleSave(Dictionary<string, string> data)
        {
            Save();
            Console.WriteLine("File saved");
            await App.SendMessage(client, App.messageMMI($"Ficheiro guardado"));
            await Task.CompletedTask;

        }

        private async Task HandleTryClosing(Dictionary<string, string> data)
        {
            LastIntent = "fechar";
            await App.SendMessage(client, App.messageMMI($"Eu percebi que querias fechar o excel, tens a certeza?"));
            await Task.CompletedTask;
        }

        private async Task HandleConfirmClosing()
        {
            await App.SendMessage(client, App.messageMMI($"Ok fechei o excel, espero te ver de novo numa proxima vez"));
            Close();
            await Task.CompletedTask;
        }

        private async Task HandleScrollUp(Dictionary<string, string> nluData)
        {
            // Scroll up by 1 row
            ws.Application.ActiveWindow.SmallScroll(Up: 1);

            Console.WriteLine("Scrolled up by 1 row");
            await App.SendMessage(client, App.messageMMI("Scroll para cima"));
            await Task.CompletedTask;
        }

        private async Task HandleScrollDown(Dictionary<string, string> nluData)
        {
            // Scroll down by 1 row
            ws.Application.ActiveWindow.SmallScroll(Down: 1);

            Console.WriteLine("Scrolled down by 1 row");
            await App.SendMessage(client, App.messageMMI("Scroll para baixo"));
            await Task.CompletedTask;
        }

        private async Task HandleScrollLeft(Dictionary<string, string> nluData)
        {
            // Scroll left by 1 column
            ws.Application.ActiveWindow.SmallScroll(ToLeft: 1);

            Console.WriteLine("Scrolled left by 1 column");
            await App.SendMessage(client, App.messageMMI("Scroll para a esquerda"));
            await Task.CompletedTask;
        }

        private async Task HandleScrollRight(Dictionary<string, string> nluData)
        {
            // Scroll right by 1 column
            ws.Application.ActiveWindow.SmallScroll(ToRight: 1);

            Console.WriteLine("Scrolled right by 1 column");
            await App.SendMessage(client, App.messageMMI("Scroll para a direita"));
            await Task.CompletedTask;
        }

        private async Task HandleCut(Dictionary<string, string> data)
        {
            ExcelRange range;
            if (data.ContainsKey("celula"))
            {
                try
                {
                    range = ParseExcelReference(data["celula"]);
                }
                catch
                {
                    await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                    Console.WriteLine("Invalid cell");
                    return;
                }
            }
            else
            {
                range = GetSelectedCellCoordinates();
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro a celula que queres copiar"));
                    return;
                }
            }
            //execution
            Copy(range);
            Delete(range);
            Console.WriteLine($"Cutted value at row {range.Row}, column {range.Column}");
            await App.SendMessage(client, App.messageMMI($"Cortei o conteudo da celula {range.Row}, {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }

        private async Task HandleBoldWrite(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "valor"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpe nao percebi o que queres escrever"));
                Console.WriteLine("Invalid cell");
                return;
            }

            ExcelRange range = GetSelectedCellCoordinates();


            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Antes de poderes escrever tens de indicar uma celula"));
                Console.WriteLine($"No cell provided to write on");
                return;
            }
            string value = data["valor"];


            //execution
            WriteToCell(range, value);
            await ChangeTextStyle(range, "negrito");
            Console.WriteLine($"Writed {value} at row {range.Row}, column {range.Column}, Bold Style");
            await App.SendMessage(client, App.messageMMI($"Escrevi {value}, estilo negrito na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }

        private async Task HandleItalicWrite(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "valor"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpe nao percebi o que queres escrever"));
                Console.WriteLine("Invalid cell");
                return;
            }

            ExcelRange range = GetSelectedCellCoordinates();


            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Antes de poderes escrever tens de indicar uma celula"));
                Console.WriteLine($"No cell provided to write on");
                return;
            }
            string value = data["valor"];


            //execution
            WriteToCell(range, value);
            await ChangeTextStyle(range, "itálico");
            Console.WriteLine($"Writed {value} at row {range.Row}, column {range.Column}, Bold Style");
            await App.SendMessage(client, App.messageMMI($"Escrevi {value}, estilo italico na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }

        private async Task HandleUnderlineWrite(Dictionary<string, string> data)
        {
            //validation
            if (!ValidateRequiredFields(data, "valor"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpe nao percebi o que queres escrever"));
                Console.WriteLine("Invalid cell");
                return;
            }

            ExcelRange range = GetSelectedCellCoordinates();

            if (!IsCellSelected())
            {
                await App.SendMessage(client, App.messageMMI($"Antes de poderes escrever tens de indicar uma celula"));
                Console.WriteLine($"No cell provided to write on");
                return;
            }
            string value = data["valor"];


            //execution
            WriteToCell(range, value);
            await ChangeTextStyle(range, "sublinhado");
            Console.WriteLine($"Writed {value} at row {range.Row}, column {range.Column}, Bold Style");
            await App.SendMessage(client, App.messageMMI($"Escrevi {value}, estilo sublinhado na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
            await Task.CompletedTask;
        }

        private async Task HandleCutSelectedCell(Dictionary<string, string> data)
        {
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que celula queres mudar"));
                Console.WriteLine("Missing cell value");
                return;

            } // Exit and wait for next message
            try
            {
                ExcelRange range = ParseExcelReference(data["celula"]);
                SelectCell(range);
                Copy(range);
                Delete(range);
                Console.WriteLine($"Cut cell at row {range.Row}, column {range.Column}");
                await App.SendMessage(client, App.messageMMI($"Cortei o valor da celula na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }
        }
        
        private async Task HandlePasteSelectedCell(Dictionary<string, string> data)
        {
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que celula queres mudar"));
                Console.WriteLine("Missing cell value");
                return;

            } // Exit and wait for next message
            try
            {
                ExcelRange range = ParseExcelReference(data["celula"]);
                SelectCell(range);
                Paste(range);
                Console.WriteLine($"Pasted cell at row {range.Row}, column {range.Column}");
                await App.SendMessage(client, App.messageMMI($"Colei o valor da celula na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }
        }
        
        private async Task HandleDeleteSelectedCell(Dictionary<string, string> data)
        {
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que celula queres mudar"));
                Console.WriteLine("Missing cell value");
                return;

            } // Exit and wait for next message
            try
            {
                ExcelRange range = ParseExcelReference(data["celula"]);
                SelectCell(range);
                Delete(range);
                Console.WriteLine($"Deelted cell at row {range.Row}, column {range.Column}");
                await App.SendMessage(client, App.messageMMI($"Apaguei o valor da celula na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }
        }
        
        private async Task HandleLockSelectedCell(Dictionary<string, string> data)
        {
            if (!ValidateRequiredFields(data, "celula"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpa nao percebi para que celula queres mudar"));
                Console.WriteLine("Missing cell value");
                return;

            } // Exit and wait for next message
            try
            {
                ExcelRange range = ParseExcelReference(data["celula"]);
                SelectCellLock(range);
                Console.WriteLine($"Selecionada a celula na linha {range.Row}, coluna {range.Column}");
                await App.SendMessage(client, App.messageMMI($"Selecionada a celula na linha {range.Row}, coluna {GetColumnLetter(range.Column)}"));
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Valor de celula invalido, podes repetir por favor"));
                Console.WriteLine("Invalid cell");
                return;
            }
        }
        
        private async Task HandleSearch(Dictionary<string, string> data)
        {
            if (!ValidateRequiredFields(data, "valor"))
            {
                await App.SendMessage(client, App.messageMMI("Desculpe não percebi que valor quer procurar"));
                Console.WriteLine("Missing value");
                return;
            }

            try
            {
                if (!IsCellSelected())
                {
                    await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro a celula onde vai comecar"));
                    return;
                }
                List<(int,int)> FoundCells = FindAllCellsWithValue(data["valor"]);
                if (FoundCells.Count == 0)
                {
                    await App.SendMessage(client, App.messageMMI($"Não encontrei {data["valor"]}"));
                    return;
                }
                else if (FoundCells.Count == 1)
                {
                    ExcelRange excelRange = ws.Cells[FoundCells[0].Item1, FoundCells[0].Item2];
                    SelectCell(excelRange);
                    await App.SendMessage(client, App.messageMMI($"Encontrei {data["valor"]} na linha {FoundCells[0].Item1} coluna {GetColumnLetter(FoundCells[0].Item2)}"));
                }
                else
                {
                    await App.SendMessage(client, App.messageMMI($"Encontrei {data["valor"]} em {FoundCells.Count} células"));
                    Console.WriteLine($"Found {FoundCells.Count} cells with value {data["valor"]}");

                    ExcelRange excelRange = ws.Cells[FoundCells[0].Item1, FoundCells[0].Item2];
                    SelectCell(excelRange);
                    await App.SendMessage(client, App.messageMMI($"Encontrei {data["valor"]} na linha {FoundCells[0].Item1} coluna {GetColumnLetter(FoundCells[0].Item2)}. É esta a celula que tu queres?"));
                    LastIntent = "search";
                    foundCells = FoundCells;
                    //await App.SendMessage(client, App.messageMMI($"E esta a celula que tu queres?"));
                }
                await Task.CompletedTask;
            }
            catch
            {
                await App.SendMessage(client, App.messageMMI("Ocurreu um erro"));
                Console.WriteLine("Invalid cell");
                return;
            }

        }

        private async Task HandleColSelect(Dictionary<string, string> data)
        {
            // Get the currently selected cell
            ExcelRange Cell = GetSelectedCellCoordinates();
            if (Cell == null)
            {
                Console.WriteLine("No cell selected");
                await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro uma celula"));
                return;
            }

            // Get the column of the selected cell
            int column = Cell.Column;

            // Select the entire column
            ExcelRange range = ws.Columns[column].Select();
            previousSelectedCell = range;
            selectedCell = range;

            Console.WriteLine($"Selected entire column {GetColumnLetter(column)}");
            await App.SendMessage(client, App.messageMMI($"Selecionada a coluna {GetColumnLetter(column)}"));
            await Task.CompletedTask;
        }

        private async Task HandleRowSelect(Dictionary<string, string> data)
        {
            // Get the currently selected cell
            ExcelRange Cell = GetSelectedCellCoordinates();
            if (Cell == null)
            {
                Console.WriteLine("No cell selected");
                await App.SendMessage(client, App.messageMMI("Por favor escolha primeiro uma celula"));
                return;
            }

            // Get the column of the selected cell
            int row = Cell.Row;

            // Select the entire column
            ExcelRange range = ws.Columns[row].Select();
            previousSelectedCell = range;
            selectedCell = range;

            Console.WriteLine($"Selected entire row {row}");
            await App.SendMessage(client, App.messageMMI($"Selecionada a linha {row}"));
            await Task.CompletedTask;
        }

        private bool ValidateRequiredFields(Dictionary<string, string> data, params string[] requiredFields)
        {
            bool isValid = true;
            List<string> missingFields = new List<string>();

            foreach (var field in requiredFields)
            {
                if (!data.ContainsKey(field))
                {
                    missingFields.Add(field);
                    isValid = false;
                }
            }

            if (!isValid)
            {
                Console.WriteLine($"Missing required fields: {string.Join(", ", missingFields)}");
                Console.WriteLine("Please provide the missing information in your next message.");
            }

            return isValid;
        }

        private ExcelRange ParseExcelReference(string reference)
        {
            // Regex to separate letters and numbers
            // Will match strings like "A1", "AA12", etc.
            var match = Regex.Match(reference.ToUpper(), @"([A-Z]+)(\d+)");

            if (!match.Success)
            {
                throw new ArgumentException($"Invalid Excel reference format: {reference}. Expected format like 'A1' or 'B12'");
                
            }

            string columnStr = match.Groups[1].Value;
            string rowStr = match.Groups[2].Value;

            // Convert row string to number (direct conversion)
            if (!int.TryParse(rowStr, out int row))
            {
                throw new ArgumentException($"Invalid row number in reference: {reference}");
            }

            // Convert column string to number (A=1, B=2, AA=27, etc.)
            int column = 0;
            for (int i = 0; i < columnStr.Length; i++)
            {
                column *= 26;
                column += (columnStr[i] - 'A' + 1);
            }

            //return (row, column);
            ExcelRange range = ws.Cells[row, column];
            return range;
        }

        private int ParseInt(string value)
        {
            if (!int.TryParse(value, out int result))
            {
                throw new ArgumentException($"Invalid integer value: {value}");
            }
            return result;
        }
        private string GetColumnLetter(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }
        private Color ConvertPortugueseColorToColor(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "vermelho":
                    return Color.Red;
                case "azul":
                    return Color.Blue;
                case "verde":
                    return Color.Green;
                case "amarelo":
                    return Color.Yellow;
                case "preto":
                    return Color.Black;
                case "branco":
                    return Color.White;
                case "cinza":
                    return Color.Gray;
                case "rosa":
                    return Color.Pink;
                case "laranja":
                    return Color.Orange;
                case "roxo":
                    return Color.Purple;
                default:
                    return Color.Brown;
            }
        }

    }
}