using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;



namespace Microsoft.Samples.Kinect.DepthBasics
{
    internal class HttpServer
    {
        //link to the informations from the kinect
        public static MainWindow fenetreKinectLocal = null;

        //server : chose the port
        public static HttpListener listener;
        public static string url = "http://localhost:2021/";
        public static int pageViews = 0;
        public static int requestCount = 0;


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

                // If `depthsensor` url requested w/ GET, then send depth sensor informations
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/depthsensor/data"))
                {
                    Console.WriteLine("Depth informations requested");

                    // Write the response info
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fenetreKinectLocal.curentFrame));
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }

                /* to be added
                 
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/depthsensor/size"))
                {
                    Console.WriteLine("Size informations requested");
                    
                    
                    --> fenetreKinectLocal.depthFrameDescription.Width
                        fenetreKinectLocal.depthFrameDescription.Height

                    // Write the response info
                    byte[] data = Encoding.UTF8.GetBytes(...);
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }
                */

                resp.Close();
            }
        }

        public async static Task setupServer(MainWindow fenetreKinect)
        {
            fenetreKinectLocal = fenetreKinect;

            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            await HandleIncomingConnections();
        }
    }
}
