﻿using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.DesignerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfServer
{ 
    internal class CheckingImage
    {
        byte[] normalP = null; //정상제품 이미지 저장할 바이트 배열
        
        public CheckingImage(string productName) //생성자
        {
            string num;
            switch(productName) //제품명에 따른 정상 제품 이미지 로드해야함
            {
                case "product 1":
                    num = "1";
                    break;
                case "product 2":
                    num = "2";
                    break;
                case "product 3":
                    num = "3";
                    break;
                default:
                    MessageBox.Show(productName);
                    return;
            }

            //해당하는 이미지 파일 읽어서 바이트 배열에 저장해야함
            Stream rFile = new FileStream("..//Image/P" + num + ".png", FileMode.Open,FileAccess.Read);
            MessageBox.Show(rFile.Length.ToString());
            normalP = new byte[rFile.Length]; //파일 사이즈에 맞춰 바이트 배열 생성
            rFile.Read(normalP, 0, normalP.Length); //파일 읽기
            rFile.Close();
            //일단 정상 제품 이미지 바이트 배열에 저장 완료

            //파일 제대로 읽나 테스트 --일단 제대로 읽음.. 이건 확인되는데
            Stream wFile = new FileStream("..//Image/Pt"+num+".png", FileMode.Create,FileAccess.Write); //저장은 제대로 됨
            wFile.Write(normalP, 0, normalP.Length); //이렇게 하면 정상적으로 저장되네 저장했다 해야하나
            wFile.Close();
            //스레드 내에서 UI(WPF)관련 건드릴때 사용하는 구문
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Mat frame = new Mat();
                //frame = Mat.FromImageData(normalP, ImreadModes.AnyColor);
                Cv2.ImShow(num, Mat.FromImageData(normalP, ImreadModes.AnyColor));
            });
            
            //왜 이미지 띄우기만 말썽일까 --해결
        }
    }
}