using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Client
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;

    class Program
    {
        private static bool operationRunning = false;
        private static bool cancelOperation = false;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += cancelKeyPressHandler;

            while (true)
            {
                Console.Write("CurlClone> ");
                string command = Console.ReadLine();

                if (!string.IsNullOrEmpty(command))
                {
                    try
                    {
                        operationRunning = true;

                        handleCommand(command);
                    }
                    catch (Exception ex)
                    {
                        WriteLine("Error executing command: {0}", ex.ToString());
                    }
                    finally
                    {
                        operationRunning = false;
                        cancelOperation = false;
                    }
                }
            }
        }

        private static void handleCommand(string command)
        {
            if (Regex.IsMatch(command, "^curl ", RegexOptions.IgnoreCase))
            {
                ulong tcpKeepAliveTimeout = 0; // in s
                ulong tcpKeepAliveInterval = 1; // in s
                ulong idleTimeoutInSec = 3600; // in s
                string operationType = "GET";
                string protocol = "HTTP/1.1";

                List<string> header = new List<string>();
                Uri url = null;

                string parameter = command.Substring(4);
                Match match = null;
                if (Match(parameter, "--keepalive ([0-9]+)", out match))
                {
                    string valueAsString = match.Groups[1].Value;
                    try
                    {
                        tcpKeepAliveTimeout = Convert.ToUInt64(valueAsString);
                    }
                    catch (Exception ex)
                    {
                        WriteLine("Invalid value for parameter --keepalive: '{0}'", valueAsString);
                        return;
                    }
                }

                if (Match(parameter, "(https?://.*)$", out match))
                {
                    string valueAsString = match.Groups[1].Value;
                    try
                    {
                        url = new Uri(valueAsString);
                    }
                    catch (Exception ex)
                    {
                        WriteLine("Invalid URL: '{0}'", valueAsString);
                        return;
                    }
                }
                else
                {
                    WriteLine("Unable to parse a url!");
                    return;
                }

                // add necessary headers
                header.Add(string.Format("Host: {0}:{1}", url.Host, url.Port));
                header.Add(string.Format("Content-Length: 0"));

                string request = CreateHttpRequest(operationType, protocol, url.PathAndQuery, header);

                using (TcpClient client = new TcpClient(url.Host, url.Port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        if (tcpKeepAliveTimeout > 0)
                        {
                            SetKeepAlive(client.Client, tcpKeepAliveTimeout * 1000, tcpKeepAliveInterval * 1000);
                        }

                        byte[] rawRequest = Encoding.UTF8.GetBytes(request);
                        stream.Write(rawRequest, 0, rawRequest.Length);
                        stream.Flush();

                        DateTime operationStart = DateTime.Now;

                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;
                        StringBuilder response = new StringBuilder();
                        bool GotRespone = false;

                        do
                        {
                            while (!cancelOperation && stream.DataAvailable && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                                GotRespone = true;
                            }
                        } while (!cancelOperation && !GotRespone && (DateTime.Now - operationStart).TotalSeconds < idleTimeoutInSec);

                        if (cancelOperation == false)
                        {
                            WriteLine(response.ToString());
                        }
                        else
                        {
                            WriteLine("Operation got cancelled by the user.");
                        }
                    }

                    client.Close();
                }
            }
        }

        private static string CreateHttpRequest(string opType, string protocol, string rawPath, List<string> headers, string body = "")
        {
            StringBuilder request = new StringBuilder();
            request.AppendFormat("{0} {1} {2}\r\n", opType.ToUpperInvariant(), rawPath, protocol);
            foreach (string header in headers)
            {
                request.AppendLine(header);
            }

            request.AppendLine();

            if (!string.IsNullOrEmpty(body))
            {
                request.AppendLine(body);
            }

            return request.ToString();
        }

        private static bool Match(string value, string regex, out Match match)
        {
            match = null;

            if (Regex.IsMatch(value, regex))
            {
                match = Regex.Match(value, regex, RegexOptions.IgnoreCase);
                return true;
            }

            return false;
        }

        private static void WriteLine(string value, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine(value);
            }
            else
            {
                Console.WriteLine(value, args);
            }
        }

        // "consts" to help understand calculations
        const int bytesperlong = 4; // 32 / 8
        const int bitsperbyte = 8;
        private static bool SetKeepAlive(Socket sock, ulong time, ulong interval)
        {
            try
            {
                // resulting structure
                byte[] SIO_KEEPALIVE_VALS = new byte[3 * bytesperlong];
                // array to hold input values
                ulong[] input = new ulong[3];
                // put input arguments in input array
                if (time == 0 || interval == 0) // enable disable keep-alive
                    input[0] = (0UL); // off
                else
                    input[0] = (1UL); // on
                input[1] = (time); // time millis
                input[2] = (interval); // interval millis
                // pack input into byte struct
                for (int i = 0; i < input.Length; i++)
                {
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 3] = (byte)(input[i] >> ((bytesperlong - 1) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 2] = (byte)(input[i] >> ((bytesperlong - 2) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 1] = (byte)(input[i] >> ((bytesperlong - 3) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 0] = (byte)(input[i] >> ((bytesperlong - 4) * bitsperbyte) & 0xff);
                }
                // create bytestruct for result (bytes pending on server socket)
                byte[] result = BitConverter.GetBytes(0);
                // write SIO_VALS to Socket IOControl
                sock.IOControl(IOControlCode.KeepAliveValues, SIO_KEEPALIVE_VALS, result);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected static void cancelKeyPressHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (operationRunning)
            {
                cancelOperation = true;
                args.Cancel = true;
            }
        }
    }
}