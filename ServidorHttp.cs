using System.Net.Sockets;
using System.Text;

class ServidorHttp
{
    private TcpListener Controlador { get; set;}
    private int Porta {get; set;}
    private int QtdeRequests {get; set;}
    public string HtmlExample {get; set;}

    private SortedList<string, string> MimeTypes {get; set;}
    private SortedList<string, string> DirectoryHosts {get; set;}

    public ServidorHttp(int porta = 8080)
    {
        this.Porta = porta;
        // this.CreateHtmlExample();
        this.PopulateMimeTypes();
        this.PopulateDirectoryHosts();
        try
        {
            this.Controlador = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), this.Porta);
            this.Controlador.Start();
            Console.WriteLine($"Servidor HTTP esta rodando na porta {this.Porta}.");
            Console.WriteLine($"Para acessar, digite no navegador: http//localhost:{this.Porta}.");
            Task servidorHttpTask = Task.Run(() => waitForRequests());
            servidorHttpTask.GetAwaiter().GetResult();
        }
        catch(Exception e)
        {
            Console.WriteLine($"Erro ao iniciar o servidor na porta {this.Porta}: \n{e.Message}");
        }
    }

    private async Task waitForRequests()
    {
        while (true)
        {
            Socket conexao = await this.Controlador.AcceptSocketAsync();
            this.QtdeRequests++;
            Task task = Task.Run(() => ProcessRequest(conexao, this.QtdeRequests));
        }
    }

    private void ProcessRequest(Socket conexao, int numeroRequest)
    {
        Console.WriteLine($"Processando request #{numeroRequest}...\n");
        if (conexao.Connected)
        {
            byte[] bytesRequest = new byte[1024];
            conexao.Receive(bytesRequest, bytesRequest.Length, 0);
            string textRequest = Encoding.UTF8.GetString(bytesRequest).Replace((char)0, ' ').Trim();
            if (textRequest.Length > 0)
            {
                Console.WriteLine($"\n{textRequest}\n");
                string[] linhas = textRequest.Split("\r\n");
                int iPrimeiroEspaco = linhas[0].IndexOf(' ');
                int iSegundoEspaco = linhas[0].LastIndexOf(' ');
                string metodoHttp = linhas[0].Substring(0, iPrimeiroEspaco);
                string recursoBuscado = linhas[0].Substring(iPrimeiroEspaco + 1, iSegundoEspaco - iPrimeiroEspaco - 1);
                if (recursoBuscado == "/") recursoBuscado = "/index.html";
                string versaoHttp = linhas[0].Substring(iSegundoEspaco + 1);
                iPrimeiroEspaco = linhas[1].IndexOf(' ');
                string nomeHost = linhas[1].Substring(iPrimeiroEspaco + 1);
                byte[] bytesHeader = null;
                byte[] bytesContent = null;
                FileInfo fiFile = new FileInfo(getFilePath(nomeHost, recursoBuscado));
                if (fiFile.Exists)
                {
                    if (MimeTypes.ContainsKey(fiFile.Extension.ToLower()))
                    {
                        bytesContent = File.ReadAllBytes(fiFile.FullName);
                        string mimeType = MimeTypes[fiFile.Extension.ToLower()];
                        bytesHeader = GenerateHeader(versaoHttp, mimeType, "200", bytesContent.Length); // headers that will be sent to client in response to request
                    }
                    else
                    {
                        bytesContent = Encoding.UTF8.GetBytes("<h1>Erro 415 - Tipo de arquivo não suportado.</h1>");
                        bytesHeader = GenerateHeader(versaoHttp, "text/html;charset=utf-8",
                            "415", bytesContent.Length);
                    }
                }
                else
                {
                    bytesContent = Encoding.UTF8.GetBytes("<h1>Error 404 - File not found</h1>");
                    bytesHeader = GenerateHeader(versaoHttp, "text/html;charset-utf-8","404", bytesContent.Length);
                }
                int bytesEnviados = conexao.Send(bytesHeader, bytesHeader.Length, 0); // send the header content to client
                bytesEnviados += conexao.Send(bytesContent, bytesContent.Length, 0); // html content that will be sent to client in response to request
                conexao.Close();
                Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta a requisicao #{numeroRequest}.");
            }
        }
        Console.WriteLine($"\n Request {numeroRequest} finalizado");
    }

    public byte[] GenerateHeader(string versaoHttp, string tipoMime, string codigoHttp, int qtdBytes = 0)
    {
        StringBuilder texto = new StringBuilder();
        texto.Append($"{versaoHttp} {codigoHttp} {Environment.NewLine}");
        texto.Append($"Server: Servidor Http 1.0 {Environment.NewLine}");
        texto.Append($"Content-Type: {tipoMime} {Environment.NewLine}");
        texto.Append($"Content-Length: {qtdBytes} {Environment.NewLine}{Environment.NewLine}");
        return Encoding.UTF8.GetBytes(texto.ToString());
    }

    // private void CreateHtmlExample()
    // {
    //     StringBuilder html = new StringBuilder();
    //     html.Append("<!DOCTYPE html><html lang=\"pt-br\"><head><meta charset=\"UTF-8\">");
    //     html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
    //     html.Append("<title>Página Estática</title></head><body>");
    //     html.Append("<h1>Página Estática</h1></body></html>");
    //     this.HtmlExample = html.ToString();
    // }

    private void PopulateMimeTypes()
    {
        this.MimeTypes = new SortedList<string, string>();
        this.MimeTypes.Add("", "");
        this.MimeTypes.Add(".html", "text/html;charset=utf-8");
        this.MimeTypes.Add(".htm", "text/html;charset=utf-8");
        this.MimeTypes.Add(".css", "text/css");
        this.MimeTypes.Add(".js", "text/javascript");
        this.MimeTypes.Add(".png", "image/png");
        this.MimeTypes.Add(".jpg", "image/jpeg");
        this.MimeTypes.Add(".gif", "image/gif");
        this.MimeTypes.Add(".svg", "image/svg+xml");
        this.MimeTypes.Add(".webp", "image/webp");
        this.MimeTypes.Add(".ico", "image/ico");
        this.MimeTypes.Add(".woff", "font/woff");
        this.MimeTypes.Add(".woff2", "font/woff2");
        this.MimeTypes.Add(".dhtml", "text/html;charset=utf-8");
    }

    private void PopulateDirectoryHosts()
    {
        this.DirectoryHosts = new SortedList<string, string>();
        this.DirectoryHosts.Add("localhost", "C:\\Users\\Alan_\\Desktop\\all\\MyServer\\www\\localhost");
        this.DirectoryHosts.Add("outrosite", "C:\\Users\\Alan_\\Desktop\\all\\MyServer\\www\\outrosite");
    }
    public string getFilePath(string host, string fileName)
    {
        string directory = this.DirectoryHosts[host.Split(":")[0]];
        string filePath = directory + fileName.Replace("/","\\");
        return filePath;
    }
}

