using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics; //TCP
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net; //TCP
using System.Net.Sockets; //TCP
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//OpenCV
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace WPFCamServer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //서버 코드 작성
            string bindIP = "127.0.0.1";
            const int bindPort = 6101;
            TcpListener server = null;

            try
            {
                IPEndPoint localAdr = new IPEndPoint(IPAddress.Parse(bindIP), bindPort); //주소 정보 설정

                server = new TcpListener(localAdr); //TCPListener 객체 생성

                server.Start(); //서버 오픈

                //while (true)
                //{
                //일단 하나의 클라이언트라도 서비스 해보자
                TcpClient client = server.AcceptTcpClient(); //클라이언트 연결 요청 수락
                NetworkStream stream = client.GetStream(); //데이터 송수신에 이용할 스트림

                //데이터 크기 수신
                byte[] size = new byte[4];
                stream.Read(size, 0, 4);
                MessageBox.Show("수신데이터 크기: "+BitConverter.ToInt32(size, 0).ToString());
                int strLen = BitConverter.ToInt32(size, 0); //받아야할 데이터 크기

                int len = 0, recvLen =0; //임시 길이 저장, 전체 수신길이 저장
                byte[] bytes = new byte[10000]; //버퍼
                recvLen = stream.Read(bytes, 0, bytes.Length); //데이터 1차 수신
                List<byte> buf = new List<byte>();
                buf.AddRange(bytes);
                //string buffer = Encoding.Default.GetString(bytes, 0, bytes.Length);
                while (recvLen<strLen)
                {
                    len = stream.Read(bytes, 0, bytes.Length);
                    buf.AddRange(bytes);
                    //buffer = buffer + Encoding.Default.GetString(bytes, 0, bytes.Length);
                    recvLen += len;
                    len = 0;
                }
                MessageBox.Show("최종 데이터 크기: " + recvLen.ToString()); //데이터 크기대로 제대로 옴 이제 이걸 띄워야해
                FileStream file = new FileStream("..//shot.bmp", FileMode.Create);
                file.Write(buf.ToArray(), 0, buf.Count); //일단 캡쳐본 수신은 성공
                //file.Write(Encoding.Default.GetBytes(buffer), 0, Encoding.Default.GetBytes(buffer).Length);
               //왜 출력이 안되는 걸까
                Mat frame = new Mat(); //여기가 왜 안되는거지
                frame = Mat.FromImageData(buf.ToArray(), ImreadModes.AnyColor); //이방법으로
                //frame = Mat.FromArray<byte>(Encoding.Default.GetBytes(buffer));
                Cam.Source = WriteableBitmapConverter.ToWriteableBitmap(frame);
                //Cam.Source=new BitmapImage(new Uri("../shot.bmp",UriKind.Relative));
                Cv2.ImShow("1", Mat.FromImageData(buf.ToArray(),ImreadModes.AnyColor));
            }
            catch (SocketException err) //소켓 오류 날때 예외처리
            {
                MessageBox.Show(err.ToString());
            }
            finally
            {
                server.Stop(); //서버 종료
            }
        }
        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
