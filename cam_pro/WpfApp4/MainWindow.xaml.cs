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
using System.Xml.Linq;


namespace ttest
{
    public partial class MainWindow : System.Windows.Window
    {
        Point[][] contours;
        HierarchyIndex[] hierarchyIndexes;

        public VideoCapture cam; //영상파일을 불러올 때 사용하는 클래스
        public Mat frame, test;

        DispatcherTimer timer;
        bool is_initCam, is_initTimer;
        //List<string> strings_0 = new List<string>() { "Yellow square", "Green circle", "Red square", "Blue triangle" };
        List<string> strings_0 = new List<string>();
        List<string> strings_1 = new List<string>();
        List<string> strings_2 = new List<string>();
        List<string> strings_3 = new List<string>();
        List<string> strings_4 = new List<string>();
        List<string> strings_5 = new List<string>();
        List<string> strings_6 = new List<string>();
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
                //cam.FrameHeight = (int)Cam_1.Height;
                //cam.FrameWidth = (int)Cam_1.Width;

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
            Cam_1.Source = WriteableBitmapConverter.ToWriteableBitmap(frame); //첫화면 출력


            Mat aaa = frame.Clone(); //원본 복사하고

            int roiWidth = 200;
            int roiHeight = 100;
            int x = 0; //x 좌표 시작 위치
            int y = 0; //y 좌표 시작 위치

            ////// 첫 번째 사각형 그리기 //aaa는 원본 복사한거
            Cv2.Rectangle(aaa, new Point(x + 20, y + 30), new Point(x + roiWidth, y + roiHeight + 20), new Scalar(0, 0, 0), 1);
            Cv2.Rectangle(aaa, new Point(x + 20, y + 120), new Point(x + roiWidth, y + roiHeight + 120), new Scalar(0, 0, 0), 1);
            Cv2.Rectangle(aaa, new Point(x + 20, y + 220), new Point(x + roiWidth, y + roiHeight + 350), new Scalar(0, 0, 0), 1);
            //관심구역 그저 표현한거

            //첫번째 영역

            Mat M_roi_1 = aaa.Clone(); //원본 복사하고


            Mat roi_1 = new Mat(M_roi_1, new Rect(x+20, y+30, x+roiWidth-20, y+roiHeight-10)); //관심 구역 지정





            Cv2.CvtColor(roi_1, roi_1, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴
            Mat mask1 = new Mat();
            Cv2.InRange(roi_1, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
            Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            int pix = 1000;
            foreach (var c in contours1)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000) //픽셀단위  숫자 이상만
                {
                    string shape = GetShape(c);

                    string name = "Yellow " + shape;
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00) + 20;
                    var cy = (int)(M.M01 / M.M00) + 30; //이 3놈 중앙 찾음
                    Cv2.DrawContours(roi_1, contours1, -1, Scalar.Red, 2); //윤곽그리고

                    Cv2.PutText(aaa, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                }
            }
            //2번째 영역
            Mat M_roi_2 = aaa.Clone();
            Mat roi_2 = new Mat(M_roi_2, new Rect(x + 20, y + 120, x + roiWidth-20, y + roiHeight)); //관심 구역 지정
            Cv2.CvtColor(roi_2, roi_2, ColorConversionCodes.BGR2GRAY); //흑백으로 만들고
            Cv2.Threshold(roi_2, roi_2, 127, 255, ThresholdTypes.Binary); //
            Cv2.FindContours(roi_2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            foreach (var c in contours1)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000) //픽셀단위  숫자 이상만
                {
                    string name = "B-Checker";
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00) + 20;
                    var cy = (int)(M.M01 / M.M00) + 120; //이 3놈 중앙 찾음

