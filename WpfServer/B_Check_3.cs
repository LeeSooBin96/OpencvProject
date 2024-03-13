using System.Collections.Generic;
using System.Windows;
// OpenCV 사용을 위한 using
using OpenCvSharp;

// Timer 사용을 위한 using
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Daneung;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;

namespace ttest
{
    public class B_Check_3
    {
        public static bool check_1 = false;
        public static bool check_2 = false;
        public static bool check_3 = false;
        public static bool check_4 = false;

        public static List<bool> Check_3(Mat r_frame, Mat t_frame)
        {
            //MainWindow MW = mw;

            //Mat t_frame = MW.frame.Clone(); //원본 복사하고

            Mat[] subImages = new Mat[3];
            int width = t_frame.Width; //test 넓이 크기
            int height = t_frame.Height; //높이
            int subWidth = width / 3; //넒이 나누기 3
            int subHeight = height;
            //최종 검사 결과
            List<bool> result = new List<bool>();

            for (int i = 0; i < 3; i++)
            {

                int startX = i * subWidth;
                int startY = 0;

                subImages[i] = t_frame.SubMat(new Rect(startX, startY, subWidth, subHeight)); //자르기
                
                First_area(subImages[i]); //첫번째 영역 
                Second_area(subImages[i]); //두번째 영역
                Third_area(subImages[i]); //세번째 영역

                Thread.Sleep(100);
                result.Add(Check_result(i));


                MatchingIMG.CalculateRateIMG(r_frame, subImages[i]);
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                //for (int i = 0; i < 3; i++)
                //{
                //    Cv2.ImShow($"SubImage{(i + 1)}_3", subImages[i]); // show subImages_3
                //}
                Cv2.ImShow(DateTime.Now.ToString(), t_frame);
            });

            //foreach (bool i in result) //하나라도 false있으면
            //    if (!i) return false;
            //return true; //하나도 없으면
            return result;
        }


        public static async void First_area(Mat subImages)
        {
            await Task.Run(() =>
            {
                //Mat M_roi_3 = subImages.Clone(); //원본 복사하고

                Mat roi_1 = new Mat(subImages, new Rect(80, 70, 100, 100)); //관심 구역 지정
                Cv2.CvtColor(roi_1, roi_1, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴
                Mat mask1 = new Mat();
                Cv2.InRange(roi_1, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
                Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
                Cv2.Erode(mask1, mask1, null, iterations: 1);
                Cv2.Dilate(mask1, mask1, null, iterations: 1);
                foreach (var c in contours1)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > 500)
                    {
                        string shape = GetShape(c);
                        string name = "Yellow " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00) + 20;
                        var cy = (int)(M.M01 / M.M00) + 30; //이 3놈 중앙 찾음
                        Cv2.DrawContours(roi_1, contours1, -1, Scalar.Red, 2); //윤곽그리고
                        if (shape == "circle")
                        {
                            Cv2.PutText(subImages, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                            check_1 = true;
                        }
                    }
                }
            });
        }


        public static async void Second_area(Mat subImages)
        {

            await Task.Run(() =>
            {
                Mat roi_2 = new Mat(subImages, new Rect(130, 170, 40, 100)); //관심 구역 지정
                Cv2.CvtColor(roi_2, roi_2, ColorConversionCodes.BGR2GRAY); //흑백으로 만들고
                Cv2.Threshold(roi_2, roi_2, 127, 255, ThresholdTypes.Binary); //
                Cv2.FindContours(roi_2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                if (contours2.Length > 1)
                {
                    string name = "B-Checker";
                    Cv2.PutText(subImages, name, new Point(150, 200), HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 2);
                    check_2 = true;
                }
            });
        }
        public static async void Third_area(Mat subImages)
        {
            await Task.Run(() =>
            {
                //Mat M_roi_3 = subImages.Clone(); //원본 복사하고
                Mat roi_3 = new Mat(subImages, new Rect(10, 150, 180, 230));
                Cv2.CvtColor(roi_3, roi_3, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴
                Mat mask3 = new Mat();
                Cv2.InRange(roi_3, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
                Mat mask4 = new Mat();
                Cv2.InRange(roi_3, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4); //파랑
                Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
                Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
                Cv2.Erode(mask3, mask3, null, iterations: 1);
                Cv2.Dilate(mask3, mask3, null, iterations: 1);
                Cv2.Erode(mask4, mask4, null, iterations: 1);
                Cv2.Dilate(mask4, mask4, null, iterations: 1);
                foreach (var c in contours3)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > 1000)
                    {
                        string shape = GetShape(c); //*형상구분
                        string name = "Red " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00);
                        var cy = (int)(M.M01 / M.M00) + 220; //이 3놈 중앙 찾음
                        if (shape == "pentagon")
                        {
                            Cv2.PutText(subImages, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
                            check_3 = true;
                        }
                    }
                }
                foreach (var c in contours4)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > 1000)
                    {
                        string shape = GetShape(c); //*형상구분
                        string name = "Blue " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00);
                        var cy = (int)(M.M01 / M.M00) + 220; //이 3놈 중앙 찾음
                        if (shape == "triangle")
                        {
                            Cv2.PutText(subImages, name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 2);
                            check_4 = true;
                        }
                    }
                }
            });
        }

        public static bool Check_result(int a)
        {
            bool ck=false;
            if (a == 0)//1번 제품 검사 결과
            {
                if (check_1 && check_2  && check_3  && check_4 )
                {
                    MessageBox.Show("1번 제품 합격"); ck = true;
                }
                else
                {
                    MessageBox.Show("1번 제품 불량"); ck = false;
                }
                check_1 = false;
                check_2 = false;
                check_3 = false;
                check_4 = false;
            }
            else if (a == 1)//1번 제품 검사
            {
                if (check_1  && check_2  && check_3  && check_4 )
                {
                    MessageBox.Show("2번 제품 합격"); ck = true;
                }
                else
                {
                    MessageBox.Show("2번 제품 불량"); ck = false;
                }
                check_1 = false;
                check_2 = false;
                check_3 = false;
                check_4 = false;
            }
            else if (a == 2)//1번 제품 검사
            {
                if (check_1 == true && check_2 == true && check_3 == true && check_4 == true)
                {
                    MessageBox.Show("3번 제품 합격"); ck = true;
                }
                else
                {
                    MessageBox.Show("3번 제품 불량"); ck = false;
                }
                check_1 = false;
                check_2 = false;
                check_3 = false;
                check_4 = false;
            }
            return ck;
        }



        public static string GetShape(Point[] c)
        {
            string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            Point[] approx = Cv2.ApproxPolyDP(c, 0.01 * peri, true);

            if (approx.Length == 3) //if the shape is a triangle, it will have 3 vertices
            {
                shape = "triangle";
            }
            else if (approx.Length > 3 && approx.Length < 6)    //if the shape has 4 vertices, it is either a square or a rectangle
            {
                shape = "pentagon";
            }
            else  //각이 없으면
            {
                shape = "circle";
            }
            return shape;
        }
    }
}


