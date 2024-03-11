using Mat = OpenCvSharp.Mat;
using System;
using System.Collections.Generic;
using System.Linq;
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

// OpenCV 사용을 위한 using
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

// Timer 사용을 위한 using
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Windows.Interop;
using Point = OpenCvSharp.Point;
using System.Windows.Media.Media3D;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;
using System.Drawing.Printing;
using System.Web.UI.WebControls;
using System.Windows.Media.Animation;
using System.Data.SqlClient;
using System.Drawing;
using Microsoft.Win32;
using System.Globalization;
using OpenCvSharp.XFeatures2D;
using ZXing.Aztec.Internal;
using Microsoft.SqlServer.Server;
using System.Collections;


namespace WPF
{
    public partial class MainWindow : System.Windows.Window
    {
        private const bool V = true;
        Point[][] contours;
        HierarchyIndex[] hierarchyIndexes;

        VideoCapture cam; //영상파일을 불러올 때 사용하는 클래스
        Mat frame, result, color, test, dst;
        string Color = "";
        DispatcherTimer timer;
        bool is_initCam, is_initTimer, A = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            // 카메라, 타이머(0.01ms 간격) 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.01);

            // 초기화 완료면 타이머 실행
            if (is_initTimer && is_initCam) timer.Start();
        }