                    Cv2.PutText(aaa, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 2);
                }
            }

            Mat M_roi_3 = aaa.Clone(); //원본 복사하고
            Mat roi_3 = new Mat(M_roi_3, new Rect(x + 20, y + 220, roiWidth - 20, roiHeight + 130));

            Cv2.CvtColor(roi_3, roi_3, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴

            Mat mask3 = new Mat();
            Cv2.InRange(roi_3, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
            Mat mask4 = new Mat();
            Cv2.InRange(roi_3, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4); //파랑

            Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            //Cv2.DrawContours(frame, contours1, -1, Scalar.Red, 2); //윤곽그리고

            foreach (var c in contours3)
            {
                var area = Cv2.ContourArea(c);
                if (area > pix)
                {
                    string shape = GetShape(c); //*형상구분
                    string name = "Red " + shape;
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00)+220 ; //이 3놈 중앙 찾음
                    //Cv2.DrawContours(aaa, contours3, -1, Scalar.Red, 2); //윤곽그리고

                    Cv2.PutText(aaa, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
                }
            }
            foreach (var c in contours4)
            {
                var area = Cv2.ContourArea(c);
                if (area > pix)
                {
                    string shape = GetShape(c); //*형상구분
                    string name = "Blue " + shape;
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00) ;
                    var cy = (int)(M.M01 / M.M00)+220; //이 3놈 중앙 찾음
                    //Cv2.DrawContours(aaa, contours4, -1, Scalar.Red, 2); //윤곽그리고

                    Cv2.PutText(aaa, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 2);

                }
            }
            Cam_2.Source = WriteableBitmapConverter.ToWriteableBitmap(aaa);

        }

        public string GetShape(Point[] c)
        {
            string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            Point[] approx = Cv2.ApproxPolyDP(c, 0.03* peri, true);

            if (approx.Length == 3) //if the shape is a triangle, it will have 3 vertices
            {
                shape = "triangle";
            }
            else if (approx.Length == 4 || approx.Length == 5)    //if the shape has 4 vertices, it is either a square or a rectangle
            {
                shape = "square";
            }
            else  //각이 없으면
            {
                shape = "circle";
            }
            return shape;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ListBox_1.Items.Clear();
            ListBox_2.Items.Clear();
            ListBox_3.Items.Clear();

        }

        private void List_add_1(int i, string b)  //리스트박스에 넣을라구 만든거
        {

            if (i == 0)
            {
                ListBox_1.Items.Add(b);
                strings_1.Add(b);
            }
            else if (i == 1)
            {
                ListBox_2.Items.Add(b);
                strings_2.Add(b);
            }
            else if (i == 2)
            {
                ListBox_3.Items.Add(b);
                strings_3.Add(b);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


            for (int i = 0; i < strings_0.Count; i++)
            {
                if (strings_0[i] != strings_1[i])
                {
                    //MessageBox.Show($"1번 {i}제품 불량", "오류");
                    ListBox_7.Items.Add("1번 제품 불량 " + strings_0[i] + " -> " + strings_1[i]);
                }

                if (strings_0[i] != strings_2[i])
                {
                    //MessageBox.Show($"2번 {i}제품 불량", "오류");
                    ListBox_7.Items.Add("2번 제품 불량 " + strings_0[i] + " -> " + strings_2[i]);
                }

                if (strings_0[i] != strings_3[i])
                {
                    //MessageBox.Show($"3번 {i}제품 불량", "오류");
                    ListBox_7.Items.Add("3번 제품 불량 " + strings_0[i] + " -> " + strings_3[i]);
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            B_Check_1.aaa(this);

            //Mat t_frame = frame.Clone(); //원본 복사하고

            //Mat[] subImages = new Mat[3];
            //Mat[] aaa = new Mat[3];

            //for (int i = 0; i < 3; i++)
            //{
            //    int width = t_frame.Width; //test 넓이 크기
            //    int height = t_frame.Height; //높이
            //    int subWidth = width / 3; //넒이 나누기 3
            //    int subHeight = height;

            //    int startX = i * subWidth;
            //    int endX = (i + 1) * subWidth;
            //    int startY = 0;
            //    int endY = height - subHeight; //좌표계산

            //    subImages[i] = t_frame.SubMat(new Rect(startX, startY, subWidth, subHeight)); //자르기
            //    //subImages_1[i] = frame.SubMat(new Rect(startX, startY, subWidth, subHeight)); //자르기

            //    Cv2.ImShow($"SubImage{(i + 1)}_3", subImages[i]); // show subImages_3


            //    aaa[i] = subImages[i].Clone(); //원본 복사하고

            //    int roiWidth = 200;
            //    int roiHeight = 100;
            //    int x = 0; //x 좌표 시작 위치
            //    int y = 0; //y 좌표 시작 위치

            //    ////// 첫 번째 사각형 그리기 //aaa는 원본 복사한거
            //    Cv2.Rectangle(aaa[i], new Point(x + 20, y + 30), new Point(x + roiWidth, y + roiHeight + 20), new Scalar(0, 0, 0), 1);
            //    Cv2.Rectangle(aaa[i], new Point(x + 20, y + 120), new Point(x + roiWidth, y + roiHeight + 120), new Scalar(0, 0, 0), 1);
            //    Cv2.Rectangle(aaa[i], new Point(x + 20, y + 220), new Point(x + roiWidth, y + roiHeight + 350), new Scalar(0, 0, 0), 1);
            //    //관심구역 그저 표현한거

            //    //첫번째 영역
            //    Mat M_roi_1 = aaa[i].Clone(); //원본 복사하고
            //    Mat roi_1 = new Mat(M_roi_1, new Rect(x + 20, y + 30, x + roiWidth - 20, y + roiHeight - 10)); //관심 구역 지정
            //    Cv2.CvtColor(roi_1, roi_1, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴
            //    Mat mask1 = new Mat();
            //    Cv2.InRange(roi_1, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
            //    Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            //    int pix = 1000;
            //    foreach (var c in contours1)
            //    {
            //        var area = Cv2.ContourArea(c);
            //        if (area > 1000) //픽셀단위  숫자 이상만
            //        {
            //            string shape = GetShape(c);
            //            string name = "Yellow " + shape;
            //            var M = Cv2.Moments(c);
            //            var cx = (int)(M.M10 / M.M00) + 20;
            //            var cy = (int)(M.M01 / M.M00) + 30; //이 3놈 중앙 찾음
            //            Cv2.DrawContours(roi_1, contours1, -1, Scalar.Red, 2); //윤곽그리고

            //            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
            //        }
            //    }
            //    //2번째 영역
            //    Mat M_roi_2 = aaa[i].Clone();
            //    Mat roi_2 = new Mat(M_roi_2, new Rect(x + 20, y + 120, x + roiWidth - 20, y + roiHeight)); //관심 구역 지정
            //    Cv2.CvtColor(roi_2, roi_2, ColorConversionCodes.BGR2GRAY); //흑백으로 만들고
            //    Cv2.Threshold(roi_2, roi_2, 127, 255, ThresholdTypes.Binary); //
            //    Cv2.FindContours(roi_2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            //    foreach (var c in contours1)
            //    {
            //        var area = Cv2.ContourArea(c);
            //        if (area > 1000) //픽셀단위  숫자 이상만
            //        {
            //            string name = "B-Checker";
            //            var M = Cv2.Moments(c);
            //            var cx = (int)(M.M10 / M.M00) + 20;
            //            var cy = (int)(M.M01 / M.M00) + 120; //이 3놈 중앙 찾음

            //            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 2);
            //        }
            //    }

            //    Mat M_roi_3 = aaa[i].Clone(); //원본 복사하고
            //    Mat roi_3 = new Mat(M_roi_3, new Rect(x + 20, y + 220, roiWidth - 20, roiHeight + 130));

            //    Cv2.CvtColor(roi_3, roi_3, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴

            //    Mat mask3 = new Mat();
            //    Cv2.InRange(roi_3, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
            //    Mat mask4 = new Mat();
            //    Cv2.InRange(roi_3, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4); //파랑

            //    Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            //    Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            //    //Cv2.DrawContours(frame, contours1, -1, Scalar.Red, 2); //윤곽그리고

            //    foreach (var c in contours3)
            //    {
            //        var area = Cv2.ContourArea(c);
            //        if (area > pix)
            //        {
            //            string shape = GetShape(c); //*형상구분
            //            string name = "Red " + shape;
            //            var M = Cv2.Moments(c);
            //            var cx = (int)(M.M10 / M.M00);
            //            var cy = (int)(M.M01 / M.M00) + 220; //이 3놈 중앙 찾음
            //                                                 //Cv2.DrawContours(aaa, contours3, -1, Scalar.Red, 2); //윤곽그리고

            //            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
            //        }
            //    }
            //    foreach (var c in contours4)
            //    {
            //        var area = Cv2.ContourArea(c);
            //        if (area > pix)
            //        {
            //            string shape = GetShape(c); //*형상구분
            //            string name = "Blue " + shape;
            //            var M = Cv2.Moments(c);
            //            var cx = (int)(M.M10 / M.M00);
            //            var cy = (int)(M.M01 / M.M00) + 220; //이 3놈 중앙 찾음
            //                                                 //Cv2.DrawContours(aaa, contours4, -1, Scalar.Red, 2); //윤곽그리고

            //            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 2);

            //        }
            //    }


            //}
            //for (int i = 0; i < 3; i++)
            //{
            //    Cv2.ImShow($"SubImage{(i + 1)}_3", aaa[i]); // show subImages_3
            //}

        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            test = new Mat(); //출력 할거
            Cv2.CvtColor(frame, test, ColorConversionCodes.BGR2HSV);
            //             입력   출력  변환식              
            Mat mask1 = new Mat();
            Cv2.InRange(test, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
            Mat mask2 = new Mat();
            Cv2.InRange(test, new Scalar(40, 70, 80), new Scalar(70, 255, 255), mask2); //초록
            Mat mask3 = new Mat();
            Cv2.InRange(test, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
            Mat mask4 = new Mat();
            Cv2.InRange(test, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4); //파랑

            Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            int pix = 1000;
            foreach (var c in contours1)
            {
                var area = Cv2.ContourArea(c);
                if (area > 1000) //픽셀단위  숫자 이상만
                {
                    string shape = GetShape(c);
                    string name = "Yellow " + shape;
                    ListBox_0.Items.Add(name);
                    strings_0.Add(name);

                }
            }
            foreach (var c in contours2)
            {
                var area = Cv2.ContourArea(c);
                if (area > pix)
                {
                    string shape = GetShape(c); //*형상구분
                    string name = "Green " + shape;
                    ListBox_0.Items.Add(name);
                    strings_0.Add(name);

                }
            }
            foreach (var c in contours3)
            {
                var area = Cv2.ContourArea(c);
                if (area > pix)
                {
                    string shape = GetShape(c); //*형상구분
                    string name = "Red " + shape;
                    ListBox_0.Items.Add(name);
                    strings_0.Add(name);
                }
            }
            foreach (var c in contours4)
            {
                var area = Cv2.ContourArea(c);
                if (area > pix)
                {
                    string shape = GetShape(c); //*형상구분
                    string name = "Blue " + shape;
                    ListBox_0.Items.Add(name);
                    strings_0.Add(name);

                }
            }

        }
    }


}

