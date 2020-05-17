using BlitzSniffer.Clone;
using BlitzSniffer.Receiver;
using BlitzSniffer.Util;
using Syroot.BinaryData;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BlitzSniffer.DemoApp
{
    /// <summary>
    /// Interaction logic for DemoApp.xaml
    /// </summary>
    public partial class DemoApp : Window
    {
        private List<Ellipse> PlayerEllipses = new List<Ellipse>();
        private List<TextBlock> PlayerTextBlocks = new List<TextBlock>();

        private List<Color> PlayerColorBrushes = new List<Color>()
        {
            Colors.Red,
            Colors.Purple,
            Colors.Pink,
            Colors.Orange,
            Colors.Yellow,
            Colors.Green,
            Colors.Blue,
            Colors.SkyBlue,
            Colors.Brown,
            Colors.Lavender,
            Colors.DarkGreen
        };

        public DemoApp()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 20;
                ellipse.Height = 20;
                ellipse.Fill = new SolidColorBrush(PlayerColorBrushes[i]);
                ellipse.Visibility = Visibility.Hidden;
                ellipse.RenderTransform = new TranslateTransform(-10, 10);

                PlayerCanvas.Children.Add(ellipse);
                PlayerEllipses.Add(ellipse);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = $"Player {i}";
                textBlock.FontSize = 16;
                textBlock.Foreground = new SolidColorBrush(PlayerColorBrushes[i]);
                textBlock.Visibility = Visibility.Hidden;

                PlayerCanvas.Children.Add(textBlock);
                PlayerTextBlocks.Add(textBlock);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(RunListener).Start();
        }

        void RunListener()
        {
            CloneHolder holder = CloneHolder.Instance;
            for (uint i = 0; i < 10; i++)
            {
                holder.RegisterClone(i + 3);
                holder.RegisterClone(i + 111);
            }

            holder.CloneChanged += HandlePlayerName;
            holder.CloneChanged += HandleNetState;

            using (PacketReceiver receiver = new RealTimeReplayPacketReceiver(@"C:\Users\Julian\Documents\Switch\Splatoon2\LAN\New\smh3-all.pcap", 100))
            {
                receiver.Start();
            }
        }

        void HandlePlayerName(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId > 13 || args.CloneId < 3)
            {
                return;
            }

            uint playerId = args.CloneId - 3;
            if (playerId > 10)
            {
                return;
            }

            if (args.ElementId != 1)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                this.Dispatcher.Invoke(() =>
                {
                    PlayerTextBlocks[(int)playerId].Visibility = Visibility.Visible;
                    string name = reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.Unicode);
                    PlayerTextBlocks[(int)playerId].Text = name;
                });
            }
        }

        void HandleNetState(object sender, CloneChangedEventArgs args)
        {
            uint playerId = args.CloneId - 111;
            if (playerId > 10)
            {
                return;
            }

            if (args.ElementId != 0)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                BitReader bitReader = new BitReader(reader);

                float[] vec3d = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    ushort ushortCoord = bitReader.ReadUInt16();
                    float floatCoord = (float)(ushortCoord) / 65535.0f * 2048.0f;

                    if (bitReader.ReadBit())
                    {
                        floatCoord *= -1.0f;
                    }

                    vec3d[i] = floatCoord;
                }

                this.Dispatcher.Invoke(() =>
                {
                    Ellipse ellipse = PlayerEllipses[(int)playerId];
                    ellipse.Visibility = Visibility.Visible;
                    Canvas.SetTop(ellipse, -vec3d[0] * 0.4);
                    Canvas.SetLeft(ellipse, vec3d[2] * 0.613);

                    TextBlock textBlock = PlayerTextBlocks[(int)playerId];
                    textBlock.Visibility = Visibility.Visible;
                    Canvas.SetTop(textBlock, (-vec3d[0] * 0.4) - 20);
                    Canvas.SetLeft(textBlock, vec3d[2] * 0.613);
                });
            }
        }

    }
}
