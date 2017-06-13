using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;

namespace sftp_upload_app
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            var host = Configuration["sftpConfiguration:host"];
            var username = Configuration["sftpConfiguration:username"];
            var password = Configuration["sftpConfiguration:password"];
            var port = Convert.ToInt32(Configuration["sftpConfiguration:port"]);

#if DEBUG
            args = new[] {@"C:\windows-version.txt", @"dest"};
#endif

            if (args == null || args.Length != 2)
            {
                Console.WriteLine("Args error");
                Console.ReadLine();
            }
            else
            {
                UploadSftpFile(host, username, password, args[0], args[1], port);
            }
        }

        public static void UploadSftpFile(string host, string username, string password, string sourcefile,
            string destinationPath, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                try
                {
                    client.Connect();
                    Console.WriteLine("Connected into the SFTP");
                    client.ChangeDirectory(destinationPath);
                    using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                    {
                        try
                        {
                            Console.WriteLine("Preparing for upload the file..");
                            client.UploadFile(fs, Path.GetFileName(sourcefile));
                            Console.WriteLine($"File upload into {destinationPath}");
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Error uploading the file into the SFTP", e);
                        }
                       
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Error creating de SFTP client", e);
                }
            }
        }

    }
}
