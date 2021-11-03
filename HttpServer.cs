using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.IO.Ports;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    internal class HttpServer
    {
        //link to the informations from the kinect
        public static MainWindow fenetreKinectLocal = null;

        //server : chose the port
        public static HttpListener listener;
        public static string url = "http://192.168.1.39:2021/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static SerialPort _serialPort;

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                Console.WriteLine("Waiting for a request...");

                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                // If `robot` url requested w/ POST, then send informations to the robot via serial port
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/robot"))
                {
                    if (req.HasEntityBody)
                    {
                        System.IO.Stream body = req.InputStream;
                        System.Text.Encoding encoding = req.ContentEncoding;
                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                        if (req.ContentType != null)
                        {
                            //Console.WriteLine("Client data content type {0}", req.ContentType);
                        }
                        //Console.WriteLine("Client data content length {0}", req.ContentLength64);
                        //Console.WriteLine("Start of client data:");
                        // Convert the data to a string and display it on the console.
                        string stringRequest = reader.ReadToEnd();
                        //Console.WriteLine(stringRequest);
                        //Console.WriteLine("End of client data:");
                        body.Close();
                        reader.Close();
                        RobotJson robotJson = JsonSerializer.Deserialize<RobotJson>(stringRequest);
                        Console.WriteLine($"data recu : {robotJson.Message}");

                        _serialPort.WriteLine(robotJson.Message);


                        // Write the response info
                        var retourRobotJson = new RetourRobotJson
                        {
                            estOk = true
                        };
                        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(retourRobotJson));
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    }
                }

                // If `depthsensor` url requested w/ GET, then send depth sensor informations
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/depthsensor"))
                {
                    Console.WriteLine("Depth informations requested");

                    // Write the response info
                    var dataJson = new dataJson
                    {
                        Largeur = fenetreKinectLocal.depthBitmap.PixelWidth,
                        Hauteur = fenetreKinectLocal.depthBitmap.PixelHeight,
                        Data = fenetreKinectLocal.curentFrame
                    };
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataJson));

                    //byte[] addDebut = Encoding.UTF8.GetBytes("{\"data\":");
                    //byte[] addFin = Encoding.UTF8.GetBytes("}");
                    //byte[] json = new byte[data.Length + addDebut.Length + addFin.Length +1];

                    //System.Array.Copy(addDebut, json, json.Length);
                    ///System.Array.Copy(data, json, data.Length);
                    //System.Array.Copy(addFin, json, addFin.Length);

                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }

                resp.Close();
            }
        }

        public async static Task setupServer(MainWindow fenetreKinect)
        {
            fenetreKinectLocal = fenetreKinect;

            _serialPort = new SerialPort();
            _serialPort.PortName = "COM8";
            _serialPort.BaudRate = 9600;
            //_serialPort.Parity = Parity.None; //Parity : None = 0,  Even = 2,  Mark = 3,  Space = 4
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One; //StopBits : None = 0,  One = 1,  Two = 2,  OnePointFive = 3
            //_serialPort.Handshake = Handshake.None; // Handshake : None = 0,  XOnXOff = 1,  RequestToSend = 2,  RequestToSendXOnXOff = 3

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();


            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            await HandleIncomingConnections();
        }

        public class dataJson
        {
            public int Largeur { get; set; }
            public int Hauteur { get; set; }
            public ushort[] Data { get; set; }
        }
        public class RobotJson
        {
            public string Message { get; set; }
        }
        public class RetourRobotJson
        {
            public bool estOk { get; set; }
        }
    }
}
