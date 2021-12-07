using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.IO.Ports;
using System.Threading;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    internal class HttpServer
    {
        //link to the informations from the kinect
        public static MainWindow fenetreKinectLocal = null;

        //server : chose the port
        public static HttpListener listener;
        //public static string url = "http://192.168.1.39:2021/";   //box
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
                //Console.WriteLine("Waiting for a request...");

                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                //Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                //Console.WriteLine(req.HttpMethod);
                //Console.WriteLine(req.UserHostName);
                //Console.WriteLine(req.UserAgent);
                ///Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                /*if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }*/

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
                        stringRequest = stringRequest.Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");
                        //Console.WriteLine(stringRequest);
                        //Console.WriteLine("End of client data:");
                        body.Close();
                        reader.Close();
                        //RobotJson robotJson = JsonSerializer.Deserialize<RobotJson>(stringRequest);
                        Console.WriteLine($"data recu : >{stringRequest}<");
                        _serialPort.WriteLine($"{stringRequest}\r");


                        while (_serialPort.BytesToRead > 0)
                        {
                            //Console.Write(_serialPort.ReadChar());
                            try
                            {
                                Console.WriteLine($"reponse du robot recu : >{_serialPort.ReadLine()}<");
                            }
                            catch (TimeoutException) { }
                        }
                        string messageRobot = "{\"statut\":ok}";
                        /*try
                        {
                            messageRobot = _serialPort.ReadLine();
                            Console.WriteLine($"reponse du robot recu : >{messageRobot}<");
                        }
                        catch (TimeoutException) { }*/

                        byte[] data = Encoding.UTF8.GetBytes(messageRobot);
                        resp.ContentType = "application/json";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    }
                }


                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/param"))
                {
                    if (req.HasEntityBody)
                    {
                        System.IO.Stream body = req.InputStream;
                        System.Text.Encoding encoding = req.ContentEncoding;
                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

                        // Convert the data to a string and display it on the console.
                        string stringRequest = reader.ReadToEnd();
                        body.Close();
                        reader.Close();

                        stringRequest = stringRequest.Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");
                        ParamJson paramJson = JsonSerializer.Deserialize<ParamJson>(stringRequest);
                        Console.WriteLine($"data recu : {stringRequest}");

                        fenetreKinectLocal.Hmin = Math.Max(paramJson.Hmin,0);
                        fenetreKinectLocal.Hmax = Math.Min(paramJson.Hmax,423);
                        fenetreKinectLocal.Lmin = Math.Max(paramJson.Lmin,0);
                        fenetreKinectLocal.Lmax = Math.Min(paramJson.Lmax,511);

                        Console.WriteLine($"lecture de lmin : >{fenetreKinectLocal.Lmin}<");

                        string messageRobot = "{\"Latence\":5}"; //modifier ici !



                        byte[] data = Encoding.UTF8.GetBytes(messageRobot);
                        resp.ContentType = "application/json";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    }
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/log"))
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

                        string stringRequest = reader.ReadToEnd();
                        stringRequest = stringRequest.Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");

                        body.Close();
                        reader.Close();

                        Console.WriteLine($"     log from headset : >{stringRequest}<");

                        string messageLog = "{\"statut\":ok}";

                        byte[] data = Encoding.UTF8.GetBytes(messageLog);
                        resp.ContentType = "application/json";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    }
                }


                // If `depthsensor` url requested w/ GET, then send depth sensor informations
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/depthsensor"))
                {
                    Console.WriteLine("Depth informations requested");

                    ushort[] frameMoy;
                    int nbval = (fenetreKinectLocal.Lmax - fenetreKinectLocal.Lmin) * (fenetreKinectLocal.Hmax - fenetreKinectLocal.Hmin);
                    frameMoy = new ushort[nbval];
                    
                    for(int i = 0; i< nbval; i++)
                    {
                        frameMoy[1] = (ushort)((fenetreKinectLocal.frameCuted0[i] + fenetreKinectLocal.frameCuted1[i] + fenetreKinectLocal.frameCuted2[i])/3);
                    }
                    // Write the response info
                    var dataJson = new dataJson
                    {
                        Data = fenetreKinectLocal.frameCuted0
                    };
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataJson));

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
            _serialPort.PortName = "COM13";
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None; //Parity : None = 0,  Even = 2,  Mark = 3,  Space = 4
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One; //StopBits : None = 0,  One = 1,  Two = 2,  OnePointFive = 3
            _serialPort.Handshake = Handshake.None; // Handshake : None = 0,  XOnXOff = 1,  RequestToSend = 2,  RequestToSendXOnXOff = 3
            _serialPort.DtrEnable = true;

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
            public ushort[] Data { get; set; }
        }
        public class ParamJson
        {
            public int Hmin { get; set; }
            public int Hmax { get; set; }
            public int Lmin { get; set; }
            public int Lmax { get; set; }
        }
    }
}
