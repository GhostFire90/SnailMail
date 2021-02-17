using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;
namespace SnailMailClient
{
    class SnailMailGui : SnailMailFunctionality
    {
        public void Start()
        {
            string _contentLabelText = "";
            Application.Init();
            List<string> _contentListViewList = new List<string>();
            TextView contentLabel = new TextView()
            {
                Width = Dim.Fill(),
                Height = 1,
                CanFocus = false
            };
            contentLabel.Text = _contentLabelText;
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.White, Color.Black);
            FrameView _selectFrame = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(1),
                Height = 9,
                CanFocus = false
            };
            FrameView _contentFrame = new FrameView()
            {
                X = 0,
                Y = 9,
                Height = Dim.Fill(0),
                Width = Dim.Fill(1),
                CanFocus = false
            };
            ListView _contentListView = new ListView(_contentListViewList)
            {
                Y = 2,
                X = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = false
            };
            TextField _ipInputField = new TextField()
            {
                Y = 1,
                X = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = false
            };

            Button[] selectors = new Button[3];
            Button _send = new Button("Send")
            {
                Y = Pos.Bottom(_selectFrame) - 3,
                CanFocus = true
            };
            Button _incoming = new Button("Incoming")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(_selectFrame) - 3,
                CanFocus = true
            };
            Button _quit = new Button("Quit")
            {
                X = Pos.Right(_selectFrame) - "Quit".Length - 6,
                Y = Pos.Bottom(_selectFrame) - 3,
                CanFocus = true
            };
            _send.Enter += (a) =>
            {
                contentLabel.Text = "Select to send a document";
                //contentLabel.Redraw(new Rect(0, 0, 1, _contentLabelText.Length));
            };
            _incoming.Enter += (a) =>
            {
                contentLabel.Text = "View Incoming or Recieved messages";
            };
            _quit.Enter += (a) =>
            {
                contentLabel.Text = "Quit the program";
            };

            selectors[0] = _send;
            selectors[1] = _incoming;
            selectors[2] = _quit;

            _selectFrame.Add(selectors);
            _selectFrame.Add(new Label(Logo()) { X = Pos.Center() });
            _contentFrame.Add(_contentListView);
            _contentFrame.Add(contentLabel);
            _contentFrame.Add(_ipInputField);
            //_contentFrame.Add();

            Application.Top.Add(_selectFrame);
            Application.Top.Add(_contentFrame);

            _send.Clicked += () => SendButton(contentLabel, _contentListView, _ipInputField);
            _quit.Clicked += () => { Application.Shutdown(); Environment.Exit(0); };
            _incoming.Clicked += () => RecieveFileList(contentLabel, _contentListView);
            _contentFrame.Leave += (a) => ClearListView(_contentListView, _contentFrame, _send, _ipInputField);
            _contentListView.OpenSelectedItem += (a) => ClearListView(_contentListView, _contentFrame, _send, _ipInputField);

            Application.Run();
            string Logo()
            {
                StringBuilder titleString = new StringBuilder();
                titleString.AppendLine("   _|_|_|                         _|   _|   _|      _|              _|   _|  ");
                titleString.AppendLine(" _|         _|_|_|       _|_|_|        _|   _|_|  _|_|     _|_|_|        _|  ");
                titleString.AppendLine("   _|_|     _|    _|   _|    _|   _|   _|   _|  _|  _|   _|    _|   _|   _|  ");
                titleString.AppendLine("       _|   _|    _|   _|    _|   _|   _|   _|      _|   _|    _|   _|   _|  ");
                titleString.AppendLine(" _|_|_|     _|    _|     _|_|_|   _|   _|   _|      _|     _|_|_|   _|   _|  ");
                return titleString.ToString();
            }

            
        }
    }
}
