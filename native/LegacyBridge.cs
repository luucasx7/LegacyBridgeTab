using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web.Script.Serialization;

internal static class LegacyBridge
{
    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0].StartsWith("--viewer64=", StringComparison.Ordinal))
        {
            string encoded = args[0].Substring("--viewer64=".Length);
            string url = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            Uri uri;
            if (!IsWebUrl(url, out uri)) return 2;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BrowserWindow(uri));
            return 0;
        }

        try
        {
            string origin = args.Length > 0 ? args[0] : "";
            if (!origin.StartsWith("chrome-extension://", StringComparison.Ordinal))
                return WriteResponse(false, "Origem do Chrome inválida.");

            Stream input = Console.OpenStandardInput();
            byte[] header = ReadExact(input, 4);
            if (header == null) return 1;
            int length = BitConverter.ToInt32(header, 0);
            if (length < 1 || length > 1024 * 1024)
                return WriteResponse(false, "Mensagem fora do limite permitido.");

            byte[] body = ReadExact(input, length);
            if (body == null) return WriteResponse(false, "Mensagem incompleta.");
            string json = Encoding.UTF8.GetString(body);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> message =
                serializer.Deserialize<Dictionary<string, object>>(json);

            if (message == null || !message.ContainsKey("action") ||
                !String.Equals(Convert.ToString(message["action"]), "open", StringComparison.Ordinal))
                return WriteResponse(false, "Ação não reconhecida.");

            string url = message.ContainsKey("url") ? Convert.ToString(message["url"]) : "";
            Uri uri;
            if (!IsWebUrl(url, out uri))
                return WriteResponse(false, "Somente endereços http e https são aceitos.");

            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(uri.AbsoluteUri));
            string hostExe = Process.GetCurrentProcess().MainModule.FileName;
            string viewerExe = Path.Combine(Path.GetDirectoryName(hostExe), "LegacyBridgeViewer.exe");
            if (!File.Exists(viewerExe))
                return WriteResponse(false, "Não encontrei o visualizador auxiliar.");
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = viewerExe;
            start.Arguments = "--viewer64=" + encoded;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(start);
            FreeConsole();
            return WriteResponse(true, "Janela auxiliar iniciada.");
        }
        catch (Exception ex)
        {
            return WriteResponse(false, "Falha no auxiliar: " + ex.Message);
        }
    }

    private static bool IsWebUrl(string value, out Uri uri)
    {
        uri = null;
        if (String.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(value, UriKind.Absolute, out uri)) return false;
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    private static byte[] ReadExact(Stream stream, int count)
    {
        byte[] result = new byte[count];
        int offset = 0;
        while (offset < count)
        {
            int read = stream.Read(result, offset, count - offset);
            if (read <= 0) return null;
            offset += read;
        }
        return result;
    }

    private static int WriteResponse(bool ok, string message)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        byte[] body = Encoding.UTF8.GetBytes(serializer.Serialize(new { ok = ok, error = ok ? null : message, message = message }));
        Stream output = Console.OpenStandardOutput();
        output.Write(BitConverter.GetBytes(body.Length), 0, 4);
        output.Write(body, 0, body.Length);
        output.Flush();
        return 0;
    }
}

internal sealed class BrowserWindow : Form
{
    private readonly WebBrowser browser;
    private readonly TextBox address;

    public BrowserWindow(Uri initial)
    {
        Text = "Legacy Bridge — janela de compatibilidade";
        Width = 1120;
        Height = 820;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 500);

        Panel bar = new Panel();
        bar.Dock = DockStyle.Top;
        bar.Height = 42;
        bar.Padding = new Padding(7, 6, 7, 6);
        bar.BackColor = Color.FromArgb(31, 37, 30);

        FlowLayoutPanel buttons = new FlowLayoutPanel();
        buttons.Dock = DockStyle.Left;
        buttons.Width = 122;
        buttons.WrapContents = false;

        Button back = MakeButton("←");
        Button forward = MakeButton("→");
        Button refresh = MakeButton("↻");
        back.Click += delegate { if (browser.CanGoBack) browser.GoBack(); };
        forward.Click += delegate { if (browser.CanGoForward) browser.GoForward(); };
        refresh.Click += delegate { browser.Refresh(); };
        buttons.Controls.Add(back);
        buttons.Controls.Add(forward);
        buttons.Controls.Add(refresh);

        Button go = MakeButton("Ir");
        go.Dock = DockStyle.Right;
        go.Width = 42;
        go.Click += delegate { NavigateAddress(); };

        address = new TextBox();
        address.Dock = DockStyle.Fill;
        address.Font = new Font("Segoe UI", 10);
        address.BorderStyle = BorderStyle.FixedSingle;
        address.Text = initial.AbsoluteUri;
        address.KeyDown += AddressKeyDown;

        bar.Controls.Add(address);
        bar.Controls.Add(go);
        bar.Controls.Add(buttons);

        browser = new WebBrowser();
        browser.Dock = DockStyle.Fill;
        browser.ScriptErrorsSuppressed = true;
        browser.NewWindow += delegate(object sender, CancelEventArgs e) { e.Cancel = true; };
        browser.Navigating += BrowserNavigating;
        browser.Navigated += delegate { if (browser.Url != null) address.Text = browser.Url.AbsoluteUri; };
        browser.DocumentCompleted += delegate
        {
            string pageTitle = browser.DocumentTitle;
            Text = String.IsNullOrEmpty(pageTitle)
                ? "Legacy Bridge — janela de compatibilidade"
                : pageTitle + " — Legacy Bridge";
        };

        Controls.Add(browser);
        Controls.Add(bar);
        browser.Navigate(initial);
    }

    private static Button MakeButton(string text)
    {
        Button button = new Button();
        button.Text = text;
        button.Width = 36;
        button.Height = 28;
        button.Margin = new Padding(0, 0, 5, 0);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(75, 86, 68);
        button.BackColor = Color.FromArgb(44, 52, 40);
        button.ForeColor = Color.White;
        return button;
    }

    private void AddressKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            NavigateAddress();
        }
    }

    private void NavigateAddress()
    {
        string value = address.Text.Trim();
        if (!value.Contains("://")) value = "https://" + value;
        Uri uri;
        if (!Uri.TryCreate(value, UriKind.Absolute, out uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            MessageBox.Show(this, "Use um endereço http ou https.", "Endereço não permitido",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        browser.Navigate(uri);
    }

    private void BrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
    {
        if (e.Url != null && e.Url.Scheme != Uri.UriSchemeHttp &&
            e.Url.Scheme != Uri.UriSchemeHttps && e.Url.Scheme != "about")
            e.Cancel = true;
    }
}