        private bool init_Timer(double interval_ms)
        {
            try
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(interval_ms);
                timer.Tick += new EventHandler(timer_tick);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool init_camera()
        {
            try
            {
                // 0번 카메라로 VideoCapture 생성 (카메라가 없으면 안됨)
                cam = new VideoCapture(0);
                cam.FrameHeight = (int)Cam_1.Height;
                cam.FrameWidth = (int)Cam_1.Width;

                // 카메라 영상을 담을 Mat 변수 생성
                frame = new Mat();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void timer_tick(object sender, EventArgs e)
        {
            // 0번 장비로 생성된 VideoCapture 객체에서 frame을 읽어옴
            frame = new Mat(); //
            cam.Read(frame); //입력할거
            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            Cam_1.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame); //첫화면 출력

            Cam_2.Source = WriteableBitmapConverter.ToWriteableBitmap(frame);

        }

        public string GetShape(Point[] c) // 도형검출
        {
            string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            Point[] approx = Cv2.ApproxPolyDP(c, 0.04 * peri, true);


            if (approx.Length == 3) //if the shape is a triangle, it will have 3 vertices
            {
                shape = "triangle";
            }
            else if (approx.Length == 4)    //if the shape has 4 vertices, it is either a square or a rectangle
            {
                Rect rect;
                rect = Cv2.BoundingRect(approx);
                double ar = rect.Width / (double)rect.Height;

                if (ar >= 0.95 && ar <= 1.05) shape = "square";
                else shape = "square";
            }
            else if (approx.Length == 5)    //if the shape has 5 vertice, it is a pantagon
            {
                shape = "pentagon";
            }
            else   //otherwise, shape is a circle
            {
                shape = "circle";
            }


            /////////////////// 색상 별 그리기 : ok, ng 구분출력?
            Mat b1 = new Mat();
            Mat c1 = new Mat();
            Cv2.CvtColor(frame, b1, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(b1, b1, 127, 255, ThresholdTypes.Binary);


            Cv2.InRange(frame, new Scalar(0, 127, 127), new Scalar(100, 255, 255), c1);

            Cv2.FindContours(b1, out Mat[] contour1, c1, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            for (int i = 0; i < contour1.Length; i++)
            {
                if (approx.Length == 4)
                {
                    Cv2.DrawContours(frame, contour1, i, Scalar.Blue, 1, LineTypes.AntiAlias);
                }
                else
                {
                    Cv2.DrawContours(frame, contour1, i, Scalar.Red, 1, LineTypes.AntiAlias);
                }
            }

            Cv2.ImShow("?", frame);
            ////////////////////////////////////////// 색상별그리기
            return shape;


        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Cv2.ImShow("클라", frame); //이 frame은 이제 사진 한장임

            Mat scrn1 = new Mat();
            Mat scrn2 = new Mat();
            Mat scrn3 = new Mat();
            Mat scrn4 = new Mat();
            Mat scrn5 = new Mat();
            Mat scrn6 = new Mat();
            //Size size = new Size(frame.Width * 2, frame.Height * 2); // 이미지 크기늘리기
            //Cv2.Resize(frame, dst, size);

            //Mat roi = new Mat();

            //Rect rect = new Rect(70, 30, 120, 120);
            //roi = src.SubMat(rect);

            //Mat dst = src.SubMat(new Rect(300, 300, 500, 300));
            //Cv2.ImShow("src", src);
            //Cv2.ImShow("dst", dst);


            /////////////////////////////////////////////////////////////
            test = new Mat(); //출력 할거
            Cv2.CvtColor(frame, test, ColorConversionCodes.BGR2HSV);
            //             입력   출력  변환식              

            Mat mask1 = new Mat(); // 색상추출
            Cv2.InRange(test, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
            Mat mask2 = new Mat();
            Cv2.InRange(test, new Scalar(40, 70, 80), new Scalar(70, 255, 255), mask2); //초록
            Mat mask3 = new Mat();
            Cv2.InRange(test, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
            Mat mask4 = new Mat();
            Cv2.InRange(test, new Scalar(100, 70, 10), new Scalar(121, 255, 255), mask4); //파랑

            Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            int yel = 0, gre = 0, red = 0, blu = 0;

            Rect rect1 = new Rect(10, 20, 180, 180);
            scrn1 = frame.SubMat(rect1);
            Rect rect2 = new Rect(200, 20, 180, 180);
            scrn2 = frame.SubMat(rect2);
            Rect rect3 = new Rect(460, 20, 180, 180);
            scrn3 = frame.SubMat(rect3);
            Rect rect4 = new Rect(10, 280, 180, 180);
            scrn4 = frame.SubMat(rect4);
            Rect rect5 = new Rect(200, 280, 180, 180);
            scrn5 = frame.SubMat(rect5);
            Rect rect6 = new Rect(450, 280, 180, 180);
            scrn6 = frame.SubMat(rect6);



            foreach (var c in contours1)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000) //픽셀단위 8000이상일때만
                {
                    //Cv2.DrawContours(test, new[] { c }, -1, Scalar.Yellow, 3); //윤곽선 그리는거 필요없음
                    string shape = GetShape(c); //형상구분함수 밑에 있음 string을 반환함
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00); //이 3놈 중앙 찾음
                    Cv2.PutText(frame, "Yellow " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                    //화면에 표기 출력할놈 //텍스트           //좌표            //폰트                             글자색
                    yel++;
                }
            }
            foreach (var c in contours2)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000)
                {
                    //Cv2.DrawContours(test, new[] { c }, -1, Scalar.Green, 3);
                    string shape = GetShape(c); //*형상구분

                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00);
                    Cv2.PutText(frame, "Green " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Green, 2);
                    gre++;
                }
            }
            foreach (var c in contours3)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000)
                {
                    //Cv2.DrawContours(test, new[] { c }, -1, Scalar.Red, 3);
                    string shape = GetShape(c); //*형상구분
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00);
                    Cv2.PutText(frame, "Red " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
                    red++;
                }
            }
            foreach (var c in contours4)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000)
                {
                    //Cv2.DrawContours(test, new[] { c }, -1, Scalar.Blue, 3);
                    string shape = GetShape(c); //*형상구분

                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00);
                    Cv2.PutText(frame, "Blue " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 2);
                    blu++;
                }
            }

            //Cv2.ImShow("서버", frame); //이 frame은 이제 사진 한장임

            Cv2.ImShow("test1", scrn1);
            Cv2.ImShow("test2", scrn2);
            Cv2.ImShow("test3", scrn3);
            Cv2.ImShow("test4", scrn4);
            Cv2.ImShow("test5", scrn5);
            Cv2.ImShow("test6", scrn6);

            //string msg = "찾은 윤곽선: [" + hierarchy1.Length.ToString() + "]";

            Label_1.Content = yel; //색갈의 갯수가 추가가되긴함
            Label_2.Content = gre;
            Label_3.Content = red;
            Label_4.Content = blu;


        }
        

        ///////////////////////2번 성공?

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (Mat changImage = frame.CvtColor(ColorConversionCodes.BGR2GRAY)) // 이미지 gray로 잘라서 씀
            {
                //Cv2.ImShow("test2", changImage); // 변환된 이미지를 보여줌

                var cutChangeImage = frame.Clone(new Rect(10, 20, 180, 180)); // 변환된 이미지를 잘라서 변수에 저장 ^^ 잘린 이미지를 원본이미지에서 찾는다. 빨간테두리로 적용됨

                Cv2.ImWrite("frame2.jpg", cutChangeImage); // 잘라서 변수에 저장된 이미지를 컴퓨터 파일에 저장
                Mat image = Cv2.ImRead("frame2.jpg"); // 자른이미지를 보기위한
                Cv2.ImShow("frame2_image?", image); // 자른이미지를 보기위함
            }
        }
        // 파일을 gray변환 -> 변환된 이미지를 자름 -> 자른이미지 컴퓨터에 파일로 저장 -> 해당 이미지출력(-)

        private void Button_Click_3(object sender, RoutedEventArgs e) // 이미지 위치 지정하고 해당 위치 원본이미지와 비교 분석함
        {
            //using (Mat mat = new Mat("test3.png"))// 이미지 테스트용 test1, 영상 출력용 frame
            //using (Mat mat = frame)


            //using (Mat temp = new Mat("frame2.jpg")) // frame 부분을 해당 "파일경로"로 지정하면 비교 매칭함
            //using (Mat temp = new Mat("test11.png")) // 카메라가 볼록이어서 이미지로 테스트하기위함

            /*using (Mat result = new Mat())
            {
                Cv2.ImShow("원본", mat); // 
                Cv2.ImShow("비교용사진", temp);

                Cv2.MatchTemplate(mat, temp, result, TemplateMatchModes.CCoeffNormed);

                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);

                var threshold = 0.7;


                if (maxval >= threshold)
                {
                    Rect rect = new Rect(maxloc.X, maxloc.Y, temp.Width, temp.Height);
                    Cv2.Rectangle(mat, rect, new OpenCvSharp.Scalar(0, 0, 255), 2);

                    Cv2.ImShow("template_show", mat); // 최종 화면 출력
                }


                else
                {
                    MessageBox.Show("못찾음요");
                }
            }*/
            using (Mat mat = new Mat("real1.png")) // ok이미지 컬러
            using (Mat temp = new Mat("real2.png")) // ng이미지 컬러
            
            using (Mat result = new Mat())
            {
                Mat sub = new Mat(); // 일단 선언

                Mat gray_image1 = mat.CvtColor(ColorConversionCodes.BGR2GRAY); // 회색조변환 // ok흑백
                Mat gray_image2 = temp.CvtColor(ColorConversionCodes.BGR2GRAY); // 회색조변환 // ng흑백

                /// 회색변환완료
                Cv2.Absdiff(gray_image1, gray_image2, sub); // 차이고
                Cv2.ImShow("변환된이미지", sub); // 차이 이미지가 뜸 굿

                Mat gray = sub; // 선언
                Mat test2 = gray.Clone(); // 복사

                Cv2.ImShow("1", gray); // =sub
                Cv2.ImShow("2", test2); // =sub, gray

                Point[][] contours; // point 배열 , 좌표값
                HierarchyIndex[] hierarchy; // 몰라뭔지

                Mat qwer1111 = new Mat(); // 선언일단하고
                Cv2.InRange(sub, new Scalar(0, 100, 100), new Scalar(0, 255, 255), gray); // 흑백으로 처리끝난값
                Cv2.FindContours(gray, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);
                Cv2.CvtColor(test2, qwer1111, ColorConversionCodes.GRAY2BGR); // 흑백처리끝난거 -> 컬러로바꿈

                Cv2.ImShow("tjnksdjnai", qwer1111); // 차이값의 이미지 -> 컬러값의 이미지
                //List<Point[]> lInPoints = new List<Point[]>();
                foreach (Point[] p in contours)
                {
                    double length = Cv2.ArcLength(p, true); // 길이?
                    double area = Cv2.ContourArea(p, true); // 구역?

                    if (length < 1 && area < 100 && p.Length < 1) continue; // 출력부분 위치잡기

                    Rect boundingRect = Cv2.BoundingRect(p); // 해당부분 사각형으로 잡기위한 구역

                    Cv2.Rectangle(temp, boundingRect, Scalar.Red, 2, LineTypes.Link8); // 사각형으로 ? 출력함
                    // 흑백처리 -> 컬러처리이미지 위에 그려 오류를
                    //lInPoints.Add(p);
                }
                Mat eeee = new Mat();
                Mat ffff = new Mat();
                Cv2.ImShow("dst", temp); // 최종출력 컬러로 바꾼상태
                                             //Cv2.CvtColor(test2, eeee, ColorConversionCodes.GRAY2BGR); // 흑백 -> 컬러 색상변경
                                             //Cv2.ImShow("변환", eeee);
                                             //Cv2.Rectangle(eeee, , Scalar.Red, 2, LineTypes.AntiAlias);

                
                //[이미지 비교 유사도]
                ///////////////////////////////////////////////////////////////////////////////////////////

                Mat aaaaa = new Mat();
                Mat bbbbb = new Mat();

                Mat screen = null, find = null, res = null;


                Mat test222 = new Mat("real2.png");
                Cv2.Resize(mat, aaaaa, new Size(158, 463)); // mat은 real1.png
                Cv2.Resize(test222, bbbbb, new Size(158, 463)); // test222는 real2.png

                screen = aaaaa; // real1
                find = bbbbb; // real2

                res = screen.MatchTemplate(find, TemplateMatchModes.CCoeffNormed);

                double min, max = 0;

                Cv2.MinMaxLoc(res, out min, out max);
                
                Label_dd.Content = ("유사도 : " + (String.Format("{0:P}", max)));

                screen.Dispose();
                find.Dispose();
                res.Dispose();

            }  
            
        }    
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //using (Mat mat = new Mat(frame))
            {
                Cv2.ImShow("sample", frame);
            }    
        }
    }
}
