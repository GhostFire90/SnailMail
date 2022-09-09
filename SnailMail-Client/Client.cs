
using SnailMailProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

using Terminal.Gui;
using System.Threading.Tasks;
using System.Threading;

namespace SnailMail_Client
{
    class Client
    {

        static async Task sending(ListViewItemEventArgs args, SnailMailClient client)
        {
            OpenDialog dialog = new OpenDialog("Open a file", "Select a file");
            
            Application.Run(dialog);
            
            
            Progress<float> progress = new Progress<float>((float a) =>
            {

            });
            await Task.Run(async () =>
            {
                if (!dialog.Canceled)
                {

                    
                    string[] paths = dialog.FilePaths[0].Split('\\');
                    string ip = args.Value.ToString();

                    
                    await client.Send(dialog.FilePaths[0], ip, progress, noKey);
                    
                    //while (progressBar.Fraction < 1) ;
                    //Application.MainLoop.Invoke(() => Application.Top.Remove(progressBar));
                                  
                }

                
            });
            
            MessageBox.Query("", "File Sent Successfully", "Ok");
            

        }

        public static async Task<bool> noKey()
        {
            bool t = await Task.Run(() =>
            {
                ManualResetEvent done = new ManualResetEvent(false);
                int i = 0;
                Application.MainLoop.Invoke(() =>
                {
                    
                    i = MessageBox.Query("No Encryption", "The recipient has not provided an encryption key, would you like to send unencrypted?", "Yes", "No");
                    done.Set();
                });
                done.WaitOne();
                return i == 0;
            });
            return t;
            /*int a = MessageBox.Query("No Encryption", "The recipient has not provided an encryption key, would you like to send unencrypted?", "Yes", "No"); 
            


            return a == 0;*/
        }


        static void recieving(ListViewItemEventArgs args, SnailMailClient client)
        {
            string dir = client.IpAsString();

            string code = client.Recieve(args.Value.ToString());
            if(code != "")
            {
                MessageBox.Query("Something went wrong!", code, "Ok"); ;
                return;
            }
            MessageBox.Query("Success", "Successfully saved the file and saved it to \"Inbox\"", "Ok");
        }
        static void addAddress(View.KeyEventEventArgs args, TextField field)
        {
            if(args.KeyEvent.Key == Key.Enter)
            {
                string input = field.Text.ToString();
                field.Text = "";
                bool created = false;


                SMAddress address;
                try
                {
                    address = new SMAddress(input);
                    if (!File.Exists("AddressBook.txt"))
                    {
                        created = true;
                        File.Create("AddressBook.txt").Close();
                    }

                    string[] addresses = getAddresses();

                    using (FileStream fs = File.Open("AddressBook.txt", FileMode.Append))
                    using (StreamWriter stream = new StreamWriter(fs))
                    {
                        if (!addresses.Contains(input))
                        {
                            stream.WriteLine(input);
                            MessageBox.Query("", $"{input} Added successfully", "Ok");
                        }
                        else
                        {
                            MessageBox.Query("", "Address already existed", "Ok");
                        }

                    }
                }
                catch(FormatException ex)
                {

                    if (created)
                    {
                        File.Delete("AddressBook.txt");
                    }
                    
                }

                

            }
        }
        static string[] getAddresses()
        {
            if (!File.Exists("AddressBook.txt"))
            {
                MessageBox.Query("", "No addresses, try to add some", "OK");
                return new string[0];
            }
            else
            {
                List<string> addresses = new List<string>();
                using (FileStream fs = File.OpenRead("AddressBook.txt"))
                using (StreamReader stream = new StreamReader(fs))
                {
                    
                    while (!stream.EndOfStream)
                    {
                        string address = stream.ReadLine();
                        addresses.Add(address);                        
                    }
                }
                return addresses.ToArray();
            }
        }

        static void Main(string[] args)
        {
            Dictionary<string, string> config;
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            if (File.Exists("config.json"))
            {
                StringBuilder builder = new StringBuilder();
                using(StreamReader reader = File.OpenText("config.json"))
                {
                    while(!reader.EndOfStream) builder.AppendLine(reader.ReadLine());
                    config = JsonConvert.DeserializeObject<Dictionary<string, string>>(builder.ToString(), settings);

                }

            }
            else
            {
                config = new Dictionary<string, string>();
                config.Add("ip", "127.0.0.1");
                config.Add("port", "90");

                string json = JsonConvert.SerializeObject(config, settings);
                using(StreamWriter writer = File.CreateText("config.json"))
                {
                    writer.Write(json);
                }

            }

            SnailMailClient client = new SnailMailClient(config["ip"], Int32.Parse(config["port"]));
            client.Connect();

            string logo = @"   _____                _  __ __  ___        _  __" + '\n' +
                          @"  / ___/ ____   ____ _ (_)/ //  |/  /____ _ (_)/ /" + '\n' +
                          @"  \__ \ / __ \ / __ `// // // /|_/ // __ `// // / " + '\n' +
                          @" ___/ // / / // /_/ // // // /  / // /_/ // // /  " + '\n' +
                          @"/____//_/ /_/ \__,_//_//_//_/  /_/ \__,_//_//_/   " + '\n';

            
            
            Application.Init();

            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.White, Color.Black);



            Window Title = new Window()
            {
                Height = (Dim)(System.Console.WindowHeight * .25f),
                Width = 52,
                X = Pos.Center(),
                CanFocus = false
                
            };


            TabView Content = new TabView()
            {

                Height = (Dim)(System.Console.WindowHeight * .75f),
                Width = Dim.Width(Application.Top),
                Y = (Pos)(System.Console.WindowHeight * .25f),
                ColorScheme = Colors.Base
            };

            ListView fileList = new ListView(client.RequestInbox())
            {
                AutoSize = true,
                Width = Content.Width - 10,
                Height = Content.Height - 10
            };
            fileList.OpenSelectedItem += (args) => recieving(args, client);
            TabView.Tab Incoming = new TabView.Tab("Incoming", fileList);
            Content.AddTab(Incoming, false);

            ListView ipList = new ListView()
            {
                AutoSize = true,
                Width = Content.Width-10,
                Height = Content.Height-10
            };

            ipList.OpenSelectedItem += async (args) => await sending(args, client);         

            TabView.Tab Send = new TabView.Tab("Send", ipList);
            Content.AddTab(Send, false);



            TextField textField = new TextField()
            {
                AutoSize = true,
                Width = Content.Width - 10,
                Height = Content.Height - 10,
                ReadOnly = false
            };
            textField.KeyDown += (args) => addAddress(args, textField);
            TabView.Tab AddAddressBook = new TabView.Tab("Add an Address", textField);

            Content.AddTab(AddAddressBook, false);

            TabView.Tab Quit = new TabView.Tab("Quit", new Button("Quits the program") 
            { 
                X = Pos.Center()
                
            });

            ((Button)Quit.View).Clicked += () => { Application.Shutdown(); client.Disconnect(); System.Environment.Exit(0); };

            

            Content.AddTab(Quit, false);

            Content.SelectedTabChanged += (object sender, TabView.TabChangedEventArgs args) =>
            {
                if(args.NewTab == Send)
                {
                    ipList.SetSource(getAddresses());
                }
                if(args.NewTab == Incoming)
                {
                    fileList.SetSource(client.RequestInbox());
                }
            };

            Title.Add(new Label(logo)
            {
                X = Pos.Center()
            });

            Application.Top.Add(Title);
            Application.Top.Add(Content);
            Application.Run();
        }


    }
}
