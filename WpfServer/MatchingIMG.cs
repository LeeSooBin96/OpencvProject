using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daneung
{
    internal class MatchingIMG
    {
        static public void CalculateRateIMG()
        {
            using (Mat mat = new Mat("real1.png")) // ok이미지 컬러 --이게 정상제품 이미지?
            using (Mat temp = new Mat("real2.png")) // ng이미지 컬러 --이게 비교할 이미지?

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

                //Label_dd.Content = ("유사도 : " + (String.Format("{0:P}", max)));

                screen.Dispose();
                find.Dispose();
                res.Dispose();
            }
        }
    }
}
